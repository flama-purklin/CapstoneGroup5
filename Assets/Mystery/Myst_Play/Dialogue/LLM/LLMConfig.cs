using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LLMUnity;
using System.IO;

[RequireComponent(typeof(LLM))]
public class LLMConfig : MonoBehaviour
{
    [Tooltip("Set this to true to automatically configure the LLM for all available characters")]
    public bool autoConfigureForAllCharacters = true;

    private void Awake()
    {
        if (autoConfigureForAllCharacters)
        {
            ConfigureForAllCharacters();
        }
    }

    public void ConfigureForAllCharacters()
    {
        LLM llm = GetComponent<LLM>();
        if (!llm)
        {
            Debug.LogError("LLM component not found!");
            return;
        }

        string charactersPath = Path.Combine(Application.streamingAssetsPath, "Characters");
        if (!Directory.Exists(charactersPath))
        {
            Debug.LogError($"Characters directory not found at: {charactersPath}");
            return;
        }

        string[] characterFiles = Directory.GetFiles(charactersPath, "*.json");
        int characterCount = characterFiles.Length;

        if (characterCount == 0)
        {
            Debug.LogWarning("No character files found! Using default parallel prompts setting.");
            return;
        }

        // If context size is critical, allocate a reasonable amount per character
        // (This helps prevent memory issues with too many characters)
        int contextPerCharacter = 4096; // Reasonable context per character
        if (llm.contextSize > 0 && characterCount > 0)
        {
            int totalNeededContext = contextPerCharacter * characterCount;
            if (totalNeededContext > llm.contextSize)
            {
                Debug.LogWarning($"Warning: Total context needed ({totalNeededContext}) exceeds LLM context size ({llm.contextSize})");
            }
        }
        
        // Configure for all characters
        llm.parallelPrompts = characterCount;
        Debug.Log($"Configured LLM for {characterCount} parallel prompts to handle all characters");
    }
}