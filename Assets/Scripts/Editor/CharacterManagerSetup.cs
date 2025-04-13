using UnityEngine; 
using UnityEditor;
using LLMUnity;

// This script provides a menu item to automatically link essential manager components
// and set some default values on CharacterManager.
public class CharacterManagerSetup : EditorWindow
{
    [MenuItem("Tools/Setup Scene References & Defaults")] 
    static void Init()
    {
        // Find the core manager objects in the scene
        LLM llm = FindFirstObjectByType<LLM>(); 
        CharacterManager characterManager = FindFirstObjectByType<CharacterManager>();
        InitializationManager initManager = FindFirstObjectByType<InitializationManager>();
        GameObject loadingOverlay = GameObject.Find("LoadingOverlay"); 
        NPCManager npcManager = FindFirstObjectByType<NPCManager>();
        TrainLayoutManager trainLayoutManager = FindFirstObjectByType<TrainLayoutManager>(); 
        // SimpleProximityWarmup proximityWarmup = FindFirstObjectByType<SimpleProximityWarmup>(); // Removed due to persistent compile errors

        // Validate objects exist
        bool errorFound = false;
        if (llm == null) { Debug.LogError("LLM object not found in scene."); errorFound = true; }
        if (characterManager == null) { Debug.LogError("CharacterManager object not found in scene."); errorFound = true; }
        if (initManager == null) { Debug.LogError("InitializationManager object not found in scene."); errorFound = true; }
        if (loadingOverlay == null) { Debug.LogError("LoadingOverlay object not found in scene."); errorFound = true; }
        if (npcManager == null) { Debug.LogError("NPCManager object not found in scene."); errorFound = true; }
        if (trainLayoutManager == null) { Debug.LogError("TrainLayoutManager object not found in scene."); errorFound = true; }
        // if (proximityWarmup == null) { Debug.LogError("SimpleProximityWarmup component not found in scene."); errorFound = true; } // Removed check


        if (errorFound) {
             Debug.LogError("Setup failed: One or more required objects not found. Please ensure they exist in the scene with expected names/components.");
             return;
        }
        
        // --- Configure CharacterManager ---
        SerializedObject serializedCharManager = new SerializedObject(characterManager);
        serializedCharManager.FindProperty("sharedLLM").objectReferenceValue = llm;
        serializedCharManager.FindProperty("npcManager").objectReferenceValue = npcManager; 
        serializedCharManager.FindProperty("templateTimeout").floatValue = 15f;
        serializedCharManager.FindProperty("warmupTimeout").floatValue = 30f;
        serializedCharManager.FindProperty("maxWarmupAttempts").intValue = 3;
        serializedCharManager.FindProperty("baseBackoffDelay").floatValue = 1f;
        serializedCharManager.FindProperty("temperature").floatValue = 0.45f; 
        serializedCharManager.FindProperty("topK").intValue = 55;
        serializedCharManager.FindProperty("topP").floatValue = 0.95f;
        serializedCharManager.FindProperty("repeatPenalty").floatValue = 1f;
        serializedCharManager.FindProperty("presencePenalty").floatValue = 0f;
        serializedCharManager.FindProperty("frequencyPenalty").floatValue = 1f;
        serializedCharManager.ApplyModifiedProperties();
        Debug.Log("CharacterManager successfully configured with references and default parameters.");
        
        // --- Configure InitializationManager ---
        SerializedObject serializedInitManager = new SerializedObject(initManager);
        serializedInitManager.FindProperty("llm").objectReferenceValue = llm;
        serializedInitManager.FindProperty("characterManager").objectReferenceValue = characterManager;
        serializedInitManager.FindProperty("npcManager").objectReferenceValue = npcManager;
        serializedInitManager.FindProperty("loadingOverlay").objectReferenceValue = loadingOverlay;
        serializedInitManager.FindProperty("trainLayoutManager").objectReferenceValue = trainLayoutManager; 
        serializedInitManager.ApplyModifiedProperties();
        Debug.Log("InitializationManager successfully configured with references.");

        // --- Configure NPCManager ---
        SerializedObject serializedNPCManager = new SerializedObject(npcManager);
        serializedNPCManager.FindProperty("characterManager").objectReferenceValue = characterManager; 
        serializedNPCManager.ApplyModifiedProperties();
        Debug.Log("NPCManager successfully configured with CharacterManager reference.");

         // --- Configure SimpleProximityWarmup --- REMOVED ---
        // SerializedObject serializedProximity = new SerializedObject(proximityWarmup); 
        // serializedProximity.FindProperty("characterManager").objectReferenceValue = characterManager;
        // serializedProximity.FindProperty("npcManager").objectReferenceValue = npcManager;
        // serializedProximity.FindProperty("llmInstance").objectReferenceValue = llm;
        // serializedProximity.ApplyModifiedProperties();
        // Debug.Log("SimpleProximityWarmup successfully configured with references.");
        Debug.LogWarning("SimpleProximityWarmup configuration removed from setup script due to compile issues. Please assign its references manually in the Inspector.");


        // Mark scene dirty and save
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        
        Debug.Log("Scene setup complete! References configured (except SimpleProximityWarmup) and scene saved.");
    }
}
