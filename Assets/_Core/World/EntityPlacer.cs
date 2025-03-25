using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Places entities (characters, evidence, etc.) in the world.
/// Because putting people in their place is what we do best.
/// </summary>
public class EntityPlacer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NPCManager npcManager;
    [SerializeField] private EvidenceManager evidenceManager;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    private void Awake()
    {
        // Get references if not already assigned
        if (npcManager == null)
        {
            npcManager = FindFirstObjectByType<NPCManager>();
            if (npcManager == null)
            {
                Debug.LogError("No NPCManager found in scene. Characters will be very lonely.");
            }
        }
        
        // Create EvidenceManager if it doesn't exist
        if (evidenceManager == null)
        {
            evidenceManager = FindFirstObjectByType<EvidenceManager>();
            if (evidenceManager == null)
            {
                // There's no evidence manager, so let's create one
                GameObject evidenceManagerObj = new GameObject("EvidenceManager");
                evidenceManagerObj.transform.SetParent(transform);
                evidenceManager = evidenceManagerObj.AddComponent<EvidenceManager>();
                LogDebug("Created EvidenceManager as none was found");
            }
        }
    }
    
    /// <summary>
    /// Places a character at the specified location.
    /// Like a very specific version of musical chairs.
    /// </summary>
    public GameObject PlaceCharacter(string characterId, MysteryCharacter data, Transform location)
    {
        if (npcManager == null)
        {
            Debug.LogError("Cannot place character: NPCManager is missing. Someone forgot to set up the chess board.");
            return null;
        }
        
        // Find a suitable spawn point
        Transform spawnPoint = GetSpawnPointInLocation(location, "character_spawn");
        
        if (spawnPoint != null)
        {
            // Spawn the character at this location
            GameObject character = npcManager.SpawnNPCInLocation(characterId, spawnPoint.position, location);
            if (character != null)
            {
                LogDebug($"Placed character {characterId} at spawn point in {location.name}");
                return character;
            }
        }
        else
        {
            // Fallback to location center
            GameObject character = npcManager.SpawnNPCInLocation(characterId, location.position, location);
            if (character != null)
            {
                LogDebug($"No spawn point found for character {characterId} at {location.name}. Using center position like a savage.");
                return character;
            }
        }
        
        Debug.LogError($"Failed to place character {characterId} at {location.name}. The void has claimed another victim.");
        return null;
    }
    
    /// <summary>
    /// Places an evidence item at the specified location.
    /// It's like hide and seek, but with clues.
    /// </summary>
    public GameObject PlaceEvidence(string evidenceId, MysteryNode data, Transform location)
    {
        if (evidenceManager == null)
        {
            Debug.LogError("Cannot place evidence: EvidenceManager is missing. The crime scene is contaminated.");
            return null;
        }
        
        // Find a suitable spawn point
        Transform spawnPoint = GetSpawnPointInLocation(location, "evidence_spawn");
        
        if (spawnPoint != null)
        {
            // Spawn the evidence at this location
            GameObject evidence = evidenceManager.SpawnEvidence(evidenceId, data, spawnPoint.position, location);
            if (evidence != null)
            {
                LogDebug($"Placed evidence {evidenceId} at spawn point in {location.name}");
                return evidence;
            }
        }
        else
        {
            // Try character spawn point as fallback
            spawnPoint = GetSpawnPointInLocation(location, "character_spawn");
            
            if (spawnPoint != null)
            {
                GameObject evidence = evidenceManager.SpawnEvidence(evidenceId, data, spawnPoint.position, location);
                if (evidence != null)
                {
                    LogDebug($"Placed evidence {evidenceId} at character spawn point in {location.name} (emergency placement)");
                    return evidence;
                }
            }
            else
            {
                // Final fallback to location center
                GameObject evidence = evidenceManager.SpawnEvidence(evidenceId, data, location.position, location);
                if (evidence != null)
                {
                    LogDebug($"No spawn point found for evidence {evidenceId} at {location.name}. Using center position. It just fell from the sky, I guess.");
                    return evidence;
                }
            }
        }
        
        Debug.LogError($"Failed to place evidence {evidenceId} at {location.name}. Some detective is going to be very confused.");
        return null;
    }
    
    /// <summary>
    /// Finds a spawn point of the specified type within a location.
    /// It's like playing "Where's Waldo" but for spawn points.
    /// </summary>
    private Transform GetSpawnPointInLocation(Transform location, string pointType)
    {
        if (location == null)
        {
            Debug.LogError("Cannot find spawn point in null location. That's a special kind of lost.");
            return null;
        }
        
        // Look for spawn point markers in the location
        SpawnPoint[] spawnPoints = location.GetComponentsInChildren<SpawnPoint>();
        
        // Filter for the right type and available points
        var availablePoints = spawnPoints
            .Where(sp => sp.PointType == pointType && sp.IsAvailable)
            .ToArray();
            
        if (availablePoints.Length > 0)
        {
            // Select a random available point
            int randomIndex = Random.Range(0, availablePoints.Length);
            SpawnPoint selectedPoint = availablePoints[randomIndex];
            
            // Mark as used
            selectedPoint.IsAvailable = false;
            
            LogDebug($"Found spawn point of type {pointType} in {location.name}");
            return selectedPoint.transform;
        }
        
        LogDebug($"No available spawn points of type {pointType} found in {location.name}");
        return null;
    }
    
    /// <summary>
    /// Resets all spawn points in the scene to be available again.
    /// Like a game of musical chairs where everyone forgot where they were sitting.
    /// </summary>
    public void ResetAllSpawnPoints()
    {
        SpawnPoint[] allSpawnPoints = FindObjectsOfType<SpawnPoint>();
        
        foreach (var point in allSpawnPoints)
        {
            point.IsAvailable = true;
        }
        
        LogDebug($"Reset {allSpawnPoints.Length} spawn points to available state. Fresh start for everyone.");
    }
    
    private void LogDebug(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[EntityPlacer] {message}");
        }
    }
}
