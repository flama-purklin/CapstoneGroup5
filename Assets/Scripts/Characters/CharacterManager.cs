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
    public string charactersFolder = "Characters";
    public LLM sharedLLM;
    private Dictionary<string, LLMCharacter> characterCache = new Dictionary<string, LLMCharacter>();
    private Dictionary<string, string> promptCache = new Dictionary<string, string>();
    private bool isInitialized = false;
    private bool isInitializing = false;
    private bool isSwitchingCharacter = false;
    private Transform charactersContainer;
    private LLMCharacter currentCharacter;
    // private ParsingControl parsingControl; // Removed reference

    public bool IsInitialized => isInitialized;
    public bool IsSwitchingCharacter => isSwitchingCharacter;
    public event System.Action OnInitializationComplete;

    [Header("Initialization Settings")]
    public float characterInitDelay = 2f;
    public float templateTimeout = 15f;
    public float warmupTimeout = 30f;
    public int maxWarmupAttempts = 3;
    public float baseBackoffDelay = 1f;

    [Header("Default Language Model Charater Settings")]
    public float temperature =  0.8f;
    public int topK = 55;
    public float topP = 0.9f;
    public float repeatPenalty = 1.0f;
    public float presencePenalty = 0.0f;
    public float frequencyPenalty = 1.0f;

    public enum CharacterState
    {
        Uninitialized,
        LoadingTemplate,
        WarmingUp,
        Ready,
        Failed
    }

    private class CharacterStateTransition
    {
        public CharacterState CurrentState { get; private set; }
        public string CharacterName { get; private set; }
        public GameObject GameObject { get; private set; }

        public CharacterStateTransition(string name, CharacterState state, GameObject gameObject = null)
        {
            CharacterName = name;
            CurrentState = state;
            GameObject = gameObject;
        }

        public bool TryTransition(CharacterState newState)
        {
            switch (CurrentState)
            {
                case CharacterState.Uninitialized:
                    if (newState != CharacterState.LoadingTemplate) return false;
                    break;
                case CharacterState.LoadingTemplate:
                    if (newState != CharacterState.WarmingUp && newState != CharacterState.Failed) return false;
                    break;
                case CharacterState.WarmingUp:
                    if (newState != CharacterState.Ready && newState != CharacterState.Failed) return false;
                    break;
                case CharacterState.Ready:
                    if (newState != CharacterState.Failed) return false;
                    break;
                case CharacterState.Failed:
                    if (newState != CharacterState.LoadingTemplate) return false;
                    break;
            }

            CurrentState = newState;
            return true;
        }
    }

    private Dictionary<string, CharacterStateTransition> stateTransitions =
    new Dictionary<string, CharacterStateTransition>();

    void Awake()
    {
        ValidateConfiguration();
        OrganizeHierarchy();
        // Initialization is now triggered explicitly by InitializationManager
    }

    /// <summary>
    /// Public method to start the character initialization process.
    /// Should be called by InitializationManager after parsing is complete.
    /// </summary>
    public void Initialize()
    {
        if (isInitialized || isInitializing)
        {
            Debug.LogWarning("CharacterManager Initialize called but already initialized or initializing.");
            return;
        }
        
        StartCoroutine(TwoPhaseInitialization());
    }

    // Removed Start() method (event subscription)
    // Removed OnDestroy() method (event unsubscription)
    // Removed HandleParsingComplete() method (event handler)

    private void ValidateConfiguration()
    {
        if (!sharedLLM)
        {
            sharedLLM = FindFirstObjectByType<LLM>();
            if (!sharedLLM)
            {
                Debug.LogError("No LLM found in scene! Please assign an LLM to CharacterManager.");
                return;
            }
        }

        characterInitDelay = Mathf.Max(2f, characterInitDelay);
        templateTimeout = Mathf.Max(5f, templateTimeout);
        warmupTimeout = Mathf.Max(10f, warmupTimeout);
        maxWarmupAttempts = Mathf.Max(1, maxWarmupAttempts);
        baseBackoffDelay = Mathf.Max(1f, baseBackoffDelay);
    }

    private void OrganizeHierarchy()
    {
        charactersContainer = transform.Find("Characters");
        if (charactersContainer == null)
        {
            GameObject containerObj = new GameObject("Characters");
            containerObj.transform.SetParent(transform);
            charactersContainer = containerObj.transform;
        }
    }

    private IEnumerator TwoPhaseInitialization()
    {
        
        if (isInitialized || isInitializing) yield break;
        isInitializing = true;

        // Phase 1: Create Character Objects and Load Prompts
        Debug.Log("CharacterManager's Initialization: Phase 1: Create Character Objects and Load Prompts...");
        yield return StartCoroutine(CreateCharacterObjects());
        Debug.Log("CharacterManager's Initialization: Phase 1 complete!");

        // Phase 2: Initialize LLM and Warm Up Characters
        Debug.Log("CharacterManager's Initialization: Phase 2: Initialize LLM and Warm Up Characters");
        yield return StartCoroutine(InitializeCharacters());
        Debug.Log("CharacterManager's Initialization: Phase 2 complete!");

        isInitialized = true;
        isInitializing = false;

        Debug.Log("CharacterManager's Initialization: Complete, invoking OnInitializationComplete!");
        OnInitializationComplete?.Invoke();
    }

    private IEnumerator CreateCharacterObjects()
    {
        // Check if GameController and mystery data are ready
        if (GameControl.GameController == null || GameControl.GameController.coreMystery == null || GameControl.GameController.coreMystery.Characters == null)
        {
            Debug.LogError("CharacterManager: GameControl or coreMystery data not ready for character creation!");
            yield break;
        }

        var charactersData = GameControl.GameController.coreMystery.Characters;
        if (charactersData.Count == 0)
        {
             Debug.LogWarning("CharacterManager: No characters found in coreMystery data.");
             yield break;
        }

        Debug.Log($"CharacterManager: Found {charactersData.Count} characters in coreMystery. Creating objects...");

        foreach (var kvp in charactersData)
        {
            string characterName = kvp.Key;
            MysteryCharacter mysteryCharacterData = kvp.Value;

            if (mysteryCharacterData == null)
            {
                Debug.LogWarning($"Character data for '{characterName}' is null. Skipping.");
                continue;
            }

            yield return StartCoroutine(CreateSingleCharacterObject(characterName, mysteryCharacterData));
        }

        if (characterCache.Count == 0)
        {
            Debug.LogError("CharacterManager: No character objects were successfully created!");
            yield break;
        }

        Debug.Log($"CharacterManager: Successfully created {characterCache.Count} character objects from coreMystery data.");
    }

    private IEnumerator CreateSingleCharacterObject(string characterName, MysteryCharacter mysteryCharacterData)
    {
        try
        {
            GameObject charObj = new GameObject($"{characterName}");
            charObj.transform.SetParent(charactersContainer, false);

            // Add (And Configure) LLMCharacter component
            LLMCharacter character = charObj.AddComponent<LLMCharacter>();
            character.llm = sharedLLM;
            character.stream = true;
            character.saveCache = true;
            character.save = characterName;
            character.setNKeepToPrompt = true;
            character.numPredict = -1;
            character.temperature = temperature;
            character.topK = topK;
            character.topP = topP;
            character.repeatPenalty = repeatPenalty;
            character.presencePenalty = presencePenalty;
            character.frequencyPenalty = frequencyPenalty;
            
            // Debug.Log($"Assigning LLM {sharedLLM.name} to {character.GetType().FullName} {characterName}");

            // Serialize the MysteryCharacter object back to JSON for the prompt generator
            string jsonContent = JsonConvert.SerializeObject(mysteryCharacterData, Formatting.Indented); // Use Newtonsoft.Json

            // Generate and set the prompt
            string systemPrompt = CharacterPromptGenerator.GenerateSystemPrompt(jsonContent, character);
            if (string.IsNullOrEmpty(systemPrompt))
            {
                Debug.LogError($"Failed to generate prompt for character: {characterName}");
                Destroy(charObj);
                yield break; // Skip this character if prompt generation fails
            }

            // Store prompt in cache and set it in the character
            promptCache[charObj.name] = systemPrompt;
            character.SetPrompt(systemPrompt, true);
            

            // Store character
            characterCache[characterName] = character;
            stateTransitions[characterName] = new CharacterStateTransition(characterName, CharacterState.Uninitialized, charObj);

            
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating character object {characterName}: {e.Message}\nStack Trace: {e.StackTrace}");
        }

        yield return null; // Yield even if there was an error to avoid blocking
    }


    private IEnumerator InitializeCharacters()
    {
        while (!sharedLLM.started) yield return null;
        yield return new WaitForSeconds(1f);

        if (characterCache.Count == 0)
        {
            Debug.LogError("CharacterManager Phase 2: No characters in cache to initialize!");
            yield break;
        }

        float progressPerCharacter = 1f / characterCache.Count;
        int charactersInitialized = 0;

        foreach (var kvp in characterCache)
        {
            
            yield return StartCoroutine(InitializeSingleCharacter(
                kvp.Key,
                kvp.Value,
                charactersInitialized,
                characterCache.Count
            ));
            

            charactersInitialized++;
        }
        Debug.Log("[CharacterManager Init] All characters initialized, assigning context.");

        // Only set context if we have characters
        if (characterCache.Count > 0)
        {
            int contextPerCharacter = sharedLLM.contextSize / characterCache.Count;
            foreach (var character in characterCache.Values)
            {
                character.nKeep = contextPerCharacter;
            }
        }
    }

    private IEnumerator InitializeSingleCharacter(string characterName, LLMCharacter character, int index, int total)
    {
        var stateTransition = stateTransitions[characterName];
        stateTransition.TryTransition(CharacterState.LoadingTemplate);

        // Load template
        
        yield return StartCoroutine(LoadTemplateWithTimeout(character, characterName));
        
        if (stateTransitions[characterName].CurrentState == CharacterState.Failed)
        {
            Debug.LogWarning($"[CharacterManager InitSingleChar: {characterName}] Failed after LoadTemplate.");
            HandleCharacterFailure(characterName, character.gameObject);
            yield break;
        }

        // Warm up
        
        yield return StartCoroutine(WarmupWithRetries(character, characterName));
        
        if (stateTransitions[characterName].CurrentState == CharacterState.Failed)
        {
            Debug.LogWarning($"[CharacterManager InitSingleChar: {characterName}] Failed after Warmup.");
            HandleCharacterFailure(characterName, character.gameObject);
            yield break;
        }

        stateTransitions[characterName].TryTransition(CharacterState.Ready);
        
    }

    private IEnumerator LoadTemplateWithTimeout(LLMCharacter character, string characterName)
    {
        
        Task templateTask = null;
        try
        {
            templateTask = character.LoadTemplate();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error starting template load for {characterName}: {e.Message}");
            stateTransitions[characterName].TryTransition(CharacterState.Failed);
            yield break;
        }

        float timeoutTime = Time.time + templateTimeout;
        while (!templateTask.IsCompleted && Time.time <= timeoutTime)
        {
            yield return null;
        }

        if (Time.time > timeoutTime || templateTask.IsFaulted)
        {
            yield return new WaitForSeconds(1f); // Add small delay before failing
            string errorMsg = templateTask.IsFaulted ?
                $"Template loading failed: {templateTask.Exception?.GetBaseException()?.Message}" :
                "Template loading timed out";
            Debug.LogError($"{characterName}: {errorMsg}");
            stateTransitions[characterName].TryTransition(CharacterState.Failed);
            yield break;
        }

        stateTransitions[characterName].TryTransition(CharacterState.WarmingUp);
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator WarmupWithRetries(LLMCharacter character, string characterName)
    {
        

        for (int attempt = 1; attempt <= maxWarmupAttempts; attempt++)
        {
            if (attempt > 1)
            {
                float backoffDelay = Mathf.Pow(2, attempt - 1) * baseBackoffDelay;
                
                yield return new WaitForSeconds(backoffDelay);
            }

            Task warmupTask = null;
            try
            {
                warmupTask = character.Warmup();
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[CharacterManager Warmup] Error starting warmup for {characterName} (attempt {attempt}): {e.Message}");
                if (attempt == maxWarmupAttempts)
                {
                    Debug.LogError($"[CharacterManager Warmup] All attempts failed for {characterName}. Transitioning to Failed state.");
                    stateTransitions[characterName].TryTransition(CharacterState.Failed);
                    yield break;
                }
                continue;
            }

            float timeoutTime = Time.time + warmupTimeout;
            bool timedOut = false;
            bool taskCompleted = false;

            while (!taskCompleted && !timedOut)
            {
                taskCompleted = warmupTask.IsCompleted;
                timedOut = Time.time > timeoutTime;
                if (!taskCompleted && !timedOut)
                {
                    // Debug.Log($"[CharacterManager Warmup] Attempt {attempt}: Task not completed yet. Yielding next frame.");
                    yield return null;
                }
            }

            if (!timedOut && !warmupTask.IsFaulted)
            {
                //Debug.LogWarning($"[CharacterManager Warmup] Attempt {attempt}: Timeout reached. Task did not complete in time.");
                stateTransitions[characterName].TryTransition(CharacterState.Ready);
                yield break;
            }

            if (attempt < maxWarmupAttempts)
            {
                Debug.LogWarning($"[CharacterManager Warmup] {characterName} warmup attempt {attempt} failed, retrying...");
            }
            else
            {
                Debug.LogError($"[CharacterManager Warmup] All warmup attempts failed for {characterName}");
                stateTransitions[characterName].TryTransition(CharacterState.Failed);
            }
        }
    }

    private void HandleCharacterFailure(string characterName, GameObject charObj)
    {
        if (charObj != null) Destroy(charObj);
        stateTransitions[characterName].TryTransition(CharacterState.Failed);
    }

    public async Task<LLMCharacter> SwitchToCharacter(string characterName)
    {
        

        if (!isInitialized || !characterCache.ContainsKey(characterName))
            return null;

        if (stateTransitions[characterName].CurrentState != CharacterState.Ready)
            return null;

        if (isSwitchingCharacter)
            return null;

        isSwitchingCharacter = true;

        try
        {
            if (currentCharacter != null &&
                stateTransitions[currentCharacter.name].CurrentState == CharacterState.Ready)
            {
                currentCharacter.CancelRequests();
                await Task.Yield();
            }

            // LogResourceUtilization();
            currentCharacter = characterCache[characterName];
            return currentCharacter;
        }
        finally
        {
            isSwitchingCharacter = false;
        }
    }
    public LLMCharacter GetCurrentCharacter() => currentCharacter;

    public string[] GetAvailableCharacters() => characterCache.Keys.ToArray();

    public bool IsCharacterReady(string characterName)
    {
        return stateTransitions.ContainsKey(characterName) &&
               stateTransitions[characterName].CurrentState == CharacterState.Ready;
    }

    public CharacterState GetCharacterState(string characterName)
    {
        return stateTransitions.ContainsKey(characterName) ?
               stateTransitions[characterName].CurrentState :
               CharacterState.Uninitialized;
    }

    public class CharacterStateSnapshot
    {
        public string CharacterName { get; set; }
        public List<ChatMessage> ChatHistory { get; set; }
        public int CurrentSlot { get; set; }
        public long LastInteractionTime { get; set; }
    }

    private Dictionary<string, CharacterStateSnapshot> characterSnapshots =
        new Dictionary<string, CharacterStateSnapshot>();

    public void SaveCharacterState(string characterName)
    {
        var character = characterCache[characterName];
        characterSnapshots[characterName] = new CharacterStateSnapshot
        {
            CharacterName = characterName,
            ChatHistory = new List<ChatMessage>(character.chat),
            CurrentSlot = character.slot,
            LastInteractionTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }

    private void OnValidate()
    {
        // Validate configuration settings in editor
        characterInitDelay = Mathf.Max(2f, characterInitDelay);
        templateTimeout = Mathf.Max(5f, templateTimeout);
        warmupTimeout = Mathf.Max(10f, warmupTimeout);
        maxWarmupAttempts = Mathf.Max(1, maxWarmupAttempts);
        baseBackoffDelay = Mathf.Max(1f, baseBackoffDelay);
    }

    // OnDisable removed, cleanup moved to OnDestroy

    private void CleanupCharacters()
    {
        // --- DIAGNOSTIC LOGGING START ---
        
        // --- DIAGNOSTIC LOGGING END ---

        // Check if currentCharacter and its LLM are valid and started before cancelling
        
        if (currentCharacter != null && currentCharacter.llm != null && currentCharacter.llm.started)
        {
            
            currentCharacter.CancelRequests();
        }
        else if (currentCharacter != null)
        {
             Debug.LogWarning($"Skipping CancelRequests for current character {currentCharacter.name}; LLM not started or reference missing.");
        }


        // --- DIAGNOSTIC LOGGING START ---
        
        // --- DIAGNOSTIC LOGGING END ---
        foreach (var character in characterCache.Values)
        {
            if (character != null)
            {
                // --- DIAGNOSTIC LOGGING START ---
                
                // --- DIAGNOSTIC LOGGING END ---
                 // Check if character and its LLM are valid and started before cancelling
                if (character.llm != null && character.llm.started)
                {
                    
                    character.CancelRequests();
                }
                else
                {
                    Debug.LogWarning($"Skipping CancelRequests for character {character.name}; LLM not started or reference missing.");
                }
                // Destroy the GameObject regardless of LLM state
                Destroy(character.gameObject);
            }
        }

        characterCache.Clear();
        promptCache.Clear();
        stateTransitions.Clear();
        isInitialized = false;
        isInitializing = false;
        isSwitchingCharacter = false;
        currentCharacter = null;
    }

    public void ResetCharacter(string characterName)
    {

        if (!characterCache.ContainsKey(characterName))
        {
            Debug.LogError($"Cannot reset non-existent character: {characterName}");
            return;
        }
        
        var character = characterCache[characterName];
        if (character != null)
        {
            character.CancelRequests();
            character.ClearChat();
            if (promptCache.ContainsKey(characterName))
            {
                string prompt = promptCache[characterName];
                
                character.SetPrompt(prompt, true);
            }
            else
            {
                Debug.LogError($"No prompt found in cache for character: {characterName}");
            }
        }
    }

    public void LogCharacterState(string characterName)
    {
        if (!characterCache.ContainsKey(characterName))
        {
            Debug.LogError($"Cannot log state for non-existent character: {characterName}");
            return;
        }

        var character = characterCache[characterName];
        if (character == null)
        {
            Debug.LogError($"Character {characterName} is null!");
            return;
        }

        
        
        
        
        
    }

    // FOR LATER USE, DO NOT DELETE
    //private async void LogTokenUsage(string characterName) 
    //{
    //    var character = characterCache[characterName];
    //    if (character != null)
    //    {
    //        try
    //        {
    //            var tokens = await character.Tokenize(character.prompt);
    //            if (tokens != null)
    //            {
    //                Debug.Log($"{characterName} system prompt uses {tokens.Count} tokens of {character.nKeep} allocated tokens");
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            Debug.LogError($"Error analyzing token usage for {characterName}: {e.Message}");
    //            Debug.LogError($"Stack trace: {e.StackTrace}");
    //        }
    //    }
    //}

    //private void MonitorMemoryUsage()
    //{
    //    long totalMemory = System.GC.GetTotalMemory(false);
    //    Debug.Log($"Total managed memory after loading: {totalMemory / 1024 / 1024} MB");
    //}


    private void LogResourceUtilization()
    {
        // foreach (var kvp in characterCache)
        // {
        //     var character = kvp.Value;
        //     Debug.Log($"Character {kvp.Key}: Slot {character.slot}, Context Used: {character.chat.Count}");
        // }
    }

    public string GetCharacterDebugInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine("Character Manager Status:");
        info.AppendLine($"Initialized: {isInitialized}");
        info.AppendLine($"Initializing: {isInitializing}");
        info.AppendLine($"Switching Character: {isSwitchingCharacter}");
        info.AppendLine($"Total Characters: {characterCache.Count}");
        info.AppendLine();

        foreach (var characterName in characterCache.Keys)
        {
            info.AppendLine($"Character: {characterName}");
            info.AppendLine($"- State: {stateTransitions[characterName].CurrentState}");
            info.AppendLine($"- Has Prompt: {promptCache.ContainsKey(characterName)}");
            var character = characterCache[characterName];
            if (character != null)
            {
                info.AppendLine($"- GameObject Active: {character.gameObject.activeInHierarchy}");
                info.AppendLine($"- Has LLM: {character.llm != null}");
            }
            else
            {
                info.AppendLine("- Character component is null");
            }
            info.AppendLine();
        }

        return info.ToString();
    }

    public float GetInitializationProgress()
    {
        int totalCharacters = stateTransitions.Count;
        if (totalCharacters == 0) return 0f;

        int readyCharacters = stateTransitions.Count(x => x.Value.CurrentState == CharacterState.Ready);
        return (float)readyCharacters / totalCharacters;
    }

    public LLMCharacter GetCharacterByName(string characterName)
    {
        if (characterCache.TryGetValue(characterName, out LLMCharacter character))
        {
            
            return character;
        }
        Debug.LogError($"No LLMCharacter found in cache for {characterName}");
        return null;
    }

    public int GetReadyCharacterCount()
    {
        return stateTransitions.Count(x => x.Value.CurrentState == CharacterState.Ready);
    }

    // Removed GetCharacterStartingCar method and CharacterLocationData helper class
}
