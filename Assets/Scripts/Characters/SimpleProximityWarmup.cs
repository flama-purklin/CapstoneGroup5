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
    [SerializeField] private NPCManager npcManager; // Keep reference even if not used directly in RefreshNPCCache for now
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
        // This is the value that determines how many characters can be "warm" simultaneously
        // Make sure maxWarmCharacters doesn't exceed the parallelPrompts setting or characters may be dropped

        // Log important configuration values
        Debug.Log($"SimpleProximityWarmup initialized with: maxWarmCharacters={maxWarmCharacters}, LLM parallelPrompts={llmInstance.parallelPrompts}");
        if (maxWarmCharacters > llmInstance.parallelPrompts)
        {
            Debug.LogWarning($"WARNING: maxWarmCharacters ({maxWarmCharacters}) is set higher than LLM parallelPrompts ({llmInstance.parallelPrompts})! " +
                           "This may cause characters to be dropped when switching between them. Consider reducing maxWarmCharacters or increasing parallelPrompts.");
        }

        lastPlayerPosition = player.position;

        // Perform initial warmup after a short delay to ensure all systems are initialized
        Invoke(nameof(UpdateWarmupState), 2f);
    }

    private void Update()
    {
        // Only run if initialization is complete
        if (!characterManager.IsInitialized || !npcManager.IsInitializationComplete) // Assuming NPCManager has similar flag
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

        // Prevent errors if cache is empty (e.g., during scene transitions or if NPCs haven't spawned)
        if (cachedNPCs.Count == 0)
        {
             // Optionally log this, but it might be noisy during setup
             // Debug.Log("[ProximityWarmup] No cached NPCs found, skipping proximity check.");
             return;
        }

        var characterDistances = new List<(string name, float distance)>();

        // Calculate distances to all characters that have a corresponding NPC object
        foreach (string characterName in characterManager.GetAvailableCharacters())
        {
            if (cachedNPCs.TryGetValue(characterName, out GameObject npcObject) && npcObject != null)
            {
                // Calculate distance from player to this NPC
                float distance = Vector3.Distance(player.position, npcObject.transform.position);
                characterDistances.Add((characterName, distance));
            }
            else
            {
                 // This might happen if CharacterManager knows about a character whose NPC hasn't spawned yet
                 // Debug.LogWarning($"[ProximityWarmup] Character '{characterName}' exists in CharacterManager but not found in NPC cache.");
            }
        }

        // Sort by distance (closest first)
        characterDistances.Sort((a, b) => a.distance.CompareTo(b.distance));

        var charactersToWarm = new List<string>();
        var charactersToCool = new List<string>();

        Debug.Log($"[ProximityWarmup] Updating states. MaxWarm={maxWarmCharacters}. PlayerPos={player.position}. Found {characterDistances.Count} characters with distance.");

        // Determine who needs warming/cooling
        for (int i = 0; i < characterDistances.Count; i++)
        {
            string name = characterDistances[i].name;
            float distance = characterDistances[i].distance;
            bool shouldBeWarm = i < maxWarmCharacters;
            var currentState = characterManager.GetCharacterState(name);

            Debug.Log($"[ProximityWarmup] Checking {name}: Distance={distance:F1}, ShouldBeWarm={shouldBeWarm}, CurrentState={currentState}");

            if (shouldBeWarm && currentState == CharacterManager.CharacterState.LoadingTemplate)
            {
                charactersToWarm.Add(name);
            }
            else if (!shouldBeWarm && currentState == CharacterManager.CharacterState.Ready)
            {
                 charactersToCool.Add(name);
            }
        }

        // Execute cooling first
        foreach (string name in charactersToCool)
        {
            Debug.Log($"[ProximityWarmup] ACTION: Cooling down distant character: {name}");
            characterManager.CooldownCharacter(name);
        }

        // Execute warming
        foreach (string name in charactersToWarm)
        {
            Debug.Log($"[ProximityWarmup] ACTION: Warming up nearby character: {name}");
            StartCoroutine(characterManager.WarmupCharacter(name));
        }
    }

    private void RefreshNPCCache()
    {
        cachedNPCs.Clear();

        // Reverted to using FindObjectsByType as GetAllSpawnedNPCs doesn't exist on NPCManager
        var activeNPCs = FindObjectsByType<Character>(FindObjectsSortMode.None);
        foreach (var npcCharacterComp in activeNPCs) // Use a different variable name to avoid conflict
        {
            if (npcCharacterComp != null)
            {
                string characterName = npcCharacterComp.GetCharacterName();
                if (!string.IsNullOrEmpty(characterName))
                {
                    // Check if it's actually an NPC managed by CharacterManager before adding
                    if (characterManager.GetCharacterByName(characterName) != null)
                    {
                         cachedNPCs[characterName] = npcCharacterComp.gameObject;
                    }
                }
            }
        }

        // Log count AFTER attempting to populate
        Debug.Log($"[ProximityWarmup RefreshNPCCache] Finished refresh. Found {cachedNPCs.Count} active NPCs via FindObjectsByType<Character>.");
        if (cachedNPCs.Count == 0 && characterManager.GetAvailableCharacters().Length > 0)
        {
            Debug.LogWarning("[ProximityWarmup RefreshNPCCache] Found 0 NPC GameObjects despite CharacterManager having characters. NPC spawning might be delayed or failing.");
        }
    }
}
