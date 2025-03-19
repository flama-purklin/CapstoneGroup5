using UnityEngine;

public class NPCAnimManager : MonoBehaviour
{
    //Necessary Object References
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] Animator animator;
    [SerializeField] NPCMovement movementControl;
    [SerializeField] NPCAnimContainer anims;
    GameObject player;

    //all possible npc anim storage (will need expanded systems for archetypes, etc);
    [SerializeField] NPCAnimContainer[] allAnims;

    //other important vars
    Sprite[] currentAnim;

    bool backward = false;

    Vector3 originalScale;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        player = GameObject.FindWithTag("Player");

        //assign an animContainer based on number of npcs in the world
        AnimContainerAssign();

        currentAnim = anims.idleFront;
        originalScale = Vector3.one;
        transform.root.localScale = originalScale;
        //Debug.Log(originalScale);
    }

    private void AnimContainerAssign(string overrideCharacterName = null)
    {
        // Get character name from various sources, with priority order
        string characterName = "unknown";
        
        // 1. Use override if provided (highest priority)
        if (!string.IsNullOrEmpty(overrideCharacterName))
        {
            characterName = overrideCharacterName;
            Debug.Log($"Using override name for animation assignment: {characterName}");
        }
        // 2. Try to get character name by working up the hierarchy
        else
        {
            // First try to get from Character component (most reliable)
            Character characterComponent = GetComponentInParent<Character>();
            if (characterComponent != null && !string.IsNullOrEmpty(characterComponent.CharacterName))
            {
                characterName = characterComponent.CharacterName;
                Debug.Log($"Found Character component with name: {characterName}");
                
                // Fix parent name if needed
                Transform parentTransform = characterComponent.transform.parent;
                if (parentTransform != null && parentTransform.name != $"NPC_{characterName}")
                {
                    Debug.LogWarning($"Fixing parent name: {parentTransform.name} -> NPC_{characterName}");
                    parentTransform.name = $"NPC_{characterName}";
                }
            }
            // Fallback to searching hierarchy
            else
            {
                // Look for parent GameObject name - search up the hierarchy
                Transform current = transform;
                while (current != null)
                {
                    // If we found a parent starting with NPC_, use it
                    if (current.name.StartsWith("NPC_"))
                    {
                        characterName = current.name.Replace("NPC_", "");
                        Debug.Log($"Found parent object with NPC_ prefix: {characterName}");
                        break;
                    }
                    current = current.parent;
                }
            }
        }
        
        // Don't proceed if we don't have valid animations
        if (allAnims == null || allAnims.Length == 0)
        {
            Debug.LogError($"No animation containers available for {characterName}");
            return;
        }
        
        // Use character name's hash code to determine animation set consistently
        int nameHashCode = characterName.GetHashCode();
        int animIndex = Mathf.Abs(nameHashCode) % allAnims.Length;
        
        // Assign the animation set
        anims = allAnims[animIndex];
        
        // Apply initial sprite
        if (sprite != null && anims != null && anims.idleFront != null && anims.idleFront.Length > 0)
        {
            sprite.sprite = anims.idleFront[0];
        }
        
        Debug.Log($"Character '{characterName}' assigned animation set #{animIndex+1} (based on name hash)");
    }

    // Update is called once per frame
    void Update()
    {
        if (movementControl.inDialogueRange)
            DirectionOverride();

    }

    public void ApplyAnim()
    {
        if (animator.GetBool("moving"))
        {
            if (movementControl.movementVector.z > 0)
                currentAnim = anims.walkBack;
            else
                currentAnim = anims.walkFront;
        }
        else
        {
            if (backward)
                currentAnim = anims.idleBack;
            else
                currentAnim = anims.idleFront;
        }

        UpdateDirection();
    }

    public void UpdateDirection()
    {
        //first apply whether the sprite should face backward or not
        if (movementControl.movementVector.z > 0)
            backward = true;
        else
            backward = false;

        //then flip anim based on last movement vector
        if ((movementControl.movementVector.x < 0 && !backward) || (movementControl.movementVector.x >= 0 && backward))
            transform.root.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
        else
            transform.root.localScale = originalScale;
    } 

    //called whenever the player is in range to initiate dialogue - the npc will turn to face them
    private void DirectionOverride()
    {
        currentAnim = anims.idleFront;
        backward = false;

        Vector3 playerDir = player.transform.position - transform.position;

        if (playerDir.x < 0)
            transform.root.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
        else
            transform.root.localScale = originalScale;
    }

    public void Animate(int currentSprite)
    {
        if (currentSprite < currentAnim.Length)
            sprite.sprite = currentAnim[currentSprite];
    }
}