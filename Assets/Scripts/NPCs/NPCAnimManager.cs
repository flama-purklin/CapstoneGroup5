using UnityEngine;

public class NPCAnimManager : MonoBehaviour
{
    //Necessary Object References
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] Animator animator;
    [SerializeField] NPCMovement movementControl;
    [SerializeField] public NPCAnimContainer anims; // This will be assigned externally now
    GameObject player;

    // Removed allAnims array - no longer needed

    //other important vars
    Sprite[] currentAnim; // Represents the sprites for the *current* animation state (idle, walk, etc.)

    bool backward = false;

    Vector3 originalScale;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // We still need the player reference.
        player = GameObject.FindWithTag("Player");

        // Basic null checks for critical references assigned in Inspector (on the prefab)
        if (sprite == null) Debug.LogError($"NPCAnimManager on {gameObject.name}: SpriteRenderer reference is not assigned in the prefab inspector!");
        if (animator == null) Debug.LogError($"NPCAnimManager on {gameObject.name}: Animator reference is not assigned in the prefab inspector!");
        if (movementControl == null) Debug.LogError($"NPCAnimManager on {gameObject.name}: NPCMovement reference is not assigned in the prefab inspector!");


        // Removed call to AnimContainerAssign()
        // currentAnim will be set in SetAnimContainer or default here if anims is somehow pre-assigned

        if (anims != null && anims.idleFront != null)
        {
            currentAnim = anims.idleFront;
        }
        else
        {
            currentAnim = null; // Or handle default/error state
            Debug.LogWarning($"NPCAnimManager on {gameObject.name} Awake: 'anims' or 'anims.idleFront' is null.");
        }

        if (sprite != null)
        {
            originalScale = sprite.transform.localScale;
        }
        else
        {
            originalScale = Vector3.one; // Fallback if sprite is missing
            Debug.LogError($"NPCAnimManager on {gameObject.name} Awake: Cannot get originalScale because sprite reference is null!");
        }
        // transform.root.localScale = originalScale; // Don't set root scale here
        //Debug.Log(originalScale);
    }

    // Removed AnimContainerAssign method entirely

    // Update is called once per frame
    void Update()
    {
        // Ensure movementControl is valid before using it.
        if (movementControl == null) return; // Exit if movement control is missing

        if (movementControl.inDialogueRange)
            DirectionOverride();
    }

    public void ApplyAnim()
    {
        if (anims == null)
        {
            Debug.LogWarning($"NPCAnimManager on {gameObject.name} ApplyAnim: 'anims' is null.");
            return; // Can't apply anims if container is missing
        }

        if (animator == null)
        {
            Debug.LogError($"NPCAnimManager on {gameObject.name} ApplyAnim: Animator reference is null!");
            return;
        }
        if (movementControl == null)
        {
             Debug.LogError($"NPCAnimManager on {gameObject.name} ApplyAnim: NPCMovement reference is null!");
             return;
        }


        if (animator.GetBool("moving"))
        {
            if (movementControl.movementVector.z > 0)
                currentAnim = anims.walkBack; // Assumes walkBack exists in NPCAnimContainer
            else
                currentAnim = anims.walkFront; // Assumes walkFront exists
        }
        else
        {
            if (backward)
                currentAnim = anims.idleBack; // Assumes idleBack exists
            else
                currentAnim = anims.idleFront; // Assumes idleFront exists
        }

        UpdateDirection();
    }

    public void UpdateDirection()
    {
        //first apply whether the sprite should face backward or not
        if (movementControl == null)
        {
             Debug.LogError($"NPCAnimManager on {gameObject.name} UpdateDirection: NPCMovement reference is null!");
             return;
        }
        if (movementControl.movementVector.z > 0)
            backward = true;
        else
            backward = false;

    //then flip anim based on last movement vector
    if (sprite != null) // Add null check for safety
    {
        if ((movementControl.movementVector.x < 0 && !backward) || (movementControl.movementVector.x >= 0 && backward))
            sprite.transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
        else
            sprite.transform.localScale = originalScale;
    }
}

    //called whenever the player is in range to initiate dialogue - the npc will turn to face them
    private void DirectionOverride()
    {
        // Add null check here to prevent errors if anims isn't assigned yet
        if (anims == null)
        {
            // Silently return or log a less spammy warning if needed
            // Debug.LogWarning($"DirectionOverride called on {gameObject.name} but anims is null.");
            return;
        }

        currentAnim = anims.idleFront;
        backward = false;

        Vector3 playerDir = player.transform.position - transform.position;

        if (sprite != null) // Add null check for safety
        {
            if (playerDir.x < 0)
                sprite.transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
            else
                sprite.transform.localScale = originalScale;
        }
    }

    public void Animate(int currentSprite)
    {
        // Ensure currentAnim is valid and the index is within bounds
        if (currentAnim != null && currentSprite >= 0 && currentSprite < currentAnim.Length)
        {
            sprite.sprite = currentAnim[currentSprite];
        }
        else if (currentAnim == null)
        {
             Debug.LogWarning($"NPCAnimManager on {gameObject.name} Animate: 'currentAnim' array is null. Cannot set sprite.");
        }
         // Optional: Log warning if index is out of bounds, though Unity's Animator usually handles this
         // else { Debug.LogWarning($"NPCAnimManager on {gameObject.name} Animate: currentSprite index {currentSprite} out of bounds for currentAnim length {currentAnim?.Length ?? 0}."); }
    }

    // New method to set the animation container externally
    public void SetAnimContainer(NPCAnimContainer container)
    {
        if (container != null)
        {
            anims = container;
            // Initialize currentAnim based on the new container
            if (anims.idleFront != null) // Default to idleFront
            {
                currentAnim = anims.idleFront;
                Debug.Log(container + " successfully added to " + gameObject.name);
            }
            else
            {
                 currentAnim = null; // Or handle default/error state
                 Debug.LogWarning($"NPCAnimManager on {gameObject.name} SetAnimContainer: Assigned container '{container.name}' lacks 'idleFront'.");
            }
            // Debug.Log($"NPCAnimManager on {gameObject.name}: Assigned AnimContainer '{container.name}'.");
        }
        else
        {
            Debug.LogError($"NPCAnimManager on {gameObject.name}: Attempted to assign a null NPCAnimContainer.");
            anims = null;
            currentAnim = null;
        }
    }
}
