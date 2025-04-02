using LLMUnity;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Image spinnerImage;
    
    private LLM llm;
    private NPCManager npcManager;
    private ParsingControl parsingControl;
    private bool isInitialized = false;
    private float currentProgress = 0f;
    private bool llmWarmupStarted = false;
    private bool extractionStarted = false;
    private bool extractionCompleted = false;
    
    private void Start()
    {
        llm = FindFirstObjectByType<LLM>();
        npcManager = FindFirstObjectByType<NPCManager>();
        parsingControl = FindFirstObjectByType<ParsingControl>();
        
        if (parsingControl != null)
        {
            parsingControl.OnParsingProgress += HandleParsingProgress;
            parsingControl.OnCharactersExtracted += HandleCharactersExtracted;
            parsingControl.OnParsingComplete += HandleParsingComplete;
        }
    }
    
    private void OnDestroy()
    {
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
        currentProgress = progress;
    }
    
    private void HandleCharactersExtracted(int count)
    {
        // Handle character extraction
    }
    
    private void HandleParsingComplete()
    {
        extractionCompleted = true;
    }
    
    private void Update()
    {
        if (progressBar != null)
        {
            progressBar.value = currentProgress;
        }
        
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (statusText == null) return;
        
        try 
        {
            if (llm != null && llm.started && !llmWarmupStarted)
            {
                llmWarmupStarted = true;
            }
            
            if (!llmWarmupStarted)
            {
                statusText.text = "Initializing language model...";
            }
            else if (extractionStarted && !extractionCompleted)
            {
                statusText.text = "Extracting characters...";
            }
            else if (npcManager != null && !npcManager.IsInitializationComplete)
            {
                statusText.text = "Loading characters...";
            }
            else if (!isInitialized)
            {
                statusText.text = "Loading game...";
                isInitialized = true;
            }
        }
        catch
        {
            // Fallback for any errors
            try
            {
                statusText.text = "Loading game...";
            }
            catch
            {
                Debug.LogError("Failed to update status text");
            }
        }
        
        if (!isInitialized && spinnerImage)
        {
            spinnerImage.transform.Rotate(0, 0, -180f * Time.deltaTime);
        }
    }
    
    public void SetupReferences(Slider progressBar, TMP_Text statusText, Image spinnerImage)
    {
        this.progressBar = progressBar;
        this.statusText = statusText;
        this.spinnerImage = spinnerImage;
    }
}