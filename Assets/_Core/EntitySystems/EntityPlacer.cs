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
    [SerializeField] private float spawnHeightOffset = 0.1f;
    [SerializeField] private bool debugMode = false;
    
    private void Awake()
    {
        // Find required references if not set
        if (npcManager == null)
        {
            npcManager = FindFirstObjectByType<NPCManager>();
        }
        
        if (evidenceManager == null)
        {
            evidenceManager = FindFirstObjectByType<EvidenceManager>();
        }
    }
    
    /// <summary>
    /// Places a character in the world.
    /// </summary>
    public GameObject PlaceCharacter(string characterId, MysteryCharacter data, Transform location)
    {
        if (string.IsNullOrEmpty(characterId) || data == null || location == null)
        {
            Debug.LogError("Cannot place character with null ID, data, or location");
            return null;
        }
        
        if (npcManager == null)
        {
            Debug.LogError("NPCManager is null, cannot place character");
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
            // Calculate a position in the location
            spawnPosition = CalculateSpawnPosition(location);
            LogDebug($"No spawn point found for character {characterId} at {location.name}. Using calculated position.");
        }
        
        // Spawn the character
        GameObject npc = npcManager.SpawnNPCInCar(characterId, spawnPosition, location);
        
        if (npc == null)
        {
            Debug.LogError($"Failed to spawn character {characterId}");
            return null;
        }
        
        LogDebug($"Placed character {characterId} at {spawnPosition}");
        return npc;
    }
    
    /// <summary>
    /// Places evidence in the world.
    /// </summary>
    public GameObject PlaceEvidence(string evidenceId, MysteryNode data, Transform location)
    {
        if (string.IsNullOrEmpty(evidenceId) || data == null || location == null)
        {
            Debug.LogError("Cannot place evidence with null ID, data, or location");
            return null;
        }
        
        if (evidenceManager == null)
        {
            Debug.LogError("EvidenceManager is null, cannot place evidence");
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
            // Calculate a position in the location
            spawnPosition = CalculateSpawnPosition(location);
            LogDebug($"No spawn point found for evidence {evidenceId} at {location.name}. Using calculated position.");
        }
        
        // Spawn the evidence
        GameObject evidence = evidenceManager.SpawnEvidence(evidenceId, data, spawnPosition, location);
        
        if (evidence == null)
        {
            Debug.LogError($"Failed to spawn evidence {evidenceId}");
            return null;
        }
        
        LogDebug($"Placed evidence {evidenceId} at {spawnPosition}");
        return evidence;
    }
    
    /// <summary>
    /// Finds a spawn point within a location.
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
        
        return null;
    }
    
    /// <summary>
    /// Calculates a spawn position within a location.
    /// </summary>
    private Vector3 CalculateSpawnPosition(Transform location)
    {
        // Check if this is a train car floor
        Transform floor = location.Find("RailCarFloor");
        if (floor != null)
        {
            // Use train car floor as the reference for positioning
            location = floor;
        }
        
        // Try to get a renderer to determine bounds
        Renderer renderer = location.GetComponentInChildren<Renderer>();
        Bounds bounds;
        
        if (renderer != null)
        {
            bounds = renderer.bounds;
        }
        else
        {
            // If no renderer, use a default space around the transform
            bounds = new Bounds(location.position, new Vector3(5f, 0.1f, 3f));
        }
        
        // Calculate random position within the bounds
        float xOffset = Random.Range(-bounds.extents.x * 0.7f, bounds.extents.x * 0.7f);
        float zOffset = Random.Range(-bounds.extents.z * 0.7f, bounds.extents.z * 0.7f);
        
        // Use the floor's Y position plus a small offset
        float yPos = bounds.min.y + spawnHeightOffset;
        
        return new Vector3(
            location.position.x + xOffset, 
            yPos, 
            location.position.z + zOffset
        );
    }
    
    private void LogDebug(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[EntityPlacer] {message}");
        }
    }
}