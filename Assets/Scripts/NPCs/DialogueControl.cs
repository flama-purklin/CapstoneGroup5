using System.Collections;
using UnityEngine;

public class DialogueControl : MonoBehaviour
{
    //character present in dialogue - REPLACE WITH CHARACTER OBJECT
    GameObject currentChar;

    //UI Elements
    [SerializeField] GameObject dialogueCanvas;
    
    //from top
    [SerializeField] RectTransform dialogueHolder;
    [SerializeField] RectTransform playerDialogue;

    //from bottom
    [SerializeField] RectTransform inputBox;
    [SerializeField] RectTransform characterImage;
    [SerializeField] RectTransform submitButton;

    //from right
    [SerializeField] RectTransform rightButtons;

    //from left
    [SerializeField] RectTransform leftButtons;

    [SerializeField] float lerpTime;

    //animation
    [SerializeField] Animator lerpingAnim;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Deactivate();
        }
    }

    //called whenever dialogue system is activated - TODO replace GameObject here with Character data container
    public void Activate(GameObject character)
    {
        currentChar = character;
        dialogueCanvas.SetActive(true);
        StartCoroutine(DialogueEntryLerp());
    }

    public void Deactivate()
    {
        StartCoroutine(DialogueExitLerp());
    }

    IEnumerator DialogueEntryLerp()
    {
        float currentTime = 0f;

        lerpingAnim.speed = 1f;
        lerpingAnim.Play("DialogueActivation");

        while (currentTime < lerpTime)
        {
            //lerp in all objects

            //from top

            //from left

            //from right

            //from bottom

            currentTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        //lerpingAnim.Stop();
        yield return null;
    }

    IEnumerator DialogueExitLerp()
    {
        float currentTime = 0f;

        //lerpingAnim.speed = -1f;
        lerpingAnim.Play("DialogueDeactivation");

        while (currentTime < lerpTime)
        {
            //lerp out all objects

            //from top

            //from left

            //from right

            //from bottom
            Debug.Log(currentTime);
            currentTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        Debug.Log("End Reached");
        dialogueCanvas.SetActive(false);
        GameControl.GameController.currentState = GameState.DEFAULT;
        yield return null;
    }
}
