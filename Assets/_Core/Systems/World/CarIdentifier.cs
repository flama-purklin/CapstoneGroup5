using UnityEngine;

/// <summary>
/// Component that identifies a train car for lookup by the WorldCoordinator.
/// </summary>
public class CarIdentifier : MonoBehaviour
{
    /// <summary>
    /// Unique identifier for this car. Used for lookups.
    /// </summary>
    public string CarId;
    
    /// <summary>
    /// Type of car (e.g., "dining", "passenger", "engine").
    /// </summary>
    public string CarType;
    
    /// <summary>
    /// Class of car (e.g., "first_class", "second_class").
    /// </summary>
    public string CarClass;
    
    /// <summary>
    /// Whether this car is currently visited by the player.
    /// </summary>
    public bool IsVisited { get; private set; }
    
    private void Awake()
    {
        IsVisited = false;
    }
    
    /// <summary>
    /// Marks this car as visited by the player.
    /// </summary>
    public void MarkAsVisited()
    {
        if (!IsVisited)
        {
            IsVisited = true;
            Debug.Log($"Car {CarId} marked as visited");
        }
    }
    
    private void OnValidate()
    {
        // Auto-generate a default ID if empty
        if (string.IsNullOrEmpty(CarId))
        {
            CarId = $"car_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
        }
    }
}
