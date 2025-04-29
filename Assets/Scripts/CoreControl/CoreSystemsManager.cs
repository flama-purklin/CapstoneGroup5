// Simplified CoreSystemsManager.cs for unified scene approach
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CoreSystemsManager : MonoBehaviour
{
    private static CoreSystemsManager instance;
    private EventSystem managedEventSystem;
    private AudioListener managedAudioListener;
    
    // Added to track all listeners in the scene
    private Dictionary<GameObject, AudioListener> knownAudioListeners = new Dictionary<GameObject, AudioListener>();
    
    // Static access to the singleton
    public static CoreSystemsManager Instance => instance;

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

        // Instead of destroying audio listeners, track them but disable duplicates
        var audioListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        
        // Clear the dictionary first to prevent stale references
        knownAudioListeners.Clear();
        
        foreach (var listener in audioListeners)
        {
            // Skip null references
            if (listener == null) continue;
            
            // Store the listener in our dictionary
            if (!knownAudioListeners.ContainsKey(listener.gameObject))
            {
                knownAudioListeners.Add(listener.gameObject, listener);
            }
            
            // If it's not the main listener, disable it
            if (listener != managedAudioListener)
            {
                listener.enabled = false;
                Debug.Log($"[CoreSystemsManager] Disabled audio listener on {listener.gameObject.name}");
            }
            else
            {
                listener.enabled = true;
            }
        }
    }
    
    // New method to toggle listeners for specific cameras
    public void ToggleAudioListenerForCamera(string cameraName, bool activate)
    {
        // First, disable all audio listeners
        foreach (var entry in knownAudioListeners)
        {
            if (entry.Value != null)
            {
                entry.Value.enabled = false;
            }
        }
        
        // Then activate the requested one
        foreach (var entry in knownAudioListeners)
        {
            if (entry.Key != null && entry.Key.name.Contains(cameraName))
            {
                if (entry.Value != null)
                {
                    entry.Value.enabled = activate;
                    Debug.Log($"[CoreSystemsManager] {(activate ? "Enabled" : "Disabled")} audio listener on {entry.Key.name}");
                }
                break;
            }
        }
    }
    
    // Method specifically for toggling between main camera and mystery camera
    public void ToggleMysteryMapMode(bool isMysteryCameraActive)
    {
        if (isMysteryCameraActive)
        {
            ToggleAudioListenerForCamera("MysteryCam", true);
        }
        else
        {
            ToggleAudioListenerForCamera("Main Camera", true);
        }
    }
}
