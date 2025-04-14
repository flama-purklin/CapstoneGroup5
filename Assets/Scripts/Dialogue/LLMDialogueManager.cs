using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;
using System;


public class LLMDialogueManager : BaseDialogueManager
{
    [Header("UI References")]
    [SerializeField] private TMP_Text playerDialogueText;
    [SerializeField] private TMP_Text npcDialogueText;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject inputBox;
    [SerializeField] private Button submitButton;
    [SerializeField] private new DialogueControl dialogueControl;

    protected override void SetupInputHandlers()
    {
        if (submitButton)
        {
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(OnSubmitClicked);
        }

        if (inputField)
        {
            inputField.onSubmit.RemoveAllListeners();
            inputField.onSubmit.AddListener((text) => {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    OnSubmitClicked();
                }
            });
        }
    }

    public void RegisterDialogueControl(DialogueControl control)
    {
        this.dialogueControl = control;
        base.dialogueControl = control; // Also set it in the base class
    }

    private void OnSubmitClicked()
    {
        if (isProcessingResponse || string.IsNullOrEmpty(inputField.text) || llmCharacter == null)
            return;

        string userInput = inputField.text;

        if (playerDialogueText)
        {
            playerDialogueText.text = userInput;
        }

        inputField.text = "";
        isProcessingResponse = true;
        DisableInput();
        currentResponse.Clear();
        lastReply = "";

        _ = llmCharacter.Chat(userInput, HandleReply, OnReplyComplete);
    }

    protected override void UpdateDialogueDisplay(string text)
    {
        /* Old implementation
        if (npcDialogueText)
        {
            npcDialogueText.text = text;
        }
        */
        if (dialogueControl != null)
        {
            dialogueControl.DisplayNPCDialogueStreaming(text);
        }
        else if (npcDialogueText != null)
        {
            npcDialogueText.text = text;
        }
    }
    
    protected override void ProcessFunctionCall(string functionCall)
    {
        // Use our direct reference to dialogueControl if available, otherwise fall back to base implementation
        if (functionCall.StartsWith("stop_conversation") && dialogueControl != null)
        {
            Debug.Log("Character requested to end conversation (using direct reference)");
            dialogueControl.Deactivate();
        }
        else
        {
            // Let the base implementation handle it
            base.ProcessFunctionCall(functionCall);
        }
    }

    protected override void EnableInput()
    {
        if (inputBox) inputBox.SetActive(true);
        if (inputField)
        {
            inputField.interactable = true;
            inputField.ActivateInputField();
        }
        if (submitButton) submitButton.interactable = true;
    }

    protected override void DisableInput()
    {
        if (inputField) inputField.interactable = false;
        if (submitButton) submitButton.interactable = false;
    }

    public override async Task ResetDialogue()
    {
        await base.ResetDialogue();

        if (playerDialogueText) playerDialogueText.text = "";
        if (npcDialogueText) npcDialogueText.text = "";
        if (inputField) inputField.text = "";
    }
}