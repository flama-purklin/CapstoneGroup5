using UnityEngine;

public class NPCAnimManager : MonoBehaviour
{
    //Necessary Object References
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] Animator animator;
    [SerializeField] NPCMovement movementControl;
    [SerializeField] NPCAnimContainer anims;
    GameObject player;

    //other important vars
    Sprite[] currentAnim;

    bool backward = false;

    Vector3 originalScale;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        player = GameObject.FindWithTag("Player");
        currentAnim = anims.idleFront;
        originalScale = Vector3.one;
        transform.root.localScale = originalScale;
        //Debug.Log(originalScale);
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
        if ((movementControl.movementVector.x > 0 && !backward) || (movementControl.movementVector.x <= 0 && backward))
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

        if (playerDir.x > 0)
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
