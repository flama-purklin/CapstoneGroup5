using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Maintains a registry of all locations in the world and provides lookup functionality.
/// Because remembering where things are is apparently too difficult for the rest of the code.
/// </summary>
public class LocationRegistry : MonoBehaviour
{
    [SerializeField] private bool debugMode = false;
    
    private Dictionary<string, Transform> locationTransforms = new Dictionary<string, Transform>();
    private Dictionary<string, List<string>> locationHierarchy = new Dictionary<string, List<string>>();
    
    /// <summary>
    /// Registers a location with the registry.
    /// </summary>
    public void RegisterLocation(string locationId, Transform locationTransform)
    {
        if (string.IsNullOrEmpty(locationId))
        {
            Debug.LogError("Cannot register location with null or empty ID. What's wrong with you?");
            return;
        }
        
        if (locationTransform == null)
        {
            Debug.LogError($"Cannot register null transform for location {locationId}. Come on, seriously?");
            return;
        }
        
        if (locationTransforms.ContainsKey(locationId))
        {
            if (debugMode)
            {
                Debug.LogWarning($"Location ID {locationId} already registered. Overwriting like we don't care.");
            }
        }
        
        locationTransforms[locationId] = locationTransform;
        
        // If this is a sub-location (contains a dot), add it to the hierarchy
        if (locationId.Contains("."))
        {
            string[] parts = locationId.Split('.');
            string parentId = parts[0];
            
            if (!locationHierarchy.ContainsKey(parentId))
            {
                locationHierarchy[parentId] = new List<string>();
            }
            
            locationHierarchy[parentId].Add(locationId);
        }
    }
    
    /// <summary>
    /// Gets a location transform by ID.
    /// </summary>
    public Transform GetLocation(string locationId)
    {
        if (string.IsNullOrEmpty(locationId))
        {
            Debug.LogError("Cannot get location with null or empty ID. How did you even manage that?");
            return null;
        }
        
        if (locationTransforms.TryGetValue(locationId, out Transform locationTransform))
        {
            return locationTransform;
        }
        
        if (debugMode)
        {
            Debug.LogWarning($"Location ID '{locationId}' not found in registry. Did you forget to register it, or can't you spell?");
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets all sub-locations for a parent location.
    /// </summary>
    public List<string> GetSubLocations(string parentId)
    {
        if (locationHierarchy.TryGetValue(parentId, out List<string> subLocations))
        {
            return new List<string>(subLocations); // Return a copy to prevent modification
        }
        
        return new List<string>();
    }
    
    /// <summary>
    /// Checks if a location exists in the registry.
    /// </summary>
    public bool LocationExists(string locationId)
    {
        return locationTransforms.ContainsKey(locationId);
    }
    
    /// <summary>
    /// Gets the parent location ID for a sub-location.
    /// </summary>
    public string GetParentLocationId(string locationId)
    {
        if (locationId.Contains("."))
        {
            return locationId.Split('.')[0];
        }
        
        return null; // No parent
    }
    
    /// <summary>
    /// Gets all registered locations.
    /// </summary>
    public List<string> GetAllLocations()
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
            Debug.Log("Location registry cleared. It's like we deleted the whole world.");
        }
    }
    
    /// <summary>
    /// Checks if a location is accessible (exists and has an active GameObject).
    /// </summary>
    public bool IsLocationAccessible(string locationId)
    {
        Transform location = GetLocation(locationId);
        return location != null && location.gameObject.activeInHierarchy;
    }
}
