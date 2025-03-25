using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Unified game manager that handles initialization and coordination of all core systems.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Core Systems")]
    [SerializeField] private LoadingUIController loadingUI;
    [SerializeField] private GameController gameController;
    [SerializeField] private WorldCoordinator worldCoordinator;
    [SerializeField] private LLMUnity.LLM llm;
    
    [Header("Mystery")]
    [SerializeField] private string mysteryFolderPath = "MysteryStorage";
    [SerializeField] private bool useRandomMystery = false;
    [SerializeField] private string specificMysteryName = "";
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private bool autoStart = true;
    
    // Events
    public event Action<float> OnInitializationProgress;
    public event Action OnInitializationComplete;
    
    // Cached components
    private ParsingControl parsingControl;
    
    private void Awake()
    {
        // Find required components if not set
        if (loadingUI == null)
        {
            loadingUI = FindFirstObjectByType<LoadingUIController>();
        }
        
        if (gameController == null)
        {
            gameController = FindFirstObjectByType<GameController>();
        }
        
        if (worldCoordinator == null)
        {
            worldCoordinator = FindFirstObjectByType<WorldCoordinator>();
        }
        
        if (llm == null)
        {
            llm = FindFirstObjectByType<LLMUnity.LLM>();
        }
        
        // Create the ParsingControl
        parsingControl = gameObject.AddComponent<ParsingControl>();
        parsingControl.mysteryFiles = mysteryFolderPath;
        
        // Hook up loading UI events
        if (loadingUI != null)
        {
            loadingUI.OnLoadingComplete += OnLoadingUIComplete;
        }
    }
    
    private void Start()
    {
        if (autoStart)
        {
            StartInitialization();
        }
    }
    
    /// <summary>
    /// Starts the game initialization sequence.
    /// </summary>
    public void StartInitialization()
    {
        if (loadingUI != null)
        {
            loadingUI.Initialize();
            StartCoroutine(InitializeGameCoroutine());
        }
        else
        {
            Debug.LogError("Loading UI is missing! Cannot start initialization.");
        }
    }
    
    /// <summary>
    /// Coroutine that handles the initialization sequence.
    /// </summary>
    private IEnumerator InitializeGameCoroutine()
    {
        LogDebug("Starting game initialization...");
        
        // Update loading UI
        loadingUI.UpdateStatus("Initializing systems...");
        loadingUI.SetProgress(0.1f);
        
        // Step 1: Wait for LLM to initialize
        LogDebug("STEP 1: Waiting for LLM to initialize...");
        yield return StartCoroutine(WaitForLLMInitialization());
        
        loadingUI.SetProgress(0.3f);
        loadingUI.UpdateStatus("LLM initialized. Loading mystery...");
        
        // Step 2: Parse mystery
        LogDebug("STEP 2: Parsing mystery...");
        Mystery mystery = null;
        
        parsingControl.OnParsingProgress += (progress) => {
            loadingUI.SetProgress(0.3f + progress * 0.4f);
        };
        
        parsingControl.OnMysteryParsed += (parsedMystery) => {
            mystery = parsedMystery;
            LogDebug($"Mystery parsed: {mystery.Metadata?.Title ?? "Unnamed Mystery"}");
        };
        
        parsingControl.OnParsingComplete += () => {
            LogDebug("Parsing complete");
        };
        
        // Start parsing
        yield return StartCoroutine(ParseMysteryCoroutine());
        
        if (mystery == null)
        {
            loadingUI.UpdateStatus("Failed to load mystery!");
            yield break;
        }
        
        loadingUI.SetProgress(0.7f);
        loadingUI.UpdateStatus("Mystery loaded. Initializing world...");
        
        // Step 3: Initialize the game with the mystery
        LogDebug("STEP 3: Initializing game with mystery...");
        gameController.InitializeGame(mystery);
        
        // Step 4: Generate the world
        LogDebug("STEP 4: Generating world...");
        worldCoordinator.InitializeWorld(mystery);
        
        // Wait for world initialization to complete
        while (!worldCoordinator.IsInitialized)
        {
            yield return null;
        }
        
        loadingUI.SetProgress(0.9f);
        loadingUI.UpdateStatus("World initialized. Starting game...");
        
        // Step 5: Final setup
        LogDebug("STEP 5: Final setup...");
        
        // Wait a bit to let everything settle
        yield return new WaitForSeconds(1f);
        
        // Complete initialization
        loadingUI.SetProgress(1.0f);
        loadingUI.UpdateStatus("Game ready!");
        
        yield return new WaitForSeconds(0.5f);
        
        // Hide loading UI
        loadingUI.CompleteLoading();
        
        LogDebug("Game initialization complete!");
        OnInitializationComplete?.Invoke();
    }
    
    /// <summary>
    /// Waits for the LLM to initialize.
    /// </summary>
    private IEnumerator WaitForLLMInitialization()
    {
        if (llm == null)
        {
            LogDebug("No LLM found, skipping LLM initialization");
            yield break;
        }
        
        float startTime = Time.time;
        int waitCount = 0;
        
        while (!llm.started)
        {
            waitCount++;
            if (waitCount % 100 == 0)
            {
                float elapsedTime = Time.time - startTime;
                loadingUI.UpdateStatus($"Initializing LLM... ({elapsedTime:F1}s)");
                LogDebug($"Still waiting for LLM to start... ({elapsedTime:F1}s elapsed)");
            }
            
            yield return null;
        }
        
        float llmLoadTime = Time.time - startTime;
        LogDebug($"LLM started successfully in {llmLoadTime:F1}s");
    }
    
    /// <summary>
    /// Coroutine for parsing the mystery.
    /// </summary>
    private IEnumerator ParseMysteryCoroutine()
    {
        bool parsingComplete = false;
        
        void OnComplete()
        {
            parsingComplete = true;
        }
        
        parsingControl.OnParsingComplete += OnComplete;
        
        // Start parsing
        parsingControl.ParseMystery();
        
        // Wait for parsing to complete
        while (!parsingComplete && !parsingControl.IsParsingComplete)
        {
            yield return null;
        }
        
        // Unsubscribe
        parsingControl.OnParsingComplete -= OnComplete;
    }
    
    /// <summary>
    /// Handler for when loading UI completes its fade-out.
    /// </summary>
    private void OnLoadingUIComplete()
    {
        // This is called after the loading UI has faded out
        LogDebug("Loading UI complete, game is now fully initialized and playable");
    }
    
    private void LogDebug(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[GameManager] {message}");
        }
    }
}