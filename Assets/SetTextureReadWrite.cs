using UnityEditor;
using UnityEngine;
using System.IO;

public class SetTextureReadWrite : EditorWindow
{
    [MenuItem("Tools/Set Read/Write on Sprites Only")]
    public static void SetSpritesReadable()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources/Sprites" });

        int changedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (path.EndsWith(".png"))
            {
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null && !importer.isReadable)
                {
                    importer.isReadable = true;
                    importer.SaveAndReimport();
                    changedCount++;
                }
            }
        }

        Debug.Log($"Set Read/Write Enabled on {changedCount} PNG textures in Resources/Sprites.");
    }
}
