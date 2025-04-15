using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public class PlayerMovement : MonoBehaviour
{
    //components
    [SerializeField] Animator playerAnim;
    [SerializeField] Rigidbody rb;

    //vars
    float startingY;
    [SerializeField] float speedMod;
    float xDelta;
    float yDelta;

    public GameObject closestInteractable;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startingY = transform.position.y;

        playerAnim.enabled = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (GameControl.GameController.currentState == GameState.DEFAULT)
            InputCheck();
        else
            playerAnim.enabled = false;
    }

    private void InputCheck()
    {
        xDelta = Input.GetAxisRaw("Horizontal");
        yDelta = Input.GetAxisRaw("Vertical");

        Vector3 posDelta = new Vector3(xDelta, startingY, yDelta);

        if (posDelta.magnitude > 0)
        {
            posDelta = posDelta.normalized;
            Vector3 goalPos = transform.position + (posDelta * Time.deltaTime * speedMod);

            rb.MovePosition(goalPos);
            playerAnim.enabled = true;
        }
        else
        {
            playerAnim.enabled = false;
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
}
