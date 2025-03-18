using LLMUnity;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Threading.Tasks;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private LLM llm;
    [SerializeField] private NPCManager npcManager;
    [SerializeField] private CharacterManager characterManager;

    private void Start()
    {
  
        GameObject persistentSystems = GameObject.Find("Persistent Systems");
        if (!persistentSystems)
        {
            persistentSystems = new GameObject("Persistent Systems");
            DontDestroyOnLoad(persistentSystems);
        }

        if (!llm) llm = FindFirstObjectByType<LLM>();
        if (!npcManager) npcManager = FindFirstObjectByType<NPCManager>();
        if (!characterManager) characterManager = FindFirstObjectByType<CharacterManager>();

        if (llm) llm.transform.SetParent(persistentSystems.transform);
        if (npcManager) npcManager.transform.SetParent(persistentSystems.transform);
        if (characterManager) characterManager.transform.SetParent(persistentSystems.transform);

        InitializeGame();
    }

    private async void InitializeGame()
    {
        Debug.Log("Starting game initialization sequence...");
        
        // Find parsing control
        ParsingControl parsingControl = FindFirstObjectByType<ParsingControl>();
        
        // Step 1: Wait for LLM to start
        Debug.Log("INITIALIZATION STEP 1: Waiting for LLM to start...");
        int waitCount = 0;
        float startTime = Time.realtimeSinceStartup;
        
        while (!llm.started)
        {
            waitCount++;
            if (waitCount % 100 == 0) // Log every 100 frames to avoid spam
            {
                float elapsedTime = Time.realtimeSinceStartup - startTime;
                Debug.Log($"Still waiting for LLM to start... ({elapsedTime:F1} seconds elapsed)");
            }
            await Task.Yield();
        }
        
        float llmLoadTime = Time.realtimeSinceStartup - startTime;
        Debug.Log($"LLM started successfully in {llmLoadTime:F1} seconds");
        
        // Step 2: Wait for mystery parsing and character extraction
        Debug.Log("INITIALIZATION STEP 2: Mystery parsing and character extraction");
        bool parsingCompleted = false;
        
        if (parsingControl != null)
        {
            // Register for completion event
            void OnParsingComplete()
            {
                parsingCompleted = true;
                Debug.Log("Received parsing completion event");
            }
            
            // Subscribe to completion event
            parsingControl.OnParsingComplete += OnParsingComplete;
            
            // Start waiting
            startTime = Time.realtimeSinceStartup;
            waitCount = 0;
            
            Debug.Log("Waiting for mystery parsing and character extraction to complete...");
            
            // Wait for either the event or the IsParsingComplete flag
            while (!parsingCompleted && !parsingControl.IsParsingComplete)
            {
                waitCount++;
                if (waitCount % 100 == 0)
                {
                    float elapsedTime = Time.realtimeSinceStartup - startTime;
                    Debug.Log($"Still waiting for parsing to complete... ({elapsedTime:F1} seconds elapsed)");
                }
                await Task.Yield();
            }
            
            // Unsubscribe from event
            parsingControl.OnParsingComplete -= OnParsingComplete;
            
            float parsingTime = Time.realtimeSinceStartup - startTime;
            Debug.Log($"Mystery parsing and character extraction complete in {parsingTime:F1} seconds");
            
            // Verify character files
            VerifyCharacterFiles();
        }
        else
        {
            Debug.LogWarning("ParsingControl not found. Character files may not be properly extracted!");
        }
        
        // Step 3: Initialize NPCs and Character Manager
        Debug.Log("INITIALIZATION STEP 3: Character Manager initialization");
        startTime = Time.realtimeSinceStartup;
        
        if (characterManager != null)
        {
            // Ensure character manager is initialized
            Debug.Log("Initializing character manager...");
            
            // Wait for character initialization to complete
            if (npcManager != null)
            {
                Debug.Log("Initializing NPCs with character data...");
                try 
                {
                    await npcManager.Initialize();
                    Debug.Log("NPC initialization complete");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error during NPC initialization: {ex.Message}");
                    Debug.LogException(ex);
                }
            }
            else
            {
                Debug.LogWarning("NPCManager not found. NPCs will not be properly initialized.");
            }
        }
        else
        {
            Debug.LogWarning("CharacterManager not found. Character dialogue may not work properly.");
        }
        
        float npcInitTime = Time.realtimeSinceStartup - startTime;
        Debug.Log($"Character initialization complete in {npcInitTime:F1} seconds");
        
        // Step 4: Finalize and load main scene
        Debug.Log("INITIALIZATION STEP 4: Loading main game scene");
        
        // Load main scene
        SceneManager.LoadScene("SystemsTest");
        Debug.Log("Main scene load requested");
    }
    
    private void VerifyCharacterFiles()
    {
        string charactersPath = System.IO.Path.Combine(Application.streamingAssetsPath, "Characters");
        if (!System.IO.Directory.Exists(charactersPath))
        {
            Debug.LogError("Characters directory not found! Character dialogue will not work correctly.");
            return;
        }
        
        string[] characterFiles = System.IO.Directory.GetFiles(charactersPath, "*.json");
        Debug.Log($"Found {characterFiles.Length} character files:");
        
        int validFileCount = 0;
        bool novaFileVerified = false;
        
        foreach (string file in characterFiles)
        {
            string fileName = System.IO.Path.GetFileName(file);
            
            try
            {
                // Load and verify the file structure
                string fileContent = System.IO.File.ReadAllText(file);
                
                // Check if it has the required two-chamber structure
                bool hasCoreSection = fileContent.Contains("\"core\":");
                bool hasMindEngineSection = fileContent.Contains("\"mind_engine\":");
                
                if (hasCoreSection && hasMindEngineSection)
                {
                    validFileCount++;
                    Debug.Log($"  ✓ {fileName} - Valid structure");
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
                        Debug.Log($"Nova's distinctive speech patterns are preserved correctly in {fileName}.");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error verifying file {fileName}: {ex.Message}");
            }
        }
        
        // Log overall validation results
        Debug.Log($"Character file validation: {validFileCount}/{characterFiles.Length} files have valid structure");
        
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