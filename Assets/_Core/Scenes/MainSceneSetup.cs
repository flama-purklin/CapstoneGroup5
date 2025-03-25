using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Utility class for setting up the main scene with the refactored architecture.
/// </summary>
public static class MainSceneSetup
{
    [MenuItem("Mystery Engine/Setup/Create Main Scene")]
    public static void CreateMainScene()
    {
        // Save any current changes
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.Log("Scene setup canceled");
            return;
        }
        
        // Create a new scene
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        // Set up scene hierarchy
        CreateSceneHierarchy();
        
        // Save the scene
        string scenePath = "Assets/_Core/Scenes/MainScene.unity";
        EditorSceneManager.SaveScene(newScene, scenePath);
        
        Debug.Log($"Main scene created at {scenePath}");
    }
    
    private static void CreateSceneHierarchy()
    {
        // Create organization objects
        GameObject coreSystems = CreateGameObject("--- Core Systems ---", null);
        GameObject environment = CreateGameObject("--- Environment ---", null);
        GameObject ui = CreateGameObject("--- UI ---", null);
        GameObject player = CreateGameObject("--- Player ---", null);
        
        // Set up core systems
        SetupCoreSystems(coreSystems);
        
        // Set up environment
        SetupEnvironment(environment);
        
        // Set up UI
        SetupUI(ui);
        
        // Set up player
        SetupPlayer(player);
    }
    
    private static void SetupCoreSystems(GameObject parent)
    {
        // Create GameManager
        GameObject gameManager = CreateGameObject("GameManager", parent.transform);
        gameManager.AddComponent<GameManager>();
        
        // Create WorldCoordinator
        GameObject worldCoordinator = CreateGameObject("WorldCoordinator", parent.transform);
        WorldCoordinator worldCoordinatorComponent = worldCoordinator.AddComponent<WorldCoordinator>();
        
        // Create LocationRegistry
        GameObject locationRegistry = CreateGameObject("LocationRegistry", worldCoordinator.transform);
        LocationRegistry locationRegistryComponent = locationRegistry.AddComponent<LocationRegistry>();
        
        // Create TrainGenerator
        GameObject trainGenerator = CreateGameObject("TrainGenerator", worldCoordinator.transform);
        TrainGenerator trainGeneratorComponent = trainGenerator.AddComponent<TrainGenerator>();
        
        // Create Train parent
        GameObject trainParent = CreateGameObject("Train", trainGenerator.transform);
        trainGeneratorComponent.trainParent = trainParent.transform;
        
        // Create EntityPlacer
        GameObject entityPlacer = CreateGameObject("EntityPlacer", worldCoordinator.transform);
        EntityPlacer entityPlacerComponent = entityPlacer.AddComponent<EntityPlacer>();
        
        // Set up references
        worldCoordinatorComponent.trainGenerator = trainGeneratorComponent;
        worldCoordinatorComponent.locationRegistry = locationRegistryComponent;
        worldCoordinatorComponent.entityPlacer = entityPlacerComponent;
        
        // Create NPCManager
        GameObject npcManager = CreateGameObject("NPCManager", parent.transform);
        NPCManager npcManagerComponent = npcManager.AddComponent<NPCManager>();
        
        // Create EvidenceManager
        GameObject evidenceManager = CreateGameObject("EvidenceManager", parent.transform);
        EvidenceManager evidenceManagerComponent = evidenceManager.AddComponent<EvidenceManager>();
        
        // Set up references
        entityPlacerComponent.npcManager = npcManagerComponent;
        entityPlacerComponent.evidenceManager = evidenceManagerComponent;
    }
    
    private static void SetupEnvironment(GameObject parent)
    {
        // Create directional light
        GameObject directionalLight = CreateGameObject("Directional Light", parent.transform);
        Light light = directionalLight.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.0f;
        light.shadows = LightShadows.Soft;
        directionalLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        
        // Create ground
        GameObject ground = CreateGameObject("Ground", parent.transform);
        ground.AddComponent<MeshFilter>().mesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
        MeshRenderer groundRenderer = ground.AddComponent<MeshRenderer>();
        groundRenderer.material = new Material(Shader.Find("Standard"));
        groundRenderer.material.color = new Color(0.5f, 0.5f, 0.5f);
        ground.transform.localScale = new Vector3(100f, 100f, 1f);
        ground.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        ground.transform.position = new Vector3(0f, -0.5f, 0f);
    }
    
    private static void SetupUI(GameObject parent)
    {
        // Create Canvas
        GameObject canvas = CreateGameObject("Canvas", parent.transform);
        Canvas canvasComponent = canvas.AddComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // Create EventSystem
        GameObject eventSystem = CreateGameObject("EventSystem", parent.transform);
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        
        // Create LoadingUI
        GameObject loadingUI = CreateGameObject("LoadingUI", canvas.transform);
        LoadingUIController loadingUIComponent = loadingUI.AddComponent<LoadingUIController>();
        CanvasGroup loadingCanvasGroup = loadingUI.AddComponent<CanvasGroup>();
        
        // Create loading panel
        GameObject loadingPanel = CreateGameObject("LoadingPanel", loadingUI.transform);
        RectTransform loadingPanelRect = loadingPanel.AddComponent<RectTransform>();
        loadingPanelRect.anchorMin = Vector2.zero;
        loadingPanelRect.anchorMax = Vector2.one;
        loadingPanelRect.sizeDelta = Vector2.zero;
        
        // Create background
        GameObject background = CreateGameObject("Background", loadingPanel.transform);
        RectTransform backgroundRect = background.AddComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.sizeDelta = Vector2.zero;
        UnityEngine.UI.Image backgroundImage = background.AddComponent<UnityEngine.UI.Image>();
        backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        // Create progress bar
        GameObject progressBarObj = CreateGameObject("ProgressBar", loadingPanel.transform);
        RectTransform progressBarRect = progressBarObj.AddComponent<RectTransform>();
        progressBarRect.anchorMin = new Vector2(0.2f, 0.4f);
        progressBarRect.anchorMax = new Vector2(0.8f, 0.5f);
        progressBarRect.sizeDelta = Vector2.zero;
        UnityEngine.UI.Slider progressBar = progressBarObj.AddComponent<UnityEngine.UI.Slider>();
        
        // Create progress bar fill
        GameObject progressBarFill = CreateGameObject("Fill", progressBarObj.transform);
        RectTransform progressBarFillRect = progressBarFill.AddComponent<RectTransform>();
        progressBarFillRect.anchorMin = Vector2.zero;
        progressBarFillRect.anchorMax = Vector2.one;
        progressBarFillRect.sizeDelta = Vector2.zero;
        UnityEngine.UI.Image progressBarFillImage = progressBarFill.AddComponent<UnityEngine.UI.Image>();
        progressBarFillImage.color = new Color(0.2f, 0.7f, 0.9f);
        
        // Set fill for the progress bar
        progressBar.fillRect = progressBarFillRect;
        progressBar.targetGraphic = progressBarFillImage;
        progressBar.minValue = 0f;
        progressBar.maxValue = 1f;
        progressBar.value = 0f;
        
        // Create status text
        GameObject statusTextObj = CreateGameObject("StatusText", loadingPanel.transform);
        RectTransform statusTextRect = statusTextObj.AddComponent<RectTransform>();
        statusTextRect.anchorMin = new Vector2(0.2f, 0.55f);
        statusTextRect.anchorMax = new Vector2(0.8f, 0.65f);
        statusTextRect.sizeDelta = Vector2.zero;
        
        // Set up UI references
        loadingUIComponent.loadingPanel = loadingPanel;
        loadingUIComponent.progressBar = progressBar;
    }
    
    private static void SetupPlayer(GameObject parent)
    {
        // Create main camera
        GameObject mainCamera = CreateGameObject("Main Camera", parent.transform);
        Camera camera = mainCamera.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
        mainCamera.transform.position = new Vector3(0f, 1.8f, -10f);
        mainCamera.AddComponent<AudioListener>();
        
        // Create player controller placeholder
        GameObject playerController = CreateGameObject("PlayerController", parent.transform);
    }
    
    private static GameObject CreateGameObject(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name);
        if (parent != null)
        {
            gameObject.transform.SetParent(parent);
        }
        return gameObject;
    }
}
#endif