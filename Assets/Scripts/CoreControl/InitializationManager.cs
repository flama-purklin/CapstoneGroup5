using LLMUnity;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using System;

/// <summary>
/// Manages the game initialization process in a unified scene approach.
/// Replaces the need for scene transitions between loading and gameplay.
/// </summary>
public class InitializationManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LLM llm;
    [SerializeField] private NPCManager npcManager;
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private GameObject loadingOverlay;
    [SerializeField] private TrainLayoutManager trainLayoutManager; // Added reference
    
    private ParsingControl parsingControl;

    private void Awake()
    {
        // Create or reference core game controllers first
        if (GameControl.GameController == null)
        {
            GameObject controllerObj = GameObject.Find("GameController");
            if (controllerObj != null)
            {
                // Wait for GameController's Awake to execute and set GameControl.GameController
                GameControl controller = controllerObj.GetComponent<GameControl>();
                if (controller != null)
                {
                    
                }
            }
        }

        // Find core components (assuming they exist in the scene now)
        if (!llm) llm = FindFirstObjectByType<LLM>();
        if (!npcManager) npcManager = FindFirstObjectByType<NPCManager>();
        if (!characterManager) characterManager = FindFirstObjectByType<CharacterManager>();
        if (!trainLayoutManager) trainLayoutManager = FindFirstObjectByType<TrainLayoutManager>(); // Find TrainLayoutManager
        parsingControl = FindFirstObjectByType<ParsingControl>(); // Now guaranteed to exist in scene

        // Log errors if any essential components are missing
        if (llm == null) Debug.LogError("LLM component not found in scene!");
        if (npcManager == null) Debug.LogError("NPCManager component not found in scene!");
        if (characterManager == null) Debug.LogError("CharacterManager component not found in scene!");
        if (trainLayoutManager == null) Debug.LogError("TrainLayoutManager component not found in scene!");
        if (parsingControl == null) Debug.LogError("ParsingControl component not found in scene!");

        // Ensure systems are properly organized
        // SetupCoreSystems(); // Commented out as CoreSystemsManager creation is redundant
    }

    private void Start()
    {
        // Make sure the loadingOverlay is set
        if (loadingOverlay == null)
        {
            loadingOverlay = GameObject.Find("LoadingOverlay");
            if (loadingOverlay != null)
            {
                
                
                // Make sure the LoadingOverlay script is enabled
                LoadingOverlay overlayScript = loadingOverlay.GetComponent<LoadingOverlay>();
                if (overlayScript != null && !overlayScript.enabled)
                {
                    overlayScript.enabled = true;
                    
                }
            }
        }
        
        // Set game state to loading
        if (GameControl.GameController != null)
        {
            GameControl.GameController.currentState = GameState.LOADING;
            
        }
        else
        {
            Debug.LogError("GameControl.GameController is still null in Start. Cannot set game state to LOADING.");
        }
        
        // Begin initialization process
        InitializeGame();
    }

    /// <summary>
    /// Ensures core systems are available and properly organized
    /// </summary>
    private void SetupCoreSystems()
    {
        // Create event system if needed
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        
        // // Create core systems manager if needed - REMOVED as it's redundant
        // if (FindFirstObjectByType<CoreSystemsManager>() == null)
        // {
        //     GameObject coreSystemsObj = new GameObject("CoreSystems");
        //     coreSystemsObj.AddComponent<CoreSystemsManager>();
        // }
    }

    /// <summary>
    /// Begins the game initialization process
    /// </summary>
    private async void InitializeGame()
    {
        Debug.Log("Starting game initialization sequence...");
        
        try
        {
            // Step 1: Wait for LLM to start - this might timeout if LLM is not configured
            await WaitForLLMStartup();
            
            // Step 2: Wait for mystery parsing
            await WaitForParsingComplete();

            // --- Explicitly trigger CharacterManager initialization ---
            if (characterManager != null)
            {
                 Debug.Log("Triggering CharacterManager initialization...");
                 characterManager.Initialize();
            }
            else
            {
                 Debug.LogError("CharacterManager is null, cannot start its initialization!");
            }
            // ---------------------------------------------------------
            
            // Step 2.5: Build the train layout using the new manager
            BuildTrain();

            // Step 3: Wait for Character Manager to initialize (which is now triggered explicitly)
            await WaitForCharacterManagerInitialization();

            // Step 3.5: Initialize NPC Manager (Caches LLM data) - Separated from Character Init
            await InitializeNPCManager();

            // Step 3.75: Spawn NPCs into the built train layout
            SpawnAllNPCs(); // Removed await, as SpawnAllNPCs is now void

            // Step 4: Complete initialization and hide loading overlay
            CompleteInitialization();
        }
        catch (System.Exception ex)
        {
            // Handle any unexpected exceptions during initialization
            Debug.LogError($"Critical error during game initialization: {ex.Message}");
            Debug.LogException(ex);
            
            // Still try to complete initialization to avoid being stuck on loading screen
            try
            {
                CompleteInitialization();
            }
            catch (System.Exception finalEx)
            {
                Debug.LogError($"Failed to complete initialization: {finalEx.Message}");
                
                // Emergency fallback - try to force state change directly
                if (GameControl.GameController != null)
                {
                    GameControl.GameController.currentState = GameState.DEFAULT;
                    Debug.Log("Forced game state to DEFAULT due to initialization errors");
                }
            }
        }
    }

    /// <summary>
    /// Step 2.5: Build the train layout
    /// </summary>
    private void BuildTrain()
    {
        Debug.Log("INITIALIZATION STEP 2.5: Building Train Layout...");
        if (trainLayoutManager != null)
        {
            // Ensure mystery data is loaded before building
            if (GameControl.GameController != null && GameControl.GameController.coreMystery != null)
            {
                 trainLayoutManager.BuildTrainLayout();
                 Debug.Log("Train build initiated.");
            }
            else
            {
                Debug.LogError("Cannot build train: GameControl or coreMystery is null.");
            }
        }
        else
        {
            Debug.LogError("TrainLayoutManager reference not found! Cannot build train.");
        }
    }

    /// <summary>
    /// Step 1: Wait for LLM to start with timeout
    /// </summary>
    private async Task WaitForLLMStartup()
    {
        Debug.Log("INITIALIZATION STEP 1: Waiting for LLM to start...");
        int waitCount = 0;
        float startTime = Time.realtimeSinceStartup;
        const float LLM_STARTUP_TIMEOUT = 120f; // 30 seconds timeout (changed from 30f to 120f)
        
        while (llm != null && !llm.started)
        {
            // Check for timeout
            float elapsedTime = Time.realtimeSinceStartup - startTime;
            if (elapsedTime > LLM_STARTUP_TIMEOUT)
            {
                Debug.LogWarning($"LLM startup timed out after {LLM_STARTUP_TIMEOUT} seconds. Proceeding with initialization.");
                break;
            }
            
            waitCount++;
            if (waitCount % 100 == 0) // Log every 100 frames to avoid spam
            {
                Debug.Log($"Still waiting for LLM to start... ({elapsedTime:F1} seconds elapsed)");
            }
            await Task.Yield();
        }
        
        float llmLoadTime = Time.realtimeSinceStartup - startTime;
        
        if (llm != null && llm.started)
        {
            Debug.Log($"LLM started successfully in {llmLoadTime:F1} seconds");
        }
        else
        {
            Debug.LogWarning($"LLM did not start properly after {llmLoadTime:F1} seconds. Game functionality may be limited.");
        }
    }

    /// <summary>
    /// Step 2: Wait for mystery parsing with timeout
    /// </summary>
    private async Task WaitForParsingComplete()
    {
        Debug.Log("INITIALIZATION STEP 2: Mystery parsing"); // Updated log message
        // bool parsingCompleted = false; // No longer needed with flag check
        const float PARSING_TIMEOUT = 60f; // 60 seconds timeout
        
        if (parsingControl != null)
        {
            // Removed event subscription logic
            
            // Start waiting
            float startTime = Time.realtimeSinceStartup;
            int waitCount = 0;
            
            
            
            // Wait for the IsParsingComplete flag or timeout
            while (!parsingControl.IsParsingComplete) // Simplified loop condition
            {
                // Check for timeout
                float elapsedTime = Time.realtimeSinceStartup - startTime;
                if (elapsedTime > PARSING_TIMEOUT)
                {
                    Debug.LogWarning($"Mystery parsing timed out after {PARSING_TIMEOUT} seconds. Proceeding with initialization.");
                    break;
                }
                
                waitCount++;
                if (waitCount % 100 == 0)
                {
                    
                }
                await Task.Yield();
            }
            
            // Removed event unsubscription logic
            
            float parsingTime = Time.realtimeSinceStartup - startTime;
            
            if (parsingControl.IsParsingComplete) // Check flag directly
            {
                Debug.Log($"Mystery parsing complete in {parsingTime:F1} seconds"); // Updated log message
            }
            else
            {
                Debug.LogWarning($"Mystery parsing did not complete normally after {parsingTime:F1} seconds. Proceeding with limited functionality.");
            }
            // Removed call to VerifyCharacterFiles();
        }
        else
        {
            Debug.LogError("ParsingControl not found! Cannot wait for parsing completion."); // Updated log message
        }
    }

    /// <summary>
    /// Step 3: Wait for Character Manager initialization with timeout
    /// </summary>
    private async Task WaitForCharacterManagerInitialization()
    {
        Debug.Log("INITIALIZATION STEP 3: Waiting for Character Manager initialization...");
        if (characterManager == null)
        {
            Debug.LogError("CharacterManager not found! Cannot wait for initialization.");
            return;
        }

        float startTime = Time.realtimeSinceStartup;
        const float CHARACTER_INIT_TIMEOUT = 240f; // Increased timeout
        bool characterInitComplete = false;

        // Define the event handler locally
        void HandleCharacterInitComplete()
        {
            
            characterInitComplete = true;
        }

        // Subscribe to the event
        characterManager.OnInitializationComplete += HandleCharacterInitComplete;

        // Wait for the event or timeout
        while (!characterInitComplete && !characterManager.IsInitialized) // Check both flag and event
        {
            float elapsedTime = Time.realtimeSinceStartup - startTime;
            if (elapsedTime > CHARACTER_INIT_TIMEOUT)
            {
                Debug.LogWarning($"Character Manager initialization timed out after {CHARACTER_INIT_TIMEOUT} seconds. Proceeding...");
                break; // Exit loop on timeout
            }
            await Task.Yield(); // Wait for the next frame
        }

        // Unsubscribe from the event to prevent memory leaks
        characterManager.OnInitializationComplete -= HandleCharacterInitComplete;

        float initTime = Time.realtimeSinceStartup - startTime;
        if (characterInitComplete || characterManager.IsInitialized)
        {
            Debug.Log($"Character Manager initialization confirmed complete in {initTime:F1} seconds.");
        }
        else
        {
             Debug.LogWarning($"Character Manager initialization did not complete normally after {initTime:F1} seconds.");
        }
    }

    /// <summary>
    /// Step 3.5: Initialize NPC Manager (caching data) with timeout
    /// </summary>
    private async Task InitializeNPCManager()
    {
        Debug.Log("INITIALIZATION STEP 3.5: Initializing NPC Manager...");
        if (npcManager == null)
        {
            Debug.LogError("NPCManager not found! Cannot initialize.");
            return;
        }

        float startTime = Time.realtimeSinceStartup;
        const float NPC_INIT_TIMEOUT = 60f; // Timeout for NPC Manager's internal setup

        try
        {
            Task initTask = npcManager.Initialize(); // Assuming NPCManager has an Initialize method
            int waitCount = 0;

            while (!initTask.IsCompleted)
            {
                float elapsedTime = Time.realtimeSinceStartup - startTime;
                if (elapsedTime > NPC_INIT_TIMEOUT)
                {
                    Debug.LogWarning($"NPC Manager initialization timed out after {NPC_INIT_TIMEOUT} seconds.");
                    break;
                }

                waitCount++;
                if (waitCount % 100 == 0) // Log periodically
                {
                     
                }
                await Task.Yield();
            }

             float initTime = Time.realtimeSinceStartup - startTime;
            if (initTask.IsCompleted && !initTask.IsFaulted)
            {
                Debug.Log($"NPC Manager initialization complete in {initTime:F1} seconds.");
            }
            else if (initTask.IsFaulted)
            {
                 Debug.LogError($"Error during NPC Manager initialization: {initTask.Exception?.GetBaseException()?.Message}");
            }
        }
        catch (System.Exception ex)
        {
             Debug.LogError($"Exception during NPC Manager initialization: {ex.Message}");
             Debug.LogException(ex);
        }
    }

    /// <summary>
    /// Step 3.75: Spawn all NPCs into their designated cars
    /// </summary>
    private void SpawnAllNPCs() // Removed async Task, as nothing is awaited inside
    {
        Debug.Log("INITIALIZATION STEP 3.75: Spawning NPCs...");
        // Add check for GameControl and coreMystery
        if (characterManager == null || npcManager == null || trainLayoutManager == null || GameControl.GameController == null || GameControl.GameController.coreMystery == null)
        {
            Debug.LogError("Cannot spawn NPCs: Missing manager references (Character, NPC, TrainLayout) or GameControl/coreMystery data.");
            return;
        }

        // Get characters directly from GameControl now
        var characterData = GameControl.GameController.coreMystery.Characters;
        if (characterData == null || characterData.Count == 0)
        {
            Debug.LogWarning("No character data found in GameControl.coreMystery to spawn NPCs.");
            return;
        }

        Debug.Log($"Attempting to spawn {characterData.Count} NPCs...");

        // Reset anchor tracking before starting the spawn loop for this sequence
        // trainLayoutManager.ResetUsedAnchorTracking();

        int i = 0; // Index for assigning appearance
        foreach (var kvp in characterData)
        {
            string characterName = kvp.Key;
            MysteryCharacter charData = kvp.Value; // Get the character data object

            if (string.IsNullOrEmpty(characterName) || charData == null)
            {
                 Debug.LogWarning($"Skipping invalid character entry (Name: {characterName}, Data: {charData != null})");
                 continue;
            }

            try
            {
                // --- Get Spawn Location Data (Direct Access) ---
                string startCarName = charData.InitialLocation; // Access directly from the object
                if (string.IsNullOrEmpty(startCarName))
                {
                    Debug.LogWarning($"No initial_location specified for character '{characterName}'. Skipping spawn.");
                    continue;
                }

                // Get car transform
                Transform carTransform = trainLayoutManager.GetCarTransform(startCarName);
                if (carTransform == null)
                {
                     Debug.LogWarning($"Could not find car transform for car name '{startCarName}' (Character: '{characterName}'). Skipping spawn.");
                     continue;
                }

                // Get spawn point within the car
                Vector3 spawnPos = trainLayoutManager.GetSpawnPointInCar(startCarName);
                // Optional: Add check if spawnPos is valid

                // --- Spawn the NPC ---
                Debug.Log($"Spawning '{characterName}' in car '{startCarName}' at {spawnPos} (Index: {i})");
                GameObject spawnedNPC = npcManager.SpawnNPCInCar(characterName, spawnPos, carTransform, i);

                if (spawnedNPC == null)
                {
                     Debug.LogError($"Failed to spawn NPC for character '{characterName}'.");
                }

                // Optional: Add a small delay if spawning many NPCs causes performance hitches
                // yield return null; // If needed, change method return type to IEnumerator
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error spawning NPC for character '{characterName}': {ex.Message}");
                Debug.LogException(ex);
            }
            i++; // Increment appearance index
        }
         Debug.Log("Finished NPC spawning loop.");
    }

    /// <summary>
    /// Step 4: Complete initialization and transition to gameplay
    /// </summary>
    private void CompleteInitialization()
    {
        Debug.Log("INITIALIZATION STEP 4: Completing initialization and transitioning to gameplay");
        
        try
        {
            // Try to find the loadingOverlay if it's null
            if (loadingOverlay == null)
            {
                loadingOverlay = GameObject.Find("LoadingOverlay");
                
            }
            
            // Hide the loading overlay
            if (loadingOverlay != null)
            {
                try
                {
                    // Get the animator if there is one for fade-out effect
                    Animator animator = loadingOverlay.GetComponent<Animator>();
                    if (animator != null)
                    {
                        // Ensure animator is enabled
                        animator.enabled = true;
                        
                        // Ensure canvas visibility first
                        Canvas canvas = loadingOverlay.GetComponentInChildren<Canvas>();
                        if (canvas != null)
                        {
                            canvas.enabled = true;
                        }
                        
                        // Trigger fade-out animation
                        animator.SetTrigger("FadeOut");
                        
                        
                        // Set the overlay to deactivate after animation completes
                        // Use a wrapper to handle exceptions in the coroutine
                        StartCoroutine(SafeCoroutine(DeactivateAfterAnimation(loadingOverlay, animator, "FadeOut")));
                    }
                    else
                    {
                        // Try to find canvas for manual fade-out if no animator
                        Canvas canvas = loadingOverlay.GetComponentInChildren<Canvas>();
                        if (canvas != null)
                        {
                            StartCoroutine(SafeCoroutine(ManualFadeOut(canvas)));
                        }
                        else
                        {
                            // No canvas or animator, just deactivate
                            loadingOverlay.SetActive(false);
                            
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    // If anything fails with the animator/canvas, fall back to simple deactivation
                    Debug.LogError($"Error handling LoadingOverlay transition: {ex.Message}");
                    try
                    {
                        loadingOverlay.SetActive(false);
                        
                    }
                    catch
                    {
                        Debug.LogError("Failed to deactivate LoadingOverlay");
                    }
                }
            }
            else
            {
                Debug.LogWarning("No LoadingOverlay found to hide");
            }
            
            // Enable player input
            EnablePlayerInput();
            
            // Change game state to DEFAULT
            if (GameControl.GameController != null)
            {
                GameControl.GameController.currentState = GameState.DEFAULT;
                
            }
            else
            {
                Debug.LogError("GameControl.GameController is null in CompleteInitialization. Cannot set game state to DEFAULT.");
            }
            
            Debug.Log("Initialization sequence complete!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Critical error during CompleteInitialization: {ex.Message}");
            Debug.LogException(ex);
            
            // Emergency state change as a last resort
            try
            {
                if (GameControl.GameController != null)
                {
                    GameControl.GameController.currentState = GameState.DEFAULT;
                    
                }
            }
            catch
            {
                Debug.LogError("Failed to perform emergency state change. Game may be in an invalid state.");
            }
        }
    }
    
    /// <summary>
    /// Wrapper for coroutines to handle exceptions safely
    /// </summary>
    private IEnumerator SafeCoroutine(IEnumerator coroutine)
    {
        bool moveNext = true;
        object current = null;
        
        while (moveNext)
        {
            try
            {
                moveNext = coroutine.MoveNext();
                if (moveNext)
                {
                    current = coroutine.Current;
                }
                else
                {
                    yield break;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Exception in coroutine: {ex.Message}");
                yield break;
            }
            
            // This part is outside the try-catch block
            yield return current;
        }
    }
    
    /// <summary>
    /// Manual fade out for canvases without an animator
    /// </summary>
    private IEnumerator ManualFadeOut(Canvas canvas)
    {
        Debug.Log("Using manual fade-out for LoadingOverlay");
        
        // Find all images and text elements to fade
        Image[] images = canvas.GetComponentsInChildren<Image>();
        TMPro.TMP_Text[] texts = canvas.GetComponentsInChildren<TMPro.TMP_Text>();
        
        // Fade out over 1 second
        float duration = 1.0f;
        float startTime = Time.time;
        
        while (Time.time - startTime < duration)
        {
            float alpha = 1.0f - ((Time.time - startTime) / duration);
            
            // Fade images
            foreach (Image img in images)
            {
                Color color = img.color;
                color.a = alpha;
                img.color = color;
            }
            
            // Fade texts
            foreach (TMPro.TMP_Text text in texts)
            {
                Color color = text.color;
                color.a = alpha;
                text.color = color;
            }
            
            yield return null;
        }
        
        // Fully transparent
        foreach (Image img in images)
        {
            Color color = img.color;
            color.a = 0f;
            img.color = color;
        }
        
        foreach (TMPro.TMP_Text text in texts)
        {
            Color color = text.color;
            color.a = 0f;
            text.color = color;
        }
        
        // Disable the canvas after fade
        canvas.gameObject.SetActive(false);
        Debug.Log("Manual fade-out complete, canvas disabled");
    }
    
    /// <summary>
    /// Enable player input after initialization
    /// </summary>
    private void EnablePlayerInput()
    {
        // Find player controller and enable input
        var playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            // Enable player movement
            playerMovement.enabled = true;
            
        }
        else
        {
            Debug.LogWarning("PlayerMovement component not found - player input may not be enabled");
        }
    }

    /// <summary>
    /// Coroutine to deactivate a GameObject after an animation completes
    /// </summary>
    private IEnumerator DeactivateAfterAnimation(GameObject target, Animator animator, string triggerName)
    {
        // Wait for current state to finish
        yield return new WaitForEndOfFrame();
        
        // Get the current animation state info
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float animationLength = stateInfo.length;
        
        // Wait for animation to complete
        yield return new WaitForSeconds(animationLength);
        
        // Deactivate the target
        target.SetActive(false);
    }

    // VerifyCharacterFiles method was removed.
}
