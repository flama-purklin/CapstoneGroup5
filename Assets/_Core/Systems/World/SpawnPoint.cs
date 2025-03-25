using UnityEngine;

/// <summary>
/// Component that marks a valid spawn point for characters or evidence.
/// </summary>
public class SpawnPoint : MonoBehaviour
{
    /// <summary>
    /// Type of entity that can spawn here (e.g., "character_spawn" or "evidence_spawn").
    /// </summary>
    public string PointType = "character_spawn";
    
    /// <summary>
    /// Whether this spawn point is currently available.
    /// </summary>
    public bool IsAvailable = true;
    
    private void OnDrawGizmos()
    {
        // Draw a small marker at the spawn point
        Gizmos.color = IsAvailable ? Color.green : Color.red;
        Gizmos.DrawSphere(transform.position, 0.25f);
        
        // Draw a small icon to indicate the type
        if (PointType.Contains("character"))
        {
            Gizmos.color = Color.blue;
        }
        else if (PointType.Contains("evidence"))
        {
            Gizmos.color = Color.yellow;
        }
        
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.3f, new Vector3(0.1f, 0.1f, 0.1f));
    }
}
