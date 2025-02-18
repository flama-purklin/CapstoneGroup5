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
    [SerializeField] private float initialPlacementDelay = 1f;
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
            // Create NPC, LLMCharacter child, and copy properties 
            GameObject npcInstance = Instantiate(npcPrefab, position, Quaternion.identity, characterContainer);
            npcInstance.name = $"NPC_{characterName}";
            GameObject llmObject = new GameObject("LLMCharacter");
            llmObject.transform.SetParent(npcInstance.transform);

            LLMCharacter newLLMChar = llmObject.AddComponent<LLMCharacter>();
            CopyLLMCharacterProperties(characterCache[characterName], newLLMChar);

       
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
                    agent.Warp(position);
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