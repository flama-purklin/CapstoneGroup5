using UnityEngine;

/// <summary>
/// Identifies a car in the train layout.
/// A digital name tag for train cars, because otherwise they'd all look the same.
/// </summary>
public class CarIdentifier : MonoBehaviour
{
    [Tooltip("Unique identifier for this car")]
    public string CarId;
    
    [Tooltip("Type of car (e.g., 'dining', 'passenger')")]
    public string CarType;
    
    [Tooltip("Class of car (e.g., 'first', 'second')")]
    public string CarClass;
    
    private void OnValidate()
    {
        // Update the GameObject name when CarId or CarType changes in the inspector
        if (!string.IsNullOrEmpty(CarId) && !string.IsNullOrEmpty(CarType))
        {
            gameObject.name = $"Train Car - {CarType} ({CarId})";
        }
    }
}

/// <summary>
/// Identifies a specific location within a car or the world.
/// Because "third seat on the left" is too vague for a computer.
/// </summary>
public class LocationIdentifier : MonoBehaviour
{
    [Tooltip("Unique identifier for this location within its parent")]
    public string LocationId;
    
    [Tooltip("Type of location (e.g., 'seat', 'table')")]
    public string LocationType;
    
    [Tooltip("Description of this location")]
    [TextArea(2, 5)]
    public string Description;
    
    private void OnValidate()
    {
        // Update GameObject name when LocationId changes in the inspector
        if (!string.IsNullOrEmpty(LocationId))
        {
            gameObject.name = LocationId;
        }
    }
    
    private void OnDrawGizmos()
    {
        // Draw a visual indicator for the location in the editor
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        
        // Draw a line to indicate the forward direction
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.5f);
    }
}

/// <summary>
/// Marks a point where entities can be spawned.
/// Like a "You Are Here" marker, but for NPCs and evidence.
/// </summary>
public class SpawnPoint : MonoBehaviour
{
    [Tooltip("Type of entities that can spawn here")]
    public string PointType = "character_spawn"; // or "evidence_spawn"
    
    [Tooltip("Whether this spawn point can be used")]
    public bool IsAvailable = true;
    
    [Tooltip("Priority for selection (higher = more likely)")]
    public int Priority = 1;
    
    private void OnDrawGizmos()
    {
        // Draw different colored gizmos based on type and availability
        if (PointType == "character_spawn")
        {
            Gizmos.color = IsAvailable ? Color.green : Color.red;
        }
        else if (PointType == "evidence_spawn")
        {
            Gizmos.color = IsAvailable ? Color.yellow : new Color(0.5f, 0.5f, 0);
        }
        else
        {
            Gizmos.color = IsAvailable ? Color.white : Color.grey;
        }
        
        Gizmos.DrawSphere(transform.position, 0.25f);
    }
}
