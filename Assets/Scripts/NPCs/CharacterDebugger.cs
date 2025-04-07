using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;

/// <summary>
/// Utility script for debugging character-related issues in the scene
/// </summary>
public class CharacterDebugger : MonoBehaviour
{
    [Header("Display Options")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private bool logDetailsOnStart = true;
    [SerializeField] private Color validCharacterColor = Color.green;
    [SerializeField] private Color invalidCharacterColor = Color.red;
    [SerializeField] private float gizmoRadius = 0.5f;
    
    [Header("Status")]
    [SerializeField] private int characterCount;
    [SerializeField] private int validHierarchyCount;
    [SerializeField] private int invalidHierarchyCount;
    [SerializeField] private int navMeshIssueCount;
    
    // Track characters with issues
    private List<Character> charactersWithIssues = new List<Character>();
    
    private void Start()
    {
        if (logDetailsOnStart)
        {
            PerformFullDiagnostics();
        }
    }
    
    public void PerformFullDiagnostics()
    {
        // Clear previous data
        charactersWithIssues.Clear();
        
        // Find all characters
        var allCharacters = FindObjectsByType<Character>(FindObjectsSortMode.None);
        characterCount = allCharacters.Length;
        
        Debug.Log($"=== CHARACTER DIAGNOSTIC REPORT ===");
        Debug.Log($"Found {characterCount} characters in scene");
        
        // Reset counters
        validHierarchyCount = 0;
        invalidHierarchyCount = 0;
        navMeshIssueCount = 0;
        
        // Check each character
        foreach (var character in allCharacters)
        {
            if (character == null) continue;
            
            bool hasIssues = false;
            string characterName = character.CharacterName;
            
            // Validate character hierarchy
            string hierarchyStatus = ValidateCharacterHierarchy(character);
            
            if (hierarchyStatus != "Valid")
            {
                invalidHierarchyCount++;
                hasIssues = true;
                Debug.LogWarning($"Character {characterName} has hierarchy issue: {hierarchyStatus}");
            }
            else
            {
                validHierarchyCount++;
            }
            
            // Check NavMesh placement
            bool onNavMesh = IsCharacterOnNavMesh(character);
            if (!onNavMesh)
            {
                navMeshIssueCount++;
                hasIssues = true;
                Debug.LogWarning($"Character {characterName} has NavMesh placement issue!");
            }
            
            // Add to issues list if needed
            if (hasIssues)
            {
                charactersWithIssues.Add(character);
            }
        }
        
        // Log summary
        Debug.Log($"Diagnostic Summary:");
        Debug.Log($"- Total characters: {characterCount}");
        Debug.Log($"- Valid hierarchy: {validHierarchyCount}");
        Debug.Log($"- Invalid hierarchy: {invalidHierarchyCount}");
        Debug.Log($"- NavMesh issues: {navMeshIssueCount}");
        Debug.Log($"- Total characters with issues: {charactersWithIssues.Count}");
        
        // List all characters with issues
        if (charactersWithIssues.Count > 0)
        {
            Debug.Log($"Characters with issues:");
            foreach (var character in charactersWithIssues)
            {
                if (character == null) continue;
                
                Debug.LogWarning($"  * {character.CharacterName} (in {character.gameObject.name})");
            }
        }
        
        Debug.Log($"=== END OF DIAGNOSTIC REPORT ===");
    }
    
    // Validate character hierarchy
    private string ValidateCharacterHierarchy(Character character)
    {
        if (character == null) return "Null character";
        
        string characterName = character.CharacterName;
        if (string.IsNullOrEmpty(characterName)) return "Missing character name";
        
        // Check parent
        Transform parentTransform = character.transform.parent;
        if (parentTransform == null) return "No parent transform";
        
        // Parent should be named NPC_{characterName}
        string expectedParentName = $"NPC_{characterName}";
        if (parentTransform.name != expectedParentName) 
            return $"Parent name mismatch: expected {expectedParentName}, found {parentTransform.name}";
        
        // Component check
        var agent = character.GetComponentInChildren<NavMeshAgent>();
        if (agent == null) return "Missing NavMeshAgent component";
        
        var animator = character.GetComponentInChildren<Animator>();
        if (animator == null) return "Missing Animator component";
        
        var animManager = character.GetComponentInChildren<NPCAnimManager>();
        if (animManager == null) return "Missing NPCAnimManager component";
        
        // All checks passed
        return "Valid";
    }
    
    // Check if character is on NavMesh
    private bool IsCharacterOnNavMesh(Character character)
    {
        if (character == null) return false;
        
        // Get position
        Vector3 position = character.transform.position;
        
        // Check NavMesh
        NavMeshHit hit;
        float maxDistance = 1.0f;  // How far from NavMesh is acceptable
        if (NavMesh.SamplePosition(position, out hit, maxDistance, NavMesh.AllAreas))
        {
            float distance = Vector3.Distance(position, hit.position);
            return distance < maxDistance;
        }
        
        return false;
    }
    
    // Draw debug visuals
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        // Draw spheres for characters with issues
        foreach (var character in charactersWithIssues)
        {
            if (character == null) continue;
            
            Gizmos.color = invalidCharacterColor;
            Gizmos.DrawWireSphere(character.transform.position, gizmoRadius);
        }
        
        // In editor, also draw valid characters
        if (!Application.isPlaying)
        {
            var allCharacters = FindObjectsByType<Character>(FindObjectsSortMode.None);
            foreach (var character in allCharacters)
            {
                if (character == null || charactersWithIssues.Contains(character)) continue;
                
                Gizmos.color = validCharacterColor;
                Gizmos.DrawWireSphere(character.transform.position, gizmoRadius * 0.5f);
            }
        }
    }
    
    // Button for inspector to trigger diagnostics
    [ContextMenu("Run Diagnostics")]
    public void RunDiagnostics()
    {
        PerformFullDiagnostics();
    }
}