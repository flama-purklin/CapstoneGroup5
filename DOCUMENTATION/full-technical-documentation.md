# Mystery Game Project - Technical Documentation

## Table of Contents
1. [Project Overview](#project-overview)
2. [Core Architecture](#core-architecture)
3. [Scene Structure](#scene-structure)
4. [Game Systems](#game-systems)
   - [Game State Management](#game-state-management)
   - [LLM Integration](#llm-integration)
   - [Mystery System](#mystery-system)
   - [Character System](#character-system)
   - [NPC System](#npc-system)
   - [Dialogue System](#dialogue-system)
   - [Player System](#player-system)
5. [Data Flow](#data-flow)
6. [Initialization Sequence](#initialization-sequence)
7. [Event System](#event-system)
8. [State Machines](#state-machines)
9. [Asset Structure](#asset-structure)
10. [Dependencies](#dependencies)
11. [Code Reference](#code-reference)

## Project Overview

This project is a **Social Deduction / Detective Mystery / Social-sim** game built with Unity. The player controls a customer service robot navigating a train, interacting with NPCs, finding clues, and solving mysteries. It leverages a "black-box" architecture, a core design principle enabling any mystery (represented as a properly structured JSON file) to be loaded into the game engine. This allows for mysteries from diverse sources (developer-created, procedural, user-generated) and varying gameplay scenarios.

A key feature differentiating the game is the integration of Large Language Models (LLM) for character dialogue. This aims to create dynamic, responsive NPC interactions that move beyond traditional dialogue trees, enhancing player agency and the social deduction experience by allowing free-form player input.

## Technical Specifications

### Target Platform
- PC / Windows 10+ (64-bit)

### System Requirement Goals
- **RAM**: 16GB
- **CPU**: 6 cores @ 2.3-2.69 GHz
- **GPU**: NVIDIA GeForce RTX 3060 (12GB)

### Performance Goals
- **Framerate**: 30 fps target
- **Resolution**: 1080p (16:9) primary display

## Core Architecture

The project is built around the following architectural components:

### Singleton Managers
- **GameControl**: Main game state manager
- **CoreSystemsManager**: Manages Unity systems (EventSystem, AudioListener)
- **ParsingControl**: Handles mystery JSON parsing. (Component should exist in the scene from start).
- **NPCManager**: Manages NPC creation and behavior. (Component should exist in the scene from start).
- **CharacterManager**: Manages LLM character creation, state, and interaction. (Component should exist in the scene from start).
- **SimpleProximityWarmup**: Manages LLM character warmup/cooldown based on proximity. (Component should exist in the scene from start).

### Data Model
- **Mystery**: Core data model containing all mystery information
- **MysteryCharacter**: Character data model linking to LLM system
- **MysteryConstellation**: Node-based structure of mystery elements

### Runtime Systems
- **LLM System**: Handles language model interactions
- **Dialogue System**: Manages character conversations
- **NPC System**: Controls NPC behavior and interactions
- **Player System**: Handles player movement and interactions

## Scene Structure

The project utilizes a **unified single-scene architecture**:

1.  **SystemsTest Scene** (Main Gameplay & Initialization)
    - Sole entry point and gameplay environment.
    - Contains the train environment where player and NPCs interact.
    - Includes a **LoadingOverlay** GameObject that manages the entire initialization sequence before revealing the gameplay area.
    - Core Initialization/Gameplay Objects:
        - Main Camera
        - Player
        - GameController
        - InitializationManager (manages startup sequence)
        - LoadingOverlay (displays progress, blocks view initially)
        - LLM (Must exist in scene)
        - `GameController`, `ParsingControl`, `CharacterManager`, `NPCManager`, `TrainLayoutManager`, `SimpleProximityWarmup` GameObjects (with respective scripts attached)
        - Train environment, NPCs, UI elements (PauseMenu, DialogueControl, etc.)

## Game Systems

### Game State Management

The game's state is managed through the `GameControl` class using a `GameState` enum:

```csharp
public enum GameState
{
    DEFAULT,   // Normal gameplay
    DIALOGUE,  // In conversation with NPCs
    PAUSE,     // Game paused
    FINAL,     // End-game state
    WIN,       // Player won
    LOSE,      // Player lost
    MINIGAME,  // Playing a minigame
    MYSTERY    // Investigating the mystery UI
}
```

State changes are managed directly by setting `GameControl.GameController.currentState`.

### LLM Integration

*Design Goal: To move beyond static dialogue trees and create dynamic, responsive NPC interactions that enhance player agency and support social deduction gameplay through free-form input.*

The LLM integration is handled through the LLMUnity package and custom management scripts:

- **LLM**: Core language model interface (LLMUnity component).
- **LLMCharacter**: Character-specific LLM instance (LLMUnity component), handles saving/loading history and cache state.
- **CharacterManager**: Custom script managing `LLMCharacter` instances, states, context allocation, and cache settings.
- **SimpleProximityWarmup**: Custom script managing which characters are "warm" (Ready state) based on player proximity.
- **LLMDialogueManager**: Custom script managing dialogue flow with characters.

The system uses character data parsed from the main mystery JSON to generate system prompts defining personality and knowledge.
Character activity and resource allocation are managed dynamically:
- **Parallel Prompts:** The number of concurrent LLM processes is set by `LLM.parallelPrompts` (must be configured in Inspector).
- **Context Allocation:** `CharacterManager` allocates context (`nKeep`) based on `LLM.parallelPrompts`.
- **Warmup/Cooldown:** `SimpleProximityWarmup` monitors player distance to NPCs and tells `CharacterManager` to `WarmupCharacter` (transition to Ready state) or `CooldownCharacter` (transition back to LoadingTemplate state) to manage active LLM instances up to the `parallelPrompts` limit.
- **Caching:** `CharacterManager` has a public `enableLLMCache` flag (Inspector toggle, default false) that controls whether `LLMCharacter` uses `.cache` files for faster re-warming. *Note: Enabling cache may currently cause `JobTempAlloc` warnings.*

### Mystery System

*Design Goal: The "black-box" JSON approach allows for flexibility in mystery sourcing (hand-crafted, procedural, user-generated) and promotes replayability. The node-based constellation structure encourages active player investigation rather than passive information gathering.*

The mystery system is built around a JSON-based data model:

1. **Mystery Parsing**:
   - `ParsingControl` script component (present in scene from start) loads and parses the main mystery JSON (`transformed-mystery.json`) in its `Awake` method.
   - Mystery data is deserialized and stored directly in `GameControl.GameController.coreMystery`.
   - No longer calls `MysteryCharacterExtractor` or generates individual files.

2. **Mystery Structure**:
   - **Metadata**: Basic mystery information
   - **Core**: Central mystery details (perpetrator, victim, etc.)
   - **Characters**: Character data (personality, relationships, etc.)
   - **Environment**: Setting and environmental details
   - **Constellation**: Node-based mystery structure

3. **Mystery Nodes**:
   - Represent key elements of the mystery
   - Connected through relationships
   - Form a graph-like structure for investigation

### Character System

*Design Goal: Character data is structured to separate core, plot-relevant information (relationships, knowledge, agenda - the CORE section in design) from personality, voice, and decision-making traits used primarily by the LLM for role-playing (the MIND ENGINE section in design).*

*(Refactoring Complete: Initialization order issues and redundant file usage have been addressed. See Initialization Sequence section for the current flow.)*

Characters are managed through multiple components:

1.  **Character Data**:
    *   Data is parsed directly from the main mystery JSON (`transformed-mystery.json`) into the `GameControl.GameController.coreMystery.Characters` dictionary by `ParsingControl` during its `Awake` phase.
    *   Individual character JSON files are no longer created or used.

2.  **Character Manager (`CharacterManager.cs`)**:
    *   Script component exists in the scene from the start.
    *   Initialization is explicitly triggered by `InitializationManager` via `Initialize()` after parsing.
    *   Reads character data from `GameControl.coreMystery.Characters`.
    *   Creates `LLMCharacter` GameObjects/components. Sets `saveCache` based on the public `enableLLMCache` field. Initializes characters only to `LoadingTemplate` state.
    *   Provides `WarmupCharacter()` and `CooldownCharacter()` methods called by `SimpleProximityWarmup` to manage transitions between `LoadingTemplate` and `Ready` states.
    *   Allocates context (`nKeep`) based on `LLM.parallelPrompts`.
    *   Handles `OnDestroy` cleanup, deleting `.json` history files and optionally `.cache` files based on `enableLLMCache`.

3.  **Character Prompt Generation (`CharacterPromptGenerator.cs`)**:
    *   Static class used by `CharacterManager` to generate system prompts for the LLM based on character data.
    *   Takes the `MysteryCharacter` object (retrieved by `CharacterManager` from the `GameControl.coreMystery` dictionary) directly as input to generate the prompt. (Intermediate JSON serialization step removed).
    *   Converts character data properties to structured prompts defining behavior and dialogue patterns.

4.  **Simple Proximity Warmup (`SimpleProximityWarmup.cs`)**:
    *   Monitors player distance to active NPCs.
    *   Determines the closest `maxWarmCharacters` NPCs.
    *   Calls `CharacterManager.WarmupCharacter()` for nearby characters in `LoadingTemplate` state.
    *   Calls `CharacterManager.CooldownCharacter()` for distant characters in `Ready` state.

### NPC System

NPCs are managed through the following components:

1. **NPCManager**:
   - Script component exists in the scene from the start.
   - Creates NPC GameObjects using the `NPC.prefab` template.
   - Connects NPCs to LLMCharacters (obtained via `CharacterManager`).
   - Handles NPC placement in the train environment based on the character's `InitialLocation` field, read directly from the `GameControl.GameController.coreMystery.Characters` dictionary during the `InitializationManager.SpawnAllNPCs` step.
   - Contains `availableAnimContainers` array (must be assigned in Inspector) which holds references to the four `NPCAnimContainer` Scriptable Objects used for visual representation.
   - Assigns animations cyclically to NPCs based on character index.

2. **Character Component**:
   - Links NPC GameObject to LLMCharacter
   - Provides access to character data and behavior

3. **NPCMovement**:
   - Controls NPC movement patterns
   - Manages state transitions (Idle ↔ Movement)
   - Detects player interaction and triggers dialogue
   - Contains safety checks for NavMeshAgent operations

4. **NPCAnimManager**:
   - Handles NPC animations
   - Syncs animations with movement and dialogue states
   - Manages sprite flipping by modifying the child sprite's transform scale (not root)
   - References the Animator component (which must be assigned in Inspector)

5. **NPC Prefab Requirements**:
   - Must have NavMeshAgent, Character, NPCMovement components
   - Should include NPCAnimManager component with references assigned
   - Should include an Animator component with "Apply Root Motion" turned OFF

### Dialogue System

*Design Goal: To facilitate player expression and strategic social interaction. The system aims to simulate the push-and-pull of detective interviews, allowing players to extract information through intelligent conversation and social deduction rather than selecting from predefined options.*

The dialogue system connects player interactions with the LLM system:

1. **DialogueControl**:
   - Manages dialogue UI activation/deactivation.
   - Controls state transitions to/from dialogue.
   - Connects NPCs to the dialogue system.
   - On activation, calls `LLMCharacter.Load()` to load history/cache.
   - On deactivation, calls `LLMCharacter.Save()` to save history/cache and notifies `CharacterManager`.

2. **LLMDialogueManager**:
   - Inherits from BaseDialogueManager.
   - Handles player input and LLM responses.
   - Manages dialogue flow and UI updates.

3. **Dialogue Flow**:
   - Player enters dialogue range and presses E.
   - `DialogueControl.ActivateDialogue()` is called.
   - `LLMCharacter.Load()` is called for the target character.
   - Dialogue UI is activated.
   - Player inputs text.
   - `LLMDialogueManager` sends input to `LLMCharacter.Chat()`.
   - `LLMCharacter` processes input and generates responses.
   - `LLMDialogueManager` displays streaming responses.
   - Player exits dialogue.
   - `DialogueControl.DeactivateDialogue()` is called.
   - `LLMCharacter.Save()` is called for the target character.
   - `CharacterManager.SaveCharacterConversation()` is called (updates snapshot time).
   - History persists only within a single game session; `CharacterManager.OnDestroy()` clears saved files on game stop.

### Player System

The player system handles player movement and interactions:

1. **PlayerMovement**:
   - Controls player character movement
   - Responds to input when in DEFAULT state
   - Movement disabled during dialogue

2. **CarDetection**:
   - Detects which train car the player is in
   - Used for environment-specific logic

3. **JorgePlayer/Player**:
   - Additional player functionality (not fully implemented)

## Core Gameplay Concepts

*Design concepts underpinning the player's interaction with the game world and mystery.*

### Energy System
*Design Goal: To create tension and forward gameplay momentum, replacing earlier concepts of a simple timer. Energy is envisioned to deplete slowly over time and be consumed actively by key player actions like running hunch simulations and potentially engaging in extended dialogue.*

### Hunch System
*Design Goal: To transform investigation into an interactive process. Players actively form theories by connecting clues on the mystery board, testing them via limited simulations. This encourages logical reasoning and rewards attention to detail, leveraging the protagonist's analytical nature.*

### Evidence & Minigames
*Design Goal: Environmental evidence objects and solvable minigames serve as progression gates. Interacting with or solving them unlocks new leads and nodes on the mystery board, making them necessary steps to unravel the case.*

## Data Flow

The game utilizes a centralized "black-box" approach for mystery data. At initialization, `ParsingControl.cs` reads the single `transformed-mystery.json` file. This JSON is deserialized using `Newtonsoft.Json` into a structured C# object instance of the `Mystery` class, which acts as the central blueprint. This `Mystery` object, containing nested specialized classes (e.g., `MysteryCharacter`, `MysteryEnvironment`, `MysteryConstellation`) representing the data "layers", is stored in the `GameControl.GameController.coreMystery` singleton variable. Subsequent systems access this single object instance: `InitializationManager` orchestrates setup, `CharacterManager` reads character data to configure `LLMCharacter` components (using `CharacterPromptGenerator` which now reads object properties directly), `TrainLayoutManager` reads environment data to build the scene, and gameplay systems like Minigames interact with the `MysteryConstellation` (via `GameControl.coreConstellation`) to update the investigation state. This ensures data consistency after the initial parse.

**Current Gaps & Areas for Improvement:**
*   **Unused JSON Data:** Sections like `Mystery.Core` (victim, perpetrator details) and `Mystery.Metadata` appear to be parsed but are not currently used by downstream systems. Fields within `MysteryCharacter` like potential `Appearance` or `Voice` data are also likely unused.
*   **Hardcoded Elements:** Some gameplay elements, such as the placement and specific node linkage of `EvidenceObj` and `LuggageObj` prefabs in the scene, seem to be configured manually in the editor rather than being fully defined and instantiated based on data within the `Mystery.Environment` or `Mystery.Constellation` sections of the JSON. Integrating these definitions into the JSON would enhance the black-box flexibility.

The project's specific data flow follows this pattern:

1. **Mystery Parsing**:
   ```
   Mystery JSON → ParsingControl (scene component) → GameControl.coreMystery
   ```
   *(Individual character file extraction step removed)*

2. **Character Initialization**:
   ```
   GameControl.coreMystery.Characters → CharacterManager (Initialize() called by InitializationManager) → LLMCharacter GameObject & Component (in LoadingTemplate state)
   ```
   *(Reads directly from GameControl data after parsing is confirmed complete)*

3. **NPC Creation**:
   ```
   GameControl.coreMystery.Characters[name].InitialLocation → InitializationManager.SpawnAllNPCs → NPCManager → NPC GameObject
   LLMCharacter (from CharacterManager) → NPCManager → NPC GameObject
   ```
   *(Relies on CharacterManager having initialized correctly; reads location data directly from GameControl)*

4. **Dialogue Flow**:
   ```
   Player Input → DialogueControl → LLMDialogueManager → LLMCharacter → Response
   ```
5. **Warmup/Cooldown Flow**:
   ```
   Player Position → SimpleProximityWarmup → CharacterManager.Warmup/CooldownCharacter → LLMCharacter State Change
   ```
6. **Save/Load Flow**:
   ```
   DialogueControl.Activate → LLMCharacter.Load()
   DialogueControl.Deactivate → LLMCharacter.Save()
   Game Stop → CharacterManager.OnDestroy() → File.Delete()
   ```

## Initialization Sequence

The game initialization occurs entirely within the `SystemsTest` scene, managed by the `InitializationManager` and visually represented by the `LoadingOverlay`. The sequence relies on components existing in the scene from the start.

1.  **Scene Load & `Awake` Phase**:
    - `SystemsTest` scene loads. Core manager components (`ParsingControl`, `CharacterManager`, `NPCManager`, etc.) are already present on their GameObjects.
    - `LoadingOverlay` is active.
    - `GameControl.Awake()` sets up the singleton.
    - `ParsingControl.Awake()` reads the main mystery JSON, populates `GameControl.coreMystery`, and sets its `IsParsingComplete` flag to true.
    - `InitializationManager.Awake()` finds references to the existing manager components.
    - Other components run their `Awake`.

2.  **`Start` Phase & `InitializeGame` Sequence**:
    - `InitializationManager.Start()` calls `InitializeGame()`.
    - **(Step 1) Wait for LLM:** `InitializeGame` awaits `WaitForLLMStartup()`.
    - **(Step 2) Wait for Parsing:** `InitializeGame` awaits `WaitForParsingComplete()`, which now quickly confirms `parsingControl.IsParsingComplete` is true.
    - **(Step 2.1) Trigger Character Init:** `InitializeGame` explicitly calls `characterManager.Initialize()`.
        - `CharacterManager` starts its `TwoPhaseInitialization` coroutine (creating `LLMCharacter` instances, loading templates - characters remain in `LoadingTemplate` state).
    - **(Step 2.5) Build Train:** `InitializeGame` calls `BuildTrain()`. `TrainLayoutManager` builds the layout using data from `GameControl.coreMystery`.
    - **(Step 3) Wait for Character Init:** `InitializeGame` awaits `WaitForCharacterManagerInitialization()`. This waits for `CharacterManager`'s `TwoPhaseInitialization` to complete (checking `IsInitialized` flag and `OnInitializationComplete` event).
    - **(Step 3.5) Init NPC Manager:** `InitializeGame` awaits `InitializeNPCManager()`.
    - **(Step 3.75) Spawn NPCs:** `InitializeGame` calls `SpawnAllNPCs()`. This reads `initial_location` directly from `GameControl.coreMystery` for each character and tells `NPCManager` where to spawn them.
    - **(Step 4) Complete Init:** `InitializeGame` calls `CompleteInitialization()`, hiding the loading overlay and enabling gameplay. (Proximity warmup starts managing character states).

## Event System

The project uses C# events for communication between systems:

1. **Parsing Events**:
   - `ParsingControl.OnParsingProgress`: Reports parsing progress (0.0 to 1.0).
   - `ParsingControl.OnMysteryParsed`: Indicates the main mystery object has been deserialized into `GameControl.coreMystery`.
   - *(Removed `OnCharactersExtracted` and `OnParsingComplete` events)*

2. **Character Manager Events**:
   - `CharacterManager.OnInitializationComplete`: Signals `CharacterManager` has finished its internal setup (creating LLMCharacters and loading templates). Triggered at the end of its `TwoPhaseInitialization` coroutine.

3. **UI Events**:
   - Standard Unity UI events (Button.onClick, InputField.onSubmit)
   - Used for dialogue interaction

## State Machines

The project implements several state machines:

1. **Game State Machine**:
   - Managed through `GameState` enum
   - Transitions controlled by various systems
   - States: DEFAULT, DIALOGUE, PAUSE, FINAL, WIN, LOSE, MINIGAME, MYSTERY

2. **Character State Machine**:
   - Managed through `CharacterManager.CharacterState` enum
   - Uses `CharacterStateTransition` class for validation
   - States: Uninitialized, LoadingTemplate, WarmingUp, Ready, Failed
   - Enforces valid transitions between states (e.g., `LoadingTemplate` -> `WarmingUp`, `Ready` -> `LoadingTemplate`).

3. **NPC Behavior State Machine**:
   - Implemented through coroutines in `NPCMovement`
   - States: IdleState, MovementState, DialogueActivate
   - Transitions based on timers and player interaction

4. **Dialogue State Machine**:
   - Managed through `DialogueControl` and `LLMDialogueManager`
   - States: Inactive, Active, ProcessingResponse, StreamingResponse
   - Transitions based on player input and LLM responses

## Asset Structure

The project's assets are organized as follows:

1. **Scripts**:
   - `Assets/Scripts/`: Main script directory
     - `CoreControl/`: Core game management (`InitializationManager`, `ParsingControl`, etc.)
     - `Characters/`: Character-related logic (`CharacterManager`, `CharacterPromptGenerator`, `SimpleProximityWarmup`)
     - `NPCs/`: NPC behavior (`NPCManager`, `NPCMovement`, `NPCAnimManager`, `DialogueControl`)
     - `Player/`: Player movement and interaction
     - `Train/`: Train environment (`TrainLayoutManager`, `TrainManager`)
     - `UI/`: User interfaces

2. **Mystery**:
   - `Assets/Mystery/`: Mystery system (potentially deprecated/refactored parts)
     - `Myst_Gen/`: Mystery generation
     - `Myst_Play/`: Mystery gameplay
       - `Dialogue/LLM/`: LLM integration (`BaseDialogueManager`, `LLMDialogueManager`)

3. **StreamingAssets**:
   - `StreamingAssets/MysteryStorage/`: Contains the main mystery JSON file (e.g., `transformed-mystery.json`).
   - *(Removed `Characters` and `CharacterBackups` directories)*

4. **LLM**:
   - `Assets/LLMUnity-release-v2.4.2/`: LLM integration package

## Art Direction

*High-level visual goals influencing asset creation and presentation.*

### Camera Setup
- 2/3rd Top-Down perspective.

### Art Style
- Blend of 3D environments and 2D character sprites.
- Requires lighting from the 3D environment to realistically affect 2D assets.

### Aesthetics
- Inspired by Mid-Century Futurism (e.g., Jetsons, Space Era illustrations) and Swinging Sixties styles (e.g., Deathloop, pop art).

## Dependencies

The project has the following key dependencies:

1. **External Libraries**:
   - Newtonsoft.Json: Used for JSON parsing
   - LLMUnity: Integration with language models

2. **Internal Dependencies**:
   - InitializationManager → ParsingControl, CharacterManager, NPCManager, TrainLayoutManager, LLM, GameControl
   - ParsingControl → GameControl
   - CharacterManager → LLM, GameControl, CharacterPromptGenerator
   - NPCManager → CharacterManager, TrainLayoutManager, GameControl (indirectly via InitializationManager for spawning)
   - DialogueControl → LLMDialogueManager, CharacterManager
   - SimpleProximityWarmup → CharacterManager, NPCManager, Player Transform

## Code Reference

### Core Scripts

1. **GameControl.cs**:
   - Manages game state
   - Stores mystery data (`coreMystery`)
   - Singleton access via `GameControl.GameController`

*(Note: PersistentSystemsManager.cs and GameInitializer.cs removed as they are deprecated or superseded by InitializationManager in the unified scene)*

### Mystery System

1. **Mystery.cs**:
   - Core data model for mysteries
   - Contains all mystery information
   - Used by game systems for gameplay

2. **ParsingControl.cs** (`Assets/Scripts/CoreControl/MysteryParsing/`):
   - Component exists in the scene from start.
   - Parses the main mystery JSON file loaded from `StreamingAssets` in its `Awake` method.
   - Populates `GameControl.coreMystery` with the parsed data.
   - Sets the `IsParsingComplete` flag when done.
   - Fires `OnParsingProgress` and `OnMysteryParsed` events.

*(Removed MysteryCharacterExtractor.cs section)*

### Character System

1. **CharacterManager.cs** (`Assets/Scripts/Characters/`):
   - Component exists in the scene from start.
   - Manages the lifecycle and state (`LoadingTemplate`, `WarmingUp`, `Ready`, `Failed`) of `LLMCharacter` instances.
   - Creates `LLMCharacter` components based on data from `GameControl.coreMystery`.
   - Handles context allocation (`nKeep`) based on `LLM.parallelPrompts`.
   - Provides `WarmupCharacter` and `CooldownCharacter` methods for `SimpleProximityWarmup`.
   - Includes `enableLLMCache` public bool to control LLMUnity caching (default `false`).
   - Cleans up saved `.json` (and `.cache` if enabled) files in `OnDestroy`.
   - Fires `OnInitializationComplete` event.

2. **SimpleProximityWarmup.cs** (`Assets/Scripts/Characters/`):
   - Component exists in the scene from start.
   - Periodically checks player distance to NPCs.
   - Calls `CharacterManager.WarmupCharacter` or `CooldownCharacter` to maintain `maxWarmCharacters` in the `Ready` state.

3. **CharacterPromptGenerator.cs** (`Assets/Scripts/Characters/`):
   - Static class used by `CharacterManager` to generate system prompts for the LLM based on character data.
   - Takes `MysteryCharacter` objects directly as input (no intermediate JSON).
   - Accesses properties of the `MysteryCharacter` object model to structure prompts for optimal LLM behavior.
   - (Handles only the current object format, logic for older formats removed).

4. **LLMCharacter.cs** (from LLMUnity):
   - Interfaces with LLM system.
   - Manages character dialogue and responses.
   - Handles prompt and context management.
   - Provides `Save()` and `Load()` methods for history persistence and optional caching (controlled by `saveCache` property, set via `CharacterManager.enableLLMCache`).

### NPC System

1. **NPCManager.cs**:
   - Component exists in the scene from start.
   - Spawns and manages NPCs.
   - Links NPCs to LLM characters (obtained from `CharacterManager`).
   - NPC placement is handled by `InitializationManager.SpawnAllNPCs`, which reads `initial_location` directly from `GameControl.coreMystery`.
   - Assigns NPCAnimContainer visuals to NPCs.

2. **Character.cs**:
   - Links NPC GameObject to LLMCharacter
   - Provides character name and data access
   - Initializes character components

3. **NPCMovement.cs**:
   - Controls NPC movement behavior
   - Manages idle and movement states
   - Detects player interaction for dialogue
   - Ensures NavMeshAgent operations occur only when on NavMesh

4. **NPCAnimManager.cs**:
   - Manages NPC appearance and animations
   - Applies animation state changes based on movement
   - Handles sprite flipping by modifying child sprite scale
   - Prevents NavMeshAgent issues by not modifying root transform scale

5. **NPCAnimContainer.cs**:
   - Scriptable Object that stores animation sprite arrays
   - Defines idle and walk animations for different directions
   - Assigned to NPCs by NPCManager during instantiation

### Train System

*Design Goal: Train layouts utilize a base template for consistent spatial flow across mysteries but allow mystery-specific variations in car types and their order. This supports the flexible "black-box" design, enabling diverse environments defined by the mystery JSON.*

1. **TrainLayoutManager.cs**:
   - Reads train car layout from mystery JSON
   - Prepares car prefabs for instantiation
   - Delegates actual car instantiation to TrainManager
   - Maps car names to their GameObjects via NameCars()
   - Provides methods to find cars and spawn points for NPCs
   - Uses NavMesh sampling for valid NPC spawn positions

2. **TrainManager.cs**:
   - Handles actual instantiation of train cars
   - Manages the physical layout and spacing of cars
   - Maintains a list of instantiated car GameObjects

### Initialization System

1. **InitializationManager.cs**:
   - Orchestrates the game initialization sequence.
   - Finds existing core components (`ParsingControl`, `CharacterManager`, `NPCManager`, etc.) in its `Awake` phase. (Does *not* add components dynamically).
   - Manages LoadingOverlay and initialization state.
   - Coordinates the startup sequence explicitly: Waits for LLM, waits for Parsing (flag), triggers CharacterManager init (template loading only), waits for CharacterManager (event/flag), initializes NPCManager, spawns NPCs (reading data directly), completes initialization (proximity warmup starts).
   - Handles NPC spawning through `SpawnAllNPCs()` method, reading `initial_location` directly from `GameControl.coreMystery`.

### Dialogue System

1. **DialogueControl.cs** (`Assets/Scripts/NPCs/`):
   - Manages dialogue UI and state transitions.
   - Activates dialogue with NPCs.
   - Calls `LLMCharacter.Load()` on activation.
   - Calls `LLMCharacter.Save()` and `CharacterManager.SaveCharacterConversation()` on deactivation.

2. **BaseDialogueManager.cs** (`Assets/Mystery/Myst_Play/Dialogue/LLM/`):
   - Abstract base class for dialogue management.
   - *Note: Does not appear to be attached to any active GameObject in the `SystemsTest` scene.*
   - Handles LLM character interaction.
   - Manages dialogue flow and callbacks.
   - Provides `CurrentCharacter` property.

3. **LLMDialogueManager.cs** (`Assets/Mystery/Myst_Play/Dialogue/LLM/`):
   - Implements concrete dialogue UI interface, inheriting from `BaseDialogueManager`.
   - *Note: Does not appear to be attached to any active GameObject in the `SystemsTest` scene.*
   - Handles player input and response display.
   - Controls dialogue UI state.
