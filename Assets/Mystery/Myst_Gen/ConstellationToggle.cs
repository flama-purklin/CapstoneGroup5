using UnityEngine;
using UnityEngine.InputSystem;

public class ConstellationToggle : MonoBehaviour
{
    private Canvas canvas;

    [Header("Object Refs")]
    [SerializeField] RectTransform mysteryHolder;
    [SerializeField] Camera mysteryCam;
    [SerializeField] GameObject mysteryHUD;
    [SerializeField] GameObject mainHUD;

    Camera mainCam;

    void Start()
    {
        // Try to get the Canvas component on this GameObject
        canvas = GetComponent<Canvas>();

        if (canvas == null)
        {
            Debug.LogWarning("No Canvas component found for Constellation.");
        }

        //set the mystery stuff for main gameplay
        canvas.enabled = false;
        mysteryHolder.gameObject.SetActive(false);
        mysteryCam.gameObject.SetActive(false);
        mysteryHUD.SetActive(false);
        mainHUD.SetActive(true);

        mainCam = Camera.main;
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
                    mysteryHolder.gameObject.SetActive(true);

                    //switch cams
                    mainCam.gameObject.SetActive(false);
                    mysteryCam.gameObject.SetActive(true);

                    //turn on the mystery HUD
                    mysteryHUD.SetActive(true);
                    mainHUD.SetActive(false);

                    GameControl.GameController.currentState = GameState.MYSTERY;
                    Time.timeScale = 0f;
                }
                else if (GameControl.GameController.currentState == GameState.MYSTERY)
                {
                    canvas.enabled = false;
                    mysteryHolder.gameObject.SetActive(false);

                    //switch cams
                    mainCam.gameObject.SetActive(true);
                    mysteryCam.gameObject.SetActive(false);

                    //turn off the mystery HUD
                    mysteryHUD.SetActive(false);
                    mainHUD.SetActive(true);

                    GameControl.GameController.currentState = GameState.DEFAULT;
                    Time.timeScale = 1f;
                }    
            }
            else
            {
                // Toggle the entire GameObject if no canvas is found
                gameObject.SetActive(!gameObject.activeSelf);
            }
        }

        /*if (Input.GetKey(KeyCode.Mouse1) && GameControl.GameController.currentState == GameState.MYSTERY)
        {
            
            float mouseDeltaX = Input.GetAxis("Mouse X") * 2.0f * Time.deltaTime;
            float mouseDeltaY = Input.GetAxis("Mouse Y") * 2.0f * Time.deltaTime;

            Debug.Log(mouseDeltaX + " " + mouseDeltaY);

            mysteryHolder.anchoredPosition = mysteryHolder.anchoredPosition + new Vector2(mouseDeltaX, mouseDeltaY);
        }*/
    }

    private void FixedUpdate()
    {
        
    }
}
