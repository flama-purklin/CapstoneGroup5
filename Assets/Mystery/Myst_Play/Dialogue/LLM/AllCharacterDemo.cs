using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

/// <summary>
/// Manages spawning all characters at once for demonstration purposes.
/// </summary>
public class AllCharacterDemo : MonoBehaviour
{
    [Header("Runtime Status")]
    [SerializeField] private int charactersLoaded = 0;
    [SerializeField] private int carsWithCharacters = 0;
    [SerializeField] private List<string> loadedCharacterNames = new List<string>();
    [SerializeField] private int availableCharactersInManager = 0;
    [SerializeField] private float monitoringInterval = 3.0f;
    [SerializeField] private bool continuousMonitoring = true;
    
    // Track character counts over time to detect changes
    private int lastCharacterCount = 0;
    
    private void Start()
    {
        Debug.Log("=== ALL CHARACTER DEMO MODE ACTIVATED - Global character spawning enabled ===");
        
        // Use the CharacterSpawnerService if available
        var spawnerService = FindFirstObjectByType<CharacterSpawnerService>();
        if (spawnerService == null)
        {
            // Create spawner service if it doesn't exist
            GameObject spawnerObj = new GameObject("CharacterSpawnerService");
            spawnerService = spawnerObj.AddComponent<CharacterSpawnerService>();
            Debug.Log("Created CharacterSpawnerService for global character management");
        }
        
        // Register for cleanup in case of scene reload
        DontDestroyOnLoad(spawnerService.gameObject);
        
        // Trigger global character spawning right at the start
        StartCoroutine(TriggerGlobalSpawning());
        
        // Setup regular monitoring
        StartCoroutine(CheckCharactersAfterDelay());
        
        if (continuousMonitoring)
        {
            StartCoroutine(PeriodicCharacterCheck());
        }
    }
    
    private IEnumerator TriggerGlobalSpawning()
    {
        // Use the CharacterSpawnerService to handle spawning
        var spawnerService = FindFirstObjectByType<CharacterSpawnerService>();
        if (spawnerService == null)
        {
            Debug.LogError("CharacterSpawnerService not found! Cannot spawn characters.");
            yield break;
        }
        
        // Wait for NPCManager to be ready
        NPCManager npcManager = FindFirstObjectByType<NPCManager>();
        if (npcManager)
        {
            // Wait until NPCManager is fully initialized
            float waitTime = 0f;
            float maxWaitTime = 30f;
            while (!npcManager.IsInitializationComplete && waitTime < maxWaitTime)
            {
                yield return new WaitForSeconds(0.5f);
                waitTime += 0.5f;
                Debug.Log($"Waiting for NPCManager to initialize... ({waitTime}s)");
            }
            
            if (npcManager.IsInitializationComplete)
            {
                Debug.Log("ALL CHARACTER DEMO: Triggering global character spawning via CharacterSpawnerService");
                
                // Let the service handle the entire spawning process
                spawnerService.ClearAllCharacters();
                spawnerService.SpawnAllCharacters();
                
                // Wait for confirmation and validation
                yield return new WaitForSeconds(1f);
                spawnerService.ValidateAllCharacters();
            }
            else
            {
                Debug.LogError("NPCManager failed to initialize after waiting! Characters may not spawn correctly.");
            }
        }
        
        yield return new WaitForSeconds(3f);
        CheckCharacterStatus();
    }
    
    // Continuous monitoring coroutine to update character counts as player moves between cars
    private IEnumerator PeriodicCharacterCheck()
    {
        // Wait a bit to let initial character spawning happen
        yield return new WaitForSeconds(monitoringInterval * 2);
        
        // Get the spawner service for validation
        var spawnerService = FindFirstObjectByType<CharacterSpawnerService>();
        
        while (true)
        {
            yield return new WaitForSeconds(monitoringInterval);
            
            // Check for character changes
            int previousCount = charactersLoaded;
            CheckCharacterStatus();
            
            // Only log if character count has changed
            if (charactersLoaded != previousCount)
            {
                Debug.Log($"Character status updated: {charactersLoaded} characters now active");
                
                // Run validation if character count changed
                if (spawnerService != null && charactersLoaded > 0)
                {
                    spawnerService.ValidateAllCharacters();
                }
            }
        }
    }

    private IEnumerator CheckCharactersAfterDelay()
    {
        // Wait for characters to be loaded
        yield return new WaitForSeconds(5f);
        
        // Get all characters
        CheckCharacterStatus();
        
        if (charactersLoaded == 0)
        {
            Debug.LogError("No characters were loaded! Demo setup failed.");
            
            // Check if NPCManager is ready before attempting spawning
            NPCManager npcManager = GameObject.FindFirstObjectByType<NPCManager>();
            if (npcManager == null)
            {
                Debug.LogError("NPCManager not found! Cannot spawn characters.");
                yield break;
            }
            
            if (!npcManager.IsInitializationComplete)
            {
                Debug.Log("Waiting for NPCManager to initialize...");
                float waitTime = 0f;
                float maxWaitTime = 30f; // Maximum wait time of 30 seconds
                
                while (!npcManager.IsInitializationComplete && waitTime < maxWaitTime)
                {
                    yield return new WaitForSeconds(1f);
                    waitTime += 1f;
                    Debug.Log($"Waiting for NPCManager: {waitTime} seconds elapsed");
                }
                
                if (!npcManager.IsInitializationComplete)
                {
                    Debug.LogError("NPCManager failed to initialize after waiting!");
                    yield break;
                }
            }
            
            // Ensure character container exists
            npcManager.PlaceNPCsInGameScene();
            
            // Attempt to trigger manual spawning
            Debug.Log("Attempting manual character spawning...");
            CarCharacters.SpawnAllAvailableCharacters();
            
            // Wait and check again
            yield return new WaitForSeconds(3f);
            CheckCharacterStatus();
        }
    }
    
    public void CheckCharacterStatus()
    {
        // Find all active NPCs in the scene
        var characters = GameObject.FindObjectsByType<Character>(FindObjectsSortMode.None);
        var carComponents = GameObject.FindObjectsByType<CarCharacters>(FindObjectsSortMode.None);
        var npcManager = GameObject.FindFirstObjectByType<NPCManager>();
        
        // Count actively loaded characters in the scene
        charactersLoaded = characters.Length;
        
        // Count cars with at least one character
        carsWithCharacters = 0;
        int totalCharactersInCars = 0;
        
        // More accurate count of cars with characters - check each car directly
        foreach (var car in carComponents)
        {
            var carChars = car.GetCurrCharacters();
            if (carChars != null && carChars.Count > 0)
            {
                carsWithCharacters++;
                totalCharactersInCars += carChars.Count;
            }
        }
        
        // Update character names list and check character component initialization
        loadedCharacterNames.Clear();
        List<string> characterNameIssues = new List<string>();
        
        foreach (var character in characters)
        {
            string charName = character.CharacterName;
            string objName = character.gameObject.name;
            Transform parentTransform = character.transform.parent;
            string parentName = parentTransform != null ? parentTransform.name : "no-parent";
            
            if (!string.IsNullOrEmpty(charName))
            {
                loadedCharacterNames.Add(charName);
                
                // Check for name inconsistencies
                if (parentName != "NPC_" + charName && !parentName.Contains(charName))
                {
                    characterNameIssues.Add($"{charName} (parent name mismatch: {parentName})");
                }
            }
            else
            {
                characterNameIssues.Add($"Unnamed character in GameObject: {objName}, Parent: {parentName}");
            }
        }
        
        // Track available characters in NPCManager
        if (npcManager != null)
        {
            var availableChars = npcManager.GetAvailableCharacters();
            availableCharactersInManager = availableChars.Length;
            
            // Check for character mismatch (characters in manager but not in scene)
            var missingCharacters = availableChars.Except(loadedCharacterNames).ToList();
            
            // Check for duplicate character objects with same name
            var duplicateNames = loadedCharacterNames.GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
            
            // Log results
            Debug.Log($"Character Check Results:");
            Debug.Log($"- Characters active in scene: {charactersLoaded}");
            Debug.Log($"- Cars with characters: {carsWithCharacters}/{carComponents.Length}");
            Debug.Log($"- Total characters in all cars: {totalCharactersInCars}");
            Debug.Log($"- Character names: {string.Join(", ", loadedCharacterNames)}");
            Debug.Log($"- Available characters in NPCManager: {availableChars.Length}");
            Debug.Log($"- Names: {string.Join(", ", availableChars)}");
            
            if (characterNameIssues.Count > 0)
            {
                Debug.LogWarning($"- Character name issues detected: {characterNameIssues.Count}");
                foreach (var issue in characterNameIssues)
                {
                    Debug.LogWarning($"  * {issue}");
                }
            }
            
            if (duplicateNames.Count > 0)
            {
                Debug.LogWarning($"- Duplicate character objects detected for: {string.Join(", ", duplicateNames)}");
            }
            
            if (missingCharacters.Count > 0)
            {
                Debug.Log($"- Characters available but not in scene: {missingCharacters.Count}");
                Debug.Log($"- Missing characters: {string.Join(", ", missingCharacters)}");
                
                // If characters are still missing after multiple attempts, try force spawning again
                if (Time.realtimeSinceStartup > 20f) // Only retry after 20 seconds into runtime
                {
                    Debug.Log("Attempting to force spawn missing characters again...");
                    CarCharacters.SpawnAllAvailableCharacters();
                }
            }
            
            // Inspect NPCAnimManager sprite assignments
            InspectCharacterSprites();
        }
    }
    
    // New method to inspect sprite assignments for characters
    private void InspectCharacterSprites()
    {
        var animManagers = GameObject.FindObjectsByType<NPCAnimManager>(FindObjectsSortMode.None);
        Debug.Log($"Found {animManagers.Length} NPCAnimManager components");
        
        // Group animation containers by parent object names
        var animContainerGroups = new Dictionary<string, List<SpriteRenderer>>();
        
        foreach (var animManager in animManagers)
        {
            var parentName = animManager.transform.parent != null ? 
                animManager.transform.parent.name : "no-parent";
                
            var spriteRenderer = animManager.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                if (!animContainerGroups.ContainsKey(parentName))
                {
                    animContainerGroups[parentName] = new List<SpriteRenderer>();
                }
                
                animContainerGroups[parentName].Add(spriteRenderer);
            }
        }
        
        // Now look for duplicate sprites
        Dictionary<Sprite, List<string>> spriteDuplicates = new Dictionary<Sprite, List<string>>();
        
        foreach (var pair in animContainerGroups)
        {
            foreach (var renderer in pair.Value)
            {
                if (renderer.sprite != null)
                {
                    if (!spriteDuplicates.ContainsKey(renderer.sprite))
                    {
                        spriteDuplicates[renderer.sprite] = new List<string>();
                    }
                    
                    spriteDuplicates[renderer.sprite].Add(pair.Key);
                }
            }
        }
        
        // Report any sprites used by multiple characters
        foreach (var pair in spriteDuplicates)
        {
            if (pair.Value.Count > 1)
            {
                Debug.LogWarning($"Sprite {pair.Key.name} is used by multiple characters: {string.Join(", ", pair.Value)}");
            }
        }
    }
}