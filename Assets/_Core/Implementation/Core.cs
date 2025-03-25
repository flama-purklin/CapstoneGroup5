using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

namespace MysteryEngine.Implementation
{
    /// <summary>
    /// Main implementation namespace for the Mystery Engine.
    /// This is a clean implementation to avoid class duplication issues.
    /// </summary>

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
        [SerializeField] private bool debugMode = true;
        
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
            TrainLayout trainLayout = GetTrainLayoutFromMystery(mystery);
            
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
        /// Extracts train layout from the mystery data.
        /// </summary>
        private TrainLayout GetTrainLayoutFromMystery(Mystery mystery)
        {
            // Check if train_layout exists directly
            if (mystery.TrainLayout != null && mystery.TrainLayout.Cars != null && mystery.TrainLayout.Cars.Count > 0)
            {
                return mystery.TrainLayout;
            }
            
            // Create a default layout as fallback
            return CreateDefaultTrainLayout();
        }
        
        /// <summary>
        /// Creates a default train layout for testing or fallback.
        /// </summary>
        private TrainLayout CreateDefaultTrainLayout()
        {
            List<TrainCarDefinition> cars = new List<TrainCarDefinition>
            {
                new TrainCarDefinition
                {
                    CarId = "engine_01",
                    CarType = "engine",
                    CarClass = "standard",
                    Properties = new Dictionary<string, string>(),
                    AvailableLocations = new List<string> { "control_room", "engine_area" }
                },
                new TrainCarDefinition
                {
                    CarId = "passenger_01",
                    CarType = "passenger",
                    CarClass = "first",
                    Properties = new Dictionary<string, string>(),
                    AvailableLocations = new List<string> { "seat_1", "seat_2", "aisle" }
                },
                new TrainCarDefinition
                {
                    CarId = "dining_01",
                    CarType = "dining",
                    CarClass = "standard",
                    Properties = new Dictionary<string, string>(),
                    AvailableLocations = new List<string> { "table_1", "table_2", "bar" }
                }
            };
            
            Dictionary<string, CarConnection> connections = new Dictionary<string, CarConnection>
            {
                { "engine_01", new CarConnection { ConnectedTo = "passenger_01" } },
                { "passenger_01", new CarConnection { ConnectedTo = "dining_01" } }
            };
            
            return new TrainLayout
            {
                Cars = cars,
                Connections = connections
            };
        }
        
        /// <summary>
        /// Registers all locations in the world for later reference.
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
            
            // Get all character locations
            Dictionary<string, string> characterLocations = GetCharacterInitialLocations(currentMystery);
            
            foreach (var kvp in characterLocations)
            {
                string characterId = kvp.Key;
                string locationId = kvp.Value;
                MysteryCharacter characterData = currentMystery.Characters[characterId];
                
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
        /// Gets all character initial locations from the mystery.
        /// </summary>
        private Dictionary<string, string> GetCharacterInitialLocations(Mystery mystery)
        {
            Dictionary<string, string> locations = new Dictionary<string, string>();
            
            if (mystery.Characters == null)
            {
                return locations;
            }
            
            foreach (var characterEntry in mystery.Characters)
            {
                string characterId = characterEntry.Key;
                MysteryCharacter character = characterEntry.Value;
                
                if (!string.IsNullOrEmpty(character.InitialLocation))
                {
                    locations[characterId] = character.InitialLocation;
                }
                else
                {
                    // Default to first car as a fallback
                    TrainLayout layout = GetTrainLayoutFromMystery(mystery);
                    if (layout != null && layout.Cars.Count > 0)
                    {
                        locations[characterId] = layout.Cars[0].CarId;
                    }
                }
            }
            
            return locations;
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

        /// <summary>
        /// Gets the train car count for testing purposes.
        /// </summary>
        public int GetTrainCarCount()
        {
            return trainGenerator.GetTrainCars().Count;
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

    /// <summary>
    /// System for mapping location IDs to transforms.
    /// </summary>
    public class LocationRegistry : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool debugMode = true;
        
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
        
        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[LocationRegistry] {message}");
            }
        }
    }

    /// <summary>
    /// Generator for train cars based on mystery data.
    /// </summary>
    public class TrainGenerator : MonoBehaviour
    {
        [Header("Car Prefabs")]
        [SerializeField] private GameObject diningCarPrefab;
        [SerializeField] private GameObject passengerCarPrefab;
        [SerializeField] private GameObject kitchenCarPrefab;
        [SerializeField] private GameObject barCarPrefab;
        [SerializeField] private GameObject storageCarPrefab;
        [SerializeField] private GameObject engineCarPrefab;
        [SerializeField] private GameObject defaultCarPrefab; // Fallback
        
        [Header("Layout Settings")]
        [SerializeField] public Transform trainParent;
        [SerializeField] private float carSpacing = 25f;
        [SerializeField] private bool debugMode = true;
        
        // Dictionary to store car prefabs by type
        private Dictionary<string, GameObject> carPrefabsByType;
        
        // Lists to track generated cars
        private List<GameObject> trainCars = new List<GameObject>();
        private Dictionary<string, GameObject> carIdToGameObject = new Dictionary<string, GameObject>();
        
        private void Awake()
        {
            // Initialize car prefabs dictionary
            carPrefabsByType = new Dictionary<string, GameObject>(System.StringComparer.OrdinalIgnoreCase)
            {
                { "dining", diningCarPrefab },
                { "passenger", passengerCarPrefab },
                { "kitchen", kitchenCarPrefab },
                { "bar", barCarPrefab },
                { "storage", storageCarPrefab },
                { "engine", engineCarPrefab }
            };
            
            // Set up train parent if not already assigned
            if (trainParent == null)
            {
                GameObject trainParentObj = new GameObject("Train");
                trainParentObj.transform.SetParent(transform);
                trainParent = trainParentObj.transform;
            }
        }
        
        /// <summary>
        /// Generates the train layout based on the provided specification.
        /// </summary>
        public void GenerateTrainFromLayout(TrainLayout layout)
        {
            // Clear any existing train cars
            ClearExistingTrainCars();
            
            if (layout == null || layout.Cars == null || layout.Cars.Count == 0)
            {
                Debug.LogError("Invalid train layout data.");
                return;
            }
            
            LogDebug($"Generating train with {layout.Cars.Count} cars");
            
            Vector3 spawnPosition = trainParent.position;
            
            // Generate each car in the layout
            foreach (var carDef in layout.Cars)
            {
                if (string.IsNullOrEmpty(carDef.CarId))
                {
                    Debug.LogError("Car definition missing CarId");
                    continue;
                }
                
                if (string.IsNullOrEmpty(carDef.CarType))
                {
                    Debug.LogWarning($"Car {carDef.CarId} missing CarType, using default");
                    carDef.CarType = "passenger";
                }
                
                // Get the appropriate prefab for this car type
                GameObject carPrefab = GetCarPrefabByType(carDef.CarType);
                
                if (carPrefab != null)
                {
                    GameObject carInstance = Instantiate(
                        carPrefab, 
                        spawnPosition, 
                        Quaternion.Euler(0, 180, 0), 
                        trainParent
                    );
                    
                    // Set car name and ID
                    carInstance.name = $"Train Car - {carDef.CarType} ({carDef.CarId})";
                    
                    // Add or update identifier component for later reference
                    CarIdentifier carIdentifier = carInstance.GetComponent<CarIdentifier>();
                    if (carIdentifier == null)
                    {
                        carIdentifier = carInstance.AddComponent<CarIdentifier>();
                    }
                    
                    carIdentifier.CarId = carDef.CarId;
                    carIdentifier.CarType = carDef.CarType;
                    carIdentifier.CarClass = carDef.CarClass;
                    
                    // If the car has custom properties, apply them
                    if (carDef.Properties != null && carDef.Properties.Count > 0)
                    {
                        ApplyCarProperties(carInstance, carDef.Properties);
                    }
                    
                    // If the car has defined locations, create them
                    if (carDef.AvailableLocations != null && carDef.AvailableLocations.Count > 0)
                    {
                        CreateLocationPoints(carInstance, carDef.AvailableLocations);
                    }
                    
                    // Fix any references in components that might need this car's ID
                    UpdateCarReferences(carInstance, carDef.CarId);
                    
                    // Store in our lists
                    trainCars.Add(carInstance);
                    carIdToGameObject[carDef.CarId] = carInstance;
                    
                    LogDebug($"Generated car: {carDef.CarId} of type {carDef.CarType}");
                    
                    // Move to next position
                    spawnPosition += Vector3.left * carSpacing;
                }
                else
                {
                    Debug.LogError($"No prefab found for car type: {carDef.CarType}");
                }
            }
            
            // If there are explicit connections defined, set them up
            if (layout.Connections != null && layout.Connections.Count > 0)
            {
                SetupCarConnections(layout.Connections);
            }
        }
        
        /// <summary>
        /// Clears any existing train cars.
        /// </summary>
        private void ClearExistingTrainCars()
        {
            foreach (var car in trainCars)
            {
                if (car != null)
                {
                    Destroy(car);
                }
            }
            
            trainCars.Clear();
            carIdToGameObject.Clear();
            
            // Also clear any direct children of the train parent
            for (int i = trainParent.childCount - 1; i >= 0; i--)
            {
                Destroy(trainParent.GetChild(i).gameObject);
            }
        }
        
        /// <summary>
        /// Gets the appropriate prefab for a car type.
        /// </summary>
        private GameObject GetCarPrefabByType(string carType)
        {
            if (carPrefabsByType.TryGetValue(carType, out GameObject prefab) && prefab != null)
            {
                return prefab;
            }
            
            // If we don't have a prefab for this type or it's null, log a warning and use default
            Debug.LogWarning($"No prefab for car type: {carType}, using default");
            return defaultCarPrefab != null ? defaultCarPrefab : passengerCarPrefab;
        }
        
        /// <summary>
        /// Applies custom properties to a car instance.
        /// </summary>
        private void ApplyCarProperties(GameObject carInstance, Dictionary<string, string> properties)
        {
            // Get or add a property container component
            CarProperties propertyComponent = carInstance.GetComponent<CarProperties>();
            if (propertyComponent == null)
            {
                propertyComponent = carInstance.AddComponent<CarProperties>();
            }
            
            // Set the properties
            propertyComponent.SetProperties(properties);
            
            // Handle special properties that might affect appearance
            if (properties.TryGetValue("color", out string colorStr))
            {
                ApplyCarColor(carInstance, colorStr);
            }
        }
        
        /// <summary>
        /// Applies a color to a car instance.
        /// </summary>
        private void ApplyCarColor(GameObject carInstance, string colorStr)
        {
            // Parse the color
            if (ColorUtility.TryParseHtmlString(colorStr, out Color color))
            {
                // Find the exterior renderer
                Transform exterior = carInstance.transform.Find("RailCarExterior");
                if (exterior != null)
                {
                    Renderer renderer = exterior.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        // Clone the material to avoid affecting other cars
                        renderer.material = new Material(renderer.material);
                        renderer.material.color = color;
                    }
                }
            }
        }
        
        /// <summary>
        /// Creates location points within a car based on the available locations list.
        /// </summary>
        private void CreateLocationPoints(GameObject carInstance, List<string> availableLocations)
        {
            // Find the floor transform where we'll add location points
            Transform floor = carInstance.transform.Find("RailCarFloor");
            if (floor == null)
            {
                Debug.LogError($"Car {carInstance.name} has no RailCarFloor child, cannot create location points");
                return;
            }
            
            CarIdentifier carId = carInstance.GetComponent<CarIdentifier>();
            if (carId == null)
            {
                Debug.LogError($"Car {carInstance.name} has no CarIdentifier component");
                return;
            }
            
            // Create a parent object for all locations
            GameObject locationsParent = new GameObject("Locations");
            locationsParent.transform.SetParent(floor);
            locationsParent.transform.localPosition = Vector3.zero;
            
            // Create location points for each available location
            foreach (string locationName in availableLocations)
            {
                GameObject locationObj = new GameObject(locationName);
                locationObj.transform.SetParent(locationsParent.transform);
                
                // Position based on layout algorithm or pattern
                PositionLocationInCar(locationObj, floor, locationName);
                
                // Add identifier component
                LocationIdentifier locationId = locationObj.AddComponent<LocationIdentifier>();
                locationId.LocationId = locationName;
                
                // Add a spawn point for character placement
                SpawnPoint spawnPoint = locationObj.AddComponent<SpawnPoint>();
                spawnPoint.PointType = "character_spawn";
                
                LogDebug($"Created location: {carId.CarId}.{locationName}");
            }
        }
        
        /// <summary>
        /// Positions a location point within a car based on its name or purpose.
        /// </summary>
        private void PositionLocationInCar(GameObject locationObj, Transform floorTransform, string locationName)
        {
            // Calculate bounds of the floor
            Renderer floorRenderer = floorTransform.GetComponent<Renderer>();
            Bounds bounds = floorRenderer != null 
                ? floorRenderer.bounds 
                : new Bounds(floorTransform.position, new Vector3(10f, 0f, 5f));
            
            // Position the location based on its name or randomly if not recognized
            Vector3 localPosition = Vector3.zero;
            
            // Use the location name to determine position if possible
            if (locationName.Contains("front"))
            {
                localPosition = new Vector3(bounds.extents.x * 0.8f, 0.1f, 0);
            }
            else if (locationName.Contains("back") || locationName.Contains("rear"))
            {
                localPosition = new Vector3(-bounds.extents.x * 0.8f, 0.1f, 0);
            }
            else if (locationName.Contains("left"))
            {
                localPosition = new Vector3(0, 0.1f, bounds.extents.z * 0.8f);
            }
            else if (locationName.Contains("right"))
            {
                localPosition = new Vector3(0, 0.1f, -bounds.extents.z * 0.8f);
            }
            else if (locationName.Contains("center"))
            {
                localPosition = new Vector3(0, 0.1f, 0);
            }
            else
            {
                // Random position within bounds if not recognized
                float x = Random.Range(-bounds.extents.x * 0.8f, bounds.extents.x * 0.8f);
                float z = Random.Range(-bounds.extents.z * 0.8f, bounds.extents.z * 0.8f);
                localPosition = new Vector3(x, 0.1f, z);
            }
            
            locationObj.transform.localPosition = localPosition;
        }
        
        /// <summary>
        /// Updates any component references that might need the car ID.
        /// </summary>
        private void UpdateCarReferences(GameObject carInstance, string carId)
        {
            // Update the CarVisibility component if present
            CarVisibility visibility = carInstance.GetComponent<CarVisibility>();
            if (visibility != null)
            {
                // Make sure car starts as not selected
                visibility.CarDeselected();
            }
            
            // Disable any CarCharacters component that might be present
            CarCharacters characterSpawner = carInstance.GetComponent<CarCharacters>();
            if (characterSpawner != null)
            {
                // Disable the random character spawning since we're doing deterministic placement
                characterSpawner.enabled = false;
            }
        }
        
        /// <summary>
        /// Sets up connections between cars as defined in the layout.
        /// </summary>
        private void SetupCarConnections(Dictionary<string, CarConnection> connections)
        {
            foreach (var connection in connections)
            {
                string sourceCarId = connection.Key;
                CarConnection connectionData = connection.Value;
                
                if (string.IsNullOrEmpty(sourceCarId) || connectionData == null || 
                    string.IsNullOrEmpty(connectionData.ConnectedTo))
                {
                    Debug.LogWarning("Invalid connection definition");
                    continue;
                }
                
                GameObject sourceCar = GetCarById(sourceCarId);
                GameObject targetCar = GetCarById(connectionData.ConnectedTo);
                
                if (sourceCar == null || targetCar == null)
                {
                    Debug.LogError($"Cannot create connection between {sourceCarId} and {connectionData.ConnectedTo}: one or both cars not found");
                    continue;
                }
                
                // Create connection between cars
                // This would involve setting up doors, passages, etc.
                LogDebug($"Created connection from {sourceCarId} to {connectionData.ConnectedTo}");
            }
        }
        
        /// <summary>
        /// Gets all generated train cars.
        /// </summary>
        public List<GameObject> GetTrainCars()
        {
            return trainCars;
        }
        
        /// <summary>
        /// Gets a car by its ID.
        /// </summary>
        public GameObject GetCarById(string carId)
        {
            if (carIdToGameObject.TryGetValue(carId, out GameObject car))
            {
                return car;
            }
            
            return null;
        }
        
        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[TrainGenerator] {message}");
            }
        }
    }

    /// <summary>
    /// System for placing entities (characters, evidence) in the world.
    /// </summary>
    public class EntityPlacer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private NPCManager npcManager;
        [SerializeField] private EvidenceManager evidenceManager;
        
        [Header("Settings")]
        [SerializeField] private bool debugMode = true;
        
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
        
        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[EntityPlacer] {message}");
            }
        }
    }

    /// <summary>
    /// Manager for evidence items in the game.
    /// </summary>
    public class EvidenceManager : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject defaultEvidencePrefab;
        [SerializeField] private List<EvidencePrefabMapping> specialEvidencePrefabs = new List<EvidencePrefabMapping>();
        
        [Header("Settings")]
        [SerializeField] private Transform evidenceContainer;
        [SerializeField] private bool debugMode = true;
        
        // Runtime data
        private Dictionary<string, GameObject> activeEvidence = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> prefabMappings = new Dictionary<string, GameObject>();
        
        private void Awake()
        {
            // Create evidence container if not set
            if (evidenceContainer == null)
            {
                GameObject containerObj = new GameObject("Evidence");
                containerObj.transform.SetParent(transform);
                evidenceContainer = containerObj.transform;
                LogDebug("Created evidence container");
            }
            
            // Initialize prefab mappings
            foreach (var mapping in specialEvidencePrefabs)
            {
                if (!string.IsNullOrEmpty(mapping.EvidenceType) && mapping.Prefab != null)
                {
                    prefabMappings[mapping.EvidenceType.ToLower()] = mapping.Prefab;
                    LogDebug($"Registered special evidence prefab for type: {mapping.EvidenceType}");
                }
            }
        }
        
        /// <summary>
        /// Spawns an evidence item at the specified location.
        /// </summary>
        public GameObject SpawnEvidence(string evidenceId, MysteryNode nodeData, Vector3 position, Transform parent = null)
        {
            if (string.IsNullOrEmpty(evidenceId) || nodeData == null)
            {
                Debug.LogError("Cannot spawn evidence with null ID or data");
                return null;
            }
            
            // Check if this evidence is already spawned
            if (activeEvidence.TryGetValue(evidenceId, out GameObject existingEvidence) && existingEvidence != null)
            {
                LogDebug($"Evidence {evidenceId} is already spawned. Moving to new position.");
                existingEvidence.transform.position = position;
                
                if (parent != null && existingEvidence.transform.parent != parent)
                {
                    existingEvidence.transform.SetParent(parent);
                }
                
                return existingEvidence;
            }
            
            try
            {
                // Determine the right prefab to use
                GameObject prefabToUse = GetEvidencePrefab(nodeData);
                
                // Determine the parent transform
                Transform spawnParent = parent != null ? parent : evidenceContainer;
                
                // Create evidence instance
                GameObject evidenceInstance = Instantiate(prefabToUse, position, Quaternion.identity, spawnParent);
                evidenceInstance.name = $"Evidence_{evidenceId}";
                
                // Add an evidence component to store data
                EvidenceComponent evidenceComponent = evidenceInstance.AddComponent<EvidenceComponent>();
                evidenceComponent.EvidenceId = evidenceId;
                evidenceComponent.NodeData = nodeData;
                
                // Add a collider if it doesn't have one
                if (evidenceInstance.GetComponent<Collider>() == null)
                {
                    BoxCollider collider = evidenceInstance.AddComponent<BoxCollider>();
                    collider.size = new Vector3(0.5f, 0.25f, 0.5f);
                    collider.center = new Vector3(0, 0.125f, 0);
                    collider.isTrigger = true;
                }
                
                // Configure appearance based on evidence type
                ConfigureEvidenceAppearance(evidenceInstance, nodeData);
                
                // Store in active evidence dictionary
                activeEvidence[evidenceId] = evidenceInstance;
                LogDebug($"Successfully spawned evidence {evidenceId} at position {position}");
                
                return evidenceInstance;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error spawning evidence {evidenceId}: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Gets the appropriate prefab for an evidence type.
        /// </summary>
        private GameObject GetEvidencePrefab(MysteryNode nodeData)
        {
            string evidenceType = "default";
            
            // Try to determine evidence type from properties
            if (nodeData.Properties != null && nodeData.Properties.TryGetValue("evidence_type", out string typeValue))
            {
                evidenceType = typeValue.ToLower();
            }
            
            // Try to find a matching prefab
            if (prefabMappings.TryGetValue(evidenceType, out GameObject prefab) && prefab != null)
            {
                return prefab;
            }
            
            // Fall back to default
            if (defaultEvidencePrefab == null)
            {
                // Create a simple cube as a last resort
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.localScale = new Vector3(0.5f, 0.25f, 0.5f);
                return cube;
            }
            
            return defaultEvidencePrefab;
        }
        
        /// <summary>
        /// Configures the appearance of an evidence object based on its data.
        /// </summary>
        private void ConfigureEvidenceAppearance(GameObject evidenceInstance, MysteryNode nodeData)
        {
            // Set the main color if we have a renderer
            Renderer renderer = evidenceInstance.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Clone the material to avoid affecting other evidence instances
                renderer.material = new Material(renderer.material);
                
                // Try to get color from properties
                if (nodeData.Properties != null && nodeData.Properties.TryGetValue("color", out string colorStr))
                {
                    if (ColorUtility.TryParseHtmlString(colorStr, out Color color))
                    {
                        renderer.material.color = color;
                    }
                }
            }
        }
        
        /// <summary>
        /// Gets a specific evidence instance by ID.
        /// </summary>
        public GameObject GetEvidenceById(string evidenceId)
        {
            if (activeEvidence.TryGetValue(evidenceId, out GameObject evidence))
            {
                return evidence;
            }
            
            return null;
        }
        
        /// <summary>
        /// Gets all active evidence instances.
        /// </summary>
        public List<GameObject> GetAllEvidence()
        {
            return new List<GameObject>(activeEvidence.Values);
        }
        
        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[EvidenceManager] {message}");
            }
        }
    }

    /// <summary>
    /// Mapping between evidence types and prefabs.
    /// </summary>
    [System.Serializable]
    public class EvidencePrefabMapping
    {
        public string EvidenceType;
        public GameObject Prefab;
    }

    /// <summary>
    /// Component that stores evidence data.
    /// </summary>
    public class EvidenceComponent : MonoBehaviour
    {
        public string EvidenceId;
        public MysteryNode NodeData;
        
        public string GetEvidenceTitle()
        {
            if (NodeData == null)
            {
                return "Unknown Evidence";
            }
            
            return NodeData.Title ?? $"Evidence {EvidenceId}";
        }
        
        public string GetEvidenceDescription()
        {
            if (NodeData == null)
            {
                return "No information available.";
            }
            
            return NodeData.Description ?? "This is a piece of evidence related to the mystery.";
        }
    }

    /// <summary>
    /// Component to identify a train car.
    /// </summary>
    public class CarIdentifier : MonoBehaviour
    {
        [Tooltip("Unique identifier for this car")]
        public string CarId;
        
        [Tooltip("Type of car (e.g., 'dining', 'passenger')")]
        public string CarType;
        
        [Tooltip("Class of car (e.g., 'first_class', 'economy')")]
        public string CarClass;
        
        private void OnValidate()
        {
            // Auto-name the GameObject based on ID
            if (!string.IsNullOrEmpty(CarId) && !string.IsNullOrEmpty(CarType))
            {
                gameObject.name = $"Train Car - {CarType} ({CarId})";
            }
        }
    }

    /// <summary>
    /// Component to identify a location within a car.
    /// </summary>
    public class LocationIdentifier : MonoBehaviour
    {
        [Tooltip("ID of this location within its parent")]
        public string LocationId;
        
        [Tooltip("Type of location (e.g., 'table', 'seat', 'bar')")]
        public string LocationType;
        
        private void OnDrawGizmos()
        {
            // Draw a small sphere to visualize this location in the editor
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position, 0.1f);
        }
    }

    /// <summary>
    /// Component that marks a valid spawn point for entities.
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        [Tooltip("Type of entities that can spawn here (e.g., 'character_spawn', 'evidence_spawn')")]
        public string PointType = "character_spawn";
        
        [Tooltip("Whether this spawn point is available for use")]
        public bool IsAvailable = true;
        
        private void OnDrawGizmos()
        {
            // Draw a small sphere to visualize this spawn point in the editor
            Gizmos.color = IsAvailable ? Color.green : Color.red;
            Gizmos.DrawSphere(transform.position, 0.15f);
        }
    }

    /// <summary>
    /// Data class for car connections in train layout.
    /// </summary>
    [System.Serializable]
    public class CarConnection
    {
        [Tooltip("ID of the car this connects to")]
        public string ConnectedTo;
        
        [Tooltip("Connection type (e.g., 'door', 'passage')")]
        public string ConnectionType = "door";
    }

    /// <summary>
    /// Component to store custom properties for a train car.
    /// </summary>
    public class CarProperties : MonoBehaviour
    {
        private Dictionary<string, string> properties = new Dictionary<string, string>();
        
        public void SetProperties(Dictionary<string, string> props)
        {
            properties = new Dictionary<string, string>(props);
        }
        
        public string GetProperty(string key, string defaultValue = "")
        {
            if (properties.TryGetValue(key, out string value))
            {
                return value;
            }
            
            return defaultValue;
        }
        
        public bool HasProperty(string key)
        {
            return properties.ContainsKey(key);
        }
    }

    /// <summary>
    /// Test runner for validating the Mystery Engine implementation.
    /// </summary>
    public class MysteryEngineTest : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WorldCoordinator worldCoordinator;
        [SerializeField] private TextAsset testMysteryJson;
        
        [Header("Settings")]
        [SerializeField] private bool runTestsOnStart = true;
        [SerializeField] private bool verboseLogging = true;
        
        // Log output
        private List<string> testLog = new List<string>();
        
        void Start()
        {
            if (runTestsOnStart)
            {
                RunTests();
            }
        }
        
        /// <summary>
        /// Runs all tests to validate the implementation.
        /// </summary>
        public void RunTests()
        {
            LogMessage("Starting Mystery Engine tests...", LogLevel.Important);
            
            // Try to find WorldCoordinator if not set
            if (worldCoordinator == null)
            {
                worldCoordinator = FindFirstObjectByType<WorldCoordinator>();
                if (worldCoordinator == null)
                {
                    LogMessage("WorldCoordinator not found in scene. Cannot run tests.", LogLevel.Error);
                    return;
                }
            }
            
            // Parse test mystery
            Mystery testMystery = ParseTestMystery();
            if (testMystery == null)
            {
                LogMessage("Failed to parse test mystery. Tests aborted.", LogLevel.Error);
                return;
            }
            
            // Initialize world with test mystery
            InitializeWorld(testMystery);
            
            // Test initialization completion
            StartCoroutine(TestInitializationComplete());
        }
        
        /// <summary>
        /// Parses the test mystery JSON.
        /// </summary>
        private Mystery ParseTestMystery()
        {
            try
            {
                if (testMysteryJson == null)
                {
                    LogMessage("No test mystery JSON assigned.", LogLevel.Error);
                    return null;
                }
                
                Mystery mystery = JsonConvert.DeserializeObject<Mystery>(testMysteryJson.text);
                LogMessage($"Parsed test mystery: {mystery.Metadata?.Title ?? "Unnamed"}", LogLevel.Info);
                return mystery;
            }
            catch (System.Exception e)
            {
                LogMessage($"Error parsing test mystery: {e.Message}", LogLevel.Error);
                return null;
            }
        }
        
        /// <summary>
        /// Initializes the world with the test mystery.
        /// </summary>
        private void InitializeWorld(Mystery mystery)
        {
            LogMessage("Initializing world with test mystery...", LogLevel.Info);
            worldCoordinator.InitializeWorld(mystery);
        }
        
        /// <summary>
        /// Tests if the world initialization completes successfully.
        /// </summary>
        private IEnumerator TestInitializationComplete()
        {
            float startTime = Time.time;
            float timeoutSeconds = 10f;
            
            while (!worldCoordinator.IsInitialized && Time.time < startTime + timeoutSeconds)
            {
                LogMessage("Waiting for world initialization to complete...", LogLevel.Info);
                yield return new WaitForSeconds(0.5f);
            }
            
            if (!worldCoordinator.IsInitialized)
            {
                LogMessage("World initialization timed out.", LogLevel.Error);
                yield break;
            }
            
            LogMessage("World initialization complete. Running validation tests...", LogLevel.Success);
            
            // Run validation tests
            TestTrainGeneration();
            TestCharacterPlacement();
            TestEvidencePlacement();
            
            // Log final results
            LogMessage("\nAll tests complete!", LogLevel.Important);
        }
        
        /// <summary>
        /// Tests if train cars are generated correctly.
        /// </summary>
        private void TestTrainGeneration()
        {
            LogMessage("Testing train generation...", LogLevel.Info);
            
            // Check if train cars were created
            int carCount = worldCoordinator.GetTrainCarCount();
            LogMessage($"Train car count: {carCount}", carCount > 0 ? LogLevel.Success : LogLevel.Error);
        }
        
        /// <summary>
        /// Tests if characters are placed in their initial locations.
        /// </summary>
        private void TestCharacterPlacement()
        {
            LogMessage("Testing character placement...", LogLevel.Info);
            
            // Check if characters are placed
            int characterCount = 0;
            foreach (var characterId in worldCoordinator.CurrentMystery.Characters.Keys)
            {
                GameObject character = worldCoordinator.GetCharacterById(characterId);
                if (character != null)
                {
                    characterCount++;
                    LogMessage($"Character found: {characterId}", LogLevel.Success);
                }
                else
                {
                    LogMessage($"Character not found: {characterId}", LogLevel.Error);
                }
            }
            
            LogMessage($"Total characters placed: {characterCount}/{worldCoordinator.CurrentMystery.Characters.Count}", 
                characterCount == worldCoordinator.CurrentMystery.Characters.Count ? LogLevel.Success : LogLevel.Warning);
        }
        
        /// <summary>
        /// Tests if evidence items are placed in their locations.
        /// </summary>
        private void TestEvidencePlacement()
        {
            LogMessage("Testing evidence placement...", LogLevel.Info);
            
            // Check if evidence is placed
            int evidenceCount = 0;
            foreach (var nodeEntry in worldCoordinator.CurrentMystery.Constellation.Nodes)
            {
                string nodeId = nodeEntry.Key;
                MysteryNode nodeData = nodeEntry.Value;
                
                if (nodeData.PhysicalEvidence && !string.IsNullOrEmpty(nodeData.Location))
                {
                    GameObject evidence = worldCoordinator.GetEvidenceById(nodeId);
                    if (evidence != null)
                    {
                        evidenceCount++;
                        LogMessage($"Evidence found: {nodeId} at {nodeData.Location}", LogLevel.Success);
                    }
                    else
                    {
                        LogMessage($"Evidence not found: {nodeId} at {nodeData.Location}", LogLevel.Error);
                    }
                }
            }
            
            int physicalEvidenceCount = worldCoordinator.CurrentMystery.Constellation.Nodes.Count(
                n => n.Value.PhysicalEvidence && !string.IsNullOrEmpty(n.Value.Location)
            );
            
            LogMessage($"Total evidence placed: {evidenceCount}/{physicalEvidenceCount}",
                evidenceCount == physicalEvidenceCount ? LogLevel.Success : LogLevel.Warning);
        }
        
        // Logging functionality
        
        private enum LogLevel
        {
            Info,
            Success,
            Warning,
            Error,
            Important
        }
        
        private void LogMessage(string message, LogLevel level = LogLevel.Info)
        {
            // Format for console
            string consoleMessage = $"[MysteryTest] {message}";
            
            // Log to Unity console
            switch (level)
            {
                case LogLevel.Info:
                    if (verboseLogging) Debug.Log(consoleMessage);
                    break;
                case LogLevel.Success:
                    Debug.Log($"<color=green>{consoleMessage}</color>");
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(consoleMessage);
                    break;
                case LogLevel.Error:
                    Debug.LogError(consoleMessage);
                    break;
                case LogLevel.Important:
                    Debug.Log($"<color=yellow><b>{consoleMessage}</b></color>");
                    break;
            }
            
            // Add to test log
            testLog.Add($"{message}");
        }
        
        /// <summary>
        /// Gets all test log messages.
        /// </summary>
        public List<string> GetTestLog()
        {
            return new List<string>(testLog);
        }
        
        /// <summary>
        /// Clears the test log.
        /// </summary>
        public void ClearTestLog()
        {
            testLog.Clear();
        }
    }
}
