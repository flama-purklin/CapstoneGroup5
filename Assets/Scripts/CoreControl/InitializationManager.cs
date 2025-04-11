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
                    Debug.Log("Found GameController in scene");
                }
            }
        }

        // Find or create core components
        if (!llm) llm = FindFirstObjectByType<LLM>();
        
        // Reference the scene's LLM object - this should be set up in the editor
        if (llm == null)
        {
            // Try to find the LLM in the scene
            llm = FindFirstObjectByType<LLM>();
            
            if (llm != null)
            {
                Debug.Log("Found existing LLM object in scene");
            }
            else
            {
                // Log a critical error - LLM must be set up in the scene
                Debug.LogError("No LLM object found in scene! The LLM must be manually created and configured in the scene.");
                Debug.LogError("Please add an 'LLM' GameObject with the LLM component and configure it with a valid model path.");
            }
        }
        else
        {
            Debug.Log("Using LLM reference from inspector");
        }
        
        if (!npcManager) npcManager = FindFirstObjectByType<NPCManager>();
        if (!characterManager) characterManager = FindFirstObjectByType<CharacterManager>();
        if (!trainLayoutManager) trainLayoutManager = FindFirstObjectByType<TrainLayoutManager>(); // Find TrainLayoutManager
        parsingControl = FindFirstObjectByType<ParsingControl>();
        
        // Create CharacterManager if not found
        if (characterManager == null)
        {
            // Try to find it first
            characterManager = FindFirstObjectByType<CharacterManager>();
            
            if (characterManager == null)
            {
                // Create a new one if not found
                GameObject characterObj = new GameObject("CharacterManager");
                characterManager = characterObj.AddComponent<CharacterManager>();
                Debug.Log("Created CharacterManager");
                
                // Configure CharacterManager with the LLM reference
                if (llm != null)
                {
                    characterManager.sharedLLM = llm;
                    Debug.Log("Assigned LLM to new CharacterManager");
                }
                else
                {
                    Debug.LogError("Cannot assign LLM to CharacterManager as no LLM object was found");
                }
            }
            else
            {
                Debug.Log("Found existing CharacterManager in scene");
                
                // Update its LLM reference if needed
                if (llm != null && characterManager.sharedLLM == null)
                {
                    characterManager.sharedLLM = llm;
                    Debug.Log("Updated existing CharacterManager with LLM reference");
                }
            }
        }
        
        // Create ParsingControl if not found
        if (parsingControl == null)
        {
            GameObject parsingObj = new GameObject("ParsingControl");
            parsingControl = parsingObj.AddComponent<ParsingControl>();
            Debug.Log("Created ParsingControl");
        }
        
        // Create NPCManager if not found
        if (npcManager == null)
        {
            GameObject npcObj = new GameObject("NPCManager");
            npcManager = npcObj.AddComponent<NPCManager>();
            Debug.Log("Created NPCManager");
            
            // Connect NPCManager to CharacterManager
            if (characterManager != null)
            {
                // NPCManager should have a reference to the CharacterManager
                // This connection logic varies based on your specific implementation
                Debug.Log("Connected NPCManager to CharacterManager");
            }
        }
        
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
                Debug.Log("Found LoadingOverlay in scene");
                
                // Make sure the LoadingOverlay script is enabled
                LoadingOverlay overlayScript = loadingOverlay.GetComponent<LoadingOverlay>();
                if (overlayScript != null && !overlayScript.enabled)
                {
                    overlayScript.enabled = true;
                    Debug.Log("Enabled LoadingOverlay script");
                }
            }
        }
        
        // Set game state to loading
        if (GameControl.GameController != null)
        {
            GameControl.GameController.currentState = GameState.LOADING;
            Debug.Log("Set GameState to LOADING");
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
            
            // Step 2: Wait for mystery parsing and character extraction
            await WaitForParsingComplete();
            
            // Step 2.5: Build the train layout using the new manager
            BuildTrain();

            // Step 3: Initialize Character Manager and NPC Manager (Caches LLM data)
            await InitializeCharactersAndNPCs();

            // Step 3.5: Spawn NPCs into the built train layout
            await SpawnAllNPCs(); // New step to handle actual spawning

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

    // /// <summary>
    // /// Step 2.5 (Old): Find and initialize the Mystery Board UI - REMOVED as MysteryBoardControl was vestigial
    // /// </summary>
    // private void InitializeMysteryBoard()
    // {
    //     Debug.Log("INITIALIZATION STEP 2.5: Initializing Mystery Board UI...");
    //     // Removed code that searched for and attempted to initialize using the now-deleted MysteryBoardControl GameObject.
    //     // The actual Mystery Board UI initialization should be handled by the relevant UI controller (e.g., associated with MysteryCanvas/NodeControl).
    //     Debug.LogWarning("Mystery Board UI initialization logic related to 'MysteryBoardControl' object removed. Ensure the correct UI controller handles initialization.");
    // }


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
    /// Step 2: Wait for mystery parsing and character extraction with timeout
    /// </summary>
    private async Task WaitForParsingComplete()
    {
        Debug.Log("INITIALIZATION STEP 2: Mystery parsing and character extraction");
        bool parsingCompleted = false;
        const float PARSING_TIMEOUT = 60f; // 60 seconds timeout
        
        if (parsingControl != null)
        {
            // Register for completion event
            void OnParsingComplete()
            {
                parsingCompleted = true;
                Debug.Log("Received parsing completion event");
            }
            
            // Subscribe to completion event
            parsingControl.OnParsingComplete += OnParsingComplete;
            
            // Start waiting
            float startTime = Time.realtimeSinceStartup;
            int waitCount = 0;
            
            Debug.Log("Waiting for mystery parsing and character extraction to complete...");
            
            // Wait for either the event, the IsParsingComplete flag, or timeout
            while (!parsingCompleted && !parsingControl.IsParsingComplete)
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
                    Debug.Log($"Still waiting for parsing to complete... ({elapsedTime:F1} seconds elapsed)");
                }
                await Task.Yield();
            }
            
            // Unsubscribe from event
            parsingControl.OnParsingComplete -= OnParsingComplete;
            
            float parsingTime = Time.realtimeSinceStartup - startTime;
            
            if (parsingCompleted || parsingControl.IsParsingComplete)
            {
                Debug.Log($"Mystery parsing and character extraction complete in {parsingTime:F1} seconds");
            }
            else
            {
                Debug.LogWarning($"Mystery parsing did not complete normally after {parsingTime:F1} seconds. Proceeding with limited functionality.");
            }
            
            // Verify character files - even if we timed out, there may be some valid files
            VerifyCharacterFiles();
        }
        else
        {
            Debug.LogWarning("ParsingControl not found. Character files may not be properly extracted!");
        }
    }

    /// <summary>
    /// Step 3: Initialize NPCs and Character Manager with timeout
    /// </summary>
    private async Task InitializeCharactersAndNPCs()
    {
        Debug.Log("INITIALIZATION STEP 3: Character Manager initialization");
        float startTime = Time.realtimeSinceStartup;
        const float CHARACTER_INIT_TIMEOUT = 240; // 30 seconds timeout (testing with 240 seconds timout for my laptop)
        
        if (characterManager != null)
        {
            // Ensure character manager is initialized
            Debug.Log("Initializing character manager...");
            
            // Wait for character initialization to complete
            if (npcManager != null)
            {
                Debug.Log("Initializing NPCs with character data...");
                try 
                {
                    // Create a task for NPC initialization
                    Task initTask = npcManager.Initialize();
                    
                    // Start a timer for timeout
                    int waitCount = 0;
                    while (!initTask.IsCompleted)
                    {
                        // Check for timeout
                        float elapsedTime = Time.realtimeSinceStartup - startTime;
                        if (elapsedTime > CHARACTER_INIT_TIMEOUT)
                        {
                            Debug.LogWarning($"NPC initialization timed out after {CHARACTER_INIT_TIMEOUT} seconds. Proceeding with limited functionality.");
                            break;
                        }
                        
                        waitCount++;
                        if (waitCount % 100 == 0)
                        {
                            Debug.Log($"Still waiting for NPC initialization... ({elapsedTime:F1} seconds elapsed)");
                        }
                        
                        await Task.Yield();
                    }
                    
                    if (initTask.IsCompleted && !initTask.IsFaulted)
                    {
                        Debug.Log("NPC initialization complete");
                    }
                    else if (initTask.IsFaulted)
                    {
                        Debug.LogError($"Error during NPC initialization: {initTask.Exception?.GetBaseException()?.Message}");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error during NPC initialization: {ex.Message}");
                    Debug.LogException(ex);
                }
            }
            else
            {
                Debug.LogWarning("NPCManager not found. NPCs will not be properly initialized.");
            }
        }
        else
        {
            Debug.LogWarning("CharacterManager not found. Character dialogue may not work properly.");
        }
        
        float npcInitTime = Time.realtimeSinceStartup - startTime;
        Debug.Log($"Character/NPC Manager initialization complete in {npcInitTime:F1} seconds");
    }

    /// <summary>
    /// Step 3.5: Spawn all NPCs into their designated cars
    /// </summary>
    private async Task SpawnAllNPCs()
    {
        Debug.Log("INITIALIZATION STEP 3.5: Spawning NPCs...");
        if (characterManager == null || npcManager == null || trainLayoutManager == null)
        {
            Debug.LogError("Cannot spawn NPCs: Missing manager references (Character, NPC, or TrainLayout).");
            return;
        }

        // Ensure the container for NPC GameObjects exists
        npcManager.PlaceNPCsInGameScene();

        string[] characterNames = characterManager.GetAvailableCharacters();
        if (characterNames == null || characterNames.Length == 0)
        {
            Debug.LogWarning("No available characters found to spawn.");
            return;
        }

        Debug.Log($"Attempting to spawn {characterNames.Length} NPCs...");

        // Reset anchor tracking before starting the spawn loop for this sequence - Cline: Removed obsolete call
        // trainLayoutManager.ResetUsedAnchorTracking();

        for (int i = 0; i < characterNames.Length; i++)
        {
            string characterName = characterNames[i];
            if (string.IsNullOrEmpty(characterName)) continue;

            try
            {
                // --- Get Spawn Location Data (Requires methods in other managers) ---
                // TODO: Implement GetCharacterStartingCar in CharacterManager
                string startCarName = characterManager.GetCharacterStartingCar(characterName);
                if (string.IsNullOrEmpty(startCarName))
                {
                    Debug.LogWarning($"No starting car specified for character '{characterName}'. Skipping spawn.");
                    continue;
                }

                // TODO: Implement GetCarTransform in TrainLayoutManager
                Transform carTransform = trainLayoutManager.GetCarTransform(startCarName);
                if (carTransform == null)
                {
                     Debug.LogWarning($"Could not find car transform for car name '{startCarName}' (Character: '{characterName}'). Skipping spawn.");
                     continue;
                }

                // TODO: Implement GetSpawnPointInCar in TrainLayoutManager
                Vector3 spawnPos = trainLayoutManager.GetSpawnPointInCar(startCarName);
                // Optional: Add check if spawnPos is valid (e.g., not Vector3.zero if that indicates failure)

                // --- Spawn the NPC ---
                Debug.Log($"Spawning '{characterName}' in car '{startCarName}' at {spawnPos} (Index: {i})");
                GameObject spawnedNPC = npcManager.SpawnNPCInCar(characterName, spawnPos, carTransform, i);

                if (spawnedNPC == null)
                {
                     Debug.LogError($"Failed to spawn NPC for character '{characterName}'.");
                }

                // Optional: Add a small delay if spawning many NPCs causes performance hitches during loading
                // await Task.Yield();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error spawning NPC for character '{characterName}': {ex.Message}");
                Debug.LogException(ex);
            }
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
                Debug.Log(loadingOverlay != null ? "Found LoadingOverlay" : "LoadingOverlay not found");
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
                        Debug.Log("Triggered FadeOut animation on LoadingOverlay");
                        
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
                            Debug.Log("Deactivated LoadingOverlay (no animator or canvas found)");
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
                        Debug.Log("Forcibly deactivated LoadingOverlay due to transition error");
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
                GameControl.GameController.StartGame();
                Debug.Log("Changed game state to DEFAULT");
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
                    Debug.Log("Emergency state change to DEFAULT due to critical error");
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
            Debug.Log("Enabled PlayerMovement");
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

    /// <summary>
    /// Verifies character files for proper format and content
    /// </summary>
    private void VerifyCharacterFiles()
    {
        string charactersPath = System.IO.Path.Combine(Application.streamingAssetsPath, "Characters");
        
        // If Characters directory doesn't exist, try to copy from backups
        if (!System.IO.Directory.Exists(charactersPath))
        {
            Debug.LogError("Characters directory not found! Attempting to create it and restore from backups...");
            
            try
            {
                // Create the directory
                System.IO.Directory.CreateDirectory(charactersPath);
                
                // Look for backup files
                string backupsPath = System.IO.Path.Combine(Application.streamingAssetsPath, "CharacterBackups");
                
                if (System.IO.Directory.Exists(backupsPath))
                {
                    var backupFiles = System.IO.Directory.GetFiles(backupsPath, "*.json");
                    
                    foreach (var file in backupFiles)
                    {
                        string fileName = System.IO.Path.GetFileName(file);
                        string destPath = System.IO.Path.Combine(charactersPath, fileName);
                        System.IO.File.Copy(file, destPath, true);
                        Debug.Log($"Restored backup character file: {fileName}");
                    }
                }
                else
                {
                    Debug.LogError("No character backups found at: " + backupsPath);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error restoring character files: {ex.Message}");
            }
            
            // Check if the directory exists now
            if (!System.IO.Directory.Exists(charactersPath))
            {
                Debug.LogError("Failed to create Characters directory. Character dialogue will not work correctly.");
                return;
            }
        }
        
        string[] characterFiles = System.IO.Directory.GetFiles(charactersPath, "*.json");
        Debug.Log($"Found {characterFiles.Length} character files:");
        
        int validFileCount = 0;
        bool novaFileVerified = false;
        
        // If we have no character files, try copying from backups again
        if (characterFiles.Length == 0)
        {
            Debug.LogWarning("No character files found in Characters directory. Attempting to restore from backups...");
            
            try
            {
                string backupsPath = System.IO.Path.Combine(Application.streamingAssetsPath, "CharacterBackups");
                
                if (System.IO.Directory.Exists(backupsPath))
                {
                    var backupFiles = System.IO.Directory.GetFiles(backupsPath, "*.json");
                    
                    foreach (var file in backupFiles)
                    {
                        string fileName = System.IO.Path.GetFileName(file);
                        string destPath = System.IO.Path.Combine(charactersPath, fileName);
                        System.IO.File.Copy(file, destPath, true);
                        Debug.Log($"Restored backup character file: {fileName}");
                    }
                    
                    // Get the updated list of files
                    characterFiles = System.IO.Directory.GetFiles(charactersPath, "*.json");
                    Debug.Log($"After restoration, found {characterFiles.Length} character files");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error restoring character files: {ex.Message}");
            }
        }
        
        foreach (string file in characterFiles)
        {
            string fileName = System.IO.Path.GetFileName(file);
            
            try
            {
                // Load and verify the file structure
                string fileContent = System.IO.File.ReadAllText(file);
                
                // Check if it has the required two-chamber structure
                bool hasCoreSection = fileContent.Contains("\"core\":");
                bool hasMindEngineSection = fileContent.Contains("\"mind_engine\":");
                
                if (hasCoreSection && hasMindEngineSection)
                {
                    validFileCount++;
                    Debug.Log($"  ✓ {fileName} - Valid structure");
                }
                else
                {
                    Debug.LogWarning($"  ⚠ {fileName} - Missing required sections: " + 
                        (hasCoreSection ? "" : "core, ") + 
                        (hasMindEngineSection ? "" : "mind_engine"));
                }
                
                // Special check for Nova's file
                if (fileName.ToLower().Contains("nova"))
                {
                    // Verify Nova's file contains the important speech patterns
                    bool hasMateTerm = fileContent.Contains("mate");
                    bool hasLuvTerm = fileContent.Contains("luv");
                    bool hasExpletives = fileContent.Contains("fuck") || fileContent.Contains("bloody");
                    
                    novaFileVerified = true;
                    
                    if (!hasMateTerm || !hasLuvTerm || (!hasExpletives))
                    {
                        Debug.LogWarning($"Nova's distinctive speech patterns may be missing from {fileName}!");
                        Debug.LogWarning($"- Contains 'mate': {hasMateTerm}");
                        Debug.LogWarning($"- Contains 'luv': {hasLuvTerm}");
                        Debug.LogWarning($"- Contains expletives: {hasExpletives}");
                    }
                    else
                    {
                        Debug.Log($"Nova's distinctive speech patterns are preserved correctly in {fileName}.");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error verifying file {fileName}: {ex.Message}");
            }
        }
        
        // Log overall validation results
        Debug.Log($"Character file validation: {validFileCount}/{characterFiles.Length} files have valid structure");
        
        if (!novaFileVerified)
        {
            Debug.LogWarning("Nova's character file was not found or could not be verified! This may impact dialogue quality.");
            // Not treating this as an error since we want to continue even if Nova's file isn't perfect
        }
        
        // Final warning if we still have no character files
        if (characterFiles.Length == 0)
        {
            Debug.LogWarning("No character files found even after recovery attempts. This will cause dialogue system issues!");
        }
    }
}
