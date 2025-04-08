using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static TMPro.TMP_Compatibility;

public class RailCarRandomizer : MonoBehaviour
{
    [SerializeField] public bool spawnOnStart = false;
    [SerializeField] public float railCarLength = 10f;
    [SerializeField] public float railCarDepth = 5f;
    [SerializeField] public float railCarHeight = 5.5f;
    [SerializeField] public float wallThickness = 0.5f;
    [SerializeField] public float floorThickness = 0.5f;
    [SerializeField] public Material floorMaterial = null;
    [SerializeField] public Material wallMaterial = null;
    [SerializeField] public Material roofMaterial = null;
    [SerializeField] public Material exteriorMaterial = null; // spelled wrong I think
    public GameObject trainCar;
    public GameObject floor, roof;
    public GameObject wall, exteriorWall, leftWall, rightWall;
    public CarVisibility carVisibilityComp;
    public CarCharacters carCharacterComp;

    [SerializeField] public int gridRows = 5;
    [SerializeField] public int gridColumns = 10;
    [SerializeField] public float cellSize = 1f; // TODO: cell size should be auto calc'd based on floor size.
    [SerializeField] public GameObject anchorPrefab = null;
    [SerializeField] public bool anchorsVissible = false;

    public GameObject backWallPrefab = null;
    public GameObject leftWallPrefab = null;
    public GameObject frontWallPrefab = null;
    public GameObject roofPrefab = null;
    public bool spawnWithWallPrefabs = false;

    [SerializeField] public int numbToSpawn = 10; // How many objects are spawned
    [SerializeField] public List<GameObject> objectsToSpawn; // List to randomize
    [SerializeField] public GameObject testNpcPrefab = null;
    public bool spawnSuccessful = false; //Flag to signal shell creation

    public List<GameObject> anchorPoints; // Stores all anchor points


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (spawnOnStart)
        {
            GenerateTrainShell();
            AttachTrainScripts();
            GenerateAnchorGrid();
            //SpawnObjects();
        }
    }

    // Update is called once per frame
    void Update()
    {
/*        // Adds runtime functionality to selected bool (should not be here, no work)
        if (carVisibilityComp.selected == true && !roof.activeSelf)
        {
            carVisibilityComp.CarSelected();
        }
        else if (carVisibilityComp.selected == false && roof.activeSelf)
        {
            carVisibilityComp.CarDeselected();
        }*/
    }

    // Enables spawning shell from outside components. Returns the gameobject or use int TrainCarLoader
    public GameObject SpawnShell()
    {
        GenerateTrainShell();
        AttachTrainScripts();
        GenerateAnchorGrid();
        return trainCar;
    }

    public void GenerateTrainShell()
    {
        spawnSuccessful = false;
        trainCar = new GameObject("Train Car");

        if (spawnWithWallPrefabs == true)
        {
            floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.localScale = new Vector3(railCarLength, floorThickness, railCarDepth); // x, y, z
            floor.name = "RailCarFloor";
            floor.transform.localPosition = new Vector3(0, floorThickness / 2, 0);
            floor.transform.SetParent(trainCar.transform);
            floor.layer = LayerMask.NameToLayer("TrainFloor"); // Ensures camera tracking and car transition works

            roof = Instantiate(roofPrefab, Vector3.zero, Quaternion.identity);
            roof.name = "RailCarRoof";
            roof.transform.localPosition = new Vector3(0, railCarHeight + (floorThickness / 2), 0);
            roof.transform.SetParent(trainCar.transform);

            wall = Instantiate(backWallPrefab, Vector3.zero, Quaternion.identity);
            wall.name = "RailCarWall";
            wall.transform.localPosition = new Vector3(-(railCarLength / 2) - (wallThickness / 2), 0, -(railCarDepth / 2) + (wallThickness / 2));
            wall.transform.SetParent(trainCar.transform);
            // -12.1, 0,  -5

            exteriorWall = Instantiate(frontWallPrefab, Vector3.zero, Quaternion.identity);
            exteriorWall.name = "RailCarExterior";
            exteriorWall.transform.localPosition = new Vector3(0, railCarHeight / 2, (railCarDepth / 2) + (wallThickness / 2));
            exteriorWall.transform.SetParent(trainCar.transform);

            leftWall = Instantiate(leftWallPrefab, Vector3.zero, Quaternion.identity);
            leftWall.name = "RailCarLeft";
            leftWall.transform.localPosition = new Vector3(-(railCarLength / 2) - (wallThickness / 2), railCarHeight / 2, 0);
            leftWall.transform.SetParent(trainCar.transform);
            leftWall.transform.Rotate(0, 180, 0);

            rightWall = Instantiate(leftWallPrefab, Vector3.zero, Quaternion.identity);
            rightWall.name = "RailCarRight";
            rightWall.transform.localPosition = new Vector3((railCarLength / 2) + (wallThickness / 2), railCarHeight / 2, 0);
            rightWall.transform.SetParent(trainCar.transform);



            if (floorMaterial != null)
            {
                floor.GetComponent<Renderer>().material = floorMaterial;
            }

        }
        else 
        {
            floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.localScale = new Vector3(railCarLength, floorThickness, railCarDepth); // x, y, z
            floor.name = "RailCarFloor";
            floor.transform.SetParent(trainCar.transform);
            floor.layer = LayerMask.NameToLayer("TrainFloor"); // Ensures camera tracking and car transition works

            roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.transform.localScale = new Vector3(railCarLength, floorThickness, (railCarDepth + (wallThickness * 2))); // x, y, z
            roof.name = "RailCarRoof";
            roof.transform.SetParent(trainCar.transform);

            // will need to replace with smarter code to include window segmented prefabs.
            wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.localScale = new Vector3(railCarLength, railCarHeight, wallThickness); // x, y, z
            wall.name = "RailCarWall";
            wall.transform.SetParent(trainCar.transform);

            exteriorWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            exteriorWall.transform.localScale = new Vector3(railCarLength, railCarHeight, wallThickness); // x, y, z
            exteriorWall.name = "RailCarExterior";
            exteriorWall.transform.SetParent(trainCar.transform);

            floor.transform.position = new Vector3(railCarLength / 2, floorThickness / 2, railCarDepth / 2); // Centered at origin.
            if (floorMaterial != null)
            {
                floor.GetComponent<Renderer>().material = floorMaterial;
            }

            roof.transform.position = new Vector3(railCarLength / 2, (railCarHeight + (floorThickness / 2)), railCarDepth / 2); // Centered at top of train
            if (roofMaterial != null)
            {
                roof.GetComponent<Renderer>().material = roofMaterial;
            }

            wall.transform.position = new Vector3(railCarLength / 2, railCarHeight / 2, (0 - (wallThickness / 2))); // Centered at back edge of floor.
            if (wallMaterial != null)
            {
                wall.GetComponent<Renderer>().material = wallMaterial;
            }

            exteriorWall.transform.position = new Vector3(railCarLength / 2, railCarHeight / 2, (railCarDepth + (wallThickness / 2))); // Centered at front of train
            if (exteriorMaterial != null)
            {
                exteriorWall.GetComponent<Renderer>().material = exteriorMaterial;
            }

        }

    }

    // Add componenets used in train car managment such as CarVisibility and CarCharacters
    public void AttachTrainScripts()
    {
        // Add CarCharacters component (now simplified, mainly for tracking)
        trainCar.AddComponent<CarCharacters>();
        carCharacterComp = trainCar.GetComponent<CarCharacters>();
        // Removed assignment to carCharacterComp.carFloor and carCharacterComp.npcPrefab as they no longer exist

        // Add CarVisibility component
        trainCar.AddComponent<CarVisibility>();
        carVisibilityComp = trainCar.GetComponent<CarVisibility>();
        carVisibilityComp.carFront = exteriorWall.GetComponent<MeshRenderer>();
        carVisibilityComp.carTop = roof;
        carVisibilityComp.CarSelected();

        // Add navmensh, collect objects within it, bake navmesh
        floor.AddComponent<NavMeshSurface>();
        NavMeshSurface navMeshSurface = floor.GetComponent<NavMeshSurface>();
        navMeshSurface.collectObjects = CollectObjects.Children;
        navMeshSurface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
        navMeshSurface.layerMask = LayerMask.GetMask("Default"); // Adjust as needed
        navMeshSurface.BuildNavMesh();
    }

    public void GenerateAnchorGrid()
    {
        // Clear current anchorPoints reference. Start with blank list.
        anchorPoints = new List<GameObject>();

        // Grid origin = floor top left (pos x, neg z)
        Vector3 floorPosition = floor.transform.position;
        float startX = floorPosition.x + (railCarLength/2) - (cellSize/2);
        float startZ = floorPosition.z - (railCarDepth/2) + (cellSize/2);
        float y = floorPosition.y + (floorThickness/2); // Anchors (should be)above floor, rn halfway in floor...

        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < gridColumns; col++)
            {
                // Spawn, name, and parent anchors.
                float x = startX - col * cellSize;
                float z = startZ + row * cellSize;
                Vector3 anchorPosition = new Vector3(x, y, z);

                GameObject anchor = new GameObject($"Anchor ({row}, {col})");
                anchor.transform.position = anchorPosition;
                anchor.transform.parent = floor.transform;

                // Add visual to the empty anchors. TODO: Make toggle work in update.
                // Also need toggle to spawn if not existant...
                if (anchorPrefab != null && anchorsVissible == true)
                {
                    Instantiate(anchorPrefab, anchorPosition, Quaternion.identity, anchor.transform);
                }

                anchorPoints.Add(anchor); // Add to list for use in object placement.
            }
        }
        spawnSuccessful = true;
    }

    public void SpawnObjects() 
    {
        // Pick random, unused anchors
        List<GameObject> shuffledAnchors = new List<GameObject>(anchorPoints);
        for (int i = 0; i < shuffledAnchors.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffledAnchors.Count);
            GameObject temp = shuffledAnchors[i];
            shuffledAnchors[i] = shuffledAnchors[randomIndex];
            shuffledAnchors[randomIndex] = temp;
        }

        // Spawn objects on random anchor points
        for (int i = 0; i < numbToSpawn; i++)
        {
            if (i >= shuffledAnchors.Count) break; // Prevent overspawning

            GameObject randomObject = objectsToSpawn[Random.Range(0, objectsToSpawn.Count)];
            GameObject anchor = shuffledAnchors[i];
            Instantiate(randomObject, anchor.transform.position, Quaternion.identity, anchor.transform);
        }
    }
}
