using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using LLMUnity;

public abstract class BaseDialogueManager : MonoBehaviour
{
    protected LLMCharacter llmCharacter;
    protected bool isProcessingResponse = false;
    protected StringBuilder currentResponse;
    protected string lastReply = "";

    /// <summary>
    /// Provides access to the current LLMCharacter being used for dialogue
    /// Used by DialogueControl for saving conversation state
    /// </summary>
    public LLMCharacter CurrentCharacter => llmCharacter;

    protected virtual void Start()
    {
        currentResponse = new StringBuilder();
        SetupInputHandlers();
    }

    protected abstract void SetupInputHandlers();

    public virtual void SetCharacter(LLMCharacter character)
    {
        llmCharacter = character;
        //Debug.Log($"Set LLMCharacter: {character.gameObject.name}");
    }

    public virtual void InitializeDialogue()
    {
        if (isProcessingResponse)
        {
            llmCharacter?.CancelRequests();
            isProcessingResponse = false;
        }

        currentResponse.Clear();
        lastReply = "";

        EnableInput();
    }

    protected virtual void HandleReply(string reply)
    {
        if (string.IsNullOrEmpty(reply)) return;

        try
        {
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

            //UpdateDialogueDisplay(currentResponse.ToString());
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in HandleReply: {e}");
            OnError();
        }
    }

    protected virtual void OnReplyComplete()
    {
        if (!isProcessingResponse) return;
        isProcessingResponse = false;
        EnableInput();
        UpdateDialogueDisplay(currentResponse.ToString());
    }

    protected virtual void OnError()
    {
        isProcessingResponse = false;
        EnableInput();
        Debug.LogError("Error processing LLM response");
    }

    protected abstract void EnableInput();
    protected abstract void DisableInput();
    protected abstract void UpdateDialogueDisplay(string text);

    public virtual async Task ResetDialogue()
    {
        if (llmCharacter == null) return;

        if (isProcessingResponse)
        {
            llmCharacter.CancelRequests();
            isProcessingResponse = false;
        }

        currentResponse.Clear();
        lastReply = "";

        // llmCharacter.ClearChat(); // Removed: History should persist until explicitly cleared elsewhere or loaded.
        await Task.Yield();

        DisableInput();
    }

    protected virtual void OnDisable()
    {
        if (llmCharacter != null && isProcessingResponse)
        {
            llmCharacter.CancelRequests();
        }
        DisableInput();
    }
}