// Simplified CoreSystemsManager.cs for unified scene approach
using UnityEngine;
using UnityEngine.EventSystems;

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

            // Create managed systems
            SetupEventSystem();
            SetupAudioListener();
            
            // Initial cleanup on start
            CleanupDuplicateSystems();
            
            
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

    public void CleanupDuplicateSystems()
    {
        // Clean up EventSystems
        var eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        foreach (var eventSystem in eventSystems)
        {
            if (eventSystem != managedEventSystem && eventSystem != null)
            {
                
                Destroy(eventSystem.gameObject);
            }
        }

        // Clean up AudioListeners
        var audioListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        foreach (var listener in audioListeners)
        {
            if (listener != managedAudioListener && listener != null)
            {
                
                Destroy(listener);
            }
        }
    }
}
