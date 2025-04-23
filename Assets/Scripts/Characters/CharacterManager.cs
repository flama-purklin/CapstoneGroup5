using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Collections;
using LLMUnity;
using System;
using TMPro;
using Newtonsoft.Json; // Added for serialization

public class CharacterManager : MonoBehaviour
{
    [Header("Configuration")]
    public LLM sharedLLM;
    [Tooltip("Enable saving/loading of LLM cache state? Improves re-warmup speed but currently causes JobTempAlloc warnings after conversations.")]
    public bool enableLLMCache = false; // Default to false to avoid warnings
    
    // References (assigned in Start/Awake)
    private NPCManager npcManager; 

    // Caches and State
    private Dictionary<string, LLMCharacter> characterCache = new Dictionary<string, LLMCharacter>(); 
    private Dictionary<string, CharacterStateTransition> stateTransitions = new Dictionary<string, CharacterStateTransition>();
    private Dictionary<string, string> promptCache = new Dictionary<string, string>(); 

    private bool isInitialized = false;
    private bool isInitializing = false;
    private bool isSwitchingCharacter = false; 
    private Transform charactersContainer; 
    private LLMCharacter currentCharacter; 

    public bool IsInitialized => isInitialized;
    public bool IsSwitchingCharacter => isSwitchingCharacter;
    public event System.Action OnInitializationComplete;

    [Header("Initialization Settings")]
    public float templateTimeout = 15f;
    public float warmupTimeout = 30f;
    public int maxWarmupAttempts = 3;
    public float baseBackoffDelay = 1f;

    [Header("Default Language Model Character Settings (Applied to created LLMCharacters)")]
    public float temperature =  0.8f;
    public int topK = 55;
    public float topP = 0.9f;
    public float repeatPenalty = 1.0f;
    public float presencePenalty = 0.0f;
    public float frequencyPenalty = 1.0f;

    public enum CharacterState { Uninitialized, LoadingTemplate, WarmingUp, Ready, Failed }

    private class CharacterStateTransition {
        public CharacterState CurrentState { get; private set; }
        public string CharacterName { get; private set; }
        public GameObject GameObject { get; private set; } 
        public CharacterStateTransition(string name, CharacterState state, GameObject gameObject = null) { CharacterName = name; CurrentState = state; GameObject = gameObject; }
        public bool TryTransition(CharacterState newState) { CurrentState = newState; return true; }
    }

    void Awake() {
        if (sharedLLM == null) sharedLLM = FindFirstObjectByType<LLM>();
        if (npcManager == null) npcManager = FindFirstObjectByType<NPCManager>();
        ValidateConfiguration();
        OrganizeHierarchy();
        
        // Apply the recommended configuration for LLM
        ApplyOptimalLLMConfiguration();
    }
    
    /// <summary>
    /// Apply the recommended configuration for the LLM to optimize performance
    /// </summary>
    private void ApplyOptimalLLMConfiguration()
    {
        if (sharedLLM != null)
        {
            Debug.Log("[LLM_UPDATE_DEBUG] Applying optimized LLM configuration");
            
            // Record original values for logging
            int originalPrompts = sharedLLM.parallelPrompts;
            int originalContext = sharedLLM.contextSize;
            
            // Apply new values
            sharedLLM.parallelPrompts = 1;     // Single-slot configuration
            sharedLLM.contextSize = 6144;      // 6K tokens context
            
            Debug.Log($"[LLM_UPDATE_DEBUG] LLM Configuration updated: parallelPrompts {originalPrompts} → {sharedLLM.parallelPrompts}, " +
                      $"contextSize {originalContext} → {sharedLLM.contextSize}");
                      
            // Note: n_batch setting would be configured in the LLM inspector 
            // or via command line arguments when launching the LLM process
        }
        else
        {
            Debug.LogError("[LLM_UPDATE_DEBUG] Cannot apply LLM configuration - sharedLLM is null");
        }
    }
    
    private void OrganizeHierarchy() {
        charactersContainer = transform.Find("Characters");
        if (charactersContainer == null) {
            GameObject containerObj = new GameObject("Characters");
            containerObj.transform.SetParent(transform);
            charactersContainer = containerObj.transform;
        }
    }

    public void Initialize() {
        if (isInitialized || isInitializing) { Debug.LogWarning("CharacterManager Initialize called but already initialized or initializing."); return; }
        Debug.Log("CharacterManager Initialize called.");
        StartCoroutine(TwoPhaseInitialization()); 
    }

    private void ValidateConfiguration() {
        if (!sharedLLM) Debug.LogError("CharacterManager: Shared LLM reference not found!");
        templateTimeout = Mathf.Max(5f, templateTimeout);
        warmupTimeout = Mathf.Max(10f, warmupTimeout);
        maxWarmupAttempts = Mathf.Max(1, maxWarmupAttempts);
        baseBackoffDelay = Mathf.Max(1f, baseBackoffDelay);
    }

    private IEnumerator TwoPhaseInitialization() {
        Debug.Log("CharacterManager: Starting TwoPhaseInitialization...");
        isInitializing = true;
        characterCache.Clear();
        stateTransitions.Clear();
        promptCache.Clear();

        Debug.Log("CharacterManager: Phase 1 - Creating LLMCharacter objects...");
        yield return StartCoroutine(CreateCharacterObjects()); 
        Debug.Log($"CharacterManager: Phase 1 complete. Created {characterCache.Count} LLMCharacter objects.");

        Debug.Log("CharacterManager: Phase 2 - Loading templates and allocating context...");
        yield return StartCoroutine(InitializeLLMCharacters()); 
        Debug.Log("CharacterManager: Phase 2 complete.");

        isInitialized = true;
        isInitializing = false;
        Debug.Log("CharacterManager: Initialization Complete. Invoking OnInitializationComplete!");
        OnInitializationComplete?.Invoke();
    }

     private IEnumerator CreateCharacterObjects() {
        if (GameControl.GameController == null || GameControl.GameController.coreMystery == null || GameControl.GameController.coreMystery.Characters == null) { Debug.LogError("CharacterManager: GameControl or coreMystery data not ready!"); yield break; }
        var charactersData = GameControl.GameController.coreMystery.Characters;
        if (charactersData.Count == 0) { Debug.LogWarning("CharacterManager: No characters found in coreMystery data."); yield break; }
        int processedCount = 0;
        Debug.Log($"CharacterManager: Found {charactersData.Count} characters. Processing...");
        foreach (var kvp in charactersData) 
        {
            // --- Victim Check ---
            string role = kvp.Value?.Core?.Involvement?.Role;
            if (!string.IsNullOrEmpty(role) && role.Equals("victim", StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"CharacterManager: Skipping character '{kvp.Key}' because their role is 'victim'.");
                continue; // Skip this character
            }
            // --- End Victim Check ---

            yield return StartCoroutine(CreateSingleCharacterObject(kvp.Key, kvp.Value)); 
            processedCount++;
        }
        Debug.Log($"CharacterManager: Finished processing. Attempted to create {processedCount} non-victim LLMCharacter objects.");
        if (characterCache.Count == 0 && processedCount > 0) Debug.LogError("CharacterManager: No character objects were successfully created despite processing non-victims!");
        else if (characterCache.Count == 0 && processedCount == 0) Debug.LogWarning("CharacterManager: No non-victim characters found to create.");
    }

    private IEnumerator CreateSingleCharacterObject(string characterName, MysteryCharacter mysteryCharacterData) {
        if (mysteryCharacterData == null) { Debug.LogWarning($"Character data for '{characterName}' is null."); yield break; }
        try {
            GameObject charObj = new GameObject($"{characterName}"); 
            charObj.transform.SetParent(charactersContainer, false);
            LLMCharacter character = charObj.AddComponent<LLMCharacter>();
            character.llm = sharedLLM;
            character.stream = true;
            character.save = characterName;
            character.saveCache = enableLLMCache; 
            character.setNKeepToPrompt = true; 
            character.numPredict = -1; 
            character.temperature = temperature; 
            character.topK = topK;
            character.topP = topP;
            character.repeatPenalty = repeatPenalty;
            character.presencePenalty = presencePenalty;
            character.frequencyPenalty = frequencyPenalty;
            // string jsonContent = JsonConvert.SerializeObject(mysteryCharacterData, Formatting.Indented); // Removed intermediate serialization
            // Pass the object directly with mystery context and title
            string mysteryContext = GameControl.GameController.coreMystery.Metadata?.Context ?? "Unknown context";
            string mysteryTitle = GameControl.GameController.coreMystery.Metadata?.Title ?? "Unknown mystery";
            string systemPrompt = CharacterPromptGenerator.GenerateSystemPrompt(mysteryCharacterData, character, mysteryContext, mysteryTitle); 
            if (string.IsNullOrEmpty(systemPrompt)) { Debug.LogError($"Failed prompt gen for {characterName}"); Destroy(charObj); yield break; }
            promptCache[characterName] = systemPrompt; 
            character.SetPrompt(systemPrompt, true); 
            characterCache[characterName] = character; 
            stateTransitions[characterName] = new CharacterStateTransition(characterName, CharacterState.Uninitialized, charObj); 
        } catch (Exception e) { Debug.LogError($"Error creating character object {characterName}: {e.Message}\n{e.StackTrace}"); }
        yield return null; 
    }

    private IEnumerator InitializeLLMCharacters() {
        if (characterCache.Count == 0) { Debug.LogError("CharacterManager: No characters in cache to initialize!"); yield break; }
        while (!sharedLLM.started) { yield return null; }
        yield return new WaitForSeconds(1f); 
        Debug.Log($"CharacterManager: Loading templates for {characterCache.Count} characters...");
        List<Task> templateLoadTasks = new List<Task>();
        foreach (var kvp in characterCache) {
            string characterName = kvp.Key;
            LLMCharacter llmCharacterRef = kvp.Value;
            if (llmCharacterRef != null) {
                 if (stateTransitions[characterName].TryTransition(CharacterState.LoadingTemplate)) {
                     templateLoadTasks.Add(LoadTemplateWithTimeout(llmCharacterRef, characterName));
                 } else { Debug.LogError($"CM: Failed state transition for {characterName} before template load."); }
            } else { Debug.LogWarning($"CM: Null reference in characterCache for {characterName} during template load."); }
        }
        if (templateLoadTasks.Count > 0) {
            Task allTemplateTasks = Task.WhenAll(templateLoadTasks);
            yield return new WaitUntil(() => allTemplateTasks.IsCompleted); 
            if (allTemplateTasks.IsFaulted) { Debug.LogError($"CM: Template load tasks failed: {allTemplateTasks.Exception}"); }
        }
        Debug.Log("CharacterManager: Template loading attempts complete.");
        AllocateContext(); 
    }

    private void AllocateContext() { 
         if (characterCache.Count > 0) {
            try {
                Debug.Log($"[LLM_UPDATE_DEBUG] AllocateContext starting, Character count: {characterCache.Count}");
                
                if (sharedLLM.parallelPrompts <= 0) { 
                    Debug.LogError($"[LLM_UPDATE_DEBUG] CRITICAL: parallelPrompts invalid: {sharedLLM.parallelPrompts}. Using fallback 3."); 
                    sharedLLM.parallelPrompts = 3; 
                }
                
                // No longer divide by parallelPrompts - use full context for each character
                int contextPerCharacter = sharedLLM.contextSize;
                
                Debug.Log($"[LLM_UPDATE_DEBUG] CONTEXT ALLOCATION: Full context used. {contextPerCharacter} tokens/char (total: {sharedLLM.contextSize}, parallelPrompts={sharedLLM.parallelPrompts})");
                Debug.Log($"[LLM_UPDATE_DEBUG] Using LLM cache: {enableLLMCache}");
                
                foreach (var kvp in characterCache) {
                    string characterName = kvp.Key;
                    LLMCharacter character = kvp.Value;
                    
                    if (character != null) { 
                        try { 
                            int previousNKeep = character.nKeep;
                            bool previousSaveCache = character.saveCache;
                            
                            character.nKeep = contextPerCharacter;
                            // Ensure cache saving is enabled
                            character.saveCache = enableLLMCache; 
                            
                            Debug.Log($"[LLM_UPDATE_DEBUG] Character '{characterName}': nKeep {previousNKeep} → {character.nKeep}, saveCache {previousSaveCache} → {character.saveCache}");
                        } catch (Exception e) { 
                            Debug.LogError($"[LLM_UPDATE_DEBUG] Failed setting context for {kvp.Key}: {e.Message}"); 
                        } 
                    } else {
                        Debug.LogWarning($"[LLM_UPDATE_DEBUG] Character '{characterName}' is null in cache");
                    }
                } 
                Debug.Log($"[LLM_UPDATE_DEBUG] Finished setting context for {characterCache.Count} characters.");
            } catch (Exception e) { 
                Debug.LogError($"[LLM_UPDATE_DEBUG] CRITICAL context allocation error: {e.Message}\n{e.StackTrace}"); 
            }
        } else { 
            Debug.LogWarning("[LLM_UPDATE_DEBUG] AllocateContext: No characters in cache."); 
        }
    }

    private async Task LoadTemplateWithTimeout(LLMCharacter character, string characterName) {
        if (character == null) { Debug.LogError($"LoadTemplate: Null LLMChar for {characterName}"); if (stateTransitions.ContainsKey(characterName)) stateTransitions[characterName].TryTransition(CharacterState.Failed); return; }
        Task templateTask = null;
        try { if (character.llm == null) character.llm = sharedLLM; templateTask = character.LoadTemplate(); } 
        catch (Exception e) { Debug.LogError($"Error starting template load for {characterName}: {e.Message}"); if (stateTransitions.ContainsKey(characterName)) stateTransitions[characterName].TryTransition(CharacterState.Failed); return; }
        Task timeoutTask = Task.Delay(TimeSpan.FromSeconds(templateTimeout));
        Task completedTask = await Task.WhenAny(templateTask, timeoutTask);
        if (completedTask == timeoutTask || templateTask.IsFaulted) { string errorMsg = templateTask.IsFaulted ? $"Template load failed: {templateTask.Exception?.GetBaseException()?.Message}" : "Template load timed out"; Debug.LogError($"{characterName}: {errorMsg}"); if (stateTransitions.ContainsKey(characterName)) stateTransitions[characterName].TryTransition(CharacterState.Failed); }
    }

    private IEnumerator WarmupWithRetries(LLMCharacter character, string characterName) {
        if (character == null) { Debug.LogError($"Warmup: Null LLMChar for {characterName}"); if (stateTransitions.ContainsKey(characterName)) stateTransitions[characterName].TryTransition(CharacterState.Failed); yield break; }
        for (int attempt = 1; attempt <= maxWarmupAttempts; attempt++) {
            if (attempt > 1) yield return new WaitForSeconds(Mathf.Pow(2, attempt - 1) * baseBackoffDelay);
            Task warmupTask = null;
            try { if (character.llm == null) character.llm = sharedLLM; warmupTask = character.Warmup(); } 
            catch (Exception e) { Debug.LogError($"[CM Warmup] Error starting warmup for {characterName} (attempt {attempt}): {e.Message}"); if (attempt == maxWarmupAttempts) { Debug.LogError($"[CM Warmup] All attempts failed for {characterName}."); if (stateTransitions.ContainsKey(characterName)) stateTransitions[characterName].TryTransition(CharacterState.Failed); yield break; } continue; }
            float timeoutTime = Time.time + warmupTimeout;
            while (!warmupTask.IsCompleted && Time.time <= timeoutTime) yield return null;
            if (Time.time > timeoutTime || warmupTask.IsFaulted) { string errorMsg = warmupTask.IsFaulted ? $"Warmup failed: {warmupTask.Exception?.GetBaseException()?.Message}" : "Warmup timed out"; Debug.LogError($"{characterName}: {errorMsg} (Attempt {attempt})"); if (attempt < maxWarmupAttempts) Debug.LogWarning($"[CM Warmup] {characterName} attempt {attempt} failed, retrying..."); else { Debug.LogError($"[CM Warmup] All attempts failed for {characterName}"); if (stateTransitions.ContainsKey(characterName)) stateTransitions[characterName].TryTransition(CharacterState.Failed); } } 
            else { if (stateTransitions.ContainsKey(characterName)) stateTransitions[characterName].TryTransition(CharacterState.Ready); yield break; }
        }
    }

    private void HandleCharacterFailure(string characterName, GameObject charObj = null) { if (stateTransitions.ContainsKey(characterName)) { stateTransitions[characterName].TryTransition(CharacterState.Failed); Debug.LogError($"Character {characterName} marked as Failed."); if (charObj != null) Destroy(charObj); } else { Debug.LogError($"Attempted to mark unknown character {characterName} as Failed."); } }
    
    public async Task<LLMCharacter> SwitchToCharacter(string characterName) { 
        if (!isInitialized || !characterCache.ContainsKey(characterName)) { Debug.LogError($"SwitchToCharacter: Cannot switch. Not initialized or '{characterName}' not cached."); return null; } 
        if (!stateTransitions.ContainsKey(characterName)) { Debug.LogError($"SwitchToCharacter: No state for '{characterName}'."); return null; } 
        if (stateTransitions[characterName].CurrentState != CharacterState.Ready) { Debug.LogWarning($"SwitchToCharacter: '{characterName}' not Ready (State: {stateTransitions[characterName].CurrentState})."); return null; } 
        if (isSwitchingCharacter) { Debug.LogWarning("SwitchToCharacter: Already switching."); return null; } 
        isSwitchingCharacter = true; 
        try { if (currentCharacter != null && stateTransitions.ContainsKey(currentCharacter.name) && stateTransitions[currentCharacter.name].CurrentState == CharacterState.Ready) { currentCharacter.CancelRequests(); await Task.Yield(); } currentCharacter = characterCache[characterName]; return currentCharacter; } 
        finally { isSwitchingCharacter = false; } 
    }

    public LLMCharacter GetCurrentCharacter() => currentCharacter;
    public string[] GetAvailableCharacters() => characterCache.Keys.ToArray(); 
    public bool IsCharacterReady(string characterName) { return stateTransitions.ContainsKey(characterName) && stateTransitions[characterName].CurrentState == CharacterState.Ready; }
    public CharacterState GetCharacterState(string characterName) { return stateTransitions.ContainsKey(characterName) ? stateTransitions[characterName].CurrentState : CharacterState.Uninitialized; }
    public class CharacterStateSnapshot { public string CharacterName { get; set; } public List<ChatMessage> ChatHistory { get; set; } public int CurrentSlot { get; set; } public long LastInteractionTime { get; set; } }
    private Dictionary<string, CharacterStateSnapshot> characterSnapshots = new Dictionary<string, CharacterStateSnapshot>();
    private void OnValidate() { templateTimeout = Mathf.Max(5f, templateTimeout); warmupTimeout = Mathf.Max(10f, warmupTimeout); maxWarmupAttempts = Mathf.Max(1, maxWarmupAttempts); baseBackoffDelay = Mathf.Max(1f, baseBackoffDelay); }
    
    private void CleanupCharacters() { 
        Debug.Log("CharacterManager: CleanupCharacters called."); 
        if (currentCharacter != null && currentCharacter.llm != null && currentCharacter.llm.started) { currentCharacter.CancelRequests(); } 
        foreach (var kvp in characterCache) { LLMCharacter character = kvp.Value; if (character != null) { if (character.llm != null && character.llm.started) { character.CancelRequests(); } Destroy(character.gameObject); } } 
        characterCache.Clear(); stateTransitions.Clear(); promptCache.Clear(); 
        isInitialized = false; isInitializing = false; isSwitchingCharacter = false; currentCharacter = null; 
        Debug.Log("CharacterManager: Cleanup complete."); 
    }

    public void ResetCharacter(string characterName) { 
        if (!characterCache.ContainsKey(characterName)) { Debug.LogError($"Cannot reset non-existent character: {characterName}"); return; } 
        var character = characterCache[characterName]; 
        if (character != null) { Debug.Log($"Resetting character {characterName}"); character.CancelRequests(); character.ClearChat(); if (promptCache.ContainsKey(characterName)) { character.SetPrompt(promptCache[characterName], true); } else { Debug.LogError($"No prompt found in cache for {characterName}"); } } 
        else { Debug.LogError($"Cached ref for {characterName} is null during Reset."); } 
    }

    public void LogCharacterState(string characterName) { if (!characterCache.ContainsKey(characterName)) { Debug.LogError($"Cannot log state for non-existent char: {characterName}"); return; } var character = characterCache[characterName]; if (character == null) { Debug.LogError($"Character {characterName} is null!"); return; } /* Add logging */ }
    
    public string GetCharacterDebugInfo() { 
        var info = new System.Text.StringBuilder(); 
        info.AppendLine("--- Character Manager Status ---"); 
        info.AppendLine($"Initialized: {isInitialized}"); info.AppendLine($"Initializing: {isInitializing}"); info.AppendLine($"Switching Character: {isSwitchingCharacter}"); 
        info.AppendLine($"Owned LLMCharacter Objects: {characterCache.Count}"); info.AppendLine($"Tracked States: {stateTransitions.Count}"); 
        info.AppendLine($"Current Dialogue Character: {(currentCharacter != null ? currentCharacter.name : "None")}"); info.AppendLine(); 
        foreach (var kvp in stateTransitions) { 
            string characterName = kvp.Key; CharacterStateTransition stateInfo = kvp.Value; 
            info.AppendLine($"Character: {characterName}"); info.AppendLine($"- State: {stateInfo.CurrentState}"); 
            if (characterCache.TryGetValue(characterName, out LLMCharacter llmChar) && llmChar != null) { 
                info.AppendLine($"- LLMCharacter InstanceID: {llmChar.GetInstanceID()}"); info.AppendLine($"- GameObject Active: {llmChar.gameObject.activeInHierarchy}"); 
                info.AppendLine($"- Has LLM Ref: {llmChar.llm != null}"); info.AppendLine($"- SaveCache Setting: {llmChar.saveCache}"); 
            } else { info.AppendLine("- LLMCharacter reference MISSING in cache!"); } info.AppendLine(); 
        } info.AppendLine("--- End Character Manager Status ---"); return info.ToString(); 
    }
    
    public float GetInitializationProgress() { if (!isInitialized) return 0f; int total = stateTransitions.Count; if (total == 0) return 0f; int ready = stateTransitions.Count(x => x.Value.CurrentState == CharacterState.Ready); return (float)ready / total; } 
    
    public LLMCharacter GetCharacterByName(string characterName) { 
        characterCache.TryGetValue(characterName, out LLMCharacter character); 
        // if (character == null) Debug.LogWarning($"GetCharacterByName: Character '{characterName}' not found in cache."); // Reduce log spam
        return character; 
    }
    
    public int GetReadyCharacterCount() { return stateTransitions.Count(x => x.Value.CurrentState == CharacterState.Ready); }

    public IEnumerator WarmupCharacter(string characterName) { 
        if (!isInitialized) { Debug.LogError($"Cannot warm up char '{characterName}' - Not initialized"); yield break; } 
        if (!characterCache.ContainsKey(characterName)) { Debug.LogError($"Cannot warm up non-existent char: {characterName}"); yield break; } 
        if (!stateTransitions.ContainsKey(characterName)) { Debug.LogError($"No state found for char: {characterName}"); yield break; } 
        if (stateTransitions[characterName].CurrentState != CharacterState.LoadingTemplate) { yield break; } 
        var character = characterCache[characterName]; 
        if (!stateTransitions[characterName].TryTransition(CharacterState.WarmingUp)) { Debug.LogError($"[CM Warmup] Failed transition {characterName} to WarmingUp!"); yield break; } 
        Debug.Log($"[CM Warmup] Transitioned {characterName} to WarmingUp. Starting coroutine..."); 
        yield return StartCoroutine(WarmupWithRetries(character, characterName)); 
        if (stateTransitions.ContainsKey(characterName)) { 
            if (stateTransitions[characterName].CurrentState == CharacterState.Failed) { Debug.LogError($"Failed warmup for {characterName}"); } 
            else if (stateTransitions[characterName].CurrentState == CharacterState.Ready) { Debug.Log($"[CM Warmup] SUCCESS: '{characterName}' warmed up."); } 
            else { Debug.LogWarning($"[CM Warmup] UNEXPECTED: '{characterName}' finished warmup in state {stateTransitions[characterName].CurrentState}."); } 
        } 
    }
    
    public void CooldownCharacter(string characterName) { 
        if (!isInitialized) { Debug.LogWarning($"Cannot cool down char '{characterName}' - Not initialized"); return; } 
        if (!characterCache.ContainsKey(characterName)) { Debug.LogError($"Cannot cool down non-existent char: {characterName}"); return; } 
        if (!stateTransitions.ContainsKey(characterName)) { Debug.LogError($"No state found for char: {characterName}"); return; } 
        if (stateTransitions[characterName].CurrentState != CharacterState.Ready) { return; } 
        var character = characterCache[characterName]; 
        if (character == null) { Debug.LogError($"Cooldown: Null LLMChar ref for {characterName}"); return; }
        character.CancelRequests(); 
        if (stateTransitions[characterName].TryTransition(CharacterState.LoadingTemplate)) { Debug.Log($"Cooled down character: {characterName}"); } 
        else { Debug.LogError($"[CM Cooldown] FAILED transition {characterName} from Ready to LoadingTemplate."); } 
    }
    
    private void OnDestroy() { 
        Debug.Log("[CharacterManager OnDestroy] Cleaning up..."); 
        List<string> characterNamesToClean = new List<string>(characterCache.Keys); 
        CleanupCharacters(); // Cleanup in-memory refs FIRST
        if (characterNamesToClean.Count > 0) { 
            Debug.Log($"[CM OnDestroy] Deleting files for {characterNamesToClean.Count} characters..."); 
            foreach (string characterName in characterNamesToClean) { 
                string saveFilePath = Path.Combine(Application.persistentDataPath, characterName + ".json"); 
                string cacheFilePath = Path.Combine(Application.persistentDataPath, characterName + ".cache"); 
                try { 
                    if (File.Exists(saveFilePath)) { File.Delete(saveFilePath); Debug.Log($"[CM OnDestroy] Deleted save file: {saveFilePath}"); } 
                    if (enableLLMCache && File.Exists(cacheFilePath)) { File.Delete(cacheFilePath); Debug.Log($"[CM OnDestroy] Deleted cache file: {cacheFilePath}"); } 
                } catch (Exception e) { Debug.LogError($"[CM OnDestroy] Error deleting file for {characterName}: {e.Message}"); } 
            } 
        } else { Debug.Log("[CM OnDestroy] No character names recorded before cleanup."); } 
        Debug.Log("[CM OnDestroy] Cleanup finished."); 
    }
}

// Helper extension method to wait for Task in Coroutine
public static class TaskExtensions
{
    public static IEnumerator AsCoroutine(this Task task)
    {
        while (!task.IsCompleted) yield return null;
        if (task.IsFaulted) throw task.Exception;
    }
}
