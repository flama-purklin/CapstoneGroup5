using UnityEngine;
using UnityEditor;
using LLMUnity;

public class CharacterManagerSetup : EditorWindow
{
    [MenuItem("Tools/Setup Character Manager")]
    static void Init()
    {
        // Find the objects
        LLM llm = GameObject.Find("LLM")?.GetComponent<LLM>();
        CharacterManager characterManager = GameObject.Find("CharacterManager")?.GetComponent<CharacterManager>();
        InitializationManager initManager = GameObject.Find("InitializationManager")?.GetComponent<InitializationManager>();
        GameObject loadingOverlay = GameObject.Find("LoadingOverlay");
        NPCManager npcManager = FindFirstObjectByType<NPCManager>();
        
        // Validate objects exist
        if (llm == null)
        {
            Debug.LogError("LLM object not found in scene. Please add an LLM object first.");
            return;
        }
        
        if (characterManager == null)
        {
            Debug.LogError("CharacterManager not found in scene. Please add a CharacterManager object first.");
            return;
        }
        
        if (initManager == null)
        {
            Debug.LogError("InitializationManager not found in scene.");
            return;
        }
        
        if (loadingOverlay == null)
        {
            Debug.LogError("LoadingOverlay not found in scene.");
            return;
        }
        
        // Configure the CharacterManager
        characterManager.charactersFolder = "Characters";
        characterManager.sharedLLM = llm;
        characterManager.characterInitDelay = 2f;
        characterManager.templateTimeout = 15f;
        characterManager.warmupTimeout = 30f;
        characterManager.maxWarmupAttempts = 3;
        characterManager.baseBackoffDelay = 1f;
        characterManager.temperature = 0.45f;
        characterManager.topK = 55;
        characterManager.topP = 0.95f;
        characterManager.repeatPenalty = 1f;
        characterManager.presencePenalty = 0f;
        characterManager.frequencyPenalty = 1f;
        
        Debug.Log("CharacterManager successfully configured with LLM reference.");
        
        // Configure InitializationManager using SerializedObject
        SerializedObject serializedObject = new SerializedObject(initManager);
        
        // Set the references using SerializedProperty
        SerializedProperty llmProp = serializedObject.FindProperty("llm");
        SerializedProperty characterManagerProp = serializedObject.FindProperty("characterManager");
        SerializedProperty npcManagerProp = serializedObject.FindProperty("npcManager");
        SerializedProperty loadingOverlayProp = serializedObject.FindProperty("loadingOverlay");
        
        if (llmProp != null) llmProp.objectReferenceValue = llm;
        if (characterManagerProp != null) characterManagerProp.objectReferenceValue = characterManager;
        if (npcManagerProp != null) npcManagerProp.objectReferenceValue = npcManager;
        if (loadingOverlayProp != null) loadingOverlayProp.objectReferenceValue = loadingOverlay;
        
        // Apply changes
        serializedObject.ApplyModifiedProperties();
        
        Debug.Log("InitializationManager successfully configured with references.");
        
        // Save the scene
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        
        Debug.Log("Scene setup complete! All references configured.");
    }
}