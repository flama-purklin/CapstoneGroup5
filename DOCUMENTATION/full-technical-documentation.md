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
   - [Post-Processing System](#post-processing-system)
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
- **Other Manager Components**: Various UI, game state, and system managers.

### Data Model
- **Mystery**: Core data model containing all mystery information
- **MysteryCharacter**: Character data model linking to LLM system
- **MysteryConstellation**: Node-based structure of mystery elements with nodes and leads

### Runtime Systems
- **LLM System**: Handles language model interactions
- **Dialogue System**: Manages character conversations
- **NPC System**: Controls NPC behavior and interactions
- **Player System**: Handles player movement and interactions
- **Post-Processing System**: Provides visual styling through custom effects

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
        - `GameController`, `ParsingControl`, `CharacterManager`, `NPCManager`, `TrainLayoutManager` GameObjects (with respective scripts attached)
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
- **CharacterManager**: Custom script managing `LLMCharacter` instances, states, context allocation, and cache settings. It includes the `WarmupCharacter()` and `CooldownCharacter()` methods to handle character state transitions.
- **LLMDialogueManager**: Custom script managing dialogue flow with characters.

The system uses character data parsed from the main mystery JSON to generate system prompts defining personality and knowledge.
Character activity and resource allocation are managed dynamically:
- **Parallel Prompts:** The number of concurrent LLM processes is set by `LLM.parallelPrompts` (must be configured in Inspector).
- **Context Allocation:** `CharacterManager` allocates context (`nKeep`) based on `LLM.parallelPrompts`.
- **Warmup/Cooldown:** The CharacterManager exposes `WarmupCharacter()` and `CooldownCharacter()` methods to transition characters between states based on external triggers (such as proximity). These methods handle the transitions between `LoadingTemplate` and `Ready` states.
- **Caching:** `CharacterManager` has a public `enableLLMCache` flag (Inspector toggle, default false) that controls whether `LLMCharacter` uses `.cache` files for faster re-warming. *Note: Enabling cache may currently cause `JobTempAlloc` warnings.*

### Mystery System

*Design Goal: The "black-box" JSON approach allows for flexibility in mystery sourcing (hand-crafted, procedural, user-generated) and promotes replayability. The node-based constellation structure encourages active player investigation rather than passive information gathering.*

The mystery system is built around a JSON-based data model:

1. **Mystery Parsing**:
   - `ParsingControl` script component (present in scene from start) loads and parses the main mystery JSON (`fashion_mystery.json` or similar) in its `Awake` method.
   - Mystery data is deserialized and stored directly in `GameControl.GameController.coreMystery`.

2. **Mystery Structure**:
   - **Metadata**: Basic mystery information (title, version, etc.) and overall `context` string for the mystery scenario.
   - **Core**: Central mystery details (perpetrator, victim, etc.)
   - **Character Profiles (`character_profiles`)**: Character data (personality, relationships, etc.). Contains `initial_location` at the top level per character, and `core` object with `involvement` (role, type), `whereabouts` (Dictionary), `relationships` (Dictionary), `agenda`, `appearance`, `voice` (used for BeepSpeak character voice settings), and `revelations` (Dictionary of gated information with triggers).
   - **Environment**: Setting and environmental details
   - **Constellation**: Contains the underlying structure of the mystery investigation
     - **Nodes**: Dictionary of mystery nodes (evidence, testimonies, facts) keyed by unique ID
     - **Leads**: Array of lead objects linking nodes, each containing:
       - `id`: Unique identifier
       - `question`: The lead's text/question
       - `inside`: ID of the node where this lead is contained/housed
       - `answer`: ID of the node this lead reveals when followed
       - `terminal`: ID of the node where this lead will appear to the player
   - **Mini-Mysteries**: Dictionary of sub-mysteries with entry points, key nodes, and revelations
   - **Scripted Events**: Triggered by discovering specific nodes, can reveal other nodes

3. **Mystery Structure Components**:
   - **Nodes**: Represent key elements of the mystery (evidence, testimonies, facts)
   - **Leads**: Connect nodes, providing investigation paths from one node to another
   - **Mini-Mysteries**: Group related leads and nodes into cohesive sub-plots
   - **Scripted Events**: Define narrative consequences for discovering certain nodes

### Character System

*Design Goal: Character data is structured to separate core, plot-relevant information (relationships, knowledge, agenda - the CORE section in design) from personality, voice, and decision-making traits used primarily by the LLM for role-playing (the MIND ENGINE section in design).*

Characters are managed through multiple components:

1.  **Character Data**:
    *   Data is parsed directly from the main mystery JSON into the `GameControl.GameController.coreMystery.Characters` dictionary (accessed via the `character_profiles` key in JSON) by `ParsingControl` during its `Awake` phase.
    *   The `MysteryCharacter.cs` model contains:
        - `initial_location` at top-level for placement in the game world
        - `core` object with dictionaries for `whereabouts`, `relationships`, and `revelations`
        - `voice` settings for character-specific audio during dialogue (used by BeepSpeak)
        - `mind_engine` containing personality traits and speech patterns
    *   Voice data from the JSON is actively used by the BeepSpeak system to modulate character speech sounds.

2.  **Character Manager (`CharacterManager.cs`)**:
    *   Script component exists in the scene from the start.
    *   Initialization is explicitly triggered by `InitializationManager` via `Initialize()` after parsing.
    *   Reads character data from `GameControl.coreMystery.Characters`.
    *   Reads mystery context and title from `GameControl.coreMystery.Metadata`.
    *   Creates `LLMCharacter` GameObjects/components, passing character data, context, and title to `CharacterPromptGenerator` to generate the system prompt.
    *   Sets `saveCache` based on the public `enableLLMCache` field. Initializes characters only to `LoadingTemplate` state.
    *   Provides `WarmupCharacter()` and `CooldownCharacter()` methods to manage transitions between `LoadingTemplate` and `Ready` states.
    *   Allocates context (`nKeep`) based on `LLM.parallelPrompts`.
    *   Handles `OnDestroy` cleanup, deleting `.json` history files and optionally `.cache` files based on `enableLLMCache`.

3.  **Character Prompt Generation (`CharacterPromptGenerator.cs`)**:
    *   Static class used by `CharacterManager` to generate system prompts for the LLM based on character data.
    *   Takes the `MysteryCharacter` object, mystery context, and mystery title as input.
    *   Generates a detailed, markdown-formatted prompt with sections for meta instructions, core identity, personality, state of mind, knowledge/secrets/relationships, whereabouts/memories, available actions (function calls), revelation rules, speech patterns, and immutable directives.
    *   Includes detailed instructions about available functions (`reveal_node` and `stop_conversation`) and how to respond to evidence presentation via the `[PLAYER_SHOWS: node_id]` tag.

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
   - **Evidence System:** Manages the evidence dropdown UI element, allowing players to select physical evidence to show the character during dialogue.
   - Provides the `RetrieveEvidence()` method that returns the currently selected evidence ID.
   - Implements selective child GameObject activation to keep critical HUD elements visible during dialogue.

2. **DialogueUIController**:
   - Handles UI elements for the dialogue interface.
   - During input submission, retrieves selected evidence via `dialogueControl.RetrieveEvidence()`.
   - Appends the special tag `[PLAYER_SHOWS: node_id]` to the player's message when evidence is selected.
   - Resets the evidence dropdown after message submission.

3. **BaseDialogueManager**:
   - Abstract class handling core dialogue logic.
   - Uses `currentResponse.Clear(); currentResponse.Append(reply)` in `HandleReply` to handle streaming response chunks.
   - Improved Function Call Handling with `actionBuffer.Clear(); actionBuffer.Append(cleaned);` to prevent redundant text accumulation.
   - Supports both `\nACTION:` and `[/ACTION]:` delimiters for function parsing.
   - Provides coroutines `ProcessActionAfterBeepSpeak` and `EnableInputAfterBeepSpeak` that wait for typing animation to complete before acting.
   - Uses dynamically calculated wait times based on text length and BeepSpeak typing speed settings.

4. **LLMDialogueManager**:
   - Inherits from BaseDialogueManager.
   - Handles player input and LLM responses.
   - Manages dialogue flow and UI updates.
   - Implements `EnableInput`/`DisableInput` for UI interactability.

5. **BeepSpeak**:
   - Handles text display with typing effect and audio.
   - **Voice Integration:** Uses the `Voice` object from character data via the `UpdateVoice(Voice v)` method.
   - Applies voice settings (ID, timbre, pitch, speed, volume) to customize the audio output for each character.
   - Manages a typing coroutine (`typingCoroutine`) for text animation.
   - The `IsPlaying` property (returns `typingCoroutine != null`) is used to determine when text animation is complete.
   - Filters function call text to prevent action text from appearing in the dialogue box.
   - Provides `ForceCompleteTyping()` method with smooth transition effect for emergency animation completion.

6. **Dialogue Flow**:
   - Player enters dialogue range and presses E.
   - `DialogueControl.Activate()` is called.
   - `LLMCharacter.Load()` is called (if save file exists).
   - Dialogue UI is activated with selective HUD deactivation.
   - Player inputs text and optionally selects evidence from the dropdown.
   - `LLMDialogueManager` sends input to `LLMCharacter.Chat()`, including the `[PLAYER_SHOWS: node_id]` tag if evidence was selected.
   - `LLMCharacter` processes input and generates responses.
   - `BaseDialogueManager.HandleReply` receives and processes response chunks.
   - Response text is displayed via BeepSpeak with character-specific voice settings.
   - Function calls (like `reveal_node` or `stop_conversation`) are processed after typing animation completes.
   - Player exits dialogue (Escape or `stop_conversation` function).
   - `DialogueControl.Deactivate()` calls `ResetDialogue()` and `Save()`, then animates the UI closed.

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

### Post-Processing System

*Design Goal: To create a distinctive visual aesthetic inspired by Technicolor films and 1960s cinematography, enhancing the game's period atmosphere.*

The post-processing system is implemented through custom components that configure Unity's HDRP post-processing stack:

1. **Filmic60sSetup.cs**:
   - Attached to the main camera
   - Configures a comprehensive suite of post-processing effects:
     - Tonemapping with custom LUT
     - Color Adjustments (contrast, saturation, exposure)
     - Bloom with optional lens dirt
     - Film Grain
     - Vignette
     - Chromatic Aberration
     - Lens Distortion
     - Depth of Field with dynamic focus control
   - Sets up a CustomPassVolume for the Halation effect
   - Configures the camera for physical properties (aperture, blade count)
   - Implements a subtle exposure flicker via the "Film-breath" coroutine to simulate film projection
   - Provides methods to dynamically update focus distance

2. **FilmicHalation.hlsl**:
   - Custom shader for the halation effect
   - Creates a characteristic glow/bleeding around bright areas
   - Implements the distinctive "Technicolor" look

3. **HalationCustomPass**:
   - Custom pass using the halation shader
   - Applied in the AfterPostProcess injection point

This system contributes significantly to the game's visual identity, reinforcing the period setting and stylistic direction.

## Core Gameplay Concepts

*Design concepts underpinning the player's interaction with the game world and mystery.*

### Energy System
*Design Goal: To create tension and forward gameplay momentum, replacing earlier concepts of a simple timer. Energy is envisioned to deplete slowly over time and be consumed actively by key player actions like running hunch simulations and potentially engaging in extended dialogue.*

### Hunch System
*Design Goal: To transform investigation into an interactive process. Players actively form theories by connecting clues on the mystery board, testing them via limited simulations. This encourages logical reasoning and rewards attention to detail, leveraging the protagonist's analytical nature.*

### Evidence & Minigames
*Design Goal: Environmental evidence objects and solvable minigames serve as progression gates. Interacting with or solving them unlocks new leads and nodes on the mystery board, making them necessary steps to unravel the case.*

## Data Flow

The game utilizes a centralized "black-box" approach for mystery data. At initialization, `ParsingControl.cs` reads the mystery JSON file (e.g., `fashion_mystery.json`). This JSON is deserialized using `Newtonsoft.Json` into a structured C# object instance of the `Mystery` class, which acts as the central blueprint. 

This `Mystery` object contains nested specialized classes representing the data "layers":
- `MysteryCharacter`: Character data including voice settings and revelations
- `MysteryConstellation`: The constellation structure with nodes, leads, and mini-mysteries
- `MysteryEnvironment`: Environmental settings

The parsed data is stored in `GameControl.GameController.coreMystery` and accessed by various subsystems:
- `InitializationManager` orchestrates setup
- `CharacterManager` reads character data to configure `LLMCharacter` components
- `TrainLayoutManager` reads environment data to build the scene
- Gameplay systems like the BeepSpeak audio system access character voice data
- The dialogue system reads and processes revelations and evidence interactions

The project's specific data flow follows this pattern:

1. **Mystery Parsing**:
   ```
   Mystery JSON → ParsingControl → GameControl.coreMystery
   ```

2. **Character Initialization**:
   ```
   GameControl.coreMystery.Characters & Metadata → CharacterManager → CharacterPromptGenerator → LLMCharacter GameObject/Component
   ```

3. **Voice Data Flow**:
   ```
   Character.Core.Voice → BeepSpeak.UpdateVoice → Audio output customization
   ```

4. **NPC Creation**:
   ```
   GameControl.coreMystery.Characters[name].InitialLocation → InitializationManager.SpawnAndLinkNPCs → NPCManager → NPC GameObject
   LLMCharacter (from CharacterManager) → NPCManager → NPC GameObject
   ```

5. **Dialogue Flow**:
   ```
   Player Input + Selected Evidence → DialogueControl → LLMDialogueManager → LLMCharacter → Response → BaseDialogueManager → DialogueUIController → BeepSpeak
   ```

6. **Warmup/Cooldown Flow**:
   ```
   External Trigger → CharacterManager.Warmup/CooldownCharacter → LLMCharacter State Change
   ```

7. **Save/Load Flow**:
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
    - **(Step 4) Complete Init:** `InitializeGame` calls `CompleteInitialization()`, hiding the loading overlay and enabling gameplay.

## Event System

The project uses C# events for communication between systems:

1. **Parsing Events**:
   - `ParsingControl.OnParsingProgress`: Reports parsing progress (0.0 to 1.0).
   - `ParsingControl.OnMysteryParsed`: Indicates the main mystery object has been deserialized into `GameControl.coreMystery`.

2. **Character Manager Events**:
   - `CharacterManager.OnInitializationComplete`: Signals `CharacterManager` has finished its internal setup (creating LLMCharacters and loading templates). Triggered at the end of its `TwoPhaseInitialization` coroutine.

3. **UI Events**:
   - Standard Unity UI events (Button.onClick, InputField.onSubmit)
   - Used for dialogue interaction and evidence selection

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
     - `Characters/`: Character-related logic (`CharacterManager`, `CharacterPromptGenerator`)
     - `NPCs/`: NPC behavior (`NPCManager`, `NPCMovement`, `NPCAnimManager`, `DialogueControl`)
     - `Player/`: Player movement and interaction
     - `Train/`: Train environment (`TrainLayoutManager`, `TrainManager`)
     - `UI/`: User interfaces including `DialogueUIController`
     - `Dialogue/`: Dialogue system (`BaseDialogueManager`, `LLMDialogueManager`)

2. **Mystery**:
   - `Assets/Mystery/`: Mystery system
     - `Myst_Gen/`: Mystery generation
     - `Myst_Play/`: Mystery gameplay

3. **StreamingAssets**:
   - `StreamingAssets/MysteryStorage/`: Contains the mystery JSON files (e.g., `fashion_mystery.json`).

4. **LLM**:
   - `Assets/LLMUnity-release-v2.4.2/`: LLM integration package

5. **Post-Processing**:
   - `Assets/PostProcessing/`: Post-processing effects
     - `Filmic60sSetup.cs`: Main setup script
     - `FilmicHalation.hlsl`: Custom shader for the halation effect

6. **DialogueSFX**:
   - `Assets/DialogueSFX/`: Contains BeepSpeak system and audio files
     - `BeepSpeak.cs`: Core implementation for dialogue audio and text animation
     - Various audio files for different character voice timbres

## Art Direction

*High-level visual goals influencing asset creation and presentation.*

### Camera Setup
- 2/3rd Top-Down perspective.

### Art Style
- Blend of 3D environments and 2D character sprites.
- Requires lighting from the 3D environment to realistically affect 2D assets.

### Aesthetics
- Inspired by Mid-Century Futurism (e.g., Jetsons, Space Era illustrations) and Swinging Sixties styles (e.g., Deathloop, pop art).
- Enhanced by the custom Technicolor-inspired post-processing effects.

## Dependencies

The project has the following key dependencies:

1. **External Libraries**:
   - Newtonsoft.Json: Used for JSON parsing
   - LLMUnity: Integration with language models

2. **Internal Dependencies**:
   - InitializationManager → ParsingControl, CharacterManager, NPCManager, TrainLayoutManager, LLM, GameControl
   - ParsingControl → GameControl
   - CharacterManager → LLM, GameControl, CharacterPromptGenerator
   - NPCManager → CharacterManager, TrainLayoutManager, GameControl
   - DialogueControl → LLMDialogueManager, CharacterManager, BeepSpeak, Evidence UI Elements
   - DialogueUIController → DialogueControl (for evidence retrieval)
   - BaseDialogueManager → DialogueControl, BeepSpeak
   - BeepSpeak → Character Voice Data

## Code Reference

### Core Scripts

1. **GameControl.cs**:
   - Manages game state
   - Stores mystery data (`coreMystery`)
   - Singleton access via `GameControl.GameController`

### Mystery System

1. **Mystery.cs** (`Assets/Scripts/CoreControl/MysteryParsing/`):
   - Core data model for mysteries, mapping to the top-level JSON structure.
   - Contains properties for `metadata` (including context), `core`, `character_profiles` (Dictionary, including revelations), `environment`, `constellation` (with nodes and leads), `scripted_events`, and more.
   - Used by `ParsingControl` for deserialization.

2. **MysteryConstellation.cs** (`Assets/Scripts/CoreControl/MysteryParsing/Constellation/`):
   - Defines the structure for nodes, leads, and mini-mysteries.
   - Contains dictionaries and collections for these elements.
   - Implements methods for node discovery and mystery navigation.

3. **ParsingControl.cs** (`Assets/Scripts/CoreControl/MysteryParsing/`):
   - Component exists in the scene from start.
   - Parses the main mystery JSON file loaded from `StreamingAssets` in its `Awake` method.
   - Populates `GameControl.coreMystery` with the parsed data.
   - Sets the `IsParsingComplete` flag when done.
   - Fires `OnParsingProgress` and `OnMysteryParsed` events.

### Character System

1. **MysteryCharacter.cs** (`Assets/Scripts/CoreControl/MysteryParsing/Characters/`):
   - Defines the structure for character data.
   - Contains properties for `core`, `mind_engine`, `initial_location`, and more.
