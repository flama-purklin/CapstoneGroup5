using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Tool to fix duplicate class definitions by adding namespaces without deleting files.
/// </summary>
public class NamespaceFixer : MonoBehaviour
{
    [MenuItem("Mystery Engine/Apply Namespaces")]
    public static void ApplyNamespaces()
    {
        Debug.Log("Starting namespace application process...");
        
        // Define namespaces for different directories
        Dictionary<string, string> directoryNamespaces = new Dictionary<string, string>()
        {
            { "Assets/_Core/Systems", "MysteryEngine.Systems" },
            { "Assets/_Core/World", "MysteryEngine.World" },
            { "Assets/_Core/WorldSystems", "MysteryEngine" }
        };
        
        // Apply namespaces
        foreach (var entry in directoryNamespaces)
        {
            string directory = entry.Key;
            string namespaceToApply = entry.Value;
            
            Debug.Log($"Applying namespace '{namespaceToApply}' to scripts in {directory}");
            
            // Find all scripts in directory
            string[] guids = AssetDatabase.FindAssets("t:Script", new[] { directory });
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                
                if (script != null)
                {
                    Debug.Log($"  Modifying: {path}");
                    
                    try
                    {
                        // Read the script's content
                        string content = script.text;
                        
                        // Skip files that already have a namespace
                        if (content.Contains("namespace "))
                        {
                            Debug.Log($"    Already has namespace, skipping");
                            continue;
                        }
                        
                        // find class/struct/enum/interface declaration
                        int typeDefIndex = -1;
                        string[] typeKeywords = new[] { "public class ", "class ", "public struct ", "struct ", "public interface ", "interface ", "public enum ", "enum " };
                        
                        foreach (string keyword in typeKeywords)
                        {
                            int index = content.IndexOf(keyword);
                            if (index >= 0 && (typeDefIndex < 0 || index < typeDefIndex))
                            {
                                typeDefIndex = index;
                            }
                        }
                        
                        if (typeDefIndex < 0)
                        {
                            Debug.Log($"    No type definition found, skipping");
                            continue;
                        }
                        
                        // Find the beginning of the line
                        int lineStart = content.LastIndexOf('\n', typeDefIndex);
                        if (lineStart < 0)
                        {
                            lineStart = 0;
                        }
                        else
                        {
                            lineStart++; // Skip the newline
                        }
                        
                        // Add namespace
                        string modifiedContent = content.Substring(0, lineStart) +
                                              $"namespace {namespaceToApply}\n{{\n" +
                                              content.Substring(lineStart) +
                                              "\n}";
                        
                        // Write the modified content back
                        System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath, path.Substring(7)), modifiedContent);
                        
                        Debug.Log($"    Added namespace: {namespaceToApply}");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"    Error processing {path}: {e.Message}");
                    }
                }
            }
        }
        
        // Refresh the database
        AssetDatabase.Refresh();
        
        Debug.Log("Namespace application complete. Please check for compilation errors.");
    }
}
#endif