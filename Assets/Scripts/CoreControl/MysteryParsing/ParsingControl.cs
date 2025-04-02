using UnityEngine;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using Newtonsoft.Json;
using System;
using CoreControl.MysteryParsing;

/// <summary>
/// Parses the mystery JSON into usable game objects and extracts character data.
/// Integrates with MysteryCharacterExtractor to generate character files for LLM integration.
/// </summary>
public class ParsingControl : MonoBehaviour
{
    [Header("Configuration")]
    public string mysteryFiles = "MysteryStorage";
    [SerializeField] private bool _verboseLogging = false;
    
    [Header("References")]
    [SerializeField] private MysteryCharacterExtractor _characterExtractor;
    
    // Events
    public event Action<float> OnParsingProgress;
    public event Action<Mystery> OnMysteryParsed;
    public event Action<int> OnCharactersExtracted;
    public event Action OnParsingComplete;
    
    // Extraction status
    private bool _parsingComplete = false;
    public bool IsParsingComplete => _parsingComplete;
    
    private void Awake()
    {
        // Find or create the character extractor if not assigned
        if (_characterExtractor == null)
        {
            _characterExtractor = FindFirstObjectByType<MysteryCharacterExtractor>();
            
            if (_characterExtractor == null && GetComponent<MysteryCharacterExtractor>() == null)
            {
                _characterExtractor = gameObject.AddComponent<MysteryCharacterExtractor>();
                Debug.Log("Added MysteryCharacterExtractor component as none was assigned");
            }
        }
        
        // Connect to character extractor events
        if (_characterExtractor != null)
        {
            _characterExtractor.OnExtractionProgress += HandleExtractionProgress;
            _characterExtractor.OnCharactersExtracted += HandleCharactersExtracted;
        }
        
        // Parse mystery
        ParseMystery();
    }
    
    private void OnDestroy()
    {
        // Disconnect from events
        if (_characterExtractor != null)
        {
            _characterExtractor.OnExtractionProgress -= HandleExtractionProgress;
            _characterExtractor.OnCharactersExtracted -= HandleCharactersExtracted;
        }
    }

    /// <summary>
    /// Parses the mystery JSON file and extracts character data
    /// </summary>
    public void ParseMystery()
    {
        // Report initial progress
        OnParsingProgress?.Invoke(0.0f);
        
        // Retrieve the mystery JSON from StreamingAssets
        string mysteryPath = Path.Combine(Application.streamingAssetsPath, mysteryFiles);
        if (!Directory.Exists(mysteryPath))
        {
            Debug.LogError($"Mystery folder not found at: {mysteryPath}");
            OnParsingProgress?.Invoke(1.0f); // Complete progress even though failed
            return;
        }
        
        // Find mystery files
        var foundMysteries = Directory.GetFiles(mysteryPath, "*.json").ToArray();
        if (foundMysteries.Length == 0)
        {
            Debug.LogError($"No mystery JSON files found in {mysteryPath}");
            OnParsingProgress?.Invoke(1.0f); // Complete progress even though failed
            return;
        }

        string firstMystery = foundMysteries[0];
        Debug.Log("Found mystery file at: " + firstMystery);

        // Read JSON to a parsable string
        string jsonContent = File.ReadAllText(firstMystery);
        
        // Report progress after reading file
        OnParsingProgress?.Invoke(0.1f);

        try
        {
            // First ensure GameControl.GameController exists
            if (GameControl.GameController == null)
            {
                Debug.LogError("GameControl.GameController is null during mystery parsing! Attempting to find it...");
                
                // Try to find GameController
                var gameController = FindFirstObjectByType<GameControl>();
                if (gameController == null)
                {
                    Debug.LogError("No GameControl found in scene! Creating one to prevent errors...");
                    GameObject controllerObj = new GameObject("GameController");
                    gameController = controllerObj.AddComponent<GameControl>();
                }
                
                // Wait for next frame to ensure Awake() has run
                StartCoroutine(DelayedParsing(jsonContent));
                return;
            }
            
            // Create a core mystery object with all information stored within
            Mystery parsedMystery = JsonConvert.DeserializeObject<Mystery>(jsonContent);
            
            // Set the mystery on the GameController
            GameControl.GameController.coreMystery = parsedMystery;
            
            // Add null check for safety
            if (GameControl.GameController.coreMystery == null)
            {
                Debug.LogError("Failed to deserialize mystery JSON - Mystery object is null");
                _parsingComplete = true;
                OnParsingProgress?.Invoke(1.0f);
                OnParsingComplete?.Invoke();
                return;
            }
            
            // Ensure Constellation property is not null
            if (GameControl.GameController.coreMystery.Constellation == null)
            {
                Debug.LogError("Mystery object has null Constellation property");
                // Create an empty constellation to prevent null reference exceptions
                GameControl.GameController.coreMystery.Constellation = new MysteryConstellation();
                GameControl.GameController.coreConstellation = GameControl.GameController.coreMystery.Constellation;
            }
            else
            {
                GameControl.GameController.coreConstellation = GameControl.GameController.coreMystery.Constellation;
            }
            
            // Report progress after deserializing
            OnParsingProgress?.Invoke(0.3f);
            
            // Fire event for mystery parsed
            OnMysteryParsed?.Invoke(GameControl.GameController.coreMystery);
            
            if (_verboseLogging)
            {
                LogMysteryContents();
            }
            
            // Report progress after validation
            OnParsingProgress?.Invoke(0.5f);
            
            // Extract characters from the mystery
            if (_characterExtractor != null)
            {
                try
                {
                    Debug.Log("Starting character extraction process");
                    _characterExtractor.ExtractCharactersFromMystery(GameControl.GameController.coreMystery);
                    
                    // Note: Progress and completion callbacks are handled by event handlers
                    
                    // Add timeout check (will be used in InitializationManager)
                    StartCoroutine(ExtractionTimeoutCheck());
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error during character extraction: {ex.Message}");
                    Debug.LogException(ex);
                    // Signal completion even though extraction failed
                    _parsingComplete = true;
                    OnParsingProgress?.Invoke(1.0f);
                    OnParsingComplete?.Invoke();
                }
            }
            else
            {
                Debug.LogWarning("No MysteryCharacterExtractor available. Character files will not be generated.");
                _parsingComplete = true;
                OnParsingProgress?.Invoke(1.0f); // Complete progress without extraction
                OnParsingComplete?.Invoke();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error parsing mystery JSON: {ex.Message}");
            Debug.LogException(ex);
            
            // Signal completion even if parsing failed
            _parsingComplete = true;
            OnParsingProgress?.Invoke(1.0f);
            OnParsingComplete?.Invoke();
        }
    }
    
    /// <summary>
    /// Asynchronously parses the mystery JSON file and extracts character data
    /// </summary>
    public async Task<Mystery> ParseMysteryAsync()
    {
        // Report initial progress
        OnParsingProgress?.Invoke(0.0f);
        
        // Retrieve the mystery JSON from StreamingAssets
        string mysteryPath = Path.Combine(Application.streamingAssetsPath, mysteryFiles);
        if (!Directory.Exists(mysteryPath))
        {
            Debug.LogError($"Mystery folder not found at: {mysteryPath}");
            OnParsingProgress?.Invoke(1.0f); // Complete progress even though failed
            return null;
        }
        
        // Find mystery files
        var foundMysteries = Directory.GetFiles(mysteryPath, "*.json").ToArray();
        if (foundMysteries.Length == 0)
        {
            Debug.LogError($"No mystery JSON files found in {mysteryPath}");
            OnParsingProgress?.Invoke(1.0f); // Complete progress even though failed
            return null;
        }

        string firstMystery = foundMysteries[0];
        Debug.Log("Found mystery file at: " + firstMystery);

        // Read JSON to a parsable string
        string jsonContent = await File.ReadAllTextAsync(firstMystery);
        
        // Report progress after reading file
        OnParsingProgress?.Invoke(0.1f);

        try
        {
            // Create a core mystery object
            Mystery mystery = JsonConvert.DeserializeObject<Mystery>(jsonContent);
            
            // Set in game controller
            GameControl.GameController.coreMystery = mystery;
            GameControl.GameController.coreConstellation = mystery.Constellation;
            
            // Report progress after deserializing
            OnParsingProgress?.Invoke(0.3f);
            
            // Fire event for mystery parsed
            OnMysteryParsed?.Invoke(mystery);
            
            if (_verboseLogging)
            {
                LogMysteryContents();
            }
            
            // Report progress after validation
            OnParsingProgress?.Invoke(0.5f);
            
            // Extract characters from the mystery asynchronously
            if (_characterExtractor != null)
            {
                try
                {
                    Debug.Log("Starting asynchronous character extraction process");
                    int count = await _characterExtractor.ExtractCharactersAsync(mystery);
                    
                    // Manually invoke handlers in case they didn't fire during async operation
                    if (!_parsingComplete)
                    {
                        Debug.Log($"Async extraction completed with {count} characters");
                        HandleCharactersExtracted(count);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error during async character extraction: {ex.Message}");
                    Debug.LogException(ex);
                    // Signal completion even though extraction failed
                    _parsingComplete = true;
                    OnParsingProgress?.Invoke(1.0f);
                    OnParsingComplete?.Invoke();
                }
            }
            else
            {
                Debug.LogWarning("No MysteryCharacterExtractor available. Character files will not be generated.");
                _parsingComplete = true;
                OnParsingProgress?.Invoke(1.0f); // Complete progress without extraction
                OnParsingComplete?.Invoke();
            }
            
            return mystery;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error parsing mystery JSON: {ex.Message}");
            OnParsingProgress?.Invoke(1.0f); // Complete progress even though failed
            return null;
        }
    }
    
    /// <summary>
    /// Handles progress updates from the character extractor
    /// </summary>
    private void HandleExtractionProgress(float progress)
    {
        // Scale extraction progress to 0.5-1.0 range (second half of overall process)
        float overallProgress = 0.5f + (progress * 0.5f);
        OnParsingProgress?.Invoke(overallProgress);
        
        if (_verboseLogging)
        {
            Debug.Log($"Character extraction progress: {progress:P0} (Overall: {overallProgress:P0})");
        }
    }
    
    /// <summary>
    /// Handles completion of character extraction
    /// </summary>
    private void HandleCharactersExtracted(int count)
    {
        Debug.Log($"Character extraction complete. {count} characters processed.");
        OnCharactersExtracted?.Invoke(count);
        OnParsingProgress?.Invoke(1.0f); // Complete progress
        
        // Signal that parsing is complete
        _parsingComplete = true;
        Debug.Log("Parsing complete - firing OnParsingComplete event");
        OnParsingComplete?.Invoke();
    }
    
    /// <summary>
    /// Timeout coroutine to ensure extraction doesn't hang indefinitely
    /// </summary>
    private IEnumerator ExtractionTimeoutCheck()
    {
        // Wait up to 60 seconds for extraction to complete
        float startTime = Time.realtimeSinceStartup;
        const float EXTRACTION_TIMEOUT = 60f;
        
        while (!_parsingComplete && Time.realtimeSinceStartup - startTime < EXTRACTION_TIMEOUT)
        {
            yield return new WaitForSeconds(1f);
        }
        
        // If still not complete after timeout, force completion
        if (!_parsingComplete)
        {
            Debug.LogWarning($"Character extraction timed out after {EXTRACTION_TIMEOUT} seconds. Forcing completion.");
            _parsingComplete = true;
            OnParsingProgress?.Invoke(1.0f);
            OnParsingComplete?.Invoke();
        }
    }
    
    /// <summary>
    /// Logs detailed mystery contents for debugging
    /// </summary>
    /// <summary>
    /// Coroutine to attempt parsing after a delay to ensure GameController is initialized
    /// </summary>
    private IEnumerator DelayedParsing(string jsonContent)
    {
        yield return new WaitForEndOfFrame();
        
        // Check if GameController is now available
        if (GameControl.GameController == null)
        {
            Debug.LogError("GameController still null after delay. Proceeding with parsing anyway with fallback.");
            
            // Create a Mystery object directly without assigning to GameController
            Mystery parsedMystery = JsonConvert.DeserializeObject<Mystery>(jsonContent);
            
            // Signal completion
            _parsingComplete = true;
            OnParsingProgress?.Invoke(1.0f);
            OnParsingComplete?.Invoke();
            yield break;
        }
        
        // Retry parsing now that GameController exists
        try
        {
            GameControl.GameController.coreMystery = JsonConvert.DeserializeObject<Mystery>(jsonContent);
            
            // Set up constellation
            if (GameControl.GameController.coreMystery.Constellation == null)
            {
                GameControl.GameController.coreMystery.Constellation = new MysteryConstellation();
            }
            
            GameControl.GameController.coreConstellation = GameControl.GameController.coreMystery.Constellation;
            
            // Continue with extraction
            OnParsingProgress?.Invoke(0.3f);
            OnMysteryParsed?.Invoke(GameControl.GameController.coreMystery);
            
            if (_verboseLogging)
            {
                LogMysteryContents();
            }
            
            OnParsingProgress?.Invoke(0.5f);
            
            // Extract characters from the mystery
            if (_characterExtractor != null)
            {
                try
                {
                    Debug.Log("Starting character extraction process");
                    _characterExtractor.ExtractCharactersFromMystery(GameControl.GameController.coreMystery);
                    
                    StartCoroutine(ExtractionTimeoutCheck());
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error during character extraction: {ex.Message}");
                    Debug.LogException(ex);
                    
                    // Signal completion even though extraction failed
                    _parsingComplete = true;
                    OnParsingProgress?.Invoke(1.0f);
                    OnParsingComplete?.Invoke();
                }
            }
            else
            {
                Debug.LogWarning("No MysteryCharacterExtractor available. Character files will not be generated.");
                _parsingComplete = true;
                OnParsingProgress?.Invoke(1.0f);
                OnParsingComplete?.Invoke();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in delayed parsing: {ex.Message}");
            Debug.LogException(ex);
            
            // Signal completion even if parsing failed
            _parsingComplete = true;
            OnParsingProgress?.Invoke(1.0f);
            OnParsingComplete?.Invoke();
        }
    }

    private void LogMysteryContents()
    {
        // Safety check
        if (GameControl.GameController == null || 
            GameControl.GameController.coreConstellation == null ||
            GameControl.GameController.coreMystery == null)
        {
            Debug.LogError("Cannot log mystery contents - GameController or mystery data is null");
            return;
        }
        
        // Log node IDs
        Debug.Log($"Mystery contains {GameControl.GameController.coreConstellation.Nodes.Count} nodes");
        foreach (var node in GameControl.GameController.coreConstellation.Nodes)
        {
            Debug.Log("Node Id: " + node.Key);
        }

        // Log character details
        Debug.Log($"Mystery contains {GameControl.GameController.coreMystery.Characters.Count} characters");
        foreach (var character in GameControl.GameController.coreMystery.Characters)
        {
            Debug.Log($"Character: {character.Key} - {character.Value.MindEngine.Identity.Name}");
            
            // Log whereabouts
            if (character.Value.Core.Whereabouts != null)
            {
                Debug.Log($"  Whereabouts: {character.Value.Core.Whereabouts.Count}");
                foreach (var whereabout in character.Value.Core.Whereabouts)
                {
                    string value = whereabout.WhereaboutData.Circumstance ?? whereabout.WhereaboutData.Location;
                    Debug.Log($"  - Whereabout {whereabout.Key}: {value}");
                }
            }

            // Log relationships
            if (character.Value.Core.Relationships != null)
            {
                Debug.Log($"  Relationships: {character.Value.Core.Relationships.Count}");
                foreach (var relationship in character.Value.Core.Relationships)
                {
                    Debug.Log($"  - Relationship with: {relationship.CharName} ({relationship.RelationshipData.Attitude})");
                }
            }
        }
    }
}
