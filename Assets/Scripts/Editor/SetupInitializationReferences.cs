using UnityEngine;
using UnityEditor;
using LLMUnity;
// using CoreControl.MysteryParsing; // Removed as MysteryCharacterExtractor is no longer used

/// <summary>
/// Editor tool to set up all the references needed for the unified scene approach
/// </summary>
[ExecuteInEditMode]
public class SetupInitializationReferences : Editor
{
    [MenuItem("Tools/Setup Scene References")]
    public static void SetupReferences()
    {
        // Find all required components
        InitializationManager initManager = FindFirstObjectByType<InitializationManager>();
        if (initManager == null)
        {
            Debug.LogError("InitializationManager not found in scene!");
            return;
        }

        LLM llm = FindFirstObjectByType<LLM>();
        CharacterManager characterManager = FindFirstObjectByType<CharacterManager>();
        NPCManager npcManager = FindFirstObjectByType<NPCManager>();
        GameObject loadingOverlay = GameObject.Find("LoadingOverlay");

        // Create any missing components
        if (npcManager == null)
        {
            GameObject npcObj = new GameObject("NPCManager");
            npcManager = npcObj.AddComponent<NPCManager>();
            
        }

        // Removed logic to create MysteryCharacterExtractor

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
        if (loadingOverlayProp != null && loadingOverlay != null) loadingOverlayProp.objectReferenceValue = loadingOverlay;
        
        // Apply changes
        serializedObject.ApplyModifiedProperties();
        
        // Configure CharacterManager with LLM reference if needed
        if (characterManager != null && llm != null)
        {
            SerializedObject charManagerObj = new SerializedObject(characterManager);
            SerializedProperty sharedLLMProp = charManagerObj.FindProperty("sharedLLM");
            if (sharedLLMProp != null) sharedLLMProp.objectReferenceValue = llm;
            charManagerObj.ApplyModifiedProperties();
        }
        
        
        EditorUtility.SetDirty(initManager);
        if (characterManager != null) EditorUtility.SetDirty(characterManager);
        if (npcManager != null) EditorUtility.SetDirty(npcManager);
    }
}
