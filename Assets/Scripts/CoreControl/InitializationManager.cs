using LLMUnity;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic; 

/// <summary>
/// Manages the game initialization process in a unified scene approach.
/// Ensures systems initialize in the correct order and links necessary components.
/// </summary>
public class InitializationManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LLM llm;
    [SerializeField] private NPCManager npcManager;
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private GameObject loadingOverlay;
    [SerializeField] private TrainLayoutManager trainLayoutManager; 

    [SerializeField] private float minLoadingTime = 20.0f; // Minimum seconds
    
    private ParsingControl parsingControl;

    private void Awake()
    {
        // Find core components 
        if (!llm) llm = FindFirstObjectByType<LLM>();
        if (!npcManager) npcManager = FindFirstObjectByType<NPCManager>();
        if (!characterManager) characterManager = FindFirstObjectByType<CharacterManager>();
        if (!trainLayoutManager) trainLayoutManager = FindFirstObjectByType<TrainLayoutManager>(); 
        parsingControl = FindFirstObjectByType<ParsingControl>(); 

        // Log errors if any essential components are missing
        if (llm == null) Debug.LogError("InitializationManager: LLM component not found!");
        if (npcManager == null) Debug.LogError("InitializationManager: NPCManager component not found!");
        if (characterManager == null) Debug.LogError("InitializationManager: CharacterManager component not found!");
        if (trainLayoutManager == null) Debug.LogError("InitializationManager: TrainLayoutManager component not found!");
        if (parsingControl == null) Debug.LogError("InitializationManager: ParsingControl component not found!");
    }

    private void Start()
    {
        if (loadingOverlay == null) loadingOverlay = GameObject.Find("LoadingOverlay");
        if (GameControl.GameController != null) GameControl.GameController.currentState = GameState.LOADING;
        else Debug.LogError("GameControl.GameController is null in Start!");
        
        InitializeGame();
    }

    /// <summary>
    /// Main initialization sequence. Order adjusted for dependencies.
    /// </summary>
    private async void InitializeGame()
    {
        Debug.Log("Starting game initialization sequence...");
        float initializationStartTime = Time.realtimeSinceStartup; // Record start time
        
        try
        {
            // Step 1: Wait for LLM to start
            Debug.Log("--- INIT STEP 1: Wait LLM ---");
            await WaitForLLMStartup();
            
            // Step 2: Wait for mystery parsing
            Debug.Log("--- INIT STEP 2: Wait Parsing ---");
            await WaitForParsingComplete();

            // Step 3: Initialize Character Manager (Creates LLMCharacter instances)
            Debug.Log("--- INIT STEP 3: Initialize CharacterManager ---");
            if (characterManager != null) {
                 characterManager.Initialize(); // Starts TwoPhaseInitialization
                 await WaitForCharacterManagerInitialization(); // Wait for it
            } else { Debug.LogError("CharacterManager is null, cannot initialize!"); }

            // Step 4: Build the train layout
            Debug.Log("--- INIT STEP 4: Build Train ---");
            BuildTrain(); 

            // Step 5: Spawn NPCs into the layout AND Link Character Components
            Debug.Log("--- INIT STEP 5: Spawn NPCs & Link Characters ---");
            SpawnAndLinkNPCs(); // Combined spawning and linking
            if (npcManager != null) { npcManager.SpawningComplete = true; Debug.Log($"NPC Spawning Complete. Flag set on NPCManager."); } 
            else { Debug.LogError("NPCManager is null, cannot set SpawningComplete flag!"); }

            // --- Minimum Loading Time Check ---
            float elapsedTime = Time.realtimeSinceStartup - initializationStartTime;
            if (elapsedTime < minLoadingTime)
            {
                float waitTime = minLoadingTime - elapsedTime;
                Debug.Log($"Initialization finished in {elapsedTime:F1}s. Waiting an additional {waitTime:F1}s for minimum loading screen display time (game will not pause).");
                
                // Wait for the remaining time without pausing the game
                await Task.Delay(TimeSpan.FromSeconds(waitTime)); 
                
                Debug.Log("Minimum loading display time reached.");
            }
            // --- End Minimum Loading Time Check ---

            // Step 6: Complete initialization and hide loading overlay
            Debug.Log("--- INIT STEP 6: Complete Initialization ---");
            CompleteInitialization(); // Now call CompleteInitialization after potential delay
        }
        catch (Exception ex) {
            // Ensure timescale is reset if an error occurs during the try block
            if (Time.timeScale == 0f) Time.timeScale = 1f; 
            Debug.LogError($"Critical error during game initialization: {ex.Message}");
            Debug.LogException(ex);
            try { CompleteInitialization(); } 
            catch (Exception finalEx) { Debug.LogError($"Failed to complete initialization after error: {finalEx.Message}"); }
        }
    }

    /// <summary> Step 3: Build the train layout </summary>
    private void BuildTrain() { 
        if (trainLayoutManager != null) {
            if (GameControl.GameController != null && GameControl.GameController.coreMystery != null) {
                 trainLayoutManager.BuildTrainLayout();
                 Debug.Log("Train build initiated by TrainLayoutManager.");
            } else { Debug.LogError("Cannot build train: GameControl or coreMystery is null."); }
        } else { Debug.LogError("TrainLayoutManager reference not found! Cannot build train."); }
     }

    /// <summary> Step 1: Wait for LLM to start with timeout </summary>
    private async Task WaitForLLMStartup() { 
        float startTime = Time.realtimeSinceStartup;
        const float LLM_STARTUP_TIMEOUT = 120f; 
        while (llm != null && !llm.started) {
            if (Time.realtimeSinceStartup - startTime > LLM_STARTUP_TIMEOUT) { Debug.LogWarning($"LLM startup timed out after {LLM_STARTUP_TIMEOUT} seconds."); break; }
            await Task.Yield();
        }
        float llmLoadTime = Time.realtimeSinceStartup - startTime;
        if (llm != null && llm.started) { Debug.Log($"LLM started successfully in {llmLoadTime:F1} seconds"); }
        else { Debug.LogWarning($"LLM did not start properly after {llmLoadTime:F1} seconds."); }
     }

    /// <summary> Step 2: Wait for mystery parsing with timeout </summary>
    private async Task WaitForParsingComplete() { 
        const float PARSING_TIMEOUT = 60f; 
        if (parsingControl != null) {
            float startTime = Time.realtimeSinceStartup;
            while (!parsingControl.IsParsingComplete) {
                if (Time.realtimeSinceStartup - startTime > PARSING_TIMEOUT) { Debug.LogWarning($"Mystery parsing timed out after {PARSING_TIMEOUT} seconds."); break; }
                await Task.Yield();
            }
            float parsingTime = Time.realtimeSinceStartup - startTime;
            if (parsingControl.IsParsingComplete) { Debug.Log($"Mystery parsing complete in {parsingTime:F1} seconds"); }
            else { Debug.LogWarning($"Mystery parsing did not complete normally after {parsingTime:F1} seconds."); }
        } else { Debug.LogError("ParsingControl not found! Cannot wait for parsing completion."); }
     }

    /// <summary> Step 3 (Wait): Wait for Character Manager initialization </summary> 
    private async Task WaitForCharacterManagerInitialization() { 
        if (characterManager == null) { Debug.LogError("CharacterManager not found!"); return; }
        float startTime = Time.realtimeSinceStartup;
        const float CHARACTER_INIT_TIMEOUT = 240f; 
        while (!characterManager.IsInitialized) { 
            if (Time.realtimeSinceStartup - startTime > CHARACTER_INIT_TIMEOUT) { Debug.LogWarning($"Character Manager initialization timed out after {CHARACTER_INIT_TIMEOUT} seconds."); break; }
            await Task.Yield(); 
        }
        float initTime = Time.realtimeSinceStartup - startTime;
        if (characterManager.IsInitialized) { Debug.Log($"Character Manager initialization confirmed complete in {initTime:F1} seconds."); }
        else { Debug.LogWarning($"Character Manager initialization did not complete normally after {initTime:F1} seconds."); }
     }

    /// <summary> Step 5: Spawn all NPCs and link their Character component to the correct LLMCharacter </summary> 
    private void SpawnAndLinkNPCs() 
    {
        if (characterManager == null || npcManager == null || trainLayoutManager == null || GameControl.GameController == null || GameControl.GameController.coreMystery == null) {
            Debug.LogError("Cannot spawn/link NPCs: Missing manager references or GameControl/coreMystery data.");
            if (npcManager != null) npcManager.SpawningComplete = true; 
            return;
        }
        var characterData = GameControl.GameController.coreMystery.Characters;
        if (characterData == null || characterData.Count == 0) {
            Debug.LogWarning("No character data found to spawn NPCs.");
            if (npcManager != null) npcManager.SpawningComplete = true; 
            return;
        }
        Debug.Log($"Attempting to spawn and link {characterData.Count} NPCs...");
        int i = 0;
        int spawnedCount = 0; 
        foreach (var kvp in characterData) {
            string characterName = kvp.Key;
            MysteryCharacter charData = kvp.Value;
            if (string.IsNullOrEmpty(characterName) || charData == null) { Debug.LogWarning($"Skipping invalid character entry for {characterName}"); continue; } 

            // --- Victim Check ---
            string role = charData?.Core?.Involvement?.Role;
            if (!string.IsNullOrEmpty(role) && role.Equals("victim", StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"InitializationManager: Skipping NPC spawn for '{characterName}' because their role is 'victim'.");
                continue; // Skip spawning this NPC
            }
            // --- End Victim Check ---

            try {
                // Ensure LLMCharacter was actually created (CharacterManager might have skipped victim)
                LLMCharacter llmCharacterRefCheck = characterManager.GetCharacterByName(characterName);
                if (llmCharacterRefCheck == null) {
                    Debug.LogWarning($"InitializationManager: Skipping NPC spawn for '{characterName}' as no corresponding LLMCharacter was found (likely skipped by CharacterManager).");
                    continue;
                }

                string startCarName = charData.InitialLocation; 
                if (string.IsNullOrEmpty(startCarName)) { Debug.LogWarning($"No initial_location found for '{characterName}'. Skipping NPC spawn."); continue; }
                Transform carTransform = trainLayoutManager.GetCarTransform(startCarName);
                if (carTransform == null) { Debug.LogWarning($"Could not find car transform '{startCarName}' for '{characterName}'. Skipping."); continue; }
                Vector3 spawnPos = trainLayoutManager.GetSpawnPointInCar(startCarName);
                GameObject spawnedNPC = npcManager.SpawnNPCInCar(characterName, spawnPos, carTransform, i);
                if (spawnedNPC != null) {
                    // Adds npc reference to TrainManager
                    if (trainLayoutManager != null)
                    {
                        foreach (TrainManager.TrainCar car in trainLayoutManager.trainManager.trainCarList)
                        {
                            if (car.trainCar.name == trainLayoutManager.GetCarReference(startCarName).name)
                            {
                                // Add npc object reference to that TrainCar structure.
                                car.npcsInCar.Add(spawnedNPC);
                                break;
                            }
                        }
                    }

                    Character characterComponent = spawnedNPC.GetComponent<Character>();
                    LLMCharacter llmCharacterRef = characterManager.GetCharacterByName(characterName);
                    if (characterComponent != null && llmCharacterRef != null) {
                        characterComponent.Initialize(characterName, llmCharacterRef); 
                    } else {
                        if (characterComponent == null) Debug.LogError($"Failed to get Character component on spawned NPC {characterName}.");
                        if (llmCharacterRef == null) Debug.LogError($"Failed to get LLMCharacter reference from CharacterManager for {characterName}."); // Should not happen due to check above, but keep for safety
                    }
                    spawnedCount++; // Increment count only if spawn was successful
                } else { Debug.LogError($"Failed to spawn NPC for character '{characterName}'."); }
            } catch (Exception ex) { Debug.LogError($"Error spawning/linking NPC for character '{characterName}': {ex.Message}"); Debug.LogException(ex); }
            i++; // Increment index regardless of spawn success for animation container assignment consistency
        }
         Debug.Log($"Finished NPC spawning and linking loop. Spawned {spawnedCount} non-victim NPCs.");
         if (npcManager != null) npcManager.SpawningComplete = true; 
         else Debug.LogError("npcManager became null before SpawningComplete could be set!");
    }

    /// <summary> Step 6: Complete initialization and transition to gameplay </summary> 
    private void CompleteInitialization() { 
        Debug.Log("INITIALIZATION STEP 6: Completing initialization and transitioning to gameplay"); 
        try {
            if (loadingOverlay == null) loadingOverlay = GameObject.Find("LoadingOverlay");
            if (loadingOverlay != null) {
                try {
                    Animator animator = loadingOverlay.GetComponent<Animator>();
                    if (animator != null) {
                        animator.enabled = true;
                        Canvas canvas = loadingOverlay.GetComponentInChildren<Canvas>();
                        if (canvas != null) canvas.enabled = true;
                        animator.SetTrigger("FadeOut");
                        StartCoroutine(SafeCoroutine(DeactivateAfterAnimation(loadingOverlay, animator, "FadeOut")));
                    } else {
                        Canvas canvas = loadingOverlay.GetComponentInChildren<Canvas>();
                        if (canvas != null) StartCoroutine(SafeCoroutine(ManualFadeOut(canvas)));
                        else loadingOverlay.SetActive(false);
                    }
                } catch (Exception ex) {
                    Debug.LogError($"Error handling LoadingOverlay transition: {ex.Message}");
                    try { loadingOverlay.SetActive(false); } catch { Debug.LogError("Failed to deactivate LoadingOverlay"); }
                }
            } else { Debug.LogWarning("No LoadingOverlay found to hide"); }
            EnablePlayerInput();
            if (GameControl.GameController != null) GameControl.GameController.StartGame();
            else Debug.LogError("GameControl.GameController is null in CompleteInitialization.");
            Debug.Log("Initialization sequence complete!");
        } catch (Exception ex) {
            Debug.LogError($"Critical error during CompleteInitialization: {ex.Message}");
            Debug.LogException(ex);
            try { if (GameControl.GameController != null) GameControl.GameController.currentState = GameState.DEFAULT; }
            catch { Debug.LogError("Failed emergency state change."); }
        }
     }
    
    private IEnumerator SafeCoroutine(IEnumerator coroutine) { 
        bool moveNext = true; object current = null; 
        while (moveNext) { 
            try { moveNext = coroutine.MoveNext(); if (moveNext) current = coroutine.Current; else yield break; } 
            catch (Exception ex) { Debug.LogError($"Exception in coroutine: {ex.Message}"); yield break; } 
            yield return current; 
        } 
    }

    private IEnumerator ManualFadeOut(Canvas canvas) { 
        Debug.Log("Using manual fade-out for LoadingOverlay"); 
        Image[] images = canvas.GetComponentsInChildren<Image>(); 
        TMP_Text[] texts = canvas.GetComponentsInChildren<TMP_Text>(); 
        float duration = 1.0f; float startTime = Time.time; 
        while (Time.time - startTime < duration) { 
            float alpha = 1.0f - ((Time.time - startTime) / duration); 
            foreach (Image img in images) { Color color = img.color; color.a = alpha; img.color = color; } 
            foreach (TMP_Text text in texts) { Color color = text.color; color.a = alpha; text.color = color; } 
            yield return null; 
        } 
        foreach (Image img in images) { Color color = img.color; color.a = 0f; img.color = color; } 
        foreach (TMP_Text text in texts) { Color color = text.color; color.a = 0f; text.color = color; } 
        canvas.gameObject.SetActive(false); 
        Debug.Log("Manual fade-out complete, canvas disabled"); 
    }

    private void EnablePlayerInput() { 
        var playerMovement = FindFirstObjectByType<PlayerMovement>(); 
        if (playerMovement != null) playerMovement.enabled = true; 
        else Debug.LogWarning("PlayerMovement component not found."); 
    }

    private IEnumerator DeactivateAfterAnimation(GameObject target, Animator animator, string triggerName) { 
        yield return new WaitForEndOfFrame(); 
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0); 
        float animationLength = stateInfo.length; 
        yield return new WaitForSeconds(animationLength); 
        target.SetActive(false); 
    }
}
