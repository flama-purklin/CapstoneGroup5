using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Editor tool to fix the LoadingOverlay references
/// </summary>
[ExecuteInEditMode]
public class FixLoadingOverlay : Editor
{
    [MenuItem("Tools/Fix LoadingOverlay References")]
    public static void FixReferences()
    {
        // Find the LoadingOverlay object
        GameObject loadingOverlayObj = GameObject.Find("LoadingOverlay");
        if (loadingOverlayObj == null)
        {
            Debug.LogError("LoadingOverlay not found in scene!");
            return;
        }
        
        LoadingOverlay loadingOverlay = loadingOverlayObj.GetComponent<LoadingOverlay>();
        if (loadingOverlay == null)
        {
            Debug.LogError("LoadingOverlay component not found on LoadingOverlay object!");
            return;
        }
        
        // Find or create the LoadingCanvas
        Transform canvasTransform = loadingOverlayObj.transform.Find("LoadingCanvas");
        if (canvasTransform == null)
        {
            GameObject canvasObj = new GameObject("LoadingCanvas");
            canvasObj.transform.SetParent(loadingOverlayObj.transform);
            canvasTransform = canvasObj.transform;
            
            // Add Canvas component
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            
            // Add CanvasScaler
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // Add GraphicRaycaster
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Find or create the StatusText
        Transform textTransform = canvasTransform.Find("StatusText");
        if (textTransform == null)
        {
            GameObject textObj = new GameObject("StatusText");
            textObj.transform.SetParent(canvasTransform);
            textTransform = textObj.transform;
            
            // Set up RectTransform
            RectTransform rectTransform = textObj.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = textObj.AddComponent<RectTransform>();
            }
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(800, 60);
            rectTransform.anchoredPosition = new Vector2(0, -120);
            
            // Add TextMeshProUGUI component
            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = "Initializing...";
            tmpText.font = TMP_Settings.defaultFontAsset;
            tmpText.fontSize = 36;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = Color.white;
        }
        else
        {
            // Ensure it has a TextMeshProUGUI component
            TextMeshProUGUI tmpText = textTransform.GetComponent<TextMeshProUGUI>();
            if (tmpText == null)
            {
                tmpText = textTransform.gameObject.AddComponent<TextMeshProUGUI>();
                tmpText.text = "Initializing...";
                tmpText.font = TMP_Settings.defaultFontAsset;
                tmpText.fontSize = 36;
                tmpText.alignment = TextAlignmentOptions.Center;
                tmpText.color = Color.white;
            }
        }
        
        // Force the LoadingOverlay to reset its setup
        var setupCompleteProp = new SerializedObject(loadingOverlay).FindProperty("setupComplete");
        if (setupCompleteProp != null)
        {
            setupCompleteProp.boolValue = false;
            new SerializedObject(loadingOverlay).ApplyModifiedProperties();
        }
        
        // Enable the LoadingOverlay component
        loadingOverlay.enabled = true;
        
        // Set the LoadingOverlay in the InitializationManager
        var initManager = FindFirstObjectByType<InitializationManager>();
        if (initManager != null)
        {
            SerializedObject serializedObject = new SerializedObject(initManager);
            SerializedProperty loadingOverlayProp = serializedObject.FindProperty("loadingOverlay");
            if (loadingOverlayProp != null)
            {
                loadingOverlayProp.objectReferenceValue = loadingOverlayObj;
                serializedObject.ApplyModifiedProperties();
            }
        }
        
        Debug.Log("Successfully fixed LoadingOverlay references");
    }
}