using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using System.IO;
using LLMUnity;

public class UIController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField inputField;
    public TMP_Text outputText;
    public TMP_Text llmStatusText;     
    public TMP_Text characterStatusText;
    public Button submitButton;

    [Header("Chat Settings")]
    [SerializeField] private int maxMessages = 3;

    [Header("LLM References")]
    public LLMCharacter llmCharacter;

    private List<ChatMessage> messageHistory;
    private StringBuilder currentResponse;
    private string lastReply = "";
    private bool isProcessing = false;
    private bool isCleanedUp = false;
    private bool isLLMReady = false;
    private string currentCharacterName = "Character";

    /// <summary>
    /// Switches the LLM to roleplay as a new character based on the provided JSON file
    /// </summary>
    /// <param name="characterJsonPath">Path to the character's JSON file</param>
    /// <returns>True if character switch was successful, false otherwise</returns>
    public async Task<bool> SwitchCharacter(string characterJsonPath)
    {
        if (!isLLMReady)
        {
            Debug.LogError("Cannot switch character - LLM not ready");
            return false;
        }

        try
        {
            string jsonContent = await File.ReadAllTextAsync(characterJsonPath);
            string systemPrompt = CharacterPromptGenerator.GenerateSystemPrompt(jsonContent);

            if (string.IsNullOrEmpty(systemPrompt))
            {
                Debug.LogError($"Failed to generate prompt from character file: {characterJsonPath}");
                return false;
            }

            ResetChat();
            llmCharacter.SetPrompt(systemPrompt);

            Debug.Log($"Successfully switched to character from: {characterJsonPath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error switching character: {e.Message}");
            return false;
        }
    }

    public void SetCurrentCharacter(string characterName)
    {
        currentCharacterName = characterName;
    }


    private class ChatMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }

    void Start()
    {
        Application.targetFrameRate = 30;
        QualitySettings.vSyncCount = 0;
        InitializeComponents();
    }

    void Update()
    {
        UpdateDisplay();
    }

    void OnEnable()
    {
        if (isCleanedUp)
        {
            InitializeComponents();
        }
    }

    void OnDisable()
    {
        CleanupComponents();
    }

    void OnDestroy()
    {
        CleanupComponents();
    }

    private void InitializeComponents()
    {
        messageHistory = new List<ChatMessage>();
        currentResponse = new StringBuilder();
        lastReply = "";
        isCleanedUp = false;
        isLLMReady = false;

        UpdateLLMStatus("Initializing...");
        UpdateCharacterStatus("No character loaded");

        // Disable until LLM is ready
        if (inputField)
        {
            inputField.interactable = false;
            inputField.onSubmit.RemoveAllListeners();
            inputField.onSubmit.AddListener(OnSubmit);
        }

        if (submitButton)
        {
            submitButton.interactable = false;
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(OnSubmitClicked);
        }

        if (llmCharacter != null)
        {
            llmCharacter.stream = true;
            StartCoroutine(InitializeLLMCoroutine());
        }
        else
        {
            Debug.LogError("LLMCharacter reference missing!");
            UpdateStatus("Error: LLM not found!");
        }

    }

    private IEnumerator InitializeLLMCoroutine()
    {
        UpdateLLMStatus("Loading LLM...");

        // Fire off the async warmup.
        Task warmupTask = null;
        try
        {
            warmupTask = llmCharacter.Warmup();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error scheduling LLM warmup: {e}");
            UpdateStatus("Error: Unable to start LLM warmup!");
            yield break;
        }

        // Wait until it's done (yield each frame).
        while (!warmupTask.IsCompleted)
        {
            yield return null;
        }

        // Check if an exception occurred in the async method:
        if (warmupTask.Exception != null)
        {
            // Flatten to see the underlying errors
            Debug.LogError($"Error during LLM warmup: {warmupTask.Exception.Flatten()}");
            UpdateStatus("Error: LLM initialization failed!");
            yield break;
        }

        // If no exception, warmup succeeded:
        OnWarmupComplete();
        UpdateLLMStatus("LLM Ready");
    }

    private void OnWarmupComplete()
    {
        if (!this.enabled) return;

        isLLMReady = true;
        submitButton.interactable = true;
        inputField.interactable = true;
        UpdateLLMStatus("Model ready");
    }

    private void CleanupComponents()
    {
        if (!isCleanedUp)
        {
            if (isProcessing){CancelRequest();}
            if (inputField){inputField.onSubmit.RemoveAllListeners();}
            if (submitButton){submitButton.onClick.RemoveAllListeners();}

            messageHistory?.Clear();
            currentResponse?.Clear();
            lastReply = "";

            if (outputText) outputText.text = "";
            if (inputField) inputField.text = "";
            if (llmStatusText) llmStatusText.text = "";
            if (characterStatusText) characterStatusText.text = "";

            isCleanedUp = true;
            isLLMReady = false;
        }
    }

    public void UpdateLLMStatus(string status)
    {
        if (llmStatusText && !isCleanedUp)
        {
            llmStatusText.text = status;
        }
    }

    public void UpdateCharacterStatus(string status)
    {
        if (characterStatusText && !isCleanedUp)
        {
            characterStatusText.text = status;
        }
    }

    private void UpdateDisplay()
    {
        if (outputText && !isCleanedUp)
        {
            StringBuilder display = new StringBuilder();

            var recentMessages = messageHistory
                .Skip(Math.Max(0, messageHistory.Count - maxMessages))
                .ToList();

            foreach (var message in recentMessages)
            {
                display.Append($"{message.Role}: {message.Content}\n\n");
            }

            if (currentResponse.Length > 0)
            {
                display.Append($"{currentCharacterName}: {currentResponse}");
            }

            outputText.text = display.ToString();
        }
    }

    void UpdateStatus(string status)
    {
        UpdateLLMStatus(status);
    }

    void HandleReply(string reply)
    {
        if (!string.IsNullOrEmpty(reply) && !isCleanedUp)
        {
            try
            {
                if (reply.Length > lastReply.Length && reply.StartsWith(lastReply))
                {
                    string newContent = reply.Substring(lastReply.Length);
                    currentResponse.Append(newContent);
                    lastReply = reply;
                }
                else if (reply.Length <= lastReply.Length)
                {
                    return;
                }
                else
                {
                    currentResponse.Append(reply);
                    lastReply = reply;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in HandleReply: {e}");
                OnError();
            }
        }
    }

    void OnReplyComplete()
    {
        if (!isCleanedUp)
        {
            try
            {
                messageHistory.Add(new ChatMessage
                {
                    Role = currentCharacterName,
                    Content = currentResponse.ToString()
                });

                currentResponse.Clear();
                lastReply = "";
                isProcessing = false;
                submitButton.interactable = true;
                inputField.interactable = true;
                UpdateLLMStatus("Ready for input");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in OnReplyComplete: {e}");
                OnError();
            }
        }
    }

    private void OnError()
    {
        isProcessing = false;
        if (isLLMReady)
        {
            submitButton.interactable = true;
            inputField.interactable = true;
            UpdateLLMStatus("Response error - try again");
        }
    }

    public void OnSubmit(string text)
    {
        // For inputField.onSubmit (Enter key)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnSubmitClicked();
        }
    }

    public void OnSubmitClicked()
    {
        if (!isLLMReady || isProcessing || isCleanedUp
            || string.IsNullOrEmpty(inputField.text) || llmCharacter == null)
        {
            return;
        }

        try
        {
            isProcessing = true;
            submitButton.interactable = false;
            inputField.interactable = false;
            UpdateStatus("Processing...");

            string userInput = inputField.text;

            messageHistory.Add(new ChatMessage
            {
                Role = "Player",
                Content = userInput
            });

            inputField.text = "";

            currentResponse.Clear();
            lastReply = "";
            _ = llmCharacter.Chat(userInput, HandleReply, OnReplyComplete);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in OnSubmitClicked: {e}");
            OnError();
        }
    }

    public void CancelRequest()
    {
        if (isProcessing && llmCharacter != null)
        {
            llmCharacter.CancelRequests();
        }

        isProcessing = false;
        if (isLLMReady)
        {
            submitButton.interactable = true;
            inputField.interactable = true;
            UpdateStatus("Ready to chat!");
        }

        currentResponse.Clear();
        lastReply = "";
    }

    public void ResetChat()
    {
        if (isProcessing)
        {
            CancelRequest();
        }

        // Clear local message history
        messageHistory.Clear();
        currentResponse.Clear();
        lastReply = "";
        if (outputText != null){outputText.text = "";}

        // Clear LLM's memory
        if (llmCharacter != null){llmCharacter.ClearChat();}

        UpdateLLMStatus(isLLMReady ? "Ready for input" : "Loading model...");
    }
}
