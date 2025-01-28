using UnityEngine;

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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startingY = transform.position.y;

        playerAnim.enabled = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        InputCheck();
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
    }
}
