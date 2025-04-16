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

The game's state is managed through the `GameControl` class using a `GameState` Enum:

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
   - **Metadata**: Basic mystery information (title, version, etc.) and overall `context` string for the mystery scenario.
   - **Core**: Central mystery details (perpetrator, victim, etc.)
   - **Character Profiles (`character_profiles`)**: Character data (personality, relationships, etc.). Contains `initial_location` at the top level per character, and `core` object with `involvement` (role, type), `whereabouts` (Dictionary), `relationships` (Dictionary), `agenda`, `appearance`, `voice`, and `revelations` (Dictionary of gated information with triggers). *Note: `key_testimonies` and `mystery_attributes` fields removed.*
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
    *   Data is parsed directly from the main mystery JSON (`transformed-mystery.json`) into the `GameControl.GameController.coreMystery.Characters` dictionary (accessed via the `character_profiles` key in JSON) by `ParsingControl` during its `Awake` phase.
    *   The `MysteryCharacter.cs` model reflects the current structure: `initial_location` is top-level; `core` contains dictionaries for `whereabouts`, `relationships`, and `revelations`. `Appearance` and `Voice` classes/properties exist but are currently unused. `KeyTestimonies` and `mystery_attributes` properties are removed.
    *   Individual character JSON files are no longer created or used.

2.  **Character Manager (`CharacterManager.cs`)**:
    *   Script component exists in the scene from the start.
    *   Initialization is explicitly triggered by `InitializationManager` via `Initialize()` after parsing.
    *   Reads character data from `GameControl.coreMystery.Characters`.
    *   Reads mystery context and title from `GameControl.coreMystery.Metadata`.
    *   Creates `LLMCharacter` GameObjects/components, passing character data, context, and title to `CharacterPromptGenerator` to generate the system prompt.
    *   Sets `saveCache` based on the public `enableLLMCache` field. Initializes characters only to `LoadingTemplate` state.
    *   Provides `WarmupCharacter()` and `CooldownCharacter()` methods called by `SimpleProximityWarmup` to manage transitions between `LoadingTemplate` and `Ready` states.
    *   Allocates context (`nKeep`) based on `LLM.parallelPrompts`.
    *   Handles `OnDestroy` cleanup, deleting `.json` history files and optionally `.cache` files based on `enableLLMCache`.

3.  **Character Prompt Generation (`CharacterPromptGenerator.cs`)**:
    *   Static class used by `CharacterManager` to generate system prompts for the LLM based on character data.
    *   Takes the `MysteryCharacter` object, mystery context, and mystery title as input.
    *   Generates a detailed, markdown-formatted prompt based on the `NEW_PROMPT.md` template.
    *   Includes sections for meta instructions, core identity, personality (with OCEAN descriptions), state of mind, knowledge/secrets/relationships, whereabouts/memories, available actions (function calls), revelation rules, speech patterns, and immutable directives.
    *   Handles formatting of lists (relationships, whereabouts, revelations) and includes helper methods for generating personality descriptions and formatting names.
    *   Logic for `KeyTestimonies` and `mystery_attributes` has been removed.

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
   - Handles NPC placement in the train environment based on the character's top-level `InitialLocation` field, read directly from the `GameControl.GameController.coreMystery.Characters` dictionary during the `InitializationManager.SpawnAndLinkNPCs` step.
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
   - Includes timing diagnostics to track UI animation times and I/O operations.
   - **Improved Responsiveness:** Removed 2-second delay before UI deactivation to improve responsiveness.

2. **BaseDialogueManager**:
   - Abstract class handling core dialogue logic.
   - **Previous Fix:** Changed `currentResponse.Append(reply)` to `currentResponse.Clear(); currentResponse.Append(reply)` in `HandleReply` to prevent text duplication, as LLM response chunks contain the complete response so far, not just new content.
   - **Action Streaming Fix:** Implemented proper handling of multi-chunk function calls using `isAccumulatingAction` flag and `actionBuffer` to accumulate text across multiple LLM response chunks when the action delimiter (`[/ACTION]:` or `\nACTION:`) appears in a chunk but the full function call parameters arrive in subsequent chunks.
   - Supports both `\nACTION:` and `[/ACTION]:` delimiters for function parsing.
   - Provides coroutines `ProcessActionAfterBeepSpeak` and `EnableInputAfterBeepSpeak` that wait for `DialogueControl.IsBeepSpeakPlaying` to become false before acting.
   - **April 2025 Fix:** Enhanced coroutines to dynamically calculate appropriate wait times based on text length (roughly 14 characters/second) before forcing completion, ensuring longer messages have time to animate properly.
   - Includes proper reset of all action state flags in `OnReplyComplete` to ensure clean state between LLM responses.

3. **LLMDialogueManager**:
   - Inherits from BaseDialogueManager.
   - Handles player input and LLM responses.
   - Manages dialogue flow and UI updates.
   - Implements `EnableInput`/`DisableInput` for UI interactability.

4. **BeepSpeak**:
   - Handles text display with typing effect and audio.
   - Manages a typing coroutine (`typingCoroutine`) for text animation.
   - The `IsPlaying` property (returns `typingCoroutine != null`) is used by `DialogueControl` and examined in `BaseDialogueManager`'s coroutines.
   - **New Fix (April 2025):** Added intelligent processing of incomplete words when LLM stops sending data, detecting when no updates have occurred for 1.5 seconds.
   - **New Fix (April 2025):** Added a smooth transition fade effect for `ForceCompleteTyping()` to prevent jarring text jumps when a long message needs to be force-completed.
   - **New Fix (April 2025):** Added longer timeout (8 seconds) for dialogue animation to ensure natural typing completion in most cases.
   - **New Fix (April 2025):** Added detailed logging and safety mechanisms to ensure typing animation always completes.

5. **Dialogue Flow**:
   - Player enters dialogue range and presses E.
   - `DialogueControl.Activate()` is called.
   - `LLMCharacter.Load()` is called (if save file exists).
   - Dialogue UI is activated.
   - Player inputs text.
   - `LLMDialogueManager` sends input to `LLMCharacter.Chat()`.
   - `LLMCharacter` processes input and generates responses.
   - `BaseDialogueManager.HandleReply` receives each response chunk:
     - **Critical Improvement:** Clears and replaces `currentResponse` instead of appending.
     - Checks for `\nACTION:` or `[/ACTION]:` delimiters.
     - If found, sets `actionFoundInCurrentStream` flag, splits dialogue from action, buffers the function call, and updates display with only the dialogue part.
     - If not found, updates display with the complete text.
   - `LLMDialogueManager.UpdateDialogueDisplay` forwards text to `DialogueControl.DisplayNPCDialogueStreaming`.
   - When streaming completes, `OnReplyComplete` is called:
     - If a function was buffered, starts `ProcessActionAfterBeepSpeak` coroutine.
     - If no function, starts `EnableInputAfterBeepSpeak` coroutine.
   - Both coroutines wait for `DialogueControl.IsBeepSpeakPlaying` to become false before acting.
   - **April 2025 Fix:** Added dynamic timeout calculation based on text length to ensure coroutines don't wait indefinitely while also giving proper time for text animation.
   - Player exits dialogue (Escape or `stop_conversation` function).
   - `DialogueControl.Deactivate()` calls `ResetDialogue()` and `Save()`, then animates the UI closed (no delay).

6. **Fixed Issues (April 2025 Update):**
   - **Text Glitching:** Fixed by implementing a smooth fade transition effect when forcing text completion, particularly for cases where the displayed text and target text have a significant difference in length.
   - **Animation Cutting Off Early:** Fixed by dynamically calculating appropriate wait times based on text length (roughly 14 characters/second) rather than using a fixed 0.5-second timeout.
   - **Input Box Not Re-enabling:** Fixed by enhancing `BeepSpeak.ProcessTyping()` to detect and handle cases when the LLM sends incomplete text without word boundaries, and by adding timeout mechanisms to both `ProcessActionAfterBeepSpeak` and `EnableInputAfterBeepSpeak` coroutines.
   - **Delayed Function Execution:** Improved by ensuring sufficient time for the animation to complete naturally before force-completing the text display.

7. **Debugging Tools:**
   - `[INPUTDBG]` logs track the input re-enabling pipeline.
   - `[TIMEDBG]` logs measure durations of animations, I/O operations, and state transitions.
   - `[BeepSpeak DEBUG - COMPARISON]` logs provide detailed information about text state during force completion.

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
   Mystery JSON (`character_profiles` key) → ParsingControl (scene component) → GameControl.coreMystery
   ```
   *(Individual character file extraction step removed)*

2. **Character Initialization**:
   ```
   GameControl.coreMystery.Characters & GameControl.coreMystery.Metadata → CharacterManager (Initialize() called by InitializationManager) → CharacterPromptGenerator → LLMCharacter GameObject & Component (with generated prompt, in LoadingTemplate state)
   ```
   *(Reads character data and metadata from GameControl, generates prompt via CharacterPromptGenerator)*

3. **NPC Creation**:
   ```
   GameControl.coreMystery.Characters[name].InitialLocation → InitializationManager.SpawnAndLinkNPCs → NPCManager → NPC GameObject
   LLMCharacter (from CharacterManager) → NPCManager → NPC GameObject
   ```
   *(Relies on CharacterManager having initialized correctly; reads top-level InitialLocation data directly from the MysteryCharacter object in GameControl)*

4. **Dialogue Flow**:
   ```
   Player Input → DialogueControl → LLMDialogueManager → LLMCharacter → Response → BaseDialogueManager (HandleReply → ProcessFunctionCall) → GameControl.coreConstellation (DiscoverNode) / DialogueControl (Deactivate)
   ```
   *(Response is parsed for ACTION: delimiter; function calls trigger constellation updates or dialogue deactivation)*
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
    - **(Step 3.75) Spawn NPCs:** `InitializeGame` calls `SpawnAndLinkNPCs()`. This reads the top-level `initial_location` directly from the `MysteryCharacter` object within `GameControl.coreMystery` for each character and tells `NPCManager` where to spawn them.
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
   - Managed through `GameState` Enum
   - Transitions controlled by various systems
   - States: DEFAULT, DIALOGUE, PAUSE, FINAL, WIN, LOSE, MINIGAME, MYSTERY

2. **Character State Machine**:
   - Managed through `CharacterManager.CharacterState` Enum
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

1. **Mystery.cs** (`Assets/Scripts/CoreControl/MysteryParsing/`):
   - Core data model for mysteries, mapping to the top-level JSON structure.
   - Contains properties for `metadata` (including context), `core`, `character_profiles` (Dictionary, including revelations), `environment`, `constellation`.
   - Used by `ParsingControl` for deserialization.

2. **ParsingControl.cs** (`Assets/Scripts/CoreControl/MysteryParsing/`):
   - Component exists in the scene from start.
   - Parses the main mystery JSON file loaded from `StreamingAssets` in its `Awake` method.
   - Populates `GameControl.coreMystery` with the parsed data.
   - Sets the `IsParsingComplete` flag when done.
   - Fires `OnParsingProgress` and `OnMysteryParsed` events.

*(Removed MysteryCharacterExt
