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

        // Find CharacterManager
        while (characterManager == null)
        {
            // Actually try and find reference to the object, instead of waiting with no link check (was previously only checking on Awake)
            characterManager = FindFirstObjectByType<CharacterManager>();
            if (characterManager == null)
            {
                await Task.Yield();
            }
        }

        // Wait for CharacterManager to initialize
        while (!characterManager.IsInitialized)
        {
            await Task.Yield();
        }
        
        // FIXED: No longer waiting for characters to be in Ready state
        // Instead directly getting references from CharacterManager regardless of their state
        
        // cache character references
        string[] characterNames = characterManager.GetAvailableCharacters();
        Debug.Log($"Found {characterNames.Length} characters to cache in NPCManager");
        
        foreach (string characterName in characterNames)
        {
            // Get character reference directly without waiting for Ready state
            LLMCharacter llmCharacterRef = characterManager.GetCharacterByName(characterName);
            
            if (llmCharacterRef != null)
            {
                // Populate NPCManager's local cache
                characterCache[characterName] = llmCharacterRef;
                Debug.Log($"Cached reference for {characterName} in NPCManager");
            }
            else
            {
                Debug.LogWarning($"Could not get reference for {characterName} from CharacterManager");
            }
        }

        isInitialized = true;
        Debug.Log($"NPCManager initialization complete - cached {characterCache.Count} characters");
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
            // Debug.Log($"Assigning LLM {newLLMChar.llm.name} to {newLLMChar.GetType().FullName} {newLLMChar.name} from scene {gameObject.scene.name}");
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
                    agent.enabled = false; // Disable agent
                    npcInstance.transform.position = position; // Set position directly
                    agent.enabled = true; // Re-enable agent

                    if (!agent.isOnNavMesh)
                    {
                        Debug.LogWarning($"NPC {characterName} placed at {position} but is not on NavMesh after re-enabling agent. Attempting to find nearest valid point.");
                        NavMeshHit hit;
                        if (NavMesh.SamplePosition(position, out hit, 1.0f, NavMesh.AllAreas))
                        {
                            Debug.Log($"Found valid NavMesh point {hit.position} for {characterName}. Setting position again.");
                            agent.enabled = false;
                            npcInstance.transform.position = hit.position; // Set to the valid point
                            agent.enabled = true;
                            if (!agent.isOnNavMesh)
                            {
                                Debug.LogError($"NPC {characterName} STILL not on NavMesh after setting position to valid point {hit.position}!");
                            }
                        }
                        else
                        {
                            Debug.LogError($"NPC {characterName} could not find ANY valid NavMesh point near {position} after initial placement failed!");
                        }
                     }
                     if (agent.isOnNavMesh) {
                         
                     } else {
                         // Use position variable here since hit might not be assigned if SamplePosition failed
                          Debug.LogWarning($"[NPCManager Debug] NPC {characterName} IS NOT on NavMesh immediately after warp attempt to {agent.transform.position} (intended: {position}).");
                      }
                 }
            }

            npcInstance.SetActive(true);
            activeNPCs[characterName] = npcInstance;
            
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
