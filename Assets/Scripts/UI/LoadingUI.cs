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
    private float characterProgressWeight;                 

    private int totalCharacters;
    private bool llmWarmupStarted = false;

    private void Start()
    {
        llm = FindFirstObjectByType<LLM>();
        npcManager = FindFirstObjectByType<NPCManager>();

        if (!llm || !npcManager)
        {
            statusText.text = "Error: Required managers not found!";
            Debug.LogError("Required managers not found!");
            return;
        }

        string[] characters = npcManager.GetAvailableCharacters();
        totalCharacters = characters?.Length ?? 0;
        characterProgressWeight = (1f - (INITIAL_PHASE_WEIGHT + LLM_WARMUP_WEIGHT + CHARACTER_START_WEIGHT));

        llmPhaseStartTime = Time.time;
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
        const float initialPhase = INITIAL_PHASE_WEIGHT;          
        const float llmPhase = LLM_WARMUP_WEIGHT;                   
        const float characterPhaseWeight = CHARACTER_START_WEIGHT + (1f - (INITIAL_PHASE_WEIGHT + LLM_WARMUP_WEIGHT + CHARACTER_START_WEIGHT));

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
        else if (!npcManager.IsInitializationComplete)
        {
            float baseProgress = initialPhase + llmPhase;
            float timeInPhase = Time.time - characterPhaseStartTime;

            // Continuous dummy progress
            float visualProgress = 1 - Mathf.Exp(-characterAsymptoteSpeed * timeInPhase);
            visualProgress = Mathf.Clamp01(visualProgress);

            float actualProgress = Mathf.Clamp01(npcManager.GetInitializationProgress());

            //  if actual progress is ahead, nudge visual progress upward gradually
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