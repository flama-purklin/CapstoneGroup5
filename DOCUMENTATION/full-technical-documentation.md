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
- **PersistentSystemsManager**: Handles cross-scene persistence
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

The project uses a multi-scene approach:

1. **LoadingScreen Scene**
   - Entry point for the game
   - Contains initialization logic
   - Handles loading UI and progress display
   - Core objects:
     - Main Camera
     - LoadingUI
     - Persistent Systems
     - GameInitializer
     - GameController

2. **SystemsTest Scene** (Main Gameplay)
   - Main gameplay environment
   - Contains train environment
   - Player and NPCs interact here
   - Loaded after initialization completes

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
   - Creates NPC GameObjects
   - Connects NPCs to LLMCharacters
   - Handles NPC placement in the train environment

2. **Character Component**:
   - Links NPC GameObject to LLMCharacter
   - Provides access to character data and behavior

3. **NPCMovement**:
   - Controls NPC movement patterns
   - Manages state transitions (Idle ↔ Movement)
   - Detects player interaction and triggers dialogue

4. **NPCAnimManager**:
   - Handles NPC animations
   - Syncs animations with movement and dialogue states

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

The game initialization follows this sequence:

1. **Game Launch**:
   - LoadingScreen scene loaded
   - GameInitializer starts initialization

2. **LLM Startup** (Step 1):
   - Wait for LLM to start
   - Log progress and timing

3. **Mystery Parsing** (Step 2):
   - Parse mystery JSON
   - Extract character data
   - Verify character files

4. **Character Initialization** (Step 3):
   - Initialize CharacterManager
   - Load and warm up character models
   - Initialize NPCs with character data

5. **Scene Loading** (Step 4):
   - Load main game scene (SystemsTest)
   - Resume normal gameplay

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

2. **PersistentSystemsManager.cs**:
   - Manages cross-scene persistence
   - Creates core systems hierarchy
   - Handles scene loading events

3. **GameInitializer.cs**:
   - Controls initialization sequence
   - Coordinates system startup
   - Triggers scene transitions

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

2. **Character.cs**:
   - Links NPC GameObject to LLMCharacter
   - Provides character name and data access
   - Initializes character components

3. **NPCMovement.cs**:
   - Controls NPC movement behavior
   - Manages idle and movement states
   - Detects player interaction for dialogue

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
