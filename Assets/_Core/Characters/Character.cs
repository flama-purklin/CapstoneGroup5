using LLMUnity;
using UnityEngine;

/// <summary>
/// Represents an in-game character entity.
/// The bridge between cold, hard data and a living, breathing NPC.
/// Well, as living and breathing as a bunch of polygons can be.
/// </summary>
public class Character : MonoBehaviour
{
    [Header("Character Info")]
    [SerializeField] private string characterName;
    [SerializeField] private bool isInitialized = false;
    [SerializeField] private bool debugMode = false;
    
    [Header("Dependencies")]
    [SerializeField] private LLMCharacter llmCharacter;
    
    // Events for character interaction
    public delegate void CharacterInteractionHandler(Character character);
    public event CharacterInteractionHandler OnInteractionStarted;
    public event CharacterInteractionHandler OnInteractionEnded;
    
    private void Awake()
    {
        // Try to find the LLMCharacter in children if not already assigned
        if (llmCharacter == null)
        {
            llmCharacter = GetComponentInChildren<LLMCharacter>();
        }
    }
    
    /// <summary>
    /// Initializes the character with a name and LLM character component.
    /// Giving life to the lifeless, one NPC at a time.
    /// </summary>
    public void Initialize(string name, LLMCharacter character)
    {
        characterName = name;
        
        // If we're given an explicit LLMCharacter reference, use it
        if (character != null)
        {
            llmCharacter = character;
            isInitialized = true;
            LogDebug($"Character {name} initialized with LLMCharacter");
        }
        else if (llmCharacter != null)
        {
            // We already have an LLMCharacter, so just use that
            isInitialized = true;
            LogDebug($"Character {name} using existing LLMCharacter");
        }
        else
        {
            Debug.LogError($"Failed to initialize Character {name} - No LLMCharacter available. They'll be rather boring.");
            isInitialized = false;
        }
        
        // Update the GameObject name
        gameObject.name = $"NPC_{name}";
    }
    
    /// <summary>
    /// Gets the LLMCharacter component.
    /// The brains behind the operation.
    /// </summary>
    public LLMCharacter GetLLMCharacter()
    {
        if (!isInitialized)
        {
            Debug.LogWarning($"Character {characterName} not properly initialized! Attempting to find LLMCharacter...");
            
            // Try to find in children first
            llmCharacter = GetComponentInChildren<LLMCharacter>();
            isInitialized = llmCharacter != null;
            
            if (!isInitialized)
            {
                Debug.LogError($"No LLMCharacter found for {characterName}. This character will be very boring.");
                return null;
            }
        }
        
        return llmCharacter;
    }
    
    /// <summary>
    /// Gets the character's name.
    /// Because even digital people need names.
    /// </summary>
    public string GetCharacterName()
    {
        return characterName ?? "Unnamed Character";
    }
    
    /// <summary>
    /// Checks if the character is fully initialized.
    /// Are they ready for their close-up?
    /// </summary>
    public bool IsInitialized()
    {
        return isInitialized && llmCharacter != null;
    }
    
    /// <summary>
    /// Starts interaction with this character.
    /// "Hello, would you like to talk about murder?"
    /// </summary>
    public void StartInteraction()
    {
        LogDebug($"Starting interaction with {characterName}");
        OnInteractionStarted?.Invoke(this);
    }
    
    /// <summary>
    /// Ends interaction with this character.
    /// "Goodbye, suspicious person!"
    /// </summary>
    public void EndInteraction()
    {
        LogDebug($"Ending interaction with {characterName}");
        OnInteractionEnded?.Invoke(this);
    }
    
    /// <summary>
    /// Generates a character response to player input.
    /// The magical moment when AI pretends to be human.
    /// </summary>
    public async void GenerateResponse(string playerInput)
    {
        if (!IsInitialized())
        {
            Debug.LogError($"Cannot generate response: {characterName} not initialized!");
            return;
        }
        
        LogDebug($"Generating response for input: {playerInput}");
        
        try
        {
            var llm = GetLLMCharacter();
            if (llm != null)
            {
                // Add user message to chat history
                llm.AddUserMessage(playerInput);
                
                // Generate response
                await llm.Generate();
                
                // The response is automatically added to the chat history
                LogDebug($"Generated response: {llm.chat[llm.chat.Count - 1].content}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error generating response: {e.Message}");
        }
    }
    
    private void LogDebug(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[Character:{characterName}] {message}");
        }
    }
}
