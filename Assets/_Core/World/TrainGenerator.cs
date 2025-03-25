using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Responsible for generating the train layout based on mystery data.
/// Creates train cars and positions them according to the layout specification.
/// The train is the backbone of our game, so naturally we're generating it by luck and prayer.
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
    [SerializeField] private GameObject defaultCarPrefab; // Fallback for when everything else fails
    
    [Header("Layout Settings")]
    [SerializeField] private Transform trainParent;
    [SerializeField] private float carSpacing = 25f;
    [SerializeField] private bool debugMode = false;
    
    // Dictionary to store car prefabs by type
    private Dictionary<string, GameObject> carPrefabsByType;
    
    // Lists to track generated cars
    private List<GameObject> trainCars = new List<GameObject>();
    private Dictionary<string, GameObject> carIdToGameObject = new Dictionary<string, GameObject>();
    
    private void Awake()
    {
        // Initialize car prefabs dictionary - case insensitive because designers can't be trusted with capitalization
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
    /// Not to be confused with actual train generation, which is much harder and involves coal.
    /// </summary>
    public void GenerateTrainFromLayout(TrainLayout layout)
    {
        // Clear any existing train cars
        ClearExistingTrainCars();
        
        if (layout == null || layout.Cars == null || layout.Cars.Count == 0)
        {
            Debug.LogError("Invalid train layout data. What kind of mystery has no train? This isn't 'Solve The Bus Incident'");
            return;
        }
        
        LogDebug($"Generating train with {layout.Cars.Count} cars. Choo choo motherf*cker.");
        
        Vector3 spawnPosition = trainParent.position;
        
        // Generate each car in the layout
        foreach (var carDef in layout.Cars)
        {
            if (string.IsNullOrEmpty(carDef.CarId))
            {
                Debug.LogError("Car definition missing CarId. It's like trying to find your train seat without a ticket number.");
                continue;
            }
            
            if (string.IsNullOrEmpty(carDef.CarType))
            {
                Debug.LogWarning($"Car {carDef.CarId} missing CarType, using default because we're too lazy to crash properly.");
                carDef.CarType = "passenger";
            }
            
            // Get the appropriate prefab for this car type
            GameObject carPrefab = GetCarPrefabByType(carDef.CarType);
            
            if (carPrefab != null)
            {
                GameObject carInstance = Instantiate(
                    carPrefab, 
                    spawnPosition, 
                    Quaternion.Euler(0, 180, 0), // Rotated because someone can't model properly, I bet
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
                Debug.LogError($"No prefab found for car type: {carDef.CarType}. Someone forgot to assign references, didn't they?");
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
    /// The digital equivalent of derailing a train and starting over.
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
    /// It's like a very limited vending machine, but for train cars.
    /// </summary>
    private GameObject GetCarPrefabByType(string carType)
    {
        if (carPrefabsByType.TryGetValue(carType, out GameObject prefab) && prefab != null)
        {
            return prefab;
        }
        
        // If we don't have a prefab for this type or it's null, log a warning and use default
        Debug.LogWarning($"No prefab for car type: {carType}, using default because why have consistent failure when we can keep limping along?");
        return defaultCarPrefab != null ? defaultCarPrefab : passengerCarPrefab;
    }
    
    /// <summary>
    /// Applies custom properties to a car instance.
    /// Imagine giving a train car a makeover. A very limited makeover.
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
    /// Because who doesn't want a hot pink dining car?
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
    /// These are the places where characters and evidence can exist, like digital feng shui.
    /// </summary>
    private void CreateLocationPoints(GameObject carInstance, List<string> availableLocations)
    {
        // Find the floor transform where we'll add location points
        Transform floor = carInstance.transform.Find("RailCarFloor");
        if (floor == null)
        {
            Debug.LogError($"Car {carInstance.name} has no RailCarFloor child, cannot create location points. Someone got creative with the prefab, I see.");
            return;
        }
        
        CarIdentifier carId = carInstance.GetComponent<CarIdentifier>();
        if (carId == null)
        {
            Debug.LogError($"Car {carInstance.name} has no CarIdentifier component. How did we even get here?");
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
    /// We're basically playing interior decorator with invisible furniture.
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
        // Because naming conventions are better than random placement, I guess
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
            // Because fuck it, if we can't figure it out, let RNGesus take the wheel
            float x = Random.Range(-bounds.extents.x * 0.8f, bounds.extents.x * 0.8f);
            float z = Random.Range(-bounds.extents.z * 0.8f, bounds.extents.z * 0.8f);
            localPosition = new Vector3(x, 0.1f, z);
        }
        
        locationObj.transform.localPosition = localPosition;
    }
    
    /// <summary>
    /// Updates any component references that might need the car ID.
    /// Fixing legacy code is like defusing a bomb while the timer keeps resetting.
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
            LogDebug($"Disabled random character spawning in car {carId}. Sorry, but your chaos ends here.");
        }
    }
    
    /// <summary>
    /// Sets up connections between cars as defined in the layout.
    /// Basically digital train coupling. Choo choo, bitches.
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
                Debug.LogWarning("Invalid connection definition. It's like trying to connect train cars with dental floss.");
                continue;
            }
            
            GameObject sourceCar = GetCarById(sourceCarId);
            GameObject targetCar = GetCarById(connectionData.ConnectedTo);
            
            if (sourceCar == null || targetCar == null)
            {
                Debug.LogError($"Cannot create connection between {sourceCarId} and {connectionData.ConnectedTo}: one or both cars not found. Can't connect what doesn't exist.");
                continue;
            }
            
            // Create connection between cars
            // This would involve setting up doors, passages, etc.
            LogDebug($"Created connection from {sourceCarId} to {connectionData.ConnectedTo}");
        }
    }
    
    /// <summary>
    /// Gets all generated train cars.
    /// A mundane function for an extraordinary collection of objects.
    /// </summary>
    public List<GameObject> GetTrainCars()
    {
        return trainCars;
    }
    
    /// <summary>
    /// Gets a car by its ID.
    /// It's like a very specific train station announcement.
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
/// It's like giving the car a personality, but without the existential crisis.
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
