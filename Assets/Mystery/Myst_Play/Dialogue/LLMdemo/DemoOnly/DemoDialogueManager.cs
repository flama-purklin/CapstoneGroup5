using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class DemoDialogueManager : BaseDialogueManager
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text outputText;
    [SerializeField] private TMP_Text llmStatusText;
    [SerializeField] private TMP_Text characterStatusText;
    [SerializeField] private Button submitButton;

    [Header("Chat Settings")]
    [SerializeField] private int maxMessages = 3;

    [Header("References")]
    public CharacterManager characterManager;

    private List<ChatMessage> messageHistory = new List<ChatMessage>();
    private bool isLLMReady = false;
    private string currentCharacterName = "Character";

    private class ChatMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }

    protected override void Start()
    {
        base.Start();
        Application.targetFrameRate = 30;
        QualitySettings.vSyncCount = 0;

        messageHistory = new List<ChatMessage>();
        UpdateLLMStatus("Initializing...");
        UpdateCharacterStatus("No character loaded");
        DisableInput();

        if (characterManager == null)
        {
            Debug.LogError("CharacterManager reference missing!");
            UpdateLLMStatus("Error: CharacterManager not found!");
            return;
        }

        // Subscribe to initialization event
        characterManager.OnInitializationComplete += OnCharacterManagerInitialized;

        // Check if already initialized
        if (characterManager.IsInitialized)
        {
            OnCharacterManagerInitialized();
        }
    }

    private void OnCharacterManagerInitialized()
    {
        Debug.Log("Character Manager Initialized - Enabling input system");
        OnWarmupComplete();
    }

    protected override void SetupInputHandlers()
    {
        Debug.Log("Setting up input handlers");
        if (submitButton)
        {
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(OnSubmitClicked);
            Debug.Log("Submit button handler set up");
        }
        else
        {
            Debug.LogError("Submit button reference missing!");
        }

        if (inputField)
        {
            inputField.onSubmit.RemoveAllListeners();
            inputField.onSubmit.AddListener(OnSubmit);
            Debug.Log("Input field handler set up");
        }
        else
        {
            Debug.LogError("Input field reference missing!");
        }
    }

    public void OnSubmit(string text)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnSubmitClicked();
        }
    }

    private void OnSubmitClicked()
    {
        if (!isLLMReady)
        {
            Debug.LogWarning("Tried to submit while LLM not ready");
            return;
        }

        if (isProcessingResponse)
        {
            Debug.LogWarning("Tried to submit while processing response");
            return;
        }

        if (string.IsNullOrEmpty(inputField.text))
        {
            Debug.LogWarning("Tried to submit empty text");
            return;
        }

        if (llmCharacter == null)
        {
            Debug.LogWarning("Tried to submit with no character selected");
            return;
        }

        string userInput = inputField.text;
        messageHistory.Add(new ChatMessage { Role = "Player", Content = userInput });

        inputField.text = "";
        isProcessingResponse = true;
        DisableInput();
        currentResponse.Clear();
        lastReply = "";

        _ = llmCharacter.Chat(userInput, HandleReply, OnReplyComplete);
    }

    protected override void OnReplyComplete()
    {
        base.OnReplyComplete();
        messageHistory.Add(new ChatMessage
        {
            Role = currentCharacterName,
            Content = currentResponse.ToString()
        });
        UpdateLLMStatus("Ready for input");
    }

    protected override void EnableInput()
    {
        Debug.Log("Enabling input");
        if (inputField)
        {
            inputField.interactable = true;
            try
            {
                inputField.ActivateInputField();
                Debug.Log("Input field activated");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error activating input field: {e}");
            }
        }
        else
        {
            Debug.LogError("Input field reference lost!");
        }

        if (submitButton)
        {
            submitButton.interactable = true;
            Debug.Log("Submit button enabled");
        }
        else
        {
            Debug.LogError("Submit button reference lost!");
        }
    }

    protected override void DisableInput()
    {
        Debug.Log("Disabling input");
        if (inputField)
        {
            inputField.interactable = false;
            Debug.Log("Input field disabled");
        }
        if (submitButton)
        {
            submitButton.interactable = false;
            Debug.Log("Submit button disabled");
        }
    }

    protected override void UpdateDialogueDisplay(string currentText)
    {
        if (!outputText) return;

        var displayText = new System.Text.StringBuilder();
        var recentMessages = messageHistory
            .Skip(System.Math.Max(0, messageHistory.Count - maxMessages))
            .ToList();

        foreach (var message in recentMessages)
        {
            displayText.Append($"{message.Role}: {message.Content}\n\n");
        }

        if (currentResponse.Length > 0)
        {
            displayText.Append($"{currentCharacterName}: {currentText}");
        }

        outputText.text = displayText.ToString();
    }

    public void SetCurrentCharacter(string characterName)
    {
        currentCharacterName = characterName;
    }

    public void OnWarmupComplete()
    {
        Debug.Log("Warmup complete - Enabling LLM system");
        isLLMReady = true;
        EnableInput();
        UpdateLLMStatus("Model ready");
    }

    public void UpdateLLMStatus(string status)
    {
        if (llmStatusText) llmStatusText.text = status;
    }

    public void UpdateCharacterStatus(string status)
    {
        if (characterStatusText) characterStatusText.text = status;
    }

    public override async Task ResetDialogue()
    {
        await base.ResetDialogue();
        messageHistory.Clear();
        if (outputText) outputText.text = "";
        if (inputField) inputField.text = "";
    }

    private void OnDestroy()
    {
        if (characterManager != null)
        {
            characterManager.OnInitializationComplete -= OnCharacterManagerInitialized;
        }
    }
}