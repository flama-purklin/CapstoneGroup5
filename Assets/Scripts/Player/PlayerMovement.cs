using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using GLTFast.Schema;

public class PlayerMovement : MonoBehaviour
{
    //components
    [SerializeField] Animator playerAnim;
    [SerializeField] Rigidbody rb;
    [SerializeField] NPCAnimContainer animContainer;
    [SerializeField] SpriteRenderer sprite;

    //vars
    float startingY;
    [SerializeField] float speedMod;
    float xDelta;
    float yDelta;
    bool backward = false;

    Sprite[] currentAnim;

    public GameObject closestInteractable;

    Vector3 originalScale;


    private void Awake()
    {
        originalScale = sprite.transform.localScale;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startingY = transform.position.y;

        //playerAnim.enabled = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (GameControl.GameController.currentState == GameState.DEFAULT)
            InputCheck();
        else
        {
            backward = false;
            xDelta = 0f;
            yDelta = 0f;
        }

        //called every fixedUpdate to make sure the player anims updated properly regardless of gamestate
        StartCoroutine(AnimationUpdate());
    }

    private void InputCheck()
    {
        xDelta = Input.GetAxisRaw("Horizontal");
        yDelta = Input.GetAxisRaw("Vertical");

        Vector3 posDelta = new Vector3(xDelta, startingY, yDelta);

        if (Mathf.Abs(xDelta) > 0 || Mathf.Abs(yDelta) > 0)
        {
            posDelta = posDelta.normalized;
            Vector3 goalPos = transform.position + (posDelta * Time.deltaTime * speedMod);

            rb.MovePosition(goalPos);
            playerAnim.SetBool("moving", true);
        }
        else
        {
            playerAnim.SetBool("moving", false);
        }

        StartCoroutine(ClosestInteractUpdate());
    }

    IEnumerator ClosestInteractUpdate()
    {
        IEnumerable<GameObject> interactables = InteractableManager.FindAllInteractablesinRange(transform.position, 2f);
        float shortestDist = 10000f;
        foreach (GameObject interactable in interactables)
        {
            float dist = Vector3.Magnitude(transform.position - interactable.transform.position);
            if (dist < shortestDist)
            {
                shortestDist = dist;
                closestInteractable = interactable;
            }
        }
        yield return null;
    }

    IEnumerator AnimationUpdate()
    {
        UpdateDirection();

        if (playerAnim.GetBool("moving"))
        {
            if (backward)
                currentAnim = animContainer.walkBack; // Assumes walkBack exists in NPCAnimContainer
            else
                currentAnim = animContainer.walkFront; // Assumes walkFront exists
        }
        else
        {
            if (backward)
                currentAnim = animContainer.idleBack; // Assumes idleBack exists
            else
                currentAnim = animContainer.idleFront; // Assumes idleFront exists
        }
        yield return null;
    }

    public void UpdateDirection()
    {
        //first apply whether the sprite should face backward or not
        /*if (movementControl == null)
        {
            Debug.LogError($"NPCAnimManager on {gameObject.name} UpdateDirection: NPCMovement reference is null!");
            return;
        }*/
        if (yDelta > 0)
            backward = true;
        else if (yDelta < 0)
            backward = false;

        //then flip anim based on last movement vector
        if (sprite != null) // Add null check for safety
        {
            if ((xDelta < 0 && !backward) || (xDelta > 0 && backward))
                sprite.transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
            else if ((xDelta > 0 && !backward) || (xDelta < 0 && backward))
                sprite.transform.localScale = originalScale;
        }
    }

    public void Animate(int currentSprite)
    {
        //Debug.Log("Animate was Called");

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

}
