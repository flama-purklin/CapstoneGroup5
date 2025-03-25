using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Central coordinator that bridges the gap between mystery data and the physical game world.
/// This unholy amalgamation of manager patterns is the beating heart of our reality model.
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
    
    // Public properties
    public bool IsInitialized => isInitialized;
    public Mystery CurrentMystery => currentMystery;
    
    private void Awake()
    {
        // Ensure we have all required components through the magic of GetComponentInChildren
        if (trainGenerator == null)
            trainGenerator = GetComponentInChildren<TrainGenerator>();
            
        if (entityPlacer == null)
            entityPlacer = GetComponentInChildren<EntityPlacer>();
            
        if (locationRegistry == null)
            locationRegistry = GetComponentInChildren<LocationRegistry>();
            
        if (trainGenerator == null || entityPlacer == null || locationRegistry == null)
        {
            Debug.LogError("WorldCoordinator is missing required components! Everything's fucked.");
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
            Debug.LogError("Cannot initialize world with null mystery data. What did you expect would happen?");
            return;
        }
        
        currentMystery = mystery;
        isInitialized = false;
        
        LogDebug("Starting world initialization. May God have mercy on our souls...");
        
        // Generate train based on mystery layout
        if (mystery.TrainLayout != null && mystery.TrainLayout.Cars != null)
        {
            trainGenerator.GenerateTrainFromLayout(mystery.TrainLayout);
            LogDebug($"Generated train with {mystery.TrainLayout.Cars.Count} cars. Choo fucking choo.");
        }
        else
        {
            Debug.LogError("Mystery is missing train layout data. What kind of mystery has no train?");
            return;
        }
        
        // Register all locations for later reference
        StartCoroutine(InitializeLocationsAndEntities());
    }
    
    private IEnumerator InitializeLocationsAndEntities()
    {
        // Wait one frame to ensure train cars are fully initialized
        // because Unity's execution order is a cruel joke
        yield return null;
        
        // Register all locations
        RegisterAllLocations();
        
        // Place characters in their initial positions
        PlaceCharactersInInitialLocations();
        
        // Place evidence objects
        PlaceEvidenceInLocations();
        
        // Mark as initialized
        isInitialized = true;
        LogDebug("World initialization complete. Let there be light... or whatever.");
    }
    
    /// <summary>
    /// Registers all locations in the world for later reference.
    /// Because hardcoded references are for masochists.
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
                Debug.LogError($"Train car {car.name} is missing CarIdentifier component. Who designed this? A toddler?");
            }
        }
    }
    
    /// <summary>
    /// Recursively registers all sub-locations within a parent location.
    /// Like a Russian nesting doll, but with more existential dread.
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
    /// Unlike the previous system, we actually USE this data instead of ignoring it like an unwanted stepchild.
    /// </summary>
    private void PlaceCharactersInInitialLocations()
    {
        if (currentMystery.Characters == null)
        {
            Debug.LogError("Mystery has no character data. It's a very lonely train.");
            return;
        }
        
        LogDebug($"Placing {currentMystery.Characters.Count} characters... watch them materialize like magic.");
        
        foreach (var characterEntry in currentMystery.Characters)
        {
            string characterId = characterEntry.Key;
            MysteryCharacter characterData = characterEntry.Value;
            
            if (string.IsNullOrEmpty(characterData.InitialLocation))
            {
                Debug.LogWarning($"Character {characterId} has no initial location specified. Homeless, are we?");
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
                Debug.LogError($"Cannot find location {characterData.InitialLocation} for character {characterId}. Did someone delete the train?");
            }
        }
    }
    
    /// <summary>
    /// Places all evidence items in their locations based on mystery data.
    /// Because randomly scattered evidence would make too much sense.
    /// </summary>
    private void PlaceEvidenceInLocations()
    {
        if (currentMystery.Constellation == null || currentMystery.Constellation.Nodes == null)
        {
            Debug.LogError("Mystery has no constellation/node data. It's a rather boring mystery, isn't it?");
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
                    Debug.LogError($"Cannot find location {nodeData.Location} for evidence {nodeId}. Is it in the void?");
                }
            }
        }
        
        LogDebug($"Placed {evidenceCount} evidence items. CSI would be proud.");
    }
    
    /// <summary>
    /// Gets a transform for a location ID.
    /// Because string references are better than hardcoded paths. Fight me.
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
    /// Useful for when you need to know who's hogging the bathroom.
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
    /// For when you need to know which car has all the bloody knives.
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
    
    /// <summary>
    /// Gets the number of train cars in the layout.
    /// Primarily for testing purposes.
    /// </summary>
    public int GetTrainCarCount()
    {
        return trainGenerator.GetTrainCars().Count;
    }
    
    /// <summary>
    /// Gets a train car by its ID.
    /// </summary>
    public GameObject GetCarById(string carId)
    {
        return trainGenerator.GetCarById(carId);
    }
    
    private void LogDebug(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[WorldCoordinator] {message}");
        }
    }
}
