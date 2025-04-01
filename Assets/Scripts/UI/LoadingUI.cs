using LLMUnity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Image spinnerImage;

    [Header("Progress Settings")]
    [SerializeField] private float smoothSpeed = 1f;
    [SerializeField] private float initialAsymptoteSpeed = 0.1f;
    [SerializeField] private float characterAsymptoteSpeed = 0.01f;
    
    /// <summary>
    /// Set up references to UI components
    /// </summary>
    public void SetupReferences(Slider progressBar, TMP_Text statusText, Image spinnerImage)
    {
        this.progressBar = progressBar;
        this.statusText = statusText;
        this.spinnerImage = spinnerImage;
    }

    private LLM llm;
    private NPCManager npcManager;
    private bool isInitialized = false;

    // Progress tracking
    private float currentProgress = 0f;
    private float llmPhaseStartTime;
    private float characterPhaseStartTime;

    // Phase weights
    private const float INITIAL_PHASE_WEIGHT = 0.10f;    
    private const float LLM_WARMUP_WEIGHT = 0.10f;        
    private const float CHARACTER_START_WEIGHT = 0.20f;                   

    private int totalCharacters;
    private bool llmWarmupStarted = false;

    private ParsingControl parsingControl;
    private bool extractionStarted = false;
    private bool extractionCompleted = false;

    private void Start()
    {
        llm = FindFirstObjectByType<LLM>();
        npcManager = FindFirstObjectByType<NPCManager>();
        parsingControl = FindFirstObjectByType<ParsingControl>();

        if (!llm || !npcManager)
        {
            statusText.text = "Error: Required managers not found!";
            Debug.LogError("Required managers not found!");
            return;
        }

        string[] characters = npcManager.GetAvailableCharacters();
        totalCharacters = characters?.Length ?? 0;

        llmPhaseStartTime = Time.time;
        
        // Connect to the parsing events
        if (parsingControl != null)
        {
            parsingControl.OnParsingProgress += HandleParsingProgress;
            parsingControl.OnCharactersExtracted += HandleCharactersExtracted;
            parsingControl.OnParsingComplete += HandleParsingComplete;
        }
    }
    
    private void OnDestroy()
    {
        // Disconnect from events
        if (parsingControl != null)
        {
            parsingControl.OnParsingProgress -= HandleParsingProgress;
            parsingControl.OnCharactersExtracted -= HandleCharactersExtracted;
            parsingControl.OnParsingComplete -= HandleParsingComplete;
        }
    }
    
    private void HandleParsingProgress(float progress)
    {
        extractionStarted = true;
        // Update the current progress based on the parsing progress
        // This is also used in CalculateTargetProgress
        currentProgress = progress;
        
        // Log the progress for debugging
        if (progress < 0.1f)
        {
            Debug.Log($"Parsing started, progress: {progress:P0}");
        }
        else if (progress >= 0.99f)
        {
            Debug.Log($"Parsing complete, progress: {progress:P0}");
        }
        else if ((int)(progress * 100) % 25 == 0) // Log at approximate 25% intervals
        {
            Debug.Log($"Parsing progress: {progress:P0}");
        }
    }
    
    private void HandleCharactersExtracted(int count)
    {
        totalCharacters = count;
    }
    
    private void HandleParsingComplete()
    {
        extractionCompleted = true;
    }

    private void Update()
    {
        float targetProgress = CalculateTargetProgress();
        currentProgress = Mathf.Lerp(currentProgress, targetProgress, Time.deltaTime * smoothSpeed);
        progressBar.value = currentProgress;

        UpdateStatusText();
        UpdateSpinner();
    }

    private float CalculateTargetProgress()
    {
        // Phase weight definitions
        const float initialPhase = INITIAL_PHASE_WEIGHT;          // 10%
        const float llmPhase = LLM_WARMUP_WEIGHT;                 // 10%
        const float extractionPhase = 0.30f;                      // 30%
        const float characterPhaseWeight = CHARACTER_START_WEIGHT + 
            (1f - (INITIAL_PHASE_WEIGHT + LLM_WARMUP_WEIGHT + CHARACTER_START_WEIGHT + extractionPhase)); // 50%

        if (!llmWarmupStarted)
        {
            // INITIAL LLM SETUP PHASE
            float timeInPhase = Time.time - llmPhaseStartTime;
            float visualProgress = 1 - Mathf.Exp(-initialAsymptoteSpeed * timeInPhase);
            float progress = Mathf.Clamp01(visualProgress) * initialPhase;

            if (llm.started)
            {
                llmWarmupStarted = true;
                characterPhaseStartTime = Time.time;
                progress = initialPhase + llmPhase;
            }
            return progress;
        }
        else if (extractionStarted && !extractionCompleted)
        {
            // CHARACTER EXTRACTION PHASE
            float baseProgress = initialPhase + llmPhase;
            
            // If we have parsingControl, use its progress
            float extractionProgress = 0f;
            if (parsingControl != null)
            {
                // Get the progress directly from events
                extractionProgress = Mathf.Clamp01(currentProgress);
            }
            else
            {
                // Fallback to time-based progress
                float timeInPhase = Time.time - characterPhaseStartTime;
                extractionProgress = 1 - Mathf.Exp(-characterAsymptoteSpeed * timeInPhase);
                extractionProgress = Mathf.Clamp01(extractionProgress);
            }
            
            float progress = baseProgress + extractionProgress * extractionPhase;
            return Mathf.Clamp01(progress);
        }
        else if (!npcManager.IsInitializationComplete)
        {
            // CHARACTER INITIALIZATION PHASE
            float baseProgress = initialPhase + llmPhase + extractionPhase;
            float timeInPhase = Time.time - characterPhaseStartTime;

            // Continuous dummy progress
            float visualProgress = 1 - Mathf.Exp(-characterAsymptoteSpeed * timeInPhase);
            visualProgress = Mathf.Clamp01(visualProgress);

            float actualProgress = Mathf.Clamp01(npcManager.GetInitializationProgress());

            // If actual progress is ahead, nudge visual progress upward gradually
            float blendedProgress = visualProgress;
            if (actualProgress > visualProgress)
            {
                blendedProgress = visualProgress + (actualProgress - visualProgress) * 0.4f; 
            }

            float progress = baseProgress + blendedProgress * characterPhaseWeight;
            return Mathf.Clamp01(progress);
        }
        else
        {
            return 1f;
        }
    }


    private void UpdateStatusText()
    {
        if (!llmWarmupStarted)
        {
            statusText.text = "Initializing language model...";
        }
        else if (extractionStarted && !extractionCompleted)
        {
            float extractionProgress = 0f;
            if (parsingControl != null)
            {
                // Estimate extraction progress
                extractionProgress = currentProgress * 100f;
                statusText.text = $"Extracting characters ({extractionProgress:F0}%)...";
            }
            else
            {
                statusText.text = "Extracting character data...";
            }
        }
        else if (!npcManager.IsInitializationComplete)
        {
            float characterPercentage = npcManager.GetInitializationProgress() * 100f;
            statusText.text = $"Loading characters ({characterPercentage:F0}%)...";
        }
        else if (!isInitialized)
        {
            statusText.text = "Loading game...";
            isInitialized = true;
        }
    }

    private void UpdateSpinner()
    {
        if (!isInitialized && spinnerImage)
        {
            spinnerImage.transform.Rotate(0, 0, -180f * Time.deltaTime);
        }
    }
}