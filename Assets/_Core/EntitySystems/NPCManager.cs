using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using LLMUnity;
using UnityEngine.AI;
using System.Linq;

/// <summary>
/// Manages NPC spawning and character data integration.
/// Refactored to work with the new architecture.
/// </summary>
public class NPCManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private Transform characterContainer;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    // Runtime data
    private Dictionary<string, GameObject> activeNPCs = new Dictionary<string, GameObject>();
    private Dictionary<string, LLMCharacter> characterCache = new Dictionary<string, LLMCharacter>();
    private bool isInitialized = false;
    
    // Properties
    public bool IsInitializationComplete => isInitialized;
    
    private void Awake()
    {
        // Find character manager if not set
        if (characterManager == null)
        {
            characterManager = FindFirstObjectByType<CharacterManager>();
            if (characterManager == null)
            {
                Debug.LogError("CharacterManager not found! NPCManager will not work properly.");
                return;
            }
        }
        
        // Create character container if not set
        if (characterContainer == null)
        {
            GameObject containerObj = new GameObject("Characters");
            containerObj.transform.SetParent(transform);
            characterContainer = containerObj.transform;
            LogDebug("Created character container");
        }
    }
    
    /// <summary>
    /// Initializes the NPCManager by loading and caching all character data.
    /// </summary>
    public async Task Initialize()
    {
        LogDebug("Starting NPCManager initialization...");
        isInitialized = false;
        
        // Clear any existing NPCs
        ClearActiveNPCs();
        
        // Wait for CharacterManager to be initialized
        while (characterManager != null && !characterManager.IsInitialized)
        {
            await Task.Yield();
        }
        
        if (characterManager == null)
        {
            Debug.LogError("Cannot initialize NPCManager without CharacterManager");
            return;
        }
        
        // Cache character data for all available characters
        string[] characterNames = characterManager.GetAvailableCharacters();
        LogDebug($"Found {characterNames.Length} characters to cache");
        
        foreach (string characterName in characterNames)
        {
            // Wait for character to be ready
            while (!characterManager.IsCharacterReady(characterName))
            {
                await Task.Yield();
            }
            
            // Cache the character data
            LLMCharacter llmCharacter = await characterManager.SwitchToCharacter(characterName);
            if (llmCharacter != null)
            {
                characterCache[characterName] = llmCharacter;
                LogDebug($"Cached LLMCharacter for: {characterName}");
            }
            else
            {
                Debug.LogWarning($"Failed to cache character data for: {characterName}");
            }
        }
        
        isInitialized = true;
        LogDebug("NPCManager initialization complete");
    }
    
    /// <summary>
    /// Spawns an NPC in a specific location with a proper character reference.
    /// </summary>
    public GameObject SpawnNPCInCar(string characterName, Vector3 position, Transform parent = null)
    {
        if (string.IsNullOrEmpty(characterName))
        {
            Debug.LogError("Cannot spawn NPC with null or empty name");
            return null;
        }
        
        // Check if this character is already spawned
        if (activeNPCs.TryGetValue(characterName, out GameObject existingNPC) && existingNPC != null)
        {
            LogDebug($"Character {characterName} is already spawned. Moving to new position.");
            existingNPC.transform.position = position;
            
            if (parent != null && existingNPC.transform.parent != parent)
            {
                existingNPC.transform.SetParent(parent);
            }
            
            return existingNPC;
        }
        
        // Check if we have the character data cached
        if (!characterCache.TryGetValue(characterName, out LLMCharacter cachedCharacter))
        {
            Debug.LogError($"No cached character data found for: {characterName}");
            return null;
        }
        
        try
        {
            // Determine the parent transform
            Transform spawnParent = parent != null ? parent : characterContainer;
            
            // Create NPC instance
            GameObject npcInstance = Instantiate(npcPrefab, position, Quaternion.identity, spawnParent);
            npcInstance.name = $"NPC_{characterName}";
            
            // Create LLMCharacter child object
            GameObject llmObject = new GameObject("LLMCharacter");
            llmObject.transform.SetParent(npcInstance.transform);
            
            // Add and configure LLMCharacter component
            LLMCharacter newLLMChar = llmObject.AddComponent<LLMCharacter>();
            CopyLLMCharacterProperties(cachedCharacter, newLLMChar);
            
            // Initialize the Character component
            var character = npcInstance.GetComponent<Character>();
            if (character != null)
            {
                character.Initialize(characterName, newLLMChar);
            }
            else
            {
                Debug.LogError($"NPC prefab missing Character component!");
            }
            
            // Configure movement if available
            var movement = npcInstance.GetComponent<NPCMovement>();
            if (movement != null)
            {
                movement.enabled = true;
                
                // Set up NavMeshAgent if present
                var agent = movement.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    agent.enabled = true;
                    agent.Warp(position); // Ensure agent is at the correct position
                }
            }
            
            // Add an identifier component
            var identifier = npcInstance.AddComponent<NPCIdentifier>();
            identifier.CharacterName = characterName;
            
            // Store in active NPCs dictionary
            activeNPCs[characterName] = npcInstance;
            LogDebug($"Successfully spawned NPC {characterName} at position {position}");
            
            return npcInstance;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error spawning NPC {characterName}: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Copies properties from one LLMCharacter to another.
    /// </summary>
    private void CopyLLMCharacterProperties(LLMCharacter source, LLMCharacter destination)
    {
        // Use JsonUtility to do a deep copy of serializable fields
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(source), destination);
        
        // Copy reference to LLM
        destination.llm = source.llm;
        
        // Ensure the gameObject is active
        destination.gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Gets all available character names.
    /// </summary>
    public string[] GetAvailableCharacters()
    {
        // If we have cached characters, return those
        if (characterCache.Count > 0)
        {
            return characterCache.Keys.ToArray();
        }
        
        // Otherwise, ask the character manager
        if (characterManager != null && characterManager.IsInitialized)
        {
            return characterManager.GetAvailableCharacters();
        }
        
        return new string[0];
    }
    
    /// <summary>
    /// Gets a specific NPC instance by character name.
    /// </summary>
    public GameObject GetNPCByName(string characterName)
    {
        if (activeNPCs.TryGetValue(characterName, out GameObject npc))
        {
            return npc;
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets all active NPC instances.
    /// </summary>
    public List<GameObject> GetAllNPCs()
    {
        return new List<GameObject>(activeNPCs.Values);
    }
    
    /// <summary>
    /// Clears all active NPCs.
    /// </summary>
    public void ClearActiveNPCs()
    {
        foreach (var npc in activeNPCs.Values)
        {
            if (npc != null)
            {
                Destroy(npc);
            }
        }
        
        activeNPCs.Clear();
        LogDebug("Cleared all active NPCs");
    }
    
    /// <summary>
    /// Gets the initialization progress.
    /// </summary>
    public float GetInitializationProgress()
    {
        if (characterManager == null) return 0f;
        return characterManager.GetInitializationProgress();
    }
    
    private void LogDebug(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[NPCManager] {message}");
        }
    }
}