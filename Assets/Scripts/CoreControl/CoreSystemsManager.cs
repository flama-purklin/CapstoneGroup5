// New CoreSystemsManager.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class CoreSystemsManager : MonoBehaviour
{
    private static CoreSystemsManager instance;
    private EventSystem managedEventSystem;
    private AudioListener managedAudioListener;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Create managed systems
            SetupEventSystem();
            SetupAudioListener();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupEventSystem()
    {
        if (managedEventSystem == null)
        {
            managedEventSystem = gameObject.AddComponent<EventSystem>();
            gameObject.AddComponent<StandaloneInputModule>();
        }
    }

    private void SetupAudioListener()
    {
        if (managedAudioListener == null)
        {
            managedAudioListener = gameObject.AddComponent<AudioListener>();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CleanupDuplicateSystems();
    }

    public void CleanupDuplicateSystems()
    {
        // Clean up EventSystems
        var eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        foreach (var eventSystem in eventSystems)
        {
            if (eventSystem != managedEventSystem)
            {
                Debug.Log($"Removing duplicate EventSystem in {eventSystem.gameObject.scene.name}");
                Destroy(eventSystem.gameObject);
            }
        }

        // Clean up AudioListeners
        var audioListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        foreach (var listener in audioListeners)
        {
            if (listener != managedAudioListener)
            {
                Debug.Log($"Removing duplicate AudioListener in {listener.gameObject.scene.name}");
                Destroy(listener);
            }
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}