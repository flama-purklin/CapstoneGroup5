using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;
using System;

public class LLMDialogueManager : BaseDialogueManager
{
    [Header("UI References (Legacy/Internal)")]
    // These might still be used internally or by the base class, but input is handled by DialogueUIController
    [SerializeField] private TMP_Text playerDialogueText;
    [SerializeField] private TMP_Text npcDialogueText;
    // [SerializeField] private TMP_InputField inputField; // REMOVED - Handled by DialogueUIController
    [SerializeField] private GameObject inputBox; // Keep for Enable/DisableInput
    // [SerializeField] private Button submitButton; // REMOVED - Handled by DialogueUIController
    [SerializeField] private DialogueControl dialogueControlRef; // Keep for DisplayNPCDialogueStreaming

    private void Awake()
    {
        // Initialization code if needed
    }

    // Provide empty implementation for abstract base method, as input is handled by DialogueUIController
    protected override void SetupInputHandlers() 
    {
        // Intentionally empty - listeners are set up in DialogueUIController
        Debug.Log("[LLMDialogueManager] SetupInputHandlers called (empty implementation).");
    }

    public void RegisterDialogueControl(DialogueControl control)
    {
        this.dialogueControlRef = control;
        base.dialogueControl = control; // Also set it in the base class
    }

    // REMOVED OnSubmitClicked - Logic moved to SubmitPlayerInputToLLM, called by DialogueUIController
    // private void OnSubmitClicked() { ... }

    /// <summary>
    /// Public method called by DialogueUIController to submit player input to the LLM.
    /// </summary>
    public void SubmitPlayerInputToLLM(string userInput)
    {
        if (isProcessingResponse || string.IsNullOrEmpty(userInput) || llmCharacter == null)
        {
            Debug.LogWarning($"[LLMDialogueManager] SubmitPlayerInputToLLM called but rejected. isProcessing={isProcessingResponse}, inputNullOrEmpty={string.IsNullOrEmpty(userInput)}, llmCharNull={llmCharacter == null}");
            return;
        }

        // Optionally update legacy player dialogue text if it exists
        if (playerDialogueText)
        {
            playerDialogueText.text = userInput;
        }

        // Core LLM submission logic
        isProcessingResponse = true;
        DisableInput(); // Disable the input box container
        currentResponse.Clear();
        lastReply = "";

        // Drain power here
        if (GameControl.GameController.powerControl != null) {
             GameControl.GameController.powerControl.PowerDrain(0.005f);
        } else {
            Debug.LogWarning("[LLMDialogueManager] PowerControl reference not found on GameControl!");
        }


        Debug.Log($"[LLMDialogueManager] Sending to LLM: '{userInput}'");
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
        // Forward text display to DialogueControl (which uses BeepSpeak via DialogueUIController)
        if (dialogueControlRef != null)
        {
            dialogueControlRef.DisplayNPCDialogueStreaming(text);
        }
        else if (npcDialogueText != null) // Fallback if DialogueControl ref is missing
        {
            npcDialogueText.text = text;
        }
    }

    protected override void ProcessFunctionCall(string functionCall)
    {
        // Use our direct reference to dialogueControlRef if available, otherwise fall back to base implementation
        if (functionCall.StartsWith("stop_conversation") && dialogueControlRef != null)
        {
            Debug.Log("Character requested to end conversation (using direct reference)");
            dialogueControlRef.Deactivate();
        }
        else
        {
            // Let the base implementation handle it
            base.ProcessFunctionCall(functionCall);
        }
    }

    protected override void EnableInput()
    {
        // Only enable/disable the container, not the specific input field/button
        if (inputBox) 
        {
            Debug.Log("[DIAGDBG] LLMDialogueManager.EnableInput - activating inputBox");
            inputBox.SetActive(true);
        }
        else
        {
            Debug.LogError("[DIAGDBG] LLMDialogueManager.EnableInput - inputBox is null");
        }

        // For diagnostic purposes, verify the activation in DialogueUIController
        if (dialogueControlRef && dialogueControlRef.UseNewUI)
        {
            Debug.Log("[DIAGDBG] LLMDialogueManager.EnableInput - checking DialogueUIController isWaitingForResponse status");
        }

        Debug.Log("[LLMDialogueManager] Input Enabled (InputBox Active)");
    }

    protected override void DisableInput()
    {
        Debug.Log("[DIAGDBG] LLMDialogueManager.DisableInput called");
        // Only enable/disable the container
        // if (inputField) inputField.interactable = false; // REMOVED
        // if (submitButton) submitButton.interactable = false; // REMOVED
        // Note: We might not want to hide the inputBox entirely, just make the field non-interactable.
        // However, DialogueUIController now manages the actual input field's interactability.
        // This method might become redundant or only control the inputBox visibility if needed.
        // For now, let's keep the inputBox visibility control.
        if (inputBox) 
        {
            Debug.Log("[DIAGDBG] LLMDialogueManager.DisableInput - deactivating inputBox");
            inputBox.SetActive(false);
        }
        else
        {
            Debug.LogError("[DIAGDBG] LLMDialogueManager.DisableInput - inputBox is null");
        }
        Debug.Log("[LLMDialogueManager] Input Disabled (InputBox Inactive)");
    }

    public override async Task ResetDialogue()
    {
        await base.ResetDialogue();

        if (playerDialogueText) playerDialogueText.text = "";
        if (npcDialogueText) npcDialogueText.text = "";
        // if (inputField) inputField.text = ""; // REMOVED
    }
}
