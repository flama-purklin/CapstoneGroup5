using UnityEngine;
using System.Collections.Generic;
using System.Linq; 
using System.IO; // Required for Path.Combine

public class TrainLayoutManager : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("The subfolder within Assets/Resources where train car prefabs are located (e.g., 'TrainCars').")]
    public string resourceFolderPath = "TrainCars"; 
    [Tooltip("The exact name of the fallback/shell prefab (without .prefab extension).")]
    public string shellCarPrefabName = "shell_car"; 

    [Header("Spawning Configuration")]
    [Tooltip("The starting point for the first car. Assign an empty GameObject transform.")]
    public Transform spawnPoint;
    [Tooltip("Distance between the centers of spawned cars.")]
    public float carSpacing = 25f;
    [Tooltip("Optional: Parent transform for spawned cars to keep hierarchy clean.")]
    public Transform trainParent; 

    private GameObject shellCarPrefab; // Cached reference to the loaded fallback prefab
    private bool hasBuiltTrain = false;
    private bool setupComplete = false;

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
        // Prevent execution if setup failed (e.g., fallback prefab missing)
        if (!setupComplete)
        {
             Debug.LogError("TrainLayoutManager: Setup incomplete (likely failed to load fallback prefab). Cannot build train.", this);
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

        // Initialize spawning position and rotation
        Vector3 currentSpawnPosition = spawnPoint.position;
        Quaternion spawnRotation = Quaternion.Euler(0, 180, 0); // Consistent rotation

        Debug.Log($"TrainLayoutManager: Building train with {layoutOrder.Count} cars.", this);

        // Loop through the layout order defined in the JSON
        for (int i = 0; i < layoutOrder.Count; i++)
        {
            string carKey = layoutOrder[i];
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
                 Debug.Log($"  Spawning car {i + 1}: Key='{carKey}', Loaded Prefab='{prefabToSpawn.name}' from Resources/{resourcePath}");
            }
            else 
            {
                // Specific prefab not found, use the pre-loaded fallback shell
                prefabToSpawn = shellCarPrefab; 
                Debug.LogWarning($"  Spawning car {i + 1}: Key='{carKey}' not found at Resources/{resourcePath}. Using fallback Shell Prefab ('{shellCarPrefab?.name ?? "NULL"}').");
            }

            // Instantiate the chosen prefab (ensure it's not null)
            if (prefabToSpawn != null)
            {
                GameObject carInstance = Instantiate(prefabToSpawn, currentSpawnPosition, spawnRotation);
                
                // Parent the instance if a parent transform is assigned
                if (trainParent != null) 
                {
                    carInstance.transform.SetParent(trainParent);
                }
                
                // Name the instance clearly for easier debugging in the hierarchy
                carInstance.name = $"TrainCar_{i}_{carKey}";

                // --- Calculate next position based on the actual bounds of the spawned car ---
                float carLength = carSpacing; // Use configured spacing as a fallback
                Collider carCollider = carInstance.GetComponent<Collider>(); // Get the main collider

                if (carCollider != null)
                {
                    // Use the collider's bounds size along the X-axis for accurate length
                    carLength = carCollider.bounds.size.x;
                    // Optional: Add a tiny buffer if needed for visual separation, e.g., carLength += 0.01f;
                    Debug.Log($"  Car {i + 1} ('{carKey}') actual length from Collider: {carLength}");
                }
                else
                {
                     Debug.LogWarning($"  Car {i + 1} ('{carKey}') has no Collider component. Falling back to configured carSpacing ({carSpacing}) for positioning. This might cause gaps/overlaps.");
                }

                // Update the position for the next car using the calculated or fallback length
                currentSpawnPosition += Vector3.right * carLength;
            }
            else
            {
                 // This error should only occur if the fallback prefab failed to load in Awake
                 Debug.LogError($"  Failed to spawn car {i + 1}: Prefab for key '{carKey}' not found AND fallback shell is missing!", this);
            }
        }

        hasBuiltTrain = true; // Mark the train as built
        Debug.Log("TrainLayoutManager: Train build process complete.", this);
    }
}
