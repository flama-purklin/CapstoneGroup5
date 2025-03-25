using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the loading UI overlay instead of using a separate loading scene.
/// Provides progress updates and status messages during initialization.
/// </summary>
public class LoadingUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private GameObject loadingSpinner;
    
    [Header("Settings")]
    [SerializeField] private float minDisplayTime = 2f;
    [SerializeField] private bool fadeOutWhenComplete = true;
    [SerializeField] private float fadeOutDuration = 1f;
    
    private float showStartTime;
    private bool isInitialized = false;
    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;
    
    // Events
    public event Action OnLoadingComplete;
    
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Make sure we're visible initially
        canvasGroup.alpha = 1f;
        
        // Hide initially if needed
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Shows the loading UI.
    /// </summary>
    public void Show()
    {
        Initialize();
    }
    
    /// <summary>
    /// Hides the loading UI.
    /// </summary>
    public void Hide()
    {
        CompleteLoading();
    }
    
    /// <summary>
    /// Initializes the loading UI.
    /// </summary>
    public void Initialize()
    {
        if (isInitialized) return;
        
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }
        
        showStartTime = Time.time;
        SetProgress(0f);
        UpdateStatus("Initializing...");
        
        isInitialized = true;
    }
    
    /// <summary>
    /// Updates the progress bar.
    /// </summary>
    public void SetProgress(float progress)
    {
        if (progressBar != null)
        {
            progressBar.value = Mathf.Clamp01(progress);
        }
    }
    
    /// <summary>
    /// Updates both progress and status text.
    /// </summary>
    public void UpdateProgress(float progress, string status)
    {
        SetProgress(progress);
        UpdateStatus(status);
    }
    
    /// <summary>
    /// Updates the status text.
    /// </summary>
    public void UpdateStatus(string status)
    {
        if (statusText != null)
        {
            statusText.text = status;
        }
        
        Debug.Log($"[Loading] {status}");
    }
    
    /// <summary>
    /// Completes the loading process and hides the UI.
    /// </summary>
    public void CompleteLoading()
    {
        // Ensure minimum display time
        float timeShown = Time.time - showStartTime;
        float remainingTime = Mathf.Max(0, minDisplayTime - timeShown);
        
        if (remainingTime > 0 && fadeOutWhenComplete)
        {
            Invoke(nameof(FadeOutAndComplete), remainingTime);
        }
        else if (fadeOutWhenComplete)
        {
            FadeOutAndComplete();
        }
        else
        {
            // Just hide immediately
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(false);
            }
            
            OnLoadingComplete?.Invoke();
        }
    }
    
    /// <summary>
    /// Fades out the loading UI and completes the loading process.
    /// </summary>
    private void FadeOutAndComplete()
    {
        if (canvasGroup != null)
        {
            // Cancel any existing fade
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            
            // Start new fade
            fadeCoroutine = StartCoroutine(FadeOut());
        }
        else
        {
            // No canvas group, just hide
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(false);
            }
            
            OnLoadingComplete?.Invoke();
        }
    }
    
    /// <summary>
    /// Coroutine for fading out the loading UI.
    /// </summary>
    private IEnumerator FadeOut()
    {
        float startTime = Time.time;
        float startAlpha = canvasGroup.alpha;
        
        while (Time.time < startTime + fadeOutDuration)
        {
            float elapsed = Time.time - startTime;
            float t = elapsed / fadeOutDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }
        
        // Ensure we reach 0
        canvasGroup.alpha = 0f;
        
        // Hide panel
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
        
        OnLoadingComplete?.Invoke();
    }
}
