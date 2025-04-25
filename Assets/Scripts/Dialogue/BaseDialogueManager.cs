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
        Debug.Log($"[HandleReply RAW]: '{reply}'"); // Enable to trace exact LLM response

        if (string.IsNullOrEmpty(reply)) return;

        try
        {
            // NEW APPROACH: Handle action accumulation completely differently
            
            // If we already found the action delimiter in an earlier chunk and are now accumulating the action
            if (isAccumulatingAction)
            {
                // IMPROVED: Replace the buffer with the latest chunk instead of appending
                // This keeps only the latest/most complete version of the function call
                bool containsActionMarker = reply.Contains("[/ACTION]:") || reply.Contains("\nACTION:");
                
                // Check if this chunk contains a closing parenthesis - likely the final chunk
                bool containsClosingParen = reply.Contains(")");
                
                if (containsActionMarker)
                {
                    // Extract the relevant parts without the marker
                    string cleaned = CleanFunctionCall(reply);
                    Debug.Log($"[HandleReply] Replacing action buffer with cleaned chunk: '{cleaned}'");
                    actionBuffer.Clear();
                    actionBuffer.Append(cleaned);
                }
                else
                {
                    // No marker found, replace the entire buffer with this chunk
                    Debug.Log($"[HandleReply] Replacing action buffer with raw chunk: '{reply}'");
                    actionBuffer.Clear();
                    actionBuffer.Append(reply);
                }
                
                // If this chunk contains a closing parenthesis, it's likely complete
                if (containsClosingParen)
                {
                    Debug.Log($"[HandleReply] Found closing parenthesis, likely the complete function call");
                }
                
                return; // Skip the rest of processing for this chunk - don't send this to UI
            }
            
            // If an action has already been found and we're not accumulating, completely ignore further text
            if (actionFoundInCurrentStream && !isAccumulatingAction)
            {
                Debug.Log($"[HandleReply] Action already found in stream, ignoring additional text");
                return;
            }

            // CRITICAL FIX: Replace append with clear+append to prevent text duplication
            // Each LLM chunk contains the complete response so far, not just new content
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
                
                // IMPORTANT: Store the function call buffer for later use in OnReplyComplete
                bufferedFunctionCall = functionCallPart;
                
                // Update currentResponse to contain only the dialogue part before the action flag
                // This is important so that later calls that use currentResponse don't include the action flag
                currentResponse.Clear();
                currentResponse.Append(dialoguePart);
                
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
    
    // Optimized helper method to check if text ends with a partial action marker
    // Avoids allocations by checking characters directly instead of EndsWith() string allocations
    private bool EndsWithPartialActionMarker(string text)
    {
        int length = text.Length;
        if (length == 0) return false;
        
        // Check for the ACTION tag pattern
        if (text[length - 1] == '[') return true;
        
        // We need at least 2 characters to check for "[/" and "\n"
        if (length < 2) return false;
        
        // Check for "[/" prefix
        if (text[length - 2] == '[' && text[length - 1] == '/') return true;
        
        // Check for newline
        if (text[length - 1] == '\n') return true;
        
        // More efficient pattern matching for partial "[/ACTION]" sequences
        // Check if the last few characters match the expected pattern
        string actionMarker = "[/ACTION]";
        
        // Check if the end of the string contains a partial segment of "[/ACTION]"
        for (int i = 2; i <= Math.Min(actionMarker.Length, length); i++)
        {
            if (length >= i)
            {
                bool match = true;
                for (int j = 0; j < i; j++)
                {
                    if (text[length - i + j] != actionMarker[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match) return true;
            }
        }
        
        // Check for partial "\nACTION" sequences
        string newlineActionMarker = "\nACTION";
        
        // Check if the end of the string contains a partial segment of "\nACTION"
        for (int i = 2; i <= Math.Min(newlineActionMarker.Length, length); i++)
        {
            if (length >= i)
            {
                bool match = true;
                for (int j = 0; j < i; j++)
                {
                    if (text[length - i + j] != newlineActionMarker[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match) return true;
            }
        }
        
        return false;
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
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("[INPUTDBG] OnReplyComplete called, isProcessingResponse=" + isProcessingResponse);
        #endif
        if (!isProcessingResponse) 
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[INPUTDBG] OnReplyComplete - early return due to !isProcessingResponse");
            #endif
            return;
        }

        // LLM finished sending data. If we've been accumulating an action, finalize it
        if (isAccumulatingAction)
        {
            string finalActionText = actionBuffer.ToString().Trim();
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[INPUTDBG] OnReplyComplete - Finalized accumulated action: '{finalActionText}'");
            #endif
            
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
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[INPUTDBG] OnReplyComplete - Single-chunk action detected: " + bufferedFunctionCall);
            #endif
            // Action was detected during HandleReply.
            // Start coroutine to process it after BeepSpeak finishes and a short delay.
            StartCoroutine(ProcessActionAfterBeepSpeak(bufferedFunctionCall));
            bufferedFunctionCall = null; // Clear buffer
            // Do NOT re-enable input here, ProcessFunctionCall will likely deactivate.
        }
        else
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[INPUTDBG] OnReplyComplete - No action detected, starting EnableInputAfterBeepSpeak");
            #endif
            
            // Only send the complete text to the display if we're not using the new UI
            // This prevents duplicate display when BeepSpeak is already updating the UI
            if (dialogueControl != null && !dialogueControl.UseNewUI)
            {
                // For legacy UI only - send the final complete text to legacy display
                dialogueControl.DisplayNPCDialogue(currentResponse.ToString());
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log("[INPUTDBG] OnReplyComplete - Sent final text to legacy display");
                #endif
            }
            else
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log("[INPUTDBG] OnReplyComplete - Not sending final text to UI (using new UI with BeepSpeak)");
                #endif
            }
            
            // Start coroutine to re-enable input only after BeepSpeak finishes.
            StartCoroutine(EnableInputAfterBeepSpeak());
        }

        // Reset all action state flags
        actionFoundInCurrentStream = false;
        isAccumulatingAction = false;
        actionBuffer.Clear();
        bufferedFunctionCall = null;
        
        isProcessingResponse = false; // Mark processing as complete
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("[INPUTDBG] OnReplyComplete - finished, set isProcessingResponse=false");
        #endif
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
        
        // Retrieve the BeepSpeak instance and its timing configuration
        float textLength = 0f;
        float characterSpeed = 0.07f; // Default fallback speed if we can't access actual settings
        float speedVariance = 0.01f;  // Default fallback variance
        BeepSpeak beepSpeakInstance = null;
        
        if (dialogueControl != null)
        {
            beepSpeakInstance = dialogueControl.GetBeepSpeak();
            if (beepSpeakInstance != null)
            {
                textLength = beepSpeakInstance.GetCurrentTargetLength();
                
                // Access the actual typing speed settings if possible
                if (beepSpeakInstance.npcVoice != null)
                {
                    characterSpeed = beepSpeakInstance.npcVoice.baseSpeed;
                    speedVariance = beepSpeakInstance.npcVoice.speedVariance;
                    Debug.Log($"[BaseDialogueManager] Retrieved BeepSpeak speed: {characterSpeed:F3}s per char, variance: {speedVariance:F3}s");
                }
            }
        }
        
        // Calculate expected typing duration based on actual settings
        // Add extra time for punctuation pauses and general processing overhead
        float estimatedCharTime = characterSpeed + speedVariance; // Worst case (slowest typing speed)
        float punctuationPauseEstimate = Mathf.Min(textLength * 0.2f, 4.0f); // Estimate extra time for punctuation pauses
        float calculatedWaitTime = Mathf.Max(3.0f, (textLength * estimatedCharTime) + punctuationPauseEstimate);
        
        // Allow much longer maximum wait time for long responses
        float maxWaitTime = Mathf.Min(calculatedWaitTime, 20.0f);
        
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
            Debug.Log($"[BaseDialogueManager] BeepSpeak still playing after {maxWaitTime:F1}s, calling ForceCompleteTyping");
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
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("[INPUTDBG] EnableInputAfterBeepSpeak started");
        #endif
        float startTime = Time.realtimeSinceStartup;
        
        // Retrieve the BeepSpeak instance and its timing configuration
        float textLength = 0f;
        float characterSpeed = 0.07f; // Default fallback speed if we can't access actual settings
        float speedVariance = 0.01f;  // Default fallback variance
        BeepSpeak beepSpeakInstance = null;
        
        if (dialogueControl != null)
        {
            beepSpeakInstance = dialogueControl.GetBeepSpeak();
            if (beepSpeakInstance != null)
            {
                textLength = beepSpeakInstance.GetCurrentTargetLength();
                
                // Access the actual typing speed settings if possible
                if (beepSpeakInstance.npcVoice != null)
                {
                    characterSpeed = beepSpeakInstance.npcVoice.baseSpeed;
                    speedVariance = beepSpeakInstance.npcVoice.speedVariance;
                    #if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log($"[INPUTDBG] Retrieved BeepSpeak speed: {characterSpeed:F3}s per char, variance: {speedVariance:F3}s");
                    #endif
                }
            }
        }
        
        // Calculate expected typing duration based on actual settings
        // Add extra time for punctuation pauses and general processing overhead
        float estimatedCharTime = characterSpeed + speedVariance; // Worst case (slowest typing speed)
        float punctuationPauseEstimate = Mathf.Min(textLength * 0.2f, 4.0f); // Estimate extra time for punctuation pauses
        float calculatedWaitTime = Mathf.Max(3.0f, (textLength * estimatedCharTime) + punctuationPauseEstimate);
        
        // Allow much longer maximum wait time for long responses
        float maxWaitTime = Mathf.Min(calculatedWaitTime, 20.0f);
        
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"[INPUTDBG] Waiting up to {maxWaitTime:F1}s for {textLength} characters to type");
        #endif
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
