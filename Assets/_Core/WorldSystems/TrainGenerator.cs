using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for generating the train layout based on mystery data.
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