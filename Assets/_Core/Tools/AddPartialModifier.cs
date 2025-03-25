using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Utility to fix class conflicts by adding 'partial' modifiers.
/// </summary>
public class AddPartialModifier : MonoBehaviour
{
    // Classes that need the partial modifier
    private static readonly string[] PartialClassNames = new[]
    {
        "Mystery",
        "MysteryNode",
        "MysteryEnvironment"
    };
    
    [MenuItem("Mystery Engine/Apply Partial Modifiers")]
    public static void ApplyPartialModifiers()
    {
        Debug.Log("Starting partial class modification...");
        
        // Find all script files
        string[] guids = AssetDatabase.FindAssets("t:Script");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ModifyScript(path);
        }
        
        AssetDatabase.Refresh();
        
        Debug.Log("Partial class modification complete!");
    }
    
    private static void ModifyScript(string path)
    {
        // Skip generated files
        if (path.Contains("Generated") || path.Contains("Editor"))
            return;
        
        // Read the file
        string content = File.ReadAllText(path);
        bool modified = false;
        
        // Check for class definitions
        foreach (string className in PartialClassNames)
        {
            // Pattern matches "class ClassName" but not "partial class ClassName"
            string pattern = @"(?<!partial\s+)class\s+" + className + @"\b";
            
            if (Regex.IsMatch(content, pattern))
            {
                // Add partial modifier
                content = Regex.Replace(content, pattern, "partial class " + className);
                modified = true;
                Debug.Log($"Added partial modifier to {className} in {path}");
            }
        }
        
        if (modified)
        {
            // Write back to the file
            File.WriteAllText(path, content);
        }
    }
}
#endif