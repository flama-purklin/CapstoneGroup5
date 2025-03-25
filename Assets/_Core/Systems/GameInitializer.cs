using LLMUnity;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Collections;

/// <summary>
/// Initializes the game systems and coordinates startup sequence.
/// The maestro of our digital orchestra, trying to get everyone to play in tune.
/// Good luck with that.
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private LLM llm;
    [SerializeField] private NPCManager npcManager;
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private WorldCoordinator worldCoordinator;
    
    [Header("UI References")]
    [SerializeField] private LoadingUI loadingUI;
    
    [Header("Settings")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private string mainSceneName = "SystemsTest";
    
    private ParsingControl parsingControl;
    private bool initializationComplete = false;
    
    // Public property to check if initialization is complete
    public bool IsInitializationComplete => initializationComplete;
    
    private void Start()
    {
        LogDebug("Game initialization starting. Hold onto your butts.");
        
        // Find required components if not already assigned
        FindRequiredComponents();
        
        // Set up persistent systems if needed
        SetupPersistentSystems();
        
        // Begin initialization sequence
        StartCoroutine(InitializeGameSequence());
    }
    
    /// <summary>
    /// Finds required components if not already assigned.
    /// Because manually assigning references is too mainstream.
    /// </summary>
    private void FindRequiredComponents()
    {
        if (llm == null) llm = FindFirstObjectByType<LLM>();
        if (npcManager == null) npcManager = FindFirstObjectByType<NPCManager>();
        if (characterManager == null) characterManager = FindFirstObjectByType<CharacterManager>();
        if (worldCoordinator == null) worldCoordinator = FindFirstObjectByType<WorldCoordinator>();
        if (loadingUI == null) loadingUI = FindFirstObjectByType<LoadingUI>();
        
        parsingControl = FindFirstObjectByType<ParsingControl>();
        
        // Log missing components
        if (llm == null) Debug.LogError("LLM not found! The characters will be very quiet.");
        if (npcManager == null) Debug.LogError("NPCManager not found! No one's coming to this party.");
        if (characterManager == null) Debug.LogError("CharacterManager not found! Characters won't have any personality.");
        if (worldCoordinator == null) Debug.LogError("WorldCoordinator not found! The world will be very empty.");
        if (parsingControl == null) Debug.LogError("ParsingControl not found! No mystery to solve today.");
        if (loadingUI == null) Debug.LogWarning("LoadingUI not found! The player will stare at a blank screen. Fun.");
    }
    
    /// <summary>
    /// Sets up persistent systems if needed.
    /// Ensuring the stuff that needs to stick around actually does.
    /// </summary>
    private void SetupPersistentSystems()
    {
        // Find or create persistent systems container
        GameObject persistentSystems = GameObject.Find("Persistent Systems");
        if (!persistentSystems)
        {
            persistentSystems = new GameObject("Persistent Systems");
            persistentSystems.AddComponent<PersistentSystemsManager>();
            DontDestroyOnLoad(persistentSystems);
            LogDebug("Created Persistent Systems container");
        }
        
        // Ensure these systems stay persistent across scenes
        if (llm != null) llm.transform.SetParent(persistentSystems.transform);
        if (npcManager != null) npcManager.transform.SetParent(persistentSystems.transform);
        if (characterManager != null) characterManager.transform.SetParent(persistentSystems.transform);
        if (worldCoordinator != null) worldCoordinator.transform.SetParent(persistentSystems.transform);
        
        // Make this initializer persistent too
        transform.SetParent(persistentSystems.transform);
    }
    
    /// <summary>
    /// Game initialization sequence.
    /// Like a rocket launch countdown, but with more bugs.
    /// </summary>
    private IEnumerator InitializeGameSequence()
    {
        // Show loading UI
        if (loadingUI != null)
        {
            loadingUI.Show();
            loadingUI.SetProgress(0, "Starting initialization...");
        }
        
        // Step 1: Wait for LLM to start
        yield return WaitForLLMInitialization();
        
        // Step 2: Parse mystery and extract characters
        Mystery mystery = null;
        yield return ParseMysteryCoroutine((parsedMystery) => { mystery = parsedMystery; });
        
        if (mystery == null)
        {
            Debug.LogError("Failed to parse mystery! The game will be very empty and meaningless.");
            CompleteInitialization(false);
            yield break;
        }
        
        // Step 3: Initialize characters
        yield return InitializeCharacters(mystery);
        
        // Step 4: Generate the world
        yield return GenerateWorld(mystery);
        
        // Step 5: Finalize and complete
        CompleteInitialization(true);
    }
    
    /// <summary>
    /// Coroutine to parse the mystery and pass result through a callback.
    /// </summary>
    private IEnumerator ParseMysteryCoroutine(System.Action<Mystery> callback)
    {
        Mystery result = null;
        yield return ParseMysteryAndExtractCharacters();
        
        // Get result from parsing control
        if (parsingControl != null && parsingControl.ParsedMystery != null)
        {
            result = parsingControl.ParsedMystery;
        }
        
        callback(result);
    }
    
    /// <summary>
    /// Wait for the LLM to initialize.
    /// Because AI takes its sweet time to wake up.
    /// </summary>
    private IEnumerator WaitForLLMInitialization()
    {
        if (llm == null)
        {
            LogDebug("No LLM assigned, skipping LLM initialization");
            if (loadingUI != null) loadingUI.SetProgress(0.1f, "No LLM available");
            yield break;
        }
        
        LogDebug("Waiting for LLM to start...");
        if (loadingUI != null) loadingUI.SetProgress(0.1f, "Initializing language model...");
        
        int waitCount = 0;
        float startTime = Time.realtimeSinceStartup;
        
        while (!llm.started)
        {
            waitCount++;
            if (waitCount % 100 == 0) // Log every 100 frames to avoid spamming
            {
                float elapsedTime = Time.realtimeSinceStartup - startTime;
                LogDebug($"Still waiting for LLM to start... ({elapsedTime:F1} seconds elapsed)");
                
                if (loadingUI != null) loadingUI.SetProgress(0.1f, $"Initializing language model... ({elapsedTime:F1}s)");
            }
            yield return null;
        }
        
        float llmLoadTime = Time.realtimeSinceStartup - startTime;
        LogDebug($"LLM started successfully in {llmLoadTime:F1} seconds");
        
        if (loadingUI != null) loadingUI.SetProgress(0.2f, "Language model ready!");
    }
    
    /// <summary>
    /// Parse the mystery JSON and extract character data.
    /// Turning a blob of JSON into a living, breathing game world. Sort of.
    /// </summary>
    private IEnumerator ParseMysteryAndExtractCharacters()
    {
        if (parsingControl == null)
        {
            Debug.LogError("No ParsingControl found! Cannot parse mystery. What game are we even playing?");
            if (loadingUI != null) loadingUI.SetProgress(0.2f, "Error: Mystery parser not found!");
            yield break;
        }
        
        LogDebug("Parsing mystery data...");
        if (loadingUI != null) loadingUI.SetProgress(0.3f, "Parsing mystery data...");
        
        // Register for progress updates
        parsingControl.OnParsingProgress += (progress) =>
        {
            float adjustedProgress = 0.3f + (progress * 0.2f); // Scale to 0.3-0.5 range
            if (loadingUI != null) loadingUI.SetProgress(adjustedProgress, "Parsing mystery data...");
        };
        
        // Register for completion
        bool parsingComplete = false;
        
        parsingControl.OnParsingComplete += () =>
        {
            parsingComplete = true;
            LogDebug("Mystery parsing and character extraction complete");
        };
        
        // Start parsing
        parsingControl.ParseMystery();
        
        // Wait for completion
        float startTime = Time.realtimeSinceStartup;
        while (!parsingComplete)
        {
            yield return null;
            
            // Timeout after 60 seconds
            if (Time.realtimeSinceStartup - startTime > 60f)
            {
                Debug.LogError("Mystery parsing timed out after 60 seconds!");
                break;
            }
        }
        
        // Unregister events
        parsingControl.OnParsingProgress -= (progress) => {};
        parsingControl.OnParsingComplete -= () => {};
        
        if (parsingControl.ParsedMystery == null)
        {
            Debug.LogError("Failed to parse mystery data!");
            if (loadingUI != null) loadingUI.SetProgress(0.4f, "Error: Failed to parse mystery!");
        }
        else
        {
            LogDebug("Mystery parsed successfully");
            if (loadingUI != null) loadingUI.SetProgress(0.5f, "Mystery data parsed successfully!");
        }
    }
    
    /// <summary>
    /// Initialize character templates and behaviors.
    /// Breathing life into digital puppets.
    /// </summary>
    private IEnumerator InitializeCharacters(Mystery mystery)
    {
        if (characterManager == null)
        {
            LogDebug("No CharacterManager found, skipping character initialization");
            if (loadingUI != null) loadingUI.SetProgress(0.6f, "Warning: No character manager available");
            yield break;
        }
        
        LogDebug("Initializing characters...");
        if (loadingUI != null) loadingUI.SetProgress(0.6f, "Initializing characters...");
        
        // Wait for character manager to initialize
        while (!characterManager.IsInitialized)
        {
            // Update progress based on initialization progress
            float progress = characterManager.GetInitializationProgress();
            if (loadingUI != null) loadingUI.SetProgress(0.6f + (progress * 0.1f), "Initializing characters...");
            
            yield return null;
        }
        
        // Initialize NPCManager
        if (npcManager != null)
        {
            LogDebug("Initializing NPCManager...");
            if (loadingUI != null) loadingUI.SetProgress(0.7f, "Preparing NPCs...");
            
            try
            {
                await npcManager.Initialize();
                LogDebug("NPCManager initialization complete");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error during NPC initialization: {ex.Message}");
                Debug.LogException(ex);
            }
        }
        else
        {
            LogDebug("No NPCManager found, skipping NPC initialization");
        }
        
        if (loadingUI != null) loadingUI.SetProgress(0.7f, "Characters ready!");
    }
    
    /// <summary>
    /// Generate the world based on mystery data.
    /// Digital world creation, slightly easier than the biblical version.
    /// </summary>
    private IEnumerator GenerateWorld(Mystery mystery)
    {
        if (worldCoordinator == null)
        {
            LogDebug("No WorldCoordinator found, skipping world generation");
            if (loadingUI != null) loadingUI.SetProgress(0.8f, "Warning: No world coordinator available");
            yield break;
        }
        
        LogDebug("Generating world...");
        if (loadingUI != null) loadingUI.SetProgress(0.8f, "Generating world...");
        
        // Initialize the world
        worldCoordinator.InitializeWorld(mystery);
        
        // Wait for world to be initialized
        float startTime = Time.realtimeSinceStartup;
        while (!worldCoordinator.IsInitialized)
        {
            yield return null;
            
            // Timeout after 30 seconds
            if (Time.realtimeSinceStartup - startTime > 30f)
            {
                Debug.LogError("World generation timed out after 30 seconds!");
                break;
            }
            
            // Update progress
            if (loadingUI != null) loadingUI.SetProgress(0.8f + ((Time.realtimeSinceStartup - startTime) / 30f * 0.1f), "Generating world...");
        }
        
        LogDebug("World generation complete");
        if (loadingUI != null) loadingUI.SetProgress(0.9f, "World ready!");
    }
    
    /// <summary>
    /// Complete the initialization process.
    /// We made it! Or did we?
    /// </summary>
    private void CompleteInitialization(bool success)
    {
        if (success)
        {
            LogDebug("Initialization complete! The game is ready to play.");
            if (loadingUI != null)
            {
                loadingUI.SetProgress(1.0f, "Ready!");
                loadingUI.Hide();
            }
        }
        else
        {
            Debug.LogError("Initialization failed! The game may not work correctly.");
            if (loadingUI != null)
            {
                loadingUI.SetProgress(1.0f, "Initialization failed! Proceeding anyway...");
                loadingUI.Hide();
            }
        }
        
        initializationComplete = true;
    }
    
    private void LogDebug(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[GameInitializer] {message}");
        }
    }
}
