using UnityEngine;

/// <summary>
/// Component that identifies a train car with a unique ID and type.
/// </summary>
public class CarIdentifier : MonoBehaviour
{
    [Tooltip("Unique identifier for this car")]
    public string CarId;
    
    [Tooltip("Type of car (e.g., 'dining', 'passenger')")]
    public string CarType;
    
    [Tooltip("Class of car (e.g., 'first_class', 'economy')")]
    public string CarClass;
}

/// <summary>
/// Component that identifies a location within a car.
/// </summary>
public class LocationIdentifier : MonoBehaviour
{
    [Tooltip("ID of this location within its parent")]
    public string LocationId;
    
    [Tooltip("Type of location (e.g., 'table', 'seat', 'bar')")]
    public string LocationType;
    
    private void OnDrawGizmos()
    {
        // Draw a small sphere to visualize this location in the editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.1f);
    }
}

/// <summary>
/// Component that marks a valid spawn point for entities.
/// </summary>
public class SpawnPoint : MonoBehaviour
{
    [Tooltip("Type of entities that can spawn here (e.g., 'character_spawn', 'evidence_spawn')")]
    public string PointType = "character_spawn";
    
    [Tooltip("Whether this spawn point is available for use")]
    public bool IsAvailable = true;
    
    private void OnDrawGizmos()
    {
        // Draw a small sphere to visualize this spawn point in the editor
        Gizmos.color = IsAvailable ? Color.green : Color.red;
        Gizmos.DrawSphere(transform.position, 0.15f);
    }
}