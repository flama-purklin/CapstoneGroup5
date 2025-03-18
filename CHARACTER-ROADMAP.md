# Character & LLM System Implementation Roadmap

## Overview

This roadmap outlines the steps needed to transition from static character JSON files to a dynamic system that extracts character data from the unified mystery JSON file. The implementation will incorporate character state management, train car proximity loading, and loading screen UI integration, while properly utilizing the existing mystery parsing system.

## Current System
- Mystery data is parsed from JSON in `ParsingControl.cs` and stored in `GameControl.GameController.coreMystery`
- Character data within the mystery is defined in the `MysteryCharacter` class structure
- MysteryCharacter has a different structure from the standalone character JSON files
- Character data for LLM is currently defined in static JSON files in `StreamingAssets/Characters/`
- `CharacterManager.cs` loads these static files during initialization
- `CharacterPromptGenerator.cs` parses standalone JSON into LLM prompts
- No dynamic generation of character files from mystery data
- No mechanism to transform `MysteryCharacter` data to the LLM character format
- No proximity-based character loading/unloading
- No integration with the mystery node discovery system

## Target Implementation
1. ✅ Parse character definitions from unified mystery JSON using the existing `MysteryCharacter` class
2. ✅ Extract character JSON directly from transformed-mystery.json to preserve format
3. ✅ Update CharacterPromptGenerator to work with extracted JSON format
4. 🔄 Implement character state transitions (Active/Suspended/Archived) with serializable memory (Phase 2)
5. 🔄 Integrate with train car proximity system using `MysteryEnvironment` data (Phase 2)
6. ✅ Update loading screen UI with accurate progress tracking
7. 🔄 Add visual indicators and UX improvements for state transitions (Phase 2)

## Detailed Implementation Plan

### Phase 1: Dynamic Character File Generation

#### Class Architecture

**1. MysteryCharacterExtractor**
```csharp
public class MysteryCharacterExtractor : MonoBehaviour
{
    // Configuration
    [SerializeField] private string charactersOutputFolder = "Characters";
    [SerializeField] private bool clearExistingCharacterFiles = true;
    
    // Events
    public event Action<float> OnExtractionProgress;
    public event Action<int> OnCharactersExtracted;
    
    // Public Methods
    public void ExtractCharactersFromMystery(Mystery mystery);
    public async Task<int> ExtractCharactersAsync(Mystery mystery);
    
    // Private Methods
    private string ConvertToLLMCharacterFormat(string characterId, MysteryCharacter character);
    private string MapMindEngine(MysteryCharacter character);
    private string MapCoreData(MysteryCharacter character);
    private string MapCaseInfo(MysteryCharacter character);
    private void ClearExistingCharacterFiles();
    private void LogExtractionResults(int count);
}
```

**2. ParsingControl (Modified)**
```csharp
public class ParsingControl : MonoBehaviour
{
    // Existing fields
    public string mysteryFiles = "MysteryStorage";
    
    // New fields
    [SerializeField] private MysteryCharacterExtractor characterExtractor;
    
    // Events
    public event Action<float> OnParsingProgress;
    public event Action<Mystery> OnMysteryParsed;
    public event Action<int> OnCharactersExtracted;
    
    // Modified Methods
    public async Task<Mystery> ParseMysteryAsync();
    public void ParseMystery(); // Updated to call ExtractCharacters
    
    // New Methods
    private void ExtractCharacters(Mystery mystery);
    private void OnExtractionProgress(float progress);
}
```

**3. CharacterPromptGenerator (Modified)**
```csharp
public static class CharacterPromptGenerator
{
    // New mapping methods
    private static string MapMysteryCharacterToPrompt(MysteryCharacter character);
    private static string MapWhereaboutsToMemory(List<Whereabouts> whereabouts);
    private static string MapRelationshipsToPrompt(List<Relationship> relationships);
    private static string MapPersonalityToPrompt(Personality personality);
    
    // Modified method
    public static string GenerateSystemPrompt(string jsonContent, LLMCharacter characterObj);
}
```

#### Implementation Details

1. **MysteryCharacterExtractor.cs**: ✅ COMPLETED
   - Direct extraction approach:
     - Load the transformed-mystery.json file
     - For each character, extract their JSON directly from the source file
     - Save each character's JSON exactly as-is to an individual file
     - Maintain the exact two-chamber structure (core and mind_engine)
   - File management:
     - Back up existing character files to CharacterBackups folder
     - Create Characters folder if it doesn't exist
     - Clear existing character files before extraction (optional)
     - Save each character as individual JSON in StreamingAssets/Characters
   - Progress reporting:
     - Emit progress events for UI integration during extraction
     - Detailed logging for monitoring 
     - Support for both synchronous and asynchronous extraction
   - Thread safety:
     - Implemented UnityMainThreadDispatcher for async operations
     - Added proper locking for concurrent resource access
     - Used semaphore for controlling concurrent operations

2. **ParsingControl.cs**: ✅ COMPLETED
   - Added reference to `MysteryCharacterExtractor`
   - Call `ExtractCharactersFromMystery()` after deserializing mystery JSON
   - Added OnParsingComplete event and IsParsingComplete property
   - Forward progress events to UI controller
   - Support both synchronous and asynchronous extraction
   - Added detailed logging for monitoring extraction process

3. **CharacterPromptGenerator.cs**: ✅ COMPLETED
   - Completely updated to handle both character format types:
     - Original structured format with explicit sections
     - Mystery format extracted directly from transformed-mystery.json
   - Added format detection to determine which parser to use
   - Created GeneratePromptFromMysteryFormat method to handle the new format
   - Preserved special speech patterns and character traits
   - Added detailed field mapping for both formats
   - Created thorough prompts that leverage all available character data

#### Testing Plan

1. **Simplified Testing Approach** ✅:
   - Run the game with the implemented system 
   - Monitor extraction progress in the loading screen
   - Check character files are created in `StreamingAssets/Characters/`
   - Verify the generated files maintain the exact structure from the mystery JSON
   - Test dialogue with the generated characters to verify correct functionality

2. **Manual Testing Steps** ✅:
   - Build and run the game in editor using the SystemsTest scene
   - Monitor extraction logs during the loading screen
   - Verify character files are created in `StreamingAssets/Characters/`
   - Pay special attention to Nova's file to ensure speech patterns are preserved
   - Test dialogue with characters to verify they function correctly
   - Verify the loading screen progress is accurate during extraction

3. **Verification Process** ✅:
   - Check the extracted files have the correct two-chamber structure (core and mind_engine)
   - Verify file contents match exactly what's in the original transformed-mystery.json
   - Ensure special characters like Nova maintain their distinctive speech patterns 
   - Confirm extraction progress is correctly reported to the loading UI
   - Test the game with the extracted characters to verify dialogue works properly
   
4. **Test Tools Implemented** ✅:
   - Added MysteryCharacterExtractor editor context menu options:
     - `Test Extraction` - Tests synchronous extraction 
     - `Test Async Extraction` - Tests asynchronous extraction
     - `Test Nova Extraction` - Tests Nova-specific extraction
     - `Verify Nova Speech Patterns` - Verifies Nova's distinctive speech
   - Added PromptGeneratorTests component:
     - `Test Character Prompt Generation` - Tests single character prompt
     - `Test Nova's Speech Patterns` - Tests Nova's speech patterns in prompt
     - `Test All Character Prompts` - Tests all character prompts
   - Added detailed logging and progress tracking
   - Implemented validation in GameInitializer.VerifyCharacterFiles
   - Added StreamingAssets/Prompts directory for saving and inspecting prompts

### Phase 2: Character Warmup System Implementation

#### Class Architecture

**1. CharacterMemorySnapshot**
```csharp
[System.Serializable]
public class CharacterMemorySnapshot
{
    // Identification
    public string characterId;
    public string characterName;
    
    // State
    public List<ChatMessage> chatHistory;
    public Dictionary<string, object> contextualMemory;
    public int contextWindowUsage;
    
    // Metadata
    public float lastInteractionTime;
    public int trainCarNumber;
    public float importanceRating;
    public bool isInActiveScene;
    
    // Methods
    public void CaptureFrom(LLMCharacter character);
    public void ApplyTo(LLMCharacter character);
    public string Serialize();
    public static CharacterMemorySnapshot Deserialize(string json);
}
```

**2. CharacterStateManager**
```csharp
public class CharacterStateManager : MonoBehaviour
{
    // Configuration
    [SerializeField] private string snapshotStoragePath = "CharacterSnapshots";
    [SerializeField] private int maxActiveCharacters = 5;
    [SerializeField] private float suspendTimeoutMinutes = 10;
    
    // State Tracking
    private Dictionary<string, CharacterMemorySnapshot> memorySnapshots;
    private Dictionary<string, CharacterMemoryState> characterStates;
    
    // Lifecycle Management
    public CharacterMemoryState GetCharacterState(string characterId);
    public bool TryGetSnapshot(string characterId, out CharacterMemorySnapshot snapshot);
    public void CaptureCharacterState(LLMCharacter character);
    public void SuspendCharacter(string characterId);
    public void ArchiveCharacter(string characterId);
    public async Task<bool> RestoreCharacter(string characterId, CharacterMemoryState targetState);
    
    // File Management
    private void SaveSnapshotToDisk(CharacterMemorySnapshot snapshot);
    private CharacterMemorySnapshot LoadSnapshotFromDisk(string characterId);
    private void CleanupOldSnapshots();
}
```

**3. CharacterManager (Modified)**
```csharp
public class CharacterManager : MonoBehaviour
{
    // New enum values
    public enum CharacterState
    {
        Uninitialized,
        LoadingTemplate,
        WarmingUp,
        Ready,
        Suspended,   // New state
        Archived,    // New state
        Failed
    }
    
    // New fields
    [SerializeField] private CharacterStateManager stateManager;
    [SerializeField] private int maxActiveCharacters = 5;
    
    // Priority configuration
    [SerializeField] private List<string> highPriorityCharacters;
    
    // New methods
    public async Task SuspendCharacter(string characterId);
    public async Task ArchiveCharacter(string characterId);
    public async Task<LLMCharacter> RestoreCharacter(string characterId, CharacterState targetState);
    public void PrioritizeCharacters(string[] characterIds);
    public Dictionary<string, CharacterState> GetAllCharacterStates();
    
    // Modified methods
    private IEnumerator TwoPhaseInitialization(); // Modified to use dynamic files
    private IEnumerator CreateCharacterObjects(); // Modified to handle priority
    
    // Memory management
    private void MonitorMemoryUsage(); // Uncomment
    private void BalanceActiveCharacters();
}
```

#### Implementation Details

1. **CharacterMemorySnapshot.cs**:
   - Implement serializable container for character state
   - Store chat history, context, and memory values
   - Add methods to capture state from `LLMCharacter`
   - Implement serialization/deserialization to/from JSON
   - Include metadata for character prioritization

2. **CharacterStateManager.cs**:
   - Implement state tracking for all characters
   - Manage file storage for archived character states
   - Handle state transitions between Active/Suspended/Archived
   - Provide cleanup methods for old snapshots
   - Calculate prioritization based on importance and recency

3. **CharacterManager.cs**:
   - Extend state machine with new states
   - Implement character prioritization
   - Add methods to suspend, archive, and restore characters
   - Manage optimal memory allocation
   - Add monitoring for memory pressure

#### Testing Plan

1. **Unit Tests**:
   - Test serialization/deserialization of `CharacterMemorySnapshot`
   - Verify state transitions follow correct rules
   - Test priority calculations with various inputs

2. **Integration Tests**:
   - Create test scene with multiple characters
   - Test suspend/archive/restore operations
   - Verify chat history survives state transitions
   - Measure memory usage during transitions

3. **Manual Testing Steps**:
   - Build and run the game with reduced character count
   - Use debug commands to trigger state transitions
   - Verify character state indicators reflect changes
   - Test dialogue before and after state transitions
   - Monitor memory usage during extended play sessions

4. **Debugging Tools**:
   - Add debug UI panel showing all character states
   - Create console commands to force transitions
   - Add memory usage graph during runtime
   - Implement detailed logging of transition events
   - Create visual indicators of character state in scene

### Phase 3: Train Car Proximity Integration

#### Class Architecture

**1. TrainCarProximityManager**
```csharp
public class TrainCarProximityManager : MonoBehaviour
{
    // Configuration
    [SerializeField] private int activeZoneSize = 3;
    [SerializeField] private int warmingZoneSize = 5;
    [SerializeField] private float zoneChangeCooldown = 5f;
    
    // References
    [SerializeField] private CharacterManager characterManager;
    
    // State
    private int currentCarIndex = 0;
    private List<TrainCar> trainCars = new List<TrainCar>();
    private Dictionary<int, List<string>> carCharacterMap = new Dictionary<int, List<string>>();
    private float lastZoneChangeTime = 0f;
    
    // Public Methods
    public void RegisterCar(TrainCar car, List<string> characterIds);
    public void UpdatePlayerPosition(int carIndex);
    public List<string> GetActiveZoneCharacters();
    public List<string> GetWarmingZoneCharacters();
    
    // Private Methods
    private void UpdateActiveZone(int newCarIndex);
    private void ActivateCarCharacters(TrainCar car);
    private void SuspendCarCharacters(TrainCar car);
    private void ArchiveCarCharacters(TrainCar car);
    private bool IsInActiveZone(int carIndex);
    private bool IsInWarmingZone(int carIndex);
}
```

**2. TrainCar (Modified)**
```csharp
public class TrainCar : MonoBehaviour
{
    // New fields
    [SerializeField] private List<string> assignedCharacterIds;
    [SerializeField] private int carNumber;
    public bool isActive { get; private set; }
    
    // New methods
    public void RegisterWithProximityManager(TrainCarProximityManager manager);
    public void SetActive(bool active);
    public List<string> GetAssignedCharacters();
}
```

**3. CarDetection (Modified)**
```csharp
public class CarDetection : MonoBehaviour
{
    // New fields
    [SerializeField] private TrainCarProximityManager proximityManager;
    private int currentCarIndex = -1;
    
    // Modified methods
    private void OnTriggerEnter(Collider other); // Update to call proximityManager
    
    // New methods
    private void NotifyCarChange(int newCarIndex);
    public int GetCurrentCarIndex();
}
```

**4. NPCManager (Modified)**
```csharp
public class NPCManager : MonoBehaviour
{
    // New fields
    [SerializeField] private TrainCarProximityManager proximityManager;
    [SerializeField] private CharacterManager characterManager;
    private Dictionary<string, GameObject> activeNPCs = new Dictionary<string, GameObject>();
    
    // Modified methods
    public void SpawnNPC(string characterId, Vector3 position); // Handle activation
    
    // New methods
    public void DeactivateNPC(string characterId);
    public void SynchronizeWithActiveZone();
    public void HandleCharacterStateChange(string characterId, CharacterManager.CharacterState newState);
}
```

#### Implementation Details

1. **TrainCarProximityManager.cs**:
   - Implement active zone calculation based on player position
   - Track character assignments for each train car
   - Manage character activation/deactivation based on proximity
   - Implement hysteresis to prevent rapid state flapping
   - Calculate prioritization based on distance and story importance

2. **TrainCar.cs**:
   - Add character assignments to each car
   - Implement registration with proximity manager
   - Add methods to activate/deactivate car and associated characters
   - Store car position and meta information

3. **CarDetection.cs**:
   - Modify to track current car index
   - Add notification to proximity manager when car changes
   - Implement debouncing for rapid transitions

4. **NPCManager.cs**:
   - Add integration with proximity system
   - Handle NPC activation/deactivation based on character state
   - Implement synchronization with active zone
   - Cache character state for visual indicators

#### Testing Plan

1. **Unit Tests**:
   - Test active zone calculations with different inputs
   - Verify character prioritization algorithm
   - Test hysteresis implementation with rapid transitions

2. **Integration Tests**:
   - Create test scene with multiple train cars
   - Test player movement through cars
   - Verify character activation/deactivation
   - Measure performance with different active zone sizes

3. **Manual Testing Steps**:
   - Build and run the game with proximity system enabled
   - Walk through multiple train cars and observe character states
   - Test rapid movement between cars for stability
   - Verify visual indicators match character states
   - Check memory usage during traversal

4. **Debugging Tools**:
   - Create debug overlay showing active/warming zones
   - Add visualization of car boundaries
   - Implement character state indicators above NPCs
   - Create debug console to control proximity parameters
   - Add performance monitoring during transitions

### Phase 4: Loading UI Integration

#### Class Architecture

**1. CharacterLoadingProgress**
```csharp
public class CharacterLoadingProgress : MonoBehaviour
{
    // Events
    public event Action<float> OnOverallProgressChanged;
    public event Action<string> OnStatusMessageChanged;
    public event Action<bool> OnLoadingComplete;
    
    // Progress tracking
    private float extractionProgress = 0f;
    private float generationProgress = 0f;
    private float loadingProgress = 0f;
    private float warmupProgress = 0f;
    
    // Weights
    [SerializeField] private float extractionWeight = 0.2f;
    [SerializeField] private float generationWeight = 0.2f;
    [SerializeField] private float loadingWeight = 0.3f;
    [SerializeField] private float warmupWeight = 0.3f;
    
    // Public Methods
    public void UpdateExtractionProgress(float progress);
    public void UpdateGenerationProgress(float progress);
    public void UpdateLoadingProgress(float progress);
    public void UpdateWarmupProgress(float progress);
    public float GetOverallProgress();
    
    // Private Methods
    private void CalculateOverallProgress();
    private string GetStatusMessage();
}
```

**2. LoadingUI (Modified)**
```csharp
public class LoadingUI : MonoBehaviour
{
    // New fields
    [SerializeField] private CharacterLoadingProgress loadingProgress;
    [SerializeField] private MysteryCharacterExtractor characterExtractor;
    [SerializeField] private ParsingControl parsingControl;
    
    // Modified methods
    private float CalculateTargetProgress(); // Update with new phases
    private void UpdateStatusText(); // Add new status messages
    
    // New methods
    private void RegisterProgressEvents();
    private void OnCharacterExtractionProgress(float progress);
    private void OnCharacterGenerationProgress(float progress);
    private void OnCharacterLoadingProgress(float progress);
}
```

**3. GameInitializer (Modified)**
```csharp
public class GameInitializer : MonoBehaviour
{
    // New fields
    [SerializeField] private CharacterLoadingProgress loadingProgress;
    
    // Modified methods
    private IEnumerator InitializeGame(); // Coordinate loading sequence
    
    // New methods
    private IEnumerator InitializeMystery();
    private IEnumerator InitializeCharacters();
    private void OnLoadingComplete();
}
```

#### Implementation Details

1. **CharacterLoadingProgress.cs**:
   - Create centralized progress tracking for all phases
   - Implement weighted progress calculation
   - Generate appropriate status messages
   - Provide events for UI updates
   - Handle edge cases and error states

2. **LoadingUI.cs**:
   - Integrate with `CharacterLoadingProgress`
   - Update UI based on progress events
   - Add new status messages for character phases
   - Improve spinner and progress bar visualization
   - Create smooth transitions between phases

3. **GameInitializer.cs**:
   - Coordinate loading sequence across systems
   - Ensure proper initialization order
   - Handle scene transitions based on loading state
   - Implement retry logic for failed operations
   - Add timeout handling for stuck operations

#### Testing Plan

1. **Unit Tests**:
   - Test progress calculations with various inputs
   - Verify status message generation
   - Test event propagation

2. **Integration Tests**:
   - Create test scene with complete loading sequence
   - Test progress reporting from all components
   - Verify smooth visualization during all phases
   - Test with artificially slowed operations

3. **Manual Testing Steps**:
   - Build and run the game with loading screen
   - Observe progress bar behavior during startup
   - Verify status messages are clear and accurate
   - Test with different character counts
   - Look for visual glitches or jumps in progress

4. **Debugging Tools**:
   - Add detailed progress logging
   - Create debug overlay with phase breakdown
   - Implement artificial loading delay controls
   - Add fast-forward option for testing
   - Create breakdown of time spent in each phase

### Phase 5: Visual Indicators & User Experience

#### Class Architecture

**1. CharacterStatusIndicator**
```csharp
public class CharacterStatusIndicator : MonoBehaviour
{
    // Visual references
    [SerializeField] private GameObject readyIndicator;
    [SerializeField] private GameObject loadingIndicator;
    [SerializeField] private GameObject unavailableIndicator;
    [SerializeField] private TMPro.TMP_Text statusText;
    
    // Animation
    [SerializeField] private Animator statusAnimator;
    
    // Configuration
    [SerializeField] private float updateInterval = 0.5f;
    
    // State
    private CharacterManager.CharacterState currentState;
    private string characterId;
    
    // Public Methods
    public void Initialize(string characterId);
    public void UpdateStatus(CharacterManager.CharacterState state);
    public void ShowLoadingProgress(float progress);
    
    // Private Methods
    private void UpdateVisuals();
    private string GetStatusDescription(CharacterManager.CharacterState state);
}
```

**2. DialogueControl (Modified)**
```csharp
public class DialogueControl : MonoBehaviour
{
    // New fields
    [SerializeField] private GameObject loadingDialoguePanel;
    [SerializeField] private TMPro.TMP_Text loadingStatusText;
    
    // Modified methods
    public void InitializeDialogue(Character character); // Handle loading state
    
    // New methods
    private void ShowLoadingDialogue(string characterId);
    private void OnCharacterStateChanged(string characterId, CharacterManager.CharacterState state);
    private void HandleRestoredCharacter(string characterId);
    private IEnumerator RestoreDialogueContext();
}
```

**3. CharacterMemoryTrigger**
```csharp
public class CharacterMemoryTrigger : MonoBehaviour
{
    // Configuration
    [SerializeField] private float recallThreshold = 30f * 60f; // 30 minutes
    [SerializeField] private List<string> recallPhrases;
    
    // References
    [SerializeField] private DialogueControl dialogueControl;
    
    // State
    private Dictionary<string, System.DateTime> lastInteractionTimes = new Dictionary<string, System.DateTime>();
    
    // Public Methods
    public void RecordInteraction(string characterId);
    public bool ShouldRecallPreviousInteraction(string characterId);
    public string GetRecallPhrase(string characterId);
    
    // Private Methods
    private float GetTimeSinceLastInteraction(string characterId);
    private string FormatRecallPhrase(string basePhrase, float timeSinceLastInteraction);
}
```

#### Implementation Details

1. **CharacterStatusIndicator.cs**:
   - Create visual indicator component for NPC state
   - Implement different status visuals (Ready/Loading/Unavailable)
   - Add animation for state transitions
   - Display loading progress when appropriate
   - Show estimated time for loading completion

2. **DialogueControl.cs**:
   - Add loading dialogue panel for transitional UI
   - Implement smooth transition when character is loading
   - Add natural dialogue for waiting states
   - Handle context restoration when character is ready
   - Add memory triggers for returning to previous conversations

3. **CharacterMemoryTrigger.cs**:
   - Track interaction history with characters
   - Generate appropriate recall phrases based on time
   - Trigger memory references in dialogue
   - Coordinate with character state transitions
   - Handle dialogue continuity across sessions

#### Testing Plan

1. **Unit Tests**:
   - Test recall phrase generation
   - Verify status indicator logic
   - Test transition animations

2. **Integration Tests**:
   - Create test scene with character state changes
   - Test dialogue transitions during state changes
   - Verify memory recall functionality
   - Test with various dialogue scenarios

3. **Manual Testing Steps**:
   - Build and run the game with visual indicators
   - Interact with characters in different states
   - Force character state transitions during dialogue
   - Test memory recall with time manipulation
   - Verify user experience is smooth during transitions

4. **Debugging Tools**:
   - Add character state inspector window
   - Create debug controls for dialogue system
   - Implement time manipulation for testing recall
   - Add visualization of dialogue history
   - Create stress test for multiple concurrent transitions

## Integration with Existing Structure

### Scene Setup & Changes

1. **LoadingScreen Scene:**
   - Add `MysteryCharacterExtractor` component to same object as `ParsingControl`
   - Add `CharacterLoadingProgress` component to track all phases
   - Connect progress events to `LoadingUI`
   - Add debug panel for detailed loading information (dev builds only)

2. **GAME Scene:**
   - Add `TrainCarProximityManager` component to train manager
   - Add `CharacterStateManager` to character manager object
   - Configure each train car with character assignments
   - Add debug overlay for proximity visualization (dev builds only)

3. **Prefabs & Components:**
   - Add `CharacterStatusIndicator` component to NPC prefab
   - Update `DialogueControl` prefab with loading transitions
   - Add visual effects for character state changes
   - Create transition animations for dialogue UI

### Code Integration Points

1. **Mystery Parsing → Character Extraction:**
   ```csharp
   // In ParsingControl.cs
   public void ParseMystery()
   {
       // retrieve the mystery json from Streaming Assets
       string mysteryPath = Path.Combine(Application.streamingAssetsPath, mysteryFiles);
       if (!Directory.Exists(mysteryPath))
       {
           Debug.LogError($"Mystery folder not found at: {mysteryPath}");
           return;
       }
       
       // expand this later into a full mystery selection area
       var foundMysteries = Directory.GetFiles(mysteryPath, "*.json")
           .ToArray();

       string firstMystery = foundMysteries[0];
       
       // read json to a parsable string
       string jsonContent = File.ReadAllText(firstMystery);
       
       // create a core mystery object with all information stored within
       GameControl.GameController.coreMystery = JsonConvert.DeserializeObject<Mystery>(jsonContent);
       GameControl.GameController.coreConstellation = GameControl.GameController.coreMystery.Constellation;
       
       // Extract characters after parsing
       if (characterExtractor != null)
       {
           // Pass the characters dictionary from the parsed mystery
           characterExtractor.ExtractCharactersFromMystery(GameControl.GameController.coreMystery);
           // Report extraction progress
           OnParsingProgress?.Invoke(0.8f); // 80% complete after parsing, before character extraction
       }
   }
   ```

2. **Mystery Character → LLM Character Format:**
   ```csharp
   // In MysteryCharacterExtractor.cs
   private string ConvertToLLMCharacterFormat(string characterId, MysteryCharacter character)
   {
       // Create the base structure matching standalone character JSON format
       var llmCharacter = new Dictionary<string, object>();
       
       // Map core data (demographics, personality, etc.)
       llmCharacter["core"] = new Dictionary<string, object>
       {
           ["archetype"] = new Dictionary<string, object>
           {
               ["type"] = character.Core.Involvement.Type,
               ["role"] = character.Core.Involvement.Role,
               ["mystery_attributes"] = character.Core.Involvement.MysteryAttributes
           },
           ["demographics"] = new Dictionary<string, object>
           {
               ["name"] = character.MindEngine.Identity.Name,
               ["occupation"] = character.MindEngine.Identity.Occupation
               // Map other demographic fields
           },
           // Map personality traits from OCEAN model
           ["personality"] = MapPersonalityFromOCEAN(character.MindEngine.Identity.Personality)
       };
       
       // Map mind_engine data
       llmCharacter["mind_engine"] = new Dictionary<string, object>
       {
           ["drive"] = new Dictionary<string, object>
           {
               ["primary_goal"] = character.Core.Agenda.PrimaryGoal,
               // Map other drive fields
           },
           ["current_state"] = new Dictionary<string, object>
           {
               ["worries"] = character.MindEngine.StateOfMind.Worries,
               ["feelings"] = character.MindEngine.StateOfMind.Feelings
           }
       };
       
       // Map case_info data (memory, relationships)
       llmCharacter["case_info"] = MapCaseInfoFromWhereabouts(character);
       
       return JsonConvert.SerializeObject(llmCharacter, Formatting.Indented);
   }
   ```

3. **Proximity Manager → MysteryEnvironment Integration:**
   ```csharp
   // In TrainCarProximityManager.cs
   
   // Initialize by mapping environment from mystery
   public void InitializeFromMystery(MysteryEnvironment environment)
   {
       if (environment == null || environment.Cars == null)
       {
           Debug.LogError("Invalid mystery environment data");
           return;
       }
       
       // Map train cars from mystery environment
       foreach (var carEntry in environment.Cars)
       {
           string carId = carEntry.Key;
           TrainCar carData = carEntry.Value;
           
           // Find the Unity TrainCar component matching this ID
           var carComponent = trainCars.Find(c => c.CarId == carId);
           if (carComponent != null)
           {
               // Find characters assigned to this car based on whereabouts
               var charactersInCar = FindCharactersInCar(carId);
               carCharacterMap[carId] = charactersInCar;
               
               // Register the car with its characters
               RegisterCar(carComponent, charactersInCar);
           }
       }
   }
   
   // Find characters whose current whereabouts match this car
   private List<string> FindCharactersInCar(string carId)
   {
       var charactersInCar = new List<string>();
       var mystery = GameControl.GameController.coreMystery;
       
       foreach (var characterEntry in mystery.Characters)
       {
           string characterId = characterEntry.Key;
           MysteryCharacter character = characterEntry.Value;
           
           // Get the most recent whereabout for this character
           var mostRecentWhereabout = character.Core.Whereabouts
               .OrderByDescending(w => w.Key)
               .FirstOrDefault();
               
           if (mostRecentWhereabout != null && 
               mostRecentWhereabout.WhereaboutData.Location != null &&
               mostRecentWhereabout.WhereaboutData.Location.Contains(carId))
           {
               charactersInCar.Add(characterId);
           }
       }
       
       return charactersInCar;
   }
   ```

4. **Mystery Node Discovery → Character State:**
   ```csharp
   // In MysteryConstellation.cs - Enhancement to existing DiscoverNode method
   public MysteryNode DiscoverNode(string nodeKey)
   {
       // Existing node discovery code...
       
       // Check if this node mentions characters
       MysteryNode discoveredNode = Nodes[nodeKey];
       if (discoveredNode.Characters != null && discoveredNode.Characters.Count > 0)
       {
           // Find CharacterManager to notify relevant characters
           var characterManager = GameObject.FindFirstObjectByType<CharacterManager>();
           if (characterManager != null)
           {
               foreach (string characterId in discoveredNode.Characters)
               {
                   // Prioritize this character as they've been mentioned in a discovered node
                   characterManager.PrioritizeCharacters(new string[] { characterId });
                   
                   // If character isn't active, try to restore them
                   if (!characterManager.IsCharacterReady(characterId))
                   {
                       // Queue this character for activation if they're currently suspended
                       var characterStateManager = characterManager.GetComponent<CharacterStateManager>();
                       if (characterStateManager != null)
                       {
                           characterStateManager.QueueCharacterForRestoration(characterId);
                       }
                   }
               }
           }
       }
       
       return discoveredNode;
   }
   ```

5. **Character States → Dialogue System:**
   ```csharp
   // In DialogueControl.cs
   public void InitializeDialogue(Character character)
   {
       // Get current state of this character
       var characterManager = FindFirstObjectByType<CharacterManager>();
       var state = characterManager.GetCharacterState(character.CharacterID);
       
       if (state != CharacterManager.CharacterState.Ready)
       {
           ShowLoadingDialogue(character.CharacterID);
           // Try to restore the character if it's suspended
           if (state == CharacterManager.CharacterState.Suspended)
           {
               StartCoroutine(RestoreCharacterAndResume(character.CharacterID));
           }
           return;
       }
       
       // Also check if this character has nodes in the mystery that could be relevant
       var mystery = GameControl.GameController.coreMystery;
       var relevantNodes = FindNodesForCharacter(character.CharacterID, mystery.Constellation);
       if (relevantNodes.Count > 0)
       {
           // Store relevant node IDs to character's dialogue context
           StoreRelevantNodeIdsToContext(character, relevantNodes);
       }
       
       // Existing initialization...
   }
   ```

## Order of Implementation and Testing Progression

1. **First:** Dynamic Character File Generation
   - Implement basic extraction
   - Test with simple mystery JSON
   - Verify file format compatibility
   - Integrate with loading screen

2. **Second:** Character Warmup System
   - Implement basic state transitions
   - Test memory snapshot serialization
   - Verify state persistence
   - Add memory management

3. **Third:** Loading UI Integration
   - Connect progress tracking
   - Add detailed status reporting
   - Test with simulated delays
   - Verify smooth UI transitions

4. **Fourth:** Train Car Proximity System
   - Implement zone calculation
   - Test with simple movements
   - Add character priority rules
   - Connect to character states

5. **Last:** Visual Indicators & UX
   - Add status indicators
   - Implement loading dialogues
   - Add memory triggers
   - Polish transitions and animations

## Success Criteria & Validation Methods

1. **Character Extraction: ✅ COMPLETED**
   - ✅ All characters in mystery JSON generate valid files
   - ✅ Generated files work with existing `CharacterManager`
   - ✅ Extraction progress is reported accurately

2. **Prompt Generation: ✅ COMPLETED**
   - ✅ CharacterPromptGenerator correctly handles both formats
   - ✅ Nova's distinctive speech patterns are preserved
   - ✅ Prompts make effective use of character data

3. **State Transitions: 🔄 PLANNED (Phase 2)**
   - 🔄 Characters successfully transition between states
   - 🔄 Memory snapshots preserve dialogue history
   - 🔄 Context allocation optimizes memory usage

4. **Proximity System: 🔄 PLANNED (Phase 2)**
   - 🔄 Character states update as player moves between cars
   - 🔄 Hysteresis prevents rapid state changes
   - 🔄 Important characters remain loaded longer

5. **Loading UI: ✅ COMPLETED**
   - ✅ Progress bar accurately reflects all phases
   - ✅ Status messages clearly indicate current operation
   - ✅ UI remains responsive during loading

6. **User Experience: 🔄 PLANNED (Phase 2)**
   - 🔄 Transitions feel natural to the player
   - 🔄 Loading states don't interrupt gameplay flow
   - 🔄 Memory continuity creates cohesive experience

## Final Performance Metrics

- Memory usage limits configurable via editor-exposed parameters
- Loading screen completes in under 45 seconds
- Character state transitions complete in under 3 seconds
- Number of active characters configurable via editor settings
- Frame rate remains above 30fps during transitions

```csharp
// In CharacterStateManager.cs
[SerializeField] private int maxMemoryUsageMB = 2048; // Configurable memory limit
[SerializeField] private int maxActiveCharacters = 5; // Configurable character limit
[SerializeField] private bool enableMemoryMonitoring = true;
[SerializeField] private float memoryCheckInterval = 30f; // Check memory usage every 30 seconds

[Header("Performance Metrics")]
[SerializeField] private bool logPerformanceMetrics = false;
[SerializeField] private float targetFrameRate = 30f;
[SerializeField] private float maxStateTransitionTime = 3f;
```