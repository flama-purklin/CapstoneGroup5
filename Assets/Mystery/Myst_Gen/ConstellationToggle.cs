using UnityEngine;
using UnityEngine.InputSystem;

public class ConstellationToggle : MonoBehaviour
{
    private Canvas canvas;

    [SerializeField] RectTransform mysteryHolder;

    void Start()
    {
        // Try to get the Canvas component on this GameObject
        canvas = GetComponent<Canvas>();

        if (canvas == null)
        {
            Debug.LogWarning("No Canvas component found for Constellation.");
        }
        canvas.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (canvas != null)
            {
                if (GameControl.GameController.currentState == GameState.DEFAULT)
                {
                    // Toggle just the canvas visibility
                    canvas.enabled = true;

                    GameControl.GameController.currentState = GameState.MYSTERY;
                }
                else if (GameControl.GameController.currentState == GameState.MYSTERY)
                {
                    canvas.enabled = false;

                    GameControl.GameController.currentState = GameState.DEFAULT;
                }    
            }
            else
            {
                // Toggle the entire GameObject if no canvas is found
                gameObject.SetActive(!gameObject.activeSelf);
            }
        }

        if (Input.GetKey(KeyCode.Mouse1) && GameControl.GameController.currentState == GameState.MYSTERY)
        {
            
            float mouseDeltaX = Input.GetAxis("Mouse X") * 2.0f * Time.deltaTime;
            float mouseDeltaY = Input.GetAxis("Mouse Y") * 2.0f * Time.deltaTime;

            Debug.Log(mouseDeltaX + " " + mouseDeltaY);

            mysteryHolder.anchoredPosition = mysteryHolder.anchoredPosition + new Vector2(mouseDeltaX, mouseDeltaY);
        }
    }

    private void FixedUpdate()
    {
        
    }
}
