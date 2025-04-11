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
    private bool parsingStarted = false; // Renamed from extractionStarted
    // private bool parsingCompleted = false; // Flag no longer needed, will check ParsingControl directly

    private void Start()
    {
        llm = FindFirstObjectByType<LLM>();
        npcManager = FindFirstObjectByType<NPCManager>();
        parsingControl = FindFirstObjectByType<ParsingControl>();
        
        if (parsingControl != null)
        {
            parsingControl.OnParsingProgress += HandleParsingProgress;
            // parsingControl.OnCharactersExtracted += HandleCharactersExtracted; // Removed subscription
            // parsingControl.OnParsingComplete += HandleParsingComplete; // Removed subscription
        }
    }
    
    private void OnDestroy()
    {
        if (parsingControl != null)
        {
            parsingControl.OnParsingProgress -= HandleParsingProgress;
            // parsingControl.OnCharactersExtracted -= HandleCharactersExtracted; // Removed unsubscription
            // parsingControl.OnParsingComplete -= HandleParsingComplete; // Removed unsubscription
        }
    }
    private void HandleParsingProgress(float progress)
    {
        parsingStarted = true; // Use renamed flag
        currentProgress = progress;
    }

    // Removed HandleCharactersExtracted method
    // Removed HandleParsingComplete method

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

            // Check parsing status directly from ParsingControl
            bool isParsingComplete = parsingControl != null && parsingControl.IsParsingComplete;

            if (!llmWarmupStarted)
            {
                statusText.text = "Initializing language model...";
            }
            else if (parsingStarted && !isParsingComplete) // Check flag directly
            {
                statusText.text = "Parsing mystery data..."; // Updated text
            }
            else if (npcManager != null && !npcManager.IsInitializationComplete) // Assuming NPCManager still has this flag/property
            {
                statusText.text = "Initializing characters..."; // Updated text
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
