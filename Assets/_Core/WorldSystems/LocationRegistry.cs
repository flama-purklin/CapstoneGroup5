using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Registry for mapping location IDs to transforms.
/// </summary>
public class LocationRegistry : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool debugMode = false;
    
    // Dictionary mapping location IDs to transforms
    private Dictionary<string, Transform> locationTransforms = new Dictionary<string, Transform>();
    
    // Dictionary mapping child locations to parent locations
    private Dictionary<string, string> locationHierarchy = new Dictionary<string, string>();
    
    /// <summary>
    /// Registers a location with the registry.
    /// </summary>
    public void RegisterLocation(string locationId, Transform locationTransform)
    {
        if (string.IsNullOrEmpty(locationId) || locationTransform == null)
        {
            Debug.LogError("Cannot register location with null ID or transform");
            return;
        }
        
        // Check if this location is already registered
        if (locationTransforms.ContainsKey(locationId))
        {
            LogDebug($"Location ID {locationId} already registered. Overwriting.");
        }
        
        // Register the location
        locationTransforms[locationId] = locationTransform;
        
        // Parse location hierarchy
        if (locationId.Contains("."))
        {
            string[] parts = locationId.Split('.');
            string parentId = string.Join(".", parts.Take(parts.Length - 1));
            locationHierarchy[locationId] = parentId;
            
            LogDebug($"Registered location hierarchy: {locationId} -> {parentId}");
        }
        
        LogDebug($"Registered location: {locationId}");
    }
    
    /// <summary>
    /// Gets a location transform by ID.
    /// </summary>
    public Transform GetLocation(string locationId)
    {
        if (string.IsNullOrEmpty(locationId))
        {
            Debug.LogWarning("Empty location ID provided");
            return null;
        }
        
        if (locationTransforms.TryGetValue(locationId, out Transform locationTransform))
        {
            return locationTransform;
        }
        
        // If not found, try to find a partial match
        string matchingId = FindPartialMatch(locationId);
        if (!string.IsNullOrEmpty(matchingId))
        {
            LogDebug($"Found partial match for {locationId}: {matchingId}");
            return locationTransforms[matchingId];
        }
        
        LogDebug($"Location ID {locationId} not found in registry");
        return null;
    }
    
    /// <summary>
    /// Gets all locations.
    /// </summary>
    public Dictionary<string, Transform> GetAllLocations()
    {
        return new Dictionary<string, Transform>(locationTransforms);
    }
    
    /// <summary>
    /// Gets all child locations of a parent location.
    /// </summary>
    public List<string> GetChildLocations(string parentId)
    {
        return locationHierarchy
            .Where(kvp => kvp.Value == parentId)
            .Select(kvp => kvp.Key)
            .ToList();
    }
    
    /// <summary>
    /// Gets the parent location of a child location.
    /// </summary>
    public string GetParentLocation(string childId)
    {
        if (locationHierarchy.TryGetValue(childId, out string parentId))
        {
            return parentId;
        }
        
        return null;
    }
    
    /// <summary>
    /// Clears all registered locations.
    /// </summary>
    public void ClearLocations()
    {
        locationTransforms.Clear();
        locationHierarchy.Clear();
        LogDebug("Cleared all registered locations");
    }
    
    /// <summary>
    /// Finds a partial match for a location ID.
    /// </summary>
    private string FindPartialMatch(string locationId)
    {
        // Try to match the end of the ID (e.g., "dining_car.table_1" -> "table_1")
        if (locationId.Contains("."))
        {
            string localId = locationId.Substring(locationId.LastIndexOf('.') + 1);
            
            foreach (var key in locationTransforms.Keys)
            {
                if (key.EndsWith($".{localId}"))
                {
                    return key;
                }
            }
        }
        
        // Try to match just the prefix (e.g., "dining_car.table_1" -> "dining_car.*")
        foreach (var key in locationTransforms.Keys)
        {
            if (locationId.StartsWith(key) || key.StartsWith(locationId))
            {
                return key;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets all locations matching a pattern.
    /// </summary>
    public List<string> FindLocationsByPattern(string pattern)
    {
        return locationTransforms.Keys
            .Where(id => id.Contains(pattern))
            .ToList();
    }
    
    private void LogDebug(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[LocationRegistry] {message}");
        }
    }
}