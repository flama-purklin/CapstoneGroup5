using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System.Reflection;
using LLMUnity;
using System.Collections;

/// <summary>
/// A centralized service for managing character spawning with strict validation.
/// </summary>
public class CharacterSpawnerService : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private bool spawnOnAwake = false;
    [SerializeField] private bool clearExistingCharacters = true;
    
    // Registry of all characters and their status
    private Dictionary<string, SpawnedCharacterInfo> characterRegistry = new Dictionary<string, SpawnedCharacterInfo>();
    private NPCManager npcManager;
    private static CharacterSpawnerService _instance;
    public static CharacterSpawnerService Instance => _instance;
    
    private void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        
        // Find NPCManager
        npcManager = FindFirstObjectByType<NPCManager>();
        
        // Spawn on awake if configured
        if (spawnOnAwake)
        {
            StartCoroutine(SpawnWhenReady());
        }
    }
    
    /// <summary>
    /// Internal info class to track character status
    /// </summary>
    private class SpawnedCharacterInfo
    {
        public string Id { get; set; }
        public GameObject GameObject { get; set; }
        public Character CharacterComponent { get; set; }
        public Transform CarTransform { get; set; }
        public bool IsActive { get; set; }
        public string ExpectedParentName => $"NPC_{Id}";
        
        public SpawnedCharacterInfo(string id, GameObject gameObject, Character character, Transform carTransform)
        {
            Id = id;
            GameObject = gameObject;
            CharacterComponent = character;
            CarTransform = carTransform;
            IsActive = gameObject != null && gameObject.activeInHierarchy;
        }
        
        public bool ValidateNaming()
        {
            if (GameObject == null) return false;
            
            bool isValid = GameObject.name == ExpectedParentName;
            if (!isValid)
            {
                Debug.LogWarning($"Character {Id} has incorrect GameObject name: {GameObject.name}, expected: {ExpectedParentName}");
                // Fix the name
                GameObject.name = ExpectedParentName;
            }
            
            return isValid;
        }
    }
    
    private IEnumerator SpawnWhenReady()
    {
        // Wait for NPCManager to be ready
        while (npcManager == null || !npcManager.IsInitializationComplete)
        {
            if (npcManager == null) npcManager = FindFirstObjectByType<NPCManager>();
            yield return new WaitForSeconds(0.5f);
        }
        
        // Clear existing characters if needed
        if (clearExistingCharacters)
        {
            ClearAllCharacters();
        }
        
        // Spawn characters globally
        SpawnAllCharacters();
    }
    
    /// <summary>
    /// Clears all existing characters in the scene
    /// </summary>
    public void ClearAllCharacters()
    {
        var existingCharacters = FindObjectsByType<Character>(FindObjectsSortMode.None);
        if (existingCharacters.Length > 0)
        {
            Debug.Log($"Cleaning up {existingCharacters.Length} existing characters");
            foreach (var character in existingCharacters)
            {
                if (character != null && character.gameObject != null)
                {
                    var parentTransform = character.transform.parent;
                    if (parentTransform != null)
                    {
                        Destroy(parentTransform.gameObject); // Destroy the parent NPC_ object
                    }
                    else
                    {
                        Destroy(character.gameObject);
                    }
                }
            }
        }
        
        // Clear registry
        characterRegistry.Clear();
    }
    
    /// <summary>
    /// Get all valid train cars available for spawning
    /// </summary>
    private CarCharacters[] GetValidTrainCars()
    {
        // Find all cars with valid carFloor
        var allCars = FindObjectsByType<CarCharacters>(FindObjectsSortMode.None)
            .Where(car => car != null && car.gameObject.activeInHierarchy && car.carFloor != null)
            .ToArray();
        
        Debug.Log($"Found {allCars.Length} valid train cars for character spawning");
        return allCars;
    }
    
    /// <summary>
    /// Mark all train cars as visited to prevent automatic spawning
    /// </summary>
    private void MarkAllCarsAsVisited(CarCharacters[] cars)
    {
        foreach (var car in cars)
        {
            // Set the visited flag using reflection
            FieldInfo visitedField = typeof(CarCharacters).GetField("visited", 
                BindingFlags.Instance | BindingFlags.NonPublic);
                
            if (visitedField != null)
            {
                bool currentValue = (bool)visitedField.GetValue(car);
                visitedField.SetValue(car, true);
                Debug.Log($"Marked car {car.gameObject.name} as visited (was: {currentValue})");
            }
        }
    }
    
    /// <summary>
    /// Reset the used characters tracking in CarCharacters
    /// </summary>
    private void ResetUsedCharacters()
    {
        // Reset via the static method
        CarCharacters.ResetUsedCharacters();
    }
    
    /// <summary>
    /// Cleans up the spawning and spawns all characters across valid cars
    /// </summary>
    public void SpawnAllCharacters()
    {
        if (npcManager == null || !npcManager.IsInitializationComplete)
        {
            Debug.LogError("NPCManager not ready for spawning");
            return;
        }
        
        // Get all valid train cars
        var validCars = GetValidTrainCars();
        if (validCars.Length == 0)
        {
            Debug.LogError("No valid train cars found for character spawning!");
            return;
        }
        
        // Reset tracking and mark all cars as visited
        ResetUsedCharacters();
        MarkAllCarsAsVisited(validCars);
        
        // Get all available characters
        string[] allCharacters = npcManager.GetAvailableCharacters();
        if (allCharacters.Length == 0)
        {
            Debug.LogError("No available characters to spawn!");
            return;
        }
        
        Debug.Log($"Spawning {allCharacters.Length} characters across {validCars.Length} train cars");
        
        // Calculate distribution
        int charsPerCar = allCharacters.Length / validCars.Length;
        int remainder = allCharacters.Length % validCars.Length;
        
        int charIndex = 0;
        int totalSpawned = 0;
        
        // Distribute characters across cars
        for (int i = 0; i < validCars.Length; i++)
        {
            int charsInThisCar = charsPerCar + (i < remainder ? 1 : 0);
            
            for (int j = 0; j < charsInThisCar && charIndex < allCharacters.Length; j++)
            {
                string characterId = allCharacters[charIndex++];
                
                // Get a spawn position from the car
                Vector3 spawnPosition = validCars[i].GetRandomSpawnPosition();
                
                // Find valid NavMesh position
                NavMeshHit hit;
                if (NavMesh.SamplePosition(spawnPosition, out hit, 5.0f, NavMesh.AllAreas))
                {
                    spawnPosition = hit.position;
                }
                
                // Spawn the character
                GameObject charObj = npcManager.SpawnNPCInCar(characterId, spawnPosition, validCars[i].transform);
                if (charObj != null)
                {
                    totalSpawned++;
                    
                    // Add to car's character list and our registry
                    validCars[i].GetCurrCharacters().Add(charObj);
                    
                    // Get the Character component
                    Character charComponent = charObj.GetComponentInChildren<Character>();
                    if (charComponent != null)
                    {
                        // Track in registry
                        characterRegistry[characterId] = new SpawnedCharacterInfo(
                            characterId, charObj, charComponent, validCars[i].transform);
                            
                        // Ensure name is correct
                        charObj.name = $"NPC_{characterId}";
                        
                        Debug.Log($"Successfully spawned and registered {characterId} in {validCars[i].gameObject.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"Spawned {characterId} but couldn't find Character component");
                    }
                }
                else
                {
                    Debug.LogError($"Failed to spawn character {characterId}");
                }
            }
        }
        
        Debug.Log($"Global character spawn completed: {totalSpawned}/{allCharacters.Length} characters spawned");
        
        // Schedule validation check
        StartCoroutine(ValidateCharactersAfterDelay(2f));
    }
    
    /// <summary>
    /// Performs a full validation check on all spawned characters
    /// </summary>
    public bool ValidateAllCharacters()
    {
        bool allValid = true;
        int fixedCount = 0;
        
        // Check expected vs actual character count
        var existingCharacters = FindObjectsByType<Character>(FindObjectsSortMode.None);
        
        Debug.Log($"Validating {existingCharacters.Length} characters against {characterRegistry.Count} registered characters");
        
        // First, fix any characters that exist in the scene but aren't in our registry
        foreach (var character in existingCharacters)
        {   
            if (character == null) continue;
            
            string charName = character.CharacterName;
            if (string.IsNullOrEmpty(charName)) continue;
            
            // If this character isn't in our registry, add it
            if (!characterRegistry.ContainsKey(charName))
            {   
                // Get parent GameObject
                Transform parentTransform = character.transform.parent;
                if (parentTransform != null)
                {
                    GameObject parentObj = parentTransform.gameObject;
                    Transform carTransform = parentTransform.parent;
                    
                    // Register it
                    characterRegistry[charName] = new SpawnedCharacterInfo(
                        charName, parentObj, character, carTransform);
                        
                    Debug.Log($"Added previously unregistered character {charName} to registry");
                    fixedCount++;
                }
            }
        }
        
        // Now check each registry entry
        foreach (var entry in characterRegistry.Values)
        {
            // Skip if null
            if (entry.GameObject == null || entry.CharacterComponent == null)
            {
                Debug.LogWarning($"Character {entry.Id} has null references in registry");
                allValid = false;
                continue;
            }
            
            // Validate naming
            if (!entry.ValidateNaming())
            {
                fixedCount++;
                allValid = false;
            }
            
            // Validate character component name
            if (entry.CharacterComponent.CharacterName != entry.Id)
            {
                Debug.LogWarning($"Character component name mismatch: {entry.CharacterComponent.CharacterName} vs {entry.Id}");
                // Try to fix the name
                var charNameField = typeof(Character).GetField("characterName", 
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (charNameField != null)
                {
                    charNameField.SetValue(entry.CharacterComponent, entry.Id);
                    Debug.Log($"Fixed character name: {entry.CharacterComponent.CharacterName}");
                    fixedCount++;
                }
                else
                {
                    allValid = false;
                }
            }
            
            // Validate animation (if applicable)
            var animManager = entry.GameObject.GetComponentInChildren<NPCAnimManager>();
            if (animManager != null)
            {
                // Force animation container refresh
                var refreshMethod = animManager.GetType().GetMethod("AnimContainerAssign", 
                    BindingFlags.Instance | BindingFlags.NonPublic);
                    
                if (refreshMethod != null)
                {
                    // Send the actual character ID to ensure consistent sprite assignment
                    refreshMethod.Invoke(animManager, new object[] { entry.Id });
                    Debug.Log($"Refreshed animation for {entry.Id}");
                }
            }
        
        // Log result
        if (allValid)
        {
            Debug.Log("All characters validated successfully");
        }
        else
        {
            Debug.LogWarning($"Character validation found issues, fixed {fixedCount} naming problems");
        }
        
        return allValid;
    }
    
    /// <summary>
    /// Schedules a validation check after delay
    /// </summary>
    private IEnumerator ValidateCharactersAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ValidateAllCharacters();
    }
}
