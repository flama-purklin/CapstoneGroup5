# Proximity-Based Character Warmup & Conversation Persistence (Working)

## High-Level Design Overview

This design implements two critical features:

1.  **Conversation History Persistence** - Ensures character conversations persist between interactions.
2.  **Proximity-Based Warmup Management** - Keeps nearby characters "warm" (loaded in memory) while cooling distant ones.

The implementation leverages existing infrastructure from both the game and the LLMUnity plugin, with minimal new components to tie things together.

## System Architecture

### Existing Components Utilized

**From Your Game:**
*   `CharacterManager` - Creates and manages LLMCharacter states (template loading, warmup).
*   `NPCManager` - Spawns NPCs and caches LLMCharacter references.
*   `DialogueControl` - Manages conversation UI and triggers save/load.
*   `LLMDialogueManager` / `BaseDialogueManager` - Handles dialogue flow and character interaction.
*   `Character` - Links NPC GameObjects with their corresponding (duplicate) LLMCharacter instances.

**From LLMUnity Plugin:**
*   `LLMCharacter.Save()` - Built-in method to save conversation state.
*   `LLMCharacter.Load()` - Built-in method to reload conversation state.
*   `LLM.parallelPrompts` - Controls how many characters can be processed simultaneously ( **Crucially, must be set in Inspector** ).

### New Components & Modifications

1.  **`SimpleProximityWarmup`** - New MonoBehaviour managing which characters stay warm based on player distance to NPC GameObjects.
2.  **`CharacterManager` Modifications** - Initialization changed to only load templates; added explicit `WarmupCharacter` and `CooldownCharacter` methods; state transitions updated.
3.  **`DialogueControl` Modifications** - Triggers conversation state loading on activation and saving on dialogue exit.
4.  **`BaseDialogueManager` Modifications** - Added `CurrentCharacter` property.
5.  **`NPCManager` Modifications** - Initialization fixed to cache references without waiting for `Ready` state.
6.  **`LLMCharacter` Modifications** - Removed automatic save call from `Chat()` method; Added `OnDestroy` logic to clear save files.

## Implementation Details (Reflecting Current Code as of 2025-04-12)

*(Note: These snippets reflect the code after fixing history persistence and warmup/cooldown logic)*

```csharp
// In CharacterManager.cs

// ... (CharacterState enum and CharacterStateTransition class remain the same, including Ready -> LoadingTemplate transition) ...

private IEnumerator InitializeSingleCharacter(string characterName, LLMCharacter character, int index, int total)
{
    var stateTransition = stateTransitions[characterName];
    stateTransition.TryTransition(CharacterState.LoadingTemplate);

    // Load template only
    yield return StartCoroutine(LoadTemplateWithTimeout(character, characterName));
    
    if (stateTransitions[characterName].CurrentState == CharacterState.Failed)
    {
        Debug.LogWarning($"[CharacterManager InitSingleChar: {characterName}] Failed after LoadTemplate.");
        HandleCharacterFailure(characterName, character.gameObject);
        yield break;
    }

    // We don't automatically warm up characters on initialization anymore.
    // Characters will be warmed up selectively by the proximity system.
    // State remains LoadingTemplate until explicitly warmed up.
    Debug.Log($"[CharacterManager InitSingleChar: {characterName}] Template loaded successfully. State remains LoadingTemplate.");
}

// ... (LoadTemplateWithTimeout and WarmupWithRetries remain the same) ...

public IEnumerator WarmupCharacter(string characterName)
{
    // ... (Implementation as added previously) ...
    // Checks state is LoadingTemplate, transitions to WarmingUp, calls WarmupWithRetries, transitions to Ready/Failed
}

public void CooldownCharacter(string characterName)
{
    // ... (Implementation as added previously) ...
    // Checks state is Ready, transitions to LoadingTemplate, cancels requests
}

public void SaveCharacterConversation(string characterName)
{
    // ... (Null checks) ...
    // NOTE: The actual character.Save() call was removed from here as it was redundant.
    // DialogueControl now handles the single save call before notifying this manager.
    // This method now primarily updates the snapshot time.
    if (!characterSnapshots.ContainsKey(characterName))
        characterSnapshots[characterName] = new CharacterStateSnapshot();
        
    characterSnapshots[characterName].LastInteractionTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    Debug.Log($"Saved conversation state for {characterName}"); // Log remains for tracking
}

// Context allocation (FIXED)
private IEnumerator InitializeCharacters()
{
    // ... (Loop calling InitializeSingleCharacter) ...
    
    Debug.Log("[CharacterManager Init] All characters initialized, assigning context.");

    // Only set context if we have characters
    if (characterCache.Count > 0)
    {
        // FIXED: Now correctly divides by parallelPrompts (set in Inspector)
        int contextPerCharacter = sharedLLM.contextSize / sharedLLM.parallelPrompts; 
        Debug.Log($"CONTEXT ALLOCATION: {contextPerCharacter} tokens per active character (total: {sharedLLM.contextSize}, active slots: {sharedLLM.parallelPrompts})");
        foreach (var character in characterCache.Values)
        {
            character.nKeep = contextPerCharacter;
            Debug.Log($"Set nKeep={contextPerCharacter} for character '{character.name}'");
        }
    }
}

// Added OnDestroy to clear save files
private void OnDestroy()
{
    Debug.Log("[CharacterManager OnDestroy] Cleaning up character save files...");
    if (characterCache != null && characterCache.Count > 0)
    {
        var characterNames = new List<string>(characterCache.Keys); 
        foreach (string characterName in characterNames)
        {
            string saveFilePath = Path.Combine(Application.persistentDataPath, characterName + ".json");
            try {
                if (File.Exists(saveFilePath)) {
                    File.Delete(saveFilePath);
                    Debug.Log($"[CharacterManager OnDestroy] Deleted save file: {saveFilePath}");
                }
            } catch (Exception e) {
                Debug.LogError($"[CharacterManager OnDestroy] Error deleting save file for {characterName}: {e.Message}");
            }
        }
    }
    CleanupCharacters(); // Also cleanup in-memory objects
}
```

```csharp
// In NPCManager.cs
public async Task Initialize()
{
    // ... (Wait for CharacterManager) ...
    
    // FIXED: No longer waiting for characters to be in Ready state
    string[] characterNames = characterManager.GetAvailableCharacters();
    Debug.Log($"Found {characterNames.Length} characters to cache in NPCManager");
    
    foreach (string characterName in characterNames)
    {
        // Get character reference directly
        LLMCharacter llmCharacterRef = characterManager.GetCharacterByName(characterName);
        
        if (llmCharacterRef != null)
        {
            // Populate NPCManager's local cache
            characterCache[characterName] = llmCharacterRef;
            Debug.Log($"Cached reference for {characterName} in NPCManager");
        }
        else
        {
            Debug.LogWarning($"Could not get reference for {characterName} from CharacterManager");
        }
    }

    isInitialized = true;
    Debug.Log($"NPCManager initialization complete - cached {characterCache.Count} characters");
}

// ... (SpawnNPCInCar remains the same, uses the now-populated characterCache) ...
```

```csharp
// In BaseDialogueManager.cs
public LLMCharacter CurrentCharacter => llmCharacter;
```

```csharp
// In DialogueControl.cs
private IEnumerator DeactivateDialogue()
{
    // ... (Wait for resetTask) ...
    
    // Save conversation state for this character
    LLMCharacter activeLLMChar = llmDialogueManager.CurrentCharacter;
    if (activeLLMChar != null)
    {
        Debug.Log($"Saving conversation state for {activeLLMChar.save}");
        // Use LLMUnity's built-in save.
        _ = activeLLMChar.Save(activeLLMChar.save);
        
        // Also update CharacterManager's snapshot time (optional but potentially useful)
        CharacterManager characterManager = FindFirstObjectByType<CharacterManager>();
        if (characterManager != null)
        {
            characterManager.SaveCharacterConversation(activeLLMChar.save);
        }
    }
    // ... (Rest of deactivate logic) ...
}

// FIXED: Logic in Activate() now correctly loads conversation state
```

```csharp
// In SimpleProximityWarmup.cs
public class SimpleProximityWarmup : MonoBehaviour 
{
    // ... (References and Configuration) ...
    
    private void Start() 
    {
        // ... (Get references) ...
        
        // REMOVED: Attempt to set parallelPrompts via script was ineffective.
        // Must be set manually in LLM component Inspector before Play.

        // ... (Rest of Start) ...
    }
    
    private void UpdateWarmupState() 
    {
        // ... (Check if managers initialized) ...
        
        RefreshNPCCache(); // Gets NPC GameObjects
        
        // ... (Calculate distances using NPC positions from cachedNPCs) ...
        
        // ... (Sort by distance) ...
        
        // Process warming/cooling
        for (int i = 0; i < characterDistances.Count; i++) 
        {
            string name = characterDistances[i].name;
            bool shouldBeWarm = i < maxWarmCharacters;
            var currentState = characterManager.GetCharacterState(name); // Check state in CharacterManager
            
            if (shouldBeWarm && currentState == CharacterManager.CharacterState.LoadingTemplate) 
            {
                // Trigger warmup via CharacterManager
                StartCoroutine(characterManager.WarmupCharacter(name));
            } 
            else if (!shouldBeWarm && currentState == CharacterManager.CharacterState.Ready) 
            {
                // Trigger cooldown via CharacterManager
                characterManager.CooldownCharacter(name);
            }
        }
    }

    private void RefreshNPCCache()
    {
        // Finds active NPC GameObjects via Character components using FindObjectsByType
    }
}
```

## Current Status & Known Issues (As of 2025-04-11)

*   **Stability:** The game now loads successfully, and NPCs are spawned correctly. The initialization deadlock has been resolved.
*   **Proximity Warmup:** The `SimpleProximityWarmup` system appears to be functioning based on logs, triggering warmup/cooldown based on distance to NPCs.
*   **ISSUE: Conversation Amnesia:** Characters do not retain conversation history. While saving is triggered (`.Save()`), the history is **not being loaded back** when initiating a new conversation.
*   **ISSUE: Incorrect Parallel Prompts:** The LLM server consistently starts with `parallelPrompts = 1` (seen in logs), despite `SimpleProximityWarmup` attempting to set it later. This severely limits concurrent processing and likely causes gibberish responses when multiple characters should be active. The value **must be set in the LLM component Inspector** before starting the game.
*   **ISSUE: Potential Insufficient Context (`nKeep`):** `CharacterManager` currently allocates context based on the *total* number of characters (9), not the number of *active* (warm) characters (intended to be 3). This likely starves active characters of needed context space, contributing to poor responses.
*   **ISSUE: Memory Leak Warnings:** Unity reports `JobTempAlloc` warnings during gameplay, potentially related to LLM operations or resource management. Needs monitoring after other fixes are applied.

## Next Steps & Potential Fixes

1.  **Fix Parallel Prompts (Manual Action Required):** The `LLM.parallelPrompts` value **must be set manually in the Unity Inspector** on the `LLM` GameObject to match the desired `maxWarmCharacters` (e.g., 3). The attempt to set it via script in `SimpleProximityWarmup.Start()` is too late and should be removed.
2.  **Implement Conversation Loading:** Modify `DialogueControl.Activate()` to call `activeLLMChar.Load(activeLLMChar.save)` *before* `llmDialogueManager.SetCharacter()`. This requires getting the `LLMCharacter` reference from the `Character` component on the `npcObject`.
3.  **Adjust Context Allocation:** Modify `CharacterManager.InitializeCharacters()` to calculate `nKeep` based on `sharedLLM.parallelPrompts` (read from the Inspector-set value) instead of `characterCache.Count`.
4.  **Monitor Memory Leaks:** After addressing the above, observe if the memory leak warnings persist. Further investigation might be needed, potentially involving profiling or examining LLMUnity internals if the issue isn't resolved by fixing the load/prompt/context logic.

*(This documentation leaves room for alternative approaches if these fixes prove insufficient, particularly regarding context management or memory leaks. The core goal is functional persistence and proximity-based resource management.)*
