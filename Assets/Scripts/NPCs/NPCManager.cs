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
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private CharacterManager characterManager;

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

    public GameObject SpawnNPCInCar(string characterName, Vector3 position, Transform carTransform)
    {
        if (!characterCache.ContainsKey(characterName))
        {
            Debug.LogError($"No cached character found for: {characterName}");
            return null;
        }

        try
        {
            // First check if this character already exists in the scene
            var existingCharacters = GameObject.FindObjectsByType<Character>(FindObjectsSortMode.None);
            foreach (var existingChar in existingCharacters)
            {
                if (existingChar != null && existingChar.CharacterName == characterName)
                {
                    Debug.LogWarning($"Character {characterName} already exists in scene - not spawning duplicate");
                    return existingChar.gameObject;
                }
            }

            // Make sure character container exists
            if (characterContainer == null)
            {
                PlaceNPCsInGameScene();
                if (characterContainer == null)
                {
                    Debug.LogError("Failed to create character container!");
                    return null;
                }
            }

            // Create NPC parent with explicit character name
            GameObject npcParent = new GameObject($"NPC_{characterName}");
            npcParent.transform.SetParent(characterContainer);
            npcParent.transform.position = position;
            
            // Then instantiate the NPC prefab as a child
            GameObject npcInstance = Instantiate(npcPrefab, position, Quaternion.identity, npcParent.transform);
            npcInstance.name = $"CharacterBody_{characterName}";
            Debug.Log($"Created NPC hierarchy with parent: NPC_{characterName}");
            
            // Create and add LLMCharacter
            GameObject llmObject = new GameObject("LLMCharacter");
            llmObject.transform.SetParent(npcInstance.transform);

            LLMCharacter newLLMChar = llmObject.AddComponent<LLMCharacter>();
            CopyLLMCharacterProperties(characterCache[characterName], newLLMChar);

            // Set the character name before NPCAnimManager processes it
            var character = npcInstance.GetComponent<Character>();
            if (character)
            {
                character.Initialize(characterName, newLLMChar);
                // Don't try to use tags as they may not be defined in project settings
                Debug.Log($"Initialized Character component with name: {characterName}");
            }
            else
            {
                Debug.LogError($"Character component not found on {npcInstance.name}!");
                Destroy(npcInstance);
                return null;
            }

            // Ensure proper placement on NavMesh
            var agent = npcInstance.GetComponentInChildren<NavMeshAgent>();
            if (agent == null)
            {
                Debug.LogError($"NavMeshAgent component not found on {npcInstance.name}!");
            }
            else
            {
                // Make sure agent is enabled
                agent.enabled = true;
                
                // Find valid NavMesh position with expanded search range
                NavMeshHit hit;
                float searchRadius = 10.0f; // Increased from 5.0f
                
                // Try multiple times with increasing radius if needed
                bool foundPosition = false;
                for (int attempt = 0; attempt < 3 && !foundPosition; attempt++)
                {
                    if (NavMesh.SamplePosition(position, out hit, searchRadius, NavMesh.AllAreas))
                    {
                        // Warp to valid position
                        agent.Warp(hit.position);
                        Debug.Log($"Placed {characterName} on NavMesh at {hit.position}");
                        foundPosition = true;
                    }
                    else
                    {
                        // Increase search radius for next attempt
                        searchRadius *= 2;
                        Debug.LogWarning($"NavMesh position search attempt {attempt+1} failed for {characterName}. Increasing radius to {searchRadius}");
                    }
                }
                
                if (!foundPosition)
                {
                    Debug.LogError($"Failed to find valid NavMesh position for {characterName} after multiple attempts!");
                }
            }
            
            // Enable NPCMovement component
            var movement = npcInstance.GetComponentInChildren<NPCMovement>();
            if (movement)
            {
                movement.enabled = true;
            }
            
            // Make sure the NPCAnimManager gets a chance to process it with the right name
            var animManager = npcInstance.GetComponentInChildren<NPCAnimManager>();
            if (animManager)
            {
                // Force a refresh of the animation assignments if needed
                StartCoroutine(DelayedAnimContainerAssign(animManager, 0.1f));
            }

            npcInstance.SetActive(true);
            activeNPCs[characterName] = npcInstance;
            Debug.Log($"Successfully spawned NPC {characterName} at position {position}");
            return npcInstance;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error spawning NPC {characterName}: {e.Message}\n{e.StackTrace}");
            return null;
        }
    }
    
    private IEnumerator DelayedAnimContainerAssign(NPCAnimManager animManager, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        var animContainerAssignMethod = animManager.GetType().GetMethod("AnimContainerAssign", 
            System.Reflection.BindingFlags.Instance | 
            System.Reflection.BindingFlags.NonPublic);
            
        if (animContainerAssignMethod != null)
        {
            animContainerAssignMethod.Invoke(animManager, null);
            Debug.Log($"Forced AnimContainerAssign refresh on {animManager.transform.parent.name}");
        }
    }

    public void PlaceNPCsInGameScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        // Check if container already exists
        if (characterContainer != null)
        {
            Debug.Log("Character container already exists, using existing container");
            return;
        }
        
        // Find existing container if it exists
        GameObject existingContainer = GameObject.Find("Characters");
        if (existingContainer != null)
        {
            characterContainer = existingContainer.transform;
            Debug.Log($"Found existing Characters container in {currentScene.name}");
            return;
        }

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