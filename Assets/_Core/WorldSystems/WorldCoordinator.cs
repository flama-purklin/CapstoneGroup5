using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Central coordinator that bridges the gap between mystery data and the physical game world.
/// </summary>
public class WorldCoordinator : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TrainGenerator trainGenerator;
    [SerializeField] private EntityPlacer entityPlacer;
    [SerializeField] private LocationRegistry locationRegistry;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    // Runtime data
    private Mystery currentMystery;
    private Dictionary<string, GameObject> characterInstances = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> evidenceInstances = new Dictionary<string, GameObject>();
    private bool isInitialized = false;
    
    // Properties
    public bool IsInitialized => isInitialized;
    public Mystery CurrentMystery => currentMystery;
    
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
    /// Initializes the world based on mystery data.
    /// </summary>
    public void InitializeWorld(Mystery mystery)
    {
        if (mystery == null)
        {
            Debug.LogError("Cannot initialize world with null mystery data");
            return;
        }
        
        currentMystery = mystery;
        isInitialized = false;
        
        LogDebug("Starting world initialization...");
        
        // Clear existing data
        characterInstances.Clear();
        evidenceInstances.Clear();
        locationRegistry.ClearLocations();
        
        // Get train layout from mystery
        TrainLayout trainLayout = mystery.GetTrainLayout();
        
        if (trainLayout == null || trainLayout.Cars == null || trainLayout.Cars.Count == 0)
        {
            Debug.LogError("Mystery has no valid train layout");
            return;
        }
        
        // Generate train based on layout
        trainGenerator.GenerateTrainFromLayout(trainLayout);
        LogDebug($"Generated train with {trainLayout.Cars.Count} cars");
        
        // Start initializing locations and entities
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
    /// Registers all locations in the world.
    /// </summary>
    private void RegisterAllLocations()
    {
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
    /// Places characters in their initial locations.
    /// </summary>
    private void PlaceCharactersInInitialLocations()
    {
        if (currentMystery.Characters == null)
        {
            Debug.LogError("Mystery has no character data");
            return;
        }
        
        LogDebug($"Placing {currentMystery.Characters.Count} characters...");
        
        // Get all character locations
        Dictionary<string, string> characterLocations = currentMystery.GetCharacterInitialLocations();
        
        foreach (var kvp in characterLocations)
        {
            string characterId = kvp.Key;
            string locationId = kvp.Value;
            MysteryCharacter characterData = currentMystery.GetCharacter(characterId);
            
            if (characterData == null)
            {
                Debug.LogError($"Character {characterId} not found in mystery data");
                continue;
            }
            
            // Get the location transform
            Transform locationTransform = locationRegistry.GetLocation(locationId);
            
            if (locationTransform != null)
            {
                GameObject characterInstance = entityPlacer.PlaceCharacter(characterId, characterData, locationTransform);
                if (characterInstance != null)
                {
                    characterInstances[characterId] = characterInstance;
                    LogDebug($"Placed character {characterId} at location {locationId}");
                }
            }
            else
            {
                Debug.LogError($"Cannot find location {locationId} for character {characterId}");
            }
        }
    }
    
    /// <summary>
    /// Places evidence items in their locations.
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
    /// Gets a character by ID.
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
    /// Gets an evidence object by ID.
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
    
    /// <summary>
    /// Resets the world state.
    /// </summary>
    public void ResetWorld()
    {
        // Clear out existing objects
        foreach (var character in characterInstances.Values)
        {
            if (character != null)
            {
                Destroy(character);
            }
        }
        
        foreach (var evidence in evidenceInstances.Values)
        {
            if (evidence != null)
            {
                Destroy(evidence);
            }
        }
        
        characterInstances.Clear();
        evidenceInstances.Clear();
        
        // Re-initialize if we have a mystery
        if (currentMystery != null)
        {
            InitializeWorld(currentMystery);
        }
    }
    
    private void LogDebug(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[WorldCoordinator] {message}");
        }
    }
}