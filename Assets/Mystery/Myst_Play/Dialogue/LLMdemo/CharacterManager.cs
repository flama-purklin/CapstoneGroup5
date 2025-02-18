using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Collections;
using LLMUnity;
using System;

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

    public bool IsInitialized => isInitialized;
    public bool IsSwitchingCharacter => isSwitchingCharacter;
    public event System.Action OnInitializationComplete;

    [Header("Initialization Settings")]
    public float characterInitDelay = 2f;
    public float templateTimeout = 15f;
    public float warmupTimeout = 30f;
    public int maxWarmupAttempts = 3;
    public float baseBackoffDelay = 1f;


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
        StartCoroutine(TwoPhaseInitialization());
    }

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
        yield return StartCoroutine(CreateCharacterObjects());

        // Phase 2: Initialize LLM and Warm Up Characters
        yield return StartCoroutine(InitializeCharacters());

        isInitialized = true;
        isInitializing = false;

        OnInitializationComplete?.Invoke();
    }

    private IEnumerator CreateCharacterObjects()
    {
        string characterPath = Path.Combine(Application.streamingAssetsPath, charactersFolder);
        if (!Directory.Exists(characterPath))
        {
            Debug.LogError($"Characters folder not found at: {characterPath}");
            yield break;
        }

        var characterFiles = Directory.GetFiles(characterPath, "*.json")
            .Select(path => Path.GetFileNameWithoutExtension(path))
            .ToArray();

        foreach (string characterName in characterFiles)
        {
            yield return StartCoroutine(CreateSingleCharacterObject(characterName, characterPath));
        }

        if (characterCache.Count == 0)
        {
            Debug.LogError("No character objects created!");
            yield break;
        }

        Debug.Log($"Successfully created {characterCache.Count} character objects");
    }

    private IEnumerator CreateSingleCharacterObject(string characterName, string characterPath)
    {
        string jsonPath = Path.Combine(characterPath, $"{characterName}.json");
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
            character.temperature =  0.3f;
            character.topK = 55;
            character.topP = 0.9f;
            character.repeatPenalty = 1.0f;
            character.presencePenalty = 0.0f;
            character.frequencyPenalty = 1.0f;

            // Load and set the prompt
            string jsonContent = File.ReadAllText(jsonPath);
            string systemPrompt = CharacterPromptGenerator.GenerateSystemPrompt(jsonContent);
            if (string.IsNullOrEmpty(systemPrompt))
            {
                Debug.LogError($"Failed to generate prompt for character: {characterName}");
                Destroy(charObj);
                yield break;
            }

            // Store  prompt in cache and set it in the character
            promptCache[charObj.name] = systemPrompt;
            character.SetPrompt(systemPrompt, true);
            Debug.Log($"Set prompt for {charObj.name}: {systemPrompt.Substring(0, Mathf.Min(100, systemPrompt.Length))}...");

            // Store  character
            characterCache[characterName] = character;
            stateTransitions[characterName] = new CharacterStateTransition(characterName, CharacterState.Uninitialized, charObj);

            Debug.Log($"Successfully created character object: {characterName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating character object {characterName}: {e.Message}");
        }

        yield return null;
    }

    private IEnumerator InitializeCharacters()
    {
        while (!sharedLLM.started) yield return null;
        yield return new WaitForSeconds(1f);

        if (characterCache.Count == 0)
        {
            Debug.LogError("No characters in cache to initialize!");
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
            HandleCharacterFailure(characterName, character.gameObject);
            yield break;
        }

        // Warm up
        yield return StartCoroutine(WarmupWithRetries(character, characterName));
        if (stateTransitions[characterName].CurrentState == CharacterState.Failed)
        {
            HandleCharacterFailure(characterName, character.gameObject);
            yield break;
        }

        stateTransitions[characterName].TryTransition(CharacterState.Ready);
        Debug.Log($"Successfully initialized {characterName}");
    }

    private IEnumerator LoadTemplateWithTimeout(LLMCharacter character, string characterName)
    {
        Debug.Log($"Loading template for {characterName}...");
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
        Debug.Log($"Warming up {characterName}...");

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
                Debug.LogError($"Error starting warmup for {characterName} (attempt {attempt}): {e.Message}");
                if (attempt == maxWarmupAttempts)
                {
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
                    yield return null;
                }
            }

            if (!timedOut && !warmupTask.IsFaulted)
            {
                stateTransitions[characterName].TryTransition(CharacterState.Ready);
                yield break;
            }

            if (attempt < maxWarmupAttempts)
            {
                Debug.LogWarning($"{characterName} warmup attempt {attempt} failed, retrying...");
            }
            else
            {
                Debug.LogError($"All warmup attempts failed for {characterName}");
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
        Debug.Log($"Attempting to switch to: {characterName}");
        Debug.Log($"Available characters: {string.Join(", ", characterCache.Keys)}");

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

            LogResourceUtilization();
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

    private void OnDisable()
    {
        CleanupCharacters();
    }

    private void OnDestroy()
    {
        CleanupCharacters();
    }

    private void CleanupCharacters()
    {
        if (currentCharacter != null)
        {
            currentCharacter.CancelRequests();
        }

        foreach (var character in characterCache.Values)
        {
            if (character != null)
            {
                character.CancelRequests();
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
                Debug.Log($"Resetting {characterName} with prompt: {prompt.Substring(0, Mathf.Min(100, prompt.Length))}...");
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

        Debug.Log($"Character State - Name: {characterName}");
        Debug.Log($"- GameObject Active: {character.gameObject.activeInHierarchy}");
        Debug.Log($"- LLM Reference Valid: {character.llm != null}");
        Debug.Log($"- Parent: {character.transform.parent?.name}");
        Debug.Log($"- Current State: {stateTransitions[characterName].CurrentState}");
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
        foreach (var kvp in characterCache)
        {
            var character = kvp.Value;
            Debug.Log($"Character {kvp.Key}: Slot {character.slot}, Context Used: {character.chat.Count}");
        }
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
            Debug.Log($"Found LLMCharacter in cache for {characterName}"); 
            return character;
        }
        Debug.LogError($"No LLMCharacter found in cache for {characterName}");
        return null;
    }

    public int GetReadyCharacterCount()
    {
        return stateTransitions.Count(x => x.Value.CurrentState == CharacterState.Ready);
    }
}