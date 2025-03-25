using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Registry for mapping location IDs to transforms.
/// Provides a central lookup system for finding locations in the world.
/// </summary>
public class LocationRegistry : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private bool drawGizmos = true;
    
    private Dictionary<string, Transform> locationTransforms = new Dictionary<string, Transform>();
    private Dictionary<string, List<string>> locationHierarchy = new Dictionary<string, List<string>>();
    
    /// <summary>
    /// Registers a location transform with an ID.
    /// </summary>
    public void RegisterLocation(string locationId, Transform locationTransform)
    {
        if (string.IsNullOrEmpty(locationId))
        {
            Debug.LogError("Cannot register location with null or empty ID");
            return;
        }
        
        if (locationTransform == null)
        {
            Debug.LogError($"Cannot register null transform for location ID: {locationId}");
            return;
        }
        
        if (locationTransforms.ContainsKey(locationId))
        {
            Debug.LogWarning($"Location ID {locationId} already registered. Overwriting.");
        }
        
        locationTransforms[locationId] = locationTransform;
        
        // Update hierarchy if this is a child location
        int lastDotIndex = locationId.LastIndexOf('.');
        if (lastDotIndex > 0)
        {
            string parentId = locationId.Substring(0, lastDotIndex);
            if (!locationHierarchy.ContainsKey(parentId))
            {
                locationHierarchy[parentId] = new List<string>();
            }
            
            locationHierarchy[parentId].Add(locationId);
        }
        
        if (debugMode)
        {
            Debug.Log($"[LocationRegistry] Registered location: {locationId}");
        }
    }
    
    /// <summary>
    /// Gets a transform for a location ID.
    /// </summary>
    public Transform GetLocation(string locationId)
    {
        if (string.IsNullOrEmpty(locationId))
        {
            Debug.LogError("Cannot get location with null or empty ID");
            return null;
        }
        
        if (locationTransforms.TryGetValue(locationId, out Transform locationTransform))
        {
            return locationTransform;
        }
        
        if (debugMode)
        {
            Debug.LogWarning($"[LocationRegistry] Location ID not found: {locationId}");
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets all child locations of a parent location.
    /// </summary>
    public List<string> GetChildLocations(string parentId)
    {
        if (locationHierarchy.TryGetValue(parentId, out List<string> children))
        {
            return new List<string>(children);
        }
        
        return new List<string>();
    }
    
    /// <summary>
    /// Checks if a location is accessible (exists in the registry).
    /// </summary>
    public bool IsLocationAccessible(string locationId)
    {
        return locationTransforms.ContainsKey(locationId);
    }
    
    /// <summary>
    /// Gets the parent location of a child location.
    /// </summary>
    public string GetParentLocation(string locationId)
    {
        int lastDotIndex = locationId.LastIndexOf('.');
        if (lastDotIndex > 0)
        {
            return locationId.Substring(0, lastDotIndex);
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets all registered location IDs.
    /// </summary>
    public List<string> GetAllLocationIds()
    {
        return new List<string>(locationTransforms.Keys);
    }
    
    /// <summary>
    /// Clears all registered locations.
    /// </summary>
    public void ClearLocations()
    {
        locationTransforms.Clear();
        locationHierarchy.Clear();
        
        if (debugMode)
        {
            Debug.Log("[LocationRegistry] All locations cleared");
        }
    }
    
    /// <summary>
    /// Draws gizmos for all registered locations.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!drawGizmos)
            return;
            
        Gizmos.color = Color.green;
        
        foreach (var location in locationTransforms)
        {
            if (location.Value != null)
            {
                // Draw different shapes for different types of locations
                if (location.Key.Contains('.'))
                {
                    // Sub-location
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(location.Value.position, 0.2f);
                }
                else
                {
                    // Main location
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(location.Value.position, Vector3.one * 0.5f);
                }
                
                // Draw connections between parent and child locations
                if (location.Key.Contains('.'))
                {
                    string parentId = GetParentLocation(location.Key);
                    if (parentId != null && locationTransforms.TryGetValue(parentId, out Transform parentTransform))
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawLine(location.Value.position, parentTransform.position);
                    }
                }
            }
        }
    }
}
