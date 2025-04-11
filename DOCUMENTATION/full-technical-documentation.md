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
- **ParsingControl**: Handles mystery JSON parsing
- **NPCManager**: Manages NPC creation and behavior
- **CharacterManager**: Manages LLM character creation and interaction

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
        - InitializationManager (manages startup)
        - LoadingOverlay (displays progress, blocks view initially)
        - LLM, CharacterManager, ParsingControl, NPCManager, etc.
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

The LLM integration is handled through the LLMUnity package:

- **LLM**: Core language model interface
- **LLMCharacter**: Character-specific LLM instance
- **LLMDialogueManager**: Manages dialogue flow with characters

The system uses character JSON files to generate prompts that define character personalities, knowledge, and behavior.

### Mystery System

*Design Goal: The "black-box" JSON approach allows for flexibility in mystery sourcing (hand-crafted, procedural, user-generated) and promotes replayability. The node-based constellation structure encourages active player investigation rather than passive information gathering.*

The mystery system is built around a JSON-based data model:

1. **Mystery Parsing**:
   - `ParsingControl` loads and parses mystery JSON
   - Mystery data is stored in `GameControl.coreMystery`
   - Character data extracted for LLM integration

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

*(Refactoring Goal: The current character data handling needs improvement to address initialization order issues and redundant file usage. See Initialization Sequence section for details.)*

Characters are managed through multiple components:

1.  **Character Data (As Implemented - Needs Refactor)**:
    *   Data is extracted from the main mystery JSON by `ParsingControl` into `GameControl.coreMystery.Characters`.
    *   Simultaneously, `MysteryCharacterExtractor` processes this data and **saves individual character JSON files** to `Assets/StreamingAssets/Characters/`.
    *   `CharacterManager` then **reads these individual JSON files** to initialize `LLMCharacter` instances.
    *   **Problem:** This creates unnecessary file I/O and potential synchronization issues (initialization race condition).
    *   **Desired Future State:** Eliminate the creation/reading of individual character JSON files. `CharacterManager` should directly use the `GameControl.coreMystery.Characters` dictionary populated *synchronously* by `ParsingControl` during initialization.

2.  **Character Manager**:
    *   **As Implemented:** Loads character data by reading individual JSON files.
    *   **Desired Future State:** Read data directly from the `GameControl.coreMystery.Characters` dictionary *after* parsing is confirmed complete.
    *   Manages `LLMCharacter` instances and GameObjects.
    *   Manages character states (Uninitialized → LoadingTemplate → WarmingUp → Ready).
    *   Handles character switching and context management.

3.  **Character Prompt Generation**:
    *   `CharacterPromptGenerator` (or `TempCharacterPromptGenerator`) creates system prompts for the LLM.
    *   **As Implemented:** Likely reads data derived from the individual character files.
    *   **Desired Future State:** Ensure the prompt generator takes the `MysteryCharacter` object directly from the in-memory dictionary as input.
    *   Converts character data to structured prompts defining behavior and dialogue patterns.

### NPC System

NPCs are managed through the following components:

1. **NPCManager**:
   - Creates NPC GameObjects using the `NPC.prefab` template
   - Connects NPCs to LLMCharacters
   - Handles NPC placement in the train environment based on the character's `initial_location` field
   - Contains `availableAnimContainers` array (must be assigned in Inspector) which holds references to the four `NPCAnimContainer` Scriptable Objects used for visual representation
   - Assigns animations cyclically to NPCs based on character index

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
   - Manages dialogue UI activation/deactivation
   - Controls state transitions to/from dialogue
   - Connects NPCs to the dialogue system

2. **LLMDialogueManager**:
   - Inherits from BaseDialogueManager
   - Handles player input and LLM responses
   - Manages dialogue flow and UI updates

3. **Dialogue Flow**:
   - Player enters dialogue range and presses E
   - DialogueControl activates dialogue UI
   - Player inputs text through the dialogue system
   - LLMCharacter processes input and generates responses
   - LLMDialogueManager displays streaming responses

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

The project's data flow follows this pattern:

1. **Mystery Parsing**:
   ```
   Mystery JSON → ParsingControl → GameControl.coreMystery
   ```
   Mystery JSON → ParsingControl → GameControl.coreMystery
   ```

2. **Character Data Extraction (As Implemented - Needs Refactor)**:
   ```
   GameControl.coreMystery.Characters → MysteryCharacterExtractor → Character JSON Files (in StreamingAssets/Characters/)
   ```
   * **Problem:** Redundant file I/O, potential race condition.
   * **Goal:** Remove this step.

3. **Character Initialization (As Implemented - Needs Refactor)**:
   ```
   Character JSON Files → CharacterManager → LLMCharacter GameObject & Component
   ```
   * **Problem:** Depends on potentially unsynchronized file writing from step 2.
   * **Goal:** `GameControl.coreMystery.Characters → CharacterManager → LLMCharacter GameObject & Component` (using data populated in step 1).

4. **NPC Creation**:
   ```
   LLMCharacter → NPCManager → NPC GameObject
   ```

5. **Dialogue Flow**:
   ```
   Player Input → DialogueControl → LLMDialogueManager → LLMCharacter → Response
   ```

## Initialization Sequence

The game initialization occurs entirely within the `SystemsTest` scene, managed by the `InitializationManager` and visually represented by the `LoadingOverlay`:

1.  **Scene Load**:
    - `SystemsTest` scene loads.
    - `LoadingOverlay` is active, blocking the game view. Player input is disabled.
    - `InitializationManager` starts the sequence.

2.  **LLM Startup** (Step 1):
    - Wait for the shared LLM service to start.
    - Update LoadingOverlay status.

3.  **Mystery Parsing & Character File Extraction (As Implemented - Needs Refactor)** (Step 2):
    - `ParsingControl` parses the main mystery JSON, populating `GameControl.coreMystery`.
    - `MysteryCharacterExtractor` is called (or runs independently) to create individual character JSON files in `StreamingAssets/Characters/`.
    - **Problem:** This file extraction step is redundant. More importantly, its timing relative to Character Manager initialization (Step 5) is not strictly enforced, leading to race conditions where `CharacterManager` might try to read files before they are written, requiring a second run in the editor.
    - **Desired Future State:** Remove the file extraction part entirely. `ParsingControl` should only parse the main JSON and populate `GameControl.coreMystery`. `InitializationManager` must explicitly wait for `ParsingControl` to finish *before* proceeding to Step 5.
    - Update LoadingOverlay status.

4.  **Train Layout Building** (Step 2.5):
    - `TrainLayoutManager` reads the `LayoutOrder` from the mystery JSON.
    - Prepares car prefabs (or JSONs) and adds them to `TrainManager.carPrefabs` list.
    - Calls `TrainManager.SpawnCars()` to instantiate the train cars.
    - `TrainLayoutManager.NameCars()` populates a dictionary mapping car keys (e.g., "business_class_1") to train car GameObjects.
    - Train cars must have the `RailCarFloor` → `Anchor (x, y)` → `walkway` hierarchical structure.
    - Each floor must have a baked NavMesh surface for NPC navigation.

5.  **Character Manager Initialization (As Implemented - Needs Refactor)** (Step 3):
    - `InitializationManager` calls `CharacterManager.Initialize()`.
    - `CharacterManager` reads character data from the **individual JSON files** in `StreamingAssets/Characters/`.
    - **Problem:** Depends on Step 3 (file extraction) completing first, which isn't guaranteed synchronously, causing the initialization race condition.
    - **Desired Future State:** `CharacterManager` should read directly from the `GameControl.coreMystery.Characters` dictionary *after* `InitializationManager` has confirmed Step 3 (Parsing) is fully complete.
    - Creates and warms up `LLMCharacter` instances and GameObjects for each character.
    - `NPCManager` initializes, caching LLM data.

6.  **NPC Spawning** (Step 3.5):
    - `InitializationManager.SpawnAllNPCs()` retrieves available character names.
    - For each character:
      - Calls `CharacterManager.GetCharacterStartingCar()` to get the car name from character JSON's `initial_location` field.
      - Calls `TrainLayoutManager.GetCarTransform()` to find the corresponding car.
      - Calls `TrainLayoutManager.GetSpawnPointInCar()` to find a valid NavMesh position in the car.
      - The spawn point is selected by prioritizing central anchors (e.g., "Anchor (3, 7)"), then non-edge anchors, then first available.
      - Uses `NavMesh.SamplePosition()` to find a valid NavMesh point near the walkway.
      - Calls `NPCManager.SpawnNPCInCar()` to instantiate the NPC with appropriate animation appearance.

7.  **Transition to Gameplay** (Step 4):
    - Once all steps are complete, `InitializationManager` signals completion.
    - `LoadingOverlay` fades out or is disabled.
    - Player input is enabled.
    - Game state transitions to `DEFAULT`.
    - Normal gameplay resumes.

## Event System

The project uses C# events for communication between systems:

1. **Parsing Events**:
   - `ParsingControl.OnParsingProgress`: Reports parsing progress
   - `ParsingControl.OnMysteryParsed`: Indicates mystery parsed
   - `ParsingControl.OnCharactersExtracted`: Reports character extraction
   - `ParsingControl.OnParsingComplete`: Signals parsing completion

2. **Character Extraction Events**:
   - `MysteryCharacterExtractor.OnExtractionProgress`: Reports extraction progress
   - `MysteryCharacterExtractor.OnCharactersExtracted`: Signals extraction completion

3. **Character Manager Events**:
   - `CharacterManager.OnInitializationComplete`: Signals character initialization complete

4. **UI Events**:
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
   - Enforces valid transitions between states

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
     - `CoreControl/`: Core game management
     - `NPCs/`: NPC behavior
     - `Player/`: Player movement and interaction
     - `Train/`: Train environment
     - `UI/`: User interfaces

2. **Mystery**:
   - `Assets/Mystery/`: Mystery system
     - `Myst_Gen/`: Mystery generation
     - `Myst_Play/`: Mystery gameplay
       - `Dialogue/LLM/`: LLM integration

3. **StreamingAssets**:
   - `StreamingAssets/MysteryStorage/`: Mystery JSON files
   - `StreamingAssets/Characters/`: Character JSON files

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
   - GameControl → Mystery
   - ParsingControl → MysteryCharacterExtractor
   - CharacterManager → LLM
   - NPCManager → CharacterManager
   - DialogueControl → LLMDialogueManager

## Code Reference

### Core Scripts

1. **GameControl.cs**:
   - Manages game state
   - Stores mystery data
   - Singleton access via GameControl.GameController

*(Note: PersistentSystemsManager.cs and GameInitializer.cs removed as they are deprecated or superseded by InitializationManager in the unified scene)*

### Mystery System

1. **Mystery.cs**:
   - Core data model for mysteries
   - Contains all mystery information
   - Used by game systems for gameplay

2. **ParsingControl.cs** (`Assets/Scripts/CoreControl/MysteryParsing/`):
   - Parses mystery JSON file loaded from `StreamingAssets`.
   - Populates `GameControl.coreMystery` with the parsed data.
   - Coordinates with `MysteryCharacterExtractor` to process character data from the `Mystery` object.
   - Fires events (`OnParsingProgress`, `OnMysteryParsed`, `OnCharactersExtracted`, `OnParsingComplete`) to signal progress and completion, which `InitializationManager` waits for.

3. **MysteryCharacterExtractor.cs** (`Assets/Scripts/CoreControl/MysteryParsing/`):
   - Processes character data from the in-memory `Mystery` object provided by `ParsingControl`.
   - *Note: No longer writes individual character files. Ensure any duplicate versions of this script are deleted.*
   - Fires events (`OnExtractionProgress`, `OnCharactersExtracted`) related to processing progress and completion, which `ParsingControl` listens for.

### Character System

1. **CharacterManager.cs**:
   - Reads character data from `GameControl.coreMystery.Characters`.
   - Manages LLM character creation
   - Handles character state transitions
   - Provides access to character instances

2. **CharacterPromptGenerator.cs** (`Assets/Scripts/Characters/`):
   - Static class used by `CharacterManager` to generate system prompts for the LLM based on character data.
   - Handles different character data formats.
   - Structures prompts for optimal LLM behavior.
   - *Note: A `TempCharacterPromptGenerator.cs` might exist; ensure the correct one is used/referenced.*

3. **LLMCharacter.cs** (from LLMUnity):
   - Interfaces with LLM system
   - Manages character dialogue and responses
   - Handles prompt and context management

### NPC System

1. **NPCManager.cs**:
   - Spawns and manages NPCs
   - Links NPCs to LLM characters
   - Handles NPC placement in environment
   - Assigns NPCAnimContainer visuals to NPCs

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
   - Orchestrates the entire game initialization sequence
   - Manages LoadingOverlay and initialization state
   - Coordinates between systems: LLM, ParsingControl, TrainLayoutManager, etc.
   - Handles NPC spawning through SpawnAllNPCs() method
   - Ensures proper startup sequence and transition to gameplay

### Dialogue System

1. **DialogueControl.cs**:
   - Manages dialogue UI and state
   - Activates dialogue with NPCs
   - Controls transitions to/from dialogue state

2. **BaseDialogueManager.cs** (`Assets/Mystery/Myst_Play/Dialogue/LLM/`):
   - Abstract base class for dialogue management.
   - *Note: Does not appear to be attached to any active GameObject in the `SystemsTest` scene.*
   - Handles LLM character interaction.
   - Manages dialogue flow and callbacks.

3. **LLMDialogueManager.cs** (`Assets/Mystery/Myst_Play/Dialogue/LLM/`):
   - Implements concrete dialogue UI interface, inheriting from `BaseDialogueManager`.
   - *Note: Does not appear to be attached to any active GameObject in the `SystemsTest` scene.*
   - Handles player input and response display.
   - Controls dialogue UI state.
