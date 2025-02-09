using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RailCarRandomizer : MonoBehaviour
{
    [SerializeField] public float railCarLength = 10f;
    [SerializeField] public float railCarDepth = 5f;
    [SerializeField] public float railCarHeight = 5.5f;
    [SerializeField] public float wallThickness = 0.5f;
    [SerializeField] public float floorThickness = 0.5f;
    [SerializeField] public Material floorMaterial = null;
    [SerializeField] public Material wallMaterial = null;
    public GameObject trainCar;
    public GameObject floor;
    public GameObject wall;

    [SerializeField] public int gridRows = 5;
    [SerializeField] public int gridColumns = 10;
    [SerializeField] public float cellSize = 1f; // TODO: cell size should be auto calc'd based on floor size.
    [SerializeField] public GameObject anchorPrefab = null;
    [SerializeField] public bool anchorsVissible = false;

    [SerializeField] public int numbToSpawn = 10; // How many objects are spawned
    [SerializeField] public List<GameObject> objectsToSpawn; // List to randomize

    public List<GameObject> anchorPoints = new List<GameObject>(); // Stores all anchor points


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateTrainShell();
        GenerateAnchorGrid();
        //SpawnObjects();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateTrainShell()
    {
        trainCar = new GameObject("Train Car");

        floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.transform.localScale = new Vector3(railCarLength, floorThickness, railCarDepth); // x, y, z
        floor.name = "RailCarFloor";
        floor.transform.SetParent(trainCar.transform);

        wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.localScale = new Vector3(railCarLength, railCarHeight, wallThickness); // x, y, z
        wall.name = "RailCarWall";
        wall.transform.SetParent(trainCar.transform);

        floor.transform.position = new Vector3(railCarLength/2, floorThickness/2, railCarDepth/2); // Centered at origin.
        if (floorMaterial != null)
        {
            floor.GetComponent<Renderer>().material = floorMaterial;
        }

        wall.transform.position = new Vector3(railCarLength/2, railCarHeight/2, 0); // Centered at back edge of floor.
        if (wallMaterial != null)
        {
            wall.GetComponent<Renderer>().material = wallMaterial;
        }
    }

    public void GenerateAnchorGrid()
    {
        // Grid origin (should)= floor center. Right now equals top right...
        Vector3 floorPosition = floor.transform.position;
        float startX = floorPosition.x - (railCarLength/2) + (cellSize/2);
        float startZ = floorPosition.z - (railCarDepth/2) + (cellSize/2);
        float y = floorPosition.y + (floorThickness/2); // Anchors (should be)above floor, rn halfway in floor...

        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < gridColumns; col++)
            {
                // Spawn, name, and parent anchors.
                float x = startX + col * cellSize;
                float z = startZ + row * cellSize;
                Vector3 anchorPosition = new Vector3(x, y, z);

                GameObject anchor = new GameObject($"Anchor ({row}, {col})");
                anchor.transform.position = anchorPosition;
                anchor.transform.parent = floor.transform;

                // Add visual to the empty anchors. TODO: Make toggle work in update.
                if (anchorPrefab != null && anchorsVissible == true)
                {
                    Instantiate(anchorPrefab, anchorPosition, Quaternion.identity, anchor.transform);
                }

                anchorPoints.Add(anchor); // Add to list for use in object placement.
            }
        }
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
