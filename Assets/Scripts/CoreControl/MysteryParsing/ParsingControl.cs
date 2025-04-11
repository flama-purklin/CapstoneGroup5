using UnityEngine;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
// Duplicate using directives removed below
using Newtonsoft.Json;
using System;
// using CoreControl.MysteryParsing; // Removed as MysteryCharacterExtractor is no longer used

/// <summary>
/// Parses the mystery JSON into usable game objects.
/// </summary>
public class ParsingControl : MonoBehaviour
{
    [Header("Configuration")]
    public string mysteryFiles = "MysteryStorage";
    [SerializeField] private bool _verboseLogging = false;

    // Removed _characterExtractor reference

    // Events
    public event Action<float> OnParsingProgress;
    public event Action<Mystery> OnMysteryParsed;
    // public event Action<int> OnCharactersExtracted; // Removed unused event
    // public event Action OnParsingComplete; // Removed event

    // Status
    private bool _parsingComplete = false;
    public bool IsParsingComplete => _parsingComplete;
    private void Awake()
    {
        // Parse mystery
        ParseMystery();
    }

    // Removed OnDestroy method

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
                // OnParsingComplete?.Invoke(); // Removed event invocation
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
            
            // Report progress after validation and fire completion event
            OnParsingProgress?.Invoke(1.0f); // Parsing itself is now the whole process

            // Signal that parsing is complete
            _parsingComplete = true;
            Debug.Log("Parsing complete - setting IsParsingComplete flag");
            // OnParsingComplete?.Invoke(); // Removed event invocation

            // Removed character extraction logic
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error parsing mystery JSON: {ex.Message}");
            Debug.LogException(ex);
            
            // Signal completion even if parsing failed
            _parsingComplete = true;
            OnParsingProgress?.Invoke(1.0f);
            // OnParsingComplete?.Invoke(); // Removed event invocation
        }
    } // Added missing closing brace for ParseMystery method

    // Removed ParseMysteryAsync method
    // Removed HandleExtractionProgress method
    // Removed HandleCharactersExtracted method
    // Removed ExtractionTimeoutCheck coroutine

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
            // OnParsingComplete?.Invoke(); // Removed event invocation
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
            
            // Report progress after validation and fire completion event
            OnParsingProgress?.Invoke(1.0f); // Parsing itself is now the whole process

            // Signal that parsing is complete
            _parsingComplete = true;
            Debug.Log("Delayed parsing complete - setting IsParsingComplete flag");
            // OnParsingComplete?.Invoke(); // Removed event invocation

            // Removed character extraction logic
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in delayed parsing: {ex.Message}");
            Debug.LogException(ex);
            
            // Signal completion even if parsing failed
            _parsingComplete = true;
            OnParsingProgress?.Invoke(1.0f);
            // OnParsingComplete?.Invoke(); // Removed event invocation
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
