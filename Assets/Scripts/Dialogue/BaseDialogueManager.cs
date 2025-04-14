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
    protected DialogueControl dialogueControl;

    /// <summary>
    /// Provides access to the current LLMCharacter being used for dialogue
    /// Used by DialogueControl for saving conversation state
    /// </summary>
    public LLMCharacter CurrentCharacter => llmCharacter;

    protected virtual void Start()
    {
        currentResponse = new StringBuilder();
        SetupInputHandlers();
        
        // Try to find DialogueControl if not already set
        if (dialogueControl == null)
        {
            dialogueControl = FindFirstObjectByType<DialogueControl>();
        }
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
            // Process the reply to check for function calls
            string processedReply = reply;
            int actionIndex = reply.IndexOf("\nACTION: ");
            
            if (actionIndex != -1)
            {
                // Split the reply into dialogue and function call
                processedReply = reply.Substring(0, actionIndex).Trim();
                string functionCall = reply.Substring(actionIndex + 9).Trim(); // +9 to skip "\nACTION: "
                
                // Process the function call
                ProcessFunctionCall(functionCall);
            }
            
            // Continue with normal reply handling using only the dialogue part
            if (processedReply.Length > lastReply.Length && processedReply.StartsWith(lastReply))
            {
                string newContent = processedReply.Substring(lastReply.Length);
                currentResponse.Append(newContent);
                lastReply = processedReply;
            }
            else if (processedReply.Length > lastReply.Length)
            {
                currentResponse.Clear();
                currentResponse.Append(processedReply);
                lastReply = processedReply;
            }

            UpdateDialogueDisplay(currentResponse.ToString());
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in HandleReply: {e}");
            OnError();
        }
    }
    
    protected virtual void ProcessFunctionCall(string functionCall)
    {
        Debug.Log($"Processing function call: {functionCall}");
        
        if (functionCall.StartsWith("stop_conversation"))
        {
            // Try to use our field first, otherwise find it in the scene
            if (dialogueControl != null)
            {
                Debug.Log("Character requested to end conversation");
                dialogueControl.Deactivate();
            }
            else
            {
                // Fall back to FindFirstObjectByType if dialogueControl is not set
                var foundDialogueControl = FindFirstObjectByType<DialogueControl>();
                if (foundDialogueControl != null)
                {
                    Debug.Log("Character requested to end conversation (using FindObjectOfType)");
                    foundDialogueControl.Deactivate();
                }
                else
                {
                    Debug.LogError("Character requested to end conversation, but DialogueControl not found");
                }
            }
        }
        else if (functionCall.StartsWith("reveal_node"))
        {
            // Extract the node ID parameter
            string nodeId = ExtractNodeId(functionCall);
            if (!string.IsNullOrEmpty(nodeId))
            {
                // Call the DiscoverNode method on the GameControl's constellation
                if (GameControl.GameController?.coreConstellation != null)
                {
                    var node = GameControl.GameController.coreConstellation.DiscoverNode(nodeId);
                    if (node != null)
                    {
                        Debug.Log($"Successfully revealed node: {nodeId}");
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to reveal node: {nodeId}");
                    }
                }
                else
                {
                    Debug.LogError("Cannot reveal node: GameControl.coreConstellation is null");
                }
            }
            else
            {
                Debug.LogError($"Failed to extract node ID from function call: {functionCall}");
            }
        }
    }
    
    private string ExtractNodeId(string functionCall)
    {
        // Parse function call like "reveal_node(node_id=testimony-two-men)" to extract "testimony-two-men"
        int startIndex = functionCall.IndexOf("node_id=");
        if (startIndex == -1) return null;
        
        startIndex += 8; // Length of "node_id="
        
        int endIndex = functionCall.IndexOf(")", startIndex);
        if (endIndex == -1) endIndex = functionCall.Length;
        
        return functionCall.Substring(startIndex, endIndex - startIndex);
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