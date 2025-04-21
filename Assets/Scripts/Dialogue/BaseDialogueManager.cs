using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using LLMUnity;
using System; // Add missing System namespace for StringSplitOptions

public abstract class BaseDialogueManager : MonoBehaviour
{
    protected LLMCharacter llmCharacter;
    protected bool isProcessingResponse = false;
    protected StringBuilder currentResponse;
    protected string lastReply = "";
    protected DialogueControl dialogueControl;
    protected string bufferedFunctionCall = null; // Buffer for detected function calls
    private bool actionFoundInCurrentStream = false; // Flag to track if action has been found in current stream
    private bool isAccumulatingAction = false; // Flag to track if we're accumulating an action across chunks
    private StringBuilder actionBuffer = new StringBuilder(); // Buffer to accumulate action text across chunks

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
        isAccumulatingAction = false; // Reset accumulation flag
        actionBuffer.Clear(); // Clear action buffer
        
        EnableInput();
    }

    protected virtual void HandleReply(string reply)
    {
        // --- DEBUG: Log raw reply chunk ---
        // Debug.Log($"[HandleReply RAW]: '{reply}'"); // Potentially very verbose, uncomment if needed

        if (string.IsNullOrEmpty(reply)) return;

        try
        {
            // NEW APPROACH: Handle action accumulation completely differently
            
            // If we already found the action delimiter in an earlier chunk and are now accumulating the action
            if (isAccumulatingAction)
            {
                // Check if this chunk contains a duplicated action marker
                bool containsActionMarker = reply.Contains("[/ACTION]:") || reply.Contains("\nACTION:");
                
                if (containsActionMarker)
                {
                    // Extract the relevant parts without the marker
                    string cleaned = CleanFunctionCall(reply);
                    Debug.Log($"[HandleReply] Accumulating action (cleaned chunk): '{cleaned}'");
                    actionBuffer.Append(cleaned);
                }
                else
                {
                    // No marker found, just append the entire chunk
                    Debug.Log($"[HandleReply] Accumulating action (raw chunk): '{reply}'");
                    actionBuffer.Append(reply);
                }
                
                return; // Skip the rest of processing for this chunk
            }
            
            // If an action has already been found and we're not accumulating, completely ignore further text
            if (actionFoundInCurrentStream && !isAccumulatingAction)
            {
                Debug.Log($"[HandleReply] Action already found in stream, ignoring additional text");
                return;
            }

            // For normal text without action markers, clear and update the display
            currentResponse.Clear();
            currentResponse.Append(reply);
            
            // Get the accumulated text so far
            string accumulatedText = currentResponse.ToString();
            
            // Check for partial action markers at the end of the text
            if (EndsWithPartialActionMarker(accumulatedText))
            {
                int markerStartIndex = GetPartialMarkerStartIndex(accumulatedText);
                if (markerStartIndex > 0)
                {
                    // Display only the clean part without the partial marker
                    string cleanText = accumulatedText.Substring(0, markerStartIndex);
                    Debug.Log($"[HandleReply] Found partial action marker, displaying only: '{cleanText}'");
                    UpdateDialogueDisplay(cleanText);
                    return;
                }
            }
            
            // Check for full action markers in the text
            int actionIndex = accumulatedText.IndexOf("[/ACTION]:");
            if (actionIndex == -1)
            {
                actionIndex = accumulatedText.IndexOf("\nACTION:");
            }

            if (actionIndex != -1)
            {
                // Action delimiter found!
                actionFoundInCurrentStream = true;
                isAccumulatingAction = true;
                
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
                
                // Extract the initial function call part and start accumulating
                string functionCallPart = accumulatedText.Substring(actionIndex + delimiterLength).Trim();
                actionBuffer.Clear();
                actionBuffer.Append(functionCallPart);
                
                Debug.Log($"[HandleReply] Action detected at index: {actionIndex}");
                Debug.Log($"[HandleReply] Dialogue part: '{dialoguePart}'");
                Debug.Log($"[HandleReply] Initial function part: '{functionCallPart}'");
                
                // Only update display with the dialogue part
                UpdateDialogueDisplay(dialoguePart);
            }
            else
            {
                // No action delimiter, just display the normal text
                UpdateDialogueDisplay(accumulatedText);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in HandleReply: {e}");
            OnError();
        }
    }
    
    // Helper method to check if text ends with a partial action marker
    private bool EndsWithPartialActionMarker(string text)
    {
        return text.EndsWith("[") || 
               text.EndsWith("[/") || 
               text.EndsWith("[/A") || 
               text.EndsWith("[/AC") || 
               text.EndsWith("[/ACT") || 
               text.EndsWith("[/ACTI") || 
               text.EndsWith("[/ACTIO") || 
               text.EndsWith("[/ACTION") || 
               text.EndsWith("[/ACTION]") ||
               text.EndsWith("\n") || // Possible start of "\nACTION:"
               text.EndsWith("\nA") || 
               text.EndsWith("\nAC") || 
               text.EndsWith("\nACT") || 
               text.EndsWith("\nACTI") || 
               text.EndsWith("\nACTIO") || 
               text.EndsWith("\nACTION");
    }
    
    // Helper method to get the starting index of a partial marker
    private int GetPartialMarkerStartIndex(string text)
    {
        if (text.EndsWith("[")) return text.LastIndexOf('[');
        if (text.EndsWith("[/")) return text.LastIndexOf('[');
        if (text.EndsWith("[/A")) return text.LastIndexOf('[');
        if (text.EndsWith("[/AC")) return text.LastIndexOf('[');
        if (text.EndsWith("[/ACT")) return text.LastIndexOf('[');
        if (text.EndsWith("[/ACTI")) return text.LastIndexOf('[');
        if (text.EndsWith("[/ACTIO")) return text.LastIndexOf('[');
        if (text.EndsWith("[/ACTION")) return text.LastIndexOf('[');
        if (text.EndsWith("[/ACTION]")) return text.LastIndexOf('[');
        
        if (text.EndsWith("\n")) return text.LastIndexOf('\n');
        if (text.EndsWith("\nA")) return text.LastIndexOf('\n');
        if (text.EndsWith("\nAC")) return text.LastIndexOf('\n');
        if (text.EndsWith("\nACT")) return text.LastIndexOf('\n');
        if (text.EndsWith("\nACTI")) return text.LastIndexOf('\n');
        if (text.EndsWith("\nACTIO")) return text.LastIndexOf('\n');
        if (text.EndsWith("\nACTION")) return text.LastIndexOf('\n');
        
        return -1; // No marker found
    }

    // Clean up function call string by removing duplicated action markers and dialogue text
    private string CleanFunctionCall(string functionCall)
    {
        // First try to get just the last occurrence of a complete function call pattern
        int lastRevealIndex = functionCall.LastIndexOf("reveal_node(");
        int lastStopIndex = functionCall.LastIndexOf("stop_conversation(");
        
        // If either pattern is found, use the later one
        if (lastRevealIndex >= 0 || lastStopIndex >= 0)
        {
            int startIndex = Math.Max(lastRevealIndex, lastStopIndex);
            if (startIndex >= 0)
            {
                // Just return the function call part
                return functionCall.Substring(startIndex);
            }
        }
        
        // Fallback: If text contains multiple [/ACTION]: prefixes, extract only the last part
        if (functionCall.Contains("[/ACTION]:"))
        {
            string[] parts = functionCall.Split(new[] { "[/ACTION]:" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                // Use the last part which should contain the actual function call
                return parts[parts.Length - 1].Trim();
            }
        }
        
        // Similarly for \nACTION:
        if (functionCall.Contains("\nACTION:"))
        {
            string[] parts = functionCall.Split(new[] { "\nACTION:" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                return parts[parts.Length - 1].Trim();
            }
        }
        
        return functionCall;
    }

    protected virtual void ProcessFunctionCall(string functionCall)
    {
        if (string.IsNullOrEmpty(functionCall)) return; // Don't process empty calls

        Debug.Log($"[ProcessFunctionCall] Processing: '{functionCall}'");
        
        // Clean up the function call - remove any duplicated prefixes
        string cleanedFunctionCall = CleanFunctionCall(functionCall);
        Debug.Log($"[ProcessFunctionCall] Cleaned function call: '{cleanedFunctionCall}'");

        // Use case-insensitive comparison for robustness
        if (cleanedFunctionCall.StartsWith("stop_conversation", System.StringComparison.OrdinalIgnoreCase))
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
        else if (cleanedFunctionCall.StartsWith("reveal_node", System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log("[ProcessFunctionCall] Detected 'reveal_node'");
            // Extract the node ID parameter
            string nodeId = ExtractNodeId(cleanedFunctionCall);
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
                
                // CRITICAL: For reveal_node, ALWAYS re-enable input since the conversation should continue
                // Unlike stop_conversation, this function does not end the dialogue
                StartCoroutine(ReenableInputAfterRevealNode());
            }
            else
            {
                Debug.LogError($"Failed to extract node ID from function call: {cleanedFunctionCall}");
                // Still try to re-enable input even if node ID extraction failed
                StartCoroutine(ReenableInputAfterRevealNode());
            }
        }
    }
    
    /// <summary>
    /// Helper coroutine to re-enable input after a reveal_node function call
    /// Unlike stop_conversation, reveal_node should continue the conversation
    /// </summary>
    private System.Collections.IEnumerator ReenableInputAfterRevealNode()
    {
        // Brief pause to let any UI updates complete
        yield return new WaitForSeconds(0.2f);
        
        Debug.Log("[ProcessFunctionCall] Re-enabling input after reveal_node");
        
        // Only re-enable input if dialogue is still active
        if (dialogueControl != null && dialogueControl.IsDialogueCanvasActive)
        {
            EnableInput();
            Debug.Log("[ProcessFunctionCall] Input re-enabled after reveal_node");
        }
        else
        {
            Debug.Log("[ProcessFunctionCall] Dialogue no longer active, not re-enabling input");
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

        // LLM finished sending data. If we've been accumulating an action, finalize it
        if (isAccumulatingAction)
        {
            string finalActionText = actionBuffer.ToString().Trim();
            Debug.Log($"[INPUTDBG] OnReplyComplete - Finalized accumulated action: '{finalActionText}'");
            
            // Start coroutine to process the accumulated action after BeepSpeak finishes
            StartCoroutine(ProcessActionAfterBeepSpeak(finalActionText));
            
            // Reset action accumulation state
            actionBuffer.Clear();
            isAccumulatingAction = false;
            bufferedFunctionCall = null;
        }
        // Otherwise check for a single-chunk function call (backward compatibility)
        else if (!string.IsNullOrEmpty(bufferedFunctionCall))
        {
            Debug.Log("[INPUTDBG] OnReplyComplete - Single-chunk action detected: " + bufferedFunctionCall);
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

        // Reset all action state flags
        actionFoundInCurrentStream = false;
        isAccumulatingAction = false;
        actionBuffer.Clear();
        bufferedFunctionCall = null;
        
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
        
        // BUGFIX: Stop BeepSpeak typing first to ensure it properly resets its state
        if (dialogueControl != null && dialogueControl.GetBeepSpeak() != null)
        {
            Debug.Log("[BaseDialogueManager.ResetDialogue] Stopping BeepSpeak typing...");
            dialogueControl.GetBeepSpeak().StopTyping();
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
    /// Waits for BeepSpeak to finish playing, then processes the buffered function call.
    /// If BeepSpeak takes too long, uses ForceCompleteTyping instead of waiting indefinitely.
    /// </summary>
    private System.Collections.IEnumerator ProcessActionAfterBeepSpeak(string action)
    {
        float startTime = Time.realtimeSinceStartup;
        
        // Calculate an appropriate wait time based on text length in BeepSpeak
        float textLength = 0f;
        if (dialogueControl != null && dialogueControl.GetBeepSpeak() != null)
        {
            textLength = dialogueControl.GetBeepSpeak().GetCurrentTargetLength();
        }
        
        // Calculate wait time based on text length - at least 2 seconds, and 0.07 seconds per character
        // (roughly 14 characters per second, which is a reasonable reading speed)
        float calculatedWaitTime = Mathf.Max(2.0f, textLength * 0.07f);
        
        // Limit max wait time to 5 seconds to avoid excessively long waits
        float maxWaitTime = Mathf.Max(calculatedWaitTime, 5.0f);
        
        Debug.Log($"[BaseDialogueManager] Waiting up to {maxWaitTime:F1}s for BeepSpeak to finish before processing action");
        
        // Wait for BeepSpeak to finish naturally, but only up to our calculated maxWaitTime
        while (dialogueControl != null && dialogueControl.IsBeepSpeakPlaying && 
               Time.realtimeSinceStartup - startTime < maxWaitTime)
        {
            yield return null;
        }
        
        // If it's still playing after maxWaitTime, force it to complete
        if (dialogueControl != null && dialogueControl.IsBeepSpeakPlaying)
        {
            Debug.Log($"[BaseDialogueManager] BeepSpeak still playing after {maxWaitTime:F1}s, calling ForceCompleteTyping from Process");
            dialogueControl.GetBeepSpeak()?.ForceCompleteTyping();
            
            // Short yield to let the transition effect complete
            yield return new WaitForSeconds(0.2f);
        }
        
        // Process the function immediately
        ProcessFunctionCall(action);
    }

    /// <summary>
    /// Waits for BeepSpeak to finish playing, then re-enables input if dialogue is still active.
    /// Gives BeepSpeak adequate time to complete naturally before forcing completion.
    /// </summary>
    private System.Collections.IEnumerator EnableInputAfterBeepSpeak()
    {
        Debug.Log("[INPUTDBG] EnableInputAfterBeepSpeak started");
        float startTime = Time.realtimeSinceStartup;
        
        // Increase maximum wait time to allow proper typing animation to complete
        // Average English reading speed is ~200-250 words/min (3-4 words/sec)
        // Assuming ~5 chars/word, that's 15-20 chars/sec, so wait time should scale with text length
        float textLength = 0f;
        if (dialogueControl != null && dialogueControl.GetBeepSpeak() != null)
        {
            textLength = dialogueControl.GetBeepSpeak().GetCurrentTargetLength();
        }
        
        // Calculate a reasonable wait time based on text length
        // Use at least 2 seconds, and add 0.07 seconds per character (roughly 14 chars/sec)
        float calculatedWaitTime = Mathf.Max(2.0f, textLength * 0.07f);
        
        // Cap the maximum wait time to avoid excessive delays for very long text
        float maxWaitTime = Mathf.Max(calculatedWaitTime, 5.0f);
        
        Debug.Log($"[INPUTDBG] Waiting up to {maxWaitTime:F1}s for {textLength} characters to type");
        int loopCount = 0;
        float lastProgressTime = Time.realtimeSinceStartup;
        
        // Wait for BeepSpeak to finish naturally, being more patient with longer text
        while (dialogueControl != null && dialogueControl.IsBeepSpeakPlaying && 
               Time.realtimeSinceStartup - startTime < maxWaitTime)
        {
            loopCount++;
            
            // Log status periodically, not every frame
            if (loopCount % 60 == 0) // Log approximately every second at 60fps
            {
                Debug.Log($"[INPUTDBG] Still waiting for BeepSpeak to finish playing. Elapsed: {Time.realtimeSinceStartup - startTime:F1}s of {maxWaitTime:F1}s max wait");
                if (dialogueControl != null && dialogueControl.GetBeepSpeak() != null)
                {
                    Debug.Log($"[INPUTDBG] BeepSpeak.typingCoroutine != null: {dialogueControl.GetBeepSpeak().GetTypingCoroutineActive()}");
                }
            }
            
            yield return null;
        }
        
        // If it's still playing after maxWaitTime, force it to complete
        if (dialogueControl != null && dialogueControl.IsBeepSpeakPlaying)
        {
            Debug.Log($"[INPUTDBG] BeepSpeak still playing after {maxWaitTime:F1}s, calling ForceCompleteTyping");
            dialogueControl.GetBeepSpeak()?.ForceCompleteTyping();
            
            // Short yield to let any other processing finish
            yield return new WaitForSeconds(0.1f);
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
