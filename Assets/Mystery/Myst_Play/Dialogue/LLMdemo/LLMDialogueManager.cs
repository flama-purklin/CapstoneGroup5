using UnityEngine;
using TMPro;
using System.Text;
using UnityEngine.UI;
using System.Threading.Tasks;
using LLMUnity;

public class LLMDialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text playerDialogueText;
    [SerializeField] private TMP_Text npcDialogueText;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject inputBox;
    [SerializeField] private Button submitButton;

    private LLMCharacter llmCharacter;
    private bool isProcessingResponse = false;
    private StringBuilder currentResponse;
    private string lastReply = "";

    private void Start()
    {
        currentResponse = new StringBuilder();
        SetupInputHandlers();
        DisableInput();
    }

    private void SetupInputHandlers()
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

    public void SetCharacter(LLMCharacter character)
    {
        llmCharacter = character;
        Debug.Log($"Set LLMCharacter: {character.gameObject.name}");
    }

    public void InitializeDialogue()
    {
        if (isProcessingResponse)
        {
            llmCharacter?.CancelRequests();
            isProcessingResponse = false;
        }

        if (playerDialogueText) playerDialogueText.text = "";
        if (npcDialogueText) npcDialogueText.text = "";
        if (inputField) inputField.text = "";

        currentResponse.Clear();
        lastReply = "";

        EnableInput();
    }

    private void OnSubmitClicked()
    {
        if (isProcessingResponse || string.IsNullOrEmpty(inputField.text) || llmCharacter == null)
            return;

        string userInput = inputField.text;

        if (playerDialogueText){playerDialogueText.text = userInput;}

        inputField.text = "";
        isProcessingResponse = true;
        DisableInput();
        currentResponse.Clear();
        lastReply = "";


        _ = llmCharacter.Chat(userInput, HandleReply, OnReplyComplete);
    }

    private void HandleReply(string reply)
    {
        if (string.IsNullOrEmpty(reply)) return;

        try
        {
            // Handle incremental response
            if (reply.Length > lastReply.Length && reply.StartsWith(lastReply))
            {
                string newContent = reply.Substring(lastReply.Length);
                currentResponse.Append(newContent);
                lastReply = reply;
            }
            else if (reply.Length > lastReply.Length)
            {
                currentResponse.Append(reply);
                lastReply = reply;
            }

            if (npcDialogueText)
            {
                npcDialogueText.text = currentResponse.ToString();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in HandleReply: {e}");
            OnError();
        }
    }

    private void OnReplyComplete()
    {
        isProcessingResponse = false;
        EnableInput();
    }

    private void OnError()
    {
        isProcessingResponse = false;
        EnableInput();
        Debug.LogError("Error processing LLM response");
    }

    private void EnableInput()
    {
        if (inputBox) inputBox.SetActive(true);
        if (inputField)
        {
            inputField.interactable = true;
            inputField.ActivateInputField();
        }
        if (submitButton) submitButton.interactable = true;
    }

    private void DisableInput()
    {
        if (inputField) inputField.interactable = false;
        if (submitButton) submitButton.interactable = false;
    }

    public async Task ResetDialogue()
    {
        if (llmCharacter == null) return;

        if (isProcessingResponse)
        {
            llmCharacter.CancelRequests();
            isProcessingResponse = false;
        }

  
        if (playerDialogueText) playerDialogueText.text = "";
        if (npcDialogueText) npcDialogueText.text = "";
        if (inputField) inputField.text = "";

        currentResponse.Clear();
        lastReply = "";

        llmCharacter.ClearChat();
        await Task.Yield();

        DisableInput();
    }

    private void OnDisable()
    {
        if (llmCharacter != null && isProcessingResponse)
        {
            llmCharacter.CancelRequests();
        }
        DisableInput();
    }
}