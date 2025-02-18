using LLMUnity;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Threading.Tasks;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private LLM llm;
    [SerializeField] private NPCManager npcManager;
    [SerializeField] private CharacterManager characterManager;

    private void Start()
    {
  
        GameObject persistentSystems = GameObject.Find("Persistent Systems");
        if (!persistentSystems)
        {
            persistentSystems = new GameObject("Persistent Systems");
            DontDestroyOnLoad(persistentSystems);
        }

        if (!llm) llm = FindFirstObjectByType<LLM>();
        if (!npcManager) npcManager = FindFirstObjectByType<NPCManager>();
        if (!characterManager) characterManager = FindFirstObjectByType<CharacterManager>();

        if (llm) llm.transform.SetParent(persistentSystems.transform);
        if (npcManager) npcManager.transform.SetParent(persistentSystems.transform);
        if (characterManager) characterManager.transform.SetParent(persistentSystems.transform);

        InitializeGame();
    }

    private async void InitializeGame()
    {
        // Wait for LLM to start
        while (!llm.started)
        {
            await Task.Yield();
        }


        await npcManager.Initialize();
        SceneManager.LoadScene("SystemsTest");
    }
}