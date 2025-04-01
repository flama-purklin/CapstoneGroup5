# Current State Analysis - Working Document

## Event & Messaging Systems

### Event Publishers
1. **ParsingControl**
   - `OnParsingProgress` - Reports parsing progress (float)
   - `OnMysteryParsed` - Fired when mystery is parsed (Mystery)
   - `OnCharactersExtracted` - Fired when characters are extracted (int)
   - `OnParsingComplete` - Fired when parsing is complete (void)

2. **MysteryCharacterExtractor**
   - `OnExtractionProgress` - Reports extraction progress (float)
   - `OnCharactersExtracted` - Fired when characters are extracted (int)

3. **CharacterManager**
   - `OnInitializationComplete` - Fired when initialization is complete (void)

### Event Subscribers
1. **GameInitializer**
   - Subscribes to `ParsingControl.OnParsingComplete`
   - Uses events for initialization flow control

2. **ParsingControl**
   - Subscribes to `MysteryCharacterExtractor.OnExtractionProgress`
   - Subscribes to `MysteryCharacterExtractor.OnCharactersExtracted`

### Event Flow Patterns
1. **Initialization Flow**
   ```
   GameInitializer -> ParsingControl.OnParsingComplete -> Character Initialization
   ```

2. **Parsing Flow**
   ```
   ParsingControl -> MysteryCharacterExtractor.OnExtractionProgress -> Update UI
   ParsingControl -> MysteryCharacterExtractor.OnCharactersExtracted -> Finalize
   ```

3. **UI Event Binding**
   - `LLMDialogueManager` uses Unity UI events:
     - `submitButton.onClick.AddListener(OnSubmitClicked)`
     - `inputField.onSubmit.AddListener((text) => {...})`

## Execution Lifecycle & State Transitions

### State Machine Implementations
1. **GameState Enum**
   - Defined in `GameControl.cs`
   - States: DEFAULT, DIALOGUE, PAUSE, FINAL, WIN, LOSE, MINIGAME, MYSTERY
   - Controlled through `GameControl.GameController.currentState`
   - No formal state machine pattern - just direct state assignment

2. **CharacterManager.CharacterState Enum**
   - States: Uninitialized, LoadingTemplate, WarmingUp, Ready, Failed
   - Managed through `CharacterStateTransition` class with validation
   - Proper transitions enforced through `TryTransition` method

### Initialization Sequences
1. **Game Initialization (GameInitializer)**
   - Step 1: Wait for LLM to start
   - Step 2: Mystery parsing and character extraction
   - Step 3: Character Manager initialization
   - Step 4: Load main scene

2. **Character Initialization (CharacterManager)**
   - Phase 1: Create Character Objects and Load Prompts
   - Phase 2: Initialize LLM and Warm Up Characters
   - Uses state transitions: Uninitialized → LoadingTemplate → WarmingUp → Ready

3. **NPC Initialization (NPCManager)**
   - Waits for CharacterManager initialization
   - Caches LLMCharacter instances
   - Creates NPC instances with character data

4. **Scene Loading Pattern**
   - Starts with "LoadingScreen" scene
   - Initializes core systems
   - Transitions to "SystemsTest" main game scene

### Core Game Loop Dependencies
1. **State-Based Behavior Control**
   - NPCMovement uses GameState to control behavior
   - PlayerMovement is disabled when not in DEFAULT state
   - DialogueControl transitions between DEFAULT and DIALOGUE states

2. **LLM Integration**
   - Character LLM processing is asynchronous
   - Dialogue system waits for LLM responses
   - Uses callbacks for streaming text responses

3. **Coroutine-Based State Management**
   - NPCMovement uses coroutines for state transitions (IdleState ↔ MovementState)
   - DialogueControl uses coroutines for UI transitions

## Runtime Data & Asset Relationships

### Mystery Data Structure
1. **Core Data Classes**
   - `Mystery` - Root class containing all mystery data
     - `MysteryMetadata` - Basic info about the mystery
     - `MysteryCore` - Core mystery details
     - `Dictionary<string, MysteryCharacter>` - Character data
     - `MysteryEnvironment` - Environmental details
     - `MysteryConstellation` - Node-based mystery structure

2. **Character Data Structure**
   - `MysteryCharacter` - Character data from mystery JSON
     - `CharacterCore` - Core character details
       - `Whereabouts` - Character location history
       - `Relationships` - Relations with other characters
     - `CharacterMindEngine` - Character personality and dialogue
       - `Identity` - Name, age, occupation, etc.
       - `Personality` - Character traits and behavior

3. **Constellation Structure**
   - `MysteryConstellation` - Graph-like structure of mystery nodes
     - `Dictionary<string, MysteryNode>` - Nodes in the constellation
     - `List<MysteryConnection>` - Connections between nodes
     - `List<MiniMystery>` - Smaller mystery elements

### Data Flow & Persistence
1. **Mystery Parsing**
   - Mystery JSON file loaded from StreamingAssets/MysteryStorage
   - Parsed into Mystery object via Newtonsoft.Json
   - Stored in GameControl singleton

2. **Character Extraction**
   - Characters extracted from Mystery JSON
   - Individual character files created in StreamingAssets/Characters
   - Used by CharacterManager to create LLMCharacter instances

3. **LLM Integration**
   - Character prompts generated from character JSON
   - LLM system manages dialogue through templates
   - Dialogue history maintained in LLMCharacter instances

### Asset References & Instantiation
1. **NPC System**
   - NPCs instantiated from prefab by NPCManager
   - NPCs placed in game world through SpawnNPCInCar method
   - LLMCharacter components attached to NPCs for dialogue

2. **UI System**
   - DialogueUI shown/hidden based on game state
   - LoadingUI displays progress during initialization
   - UI-to-Game communication through exposed methods
