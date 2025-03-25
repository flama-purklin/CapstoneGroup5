using System.Collections;
using UnityEngine;

/// <summary>
/// Main game controller that manages the game state and coordinates between subsystems.
/// </summary>
public class GameController : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private WorldCoordinator worldCoordinator;
    
    [Header("Game State")]
    [SerializeField] private GameState _currentState = GameState.DEFAULT;
    
    // We'll keep this as a static reference for easy access from other scripts
    public static GameController Instance { get; private set; }
    
    // Game data
    private Mystery coreMystery;
    private MysteryConstellation coreConstellation;
    
    // Properties
    public GameState CurrentState 
    { 
        get => _currentState;
        set 
        {
            if (_currentState != value)
            {
                GameState oldState = _currentState;
                _currentState = value;
                OnGameStateChanged(oldState, _currentState);
            }
        }
    }
    
    public Mystery CoreMystery => coreMystery;
    public MysteryConstellation CoreConstellation => coreConstellation;
    
    // Events
    public delegate void GameStateChangedDelegate(GameState oldState, GameState newState);
    public event GameStateChangedDelegate OnGameStateChangedEvent;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Find references if not set
        if (worldCoordinator == null)
        {
            worldCoordinator = FindFirstObjectByType<WorldCoordinator>();
        }
    }
    
    /// <summary>
    /// Initializes the game with the provided mystery.
    /// </summary>
    public void InitializeGame(Mystery mystery)
    {
        if (mystery == null)
        {
            Debug.LogError("Cannot initialize game with null mystery");
            return;
        }
        
        // Store references to the mystery data
        coreMystery = mystery;
        coreConstellation = mystery.Constellation;
        
        Debug.Log($"Game initialized with mystery: {mystery.Metadata?.Title ?? "Unnamed Mystery"}");
        
        // Start in DEFAULT state
        CurrentState = GameState.DEFAULT;
    }
    
    /// <summary>
    /// Handler for state changes.
    /// </summary>
    private void OnGameStateChanged(GameState oldState, GameState newState)
    {
        Debug.Log($"Game state changed: {oldState} -> {newState}");
        
        // Handle specific state transitions if needed
        switch (newState)
        {
            case GameState.DIALOGUE:
                // Enable dialogue UI, disable player movement, etc.
                break;
                
            case GameState.PAUSE:
                // Show pause menu, etc.
                break;
                
            case GameState.MINIGAME:
                // Start minigame, etc.
                break;
                
            case GameState.MYSTERY:
                // Show mystery constellation UI, etc.
                break;
                
            case GameState.FINAL:
                // Trigger end game sequence
                break;
                
            case GameState.WIN:
                // Show win screen
                break;
                
            case GameState.LOSE:
                // Show game over screen
                break;
        }
        
        // Broadcast state change event
        OnGameStateChangedEvent?.Invoke(oldState, newState);
    }
    
    /// <summary>
    /// Restarts the game with the current mystery.
    /// </summary>
    public void RestartGame()
    {
        // Reset game state
        CurrentState = GameState.DEFAULT;
        
        // Clear any temporary data
        
        // Reinitialize the world
        if (worldCoordinator != null && coreMystery != null)
        {
            worldCoordinator.InitializeWorld(coreMystery);
        }
    }
    
    /// <summary>
    /// Gets a character by ID.
    /// </summary>
    public GameObject GetCharacter(string characterId)
    {
        if (worldCoordinator != null)
        {
            return worldCoordinator.GetCharacterById(characterId);
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets evidence by ID.
    /// </summary>
    public GameObject GetEvidence(string evidenceId)
    {
        if (worldCoordinator != null)
        {
            return worldCoordinator.GetEvidenceById(evidenceId);
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets the transform for a location ID.
    /// </summary>
    public Transform GetLocation(string locationId)
    {
        if (worldCoordinator != null)
        {
            return worldCoordinator.GetLocationTransform(locationId);
        }
        
        return null;
    }
}