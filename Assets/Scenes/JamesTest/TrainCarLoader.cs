using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Collections;
using Unity.AI.Navigation;
using UnityEngine.AI;
using static TrainManager;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
// Struct for storing data from Json
public class TrainCarObject
{
    public int[] position; // [row, column]
    public string type; // Type of object (keyword)
    public int rotation; // Rotation angle (0 is down? (-z facing?))
}

// Stores the info of each cart, will be used to save the generated carts
[System.Serializable]
public class TrainCarLayout
{
    public List<TrainCarObject> train_car;
}

// GIves context to items in list, kind of like a dictionary. Can add theme for addtional variety 
[System.Serializable]
public class PrefabEntry
{
    public string key;
    public CarClass carClass; // Allows for palet swaps.
    public CarType carType; // Allows different pallets for the Special cars.
    //public string theme;
    public GameObject prefab;
}

[System.Serializable]
public class CarLoadConfig
{
    public TextAsset layoutJson;
    public CarClass carClass;
    public CarType carType;
}

public class TrainCarLoader : MonoBehaviour
{
    [SerializeField] public TextAsset jsonFile; // Assign JSON in inspector
    [SerializeField] public RailCarRandomizer railCarRandomizer; // Assign RailCarRandomizer script
    [SerializeField] public bool spawnOnStart = false; // Added for support with placement scene?
    [SerializeField] private TrainCarLayout trainCarLayout;
    [SerializeField] public List<PrefabEntry> objectPrefabsList;
    [SerializeField] public CarLoadConfig currentConfig; // To be set before each JSON is run.
    [SerializeField] public string prefabSavePath = "Assets/Scenes/JamesTest/Prefabs/";
    [SerializeField] public CarClass selectedCarClass = CarClass.SecondClass;
    [SerializeField] public CarType selectedCarType = CarType.Passenger;

    private List<GameObject> anchorPoints = null; // Stores all anchor points, instantiate at runtime after railCarRandomizer runs
                                                  // Above is of naming convention $"Anchor ({row}, {col})"
    //private Dictionary<string, GameObject> objectPrefabs; // Internal dictionary for internal use (dicts dont show in inspector)

    void Start()
    {
        // Dictonary lookup not needed/not robust enough. Just querie the objectPrefabsList directly.
/*        // Convert List to Dictionary
        objectPrefabs = new Dictionary<string, GameObject>();
        foreach (PrefabEntry entry in objectPrefabsList)
        {
            objectPrefabs[entry.key] = entry.prefab;
        }*/

        // *NOte, unneded as method modified to just take in a reference to the train car shell object.
        // Wait for RailCarRandomizer to finish generating the train shell
        if (spawnOnStart == true)
        {
            StartCoroutine(WaitForTrainShell());
        }

        if (currentConfig == null)
        {
            currentConfig = new CarLoadConfig();
            currentConfig.layoutJson = jsonFile;
            currentConfig.carClass = selectedCarClass;
            currentConfig.carType = selectedCarType;
        }
    }

    // *NOte, unneded as method modified to just take in a reference to the train car shell object.
    // *Note, still used for now in the train car placement scene
    // Coroutine to wait for the floor to be generated
    IEnumerator WaitForTrainShell()
    {
        // Wait until shell and anchors are generated in RailCarRandomizer
        while (railCarRandomizer.spawnSuccessful == false)
        {
            yield return null;
        }

        // Below triggers after floor is generated (should mean rCR is done but check anchor points are done to be safe)
        if (railCarRandomizer.anchorPoints != null)
        {
            this.anchorPoints = railCarRandomizer.anchorPoints;
        }
        LoadTrainCarLayout();
        PlaceObjects();
    }

    // Use JsonTility to parse data
    void LoadTrainCarLayout()
    {
        if (jsonFile == null)
        {
            Debug.LogError("JSON file is missing!");
            return;
        }

        trainCarLayout = JsonUtility.FromJson<TrainCarLayout>(jsonFile.text);
    }

    // To be called outside of the class, uses previously spawned shell so RailCarRandomizer.SpawnShell() must be called first to set it up
    public GameObject PopulateTrain(TextAsset json)
    {
        // Load shell, setup loader for placement, return finished object
        GameObject shell = railCarRandomizer.SpawnShell();
        this.anchorPoints = railCarRandomizer.anchorPoints; // Gets anchor points of current shell
        this.jsonFile = json;
        this.currentConfig.layoutJson = jsonFile;
        LoadTrainCarLayout();
        PlaceObjects();
        return railCarRandomizer.trainCar;
    }

    // To be called outside of the class, uses previously spawned shell so RailCarRandomizer.SpawnShell() must be called first to set it up (allows easy changing of pallet between each run)
    public GameObject PopulateTrain(CarLoadConfig config)
    {
        // Load shell, setup loader for placement, return finished object
        GameObject shell = railCarRandomizer.SpawnShell();
        this.anchorPoints = railCarRandomizer.anchorPoints; // Gets anchor points of current shell
        this.jsonFile = config.layoutJson;
        this.selectedCarClass = config.carClass;
        this.selectedCarType = config.carType;
        this.currentConfig = config; // Above should be redundent with this assignment but better safe than sorry
        LoadTrainCarLayout();
        PlaceObjects();
        return railCarRandomizer.trainCar;
    }

    // Places objects at anchor points, not just in world. $"Anchor ({row}, {col})". Starts at -z, -x of floor. Populates twords positive z first, then positive x.
    // This is anoying but doesnt effect refult.
    void PlaceObjects()
    {
        if (trainCarLayout == null || railCarRandomizer == null || objectPrefabsList == null)
        {
            Debug.LogError("Missing references in TrainCarLoader!");
            return;
        }

        Vector3 floorPosition = railCarRandomizer.floor.transform.position;
        float startX = floorPosition.x + (railCarRandomizer.railCarLength / 2) - (railCarRandomizer.cellSize / 2);
        float startZ = floorPosition.z - (railCarRandomizer.railCarDepth / 2) + (railCarRandomizer.cellSize / 2);
        float y = floorPosition.y + (railCarRandomizer.floorThickness / 2);

        // Note* grid starts from top right, aka -z, -x. so populates top right, to bottom left, rows aka z's first.
        foreach (TrainCarObject obj in trainCarLayout.train_car)
        {
            float x = startX - obj.position[1] * railCarRandomizer.cellSize;
            float z = startZ + obj.position[0] * railCarRandomizer.cellSize;
            Vector3 position = new Vector3(x, y, z);

            // Use helper method to lookup the prefab to spawn based on currentConfig
            GameObject prefab = GetMatchingPrefab(obj.type, currentConfig.carClass, currentConfig.carType);
            if (prefab != null)
            {
                string anchorName = $"Anchor ({obj.position[0]}, {obj.position[1]})";
                GameObject instance = Instantiate(prefab, position, Quaternion.Euler(0, obj.rotation, 0));
                var anchor = anchorPoints.Find(a => a.name == anchorName); // Prevents null when anchor not found
                if (anchor == null)
                {
                    Debug.LogWarning($"Anchor not found: {anchorName}");
                    continue;
                }
                instance.transform.SetParent(anchor.transform);
                instance.name = $"{obj.type} ({obj.position[0]}, {obj.position[1]})";
            }
        }

        // Finish prefab setup and save if in editor
        BakeNavMesh();
        SaveAsPrefab(railCarRandomizer.trainCar);
    }

    // Get component on floor, get objects and bake around them
    public void BakeNavMesh()
    {
        GameObject floor = railCarRandomizer.floor;
        NavMeshSurface navMeshSurface = floor.GetComponent<NavMeshSurface>();
        navMeshSurface.collectObjects = CollectObjects.Children;
        navMeshSurface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
        navMeshSurface.layerMask = ~0; // Was "LayerMask.GetMask("Default")", but since floor is on "TrainFloor" may have caused errors.
        navMeshSurface.BuildNavMesh();
    }

    private GameObject GetMatchingPrefab(string key, CarClass carClass, CarType carType)
    {
        // Try full match: key + class + type (basically used wehen type==special)
        foreach (var entry in objectPrefabsList)
        {
            if (entry.key == key && entry.carClass == carClass && entry.carType == carType)
                return entry.prefab;
        }

        // Fallback: match only on key + class
        foreach (var entry in objectPrefabsList)
        {
            if (entry.key == key && entry.carClass == carClass)
                return entry.prefab;
        }

        // Fallback: match only on key (good for generics like 'walkway')
        foreach (var entry in objectPrefabsList)
        {
            if (entry.key == key)
                return entry.prefab;
        }

        Debug.LogWarning($"No prefab found for key '{key}' with class '{carClass}' and type '{carType}'.");
        return null;
    }

    // WARNING* "Debug.LogError("Prefab saving is only supported in the Unity Editor.");"
    // This may end up being a big problem. May have to generate each from json on level load. Which should be fine.
    public void SaveAsPrefab(GameObject runtimeObject)
    {
#if UNITY_EDITOR
        // Should use File.Exists(fullPath) to match file, not directory. This way overwrites existing prefabs, fine for now since still changing.
        // Ensure save path exists
        if (!System.IO.Directory.Exists(prefabSavePath))
        {
            System.IO.Directory.CreateDirectory(prefabSavePath);
        }

        // Define full save path (added class and type to name)
        string fullPath = prefabSavePath + runtimeObject.name + $"_{currentConfig.carClass.ToString()}_{currentConfig.carType.ToString()}(" + jsonFile.name + ").prefab";

        // Ensure no duplicate exists
        if (System.IO.Directory.Exists(fullPath))
        {
            Debug.Log($"Prefab of same name exists at: {fullPath}.");
            Debug.Log("Runtime object not saved!");
            return;
        }

        // Create the prefab
        PrefabUtility.SaveAsPrefabAsset(runtimeObject, fullPath);

        Debug.Log($"Prefab saved to {fullPath}");
#else
        // This may end up being a big problem. May have to generate each from json on level load. Which should be fine.
        Debug.LogError("Prefab saving is only supported in the Unity Editor.");
#endif
    }

}