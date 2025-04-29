using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUIController : MonoBehaviour
{
    [Header("Canvas & Fade")]
    [SerializeField] private CanvasGroup dialogueCanvasGroup;
    [SerializeField] private CanvasFade canvasFade;

    [Header("Character Info")]
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private Image characterPortraitImage;

    [Header("Dialogue Components")]
    [SerializeField] private ScrollRect responseScrollRect;
    [SerializeField] private TextMeshProUGUI responseText;
    [SerializeField] private ScrollFadeManager scrollFadeManager;

    [Header("Input Components")]
    [SerializeField] private TMP_InputField playerInputField;
    [SerializeField] private InputGhostManager inputGhostManager;
    [SerializeField] private DynamicInputHeight dynamicInputHeight;

    [Tooltip("Threshold for auto‑scroll (<= this value)")]
    [SerializeField, Range(0f, 1f)] private float autoScrollThreshold = 0.1f;

    // public event Action<string> OnPlayerMessageSubmitted; // REMOVED - No longer needed

    private bool isWaitingForResponse = false;
    private string currentResponse = string.Empty;
    private DialogueControl dialogueControl; // Reference to the main controller

    // Add a property to expose the responseText for BeepSpeak
    public TextMeshProUGUI ResponseTextComponent => responseText;

    private void Awake()
    {
        Debug.Log("[DialogueUIController] Awake called");
        
        if (dialogueCanvasGroup == null)
        {
            dialogueCanvasGroup = GetComponent<CanvasGroup>();
            Debug.Log($"[DialogueUIController] Found dialogueCanvasGroup: {dialogueCanvasGroup != null}");
        }
        
        if (canvasFade == null)
        {
            canvasFade = GetComponent<CanvasFade>();
            Debug.Log($"[DialogueUIController] Found canvasFade: {canvasFade != null}");
        }
        
        Debug.Log($"[DialogueUIController] responseText exists: {responseText != null}");
        Debug.Log($"[DialogueUIController] inputGhostManager exists before fallback: {inputGhostManager != null}");
        // Fallback: locate InputGhostManager in children if not assigned
        if (inputGhostManager == null)
        {
            inputGhostManager = GetComponentInChildren<InputGhostManager>(true); // true to include inactive objects
            Debug.Log($"[DialogueUIController] Fallback assigned inputGhostManager via GetComponentInChildren: {inputGhostManager != null}");
        }

        // Get reference to the parent DialogueControl
        dialogueControl = GetComponentInParent<DialogueControl>();
        if (dialogueControl == null) {
            Debug.LogError("[DialogueUIController] Could not find DialogueControl in parent hierarchy!");
        } else {
            Debug.Log("[DialogueUIController] Successfully found DialogueControl reference.");
        }
    }

    private void Start()
    {
        Debug.Log("[DialogueUIController] Start called");
        
        if (inputGhostManager != null)
        {
            Debug.Log($"[DialogueUIController] inputGhostManager reference OK, GameObject: {inputGhostManager.gameObject.name}");
            inputGhostManager.LogCurrentState();
        }
        else
        {
            Debug.LogError("[DialogueUIController] inputGhostManager reference is NULL!");
        }
        
        playerInputField.onSubmit.AddListener(OnPlayerSubmitInput);
        Debug.Log("[DialogueUIController] Registered OnPlayerSubmitInput to inputField.onSubmit");

        // Ensure the input field is single-line so Enter triggers submit and also listen to end-edit
        playerInputField.lineType = TMP_InputField.LineType.SingleLine;
        playerInputField.onEndEdit.AddListener(OnPlayerSubmitInput);
        Debug.Log("[DialogueUIController] Registered OnPlayerSubmitInput to inputField.onEndEdit and set lineType to SingleLine");

        // initialize hidden
        dialogueCanvasGroup.alpha = 0f;
        dialogueCanvasGroup.blocksRaycasts = false;
        Debug.Log("[DialogueUIController] Set dialogueCanvasGroup invisible");
    }

    private void OnDestroy()
    {
        playerInputField.onSubmit.RemoveListener(OnPlayerSubmitInput);
        // Also remove the onEndEdit listener we added
        playerInputField.onEndEdit.RemoveListener(OnPlayerSubmitInput);
    }

    /// <summary>
    /// Show the dialogue UI for a given character.
    /// </summary>
    public void ShowDialogue(string characterName, Sprite characterPortrait = null)
    {
        Debug.Log($"[DialogueUIController] ShowDialogue called with character: '{characterName}'");
        
        characterNameText.text = characterName;
        if (characterPortraitImage != null && characterPortrait != null)
            characterPortraitImage.sprite = characterPortrait;

        Debug.Log("[DialogueUIController] About to call ClearDialogue()");
        ClearDialogue();
        
        // Make sure the GameObject is active before starting the fade coroutine
        if (dialogueCanvasGroup && !dialogueCanvasGroup.gameObject.activeInHierarchy)
        {
            dialogueCanvasGroup.gameObject.SetActive(true);
            Debug.Log("[DialogueUIController] Activated dialogueCanvasGroup GameObject");
        }
        else
        {
            Debug.Log($"[DialogueUIController] dialogueCanvasGroup already active: {dialogueCanvasGroup?.gameObject.activeInHierarchy}");
        }
        
        Debug.Log("[DialogueUIController] About to call canvasFade.FadeIn()");
        canvasFade.FadeIn();
        StartCoroutine(FocusInputNextFrame());
    }

    /// <summary>
    /// Hide the dialogue UI.
    /// </summary>
    public void HideDialogue()
    {
        Debug.Log("[DialogueUIController] HideDialogue called");
        // Clear all UI elements when hiding dialogue
        ClearDialogue();
        canvasFade.FadeOut();
    }

    /// <summary>
    /// Clears UI to initial state.
    /// </summary>
    private void ClearDialogue()
    {
        Debug.Log("[DialogueUIController] ClearDialogue called");
        
        if (inputGhostManager != null)
        {
            Debug.Log("[DialogueUIController] Calling inputGhostManager.ClearGhost()");
            inputGhostManager.ClearGhost();
            inputGhostManager.LogCurrentState();
        }
        else
        {
            Debug.LogError("[DialogueUIController] inputGhostManager is null in ClearDialogue!");
        }
        
        // Clear player input field
        playerInputField.text = string.Empty;
        
        // Clear the response text area
        if (responseText != null)
            responseText.text = string.Empty;
        
        dynamicInputHeight.ResetHeightToMin();
        isWaitingForResponse = false;
        currentResponse = string.Empty;
        ScrollToBottom();
        scrollFadeManager?.ContentChanged();
        
        Debug.Log("[DialogueUIController] ClearDialogue completed");
    }

    /// <summary>
    /// Called when player presses Enter.
    /// </summary>
    private void OnPlayerSubmitInput(string input)
    {
        Debug.Log($"[DialogueUIController] OnPlayerSubmitInput called with input: '{input}'");
        Debug.Log($"[DialogueUIController] isWaitingForResponse: {isWaitingForResponse}, input empty: {string.IsNullOrWhiteSpace(input)}");
        
        if (isWaitingForResponse || string.IsNullOrWhiteSpace(input))
        {
            Debug.Log("[DialogueUIController] Early return from OnPlayerSubmitInput");
            return;
        }

        string trimmed = input.Trim();
        
        // Clear the input field with slight delay to avoid conflicts
        // This helps with onEndEdit called events
        StartCoroutine(ClearInputFieldNextFrame());
        
        dynamicInputHeight.ResetHeightToMin();

        Debug.Log($"[DialogueUIController] Calling inputGhostManager.SetGhostText with: '{trimmed}'");
        if (inputGhostManager != null)
        {
            // CRITICAL FIX: Force activate the InputGhost GameObject if necessary
            if (inputGhostManager.gameObject != null && !inputGhostManager.gameObject.activeSelf)
            {
                inputGhostManager.gameObject.SetActive(true);
                Debug.Log("[DialogueUIController] CRITICAL FIX: Force-activated InputGhost GameObject");
            }
            
            // Now set the ghost text
            inputGhostManager.SetGhostText(trimmed);
            inputGhostManager.LogCurrentState();
            
            // Double-check ghost state after setting text
            Debug.Log($"[DialogueUIController] After SetGhostText - InputGhost active: {inputGhostManager.gameObject.activeSelf}, text set: {!string.IsNullOrEmpty(inputGhostManager.GetCurrentText())}");
        }
        else
        {
            Debug.LogError("[DialogueUIController] inputGhostManager is NULL in OnPlayerSubmitInput!");
        }

        // Clear the response area for the new response - BeepSpeak will handle populating it
        if (responseText != null)
            responseText.text = string.Empty;
        
        ScrollToBottom();
        scrollFadeManager?.ContentChanged();

        isWaitingForResponse = true;
        
        // Get LLM Manager from DialogueControl and submit input
        if (dialogueControl != null) {
            LLMDialogueManager llmManager = dialogueControl.GetLLMDialogueManager();
            if (llmManager != null) {
                // NEW: Check if evidence is selected and append the tag to the message
                string selectedEvidenceId = dialogueControl.RetrieveEvidence();
                string finalInput = trimmed;
                
                if (!string.IsNullOrEmpty(selectedEvidenceId)) {
                    finalInput = trimmed + "\n[PLAYER_SHOWS: " + selectedEvidenceId + "]";
                    Debug.Log($"[DialogueUIController] Added evidence to message. Evidence ID: {selectedEvidenceId}");
                }
                
                Debug.Log($"[DialogueUIController] Calling llmManager.SubmitPlayerInputToLLM with: '{finalInput}'");
                llmManager.SubmitPlayerInputToLLM(finalInput);
                
                // NEW: Reset the evidence dropdown to "No Evidence" after submission
                if (dialogueControl.evidenceSelect != null && dialogueControl.evidenceSelect.options.Count > 0) {
                    // "No Evidence" is always the last option added by UpdateEvidence
                    dialogueControl.evidenceSelect.value = dialogueControl.evidenceSelect.options.Count - 1;
                    dialogueControl.evidenceSelect.RefreshShownValue(); // Update UI
                    Debug.Log("[DialogueUIController] Reset evidence dropdown to default after message submission.");
                }
            } else {
                Debug.LogError("[DialogueUIController] Failed to get LLMDialogueManager from DialogueControl!");
                isWaitingForResponse = false; // Reset flag if submission fails
            }
        } else {
             Debug.LogError("[DialogueUIController] DialogueControl reference is null! Cannot submit input.");
             isWaitingForResponse = false; // Reset flag if submission fails
        }


        StartCoroutine(FocusInputNextFrame());
    }

    // New method to clear the input field properly after a slight delay
    private IEnumerator ClearInputFieldNextFrame()
    {
        // Wait for the current frame to complete 
        yield return null;
        
        // Clear the text field using the public property only
        // Avoid accessing private members m_Text and m_TextComponent
        playerInputField.text = string.Empty;
        
        // Force refresh the input field by deactivating and reactivating it
        playerInputField.DeactivateInputField();
        playerInputField.ActivateInputField();
        
        Debug.Log("[DialogueUIController] Input field cleared on next frame");
    }

    /// <summary>
    /// Called by LLM manager when the full response arrives.
    /// THIS METHOD IS NOW ONLY USED FOR INTERNAL TRACKING, NOT DISPLAY.
    /// BeepSpeak now controls the actual text display.
    /// </summary>
    public void SetNPCResponse(string fullResponse)
    {
        Debug.Log($"[DialogueUIController] SetNPCResponse called, response length: {fullResponse?.Length ?? 0}");
        Debug.Log("[DialogueUIController] NOTE: This method no longer directly sets UI text. BeepSpeak handles display.");
        
        isWaitingForResponse = false;
        currentResponse = fullResponse;
        
        // We no longer set responseText.text here - BeepSpeak will handle that
        // responseText.text = fullResponse; // REMOVED
        
        // ForceShowGhostText call removed as InputGhost is now always visible
        // if (inputGhostManager != null)
        // {
        //     inputGhostManager.ForceShowGhostText(); // REMOVED
        //     inputGhostManager.LogCurrentState();
        // }
        // else
        {
            Debug.LogError("[DialogueUIController] inputGhostManager is NULL in SetNPCResponse!");
        }

        // BeepSpeak will trigger scroll as needed when it updates text
        // ScrollToBottom();
        // scrollFadeManager?.ContentChanged();
        
        Debug.Log("[DialogueUIController] SetNPCResponse completed");
    }

    /// <summary>
    /// For streaming scenarios: append partial text.
    /// THIS METHOD IS NOW ONLY USED FOR INTERNAL TRACKING, NOT DISPLAY.
    /// BeepSpeak now controls the actual text display.
    /// </summary>
    public void AppendToNPCResponse(string chunk)
    {
        if (chunk != null && chunk.Length > 0)
        {
            Debug.Log($"[DialogueUIController] AppendToNPCResponse called with chunk: '{chunk}'");
            Debug.Log("[DialogueUIController] NOTE: This method no longer directly modifies UI text. BeepSpeak handles display.");
        }
        
        currentResponse += chunk;
        // We no longer set responseText.text here - BeepSpeak will handle that
        // responseText.text = currentResponse; // REMOVED

        // Ensure ghost text remains visible during streaming - No longer needed, ghost is always visible
        // if (!string.IsNullOrEmpty(chunk))
        // {
        //     if (inputGhostManager != null)
        //     {
        //         Debug.Log("[DialogueUIController] Calling inputGhostManager.ForceShowGhostText from AppendToNPCResponse");
        //         inputGhostManager.ForceShowGhostText(); // REMOVED
        //     }
        //     else
        //     {
        //         Debug.LogError("[DialogueUIController] inputGhostManager is NULL in AppendToNPCResponse!");
        //     }
        // }

        // BeepSpeak will trigger scroll as needed when it updates text
        // if (responseScrollRect.verticalNormalizedPosition <= autoScrollThreshold)
        //     ScrollToBottom();
        // scrollFadeManager?.ContentChanged();
    }

    /// <summary>
    /// Scrolls to bottom using a targeted rebuild.
    /// This method is public so BeepSpeak can call it.
    /// </summary>
    public void ScrollToBottom()
    {
        if (responseText != null && responseScrollRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(responseText.rectTransform);
            responseScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    /// <summary>
    /// Notify the ScrollFadeManager that content has changed.
    /// This method is public so BeepSpeak can call it.
    /// </summary>
    public void NotifyContentChanged()
    {
        if (scrollFadeManager != null)
            scrollFadeManager.ContentChanged();
    }

    private IEnumerator FocusInputNextFrame()
    {
        yield return null;
        Debug.Log("[DialogueUIController] FocusInputNextFrame activating input field");
        playerInputField.ActivateInputField();
    }
    
    // For debugging from DialogueControl or other components
    public void LogRuntimeState()
    {
        Debug.Log("==== DIALOGUE UI CONTROLLER STATE ====");
        Debug.Log($"[DialogueUIController] GameObject active: {gameObject.activeInHierarchy}");
        Debug.Log($"[DialogueUIController] dialogueCanvasGroup: {dialogueCanvasGroup != null}, alpha: {dialogueCanvasGroup?.alpha}");
        Debug.Log($"[DialogueUIController] inputGhostManager exists: {inputGhostManager != null}");
        Debug.Log($"[DialogueUIController] responseText exists: {responseText != null}");
        Debug.Log($"[DialogueUIController] isWaitingForResponse: {isWaitingForResponse}");
        Debug.Log($"[DialogueUIController] currentResponse length: {currentResponse?.Length ?? 0}");
        
        if (inputGhostManager != null)
        {
            Debug.Log("--- Input Ghost Manager State ---");
            inputGhostManager.LogCurrentState();
        }
    }
}
