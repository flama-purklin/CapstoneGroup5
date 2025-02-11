using System.Collections.Generic;
using UnityEngine;

public class CarCharacters: MonoBehaviour
{
    bool visited = false;
    [SerializeField] GameObject npcPrefab;

    List<GameObject> carCharacters;

    [SerializeField] MeshRenderer carFloor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GetComponent<CarVisibility>().selected)
            InitializeCharacters();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Called on first visit to a car - might change if we want NPCs to move between cars
    public void InitializeCharacters()
    {
        if (!visited)
        {
            visited = true;
            carCharacters = new List<GameObject>();

            int characterCount = Random.Range(1, 3);

            for (int i = 0; i < characterCount; i++)
            {
                //calculate random location for them to spawn into
                Vector3 centerPos = carFloor.transform.position;

                float xOffset = Random.Range(-carFloor.bounds.size.x / 3, carFloor.bounds.size.x / 3);
                float zOffset = Random.Range(-carFloor.bounds.size.z / 3, carFloor.bounds.size.z / 3);
                Vector3 spawnPos = centerPos + new Vector3(xOffset, 0f, zOffset);

                GameObject newChar = Instantiate(npcPrefab, spawnPos, Quaternion.identity);
                carCharacters.Add(newChar);
            }

            Debug.Log(characterCount + " characters generated");
        }
    }
}
