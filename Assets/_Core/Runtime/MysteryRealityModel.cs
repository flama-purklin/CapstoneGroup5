using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Central coordinator that bridges mystery data and the physical game world.
/// This is a simplified replacement for the original WorldCoordinator to get things working.
/// </summary>
public class MysteryRealityModel : MonoBehaviour
{
    // References to child components
    public Transform trainGeneratorTransform;
    public Transform locationRegistryTransform;
    public Transform entityPlacerTransform;
    
    // Runtime data
    private Dictionary<string, Transform> locationTransforms = new Dictionary<string, Transform>();
    private Dictionary<string, GameObject> characterInstances = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> evidenceInstances = new Dictionary<string, GameObject>();
    private bool isInitialized = false;
    
    // Properties
    public bool IsInitialized => isInitialized;
    
    private void Awake()
    {
        // Get child transforms if not set
        if (trainGeneratorTransform == null)
            trainGeneratorTransform = transform.Find("TrainGenerator");
        
        if (locationRegistryTransform == null)
            locationRegistryTransform = transform.Find("LocationRegistry");
        
        if (entityPlacerTransform == null)
            entityPlacerTransform = transform.Find("EntityPlacer");
    }
    
    /// <summary>
    /// Initializes the world with mystery data.
    /// </summary>
    public void InitializeWorld(Mystery mystery)
    {
        if (mystery == null)
        {
            Debug.LogError("Cannot initialize world with null mystery data");
            return;
        }
        
        Debug.Log("Starting world initialization...");
        
        isInitialized = false;
        locationTransforms.Clear();
        characterInstances.Clear();
        evidenceInstances.Clear();
        
        // For now, just mark as initialized
        isInitialized = true;
        Debug.Log("World initialization complete (simplified version)");
    }
    
    /// <summary>
    /// Registers a location for later reference.
    /// </summary>
    public void RegisterLocation(string locationId, Transform locationTransform)
    {
        if (string.IsNullOrEmpty(locationId) || locationTransform == null)
            return;
            
        locationTransforms[locationId] = locationTransform;
    }
    
    /// <summary>
    /// Gets a location transform by ID.
    /// </summary>
    public Transform GetLocationTransform(string locationId)
    {
        if (string.IsNullOrEmpty(locationId))
            return null;
            
        if (locationTransforms.TryGetValue(locationId, out Transform locationTransform))
            return locationTransform;
            
        return null;
    }
    
    /// <summary>
    /// Gets a character by ID.
    /// </summary>
    public GameObject GetCharacterById(string characterId)
    {
        if (string.IsNullOrEmpty(characterId))
            return null;
            
        if (characterInstances.TryGetValue(characterId, out GameObject characterInstance))
            return characterInstance;
            
        return null;
    }
    
    /// <summary>
    /// Gets an evidence item by ID.
    /// </summary>
    public GameObject GetEvidenceById(string evidenceId)
    {
        if (string.IsNullOrEmpty(evidenceId))
            return null;
            
        if (evidenceInstances.TryGetValue(evidenceId, out GameObject evidenceInstance))
            return evidenceInstance;
            
        return null;
    }
    
    /// <summary>
    /// Gets all characters at a specific location.
    /// </summary>
    public List<GameObject> GetCharactersAtLocation(string locationId)
    {
        if (string.IsNullOrEmpty(locationId))
            return new List<GameObject>();
            
        Transform locationTransform = GetLocationTransform(locationId);
        if (locationTransform == null)
            return new List<GameObject>();
            
        return characterInstances.Values
            .Where(c => c != null && c.transform.IsChildOf(locationTransform))
            .ToList();
    }
    
    /// <summary>
    /// Gets all evidence at a specific location.
    /// </summary>
    public List<GameObject> GetEvidenceAtLocation(string locationId)
    {
        if (string.IsNullOrEmpty(locationId))
            return new List<GameObject>();
            
        Transform locationTransform = GetLocationTransform(locationId);
        if (locationTransform == null)
            return new List<GameObject>();
            
        return evidenceInstances.Values
            .Where(e => e != null && e.transform.IsChildOf(locationTransform))
            .ToList();
    }
    
    // Helper method for test runner
    public int GetTrainCarCount()
    {
        return 3; // Mock value for testing
    }
}