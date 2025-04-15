using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using LLMUnity;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;
using System; // Added for Exception

// Modified NPCManager.cs
public class NPCManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private CharacterManager characterManager; // Reference needed by InitializationManager to link Character component
    [SerializeField] private NPCAnimContainer[] availableAnimContainers = new NPCAnimContainer[4]; 

    private Dictionary<string, GameObject> activeNPCs = new Dictionary<string, GameObject>();
    private bool isInitialized = false; // Tracks if Awake setup is done
    public bool IsInitializationComplete => isInitialized; 
    public bool SpawningComplete { get; set; } = false; // Flag set by InitializationManager after all NPCs are spawned
    private Transform npcContainerTransform; 
    
    private void Awake()
    {
        // Get references early
        if (characterManager == null) characterManager = FindFirstObjectByType<CharacterManager>();
        if (!characterManager) { Debug.LogError("NPCManager: CharacterManager not found!"); enabled = false; return; }
        
        // Create container for NPC instances
        npcContainerTransform = transform.Find("NPCInstances");
        if (npcContainerTransform == null) {
            npcContainerTransform = new GameObject("NPCInstances").transform;
            npcContainerTransform.SetParent(transform);
        }
        isInitialized = true; 
    }

    // Initialize method removed 

    // Spawns a single NPC instance using the prefab.
    // Does NOT initialize the Character component here; InitializationManager handles linking later.
    // Called by InitializationManager.
    public GameObject SpawnNPCInCar(string characterName, Vector3 position, Transform carTransform, int characterIndex)
    {
        try
        {
            if (npcPrefab == null) { Debug.LogError("NPCManager: npcPrefab is not assigned!"); return null; }
            
            GameObject npcInstance = Instantiate(npcPrefab, position, Quaternion.identity, npcContainerTransform); 
            npcInstance.name = $"NPC_{characterName}";

            // --- LLMCharacter Child should NOT be on the prefab ---

            // --- Assign Animation Container ---
            NPCAnimManager npcAnimManager = npcInstance.GetComponentInChildren<NPCAnimManager>();
            if (npcAnimManager == null) {
                Debug.LogWarning($"NPCManager: Adding missing NPCAnimManager to {npcInstance.name}. Check prefab!");
                npcAnimManager = npcInstance.AddComponent<NPCAnimManager>();
            }
            if (npcAnimManager != null) {
                if (availableAnimContainers != null && availableAnimContainers.Length > 0) {
                    int containerIndex = characterIndex % availableAnimContainers.Length;
                    //NPCAnimContainer containerToAssign = availableAnimContainers[containerIndex];
                    NPCAnimContainer containerToAssign = RetrieveCharacter(characterName);
                    if (containerToAssign != null) { npcAnimManager.SetAnimContainer(containerToAssign); }
                    else { Debug.LogWarning($"NPCManager: AnimContainer at index {containerIndex} is null for {characterName}."); }
                } else { Debug.LogWarning($"NPCManager: availableAnimContainers array is null or empty for {characterName}."); }
            }
            // --- End Assign Animation Container ---

            // --- Character Component Initialization REMOVED ---
            // InitializationManager will call character.Initialize(name, llmRef) later.
            // var character = npcInstance.GetComponent<Character>();
            // if (character) {
            //      // character.Initialize(characterName); // REMOVED - Incorrect signature and wrong place
            // } else { Debug.LogError($"NPCManager: Missing Character component on {npcInstance.name}."); }

            // Initialize Movement and NavMeshAgent
            var movement = npcInstance.GetComponent<NPCMovement>();
            var agent = npcInstance.GetComponent<NavMeshAgent>();
            if (movement && agent) {
                agent.enabled = false; 
                npcInstance.transform.position = position; 
                agent.enabled = true; 
                if (!agent.isOnNavMesh) {
                    Debug.LogWarning($"NPC {characterName} at {position} not on NavMesh. Warping...");
                    if (!agent.Warp(position)) Debug.LogError($"NPC {characterName} warp failed!");
                }
                movement.enabled = true; 
            } else {
                 if (!movement) Debug.LogError($"NPCManager: Missing NPCMovement on {npcInstance.name}.");
                 if (!agent) Debug.LogError($"NPCManager: Missing NavMeshAgent on {npcInstance.name}.");
            }

            npcInstance.SetActive(true);
            activeNPCs[characterName] = npcInstance; 
            
            return npcInstance;
        }
        catch (Exception e) { 
            Debug.LogError($"Error spawning NPC {characterName}: {e.Message}\nStack Trace: {e.StackTrace}");
            return null;
        }
    }

    // Method for other systems to get all spawned NPC instances
    public Dictionary<string, GameObject> GetAllNPCInstances()
    {
        return activeNPCs;
    }

    // GetAvailableCharacters now returns keys from activeNPCs
    public string[] GetAvailableCharacters() { return activeNPCs.Keys.ToArray(); }
    public GameObject GetNPCByName(string name) { activeNPCs.TryGetValue(name, out GameObject npc); return npc; }

    public float GetInitializationProgress() { return isInitialized ? 1.0f : 0.0f; }
    
    public NPCAnimContainer RetrieveCharacter(string charId)
    {
        Debug.Log("Attempting to retrieve anims for " + charId);
        switch (charId)
        {
            case "maxwell_porter": return availableAnimContainers[5];
            case "gregory_crowe": return availableAnimContainers[6];
            case "victoria_blackwood": return availableAnimContainers[0];
            case "eleanor_verne": return availableAnimContainers[1];
            case "nova_winchester": return availableAnimContainers[2];
            case "penelope_valor": return availableAnimContainers[3];
            case "gideon_marsh": return availableAnimContainers[7];
            case "mira_sanchez": return availableAnimContainers[4];
            case "timmy_seol": return availableAnimContainers[8];
            default: return null;
        }
    }
}
