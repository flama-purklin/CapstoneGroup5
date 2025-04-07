using System.Collections.Generic;
using UnityEngine;
using System.IO;
using static UnityEditor.Experimental.GraphView.GraphView;
using UnityEngine.UI;
using TMPro;

public class TrainCarSaver : MonoBehaviour
{
    public string saveFileName = "Custom.json"; // Default filename
    public string savePath = "Assets/TrainAssets/Jsons/"; //Default path.
    public int maxRows = 5; // Settable max rows
    public int maxColumns = 15; // Settable max columns
    public TMP_InputField fileNameField, pathField;

    void Start()
    {
        //SaveTrainCarLayout();
        fileNameField.onValueChanged.AddListener(setFileName);
        pathField.onValueChanged.AddListener(setPath);
    }

    public void setRows(int x) { maxRows = x; }
    public void setColumns(int y) { maxColumns = y; }
    public void setFileName(string name) { saveFileName = name; }
    public void setPath(string path) {  savePath = path; }

    public void SaveTrainCarLayout()
    {
        // Find the TrainCar object
        GameObject trainCar = GameObject.Find("Train Car");
        if (trainCar == null)
        {
            Debug.LogError("Train Car not found!");
            return;
        }

        // Find the RailCarFloor child
        Transform railCarFloor = trainCar.transform.Find("RailCarFloor");
        if (railCarFloor == null)
        {
            Debug.LogError("RailCarFloor not found in Train Car!");
            return;
        }

        TrainCarLayout trainCarLayout = new TrainCarLayout();
        if (trainCarLayout.train_car == null)
        {
            trainCarLayout.train_car = new List<TrainCarObject>(); // Ensure it's initialized
        }

        HashSet<string> occupiedPositions = new HashSet<string>();

        // Iterate over each Anchor in floor
        foreach (Transform anchor in railCarFloor)
        {
            if (!anchor.name.StartsWith("Anchor")) continue;

            // Extract coordinates from anchor
            string[] parts = anchor.name.Replace("Anchor (", "").Replace(")", "").Split(',');
            if (parts.Length != 2) continue;
            int x = int.Parse(parts[0].Trim());
            int y = int.Parse(parts[1].Trim());

            // Find placed objects
            bool objectPlaced = false;
            foreach (Transform child in anchor)
            {
                if (child.name.Contains("AnchorPrefab")) continue; // Ignore anchor prefab

                // Extract type and rotation
                string[] objParts = child.name.Split(' ');
                Debug.Log($"Name Parts: '{objParts[0]}, {objParts[1]}, {objParts[2]}'");
                if (objParts.Length < 3) continue; // Ensure valid

                string type = objParts[0];
                int rotation = int.Parse(objParts[2]);

                // Add to JSON structure
                trainCarLayout.train_car.Add(new TrainCarObject
                {
                    position = new int[] { x, y },
                    type = type.ToLower(),
                    rotation = rotation
                });

                occupiedPositions.Add($"{x},{y}");
                objectPlaced = true;
            }

            // If no object was placed, add "walkway" as default
            if (!objectPlaced)
            {
                trainCarLayout.train_car.Add(new TrainCarObject
                {
                    position = new int[] { x, y },
                    type = "walkway",
                    rotation = 0
                });

                occupiedPositions.Add($"{x},{y}");
            }
        }

        // Fill missing walkways
        FillMissingWalkways(trainCarLayout, occupiedPositions);

        // Convert to JSON
        string json = JsonUtility.ToJson(trainCarLayout, true); // False compacts printing... but too much

        // Save JSON to file
        //string filePath = Path.Combine(Application.persistentDataPath, saveFileName);
        if (!saveFileName.EndsWith(".json", System.StringComparison.OrdinalIgnoreCase))
        {
            saveFileName += ".json"; // Add extension if not specified
        }
        string filePath = Path.Combine(savePath, saveFileName);
        File.WriteAllText(filePath, json);

        Debug.Log($"Train car layout saved to: {filePath}");
    }

    private void FillMissingWalkways(TrainCarLayout layout, HashSet<string> occupiedPositions)
    {
        for (int x = 0; x < maxRows; x++)
        {
            for (int y = 0; y < maxColumns; y++)
            {
                if (!occupiedPositions.Contains($"{x},{y}"))
                {
                    layout.train_car.Add(new TrainCarObject
                    {
                        position = new int[] { x, y },
                        type = "walkway",
                        rotation = 0
                    });
                }
            }
        }
    }
}
