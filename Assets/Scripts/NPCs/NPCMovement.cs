using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPCMovement : MonoBehaviour
{
    GameObject player;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator anim;
    [SerializeField] GameObject speechBubble;

    public float dialogueDist = 1f;
    public float maxMovementDist = 10f;
    public bool inDialogueRange = false;

    private DialogueControl dialogueControl;
    private bool isInitialized = false;

    private void Awake()
    {
        enabled = true;
        if (TryGetComponent<NavMeshAgent>(out var agent))
        {
            agent.enabled = true;
        }
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

        //Debug.Log("Now Idling");
        //reset time variables, remove goal for navmesh agent
        float idleTime = Random.Range(2, 6);
        float currentTime = 0f;
        agent.isStopped = true;

        //update animation state
        anim.SetBool("moving", false);

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
        //Debug.Log("Now Moving");
        //calculate a random point in a 20 meter radius
        Vector3 movementVector = Random.insideUnitSphere * maxMovementDist;

        //find a place in the actual navmesh that works and set destination
        movementVector += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(movementVector, out hit, maxMovementDist, 1);

        //begin navigation
        agent.isStopped = false;
        agent.SetDestination(hit.position);

        //update anim
        anim.SetBool("moving", true);


        while (agent.remainingDistance > agent.stoppingDistance)
        {
            //break out if the player enters dialogue range
            if (Vector3.Distance(player.transform.position, transform.position) < dialogueDist)
                break;
            else
                yield return new WaitForFixedUpdate();
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
