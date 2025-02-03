using UnityEngine;
using TMPro;
using UnityEngine.UI;
using LLMUnity;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class UIController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField inputField;
    public TMP_Text outputText;
    public TMP_Text statusText;
    public Button submitButton;

    [Header("Chat Settings")]
    [SerializeField] private int maxMessages = 4;

    [Header("LLM References")]
    public LLMCharacter llmCharacter;

    [Header("Performance")]

    private List<ChatMessage> messageHistory;
    private StringBuilder currentResponse;
    private string lastReply = "";
    private bool isProcessing = false;
    private bool isCleanedUp = false;
    private bool isLLMReady = false;

    private class ChatMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }

    void Start()
    {
        Application.targetFrameRate = 60; // Limit frame rate
        QualitySettings.vSyncCount = 0;   // Disable VSync for better control
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

        // Disable interaction until LLM is ready
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
            StartCoroutine(InitializeLLMCoroutine());  // Changed this line
        }
        else
        {
            Debug.LogError("LLMCharacter reference missing!");
            UpdateStatus("Error: LLM not found!");
        }
    }

    // Add these new methods
    private IEnumerator InitializeLLMCoroutine()
    {
        UpdateStatus("Loading LLM...");

        bool initializationComplete = false;
        bool hasError = false;

        try
        {
            llmCharacter.Warmup(() =>
            {
                initializationComplete = true;
                OnWarmupComplete();
            });
        }
        catch (Exception e)
        {
            Debug.LogError($"Error during LLM initialization: {e}");
            UpdateStatus("Error: LLM initialization failed!");
            hasError = true;
        }

        if (!hasError)
        {
            // Wait for initialization to complete
            while (!initializationComplete)
            {
                yield return new WaitForEndOfFrame();
            }
        }
    }

    private void OnWarmupComplete()
    {
        if (!this.enabled) return;  // Safety check

        isLLMReady = true;
        submitButton.interactable = true;
        inputField.interactable = true;
        UpdateStatus("Ready to chat!");
    }

    private void EnableInteraction()
    {
        if (isLLMReady && !isCleanedUp)
        {
            submitButton.interactable = true;
            inputField.interactable = true;
            UpdateStatus("Ready to chat!");
        }
    }

    private void CleanupComponents()
    {
        if (!isCleanedUp)
        {
            if (isProcessing)
            {
                CancelRequest();
            }

            if (inputField)
            {
                inputField.onSubmit.RemoveAllListeners();
            }

            if (submitButton)
            {
                submitButton.onClick.RemoveAllListeners();
            }

            messageHistory?.Clear();
            currentResponse?.Clear();
            lastReply = "";

            if (outputText) outputText.text = "";
            if (inputField) inputField.text = "";
            if (statusText) statusText.text = "";

            isCleanedUp = true;
            isLLMReady = false;
        }
    }

    private void UpdateDisplay()
    {
        if (outputText && !isCleanedUp)
        {
            StringBuilder display = new StringBuilder();

            var recentMessages = messageHistory.Skip(Math.Max(0, messageHistory.Count - maxMessages)).ToList();

            foreach (var message in recentMessages)
            {
                display.Append($"{message.Role}: {message.Content}\n\n");
            }

            if (currentResponse.Length > 0)
            {
                display.Append($"Ms.Winchester: {currentResponse}");
            }

            outputText.text = display.ToString();
        }
    }

    void UpdateStatus(string status)
    {
        if (statusText && !isCleanedUp)
        {
            statusText.text = status;
        }
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
                    Role = "Ms.Winchester",
                    Content = currentResponse.ToString()
                });

                currentResponse.Clear();
                lastReply = "";
           

                isProcessing = false;
                submitButton.interactable = true;
                inputField.interactable = true;
                UpdateStatus("Ready to chat!");

   
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
            UpdateStatus("Error occurred - Please try again");
        }
    }

    public void OnSubmit(string text)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnSubmitClicked();
        }
    }

    public void OnSubmitClicked()
    {
        if (!isLLMReady || isProcessing || isCleanedUp || string.IsNullOrEmpty(inputField.text) || llmCharacter == null)
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

        messageHistory.Clear();
        currentResponse.Clear();
        lastReply = "";
       
        UpdateStatus(isLLMReady ? "Chat reset - Ready to chat!" : "Loading LLM...");
    }
}