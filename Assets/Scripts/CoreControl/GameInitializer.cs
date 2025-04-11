using LLMUnity;
using UnityEngine;
using System.IO;

/// <summary>
/// Legacy GameInitializer script - updated to work with the unified scene approach.
/// This script is now just a bridge to the new InitializationManager.
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [SerializeField] private LLM llm;
    [SerializeField] private NPCManager npcManager;
    [SerializeField] private CharacterManager characterManager;
    
    [Header("Unified Scene Configuration")]
    [SerializeField] private GameObject loadingOverlay;
    
    // useUnifiedSceneApproach field removed - all code now uses unified approach

    private void Start()
    {
        // Always use the unified scene approach
        // Set up InitializationManager
        var initializationManager = FindFirstObjectByType<InitializationManager>();
        if (initializationManager == null)
        {
            GameObject initManagerObj = new GameObject("InitializationManager");
            initializationManager = initManagerObj.AddComponent<InitializationManager>();
            
            // Configure the initialization manager
            var initManagerComponent = initManagerObj.GetComponent<InitializationManager>();
            if (initManagerComponent != null)
            {
                // Use SerializedField to assign references in editor
                Debug.Log("Using unified scene approach with InitializationManager");
            }
        }
        
        // Legacy code removed completely - all initialization now handled by InitializationManager
    }
    
    /// <summary>
    /// Legacy method - kept for backwards compatibility but not used in unified scene approach
    /// </summary>
    private void VerifyCharacterFiles()
    {
        string charactersPath = Path.Combine(Application.streamingAssetsPath, "Characters");
        if (!Directory.Exists(charactersPath))
        {
            Debug.LogError("Characters directory not found! Character dialogue will not work correctly.");
            return;
        }
        
        string[] characterFiles = Directory.GetFiles(charactersPath, "*.json");
        
        
        int validFileCount = 0;
        bool novaFileVerified = false;
        
        foreach (string file in characterFiles)
        {
            string fileName = Path.GetFileName(file);
            
            try
            {
                // Load and verify the file structure
                string fileContent = File.ReadAllText(file);
                
                // Check if it has the required two-chamber structure
                bool hasCoreSection = fileContent.Contains("\"core\":");
                bool hasMindEngineSection = fileContent.Contains("\"mind_engine\":");
                
                if (hasCoreSection && hasMindEngineSection)
                {
                    validFileCount++;
                    
                }
                else
                {
                    Debug.LogWarning($"  ⚠ {fileName} - Missing required sections: " + 
                        (hasCoreSection ? "" : "core, ") + 
                        (hasMindEngineSection ? "" : "mind_engine"));
                }
                
                // Special check for Nova's file
                if (fileName.ToLower().Contains("nova"))
                {
                    // Verify Nova's file contains the important speech patterns
                    bool hasMateTerm = fileContent.Contains("mate");
                    bool hasLuvTerm = fileContent.Contains("luv");
                    bool hasExpletives = fileContent.Contains("fuck") || fileContent.Contains("bloody");
                    
                    novaFileVerified = true;
                    
                    if (!hasMateTerm || !hasLuvTerm || (!hasExpletives))
                    {
                        Debug.LogWarning($"Nova's distinctive speech patterns may be missing from {fileName}!");
                        Debug.LogWarning($"- Contains 'mate': {hasMateTerm}");
                        Debug.LogWarning($"- Contains 'luv': {hasLuvTerm}");
                        Debug.LogWarning($"- Contains expletives: {hasExpletives}");
                    }
                    else
                    {
                        
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error verifying file {fileName}: {ex.Message}");
            }
        }
        
        // Log overall validation results
        
        
        if (!novaFileVerified)
        {
            Debug.LogError("Nova's character file was not found or could not be verified!");
        }
        
        // Create the character files directory if it doesn't exist (only a safeguard)
        if (characterFiles.Length == 0)
        {
            Debug.LogWarning("No character files found. This will cause dialogue system issues!");
        }
    }
}
