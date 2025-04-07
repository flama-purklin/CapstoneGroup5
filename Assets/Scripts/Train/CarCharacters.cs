using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CarCharacters : MonoBehaviour
{
    private bool visited = false;
    [SerializeField] public GameObject npcPrefab;
    [SerializeField] public MeshRenderer carFloor;
    [SerializeField] private int minCharacters = 1;
    [SerializeField] private int maxCharacters = 3;

    private List<GameObject> carCharacters = new List<GameObject>();
    private NPCManager npcManager;

    private static HashSet<string> usedCharacters = new HashSet<string>();

    private void Start()
    {
        npcManager = FindFirstObjectByType<NPCManager>();
        if (!npcManager)
        {
            Debug.LogError("NPCManager not found in scene!");
            return;
        }

        if (GetComponent<CarVisibility>().selected)
        {
            InitializeCharacters();
        }
    }

    public void InitializeCharacters()
    {
        if (visited || !npcManager) return;
        if (npcManager.GetAvailableCharacters().Where(c => !usedCharacters.Contains(c)).ToList().Count() == 0) { return; } //TEMPORARY  
        visited = true;
        StartCoroutine(SpawnCharactersWhenReady());
    }

    private IEnumerator SpawnCharactersWhenReady()
    {
        // wait for NPCManager to be ready
        while (!npcManager.IsInitializationComplete)
        {
            yield return new WaitForSeconds(0.1f);
        }

        string[] allCharacters = npcManager.GetAvailableCharacters();
        if (allCharacters == null || allCharacters.Length == 0)
        {
            Debug.LogError("No characters available to spawn!");
            yield break;
        }

        List<string> availableCharacters = allCharacters
            .Where(c => !usedCharacters.Contains(c))
            .ToList();

        if (availableCharacters.Count == 0)
        {
            Debug.LogWarning("All characters have been used!");
        }

        int maxPossibleCharacters = Mathf.Min(maxCharacters, availableCharacters.Count);
        int characterCount = Random.Range(minCharacters, maxPossibleCharacters + 1);

 
        for (int i = 0; i < characterCount; i++)
        {
            if (availableCharacters.Count == 0) break;

            // randomly select a character from remaining available characters
            int randomIndex = Random.Range(0, availableCharacters.Count);
            string selectedCharacter = availableCharacters[randomIndex];

            availableCharacters.RemoveAt(randomIndex);
            usedCharacters.Add(selectedCharacter);

     
            SpawnCharacter(selectedCharacter);

            yield return null;
        }

        Debug.Log($"{characterCount} characters generated in {gameObject.name}: {string.Join(", ", carCharacters.Select(c => c.name))}");
    }

    private void SpawnCharacter(string characterName)
    {
        Vector3 spawnPos = GetRandomSpawnPosition();
        GameObject npc = npcManager.SpawnNPCInCar(characterName, spawnPos, transform);
        if (npc)
        {
            carCharacters.Add(npc);
            Debug.Log($"Spawned {characterName} in {gameObject.name}");
        }
    }

    public bool IsVisited()
    {
        return visited;
    }

    public Vector3 GetRandomSpawnPosition()
    {
        Vector3 centerPos = carFloor.transform.position;
        float xOffset = Random.Range(-carFloor.bounds.size.x / 3, carFloor.bounds.size.x / 3);
        float zOffset = Random.Range(-carFloor.bounds.size.z / 3, carFloor.bounds.size.z / 3);
        return centerPos + new Vector3(xOffset, 0f, zOffset);
    }

    // method to reset used characters if needed
    public static void ResetUsedCharacters()
    {
        usedCharacters.Clear();
        Debug.Log("Reset used characters list");
    }

    // method to access list of npcs currently in car
    public List<GameObject> GetCurrCharacters()
    {
        return this.carCharacters;
    }
}