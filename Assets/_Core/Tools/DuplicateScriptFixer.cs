using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Utility to fix duplicate script definitions by adding namespaces.
/// </summary>
public class DuplicateScriptFixer : MonoBehaviour
{
    // Directory to keep as primary source
    private const string PRIMARY_DIRECTORY = "Assets/_Core/WorldSystems";
    
    // Namespace to add to scripts
    private const string NAMESPACE_NAME = "MysteryEngine";
    
    [MenuItem("Mystery Engine/Fix Duplicate Scripts")]
    public static void FixDuplicateScripts()
    {
        // Find all script files in the project
        string[] scriptFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
        
        // Group by filename to find duplicates
        var fileGroups = scriptFiles.GroupBy(Path.GetFileName).Where(g => g.Count() > 1);
        
        // Process each group of duplicates
        foreach (var group in fileGroups)
        {
            string fileName = group.Key;
            Debug.Log($"Processing duplicate: {fileName}");
            
            // Find the primary file (the one in PRIMARY_DIRECTORY)
            string primaryFile = group.FirstOrDefault(f => f.Replace('\\', '/').Contains(PRIMARY_DIRECTORY));
            
            if (string.IsNullOrEmpty(primaryFile))
            {
                // If no file in primary directory, use the first one
                primaryFile = group.First();
                Debug.Log($"  No file in primary directory, using: {primaryFile}");
                
                // Add namespace to this file
                AddNamespaceToFile(primaryFile);
            }
            else
            {
                Debug.Log($"  Primary file: {primaryFile}");
                
                // Add namespace to the primary file
                AddNamespaceToFile(primaryFile);
            }
            
            // Delete all other duplicates
            foreach (string duplicateFile in group.Where(f => f != primaryFile))
            {
                Debug.Log($"  Deleting duplicate: {duplicateFile}");
                
                // Get the asset path for Unity
                string assetPath = "Assets" + duplicateFile.Substring(Application.dataPath.Length).Replace('\\', '/');
                
                // Delete the asset
                AssetDatabase.DeleteAsset(assetPath);
            }
        }
        
        // Refresh the asset database
        AssetDatabase.Refresh();
        
        Debug.Log("Duplicate script fixing complete!");
    }
    
    private static void AddNamespaceToFile(string filePath)
    {
        // Read the file content
        string content = File.ReadAllText(filePath);
        
        // Skip files that already have a namespace
        if (content.Contains("namespace "))
        {
            Debug.Log($"  File already has a namespace, skipping: {filePath}");
            return;
        }
        
        // Find the first namespace-able code block
        int firstTypeDefIndex = -1;
        foreach (string typeKeyword in new[] { "class ", "struct ", "interface ", "enum " })
        {
            int index = content.IndexOf(typeKeyword);
            if (index >= 0 && (firstTypeDefIndex < 0 || index < firstTypeDefIndex))
            {
                firstTypeDefIndex = index;
            }
        }
        
        if (firstTypeDefIndex < 0)
        {
            Debug.LogWarning($"  Could not find class/struct/interface/enum in file: {filePath}");
            return;
        }
        
        // Find the line start for the class
        int lineStart = content.LastIndexOf('\n', firstTypeDefIndex);
        if (lineStart < 0) lineStart = 0;
        else lineStart += 1; // Skip the newline
        
        // Insert namespace wrapper
        string modified = content.Substring(0, lineStart) + 
                        $"namespace {NAMESPACE_NAME}\n{{\n" + 
                        content.Substring(lineStart) +
                        "\n}";
        
        // Write back to file
        File.WriteAllText(filePath, modified);
        
        Debug.Log($"  Added namespace to file: {filePath}");
    }
}
#endif