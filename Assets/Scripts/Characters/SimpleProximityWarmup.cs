using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LLMUnity;
using System.Linq;

/// <summary>
/// Manages which characters stay warm (loaded in memory) based on proximity to the player
/// This reduces memory usage and improves performance by only keeping nearby characters active
/// </summary>
public class SimpleProximityWarmup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private NPCManager npcManager;
    [SerializeField] private Transform player;
    [SerializeField] private LLM llmInstance;
    
    [Header("Configuration")]
    [Range(1, 10)]
    [SerializeField] private int maxWarmCharacters = 3;
    [SerializeField] private float checkInterval = 5f;
    [SerializeField] private float significantMovementThreshold = 5f;
    
    // State tracking
    private float lastCheckTime = 0f;
    private Vector3 lastPlayerPosition;
    private Dictionary<string, GameObject> cachedNPCs = new Dictionary<string, GameObject>();
    
    private void Start()
    {
        // Find references if not set in inspector
        if (characterManager == null)
            characterManager = FindFirstObjectByType<CharacterManager>();
            
        if (npcManager == null)
            npcManager = FindFirstObjectByType<NPCManager>();
            
        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;
            
        if (llmInstance == null)
            llmInstance = FindFirstObjectByType<LLM>();
            
        // Validate required references
        if (characterManager == null || npcManager == null || player == null || llmInstance == null)
        {
            Debug.LogError("SimpleProximityWarmup: Missing required references. Disabling component.");
            enabled = false;
            return;
        }
        
        // Note: parallelPrompts must be set in the Inspector on the LLM component before Play
        // Attempts to set it at runtime are ineffective as the server has already started
        
        lastPlayerPosition = player.position;
        
        // Perform initial warmup after a short delay to ensure all systems are initialized
        Invoke(nameof(UpdateWarmupState), 2f);
    }
    
    private void Update()
    {
        // Only run if initialization is complete
        if (!characterManager.IsInitialized || !npcManager.IsInitializationComplete)
            return;
            
        bool shouldCheck = false;
        
        // Check if player has moved significantly
        if (Vector3.Distance(lastPlayerPosition, player.position) > significantMovementThreshold)
        {
            lastPlayerPosition = player.position;
            shouldCheck = true;
        }
        // Or if it's time for a periodic check
        else if (Time.time - lastCheckTime > checkInterval)
        {
            shouldCheck = true;
        }
        
        if (shouldCheck)
        {
            lastCheckTime = Time.time;
            UpdateWarmupState();
        }
    }
    
    private void UpdateWarmupState()
    {
        if (!characterManager.IsInitialized || !npcManager.IsInitializationComplete)
            return;
            
        // Get all NPC references from NPCManager
        RefreshNPCCache();
        
        var characterDistances = new List<(string name, float distance)>();
        
        // Calculate distances to all characters
        foreach (string characterName in characterManager.GetAvailableCharacters())
        {
            if (!cachedNPCs.TryGetValue(characterName, out GameObject npcObject) || npcObject == null)
            {
                // Skip characters that don't have an NPC object (unusual, but handle gracefully)
                Debug.LogWarning($"Character {characterName} doesn't have a matching NPC GameObject in scene.");
                continue;
            }
            
            // Calculate distance from player to this NPC
            float distance = Vector3.Distance(player.position, npcObject.transform.position);
            characterDistances.Add((characterName, distance));
        }
        
        // Sort by distance (closest first)
        characterDistances.Sort((a, b) => a.distance.CompareTo(b.distance));
        
        Debug.Log($"Updating character warmup states based on proximity. Found {characterDistances.Count} characters.");
        
        // Process warming/cooling
        for (int i = 0; i < characterDistances.Count; i++)
        {
            string name = characterDistances[i].name;
            bool shouldBeWarm = i < maxWarmCharacters;
            var currentState = characterManager.GetCharacterState(name);
            
            if (shouldBeWarm && currentState == CharacterManager.CharacterState.LoadingTemplate)
            {
                Debug.Log($"Warming up nearby character: {name} (distance: {characterDistances[i].distance:F1})");
                StartCoroutine(characterManager.WarmupCharacter(name));
            }
            else if (!shouldBeWarm && currentState == CharacterManager.CharacterState.Ready)
            {
                Debug.Log($"Cooling down distant character: {name} (distance: {characterDistances[i].distance:F1})");
                characterManager.CooldownCharacter(name);
            }
        }
    }
    
    private void RefreshNPCCache()
    {
        cachedNPCs.Clear();
        
        // Get character names from existing NPCs in the scene
        var activeNPCs = FindObjectsByType<Character>(FindObjectsSortMode.None);
        foreach (var npc in activeNPCs)
        {
            string characterName = npc.GetCharacterName();
            if (!string.IsNullOrEmpty(characterName))
            {
                cachedNPCs[characterName] = npc.gameObject;
            }
        }
        
        Debug.Log($"NPC cache refreshed. Found {cachedNPCs.Count} active NPCs.");
    }
}
