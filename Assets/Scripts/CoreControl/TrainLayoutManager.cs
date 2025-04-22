// using UnityEngine; // Duplicate removed
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO; // Required for Path.Combine
using UnityEngine.AI; // Added for potential NavMesh use

public class TrainLayoutManager : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("The subfolder within Assets/Resources where train car prefabs are located (e.g., 'TrainCars').")]
    public string resourceFolderPath = "TrainCars";
    [Tooltip("The exact name of the fallback/shell prefab (without .prefab extension).")]
    public string shellCarPrefabName = "shell_car";
    // Added by James, for future use
    [Tooltip("The subfolder within Assets/Resources where train car jsons are located (e.g., 'TrainJsons').")]
    public string jsonFolderPath = "TrainJsons";
    [Tooltip("The exact name of the fallback/shell json (without .json extension).")]
    public string shellCarJsonName = "shell";

    [Header("Spawning Configuration")]
    [Tooltip("The starting point for the first car. Assign an empty GameObject transform.")]
    public Transform spawnPoint;
    [Tooltip("Distance between the centers of spawned cars.")]
    public float carSpacing = 25f;
    [Tooltip("Optional: Parent transform for spawned cars to keep hierarchy clean.")]
    public Transform trainParent;
    // Added by James, I know its not pretty
    public TrainManager trainManager; // Should have couroutine to find if not set in editor
    public bool spawnFromPrefab = true; // If false will spawn from json files

    private GameObject shellCarPrefab; // Cached reference to the loaded fallback prefab
    private bool hasBuiltTrain = false;
    private bool setupComplete = false;
    private Dictionary<string, GameObject> carInstanceMap = new Dictionary<string, GameObject>(); // Map car keys to spawned instances
    private Dictionary<Transform, List<Transform>> usedAnchorsThisSpawn = new Dictionary<Transform, List<Transform>>(); // Track used anchors per car during a spawn sequence

    void Awake()
    {
        // Load the fallback prefab immediately and check setup status
        LoadFallbackPrefab();
        setupComplete = (shellCarPrefab != null);

        if (spawnPoint == null)
        {
            Debug.LogWarning("TrainLayoutManager: Spawn Point is not assigned in the Inspector. Using this object's position as default.", this);
            spawnPoint = transform;
        }
        if (trainManager == null)
        {
            trainManager = FindFirstObjectByType<TrainManager>();
            if (trainManager == null)
            {
                 Debug.LogError("TrainLayoutManager: TrainManager reference not assigned in Inspector and not found in scene! Car spawning will fail.", this);
                 setupComplete = false; // Prevent building if TrainManager is missing
            }
        }
    }

    /// <summary>
    /// Loads and caches the essential fallback shell prefab from the Resources folder.
    /// </summary>
    private void LoadFallbackPrefab()
    {
        // Construct the path within the Resources folder
        string path = Path.Combine(resourceFolderPath, shellCarPrefabName);

        // Attempt to load the GameObject prefab
        shellCarPrefab = Resources.Load<GameObject>(path);

        if (shellCarPrefab == null)
        {
            // Log an error if the fallback prefab cannot be loaded - this is critical
            Debug.LogError($"TrainLayoutManager: CRITICAL - Failed to load fallback Shell Car Prefab from Resources path: 'Resources/{path}'. Train building will fail!", this);
        }
        else
        {
             // Log success for confirmation
             Debug.Log($"TrainLayoutManager: Successfully loaded fallback prefab '{shellCarPrefab.name}' from 'Resources/{path}'", this);
        }
    }

    /// <summary>
    /// Builds the train layout based on the loaded mystery data.
    /// Should be called by InitializationManager after mystery parsing is complete.
    /// </summary>
    public void BuildTrainLayout()
    {
        // Prevent execution if setup failed (e.g., fallback prefab missing or TrainManager missing)
        if (!setupComplete)
        {
             Debug.LogError("TrainLayoutManager: Setup incomplete (likely failed to load fallback prefab or missing TrainManager). Cannot build train.", this);
             return;
        }
        // Prevent building the train more than once
        if (hasBuiltTrain)
        {
            Debug.LogWarning("TrainLayoutManager: Train has already been built. Ignoring duplicate call.", this);
            return;
        }

        Debug.Log("TrainLayoutManager: Starting train build process...", this);

        // --- Crucial Prerequisite: Delete Static Cars ---
        // Assumes static train cars are DELETED from the scene hierarchy in the Editor.

        // Validate that necessary game data is available
        if (GameControl.GameController?.coreMystery?.Environment?.LayoutOrder == null)
        {
            Debug.LogError("TrainLayoutManager: Cannot build train. Mystery data, Environment, or LayoutOrder not found in GameControl.", this);
            return;
        }

        List<string> layoutOrder = GameControl.GameController.coreMystery.Environment.LayoutOrder;

        if (layoutOrder.Count == 0)
        {
            Debug.LogWarning("TrainLayoutManager: Layout order in mystery JSON is empty. No train cars will be spawned.", this);
            return;
        }
        

        if (trainManager != null)
        {
            trainManager.carPrefabs.Clear();
            trainManager.carJsons.Clear();
            trainManager.carJsonConfigs.Clear();
        }
        else
        {
             Debug.LogError("TrainLayoutManager: TrainManager reference is null in BuildTrainLayout. Cannot proceed.", this);
             return; // Cannot proceed without TrainManager
        }


        // Initialize spawning position and rotation
        Vector3 currentSpawnPosition = spawnPoint.position; // This position isn't used if TrainManager handles instantiation
        Quaternion spawnRotation = Quaternion.Euler(0, 180, 0); // Consistent rotation

        Debug.Log($"TrainLayoutManager: Building train with {layoutOrder.Count} cars.", this);

        // Logic for reverse order spawning to preserve json's intended layout, keeping references to use in naming
        List<string> reversedLayout = new List<string>(layoutOrder);
        reversedLayout.Reverse();

        // Loop through the layout order defined in the JSON
        for (int i = 0; i < reversedLayout.Count; i++)
        {
            if (spawnFromPrefab)
            {
                trainManager.spawnWithJsons = false;    // This is set more times than needs to be but idc

                string carKey = reversedLayout[i];
                GameObject prefabToLoad = null;
                // Construct the path within the Resources folder (e.g., "TrainCars/first_class")
                string resourcePath = Path.Combine(resourceFolderPath, carKey);

                // Try loading the specific prefab based on the JSON key
                prefabToLoad = Resources.Load<GameObject>(resourcePath);

                // Determine which prefab to actually instantiate
                GameObject prefabToSpawn;
                if (prefabToLoad != null)
                {
                    // Specific prefab found, use it
                    prefabToSpawn = prefabToLoad;
                    Debug.Log($"  Adding car {i + 1}: Key='{carKey}', Loaded Prefab='{prefabToSpawn.name}' from Resources/{resourcePath}");
                }
                else
                {
                    // Specific prefab not found, use the pre-loaded fallback shell
                    prefabToSpawn = shellCarPrefab;
                    Debug.LogWarning($"  Adding car {i + 1}: Key='{carKey}' not found at Resources/{resourcePath}. Using fallback Shell Prefab ('{shellCarPrefab?.name ?? "NULL"}').");
                }

                // Add prefab to TrainManager.cs's carPrefab list (ensure prefabToSpawn is not null)
                if (prefabToSpawn != null)
                {
                    trainManager.carPrefabs.Add(prefabToSpawn);
                }
                else
                {
                    // This error should only occur if the fallback prefab failed to load in Awake
                    Debug.LogError($"  Failed to add car {i + 1}: Prefab for key '{carKey}' not found AND fallback shell is missing!", this);
                }
            }
            else
            {
                // Logic here to spawn from jsons, basically the same  as above, just add the jsons to the carJsons instead
                trainManager.spawnWithJsons = true;    // This is set more times than needs to be but idc

                string carKey = reversedLayout[i];
                TextAsset jsonToLoad = null;
                // Construct the path within the Resources folder (e.g., "TrainJsons/shell.json")
                string resourcePath = Path.Combine(jsonFolderPath, carKey);

                // Try loading the specific json based on the JSON key
                jsonToLoad = Resources.Load<TextAsset>(resourcePath);

                // Determine which json to actually instantiate
                TextAsset jsonToSpawn;
                if (jsonToLoad != null)
                {
                    // Specific json found, use it
                    jsonToSpawn = jsonToLoad;
                    Debug.Log($"  Adding car {i + 1}: Key='{carKey}', Loaded Json='{jsonToSpawn.name}' from Resources/{resourcePath}");
                }
                else
                {
                    // Specific json not found, use the pre - loaded fallback shell json
                    jsonToSpawn = Resources.Load<TextAsset>(Path.Combine(jsonFolderPath, shellCarJsonName));
                    if (jsonToSpawn != null)
                    {
                        Debug.LogWarning($"  Adding car {i + 1}: Key='{carKey}' not found. Using fallback Shell Json ('{jsonToSpawn.name}') from Resources/{jsonFolderPath}/{shellCarJsonName}");
                    }
                    else
                    {
                        Debug.LogError($"  Failed to load fallback shell JSON ('{shellCarJsonName}') from Resources/{jsonFolderPath}.");
                    }
                }

                // Add json to TrainManager.cs's carJson list (ensure jsonToSpawn is not null)
                if (jsonToSpawn != null)
                {
                    trainManager.carJsons.Add(jsonToSpawn);
                }
                else
                {
                    // This error should only occur if the fallback Json failed to load in Awake
                    Debug.LogError($"  Failed to add car {i + 1}: Json for key '{carKey}' not found AND fallback shell is missing!", this);
                }
            }
        }

        // Setup TrainManager script and call it to spawn cars added to its lists above.
        trainManager.carSpacing = carSpacing;
        trainManager.spawnPoint = spawnPoint;
        trainManager.SpawnCars(); // TrainManager now handles instantiation

        NameCars(); // NameCars now needs to find the instances from TrainManager

        hasBuiltTrain = true; // Mark the train as built
        Debug.Log("TrainLayoutManager: Train build process complete.", this);
    }

    /// <summary>
    /// Clears the tracking of used anchor points. Call this before starting a new NPC spawn sequence.
    /// </summary>
    public void ResetUsedAnchorTracking()
    {
        usedAnchorsThisSpawn.Clear();
        // Debug.Log("Reset used anchor tracking.");
    }

    // Modified to handle duplicate car keys with suffixes (_1, _2, etc.) for map population
    private void NameCars()
    {
        List<string> layoutOrder = GameControl.GameController.coreMystery.Environment.LayoutOrder;
        Dictionary<string, int> carTypeCounts = new Dictionary<string, int>(); // To track counts for suffixes

        // Logic for reverse order spawning to preserve json's intended layout, keeping references to use in naming
        List<string> reversedLayout = new List<string>(layoutOrder);
        reversedLayout.Reverse();

        if (trainManager == null || trainManager.trainCarList == null || trainManager.trainCarList.Count != layoutOrder.Count)
        {
             Debug.LogError($"NameCars: TrainManager list is null or count ({trainManager?.trainCarList?.Count ?? -1}) doesn't match layout order ({layoutOrder.Count}). Cannot name cars.");
             return;
        }


        for (int i = 0; i < reversedLayout.Count; i++)
        {
            string baseCarKey = reversedLayout[i]; // e.g., "business_class"
            string finalCarKey = baseCarKey; // Key to use for map and lookup

            // Check if this car type has appeared before
            if (carTypeCounts.ContainsKey(baseCarKey))
            {
                carTypeCounts[baseCarKey]++;
                finalCarKey = $"{baseCarKey}_{carTypeCounts[baseCarKey]}"; // Add suffix, e.g., "business_class_2"
            }
            else
            {
                // First time seeing this type, count starts at 1
                carTypeCounts[baseCarKey] = 1;
                // Check if ANY other car in the layout has the same base key. If so, the first one also needs "_1"
                bool needsSuffix = reversedLayout.FindAll(key => key == baseCarKey).Count > 1;
                if (needsSuffix)
                {
                     finalCarKey = $"{baseCarKey}_1"; // Add suffix, e.g., "business_class_1"
                }
                // If it's unique, finalCarKey remains baseCarKey
            }

            // Ensure the car GameObject exists before trying to rename and map
            GameObject currentCarInstance = trainManager.trainCarList[i]?.trainCar; // Get instance from TrainManager's list
            if (currentCarInstance != null)
            {
                // Name the GameObject (optional, but good for debugging)
                currentCarInstance.name = $"TrainCar_{i}_{finalCarKey}"; // Use the potentially suffixed key in the name

                // Populate the map using the potentially suffixed key
                carInstanceMap[finalCarKey] = currentCarInstance;
                 
            }
            else
            {
                Debug.LogWarning($"NameCars: GameObject at index {i} in trainManager.trainCarList is null. Cannot rename or map key '{finalCarKey}'.");
            }
        }
         
    }

    /// Like getCarTransform but for the object reference.
    public GameObject GetCarReference(string carNameKey)
    {
        if (trainManager == null || trainManager.trainCarList == null)
        {
            Debug.LogError("GetCarReference: TrainManager or its trainCarList is not available.");
            return null;
        }
        if (string.IsNullOrEmpty(carNameKey))
        {
            Debug.LogError("GetCarReference: Provided carNameKey is null or empty.");
            return null;
        }

        carInstanceMap.TryGetValue(carNameKey, out GameObject carInstance);

        if (carInstance == null)
        {
            Debug.LogError($"GetCarReference: Found key '{carNameKey}' in map, but the GameObject reference is null!");
            return null;
        }
        return carInstance;
    }

    /// <summary>
    /// Finds the Transform of a spawned train car based on its key name from the layout.
    /// </summary>
    /// <param name="carNameKey">The key name of the car (e.g., "first_class", "dining_car").</param>
    /// <returns>The Transform of the car GameObject, or null if not found.</returns>
    public Transform GetCarTransform(string carNameKey)
    {
        if (trainManager == null || trainManager.trainCarList == null)
        {
            Debug.LogError("GetCarTransform: TrainManager or its trainCarList is not available.");
            return null;
        }
        if (string.IsNullOrEmpty(carNameKey))
        {
             Debug.LogError("GetCarTransform: Provided carNameKey is null or empty.");
             return null;
        }

        // Use the map populated during NameCars for direct lookup
        if (carInstanceMap.TryGetValue(carNameKey, out GameObject carInstance))
        {
             if (carInstance == null) {
                 Debug.LogError($"GetCarTransform: Found key '{carNameKey}' in map, but the GameObject reference is null!");
                 return null;
             }
            return carInstance.transform;
        }
        else
        {
            Debug.LogWarning($"GetCarTransform: Could not find car instance for key '{carNameKey}' in the map.");
            return null;
        }
    }

    /// <summary>
    /// Gets a suitable spawn point position within the specified car.
    /// Currently tries to find a child named "SpawnPoint", otherwise returns the car's center.
    /// </summary>
    /// <param name="carNameKey">The key name of the car (e.g., "first_class").</param>
    /// <returns>A Vector3 position for spawning, or Vector3.zero if the car is not found.</returns>
    public Vector3 GetSpawnPointInCar(string carNameKey)
    {
        Transform carTransform = GetCarTransform(carNameKey);
        if (carTransform == null)
        {
            Debug.LogError($"GetSpawnPointInCar: Could not find car transform for key '{carNameKey}'. Returning Vector3.zero.");
            return Vector3.zero; // Indicate failure
        }

        Transform chosenAnchor = null;
        Transform floorTransform = carTransform.Find("RailCarFloor");

        if (floorTransform != null)
        {
            List<Transform> anchors = new List<Transform>();
            foreach (Transform child in floorTransform)
            {
                if (child.name.StartsWith("Anchor"))
                {
                    anchors.Add(child);
                }
            }

            if (anchors.Count > 0)
            {
                // Attempt 1: Find a specific central anchor (e.g., "Anchor (3, 7)")
                string targetAnchorName = "Anchor (3, 7)"; // Or adjust based on typical grid size
                chosenAnchor = anchors.FirstOrDefault(a => a.name == targetAnchorName);

                // Fallback 1: If specific anchor not found, try finding *any* anchor not at (0,0) or similar edge
                if (chosenAnchor == null)
                {
                    chosenAnchor = anchors.FirstOrDefault(a => !a.name.Contains("(0,") && !a.name.Contains(", 0)"));
                     if (chosenAnchor != null) Debug.LogWarning($"GetSpawnPointInCar: Target anchor '{targetAnchorName}' not found in '{carNameKey}'. Using fallback anchor '{chosenAnchor.name}'.");
                }


                // Fallback 2: If still no suitable anchor, just use the first one found
                if (chosenAnchor == null)
                {
                    chosenAnchor = anchors[0];
                    Debug.LogWarning($"GetSpawnPointInCar: Target anchor '{targetAnchorName}' and non-edge anchors not found in '{carNameKey}'. Using first available anchor '{chosenAnchor.name}'.");
                }
            }
            else
            {
                 Debug.LogWarning($"GetSpawnPointInCar: No 'Anchor' children found under 'RailCarFloor' in car '{carNameKey}'. Trying car center fallback.");
            }
        }
        else
        {
             Debug.LogError($"GetSpawnPointInCar: Could not find 'RailCarFloor' child in car '{carNameKey}'. Returning car center as fallback.");
        }

        // Proceed if we found an anchor
        if (chosenAnchor != null)
        {
             // Debug.Log($"[GetSpawnPoint Debug] Chosen Anchor for '{carNameKey}': {chosenAnchor.name}");

             // Find the "walkway" child of the chosen anchor
             Transform walkwayChild = chosenAnchor.Find("walkway"); // Assuming name is exactly "walkway"
             if (walkwayChild == null)
             {
                 // Try finding by partial name in case it's like "walkway (x, y)"
                 foreach (Transform child in chosenAnchor)
                 {
                     if (child.name.StartsWith("walkway"))
                     {
                         walkwayChild = child;
                         break;
                     }
                 }
             }

             if (walkwayChild != null)
             {
                 Vector3 targetPos = walkwayChild.position;
                 // Debug.Log($"[GetSpawnPoint Debug] Found walkway '{walkwayChild.name}' under '{chosenAnchor.name}'. Target Position: {targetPos}");
                 // Debug.Log($"GetSpawnPointInCar: Targeting anchor '{chosenAnchor.name}', walkway '{walkwayChild.name}' at world position {targetPos} in '{carNameKey}'."); // Redundant log removed

                 // Find the closest point on the NavMesh to the walkway position
                 if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
                 {
                     // Debug.Log($"[GetSpawnPoint Debug] NavMesh.SamplePosition SUCCESS near walkway. Returning validated point: {hit.position}");
                     // Debug.Log($"GetSpawnPointInCar: Found valid NavMesh point {hit.position} near walkway '{walkwayChild.name}'."); // Redundant log removed
                     // Removed anchor tracking: usedInThisCar.Add(chosenAnchor);
                     return hit.position; // Return the valid NavMesh point
                 }
                 else
                 {
                     Debug.LogWarning($"GetSpawnPointInCar: Could not find valid NavMesh point within 5.0 units of walkway '{walkwayChild.name}' ({targetPos}) in '{carNameKey}'. Trying car center fallback.");
                 }
             }
             else
             {
                 Debug.LogWarning($"GetSpawnPointInCar: Could not find 'walkway' child under chosen anchor '{chosenAnchor.name}' in '{carNameKey}'. Trying car center fallback.");
             }
        }


        // Fallback: Sample NavMesh near the car's center position
        if (NavMesh.SamplePosition(carTransform.position, out NavMeshHit centerHit, 5.0f, NavMesh.AllAreas))
        {
            Debug.LogWarning($"GetSpawnPointInCar: Using NavMesh point near center as fallback: {centerHit.position}");
            return centerHit.position;
        }
        else
        {
            // Attempt 5 (Last Resort): Return car center anyway, but warn more severely
             Debug.LogError($"GetSpawnPointInCar: Failed to find valid anchor AND failed to sample NavMesh near center for car '{carNameKey}'. Returning raw car center ({carTransform.position}) - NPC NavMeshAgent WILL likely fail!");
            return carTransform.position;
        }
    }
}
