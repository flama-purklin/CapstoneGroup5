using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using LLMUnity;

/// <summary>
/// Manages NPC creation, initialization, and behavior.
/// Because herding cats would be easier than managing NPCs.
/// </summary>
public class NPCManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private CharacterManager characterManager;
    
    [Header("Settings")]
    [SerializeField] private bool debugMode = false;
    
    private Dictionary<string, GameObject> activeNPCs = new Dictionary<string, GameObject>();
    private bool isInitialized = false;
    
    // Public properties
    public bool IsInitializationComplete => isInitialized;
    
    private void Awake()
    {
        // Find CharacterManager if not already assigned
        if (characterManager == null)
        {
            characterManager = FindFirstObjectByType<CharacterManager>();
            if (characterManager == null)
            {
                Debug.LogError("CharacterManager not found! NPCs will be very boring without personalities.");
            }
        }
        
        // Validate NPC prefab
        if (npcPrefab == null)
        {
            Debug.LogError("NPC prefab not assigned! We can't spawn invisible people... well, we could, but that's a different game.");
        }
    }
    
    /// <summary>
    /// Initializes the NPCManager and waits for the CharacterManager to be ready.
    /// </summary>
    public async Task Initialize()
    {
        if (isInitialized)
        {
            LogDebug("Already initialized. Nothing to do here. Move along.");
            return;
        }
        
        LogDebug("Starting NPCManager initialization...");
        
        if (characterManager == null)
        {
            Debug.LogError("Cannot initialize NPCManager: CharacterManager not found. NPCs will have no soul.");
            return;
        }
        
        // Wait for CharacterManager to finish initializing
        while (!characterManager.IsInitialized)
        {
            LogDebug("Waiting for CharacterManager to initialize...");
            await Task.Delay(100);
        }
        
        isInitialized = true;
        LogDebug("NPCManager initialization complete. NPCs are ready to be given life.");
    }
    
    /// <summary>
    /// Spawns an NPC at the specified location.
    /// It's like playing god, but on a much smaller scale.
    /// </summary>
    public GameObject SpawnNPCInLocation(string characterName, Vector3 position, Transform parent)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("NPCManager not initialized. Attempting to initialize now, but this might go poorly.");
            _ = Initialize();
        }
        
        // Check if this character is already spawned
        if (activeNPCs.TryGetValue(characterName, out GameObject existingNPC))
        {
            Debug.LogWarning($"Character {characterName} is already spawned. Returning existing instance because we're not in the business of cloning.");
            return existingNPC;
        }
        
        if (npcPrefab == null)
        {
            Debug.LogError("Cannot spawn NPC: npcPrefab is null. Someone forgot to assign references.");
            return null;
        }
        
        // Create NPC GameObject
        GameObject npcInstance = Instantiate(npcPrefab, position, Quaternion.identity, parent);
        npcInstance.name = $"NPC_{characterName}";
        
        // Get LLM character from manager
        LLMCharacter llmCharacter = null;
        if (characterManager != null)
        {
            llmCharacter = characterManager.GetCharacterByName(characterName);
            
            if (llmCharacter == null)
            {
                Debug.LogError($"Failed to get LLMCharacter for {characterName}. This NPC will be very quiet.");
            }
        }
        
        // Initialize character component
        Character character = npcInstance.GetComponent<Character>();
        if (character != null)
        {
            character.Initialize(characterName, llmCharacter);
        }
        else
        {
            Debug.LogError($"NPC prefab does not have Character component. {characterName} will be an empty husk.");
        }
        
        // Enable movement component
        NPCMovement movement = npcInstance.GetComponent<NPCMovement>();
        if (movement != null)
        {
            movement.enabled = true;
        }
        
        // Store in active NPCs dictionary
        activeNPCs[characterName] = npcInstance;
        
        LogDebug($"Spawned NPC {characterName} at position {position}");
        return npcInstance;
    }
    
    /// <summary>
    /// Moves an NPC to a new location.
    /// Because sometimes NPCs need a change of scenery too.
    /// </summary>
    public bool MoveNPCToLocation(string characterName, Vector3 position, Transform parent)
    {
        if (!activeNPCs.TryGetValue(characterName, out GameObject npc))
        {
            Debug.LogError($"Cannot move {characterName}: NPC not found. Did they leave the train?");
            return false;
        }
        
        // Update position and parent
        npc.transform.position = position;
        npc.transform.SetParent(parent);
        
        LogDebug($"Moved NPC {characterName} to position {position}");
        return true;
    }
    
    /// <summary>
    /// Gets all active NPCs.
    /// For when you need to know who's still alive in your mystery.
    /// </summary>
    public GameObject[] GetAllNPCs()
    {
        return activeNPCs.Values.ToArray();
    }
    
    /// <summary>
    /// Gets an NPC by name.
    /// Like an extremely specific PA announcement.
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
    /// Gets the names of all available characters that can be spawned.
    /// The character selection screen of our little game.
    /// </summary>
    public string[] GetAvailableCharacters()
    {
        if (characterManager != null)
        {
            return characterManager.GetAvailableCharacters();
        }
        
        Debug.LogError("Cannot get available characters: CharacterManager is missing. The game is short on actors.");
        return new string[0];
    }
    
    private void LogDebug(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[NPCManager] {message}");
        }
    }
}
