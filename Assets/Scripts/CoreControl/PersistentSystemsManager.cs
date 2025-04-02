using UnityEngine;

/// <summary>
/// DEPRECATED: This manager is no longer needed in the unified scene approach.
/// It previously handled persistence between scenes, but now we use a single scene.
/// 
/// This class is kept for backwards compatibility only and should not be used.
/// Use InitializationManager instead for any setup that needs to be done.
/// </summary>
[System.Obsolete("PersistentSystemsManager is deprecated and will be removed. Use InitializationManager instead.")]
public class PersistentSystemsManager : MonoBehaviour
{
    public static PersistentSystemsManager Instance { get; private set; }

    private void Awake()
    {
        // Mark as obsolete in the logs
        Debug.LogWarning("PersistentSystemsManager is deprecated in the unified scene approach. Use InitializationManager instead.");
        
        // Simplified singleton approach
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        // Set up hierarchy with deprecated marker
        transform.name = "Persistent Systems (DEPRECATED)";

        // Create CoreSystems (if it doesn't exist)
        // This is kept for backward compatibility only
        Transform coreSystems = transform.Find("CoreSystems");
        if (coreSystems == null)
        {
            GameObject coreSystemsObj = new GameObject("CoreSystems");
            coreSystemsObj.transform.SetParent(transform);
            coreSystemsObj.AddComponent<CoreSystemsManager>();
        }
    }
}
