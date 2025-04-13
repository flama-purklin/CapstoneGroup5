using LLMUnity;
using System.Threading.Tasks;
using UnityEngine;

public class Character : MonoBehaviour
{
    private string characterName;
    private LLMCharacter llmCharacter; // This reference MUST be set by Initialize
    private bool isInitialized = false; // Tracks if Initialize was called successfully

    // Initialize is called by InitializationManager after NPC spawn and CharacterManager init
    public void Initialize(string name, LLMCharacter characterRef)
    {
        characterName = name;
        llmCharacter = characterRef; // Store the reference passed from CharacterManager
        isInitialized = llmCharacter != null;

        if (!isInitialized)
        {
            Debug.LogError($"Character '{name}': Failed to initialize - Provided LLMCharacter reference was null!");
        }
    }

    // Returns the externally set LLMCharacter reference.
    public LLMCharacter GetLLMCharacter()
    {
        if (!isInitialized || llmCharacter == null)
        {
            // This should not happen if InitializationManager works correctly.
            Debug.LogError($"Character '{GetCharacterName()}': LLMCharacter reference not set or null! Was Initialize called correctly?");
            // Returning null here will cause errors downstream (like in DialogueControl), highlighting the initialization issue.
            // DO NOT use GetComponentInChildren as a fallback - it hides the real problem.
            return null; 
        }
        return llmCharacter;
    }

    public string GetCharacterName()
    {
        return characterName ?? gameObject.name ?? "Unnamed Character"; 
    }
}
