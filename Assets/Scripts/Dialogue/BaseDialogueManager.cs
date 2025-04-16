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
    protected string bufferedFunctionCall = null; // Buffer for detected function calls
    private bool actionFoundInCurrentStream = false; // Flag to track if action has been found in current stream

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
            #pragma warning disable 0618 // Suppress FindObjectOfType deprecation warning
            dialogueControl = FindObjectOfType<DialogueControl>();
            #pragma warning restore 0618
             if (dialogueControl == null) {
                 Debug.LogWarning("[BaseDialogueManager] Could not find DialogueControl in Start using FindObjectOfType.");
             }
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
        bufferedFunctionCall = null; // Clear buffer on init
        actionFoundInCurrentStream = false; // Reset action flag

        EnableInput();
    }

    protected virtual void HandleReply(string reply)
    {
        // --- DEBUG: Log raw reply chunk ---
        // Debug.Log($"[HandleReply RAW]: '{reply}'"); // Potentially very verbose, uncomment if needed

        if (string.IsNullOrEmpty(reply)) return;

        try
        {
            // If an action has already been found in this stream, completely ignore any further text
            if (actionFoundInCurrentStream)
            {
                Debug.Log($"[HandleReply] Action already found in stream, ignoring additional text: '{reply}'");
                return;
            }

            // CRITICAL FIX: Reset StringBuilder and use the complete response from the LLM
            // Instead of accumulating chunks and creating duplications
            currentResponse.Clear();
            currentResponse.Append(reply);
            
            // Get the accumulated text so far
            string accumulatedText = currentResponse.ToString();
            
            // Check for both the original and new action delimiter formats
            int actionIndex = accumulatedText.IndexOf("[/ACTION]:");
            if (actionIndex == -1)
            {
                // Also check for the original format as a fallback
                actionIndex = accumulatedText.IndexOf("\nACTION:");
            }

            if (actionIndex != -1)
            {
                // Action delimiter found!
                actionFoundInCurrentStream = true; // Set the flag for this stream
                
                // Extract the dialogue part (text before the delimiter)
                string dialoguePart = accumulatedText.Substring(0, actionIndex).Trim();
                
                // Determine which delimiter was found to calculate the correct offset
                int delimiterLength;
                if (accumulatedText.IndexOf("[/ACTION]:") == actionIndex)
                {
                    delimiterLength = "[/ACTION]:".Length;
                }
                else
                {
                    delimiterLength = "\nACTION:".Length;
                }
                
                // Extract the function call part (text after the delimiter)
                string functionCallPart = accumulatedText.Substring(actionIndex + delimiterLength).Trim();
                
                Debug.Log($"[HandleReply] Action detected. Delimiter index: {actionIndex}");
                Debug.Log($"[HandleReply] Split Dialogue: '{dialoguePart}'");
                Debug.Log($"[HandleReply] Split Function Call: '{functionCallPart}'");
                
                // Buffer the function call for processing after stream completes
                bufferedFunctionCall = functionCallPart;
                
                // Update the display with ONLY the dialogue part
                UpdateDialogueDisplay(dialoguePart);
                
                // CRITICAL: Clear the currentResponse - this dialogue turn is effectively finished
                // We no longer need to accumulate text since we've found the action
                currentResponse.Clear();
                lastReply = ""; // Reset this as well to avoid any carry-over issues
            }
            else
            {
                // No action delimiter found yet, update the display with the accumulated text
                UpdateDialogueDisplay(accumulatedText);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in HandleReply: {e}");
            OnError();
        }
    }
    protected virtual void ProcessFunctionCall(string functionCall)
    {
        if (string.IsNullOrEmpty(functionCall)) return; // Don't process empty calls

        Debug.Log($"[ProcessFunctionCall] Attempting to process: '{functionCall}'");

        // Use case-insensitive comparison for robustness
        if (functionCall.StartsWith("stop_conversation", System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log("[ProcessFunctionCall] Detected 'stop_conversation'");
            // Try to use our field first, otherwise find it in the scene
            if (dialogueControl != null)
            {
                Debug.Log("Character requested to end conversation");
                dialogueControl.Deactivate();
            }
            else
            {
                // Fall back to FindObjectOfType if dialogueControl is not set
                #pragma warning disable 0618 // Suppress FindObjectOfType deprecation warning
                 var foundDialogueControl = FindObjectOfType<DialogueControl>();
                #pragma warning restore 0618
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
        // Use case-insensitive comparison for robustness
        else if (functionCall.StartsWith("reveal_node", System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log("[ProcessFunctionCall] Detected 'reveal_node'");
            // Extract the node ID parameter
            string nodeId = ExtractNodeId(functionCall);
            Debug.Log($"[ProcessFunctionCall] Extracted Node ID: '{nodeId ?? "NULL"}'");
            if (!string.IsNullOrEmpty(nodeId))
            {
                // Call the DiscoverNode method on the GameControl's constellation
                if (GameControl.GameController?.coreConstellation != null)
                {
                    Debug.Log($"[ProcessFunctionCall] Calling DiscoverNode for '{nodeId}'...");
                    var node = GameControl.GameController.coreConstellation.DiscoverNode(nodeId);
                    if (node != null)
                    {
                        Debug.Log($"[ProcessFunctionCall] Successfully revealed node: {nodeId}");
                    }
                    else
                    {
                        // DiscoverNode already logs failure/already discovered
                        Debug.LogWarning($"[ProcessFunctionCall] DiscoverNode returned null for: {nodeId} (Node might already be discovered or key invalid)");
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
        Debug.Log("[INPUTDBG] OnReplyComplete called, isProcessingResponse=" + isProcessingResponse);
        if (!isProcessingResponse) 
        {
            Debug.Log("[INPUTDBG] OnReplyComplete - early return due to !isProcessingResponse");
            return;
        }

        // LLM finished sending data. Decide what to do based on whether an action was buffered.
        if (!string.IsNullOrEmpty(bufferedFunctionCall))
        {
            Debug.Log("[INPUTDBG] OnReplyComplete - Action detected: " + bufferedFunctionCall);
            // Action was detected during HandleReply.
            // Start coroutine to process it after BeepSpeak finishes and a short delay.
            StartCoroutine(ProcessActionAfterBeepSpeak(bufferedFunctionCall));
            bufferedFunctionCall = null; // Clear buffer
            // Do NOT re-enable input here, ProcessFunctionCall will likely deactivate.
        }
        else
        {
            Debug.Log("[INPUTDBG] OnReplyComplete - No action detected, starting EnableInputAfterBeepSpeak");
            // No action detected, normal end of reply.
            // Start coroutine to re-enable input only after BeepSpeak finishes.
            StartCoroutine(EnableInputAfterBeepSpeak());
        }

        isProcessingResponse = false; // Mark processing as complete
        Debug.Log("[INPUTDBG] OnReplyComplete - finished, set isProcessingResponse=false");
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
        Debug.Log("[BaseDialogueManager.ResetDialogue] Called.");
        if (llmCharacter == null) 
        {
             Debug.LogWarning("[BaseDialogueManager.ResetDialogue] llmCharacter is null, returning.");
             return;
        }

        if (isProcessingResponse)
        {
            Debug.Log("[BaseDialogueManager.ResetDialogue] Processing response, attempting CancelRequests...");
            llmCharacter.CancelRequests();
            Debug.Log("[BaseDialogueManager.ResetDialogue] CancelRequests called.");
            isProcessingResponse = false;
        }
        else
        {
             Debug.Log("[BaseDialogueManager.ResetDialogue] Not processing response, attempting CancelRequests anyway...");
             llmCharacter.CancelRequests(); // Call even if not processing, just in case
             Debug.Log("[BaseDialogueManager.ResetDialogue] CancelRequests called (precautionary).");
        }

        currentResponse.Clear();
        lastReply = "";
        bufferedFunctionCall = null; // Clear buffer on reset too
        actionFoundInCurrentStream = false; // Reset action flag

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

    /// <summary>
    /// Waits for BeepSpeak to finish playing, then processes the buffered function call after a short delay.
    /// </summary>
    private System.Collections.IEnumerator ProcessActionAfterBeepSpeak(string action)
    {
        // Wait for BeepSpeak to finish
        // Add a timeout safeguard? Maybe later if needed.
        while (dialogueControl != null && dialogueControl.IsBeepSpeakPlaying)
        {
            yield return null;
        }

        // Add a short delay for readability before the action happens
        yield return new WaitForSeconds(0.5f); // Configurable?

        ProcessFunctionCall(action);
    }

    /// <summary>
    /// Waits for BeepSpeak to finish playing, then re-enables input if dialogue is still active.
    /// </summary>
    private System.Collections.IEnumerator EnableInputAfterBeepSpeak()
    {
        Debug.Log("[INPUTDBG] EnableInputAfterBeepSpeak started");
        float startTime = Time.realtimeSinceStartup;
        int loopCount = 0;
        
        // Wait for BeepSpeak to finish
        while (dialogueControl != null && dialogueControl.IsBeepSpeakPlaying)
        {
            loopCount++;
            if (loopCount % 30 == 0) // Log every ~0.5 seconds (assuming 60fps)
            {
                Debug.Log($"[INPUTDBG] Still waiting for BeepSpeak to finish playing. Elapsed: {Time.realtimeSinceStartup - startTime:F1}s");
                if (dialogueControl != null)
                {
                    Debug.Log($"[INPUTDBG] dialogueControl.IsBeepSpeakPlaying = {dialogueControl.IsBeepSpeakPlaying}");
                    if (dialogueControl.IsBeepSpeakPlaying && dialogueControl.GetBeepSpeak() != null)
                    {
                        Debug.Log($"[INPUTDBG] BeepSpeak.typingCoroutine != null: {dialogueControl.GetBeepSpeak().GetTypingCoroutineActive()}");
                    }
                }
            }
            yield return null;
        }

        Debug.Log($"[INPUTDBG] BeepSpeak finished after {Time.realtimeSinceStartup - startTime:F1}s. Ready to enable input.");

        // Only enable if dialogue is still active (ProcessFunctionCall might have deactivated)
        if (dialogueControl == null)
        {
            Debug.Log("[INPUTDBG] dialogueControl is null, cannot re-enable input");
        }
        else if (dialogueControl.IsDialogueCanvasActive)
        {
            Debug.Log("[INPUTDBG] Dialogue still active, calling EnableInput()");
            EnableInput();
        }
        else
        {
            Debug.Log("[INPUTDBG] Dialogue no longer active, not calling EnableInput()");
        }
    }
}
