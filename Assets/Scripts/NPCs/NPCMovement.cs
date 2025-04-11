using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPCMovement : MonoBehaviour
{
    GameObject player;
    [SerializeField] NavMeshAgent agent;

    [SerializeField] GameObject speechBubble;

    public float dialogueDist = 1f;
    public float maxMovementDist = 10f;
    public Vector3 movementVector;
    public bool inDialogueRange = false;

    private DialogueControl dialogueControl;
    private bool isInitialized = false;


    //animation related variables
    [SerializeField] Animator animator;
    [SerializeField] NPCAnimManager animManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        player = GameObject.FindWithTag("Player");
        dialogueControl = GameObject.FindWithTag("DialogueControl").GetComponent<DialogueControl>();

        if (agent == null) agent = GetComponent<NavMeshAgent>(); // Ensure agent is assigned
        if (agent != null)
        {
            // Adjust agent properties for potentially smoother movement
            agent.stoppingDistance = 0.1f; // Give a small buffer
            agent.angularSpeed = 120f;     // Allow smoother turning (adjust value as needed)
            agent.acceleration = 8f;       // Default Unity value, ensure it's reasonable
            // agent.updatePosition = false;
            agent.enabled = true;          // Ensure agent is enabled
        }
        else
        {
            Debug.LogError($"NPCMovement on {gameObject.name}: NavMeshAgent component is missing!");
            enabled = false; // Disable if no agent
            return;
        }

        // Check for Rigidbody and ensure it's kinematic
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            if (!rb.isKinematic)
            {
                Debug.LogWarning($"NPCMovement on {gameObject.name}: Rigidbody is not kinematic. Setting it to kinematic to avoid conflicts with NavMeshAgent.");
                rb.isKinematic = true;
            }
        }

        //set the default value for movement vector
        movementVector = Random.insideUnitSphere; // This seems less useful now, movement is calculated differently
        // StartCoroutine(IdleState()); // Moved to InitializeWhenReady
    }

    private void OnEnable()
    {
        StartCoroutine(InitializeWhenReady());
    }

    private IEnumerator InitializeWhenReady()
    {
        // Wait for all required components
        while (player == null || dialogueControl == null)
        {
            if (!player) player = GameObject.FindWithTag("Player");
            if (!dialogueControl) dialogueControl = FindFirstObjectByType<DialogueControl>();
            yield return new WaitForSeconds(0.1f);
        }

        if (agent) agent.enabled = true;
        isInitialized = true;
        yield return null; // Add a small delay before starting IdleState
        StartCoroutine(IdleState());
    }

    void Update()
    {
        if (!isInitialized) return;


        inDialogueRange = Vector3.Distance(player.transform.position, transform.position) < dialogueDist;
        speechBubble.SetActive(inDialogueRange);

        if (inDialogueRange && Input.GetKeyDown(KeyCode.E) && GameControl.GameController.currentState != GameState.DIALOGUE)
        {
            StartCoroutine(DialogueActivate());
        }
    }

    //may be necessary with expanded state functionality
    /*IEnumerator StateUpdate()
    {
        while (gameObject.activeSelf)
        {

            yield return new WaitForFixedUpdate();
        }
        
    }*/

    IEnumerator IdleState()
    {
        // OPTIONAL NAVMESH CHARACTER IDLE-STATE DEBUGGING
        //if (agent != null) {
        //     Debug.Log($"[NPCMovement Debug] NPC {gameObject.name} starting IdleState at position {agent.transform.position}. Is on NavMesh: {agent.isOnNavMesh}");
        //} else {
        //     Debug.LogWarning($"[NPCMovement Debug] NPC {gameObject.name} starting IdleState but agent is null!");
        //}

        //Debug.Log("Now Idling");
        //reset time variables, remove goal for navmesh agent
        float idleTime = Random.Range(2, 6);
        float currentTime = 0f;

        // Ensure agent is valid and on NavMesh before stopping
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }
        else if (agent != null)
        {
             // Agent exists but isn't on NavMesh, likely due to spawn issue. Log warning and prevent movement.
             Debug.LogWarning($"NPCMovement ({gameObject.name}): Agent is not on NavMesh in IdleState. Cannot stop or move.");
             // Optionally, disable the component or prevent state transitions
             // this.enabled = false;
             yield break; // Exit coroutine to prevent further errors
        }
        else
        {
            Debug.LogError($"NPCMovement ({gameObject.name}): NavMeshAgent component is missing in IdleState!");
             yield break; // Exit coroutine
        }


        //update animation state
        animator.SetBool("moving", false); // Re-enabled
        animManager.ApplyAnim(); // Re-enabled

        while (currentTime < idleTime)
        {
            //stay idle if the player is in or enters dialogue range
            if (Vector3.Distance(player.transform.position, transform.position) < dialogueDist)
            {
                currentTime = 0f;
                inDialogueRange = true;
            }
            //otherwise increment time until motion time
            else
            {
                currentTime += Time.fixedDeltaTime;
                inDialogueRange = false;
            }
            yield return new WaitForFixedUpdate();
        }

        StartCoroutine(MovementState());
    }

    IEnumerator MovementState()
    {
        // OPTIONAL NAVMESH CHARACTER MOVEMENT-STATE DEBUGGING
        //if (agent != null) {
        //     Debug.Log($"[NPCMovement Debug] NPC {gameObject.name} starting MovementState at position {agent.transform.position}. Is on NavMesh: {agent.isOnNavMesh}");
        //} else {
        //     Debug.LogWarning($"[NPCMovement Debug] NPC {gameObject.name} starting MovementState but agent is null!");
        //}

        //Debug.Log("Now Moving");
        Vector3 randomDirection = Random.insideUnitSphere * maxMovementDist;
        randomDirection += transform.position; // Get a random point *around* the current position

        NavMeshHit hit;
        float sampleRadius = 50f; // Use a very large radius to see if *any* point can be found
        // Try to find a valid point on the NavMesh near the random direction
        if (NavMesh.SamplePosition(randomDirection, out hit, sampleRadius, NavMesh.AllAreas))
        {
            //begin navigation only if agent is valid and on NavMesh
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(hit.position);
                // Debug.Log($"NPCMovement ({gameObject.name}): Moving to {hit.position}"); // Optional debug
            }
            else
            {
                Debug.LogWarning($"NPCMovement ({gameObject.name}): Agent not valid or not on NavMesh in MovementState. Cannot set destination.");
                StartCoroutine(IdleState()); // Go back to idle if we can't move
                yield break; // Exit this coroutine
            }
        }
        else
        {
            // Failed to find a sample position, maybe stuck? Go back to idle.
            Debug.LogWarning($"NPCMovement ({gameObject.name}): Failed to find valid NavMesh point near {randomDirection}. Returning to Idle.");
            StartCoroutine(IdleState());
            yield break;
        }


        //update anim
        if (animator != null) animator.SetBool("moving", true);
        if (animManager != null) animManager.ApplyAnim();


        // Check agent validity within the loop as well
        while (agent != null && agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance > agent.stoppingDistance)
        {
            //break out if the player enters dialogue range
            if (Vector3.Distance(player.transform.position, transform.position) < dialogueDist)
                break; // Exit loop if player is close
            else
                yield return new WaitForFixedUpdate(); // Wait for next physics update
        }

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            // agent.ResetPath();
        }

        StartCoroutine(IdleState());
    }

    IEnumerator DialogueActivate()
    {
        if (dialogueControl == null)
        {
            Debug.LogError("Cannot activate dialogue: DialogueControl is null!");
            yield break;
        }

        GameControl.GameController.currentState = GameState.DIALOGUE;
        dialogueControl.Activate(gameObject);
        yield return null;
    }

}
