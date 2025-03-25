using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages evidence items in the world.
/// Because someone has to keep track of all those bloody knives and suspicious letters.
/// </summary>
public class EvidenceManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject defaultEvidencePrefab;
    [SerializeField] private GameObject documentPrefab;
    [SerializeField] private GameObject weaponPrefab;
    [SerializeField] private GameObject clothingPrefab;
    
    [Header("Settings")]
    [SerializeField] private bool debugMode = false;
    
    private Dictionary<string, GameObject> activeEvidenceItems = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> evidencePrefabsByType;
    
    private void Awake()
    {
        // Initialize evidence prefabs dictionary
        evidencePrefabsByType = new Dictionary<string, GameObject>(System.StringComparer.OrdinalIgnoreCase);
        
        // Add the prefabs we have
        if (defaultEvidencePrefab != null) evidencePrefabsByType["default"] = defaultEvidencePrefab;
        if (documentPrefab != null) evidencePrefabsByType["document"] = documentPrefab;
        if (weaponPrefab != null) evidencePrefabsByType["weapon"] = weaponPrefab;
        if (clothingPrefab != null) evidencePrefabsByType["clothing"] = clothingPrefab;
        
        // If we don't have a default evidence prefab, create a simple one
        if (defaultEvidencePrefab == null)
        {
            LogDebug("No default evidence prefab assigned, creating a basic one");
            defaultEvidencePrefab = CreateBasicEvidencePrefab();
            evidencePrefabsByType["default"] = defaultEvidencePrefab;
        }
    }
    
    /// <summary>
    /// Spawns an evidence item at the specified location.
    /// Making clues materialize out of thin air since 2023.
    /// </summary>
    public GameObject SpawnEvidence(string evidenceId, MysteryNode data, Vector3 position, Transform parent)
    {
        // Determine the evidence type
        string evidenceType = DetermineEvidenceType(data);
        
        // Get the appropriate prefab
        GameObject prefab = GetEvidencePrefabByType(evidenceType);
        
        // Create the evidence instance
        GameObject evidenceInstance = Instantiate(prefab, position, Quaternion.identity, parent);
        evidenceInstance.name = $"Evidence_{evidenceId}";
        
        // Add identifier component
        EvidenceIdentifier identifier = evidenceInstance.AddComponent<EvidenceIdentifier>();
        identifier.EvidenceId = evidenceId;
        identifier.NodeData = data;
        
        // Configure the evidence based on its data
        ConfigureEvidence(evidenceInstance, data);
        
        // Store in active evidence dictionary
        activeEvidenceItems[evidenceId] = evidenceInstance;
        
        LogDebug($"Spawned evidence {evidenceId} of type {evidenceType} at {position}");
        return evidenceInstance;
    }
    
    /// <summary>
    /// Determines the evidence type based on node data.
    /// It's like profiling, but for inanimate objects.
    /// </summary>
    private string DetermineEvidenceType(MysteryNode data)
    {
        // Check for explicit type in the node data
        if (data.Properties != null && data.Properties.ContainsKey("evidence_type"))
        {
            string type = data.Properties["evidence_type"];
            if (evidencePrefabsByType.ContainsKey(type))
            {
                return type;
            }
        }
        
        // Try to determine type from title or description
        string titleAndDesc = (data.Title + " " + data.Description).ToLower();
        
        if (titleAndDesc.Contains("letter") || titleAndDesc.Contains("note") || 
            titleAndDesc.Contains("document") || titleAndDesc.Contains("diary") ||
            titleAndDesc.Contains("book") || titleAndDesc.Contains("page"))
        {
            return "document";
        }
        else if (titleAndDesc.Contains("knife") || titleAndDesc.Contains("gun") || 
                 titleAndDesc.Contains("weapon") || titleAndDesc.Contains("sword") ||
                 titleAndDesc.Contains("blade") || titleAndDesc.Contains("rifle"))
        {
            return "weapon";
        }
        else if (titleAndDesc.Contains("cloth") || titleAndDesc.Contains("shirt") || 
                 titleAndDesc.Contains("dress") || titleAndDesc.Contains("hat") ||
                 titleAndDesc.Contains("glove") || titleAndDesc.Contains("scarf"))
        {
            return "clothing";
        }
        
        // Default case
        return "default";
    }
    
    /// <summary>
    /// Gets the evidence prefab for a specific type.
    /// It's like a vending machine, but for evidence. And without the snacks.
    /// </summary>
    private GameObject GetEvidencePrefabByType(string evidenceType)
    {
        if (evidencePrefabsByType.TryGetValue(evidenceType, out GameObject prefab) && prefab != null)
        {
            return prefab;
        }
        
        LogDebug($"No prefab for evidence type: {evidenceType}, using default");
        return defaultEvidencePrefab;
    }
    
    /// <summary>
    /// Configures an evidence instance based on its data.
    /// Teaching clues how to behave in society.
    /// </summary>
    private void ConfigureEvidence(GameObject evidenceInstance, MysteryNode data)
    {
        // Add interaction components if not already present
        if (!evidenceInstance.GetComponent<Collider>())
        {
            // Add a box collider for interaction
            BoxCollider collider = evidenceInstance.AddComponent<BoxCollider>();
            collider.size = new Vector3(1f, 1f, 1f);
            collider.isTrigger = true;
        }
        
        // Set the evidence name/title
        TextMesh textMesh = evidenceInstance.GetComponentInChildren<TextMesh>();
        if (textMesh != null)
        {
            textMesh.text = data.Title;
        }
        
        // TODO: Add more configuration based on evidence type
    }
    
    /// <summary>
    /// Creates a basic evidence prefab if none is assigned.
    /// MacGyver would be proud.
    /// </summary>
    private GameObject CreateBasicEvidencePrefab()
    {
        GameObject prefab = new GameObject("BasicEvidencePrefab");
        
        // Add a visual representation
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.transform.SetParent(prefab.transform);
        visual.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
        
        // Add a text label
        GameObject label = new GameObject("Label");
        label.transform.SetParent(prefab.transform);
        label.transform.localPosition = new Vector3(0, 0.2f, 0);
        
        TextMesh textMesh = label.AddComponent<TextMesh>();
        textMesh.text = "Evidence";
        textMesh.fontSize = 14;
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.LowerCenter;
        
        return prefab;
    }
    
    /// <summary>
    /// Gets an evidence item by its ID.
    /// Like a very specific lost and found.
    /// </summary>
    public GameObject GetEvidenceById(string evidenceId)
    {
        if (activeEvidenceItems.TryGetValue(evidenceId, out GameObject evidence))
        {
            return evidence;
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets all active evidence items.
    /// For when you need to double-check that you've hidden all the clues.
    /// </summary>
    public Dictionary<string, GameObject> GetAllEvidence()
    {
        return new Dictionary<string, GameObject>(activeEvidenceItems);
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
/// Identifies an evidence item and stores its data.
/// It's like a museum placard, but for crime scenes.
/// </summary>
public class EvidenceIdentifier : MonoBehaviour
{
    [Tooltip("Unique identifier for this evidence")]
    public string EvidenceId;
    
    [Tooltip("Node data for this evidence")]
    public MysteryNode NodeData;
    
    // Store a reference to the original node data
    private void OnValidate()
    {
        // Update the GameObject name when EvidenceId changes
        if (!string.IsNullOrEmpty(EvidenceId))
        {
            gameObject.name = $"Evidence_{EvidenceId}";
        }
    }
}
