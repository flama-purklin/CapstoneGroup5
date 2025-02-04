using System.Collections;
using UnityEngine;

public class DialogueControl : MonoBehaviour
{
    //character present in dialogue
    GameObject currentChar;

    //UI Elements
    [SerializeField] GameObject dialogueCanvas;
    [SerializeField] RectTransform inputBox;
    [SerializeField] RectTransform dialogueHolder;
    [SerializeField] RectTransform characterImage;
    [SerializeField] RectTransform playerImage;
    [SerializeField] RectTransform exitButton;

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
        yield return null;
    }

    IEnumerator DialogueExitLerp()
    {
        dialogueCanvas.SetActive(false);
        GameControl.GameController.currentState = GameState.DEFAULT;
        yield return null;
    }
}
