using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CarCharacters : MonoBehaviour
{
    private bool visited = false;
    [SerializeField] public GameObject npcPrefab;
    [SerializeField] public MeshRenderer carFloor;
    // Not used in all-character demo mode, but kept for compatibility
    [SerializeField] private int minCharacters = 3;
    // Increased max characters per car to allow for more characters in all-character demo mode
    [SerializeField] private int maxCharacters = 10; // Increased from 3 to accommodate all characters

    private List<GameObject> carCharacters = new List<GameObject>();
    private NPCManager npcManager;

    private static HashSet<string> usedCharacters = new HashSet<string>();
    // Global character spawning - this flag prevents multiple spawns
    private static bool globalSpawnCompleted = false;

    private void Start()
    {
        npcManager = FindFirstObjectByType<NPCManager>();
        if (!npcManager)
        {
            Debug.LogError("NPCManager not found in scene!");
            return;
        }

        if (GetComponent<CarVisibility>().selected)
        {
            InitializeCharacters();
        }
    }

    public void InitializeCharacters()
    {
        // Check if this is a demo mode where all characters should be pre-spawned
        bool allCharacterDemoActive = GameObject.FindFirstObjectByType<AllCharacterDemo>() != null;
        
        // In all-character demo mode, we don't spawn characters on car visibility change
        if (allCharacterDemoActive)
        {
            // Just mark as visited but don't spawn - characters are handled globally
            visited = true;
            return;
        }
        
        // Normal mode behavior - only spawn if not already visited
        if (visited || !npcManager) return;
        
        visited = true;
        StartCoroutine(SpawnCharactersWhenReady());
    }

    private IEnumerator SpawnCharactersWhenReady()
    {
        // wait for NPCManager to be ready
        while (!npcManager.IsInitializationComplete)
        {
            yield return new WaitForSeconds(0.1f);
        }

        string[] allCharacters = npcManager.GetAvailableCharacters();
        if (allCharacters == null || allCharacters.Length == 0)
        {
            Debug.LogError("No characters available to spawn!");
            yield break;
        }

        // Check if AllCharacterDemo is present, which indicates we're in all-character mode
        bool allCharacterDemoActive = GameObject.FindFirstObjectByType<AllCharacterDemo>() != null;
        
        // Get available characters (all chars or filter out used ones based on mode)
        List<string> availableCharacters;
        if (allCharacterDemoActive)
        {
            // In all-character demo mode, use all characters
            availableCharacters = new List<string>(allCharacters);
            Debug.Log("AllCharacterDemo active - using all available characters");
        }
        else
        {
            // In normal mode, filter out already used characters
            availableCharacters = allCharacters
                .Where(c => !usedCharacters.Contains(c))
                .ToList();
        }

        if (availableCharacters.Count == 0)
        {
            Debug.LogWarning("No characters available to spawn!");
            yield break;
        }

        // Limit character count based on mode
        int characterCount;
        if (allCharacterDemoActive)
        {
            // In all-character demo mode, use as many as possible
            characterCount = Mathf.Min(maxCharacters, availableCharacters.Count);
            Debug.Log($"All-character demo mode: spawning up to {characterCount} characters");
        }
        else
        {
            // In normal mode, use random count within range
            int maxPossibleCharacters = Mathf.Min(maxCharacters, availableCharacters.Count);
            characterCount = Random.Range(minCharacters, maxPossibleCharacters + 1);
        }

        for (int i = 0; i < characterCount; i++)
        {
            if (availableCharacters.Count == 0) break;

            // randomly select a character from remaining available characters
            int randomIndex = Random.Range(0, availableCharacters.Count);
            string selectedCharacter = availableCharacters[randomIndex];

            availableCharacters.RemoveAt(randomIndex);
            usedCharacters.Add(selectedCharacter);

            SpawnCharacter(selectedCharacter);

            yield return null;
        }

        Debug.Log($"{characterCount} characters generated in {gameObject.name}: {string.Join(", ", carCharacters.Select(c => c.name))}");
    }

    public void SpawnCharacter(string characterName)
    {
        Vector3 spawnPos = GetRandomSpawnPosition();
        GameObject npc = npcManager.SpawnNPCInCar(characterName, spawnPos, transform);
        if (npc)
        {
            carCharacters.Add(npc);
            Debug.Log($"Spawned {characterName} in {gameObject.name}");
        }
    }

    public bool IsVisited()
    {
        return visited;
    }

    public Vector3 GetRandomSpawnPosition()
    {
        Vector3 centerPos = carFloor.transform.position;
        float xOffset = Random.Range(-carFloor.bounds.size.x / 3, carFloor.bounds.size.x / 3);
        float zOffset = Random.Range(-carFloor.bounds.size.z / 3, carFloor.bounds.size.z / 3);
        return centerPos + new Vector3(xOffset, 0f, zOffset);
    }

    // method to reset used characters if needed
    public static void ResetUsedCharacters()
    {
        usedCharacters.Clear();
        Debug.Log("Reset used characters list");
    }

    // method to access list of npcs currently in car
    public List<GameObject> GetCurrCharacters()
    {
        return this.carCharacters;
    }
    
    // Global method to spawn all available characters across all cars at once
    public static void SpawnAllAvailableCharacters()
    {
        // Prevent multiple spawns - this is critical to avoid duplicate characters
        if (globalSpawnCompleted)
        {
            Debug.Log("GLOBAL SPAWN ALREADY COMPLETED - Ignoring duplicate spawn request");
            return;
        }
        
        // Find all car character components (only active ones with valid floor)
        var carComponents = GameObject.FindObjectsByType<CarCharacters>(FindObjectsSortMode.None)
            .Where(car => car.gameObject.activeInHierarchy && car.carFloor != null)
            .ToArray();
        
        // If no car components found, log error and return
        if (carComponents.Length == 0)
        {
            Debug.LogError("No valid train cars found for character spawning!");
            return;
        }
        
        // Find the NPCManager
        NPCManager npcManager = GameObject.FindFirstObjectByType<NPCManager>();
        if (!npcManager)
        {
            Debug.LogError("NPCManager not found! Cannot spawn characters.");
            return;
        }
        
        // Wait until NPCManager is initialized (called from GameInitializer's OnSceneLoaded)
        if (!npcManager.IsInitializationComplete)
        {
            Debug.LogWarning("NPCManager not yet initialized. Characters will be spawned by GameInitializer when ready.");
            return;
        }
        
        Debug.Log("GLOBAL CHARACTER SPAWN: Beginning one-time global character distribution");
        
        // Mark ALL cars as visited to prevent future spawning
        foreach (var car in carComponents)
        {
            // Set visited flag to prevent future spawning
            var visitedField = typeof(CarCharacters).GetField("visited", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (visitedField != null)
            {
                bool wasVisited = (bool)visitedField.GetValue(car);
                visitedField.SetValue(car, true);
                Debug.Log($"Car {car.gameObject.name} marked as visited (was: {wasVisited})");
            }
        }
        
        // Clear any previously used characters
        usedCharacters.Clear();
        
        // First clean up any existing characters to prevent duplicates
        CleanupExistingCharacters();
        
        // Only activate cars with characters if we're spawning characters
        foreach (var car in carComponents)
        {
            if (car.gameObject.activeInHierarchy == false)
            {
                car.gameObject.SetActive(true);
                Debug.Log($"Activated train car: {car.gameObject.name}");
            }
        }
            
        // Distribute characters across all active cars
        bool success = DistributeCharactersEvenly(npcManager, carComponents, new HashSet<string>());
        
        // Mark global spawn as completed
        globalSpawnCompleted = success;
        Debug.Log($"GLOBAL CHARACTER SPAWN: completed, {usedCharacters.Count} characters active across {carComponents.Length} cars");
    }
    
    // Helper method to clean up existing characters
    private static void CleanupExistingCharacters()
    {
        var existingCharacters = GameObject.FindObjectsByType<Character>(FindObjectsSortMode.None);
        if (existingCharacters.Length > 0)
        {
            Debug.Log($"Cleaning up {existingCharacters.Length} existing characters before global spawn");
            foreach (var character in existingCharacters)
            {
                if (character != null && character.gameObject != null)
                {
                    Debug.Log($"Destroying character: {character.CharacterName}");
                    GameObject.Destroy(character.gameObject);
                }
            }
        }
    }
    
    // Helper method to distribute characters evenly across train cars
    // Returns true if at least one character was spawned
    private static bool DistributeCharactersEvenly(NPCManager npcManager, CarCharacters[] cars, HashSet<string> skipCharacters = null)
    {
        if (skipCharacters == null) skipCharacters = new HashSet<string>();
        
        // Get all available characters
        string[] allCharacters = npcManager.GetAvailableCharacters();
        if (allCharacters.Length == 0) 
        {
            Debug.LogError("No characters available to distribute!");
            return false;
        }
        
        // Filter out characters that should be skipped (already spawned)
        List<string> charactersToSpawn = new List<string>();
        foreach (string charName in allCharacters)
        {
            if (!skipCharacters.Contains(charName) && !usedCharacters.Contains(charName))
            {
                charactersToSpawn.Add(charName);
            }
        }
        
        Debug.Log($"Found {skipCharacters.Count} characters to skip (already in scene)");
        Debug.Log($"Will distribute {charactersToSpawn.Count} new characters across {cars.Length} train cars");
        
        // If there are no new characters to spawn, we're done
        if (charactersToSpawn.Count == 0)
        {
            Debug.Log("All characters are already in scene - no new characters to spawn");
            return false;
        }

        // Calculate characters per car (with remainder handling)
        int charsPerCar = charactersToSpawn.Count / cars.Length;
        int remainder = charactersToSpawn.Count % cars.Length;
        
        int charIndex = 0;
        int spawnedCount = 0;
        
        for (int i = 0; i < cars.Length; i++)
        {
            int charsInThisCar = charsPerCar + (i < remainder ? 1 : 0);
            for (int j = 0; j < charsInThisCar && charIndex < charactersToSpawn.Count; j++)
            {
                string characterToSpawn = charactersToSpawn[charIndex];
                
                if (!usedCharacters.Contains(characterToSpawn))
                {
                    try {
                        // Spawn the character
                        cars[i].SpawnCharacter(characterToSpawn);
                        Debug.Log($"Assigned character {characterToSpawn} to car {cars[i].gameObject.name}");
                        
                        // Add to used characters to prevent duplicate spawning
                        usedCharacters.Add(characterToSpawn);
                        spawnedCount++;
                    }
                    catch (System.Exception ex) {
                        Debug.LogError($"Error spawning character {characterToSpawn}: {ex.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Skipping already used character: {characterToSpawn}");
                }
                
                charIndex++;
            }
        }
        
        return spawnedCount > 0;
    }
}