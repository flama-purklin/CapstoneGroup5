using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game manager responsible for overall game initialization,
/// state management, and high-level control flow.
/// 
/// This replaces the old GameController and GameInitializer components
/// with a unified, more cohesive approach.
/// </summary>
public class GameManager : MonoBehaviour
{
    // The singleton instance of the GameManager
    public static GameManager Instance { get; private set; }
    
    [Header("System Components")]
    [SerializeField] private LLMUnity.LLM llm;
    [SerializeField] private WorldCoordinator worldCoordinator;
    [SerializeField] private LoadingUIController loadingUI;
    
    [Header("Mystery Configuration")]
    [SerializeField] private string mysteryFolderPath = "MysteryStorage";
    [SerializeField] private string defaultMysteryFilename = "default_mystery.json";
    
    [Header("Game State")]
    [SerializeField] private GameState currentState = GameState.DEFAULT;
    
    // Initialization state
    private bool isInitialized = false;
    private Mystery currentMystery;
    
    // Properties
    public bool IsInitialized => isInitialized;
    public GameState CurrentState => currentState;
    public Mystery CurrentMystery => currentMystery;
    
    // Events
    public System.Action<GameState> OnGameStateChanged;
    public System.Action<float> OnInitializationProgress;
    public System.Action OnInitializationComplete;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Find required components if not assigned
        if (llm == null)
        {
            llm = FindFirstObjectByType<LLMUnity.LLM>();
            if (llm == null)
            {
                Debug.LogError("LLM component not found in scene!");
            }
        }
        
        if (worldCoordinator == null)
        {
            worldCoordinator = FindFirstObjectByType<WorldCoordinator>();
            if (worldCoordinator == null)
            {
                Debug.LogError("WorldCoordinator component not found in scene!");
            }
        }
        
        if (loadingUI == null)
        {
            loadingUI = FindFirstObjectByType<LoadingUIController>();
            if (loadingUI == null)
            {
                Debug.LogWarning("LoadingUIController not found - loading feedback may be limited.");
            }
        }
    }
    
    private void Start()
    {
        // Don't auto-start initialization in case we're in test mode
        // Use StartInitialization() to begin the process
    }
    
    /// <summary>
    /// Starts the game initialization sequence.
    /// </summary>
    public void StartInitialization()
    {
        if (isInitialized)
        {
            Debug.LogWarning("Game is already initialized. Call ResetGame() first if you want to reinitialize.");
            return;
        }
        
        // Show loading UI
        if (loadingUI != null)
        {
            loadingUI.Show();
        }
        
        // Start initialization sequence
        StartCoroutine(InitializationSequence());
    }
    
    /// <summary>
    /// Resets the game state for a fresh start.
    /// </summary>
    public void ResetGame()
    {
        isInitialized = false;
        currentMystery = null;
        
        // TODO: Add cleanup logic for train cars, characters, etc.
        
        // Reset game state
        SetGameState(GameState.DEFAULT);
    }
    
    /// <summary>
    /// The main initialization sequence.
    /// </summary>
    private IEnumerator InitializationSequence()
    {
        // Step 1: Wait for LLM to initialize
        yield return StartCoroutine(WaitForLLMInitialization());
        
        // Step 2: Load and parse mystery
        yield return StartCoroutine(LoadMystery());
        
        // Step 3: Initialize world with mystery data
        yield return StartCoroutine(InitializeWorld());
        
        // Step 4: Complete initialization
        CompleteInitialization();
    }
    
    /// <summary>
    /// Waits for the LLM to be fully initialized.
    /// </summary>
    private IEnumerator WaitForLLMInitialization()
    {
        if (llm == null)
        {
            Debug.LogError("Cannot initialize LLM: component not found!");
            yield break;
        }
        
        UpdateProgress(0.1f, "Initializing LLM...");
        
        int waitCount = 0;
        float startTime = Time.realtimeSinceStartup;
        
        while (!llm.started)
        {
            waitCount++;
            if (waitCount % 50 == 0) // Log every 50 frames to avoid spam
            {
                float elapsedTime = Time.realtimeSinceStartup - startTime;
                Debug.Log($"Still waiting for LLM to start... ({elapsedTime:F1} seconds elapsed)");
                UpdateProgress(0.1f, $"Waiting for LLM ({elapsedTime:F1}s)...");
            }
            yield return null;
        }
        
        float llmLoadTime = Time.realtimeSinceStartup - startTime;
        Debug.Log($"LLM started successfully in {llmLoadTime:F1} seconds");
        UpdateProgress(0.2f, "LLM initialization complete");
    }
    
    /// <summary>
    /// Loads and parses the mystery JSON.
    /// </summary>
    private IEnumerator LoadMystery()
    {
        UpdateProgress(0.3f, "Loading mystery data...");
        
        // Get the path to the mystery file
        string mysteryFolderFullPath = Path.Combine(Application.streamingAssetsPath, mysteryFolderPath);
        string mysteryFilePath = Path.Combine(mysteryFolderFullPath, defaultMysteryFilename);
        
        // Find any JSON file if the default isn't found
        if (!File.Exists(mysteryFilePath))
        {
            Debug.LogWarning($"Default mystery file not found at: {mysteryFilePath}");
            
            if (Directory.Exists(mysteryFolderFullPath))
            {
                var jsonFiles = Directory.GetFiles(mysteryFolderFullPath, "*.json");
                if (jsonFiles.Length > 0)
                {
                    mysteryFilePath = jsonFiles[0];
                    Debug.Log($"Using alternative mystery file: {mysteryFilePath}");
                }
                else
                {
                    Debug.LogError("No mystery JSON files found!");
                    UpdateProgress(0.3f, "Failed to find mystery data");
                    yield break;
                }
            }
            else
            {
                Debug.LogError($"Mystery folder not found at: {mysteryFolderFullPath}");
                UpdateProgress(0.3f, "Failed to find mystery folder");
                yield break;
            }
        }
        
        // Read and parse the mystery JSON
        try
        {
            string jsonContent = File.ReadAllText(mysteryFilePath);
            UpdateProgress(0.4f, "Parsing mystery data...");
            
            // Parse using JsonUtility
            currentMystery = JsonUtility.FromJson<Mystery>(jsonContent);
            
            // Validate the mystery data
            if (currentMystery == null)
            {
                Debug.LogError("Failed to parse mystery JSON!");
                UpdateProgress(0.4f, "Failed to parse mystery data");
                yield break;
            }
            
            if (currentMystery.Characters == null || currentMystery.Characters.Count == 0)
            {
                Debug.LogWarning("Mystery contains no characters!");
            }
            
            if (currentMystery.TrainLayout == null || currentMystery.TrainLayout.Cars == null || currentMystery.TrainLayout.Cars.Count == 0)
            {
                Debug.LogWarning("Mystery contains no train layout data!");
            }
            
            Debug.Log($"Successfully loaded mystery: {currentMystery.Metadata?.Title ?? "Untitled"}");
            UpdateProgress(0.5f, "Mystery data loaded");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error loading mystery file: {ex.Message}");
            UpdateProgress(0.4f, "Error loading mystery file");
            yield break;
        }
        
        yield return null;
    }
    
    /// <summary>
    /// Initializes the world with mystery data.
    /// </summary>
    private IEnumerator InitializeWorld()
    {
        if (currentMystery == null)
        {
            Debug.LogError("Cannot initialize world: No mystery data loaded!");
            yield break;
        }
        
        if (worldCoordinator == null)
        {
            Debug.LogError("Cannot initialize world: WorldCoordinator not found!");
            yield break;
        }
        
        UpdateProgress(0.6f, "Generating world...");
        
        // Initialize the world
        worldCoordinator.InitializeWorld(currentMystery);
        
        // Wait for world to initialize (this could be improved with an event)
        int waitCount = 0;
        float startTime = Time.realtimeSinceStartup;
        
        while (!worldCoordinator.IsInitialized && waitCount < 100)
        {
            waitCount++;
            if (waitCount % 20 == 0)
            {
                float elapsedTime = Time.realtimeSinceStartup - startTime;
                Debug.Log($"Waiting for world initialization... ({elapsedTime:F1} seconds elapsed)");
                UpdateProgress(0.7f, $"Setting up world ({elapsedTime:F1}s)...");
            }
            yield return null;
        }
        
        if (!worldCoordinator.IsInitialized)
        {
            Debug.LogWarning("World initialization may not have completed properly");
        }
        
        float worldLoadTime = Time.realtimeSinceStartup - startTime;
        Debug.Log($"World initialization completed in {worldLoadTime:F1} seconds");
        UpdateProgress(0.8f, "World generation complete");
        
        yield return null;
    }
    
    /// <summary>
    /// Completes the initialization process.
    /// </summary>
    private void CompleteInitialization()
    {
        UpdateProgress(0.9f, "Finalizing initialization...");
        
        // Set the game state to ready
        SetGameState(GameState.DEFAULT);
        
        // Mark as initialized
        isInitialized = true;
        
        // Hide loading UI
        if (loadingUI != null)
        {
            loadingUI.Hide();
        }
        
        UpdateProgress(1.0f, "Initialization complete");
        
        // Notify listeners
        OnInitializationComplete?.Invoke();
        
        Debug.Log("Game initialization complete! Game is ready to play.");
    }
    
    /// <summary>
    /// Sets the current game state.
    /// </summary>
    public void SetGameState(GameState newState)
    {
        if (currentState == newState)
            return;
            
        // Store old state for reference
        GameState oldState = currentState;
        
        // Set new state
        currentState = newState;
        
        // Log state change
        Debug.Log($"Game state changed: {oldState} -> {currentState}");
        
        // Notify listeners
        OnGameStateChanged?.Invoke(currentState);
    }
    
    /// <summary>
    /// Updates the initialization progress.
    /// </summary>
    private void UpdateProgress(float progress, string status)
    {
        // Update loading UI
        if (loadingUI != null)
        {
            loadingUI.UpdateProgress(progress, status);
        }
        
        // Notify listeners
        OnInitializationProgress?.Invoke(progress);
        
        Debug.Log($"Initialization progress: {progress:P0} - {status}");
    }
}
