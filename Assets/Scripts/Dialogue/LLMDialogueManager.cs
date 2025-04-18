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
    [SerializeField] private DialogueControl dialogueControlRef;
    [SerializeField] private DialogueUIController dialogueUIController; // Direct reference to DialogueUIController
    
    private string previousText = string.Empty; // Track previous text to determine new content

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
        this.dialogueControlRef = control;
        base.dialogueControl = control; // Also set it in the base class

        // Try to find dialogueUIController if not set
        if (dialogueUIController == null && control != null)
        {
            dialogueUIController = control.GetComponentInChildren<DialogueUIController>();
            if (dialogueUIController != null)
            {
                Debug.Log("Found and assigned DialogueUIController reference from DialogueControl");
            }
        }
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
        previousText = string.Empty; // Reset previous text on new message

        //drain power here
        GameControl.GameController.powerControl.PowerDrain(0.005f);

        _ = llmCharacter.Chat(userInput, HandleReply, OnReplyComplete);
    }

    protected override void UpdateDialogueDisplay(string text)
    {
        // First priority: Use new DialogueUIController if available
        if (dialogueUIController != null)
        {
            // Calculate only the new part of the text to append
            if (text.Length > previousText.Length && text.StartsWith(previousText))
            {
                // Extract only the new characters
                string newChunk = text.Substring(previousText.Length);
                
                // First update the new UI immediately - don't wait for BeepSpeak
                dialogueUIController.AppendToNPCResponse(newChunk);
                
                // Then feed the text to BeepSpeak if available - but only for sound effects
                // Don't let BeepSpeak control text display timing
                if (dialogueControlRef != null && dialogueControlRef.GetBeepSpeak() != null)
                {
                    // Use a special "sound only" mode for BeepSpeak if available
                    // This keeps the audio synced but doesn't wait for typing animation
                    var beepSpeak = dialogueControlRef.GetBeepSpeak();
                    if (beepSpeak != null)
                    {
                        // Here we're still sending the complete text so BeepSpeak can track
                        // where it should be in the audio, but we don't want it controlling UI display
                        dialogueControlRef.DisplayNPCDialogueStreaming(text);
                        Debug.Log($"[LLMDialogueManager] Streaming to BeepSpeak (audio only): {text.Length} chars");
                    }
                }
                
                Debug.Log($"[LLMDialogueManager] Appending only new chunk: '{newChunk}'");
            }
            else
            {
                // If we can't determine the new part (e.g., first chunk or text changed),
                // clear and set the entire text
                dialogueUIController.SetNPCResponse(text);
                
                // Also reset BeepSpeak with the full text (for audio only)
                if (dialogueControlRef != null && dialogueControlRef.GetBeepSpeak() != null)
                {
                    dialogueControlRef.DisplayNPCDialogueStreaming(text);
                    Debug.Log($"[LLMDialogueManager] Setting complete BeepSpeak text: {text.Length} chars");
                }
                
                Debug.Log($"[LLMDialogueManager] Setting complete response: '{text}'");
            }
            
            // Update previous text for next comparison
            previousText = text;
            return; // Skip legacy UI processing after handling both UI and BeepSpeak
        }
        
        // Second priority: Use DialogueControl for legacy UI with BeepSpeak support
        else if (dialogueControlRef != null)
        {
            dialogueControlRef.DisplayNPCDialogueStreaming(text);
        }
        // Fallback: Use direct text assignment to the old UI component
        else if (npcDialogueText != null)
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
        previousText = string.Empty; // Reset previous text
    }
}