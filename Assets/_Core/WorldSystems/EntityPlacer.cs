using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Responsible for placing entities (characters, evidence) in the world.
/// </summary>
public class EntityPlacer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NPCManager npcManager;
    [SerializeField] private EvidenceManager evidenceManager;
    
    [Header("Settings")]
    [SerializeField] private bool debugMode = false;
    
    private void Awake()
    {
        // Try to find managers if not assigned
        if (npcManager == null)
        {
            npcManager = FindFirstObjectByType<NPCManager>();
            if (npcManager == null)
            {
                Debug.LogError("NPCManager not found in scene. Character placement will fail.");
            }
        }
        
        if (evidenceManager == null)
        {
            evidenceManager = FindFirstObjectByType<EvidenceManager>();
            if (evidenceManager == null)
            {
                Debug.LogError("EvidenceManager not found in scene. Evidence placement will fail.");
            }
        }
    }
    
    /// <summary>
    /// Places a character at a specific location.
    /// </summary>
    public GameObject PlaceCharacter(string characterId, MysteryCharacter data, Transform location)
    {
        if (npcManager == null)
        {
            Debug.LogError("Cannot place character: NPCManager not assigned");
            return null;
        }
        
        if (string.IsNullOrEmpty(characterId) || data == null || location == null)
        {
            Debug.LogError("Cannot place character with null ID, data, or location");
            return null;
        }
        
        // Find a suitable spawn point
        Transform spawnPoint = GetSpawnPointInLocation(location, "character_spawn");
        Vector3 spawnPosition;
        
        if (spawnPoint != null)
        {
            // Use the spawn point position
            spawnPosition = spawnPoint.position;
            LogDebug($"Using spawn point for character {characterId} at {location.name}");
        }
        else
        {
            // Fallback to location center with a small offset
            spawnPosition = location.position + new Vector3(0, 0.1f, 0);
            LogDebug($"No spawn point found for character {characterId} at {location.name}, using center");
        }
        
        // Spawn the character at this location
        GameObject npcInstance = npcManager.SpawnNPCInCar(characterId, spawnPosition, location);
        
        if (npcInstance != null)
        {
            LogDebug($"Placed character {characterId} at {location.name}");
            return npcInstance;
        }
        else
        {
            Debug.LogError($"Failed to spawn character {characterId} at {location.name}");
            return null;
        }
    }
    
    /// <summary>
    /// Places evidence at a specific location.
    /// </summary>
    public GameObject PlaceEvidence(string evidenceId, MysteryNode data, Transform location)
    {
        if (evidenceManager == null)
        {
            Debug.LogError("Cannot place evidence: EvidenceManager not assigned");
            return null;
        }
        
        if (string.IsNullOrEmpty(evidenceId) || data == null || location == null)
        {
            Debug.LogError("Cannot place evidence with null ID, data, or location");
            return null;
        }
        
        // Find a suitable spawn point
        Transform spawnPoint = GetSpawnPointInLocation(location, "evidence_spawn");
        Vector3 spawnPosition;
        
        if (spawnPoint != null)
        {
            // Use the spawn point position
            spawnPosition = spawnPoint.position;
            LogDebug($"Using spawn point for evidence {evidenceId} at {location.name}");
        }
        else
        {
            // Fallback to location center with a small offset
            spawnPosition = location.position + new Vector3(0, 0.05f, 0);
            LogDebug($"No spawn point found for evidence {evidenceId} at {location.name}, using center");
        }
        
        // Spawn the evidence at this location
        GameObject evidenceInstance = evidenceManager.SpawnEvidence(evidenceId, data, spawnPosition, location);
        
        if (evidenceInstance != null)
        {
            LogDebug($"Placed evidence {evidenceId} at {location.name}");
            return evidenceInstance;
        }
        else
        {
            Debug.LogError($"Failed to spawn evidence {evidenceId} at {location.name}");
            return null;
        }
    }
    
    /// <summary>
    /// Gets a suitable spawn point in a location for an entity.
    /// </summary>
    private Transform GetSpawnPointInLocation(Transform location, string pointType)
    {
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
            
            return selectedPoint.transform;
        }
        
        // If no typed spawn points found, try any available spawn point
        availablePoints = spawnPoints
            .Where(sp => sp.IsAvailable)
            .ToArray();
            
        if (availablePoints.Length > 0)
        {
            // Select a random available point
            int randomIndex = Random.Range(0, availablePoints.Length);
            SpawnPoint selectedPoint = availablePoints[randomIndex];
            
            // Mark as used
            selectedPoint.IsAvailable = false;
            LogDebug($"Using untyped spawn point for {pointType} at {location.name}");
            
            return selectedPoint.transform;
        }
        
        return null;
    }
    
    /// <summary>
    /// Resets all spawn points to available.
    /// </summary>
    public void ResetSpawnPoints()
    {
        var allSpawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        
        foreach (var spawnPoint in allSpawnPoints)
        {
            spawnPoint.IsAvailable = true;
        }
        
        LogDebug($"Reset {allSpawnPoints.Length} spawn points to available");
    }
    
    private void LogDebug(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[EntityPlacer] {message}");
        }
    }
}