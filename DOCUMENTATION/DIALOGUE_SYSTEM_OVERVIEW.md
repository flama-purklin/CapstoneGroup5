# Dialogue System Overview

This document outlines the key components, flow, and mechanisms involved in the LLM-powered dialogue system.

> ✅ **CURRENT STATUS: FULLY FUNCTIONAL (May 2025)** ✅  
> The dialogue system has been fully refactored to fix all known issues with text streaming, function calling, and UI integration.

## 1. Key Components

### Scripts

*   **`DialogueControl.cs` (`Assets/Scripts/NPCs/`)**:
    *   Attached to the main Dialogue UI Canvas/GameObject.
    *   Manages the activation and deactivation of the dialogue UI (including animations).
    *   Handles game state transitions (`GameState.DIALOGUE` <-> `GameState.DEFAULT`).
    *   Acts as the entry point when player interacts with an NPC.
    *   Calls `LLMCharacter.Load()` on activation (if save exists) and `LLMCharacter.Save()` on deactivation.
    *   Calls `LLMDialogueManager.InitializeDialogue()` and `ResetDialogue()`.
    *   Provides methods (`DisplayNPCDialogue`, `DisplayNPCDialogueStreaming`) for updating the UI text, integrating with `BeepSpeak.cs`.
    *   **IMPROVED:** Now selectively hides non-critical HUD elements while preserving `NodeUnlockNotif` and `PowerControl` elements during dialogue.
    *   Handles Escape key input to deactivate dialogue.

*   **`BaseDialogueManager.cs` (`Assets/Scripts/Dialogue/`)**:
    *   Abstract base class providing core dialogue logic.
    *   Holds reference to the current `LLMCharacter`.
    *   Manages response processing state (`isProcessingResponse`).
    *   Receives LLM responses via the `HandleReply` callback.
    *   **FIXED:** In `HandleReply`, uses `currentResponse.Clear(); currentResponse.Append(reply);` to correctly handle streaming text chunks from the LLM (fixed text duplication issue).
    *   **IMPROVED:** Function call handling via `actionBuffer.Clear(); actionBuffer.Append(cleaned);` to prevent redundant text accumulation (eliminates "revealreveal_node..." issues).
    *   **Parses Function Calls:** Checks for `\nACTION:` or `[/ACTION]:` delimiter in `HandleReply`. If found, sets the `actionFoundInCurrentStream` flag, splits dialogue from action string, and buffers the function call for later processing.
    *   **Handles Function Calls (`ProcessFunctionCall`):**
        *   `stop_conversation`: Calls `DialogueControl.Deactivate()`.
        *   `reveal_node`: Extracts `node_id` parameter using `ExtractNodeId` and calls `GameControl.coreConstellation.DiscoverNode(nodeId)`.
    *   **IMPROVED:** Dynamic timeout calculation in `ProcessActionAfterBeepSpeak` and `EnableInputAfterBeepSpeak` coroutines based on text length and BeepSpeak typing speed settings, with a maximum wait time of 20 seconds (up from 5).

*   **`LLMDialogueManager.cs` (`Assets/Scripts/Dialogue/`)**:
    *   Concrete implementation inheriting from `BaseDialogueManager`.
    *   Likely attached to the same GameObject as `DialogueControl` or a child object within the Dialogue Canvas prefab.
    *   Holds references to specific UI elements (`TMP_InputField`, `TMP_Text` for player/NPC dialogue, `Button`).
    *   Handles player input submission (`OnSubmitClicked`) and sends it to `LLMCharacter.Chat()`.
    *   Implements `UpdateDialogueDisplay` to forward text to `DialogueControl` (for BeepSpeak/UI).
    *   Implements `EnableInput`/`DisableInput` to manage UI interactability.
    *   Registers itself with `DialogueControl`.

*   **`DialogueUIController.cs` (`Assets/Scripts/UI/Dialogue/`)**:
    *   Manages the new UI layout for dialogue.
    *   Provides methods to show/hide the dialogue UI.
    *   Handles player input submission and passes it to `DialogueControl`.
    *   Exposes the response text component for BeepSpeak integration.

*   **`BeepSpeak.cs` (`Assets/DialogueSFX/`)**:
    *   Controls the typing animation and audio feedback.
    *   Exposes `IsPlaying` property to indicate when typing animation is active.
    *   **IMPROVED:** Properly handles streaming text updates without duplicating or skipping content.
    *   **FIXED:** Removed redundant 8-second timeout that caused premature animation termination.
    *   Provides `ForceCompleteTyping()` method for emergency animation completion.
    *   Properly filters function call text to prevent it from appearing in the dialogue box.

## 2. Dialogue Flow

1.  **Activation**: Player interacts with an NPC (e.g., presses 'E'). The NPC's interaction logic calls `DialogueControl.Activate(npcGameObject)`.
2.  **Character Setup**: `DialogueControl` gets the `Character` component, retrieves the associated `LLMCharacter` instance, and calls `llmDialogueManager.SetCharacter()`.
3.  **Load History**: `DialogueControl` checks if a save file exists for the character and calls `await llmCharacter.Load()` if it does.
4.  **UI Activation**: `DialogueControl` activates the Dialogue Canvas and plays an activation animation. `GameControl` state is set to `DIALOGUE`.
5.  **HUD Management**: `DialogueControl` now selectively deactivates non-critical HUD elements while keeping `NodeUnlockNotif` and `PowerControl` visible.
6.  **Initialization**: Once the animation is complete, `DialogueControl` calls `llmDialogueManager.InitializeDialogue()`, which enables the input field.
7.  **Player Input**: Player types in the `InputField` and clicks `Submit` or presses Enter.
8.  **Send to LLM**: `LLMDialogueManager.OnSubmitClicked` disables input, clears previous response data, and calls `llmCharacter.Chat(userInput, HandleReply, OnReplyComplete)`.
9.  **LLM Response (Streaming)**: `LLMCharacter` sends the request to the LLM. As response chunks arrive, the `HandleReply` callback in `BaseDialogueManager` is invoked.
10. **Response Parsing (`HandleReply`)**:
    *   **FIXED:** Uses `currentResponse.Clear(); currentResponse.Append(reply);` to prevent text duplication.
    *   Checks for delimiter (`\nACTION:` or `[/ACTION]:`).
    *   If found, sets the `actionFoundInCurrentStream` flag, splits the text into dialogue (text before delimiter) and action (text after delimiter).
    *   **IMPROVED:** When accumulating action text, uses `actionBuffer.Clear(); actionBuffer.Append(cleaned);` to prevent redundant text buildup.
    *   Updates the display with ONLY the dialogue part via `UpdateDialogueDisplay(dialoguePart)`.
    *   If no delimiter found, updates the display with the complete accumulated text.
11. **UI Update (`UpdateDialogueDisplay`)**: `LLMDialogueManager` forwards the accumulating dialogue text to `DialogueControl`, which updates the NPC text UI via BeepSpeak.
12. **Stream Complete (`OnReplyComplete`)**: Called by `LLMCharacter` when the full response is received. Checks if an action was buffered:
    *   If buffered, starts `ProcessActionAfterBeepSpeak` coroutine to wait for dialogue text animation to finish, then process the action.
    *   If no action, starts `EnableInputAfterBeepSpeak` coroutine to wait for dialogue animation to finish, then re-enable input.
13. **Dynamic Wait Timing**: Both coroutines now calculate appropriate wait times based on text length and BeepSpeak's configured speed settings:
    ```csharp
    float estimatedCharTime = characterSpeed + speedVariance;
    float punctuationPauseEstimate = Mathf.Min(textLength * 0.2f, 4.0f);
    float calculatedWaitTime = Mathf.Max(3.0f, (textLength * estimatedCharTime) + punctuationPauseEstimate);
    float maxWaitTime = Mathf.Min(calculatedWaitTime, 20.0f);
    ```
14. **Function Execution (`ProcessActionAfterBeepSpeak` -> `ProcessFunctionCall`)**: If an action was parsed:
    *   `stop_conversation`: `DialogueControl.Deactivate()` is called.
    *   `reveal_node`: `node_id` is extracted, `GameControl.coreConstellation.DiscoverNode(nodeId)` is called.
15. **Input Re-enabling (`EnableInputAfterBeepSpeak` -> `EnableInput`)**: After BeepSpeak finishes, input is re-enabled if dialogue is still active.
16. **Deactivation (Escape Key)**: Player presses Escape. `DialogueControl.Update()` detects this and calls `Deactivate()`.
17. **Deactivation (Function Call)**: `stop_conversation` function call triggers `DialogueControl.Deactivate()`.
18. **Deactivation Process (`DeactivateDialogue` Coroutine)**:
    *   Calls `llmDialogueManager.ResetDialogue()` which calls `llmCharacter.CancelRequests()` and stops BeepSpeak typing. Waits for this task.
    *   Calls `llmCharacter.Save()` to save conversation history. Waits for this task.
    *   Plays deactivation animation.
    *   Deactivates Dialogue Canvas.
    *   Resets `GameControl` state to `DEFAULT`.

## 3. Function Calling Details

*   **Prompting**: `CharacterPromptGenerator` explicitly tells the LLM the available functions (`reveal_node`, `stop_conversation`), their parameters, and the **exact** output format (`\nACTION: function_name(param=value)` on a new line after dialogue, or alternatively `[/ACTION]: function_name(param=value)`). It also links revelation triggers to the `reveal_node` action.
*   **Parsing**: `BaseDialogueManager.HandleReply` checks for both `\nACTION:` and `[/ACTION]:` delimiters using `IndexOf`. When found, it sets the `actionFoundInCurrentStream` flag and extracts both the dialogue part (before the delimiter) and the function call part (after the delimiter). The function call is buffered for later processing.
*   **Improved Handling**: Now uses `actionBuffer.Clear(); actionBuffer.Append(cleaned);` to prevent redundant text accumulation, fixing the "revealreveal_node..." issue.
*   **Execution**: After BeepSpeak finishes (with dynamically calculated timeout), `ProcessActionAfterBeepSpeak` calls `ProcessFunctionCall` which uses `StartsWith` to identify the function name and then calls the relevant game logic (`DialogueControl.Deactivate` or `MysteryConstellation.DiscoverNode`).

## 4. State Management

*   **Game State**: `GameControl.currentState` is set to `GameState.DIALOGUE` during activation and back to `GameState.DEFAULT` during deactivation.
*   **Dialogue UI State**: `DialogueControl` manages the active state of the canvas and UI animations (`isTransitioning`).
*   **LLM Response State**: `BaseDialogueManager.isProcessingResponse` flag prevents sending new requests while waiting for a reply. `actionFoundInCurrentStream` flag prevents processing additional text chunks after an action has been found in the current stream.
*   **BeepSpeak State**: `BeepSpeak.typingCoroutine` tracks the active typing animation. `IsPlaying` property returns whether this coroutine is active.
*   **Input State**: `LLMDialogueManager` enables/disables the input field and submit button via `EnableInput`/`DisableInput`.
*   **HUD Element State**: `DialogueControl` now selectively manages HUD element visibility, keeping critical elements like `NodeUnlockNotif` and `PowerControl` visible during dialogue.

## 5. Dependencies & Interactions

*   `Player Interaction` -> `NPC` -> `DialogueControl.Activate`
*   `DialogueControl` -> `LLMCharacter` (Load, Save)
*   `DialogueControl` -> `LLMDialogueManager` (SetCharacter, InitializeDialogue, ResetDialogue)
*   `LLMDialogueManager` -> `LLMCharacter` (Chat)
*   `LLMCharacter` -> `BaseDialogueManager` (HandleReply, OnReplyComplete callbacks)
*   `BaseDialogueManager` -> `LLMDialogueManager` (UpdateDialogueDisplay, EnableInput, DisableInput - abstract calls)
*   `BaseDialogueManager` -> `DialogueControl` (Deactivate via ProcessFunctionCall)
*   `BaseDialogueManager` -> `MysteryConstellation` (DiscoverNode via ProcessFunctionCall)
*   `LLMDialogueManager` -> `DialogueControl` (DisplayNPCDialogueStreaming)
*   `DialogueControl` -> `BeepSpeak` (UpdateStreamingText)
*   `BaseDialogueManager` -> `BeepSpeak` (Waits for `IsPlaying` to become false via `DialogueControl.IsBeepSpeakPlaying`)

## 6. Resolved Issues

### ✅ Text Duplication
Fixed by changing `currentResponse.Append(reply)` to `currentResponse.Clear(); currentResponse.Append(reply)` in `BaseDialogueManager.HandleReply`. LLM response chunks contain the complete response so far, not just new content, so appending created duplicated text.

### ✅ Premature Animation Termination
Fixed by removing the redundant internal timeout in `BeepSpeak.cs` and improving timeout calculation in `BaseDialogueManager` coroutines to be based on text length and typing speed.

### ✅ Function Call Redundancy
Fixed by replacing `actionBuffer.Append(cleaned)` with `actionBuffer.Clear(); actionBuffer.Append(cleaned)` in `BaseDialogueManager.HandleReply` to prevent redundant text accumulation across chunks.

### ✅ Missing HUD Elements
Fixed by implementing selective child GameObject activation in `DialogueControl.Activate` to keep critical HUD elements like `NodeUnlockNotif` and `PowerControl` visible during dialogue.

### ✅ Animation Speed Issues
Fixed by dynamically calculating appropriate wait times based on text length and BeepSpeak's configured speed settings in `ProcessActionAfterBeepSpeak` and `EnableInputAfterBeepSpeak` coroutines.

## 7. Debugging Tools

### Input Flow Diagnostics
Debug logs with `[INPUTDBG]` tags track input re-enabling:
- When `OnReplyComplete` is called and `isProcessingResponse` flag status
- When `EnableInputAfterBeepSpeak` is started and waiting
- `BeepSpeak.typingCoroutine` state during waiting
- When `EnableInput` is called or skipped and why

### Performance Timing
Debug logs with `[TIMEDBG]` tags measure:
- Activation and deactivation durations
- UI animation times
- Dialogue initialization timing
- Conversation history saving/loading times
