using NUnit.Framework.Internal;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static TrainManager;

public class TrainManager : MonoBehaviour
{
    // Enume for car types
    public enum CarClass
    {
        FirstClass,
        SecondClass,
        BusinessClass,
        Special
    }

    // Enum for car name/theme
    public enum CarType
    {
        Passenger,
        Dining,
        Kitchen,
        Bar,
        Engine,
        Storage
    }

    // Class for centralizing car info. Should add proper access protection later, all pub now
    [System.Serializable]
    public class TrainCar
    {
        public GameObject trainCar;
        public CarClass carClass;
        public CarType type;
        public int carNumber;
        public bool isSelected = false; // Means player is in it.
        public List<GameObject> npcsInCar = new List<GameObject>();
    }

    public List<GameObject> carPrefabs; // List of spawnable cars
    public List<TextAsset> carJsons; // List of json files, will call Randomizer and Loader to make shell and populate.
    public bool spawnWithJsons = false;
    public Transform spawnPoint;
    private Vector3 tempPosition; // For car placment manipulation
    public float carSpacing = 25f;

    public List<TrainCar> trainCarList = new List<TrainCar>();
    public TrainCarLoader loader;
    public GameObject playerPrefab;
    public GameObject cameraPrefab;
    public GameObject playerInstance;
    public GameObject cameraInstance;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (spawnPoint == null) { spawnPoint = this.transform; }

        tempPosition = spawnPoint.position; // Use the stored position

        SpawnCars();
    }

    // Method to spawn cars
    public void SpawnCars()
    {
        tempPosition = spawnPoint.position; // Reset in case need to respawn cars.

        if (spawnWithJsons)
        {
            // Errror checking
            if (loader == null)
            {
                Debug.Log("Spawn with Jsons selected but no refernce to TrainCarLoader script.");
                return;
            }

            // For each Json in list, call loader, then place that object
            foreach (TextAsset jsonFile in carJsons)
            {
                GameObject carInstance = loader.PopulateTrain(jsonFile);
                carInstance.transform.rotation = Quaternion.Euler(0, 180, 0); // Apply rotation as if it were a prefab (used since spawn goes from + to neg x and camera wierd)
                CarVisibility carSelected = carInstance.GetComponent<CarVisibility>(); // Ensures cars spawn closed
                carSelected.CarDeselected();
                PositionAndStoreTrainCar(carInstance);
            }
        }
        else
        {
            foreach (GameObject prefab in carPrefabs)
            {
                GameObject carInstance = Instantiate(prefab, tempPosition, Quaternion.Euler(0, 180, 0)); //Included 180 rotation
                CarVisibility carSelected = carInstance.GetComponent<CarVisibility>(); // Ensures cars spawn closed
                carSelected.CarDeselected();
                PositionAndStoreTrainCar(carInstance);
            }
        }

        // Spawn player if cars exists, else spawn at train manager pos
        if (trainCarList.Count > 0)
        {
            SpawnPlayerAndCamera(trainCarList[0]);
        }
        else { }
    }

    // Abstracted so whether spawning from json or prefab they are added to the train manager properly
    private void PositionAndStoreTrainCar(GameObject carInstance)
    {
        carInstance.transform.SetParent(transform.Find("Rail Cars"));
        carInstance.transform.position = tempPosition; // The part that actually moves the car...

        TrainCar trainCar = new TrainCar
        {
            trainCar = carInstance,
            npcsInCar = carInstance.GetComponent<CarCharacters>().GetCurrCharacters(),
            isSelected = carInstance.GetComponent<CarVisibility>().selected,
            carNumber = trainCarList.Count
        };
        trainCarList.Add(trainCar);

        tempPosition += Vector3.left * carSpacing; // Spacing, left is -x?
    }

    // Method to spawn the player and camera in the first car
    private void SpawnPlayerAndCamera(TrainCar car)
    {
        // Position player and camera relative to car position
        Vector3 playerPosition = car.trainCar.transform.position + new Vector3(0, loader.railCarRandomizer.floorThickness, 0); // Adjust based on the car's layout
        playerInstance = Instantiate(playerPrefab, playerPosition, Quaternion.identity);

        /// *** Cant figure out camera spawning for the life of me!!!
        // Spawn camera with offset, script on player will take care of follow.
        //cameraInstance = Instantiate(cameraPrefab, new Vector3(-698.669983f, -385.386841f, -8.52000046f), Quaternion.Euler(30.0000076f, 0f, 0f));
    }

    // Method to spawn the player and camera at manager location
    public void SpawnPlayerAndCamera()
    {
        // Position player and camera relative to manager
        Vector3 playerPosition = this.transform.position + new Vector3(0, loader.railCarRandomizer.floorThickness, 0);
        playerInstance = Instantiate(playerPrefab, playerPosition, Quaternion.identity);

        // Spawn camera with offset, script on player will take care of follow.
        //cameraInstance = Instantiate(cameraPrefab, new Vector3(-698.669983f, -385.386841f, -8.52000046f), Quaternion.Euler(30.0000076f, 0f, 0f));
    }

    // Update information in train car manager from cars in the train
    public void GetUpdatedInfo()
    {
        foreach (TrainCar car in trainCarList)
        {
            car.npcsInCar = car.trainCar.GetComponent<CarCharacters>().GetCurrCharacters();
            car.isSelected = car.trainCar.GetComponent<CarVisibility>().selected;
        }
    }

}
