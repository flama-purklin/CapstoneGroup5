using LLMUnity;
using System.Threading.Tasks;
using UnityEngine;

public class Character : MonoBehaviour
{
    private string characterName;
    private LLMCharacter llmCharacter;
    private bool isInitialized = false;

    public void Initialize(string name, LLMCharacter character)
    {
        characterName = name;
        llmCharacter = character;
        isInitialized = character != null;

        if (isInitialized)
        {
            Debug.Log($"Character {name} initialized successfully with LLMCharacter");
        }
        else
        {
            Debug.LogError($"Failed to initialize Character {name} - LLMCharacter is null");
        }
    }

    public  LLMCharacter GetLLMCharacter()
    {
        if (!llmCharacter)
        {
            // Try to find in children first
            llmCharacter = GetComponentInChildren<LLMCharacter>();
            isInitialized = llmCharacter != null;
        }

        if (!isInitialized)
        {
            Debug.LogError($"Character {characterName} not properly initialized!");
            return null;
        }

        return llmCharacter;
    }

    public string GetCharacterName()
    {
        return characterName ?? "Unnamed Character";
    }
}