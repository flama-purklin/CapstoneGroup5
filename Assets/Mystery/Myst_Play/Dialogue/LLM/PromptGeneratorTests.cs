using UnityEngine;
using System.IO;
using LLMUnity;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Test class for testing the CharacterPromptGenerator with extracted character files.
/// This class provides editor utilities to verify the prompt generation for both formats.
/// </summary>
public class PromptGeneratorTests : MonoBehaviour
{
    [Header("Character Selection")]
    [SerializeField] private string characterId = "nova_winchester";
    
    [Header("Results")]
    [SerializeField, TextArea(5, 20)] private string resultPrompt;
    
    [Header("Debug Options")]
    [SerializeField] private bool verboseLogging = true;
    [SerializeField] private bool savePromptToFile = true;
    
    /// <summary>
    /// Test method that can be triggered in the Editor
    /// </summary>
    [ContextMenu("Test Character Prompt Generation")]
    public void TestCharacterPromptGeneration()
    {
        StartCoroutine(TestPromptGenerationCoroutine());
    }
    
    /// <summary>
    /// Test method specifically for Nova's character to verify speech patterns
    /// </summary>
    [ContextMenu("Test Nova's Speech Patterns")]
    public void TestNovaPromptGeneration()
    {
        characterId = "nova_winchester";
        StartCoroutine(TestPromptGenerationCoroutine());
    }
    
    /// <summary>
    /// Coroutine to test the prompt generation
    /// </summary>
    private IEnumerator TestPromptGenerationCoroutine()
    {
        Debug.Log($"Testing character prompt generation for: {characterId}");
        
        // 1. Load the character file
        string characterPath = Path.Combine(Application.streamingAssetsPath, "Characters", $"{characterId}.json");
        if (!File.Exists(characterPath))
        {
            Debug.LogError($"Character file not found at: {characterPath}");
            resultPrompt = "ERROR: Character file not found";
            yield break;
        }
        
        // 2. Read the JSON content
        string jsonContent = File.ReadAllText(characterPath);
        
        // 3. Create a mock LLMCharacter
        GameObject tempObj = new GameObject("TempCharacter");
        LLMCharacter character = tempObj.AddComponent<LLMCharacter>();
        
        // 4. Generate the prompt
        string prompt = CharacterPromptGenerator.GenerateSystemPrompt(jsonContent, character);
        
        // 5. Show the results
        if (string.IsNullOrEmpty(prompt))
        {
            Debug.LogError("Failed to generate prompt");
            resultPrompt = "ERROR: Failed to generate prompt";
        }
        else
        {
            Debug.Log($"Successfully generated prompt for {characterId}");
            resultPrompt = prompt;
            
            // Check for Nova's speech patterns
            if (characterId.ToLower().Contains("nova"))
            {
                bool hasMateTerm = prompt.Contains("mate");
                bool hasLuvTerm = prompt.Contains("luv");
                bool hasExpletives = prompt.Contains("fuck") || prompt.Contains("bloody");
                
                Debug.Log($"Nova Speech Pattern Check:");
                Debug.Log($"- Contains 'mate': {hasMateTerm}");
                Debug.Log($"- Contains 'luv': {hasLuvTerm}");
                Debug.Log($"- Contains expletives: {hasExpletives}");
                
                if (hasMateTerm && hasLuvTerm && hasExpletives)
                {
                    Debug.Log("✅ Nova's distinctive speech patterns were preserved in the prompt");
                }
                else
                {
                    Debug.LogWarning("⚠️ Nova's distinctive speech patterns may be incomplete in the prompt!");
                }
            }
            
            // Verbose logging
            if (verboseLogging)
            {
                Debug.Log("Generated Prompt:");
                // Log in chunks to avoid console truncation
                for (int i = 0; i < prompt.Length; i += 1000)
                {
                    int length = Mathf.Min(1000, prompt.Length - i);
                    Debug.Log(prompt.Substring(i, length));
                }
            }
            
            // Save to file
            if (savePromptToFile)
            {
                string promptsPath = Path.Combine(Application.streamingAssetsPath, "Prompts");
                if (!Directory.Exists(promptsPath)) Directory.CreateDirectory(promptsPath);
                
                string outputPath = Path.Combine(promptsPath, $"{characterId}_prompt.txt");
                File.WriteAllText(outputPath, prompt);
                Debug.Log($"Saved prompt to: {outputPath}");
            }
        }
        
        // 6. Clean up
        DestroyImmediate(tempObj);
        
        yield return null;
    }
    
    /// <summary>
    /// Test method to verify all characters in the Characters directory
    /// </summary>
    [ContextMenu("Test All Character Prompts")]
    public void TestAllCharacterPrompts()
    {
        StartCoroutine(TestAllCharacterPromptsCoroutine());
    }
    
    /// <summary>
    /// Coroutine to test all character prompts
    /// </summary>
    private IEnumerator TestAllCharacterPromptsCoroutine()
    {
        Debug.Log("Testing all character prompts...");
        
        // 1. Get all character files
        string charactersPath = Path.Combine(Application.streamingAssetsPath, "Characters");
        if (!Directory.Exists(charactersPath))
        {
            Debug.LogError($"Characters directory not found at: {charactersPath}");
            yield break;
        }
        
        string[] characterFiles = Directory.GetFiles(charactersPath, "*.json");
        Debug.Log($"Found {characterFiles.Length} character files");
        
        int successCount = 0;
        List<string> failedCharacters = new List<string>();
        
        // 2. Process each character
        foreach (string characterFile in characterFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(characterFile);
            Debug.Log($"Processing character: {fileName}");
            
            // Read the JSON content
            string jsonContent = File.ReadAllText(characterFile);
            
            // Create a mock LLMCharacter
            GameObject tempObj = new GameObject("TempCharacter");
            LLMCharacter character = tempObj.AddComponent<LLMCharacter>();
            
            // Generate the prompt
            string prompt = CharacterPromptGenerator.GenerateSystemPrompt(jsonContent, character);
            
            // Check result
            if (string.IsNullOrEmpty(prompt))
            {
                Debug.LogError($"Failed to generate prompt for {fileName}");
                failedCharacters.Add(fileName);
            }
            else
            {
                successCount++;
                
                // Save to file if requested
                if (savePromptToFile)
                {
                    string promptsPath = Path.Combine(Application.streamingAssetsPath, "Prompts");
                    if (!Directory.Exists(promptsPath)) Directory.CreateDirectory(promptsPath);
                    
                    string outputPath = Path.Combine(promptsPath, $"{fileName}_prompt.txt");
                    File.WriteAllText(outputPath, prompt);
                }
            }
            
            // Clean up
            DestroyImmediate(tempObj);
            
            yield return null;
        }
        
        // 3. Report results
        Debug.Log($"Prompt generation complete: {successCount}/{characterFiles.Length} successful");
        
        if (failedCharacters.Count > 0)
        {
            Debug.LogError($"Failed characters: {string.Join(", ", failedCharacters)}");
        }
        
        yield return null;
    }
}