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

    public event Action<string> OnPlayerMessageSubmitted;

    private bool isWaitingForResponse = false;
    private string currentResponse = string.Empty;

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
        Debug.Log($"[DialogueUIController] inputGhostManager exists: {inputGhostManager != null}");
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

        // initialize hidden
        dialogueCanvasGroup.alpha = 0f;
        dialogueCanvasGroup.blocksRaycasts = false;
        Debug.Log("[DialogueUIController] Set dialogueCanvasGroup invisible");
    }

    private void OnDestroy()
    {
        playerInputField.onSubmit.RemoveListener(OnPlayerSubmitInput);
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
        playerInputField.text = string.Empty;
        dynamicInputHeight.ResetHeightToMin();

        Debug.Log($"[DialogueUIController] Calling inputGhostManager.SetGhostText with: '{trimmed}'");
        if (inputGhostManager != null)
        {
            inputGhostManager.SetGhostText(trimmed);
            inputGhostManager.LogCurrentState();
        }
        else
        {
            Debug.LogError("[DialogueUIController] inputGhostManager is NULL in OnPlayerSubmitInput!");
        }

        // Clear the response area for the new response
        responseText.text = string.Empty;
        ScrollToBottom();
        scrollFadeManager?.ContentChanged();

        isWaitingForResponse = true;
        Debug.Log("[DialogueUIController] About to invoke OnPlayerMessageSubmitted event");
        OnPlayerMessageSubmitted?.Invoke(trimmed);

        StartCoroutine(FocusInputNextFrame());
    }

    /// <summary>
    /// Called by LLM manager when the full response arrives.
    /// </summary>
    public void SetNPCResponse(string fullResponse)
    {
        Debug.Log($"[DialogueUIController] SetNPCResponse called, response length: {fullResponse?.Length ?? 0}");
        
        isWaitingForResponse = false;
        currentResponse = fullResponse;
        responseText.text = fullResponse;

        Debug.Log("[DialogueUIController] About to call inputGhostManager.ForceShowGhostText");
        if (inputGhostManager != null)
        {
            inputGhostManager.ForceShowGhostText();
            inputGhostManager.LogCurrentState();
        }
        else
        {
            Debug.LogError("[DialogueUIController] inputGhostManager is NULL in SetNPCResponse!");
        }

        ScrollToBottom();
        scrollFadeManager?.ContentChanged();
        
        Debug.Log("[DialogueUIController] SetNPCResponse completed");
    }

    /// <summary>
    /// For streaming scenarios: append partial text.
    /// </summary>
    public void AppendToNPCResponse(string chunk)
    {
        if (chunk != null && chunk.Length > 0)
        {
            Debug.Log($"[DialogueUIController] AppendToNPCResponse called with chunk: '{chunk}'");
        }
        
        currentResponse += chunk;
        responseText.text = currentResponse;

        // Ensure ghost text remains visible during streaming
        if (!string.IsNullOrEmpty(chunk))
        {
            if (inputGhostManager != null)
            {
                Debug.Log("[DialogueUIController] Calling inputGhostManager.ForceShowGhostText from AppendToNPCResponse");
                inputGhostManager.ForceShowGhostText();
            }
            else
            {
                Debug.LogError("[DialogueUIController] inputGhostManager is NULL in AppendToNPCResponse!");
            }
        }

        if (responseScrollRect.verticalNormalizedPosition <= autoScrollThreshold)
            ScrollToBottom();

        scrollFadeManager?.ContentChanged();
    }

    /// <summary>
    /// Scrolls to bottom using a targeted rebuild.
    /// </summary>
    private void ScrollToBottom()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(responseText.rectTransform);
        responseScrollRect.verticalNormalizedPosition = 0f;
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
