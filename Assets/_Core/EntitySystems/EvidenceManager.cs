using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages evidence objects in the game world.
/// </summary>
public class EvidenceManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject defaultEvidencePrefab;
    [SerializeField] private List<EvidencePrefabMapping> specialEvidencePrefabs = new List<EvidencePrefabMapping>();
    
    [Header("Settings")]
    [SerializeField] private Transform evidenceContainer;
    [SerializeField] private bool debugMode = false;
    
    // Runtime data
    private Dictionary<string, GameObject> activeEvidence = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> prefabMappings = new Dictionary<string, GameObject>();
    
    private void Awake()
    {
        // Create evidence container if not set
        if (evidenceContainer == null)
        {
            GameObject containerObj = new GameObject("Evidence");
            containerObj.transform.SetParent(transform);
            evidenceContainer = containerObj.transform;
            LogDebug("Created evidence container");
        }
        
        // Initialize prefab mappings
        foreach (var mapping in specialEvidencePrefabs)
        {
            if (!string.IsNullOrEmpty(mapping.EvidenceType) && mapping.Prefab != null)
            {
                prefabMappings[mapping.EvidenceType.ToLower()] = mapping.Prefab;
                LogDebug($"Registered special evidence prefab for type: {mapping.EvidenceType}");
            }
        }
    }
    
    /// <summary>
    /// Spawns an evidence item at the specified location.
    /// </summary>
    public GameObject SpawnEvidence(string evidenceId, MysteryNode nodeData, Vector3 position, Transform parent = null)
    {
        if (string.IsNullOrEmpty(evidenceId) || nodeData == null)
        {
            Debug.LogError("Cannot spawn evidence with null ID or data");
            return null;
        }
        
        // Check if this evidence is already spawned
        if (activeEvidence.TryGetValue(evidenceId, out GameObject existingEvidence) && existingEvidence != null)
        {
            LogDebug($"Evidence {evidenceId} is already spawned. Moving to new position.");
            existingEvidence.transform.position = position;
            
            if (parent != null && existingEvidence.transform.parent != parent)
            {
                existingEvidence.transform.SetParent(parent);
            }
            
            return existingEvidence;
        }
        
        try
        {
            // Determine the right prefab to use
            GameObject prefabToUse = GetEvidencePrefab(nodeData);
            
            // Determine the parent transform
            Transform spawnParent = parent != null ? parent : evidenceContainer;
            
            // Create evidence instance
            GameObject evidenceInstance = Instantiate(prefabToUse, position, Quaternion.identity, spawnParent);
            evidenceInstance.name = $"Evidence_{evidenceId}";
            
            // Add an evidence component to store data
            EvidenceComponent evidenceComponent = evidenceInstance.AddComponent<EvidenceComponent>();
            evidenceComponent.EvidenceId = evidenceId;
            evidenceComponent.NodeData = nodeData;
            
            // Add a collider if it doesn't have one
            if (evidenceInstance.GetComponent<Collider>() == null)
            {
                BoxCollider collider = evidenceInstance.AddComponent<BoxCollider>();
                collider.size = new Vector3(0.5f, 0.25f, 0.5f);
                collider.center = new Vector3(0, 0.125f, 0);
                collider.isTrigger = true;
            }
            
            // Configure appearance based on evidence type
            ConfigureEvidenceAppearance(evidenceInstance, nodeData);
            
            // Store in active evidence dictionary
            activeEvidence[evidenceId] = evidenceInstance;
            LogDebug($"Successfully spawned evidence {evidenceId} at position {position}");
            
            return evidenceInstance;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error spawning evidence {evidenceId}: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Gets the appropriate prefab for an evidence type.
    /// </summary>
    private GameObject GetEvidencePrefab(MysteryNode nodeData)
    {
        string evidenceType = "default";
        
        // Try to determine evidence type from properties
        if (nodeData.Properties != null && nodeData.Properties.TryGetValue("evidence_type", out string typeValue))
        {
            evidenceType = typeValue.ToLower();
        }
        
        // Try to find a matching prefab
        if (prefabMappings.TryGetValue(evidenceType, out GameObject prefab) && prefab != null)
        {
            return prefab;
        }
        
        // Fall back to default
        return defaultEvidencePrefab;
    }
    
    /// <summary>
    /// Configures the appearance of an evidence object based on its data.
    /// </summary>
    private void ConfigureEvidenceAppearance(GameObject evidenceInstance, MysteryNode nodeData)
    {
        // Set the main color if we have a renderer
        Renderer renderer = evidenceInstance.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Clone the material to avoid affecting other evidence instances
            renderer.material = new Material(renderer.material);
            
            // Try to get color from properties
            if (nodeData.Properties != null && nodeData.Properties.TryGetValue("color", out string colorStr))
            {
                if (ColorUtility.TryParseHtmlString(colorStr, out Color color))
                {
                    renderer.material.color = color;
                }
            }
        }
        
        // You could add more appearance configuration here (scale, rotation, etc.)
    }
    
    /// <summary>
    /// Gets a specific evidence instance by ID.
    /// </summary>
    public GameObject GetEvidenceById(string evidenceId)
    {
        if (activeEvidence.TryGetValue(evidenceId, out GameObject evidence))
        {
            return evidence;
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets all active evidence instances.
    /// </summary>
    public List<GameObject> GetAllEvidence()
    {
        return new List<GameObject>(activeEvidence.Values);
    }
    
    /// <summary>
    /// Clears all active evidence.
    /// </summary>
    public void ClearAllEvidence()
    {
        foreach (var evidence in activeEvidence.Values)
        {
            if (evidence != null)
            {
                Destroy(evidence);
            }
        }
        
        activeEvidence.Clear();
        LogDebug("Cleared all active evidence");
    }
    
    private void LogDebug(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[EvidenceManager] {message}");
        }
    }
}

/// <summary>
/// Mapping between evidence types and prefabs.
/// </summary>
[System.Serializable]
public class EvidencePrefabMapping
{
    public string EvidenceType;
    public GameObject Prefab;
}

/// <summary>
/// Component that stores evidence data.
/// </summary>
public class EvidenceComponent : MonoBehaviour
{
    public string EvidenceId;
    public MysteryNode NodeData;
    
    public string GetEvidenceTitle()
    {
        if (NodeData == null)
        {
            return "Unknown Evidence";
        }
        
        if (NodeData.Properties != null && NodeData.Properties.TryGetValue("title", out string title))
        {
            return title;
        }
        
        return $"Evidence {EvidenceId}";
    }
    
    public string GetEvidenceDescription()
    {
        if (NodeData == null)
        {
            return "No information available.";
        }
        
        if (NodeData.Properties != null && NodeData.Properties.TryGetValue("description", out string description))
        {
            return description;
        }
        
        return "This is a piece of evidence related to the mystery.";
    }
}