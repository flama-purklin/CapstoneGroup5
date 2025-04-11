using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Sets up the LoadingOverlay prefab with UI components for the unified scene approach.
/// This script runs in the editor to configure the LoadingOverlay object.
/// </summary>
[ExecuteInEditMode]
public class LoadingOverlay : MonoBehaviour
{
    // References to UI components
    [SerializeField] private Canvas loadingCanvas;
    [SerializeField] private Image backgroundPanel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Image spinnerImage;
    
    // Loading UI component reference
    [SerializeField] private LoadingUI loadingUI;
    
    [Header("Configuration")]
    [SerializeField] private bool setupComplete = false;
    [SerializeField] private int canvasSortOrder = 100;

    private void Awake()
    {
        // Always try to setup even at runtime
        if (!setupComplete)
        {
            // --- DIAGNOSTIC LOGGING START ---
            
        }
    }
    
    /// <summary>
    /// Sets up the LoadingOverlay with the necessary UI components
    /// </summary>
    public void SetupLoadingOverlay()
    {
        // Create canvas if it doesn't exist
        if (loadingCanvas == null)
        {
            Transform canvasTransform = transform.Find("LoadingCanvas");
            if (canvasTransform == null)
            {
                GameObject canvasObj = new GameObject("LoadingCanvas");
                canvasObj.transform.SetParent(transform);
                canvasTransform = canvasObj.transform;
            }
            
            loadingCanvas = canvasTransform.GetComponent<Canvas>();
            if (loadingCanvas == null)
            {
                loadingCanvas = canvasTransform.gameObject.AddComponent<Canvas>();
            }
            
            // Configure canvas
            loadingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            loadingCanvas.sortingOrder = canvasSortOrder;
            
            // Add required components
            if (canvasTransform.GetComponent<CanvasScaler>() == null)
            {
                CanvasScaler scaler = canvasTransform.gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
            }
            
            if (canvasTransform.GetComponent<GraphicRaycaster>() == null)
            {
                canvasTransform.gameObject.AddComponent<GraphicRaycaster>();
            }
        }
        
        // Set up background panel
        if (backgroundPanel == null)
        {
            Transform panelTransform = loadingCanvas.transform.Find("BackgroundPanel");
            if (panelTransform == null)
            {
                GameObject panelObj = new GameObject("BackgroundPanel");
                panelObj.transform.SetParent(loadingCanvas.transform);
                panelTransform = panelObj.transform;
                
                // Set up RectTransform
                RectTransform rectTransform = panelObj.AddComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
            }
            
            backgroundPanel = panelTransform.GetComponent<Image>();
            if (backgroundPanel == null)
            {
                backgroundPanel = panelTransform.gameObject.AddComponent<Image>();
                backgroundPanel.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            }
        }
        
        // Set up progress bar
        if (progressBar == null)
        {
            Transform barTransform = loadingCanvas.transform.Find("ProgressBar");
            if (barTransform == null)
            {
                GameObject barObj = new GameObject("ProgressBar");
                barObj.transform.SetParent(loadingCanvas.transform);
                barTransform = barObj.transform;
                
                // Set up RectTransform
                RectTransform rectTransform = barObj.AddComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.sizeDelta = new Vector2(800, 40);
                rectTransform.anchoredPosition = new Vector2(0, -200);
                
                // Add slider component
                progressBar = barObj.AddComponent<Slider>();
                progressBar.minValue = 0;
                progressBar.maxValue = 1;
                progressBar.value = 0;
                
                // Create background
                GameObject bgObj = new GameObject("Background");
                bgObj.transform.SetParent(barTransform);
                Image bgImage = bgObj.AddComponent<Image>();
                bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
                
                RectTransform bgRect = bgObj.GetComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = Vector2.zero;
                bgRect.offsetMax = Vector2.zero;
                
                // Create fill area
                GameObject fillAreaObj = new GameObject("Fill Area");
                fillAreaObj.transform.SetParent(barTransform);
                
                RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
                fillAreaRect.anchorMin = Vector2.zero;
                fillAreaRect.anchorMax = Vector2.one;
                fillAreaRect.offsetMin = new Vector2(5, 5);
                fillAreaRect.offsetMax = new Vector2(-5, -5);
                
                // Create fill
                GameObject fillObj = new GameObject("Fill");
                fillObj.transform.SetParent(fillAreaObj.transform);
                Image fillImage = fillObj.AddComponent<Image>();
                fillImage.color = new Color(0.2f, 0.7f, 1f, 1f);
                
                RectTransform fillRect = fillObj.GetComponent<RectTransform>();
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = new Vector2(1, 1);
                fillRect.offsetMin = Vector2.zero;
                fillRect.offsetMax = Vector2.zero;
                
                // Configure slider
                progressBar.targetGraphic = bgImage;
                progressBar.fillRect = fillRect;
            }
            else
            {
                progressBar = barTransform.GetComponent<Slider>();
            }
        }
        
        // Set up status text
        if (statusText == null)
        {
            Transform textTransform = loadingCanvas.transform.Find("StatusText");
            if (textTransform == null)
            {
                GameObject textObj = new GameObject("StatusText");
                textObj.transform.SetParent(loadingCanvas.transform);
                textTransform = textObj.transform;
                
                // Set up RectTransform
                RectTransform rectTransform = textObj.AddComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.sizeDelta = new Vector2(800, 60);
                rectTransform.anchoredPosition = new Vector2(0, -120);
                
                // Add text component (using concrete implementation)
                statusText = textObj.AddComponent<TMPro.TextMeshProUGUI>();
                statusText.text = "Initializing...";
                statusText.font = TMP_Settings.defaultFontAsset;
                statusText.fontSize = 36;
                statusText.alignment = TextAlignmentOptions.Center;
                statusText.color = Color.white;
            }
            else
            {
                statusText = textTransform.GetComponent<TMP_Text>();
            }
        }
        
        // Set up spinner
        if (spinnerImage == null)
        {
            Transform spinnerTransform = loadingCanvas.transform.Find("Loading Spinner");
            if (spinnerTransform == null)
            {
                GameObject spinnerObj = new GameObject("Loading Spinner");
                spinnerObj.transform.SetParent(loadingCanvas.transform);
                spinnerTransform = spinnerObj.transform;
                
                // Set up RectTransform
                RectTransform rectTransform = spinnerObj.AddComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.sizeDelta = new Vector2(100, 100);
                rectTransform.anchoredPosition = Vector2.zero;
                
                // Add image component
                spinnerImage = spinnerObj.AddComponent<Image>();
                spinnerImage.color = Color.white;
            }
            else
            {
                spinnerImage = spinnerTransform.GetComponent<Image>();
            }
        }
        
        // Set up LoadingUI component
        if (loadingUI == null)
        {
            loadingUI = GetComponent<LoadingUI>();
            if (loadingUI == null)
            {
                loadingUI = gameObject.AddComponent<LoadingUI>();
            }
            
            // Set references
            loadingUI.SetupReferences(progressBar, statusText, spinnerImage);
        }
        
        setupComplete = true;
        
    }
}
