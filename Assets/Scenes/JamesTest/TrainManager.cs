using System.Collections.Generic;
using UnityEngine;

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

    public List<GameObject> carPrefabs; // List of spawnable cars, 
    public Transform spawnPoint;
    public float carSpacing = 25f;

    public List<TrainCar> trainCarList = new List<TrainCar>();
    public GameObject playerPrefab;
    public GameObject cameraPrefab;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (spawnPoint == null) { spawnPoint = this.transform; }

        SpawnCars();
    }

    // Method to spawn cars, currently simple with just spawning prefabs. Hook up to spawners later
    public void SpawnCars()
    {
        Vector3 tempPosition = spawnPoint.position; // Use the stored position

        // Spawn and get info from cars
        foreach (GameObject prefab in carPrefabs)
        {
            GameObject carInstance = Instantiate(prefab, tempPosition, Quaternion.Euler(0, 180, 0)); //Included 180 rotation
            carInstance.transform.SetParent(transform.Find("Rail Cars"));
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
