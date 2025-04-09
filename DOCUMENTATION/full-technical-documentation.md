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

This project is a mystery game built with Unity that leverages a "black-box" architecture. This approach enables any mystery, represented as a properly structured JSON file, to be loaded into the game engine. The engine parses this data into a playable experience with characters, events, and clues.

A key feature of the project is the integration of Large Language Models (LLM) for character dialogue, creating dynamic and responsive NPC interactions.

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

The LLM integration is handled through the LLMUnity package:

- **LLM**: Core language model interface
- **LLMCharacter**: Character-specific LLM instance
- **LLMDialogueManager**: Manages dialogue flow with characters

The system uses character JSON files to generate prompts that define character personalities, knowledge, and behavior.

### Mystery System

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

Characters are managed through multiple components:

1. **Character Data**:
   - Extracted from mystery JSON
   - Stored as individual JSON files
   - Generated character files contain:
     - Core identity information
     - Personality traits
     - Knowledge and memory
     - Dialogue patterns

2. **Character Manager**:
   - Loads character data
   - Creates LLMCharacter instances
   - Manages character states (Uninitialized → LoadingTemplate → WarmingUp → Ready)
   - Handles character switching and context management

3. **Character Prompt Generation**:
   - `CharacterPromptGenerator` creates system prompts for LLM
   - Converts character data to structured prompts
   - Defines character behavior and dialogue patterns

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

## Data Flow

The project's data flow follows this pattern:

1. **Mystery Parsing**:
   ```
   Mystery JSON → ParsingControl → GameControl.coreMystery
   ```

2. **Character Extraction**:
   ```
   Mystery → MysteryCharacterExtractor → Character JSON Files
   ```

3. **Character Initialization**:
   ```
   Character JSON → CharacterManager → LLMCharacter
   ```

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

3.  **Mystery Parsing & Character Extraction** (Step 2):
    - `ParsingControl` parses the mystery JSON.
    - `MysteryCharacterExtractor` extracts character data to files.
    - Character files are validated.
    - Update LoadingOverlay status.

4.  **Train Layout Building** (Step 2.5):
    - `TrainLayoutManager` reads the `LayoutOrder` from the mystery JSON.
    - Prepares car prefabs (or JSONs) and adds them to `TrainManager.carPrefabs` list.
    - Calls `TrainManager.SpawnCars()` to instantiate the train cars.
    - `TrainLayoutManager.NameCars()` populates a dictionary mapping car keys (e.g., "business_class_1") to train car GameObjects.
    - Train cars must have the `RailCarFloor` → `Anchor (x, y)` → `walkway` hierarchical structure.
    - Each floor must have a baked NavMesh surface for NPC navigation.

5.  **Character Manager Initialization** (Step 3):
    - `CharacterManager` initializes.
    - Loads character data from files.
    - Creates and warms up `LLMCharacter` instances for each character.
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

2. **ParsingControl.cs**:
   - Parses mystery JSON files
   - Extracts character data
   - Reports parsing progress

3. **MysteryCharacterExtractor.cs**:
   - Extracts character data from mystery
   - Creates individual character files
   - Verifies character file structure

### Character System

1. **CharacterManager.cs**:
   - Manages LLM character creation
   - Handles character state transitions
   - Provides access to character instances

2. **CharacterPromptGenerator.cs**:
   - Generates LLM prompts from character data
   - Handles different character data formats
   - Structures prompts for optimal LLM behavior

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

2. **BaseDialogueManager.cs**:
   - Abstract base class for dialogue management
   - Handles LLM character interaction
   - Manages dialogue flow and callbacks

3. **LLMDialogueManager.cs**:
   - Implements concrete dialogue UI interface
   - Handles player input and response display
   - Controls dialogue UI state
