using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class DialogueControl : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private LLMDialogueManager llmDialogueManager;
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private GameObject defaultHud;
    [SerializeField] private RectTransform dialoguePanel;

    [Header("Canvas Components")]
    [SerializeField] Image characterProf;
    [SerializeField] TMP_Text characterName;

    [SerializeField] private Animator anim;

    private bool isTransitioning = false;
    private bool shutdown = false;

    private void Start()
    {
        if (!llmDialogueManager)
        {
            Debug.LogError("LLMDialogueManager reference not set in DialogueControl!!");
            enabled = false;
            return;
        }

        if (!dialogueCanvas)
        {
            Debug.LogError("DialogueCanvas reference not set!");
            enabled = false;
            return;
        }

        if (!anim)
        {
            Debug.LogError("Animator reference not set!");
            enabled = false;
            return;
        }

        dialogueCanvas.SetActive(false);
    }

    public async void Activate(GameObject npcObject)
    {
        if (isTransitioning) return;

        Debug.Log($"Attempting to activate dialogue with {npcObject.name}");

        Character character = npcObject.GetComponent<Character>();
        if (character == null)
        {
            Debug.LogError($"No Character component found on NPC: {npcObject.name}");
            return;
        }

        var llmCharacter = await character.GetLLMCharacter();
        if (llmCharacter == null)
        {
            Debug.LogError($"Failed to get LLMCharacter for {character.GetCharacterName()}");
            return;
        }

        llmDialogueManager.SetCharacter(llmCharacter);
        GameControl.GameController.currentState = GameState.DIALOGUE;

        if (defaultHud)
        {
            defaultHud.SetActive(false);
        }

        StartCoroutine(ActivateDialogue());
    }

    private IEnumerator ActivateDialogue()
    {
        isTransitioning = true;
        dialogueCanvas.SetActive(true);
        anim.Play("DialogueActivate");

        // wait for animation to start
        yield return null;

        // wait for animation to complete
        while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
        {
            yield return null;
        }

        llmDialogueManager.InitializeDialogue();
        isTransitioning = false;

        //return null; // for now...
    }

    private IEnumerator DeactivateDialogue()
    {
        isTransitioning = true;

        // Start the reset process
        var resetTask = llmDialogueManager.ResetDialogue();
        while (!resetTask.IsCompleted)
        {
            yield return null;
        }

        anim.Play("DialogueDeactivate");

        // Wait for animation to complete
        while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
        {
            yield return null;
        }

        dialogueCanvas.SetActive(false);

        if (GameControl.GameController.currentState == GameState.DIALOGUE)
        {
            defaultHud.SetActive(true);
            GameControl.GameController.currentState = GameState.DEFAULT;
        }

        isTransitioning = false;
    }

    private void Update()
    {
        if (isTransitioning) return;

        if ((Input.GetKeyDown(KeyCode.Escape) && GameControl.GameController.currentState == GameState.DIALOGUE) ||
            (GameControl.GameController.currentState == GameState.FINAL && !shutdown))
        {
            if (GameControl.GameController.currentState == GameState.FINAL)
            {
                shutdown = true;
            }
            StartCoroutine(DeactivateDialogue());
        }
    }

    public void Deactivate()
    {
        if (isTransitioning || !dialogueCanvas.activeInHierarchy) return;
        StartCoroutine(DeactivateDialogue());
    }
}