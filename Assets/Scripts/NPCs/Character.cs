using LLMUnity;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class Character : MonoBehaviour
{
    private string characterName;
    private LLMCharacter llmCharacter;
    private bool isInitialized = false;
    
    // Public property for character name
    public string CharacterName => characterName ?? "Unnamed Character";

    public void Initialize(string name, LLMCharacter character)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError("Cannot initialize character with null or empty name");
            return;
        }

        characterName = name;
        llmCharacter = character;
        isInitialized = character != null;

        // Rename the GameObject to include the character name for better identification
        if (transform.parent != null)
        {
            // Check if name already matches expected format
            string expectedName = $"NPC_{name}";
            
            // Fix the parent name if needed - we want the direct parent to be NPC_{name}
            Transform directParent = transform.parent;
            
            // Check if the parent's name is incorrect
            if (directParent.name != expectedName)
            {
                Debug.Log($"Renaming parent from {directParent.name} to {expectedName}");
                directParent.name = expectedName;
            }
        }
        else
        {
            Debug.LogWarning($"Character {name} has no parent GameObject - cannot rename properly");
        }

        if (isInitialized)
        {
            Debug.Log($"Character {name} initialized successfully with LLMCharacter");
            
            // Force animation assignment after a short delay
            var animManager = GetComponentInChildren<NPCAnimManager>();
            if (animManager != null)
            {
                StartCoroutine(ForceAnimAssignment(animManager, name));
            }
        }
        else
        {
            Debug.LogError($"Failed to initialize Character {name} - LLMCharacter is null");
        }
    }
    
    private IEnumerator ForceAnimAssignment(NPCAnimManager animManager, string characterName)
    {
        yield return new WaitForSeconds(0.2f);
        
        var animMethod = animManager.GetType().GetMethod("AnimContainerAssign", 
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            
        if (animMethod != null)
        {
            animMethod.Invoke(animManager, new object[] { characterName });
            Debug.Log($"Forced animation assignment for {characterName}");
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