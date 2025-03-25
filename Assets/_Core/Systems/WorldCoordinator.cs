using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Central coordinator that bridges the gap between mystery data and the physical game world.
/// Responsible for generating the train layout, placing characters and evidence, and maintaining
/// a registry of all locations in the world.
/// </summary>
public class WorldCoordinator : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TrainGenerator trainGenerator;
    [SerializeField] private EntityPlacer entityPlacer;
    [SerializeField] private LocationRegistry locationRegistry;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    private Mystery currentMystery;
    private Dictionary<string, GameObject> characterInstances = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> evidenceInstances = new Dictionary<string, GameObject>();
    private bool isInitialized = false;
    
    public bool IsInitialized => isInitialized;
    
    private void Awake()
    {
        // Ensure we have all required components
        if (trainGenerator == null)
            trainGenerator = GetComponentInChildren<TrainGenerator>();
            
        if (entityPlacer == null)
            entityPlacer = GetComponentInChildren<EntityPlacer>();
            
        if (locationRegistry == null)
            locationRegistry = GetComponentInChildren<LocationRegistry>();
            
        if (trainGenerator == null || entityPlacer == null || locationRegistry == null)
        {
            Debug.LogError("WorldCoordinator is missing required components!");
        }
    }
    
    /// <summary>
    /// Initializes the game world based on mystery data.
    /// This is the main entry point for world generation.
    /// </summary>
    public void InitializeWorld(Mystery mystery)
    {
        if (mystery == null)
        {
            Debug.LogError("Cannot initialize world with null mystery data.");
            return;
        }
        
        currentMystery = mystery;
        isInitialized = false;
        
        LogDebug("Starting world initialization...");
        
        // Generate train based on mystery layout
        if (mystery.TrainLayout != null && mystery.TrainLayout.Cars != null)
        {
            trainGenerator.GenerateTrainFromLayout(mystery.TrainLayout);
            LogDebug($"Generated train with {mystery.TrainLayout.Cars.Count} cars");
        }
        else
        {
            Debug.LogError("Mystery is missing train layout data.");
            return;
        }
        
        // Register all locations for later reference
        StartCoroutine(InitializeLocationsAndEntities());
    }
    
    private IEnumerator InitializeLocationsAndEntities()
    {
        // Wait one frame to ensure train cars are fully initialized
        yield return null;
        
        // Register all locations
        RegisterAllLocations();
        
        // Place characters in their initial positions
        PlaceCharactersInInitialLocations();
        
        // Place evidence objects
        PlaceEvidenceInLocations();
        
        // Mark as initialized
        isInitialized = true;
        LogDebug("World initialization complete");
    }
    
    /// <summary>
    /// Registers all locations in the world for later reference.
    /// </summary>
    private void RegisterAllLocations()
    {
        locationRegistry.ClearLocations();
        
        // Iterate through all train cars and register their locations
        foreach (var car in trainGenerator.GetTrainCars())
        {
            CarIdentifier carIdentifier = car.GetComponent<CarIdentifier>();
            if (carIdentifier != null)
            {
                locationRegistry.RegisterLocation(carIdentifier.CarId, car.transform);
                LogDebug($"Registered car location: {carIdentifier.CarId}");
                
                // Register sub-locations within the car
                RegisterSubLocations(car.transform, carIdentifier.CarId);
            }
            else
            {
                Debug.LogError($"Train car {car.name} is missing CarIdentifier component");
            }
        }
    }
    
    /// <summary>
    /// Recursively registers all sub-locations within a parent location.
    /// </summary>
    private void RegisterSubLocations(Transform parent, string parentId)
    {
        // Find all location identifiers in children
        LocationIdentifier[] locations = parent.GetComponentsInChildren<LocationIdentifier>();
        
        foreach (var location in locations)
        {
            string locationId = $"{parentId}.{location.LocationId}";
            locationRegistry.RegisterLocation(locationId, location.transform);
            LogDebug($"Registered sub-location: {locationId}");
        }
    }
    
    /// <summary>
    /// Places all characters in their initial locations based on mystery data.
    /// </summary>
    private void PlaceCharactersInInitialLocations()
    {
        if (currentMystery.Characters == null)
        {
            Debug.LogError("Mystery has no character data");
            return;
        }
        
        LogDebug($"Placing {currentMystery.Characters.Count} characters...");
        
        foreach (var characterEntry in currentMystery.Characters)
        {
            string characterId = characterEntry.Key;
            MysteryCharacter characterData = characterEntry.Value;
            
            if (string.IsNullOrEmpty(characterData.InitialLocation))
            {
                Debug.LogWarning($"Character {characterId} has no initial location specified");
                continue;
            }
            
            // Get the location transform from the registry
            Transform locationTransform = locationRegistry.GetLocation(characterData.InitialLocation);
            
            if (locationTransform != null)
            {
                GameObject characterInstance = entityPlacer.PlaceCharacter(characterId, characterData, locationTransform);
                if (characterInstance != null)
                {
                    characterInstances[characterId] = characterInstance;
                    LogDebug($"Placed character {characterId} at location {characterData.InitialLocation}");
                }
            }
            else
            {
                Debug.LogError($"Cannot find location {characterData.InitialLocation} for character {characterId}");
            }
        }
    }
    
    /// <summary>
    /// Places all evidence items in their locations based on mystery data.
    /// </summary>
    private void PlaceEvidenceInLocations()
    {
        if (currentMystery.Constellation == null || currentMystery.Constellation.Nodes == null)
        {
            Debug.LogError("Mystery has no constellation/node data");
            return;
        }
        
        int evidenceCount = 0;
        
        foreach (var nodeEntry in currentMystery.Constellation.Nodes)
        {
            string nodeId = nodeEntry.Key;
            MysteryNode nodeData = nodeEntry.Value;
            
            // Only place physical evidence
            if (nodeData.PhysicalEvidence && !string.IsNullOrEmpty(nodeData.Location))
            {
                Transform locationTransform = locationRegistry.GetLocation(nodeData.Location);
                
                if (locationTransform != null)
                {
                    GameObject evidenceInstance = entityPlacer.PlaceEvidence(nodeId, nodeData, locationTransform);
                    if (evidenceInstance != null)
                    {
                        evidenceInstances[nodeId] = evidenceInstance;
                        evidenceCount++;
                    }
                }
                else
                {
                    Debug.LogError($"Cannot find location {nodeData.Location} for evidence {nodeId}");
                }
            }
        }
        
        LogDebug($"Placed {evidenceCount} evidence items");
    }
    
    /// <summary>
    /// Gets a transform for a location ID.
    /// </summary>
    public Transform GetLocationTransform(string locationId)
    {
        return locationRegistry.GetLocation(locationId);
    }
    
    /// <summary>
    /// Gets a character instance by ID.
    /// </summary>
    public GameObject GetCharacterById(string characterId)
    {
        if (characterInstances.TryGetValue(characterId, out GameObject characterInstance))
        {
            return characterInstance;
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets an evidence instance by ID.
    /// </summary>
    public GameObject GetEvidenceById(string evidenceId)
    {
        if (evidenceInstances.TryGetValue(evidenceId, out GameObject evidenceInstance))
        {
            return evidenceInstance;
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets all characters at a specific location.
    /// </summary>
    public List<GameObject> GetCharactersAtLocation(string locationId)
    {
        Transform locationTransform = locationRegistry.GetLocation(locationId);
        if (locationTransform == null)
            return new List<GameObject>();
            
        return characterInstances.Values
            .Where(c => c.transform.IsChildOf(locationTransform))
            .ToList();
    }
    
    /// <summary>
    /// Gets all evidence at a specific location.
    /// </summary>
    public List<GameObject> GetEvidenceAtLocation(string locationId)
    {
        Transform locationTransform = locationRegistry.GetLocation(locationId);
        if (locationTransform == null)
            return new List<GameObject>();
            
        return evidenceInstances.Values
            .Where(e => e.transform.IsChildOf(locationTransform))
            .ToList();
    }
    
    private void LogDebug(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[WorldCoordinator] {message}");
        }
    }
}
