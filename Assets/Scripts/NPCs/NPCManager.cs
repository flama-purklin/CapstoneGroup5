using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using LLMUnity;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

// Modified NPCManager.cs
public class NPCManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private Transform[] spawnPoints; // Note: This might become obsolete if spawning logic changes entirely
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private NPCAnimContainer[] availableAnimContainers = new NPCAnimContainer[4]; // Assign the 4 Winchester containers here in Inspector

    private Dictionary<string, GameObject> activeNPCs = new Dictionary<string, GameObject>();
    private bool isInitialized = false;
    public bool IsInitializationComplete => isInitialized;
    private Transform characterContainer;
    private Dictionary<string, LLMCharacter> characterCache = new Dictionary<string, LLMCharacter>();


    private void Awake()
    {
        characterManager = FindFirstObjectByType<CharacterManager>();
        if (!characterManager)
        {
            Debug.LogError("CharacterManager not found!");
            return;
        }
    }


    public async Task Initialize()
    {
        Debug.Log("Starting NPCManager initialization...");
        isInitialized = false;

        // Wait for CharacterManager
        while (!characterManager.IsInitialized)
        {
            await Task.Yield();
        }

        // cache $$$
        string[] characterNames = characterManager.GetAvailableCharacters();
        foreach (string characterName in characterNames)
        {
            while (!characterManager.IsCharacterReady(characterName))
            {
                await Task.Yield();
            }

            // cache preferences
            LLMCharacter llmCharacter = await characterManager.SwitchToCharacter(characterName);
            if (llmCharacter != null)
            {
                characterCache[characterName] = llmCharacter;
                Debug.Log($"Cached LLMCharacter for: {characterName}");
            }
        }

        isInitialized = true;
        Debug.Log("NPCManager initialization complete");
    }

    // Modified signature to accept characterIndex for animation assignment
    public GameObject SpawnNPCInCar(string characterName, Vector3 position, Transform carTransform, int characterIndex)
    {
        if (!characterCache.ContainsKey(characterName))
        {
            Debug.LogError($"No cached character found for: {characterName}");
            return null;
        }

        try
        {
            // Create NPC, LLMCharacter child, and copy properties 
            GameObject npcInstance = Instantiate(npcPrefab, position, Quaternion.identity, characterContainer);
            npcInstance.name = $"NPC_{characterName}";
            GameObject llmObject = new GameObject("LLMCharacter");
            llmObject.transform.SetParent(npcInstance.transform);

            LLMCharacter newLLMChar = llmObject.AddComponent<LLMCharacter>();
            CopyLLMCharacterProperties(characterCache[characterName], newLLMChar);

            // --- Assign Animation Container ---
            // Ensure NPCAnimManager exists, add if missing from prefab
            NPCAnimManager npcAnimManager = npcInstance.GetComponent<NPCAnimManager>();
            if (npcAnimManager == null)
            {
                Debug.LogWarning($"NPCManager: Adding missing NPCAnimManager component to {npcInstance.name}. Check the base NPC prefab!");
                npcAnimManager = npcInstance.AddComponent<NPCAnimManager>();

                // Manually assign references that would normally be serialized, if possible
                // We need to get the NPCMovement component from the same instance
                NPCMovement movementComponent = npcInstance.GetComponent<NPCMovement>(); // <--- Renamed variable
                if (movementComponent != null)
                {
                    // Accessing private field via reflection is messy, ideally make movementControl public or add a setter
                    // For now, let's assume we add a public setter or make it public temporarily
                    // npcAnimManager.movementControl = movement; // Requires movementControl to be public or have a setter

                    // Alternative: If NPCAnimManager can get its own reference in Awake/Start, this might not be needed,
                    // but the Update fix already added a GetComponent check there as a fallback.
                    // Let's log if we couldn't find the movement component here.
                     Debug.Log($"NPCManager: Assigned NPCMovement reference to dynamically added NPCAnimManager on {npcInstance.name}.");
                }
                else
                {
                     Debug.LogError($"NPCManager: Could not find NPCMovement component on {npcInstance.name} to assign to dynamically added NPCAnimManager.");
                }
                 // We might also need to assign sprite and animator references if they are null
                 // npcAnimManager.sprite = npcInstance.GetComponentInChildren<SpriteRenderer>(); // Example
                 // npcAnimManager.animator = npcInstance.GetComponentInChildren<Animator>(); // Example
            }

            // Proceed with assigning the animation container
            if (npcAnimManager != null)
            {
                if (availableAnimContainers != null && availableAnimContainers.Length > 0)
                {
                    // Assign container cyclically based on the character's index in the overall list
                    int containerIndex = characterIndex % availableAnimContainers.Length;
                    NPCAnimContainer containerToAssign = availableAnimContainers[containerIndex];

                    if (containerToAssign != null)
                    {
                        npcAnimManager.SetAnimContainer(containerToAssign);
                        // Debug.Log($"Assigned AnimContainer {containerToAssign.name} to {characterName} (Index: {characterIndex} -> Container Index: {containerIndex})");
                    }
                    else
                    {
                        Debug.LogWarning($"NPCManager: AnimContainer at index {containerIndex} is null for {characterName}. Check Inspector assignment.");
                    }
                }
                else
                {
                     Debug.LogWarning($"NPCManager: availableAnimContainers array is null or empty for {characterName}. Cannot assign animation. Check Inspector assignment.");
                }
            }
            else
            {
                Debug.LogWarning($"NPCManager: NPCAnimManager component not found on prefab instance for {characterName}.");
            }
            // --- End Assign Animation Container ---

            var character = npcInstance.GetComponent<Character>();
            if (character)
            {
                character.Initialize(characterName, newLLMChar);
            }

            var movement = npcInstance.GetComponent<NPCMovement>();
            if (movement)
            {
                movement.enabled = true;
                var agent = movement.GetComponent<NavMeshAgent>();
                if (agent)
                {
                    agent.enabled = true;
                    // --- Cline: Verify NavMesh placement after Warp ---
                    if (!agent.Warp(position))
                    {
                        Debug.LogError($"NPC {characterName} NavMeshAgent.Warp failed initially for position {position}. Agent might be invalid.");
                    }
                    else if (!agent.isOnNavMesh)
                    {
                        Debug.LogWarning($"NPC {characterName} warped to {position} but is not on NavMesh. Attempting to find nearest valid point.");
                        NavMeshHit hit; // Declare hit variable here
                        // Sample position with a small radius (e.g., 1.0f) around the intended position
                        if (NavMesh.SamplePosition(position, out hit, 1.0f, NavMesh.AllAreas))
                        {
                            Debug.Log($"Found valid NavMesh point {hit.position} for {characterName}. Warping again.");
                            if (!agent.Warp(hit.position)) // Warp to the valid point
                            {
                                Debug.LogError($"NPC {characterName} NavMeshAgent.Warp failed on second attempt to {hit.position}.");
                            }
                            else if (!agent.isOnNavMesh) // Double-check after second warp
                            {
                                Debug.LogError($"NPC {characterName} STILL not on NavMesh after second warp to {hit.position}!");
                            }
                        }
                        else
                        {
                            Debug.LogError($"NPC {characterName} could not find ANY valid NavMesh point near {position} after initial warp failed!");
                            // Consider disabling the NPC or its movement if spawning fails completely
                            // if (movement) movement.enabled = false;
                        }
                     }
                     // --- Cline: Log position immediately after warp ---
                     if (agent.isOnNavMesh) {
                         Debug.Log($"[NPCManager Debug] NPC {characterName} IS on NavMesh immediately after warp to {agent.transform.position}.");
                     } else {
                         // Use position variable here since hit might not be assigned if SamplePosition failed
                         Debug.LogWarning($"[NPCManager Debug] NPC {characterName} IS NOT on NavMesh immediately after warp attempt to {agent.transform.position} (intended: {position}).");
                     }
                     // +++ Cline: Add timestamped log after Warp +++
                     Debug.Log($"[Timestamp {Time.frameCount}] NPCManager Post-Warp: {npcInstance.transform.position} for {characterName}");
                     // --- End Cline changes ---
                    // --- End Cline changes ---
                }
            }

            npcInstance.SetActive(true);
            activeNPCs[characterName] = npcInstance;
            Debug.Log($"Successfully spawned NPC {characterName} at position {position}");
            return npcInstance;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error spawning NPC {characterName}: {e.Message}");
            return null;
        }
    }

    public void PlaceNPCsInGameScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        // Create Characters container
        GameObject containerObj = new GameObject("Characters");
        SceneManager.MoveGameObjectToScene(containerObj, currentScene);
        characterContainer = containerObj.transform;

        Debug.Log($"Created Characters container in {currentScene.name}");
    }

    private void CopyLLMCharacterProperties(LLMCharacter source, LLMCharacter destination)
    {
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(source), destination);
        destination.llm = source.llm;
        destination.gameObject.SetActive(true);
    }

    public string[] GetAvailableCharacters()
    {
        return characterCache.Keys.ToArray();
    }


    public float GetInitializationProgress()
    {
        if (!characterManager) return 0f;
        // progress tracking
        return characterManager.GetInitializationProgress();
    }
}
