using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentSystemsManager : MonoBehaviour
{
    public static PersistentSystemsManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton bullshit 
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Set up hierarchy
        transform.name = "Persistent Systems";

        // Create CoreSystems (if it doesn't exist)
        Transform coreSystems = transform.Find("CoreSystems");
        if (coreSystems == null)
        {
            GameObject coreSystemsObj = new GameObject("CoreSystems");
            coreSystemsObj.transform.SetParent(transform);
            coreSystemsObj.AddComponent<CoreSystemsManager>();
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Proper setup when new scenes load
        var coreSystems = transform.Find("CoreSystems")?.GetComponent<CoreSystemsManager>();
        if (coreSystems != null)
        {
            coreSystems.CleanupDuplicateSystems();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
