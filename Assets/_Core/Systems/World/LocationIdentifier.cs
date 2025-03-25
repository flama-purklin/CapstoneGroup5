using UnityEngine;

/// <summary>
/// Component that identifies a location within a train car.
/// </summary>
public class LocationIdentifier : MonoBehaviour
{
    /// <summary>
    /// Identifier for this location within its parent car.
    /// Full location ID is constructed as "carId.locationId".
    /// </summary>
    public string LocationId;
    
    /// <summary>
    /// Type of location (e.g., "seat", "table", "walkway").
    /// </summary>
    public string LocationType;
    
    /// <summary>
    /// Whether this location is currently occupied.
    /// </summary>
    public bool IsOccupied { get; set; }
    
    /// <summary>
    /// Whether this location has been searched by the player.
    /// </summary>
    public bool IsSearched { get; private set; }
    
    private void Awake()
    {
        IsOccupied = false;
        IsSearched = false;
    }
    
    /// <summary>
    /// Marks this location as searched by the player.
    /// </summary>
    public void MarkAsSearched()
    {
        if (!IsSearched)
        {
            IsSearched = true;
            Debug.Log($"Location {LocationId} marked as searched");
        }
    }
    
    private void OnValidate()
    {
        // Auto-generate a default ID if empty
        if (string.IsNullOrEmpty(LocationId))
        {
            LocationId = $"location_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
        }
        
        // Auto-generate a default type if empty
        if (string.IsNullOrEmpty(LocationType))
        {
            LocationType = "generic";
        }
    }
    
    private void OnDrawGizmos()
    {
        // Draw a small sphere at the location
        Gizmos.color = IsOccupied ? Color.red : Color.green;
        Gizmos.DrawSphere(transform.position, 0.1f);
    }
}
