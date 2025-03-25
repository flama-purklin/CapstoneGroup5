using UnityEngine;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.SceneManagement;

/// <summary>
/// Utility to fix scene objects by attaching the correct components.
/// </summary>
public class SceneComponentFixer : MonoBehaviour
{
    [MenuItem("Mystery Engine/Fix Scene Components")]
    public static void FixSceneComponents()
    {
        // Get the current scene
        Scene currentScene = SceneManager.GetActiveScene();
        Debug.Log($"Fixing components in scene: {currentScene.name}");
        
        // WorldCoordinator
        GameObject worldCoordinatorObj = GameObject.Find("WorldCoordinator");
        if (worldCoordinatorObj != null)
        {
            Debug.Log("Found WorldCoordinator, adding component");
            Type worldCoordinatorType = GetTypeByName("MysteryEngine.WorldCoordinator");
            if (worldCoordinatorType != null)
            {
                AddOrReplaceComponent(worldCoordinatorObj, worldCoordinatorType);
            }
            else
            {
                Debug.LogError("Could not find WorldCoordinator type");
            }
        }
        
        // LocationRegistry
        GameObject locationRegistryObj = GameObject.Find("LocationRegistry");
        if (locationRegistryObj != null)
        {
            Debug.Log("Found LocationRegistry, adding component");
            Type locationRegistryType = GetTypeByName("MysteryEngine.LocationRegistry");
            if (locationRegistryType != null)
            {
                AddOrReplaceComponent(locationRegistryObj, locationRegistryType);
            }
            else
            {
                Debug.LogError("Could not find LocationRegistry type");
            }
        }
        
        // TrainGenerator
        GameObject trainGeneratorObj = GameObject.Find("TrainGenerator");
        if (trainGeneratorObj != null)
        {
            Debug.Log("Found TrainGenerator, adding component");
            Type trainGeneratorType = GetTypeByName("MysteryEngine.TrainGenerator");
            if (trainGeneratorType != null)
            {
                AddOrReplaceComponent(trainGeneratorObj, trainGeneratorType);
            }
            else
            {
                Debug.LogError("Could not find TrainGenerator type");
            }
        }
        
        // EntityPlacer
        GameObject entityPlacerObj = GameObject.Find("EntityPlacer");
        if (entityPlacerObj != null)
        {
            Debug.Log("Found EntityPlacer, adding component");
            Type entityPlacerType = GetTypeByName("MysteryEngine.EntityPlacer");
            if (entityPlacerType != null)
            {
                AddOrReplaceComponent(entityPlacerObj, entityPlacerType);
            }
            else
            {
                Debug.LogError("Could not find EntityPlacer type");
            }
        }
        
        // NPCManager
        GameObject npcManagerObj = GameObject.Find("NPCManager");
        if (npcManagerObj != null)
        {
            Debug.Log("Found NPCManager, adding component");
            Type npcManagerType = GetTypeByName("MysteryEngine.NPCManager");
            if (npcManagerType != null)
            {
                AddOrReplaceComponent(npcManagerObj, npcManagerType);
            }
            else
            {
                Debug.LogError("Could not find NPCManager type");
            }
        }
        
        // EvidenceManager
        GameObject evidenceManagerObj = GameObject.Find("EvidenceManager");
        if (evidenceManagerObj != null)
        {
            Debug.Log("Found EvidenceManager, adding component");
            Type evidenceManagerType = GetTypeByName("MysteryEngine.EvidenceManager");
            if (evidenceManagerType != null)
            {
                AddOrReplaceComponent(evidenceManagerObj, evidenceManagerType);
            }
            else
            {
                Debug.LogError("Could not find EvidenceManager type");
            }
        }
        
        // Set up component references
        SetupComponentReferences();
        
        // Save the scene
        EditorSceneManager.SaveScene(currentScene);
        
        Debug.Log("Scene component fixing complete!");
    }
    
    private static System.Type GetTypeByName(string typeName)
    {
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(typeName);
            if (type != null)
            {
                return type;
            }
        }
        
        return null;
    }
    
    private static void AddOrReplaceComponent(GameObject gameObject, System.Type componentType)
    {
        // Remove existing component
        Component existingComponent = gameObject.GetComponent(componentType);
        if (existingComponent != null)
        {
            DestroyImmediate(existingComponent);
        }
        
        // Add new component
        gameObject.AddComponent(componentType);
        Debug.Log($"Added component {componentType.Name} to {gameObject.name}");
    }
    
    private static void SetupComponentReferences()
    {
        // Get objects
        GameObject worldCoordinatorObj = GameObject.Find("WorldCoordinator");
        GameObject locationRegistryObj = GameObject.Find("LocationRegistry");
        GameObject trainGeneratorObj = GameObject.Find("TrainGenerator");
        GameObject entityPlacerObj = GameObject.Find("EntityPlacer");
        GameObject npcManagerObj = GameObject.Find("NPCManager");
        GameObject evidenceManagerObj = GameObject.Find("EvidenceManager");
        GameObject trainParentObj = GameObject.Find("TrainParent");
        
        if (worldCoordinatorObj == null || locationRegistryObj == null || 
            trainGeneratorObj == null || entityPlacerObj == null)
        {
            Debug.LogError("Missing required objects");
            return;
        }
        
        // Get components
        var worldCoordinator = worldCoordinatorObj.GetComponent(GetTypeByName("MysteryEngine.WorldCoordinator"));
        var locationRegistry = locationRegistryObj.GetComponent(GetTypeByName("MysteryEngine.LocationRegistry"));
        var trainGenerator = trainGeneratorObj.GetComponent(GetTypeByName("MysteryEngine.TrainGenerator"));
        var entityPlacer = entityPlacerObj.GetComponent(GetTypeByName("MysteryEngine.EntityPlacer"));
        var npcManager = npcManagerObj?.GetComponent(GetTypeByName("MysteryEngine.NPCManager"));
        var evidenceManager = evidenceManagerObj?.GetComponent(GetTypeByName("MysteryEngine.EvidenceManager"));
        
        if (worldCoordinator == null || locationRegistry == null || 
            trainGenerator == null || entityPlacer == null)
        {
            Debug.LogError("Missing required components");
            return;
        }
        
        // Set references via SerializedObject
        SerializedObject worldCoordinatorSO = new SerializedObject(worldCoordinator);
        SerializedProperty trainGeneratorProp = worldCoordinatorSO.FindProperty("trainGenerator");
        SerializedProperty entityPlacerProp = worldCoordinatorSO.FindProperty("entityPlacer");
        SerializedProperty locationRegistryProp = worldCoordinatorSO.FindProperty("locationRegistry");
        
        if (trainGeneratorProp != null && entityPlacerProp != null && locationRegistryProp != null)
        {
            trainGeneratorProp.objectReferenceValue = trainGenerator;
            entityPlacerProp.objectReferenceValue = entityPlacer;
            locationRegistryProp.objectReferenceValue = locationRegistry;
            worldCoordinatorSO.ApplyModifiedProperties();
        }
        
        // Set references for train generator
        if (trainParentObj != null)
        {
            SerializedObject trainGeneratorSO = new SerializedObject(trainGenerator);
            SerializedProperty trainParentProp = trainGeneratorSO.FindProperty("trainParent");
            
            if (trainParentProp != null)
            {
                trainParentProp.objectReferenceValue = trainParentObj.transform;
                trainGeneratorSO.ApplyModifiedProperties();
            }
        }
        
        // Set references for entity placer
        if (npcManager != null && evidenceManager != null)
        {
            SerializedObject entityPlacerSO = new SerializedObject(entityPlacer);
            SerializedProperty npcManagerProp = entityPlacerSO.FindProperty("npcManager");
            SerializedProperty evidenceManagerProp = entityPlacerSO.FindProperty("evidenceManager");
            
            if (npcManagerProp != null && evidenceManagerProp != null)
            {
                npcManagerProp.objectReferenceValue = npcManager;
                evidenceManagerProp.objectReferenceValue = evidenceManager;
                entityPlacerSO.ApplyModifiedProperties();
            }
        }
        
        Debug.Log("Component references set up");
    }
}
#endif