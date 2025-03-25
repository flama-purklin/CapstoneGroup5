using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Configuration script for setting up the main scene.
/// This is a utility class for scene setup, not meant to be used at runtime.
/// </summary>
#if UNITY_EDITOR
using UnityEditor;

public class MainSceneConfiguration : MonoBehaviour
{
    [MenuItem("Mystery Engine/Scene Configuration/Setup Main Scene")]
    public static void SetupMainScene()
    {
        // Make sure the scene is saved first
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.Log("Scene setup canceled by user");
            return;
        }
        
        // Create the main scene
        Scene mainScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        // Create core GameObject hierarchy
        CreateGameObjectHierarchy();
        
        // Create manager objects
        SetupManagers();
        
        // Create environment objects
        SetupEnvironment();
        
        // Create UI components
        SetupUI();
        
        // Set up player
        SetupPlayer();
        
        // Save the scene
        string scenePath = "Assets/_Core/Scenes/MainScene.unity";
        bool success = EditorSceneManager.SaveScene(mainScene, scenePath);
        
        if (success)
        {
            Debug.Log($"Successfully created and saved MainScene at {scenePath}");
        }
        else
        {
            Debug.LogError($"Failed to save MainScene at {scenePath}");
        }
    }
    
    private static void CreateGameObjectHierarchy()
    {
        // Create root GameObjects for organization
        CreateGameObject("--- Core Systems ---", null);
        CreateGameObject("--- Environment ---", null);
        CreateGameObject("--- UI ---", null);
        CreateGameObject("--- Player ---", null);
    }
    
    private static void SetupManagers()
    {
        // Create GameManager
        GameObject gameManagerObj = CreateGameObject("GameManager", null);
        gameManagerObj.AddComponent<GameManager>();
        
        // Create WorldCoordinator
        GameObject worldCoordinatorObj = CreateGameObject("WorldCoordinator", null);
        worldCoordinatorObj.AddComponent<WorldCoordinator>();
        
        GameObject trainGeneratorObj = CreateGameObject("TrainGenerator", worldCoordinatorObj.transform);
        trainGeneratorObj.AddComponent<TrainGenerator>();
        
        GameObject entityPlacerObj = CreateGameObject("EntityPlacer", worldCoordinatorObj.transform);
        entityPlacerObj.AddComponent<EntityPlacer>();
        
        GameObject locationRegistryObj = CreateGameObject("LocationRegistry", worldCoordinatorObj.transform);
        locationRegistryObj.AddComponent<LocationRegistry>();
        
        // Create other managers
        GameObject npcManagerObj = CreateGameObject("NPCManager", null);
        npcManagerObj.AddComponent<NPCManager>();
        
        GameObject evidenceManagerObj = CreateGameObject("EvidenceManager", null);
        evidenceManagerObj.AddComponent<EvidenceManager>();
    }
    
    private static void SetupEnvironment()
    {
        GameObject environmentRoot = GameObject.Find("--- Environment ---");
        
        // Create a light
        GameObject lightObj = CreateGameObject("Directional Light", environmentRoot.transform);
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = Color.white;
        lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        
        // Create a ground plane
        GameObject groundObj = CreateGameObject("Ground", environmentRoot.transform);
        groundObj.AddComponent<MeshFilter>().mesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
        groundObj.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
        groundObj.transform.localScale = new Vector3(100f, 100f, 1f);
        groundObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        groundObj.transform.position = new Vector3(0f, -0.1f, 0f);
    }
    
    private static void SetupUI()
    {
        GameObject uiRoot = GameObject.Find("--- UI ---");
        
        // Create Canvas
        GameObject canvasObj = CreateGameObject("Canvas", uiRoot.transform);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // Create EventSystem
        GameObject eventSystemObj = CreateGameObject("EventSystem", uiRoot.transform);
        eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        
        // Create LoadingUI
        GameObject loadingUIObj = CreateGameObject("LoadingUI", canvasObj.transform);
        LoadingUIController loadingUI = loadingUIObj.AddComponent<LoadingUIController>();
        
        // Add CanvasGroup for fading
        loadingUIObj.AddComponent<CanvasGroup>();
        
        // Create loading panel
        GameObject loadingPanelObj = CreateGameObject("LoadingPanel", loadingUIObj.transform);
        
        // Create background image
        GameObject bgImageObj = CreateGameObject("Background", loadingPanelObj.transform);
        UnityEngine.UI.Image bgImage = bgImageObj.AddComponent<UnityEngine.UI.Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        bgImageObj.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        bgImageObj.GetComponent<RectTransform>().anchorMax = Vector2.one;
        bgImageObj.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        
        // Create progress bar
        GameObject progressBarObj = CreateGameObject("ProgressBar", loadingPanelObj.transform);
        UnityEngine.UI.Slider progressBar = progressBarObj.AddComponent<UnityEngine.UI.Slider>();
        progressBar.interactable = false;
        progressBar.minValue = 0f;
        progressBar.maxValue = 1f;
        progressBar.value = 0.5f;
        
        // Create status text
        GameObject statusTextObj = CreateGameObject("StatusText", loadingPanelObj.transform);
        TMPro.TextMeshProUGUI statusText = statusTextObj.AddComponent<TMPro.TextMeshProUGUI>();
        statusText.text = "Loading...";
        statusText.fontSize = 24f;
        statusText.alignment = TMPro.TextAlignmentOptions.Center;
        
        // Set references
        loadingUI.loadingPanel = loadingPanelObj;
        loadingUI.progressBar = progressBar;
        loadingUI.statusText = statusText;
    }
    
    private static void SetupPlayer()
    {
        GameObject playerRoot = GameObject.Find("--- Player ---");
        
        // Create camera
        GameObject cameraObj = CreateGameObject("Main Camera", playerRoot.transform);
        Camera camera = cameraObj.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.19f, 0.19f, 0.19f, 1f);
        cameraObj.transform.position = new Vector3(0f, 1.8f, -10f);
        
        // Create player controller placeholder
        GameObject playerObj = CreateGameObject("Player", playerRoot.transform);
        playerObj.transform.position = new Vector3(0f, 0f, 0f);
    }
    
    private static GameObject CreateGameObject(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name);
        if (parent != null)
        {
            gameObject.transform.SetParent(parent);
            gameObject.transform.localPosition = Vector3.zero;
        }
        return gameObject;
    }
}
#endif
