# Unity Project Documentation - Executive Summary

## Project Overview

This Unity project implements a mystery game built around a modular "black box" design that processes mystery JSON files to create unique gameplay experiences. The game leverages an LLM (Large Language Model) integration for dynamic character interactions and dialogue.

## Key Architecture Components

### Event & Messaging System
- **Event-based Communication**: The project uses C# events (System.Action delegates) for cross-component communication, primarily for initialization flow and progress reporting.
- **Primary Event Publishers**: `ParsingControl`, `MysteryCharacterExtractor`, and `CharacterManager` publish events for initialization progress and completion.
- **Primary Event Subscribers**: `GameInitializer` subscribes to events to coordinate the sequential initialization process.
- **UI Event Binding**: Standard Unity UI event binding for dialogue interactions.

### Execution Lifecycle & State Transitions
- **Game State Management**: Uses a `GameState` enum in the `GameControl` singleton to track high-level game states (DEFAULT, DIALOGUE, PAUSE, etc.).
- **Character State Management**: Implements a more sophisticated state machine for character initialization (Uninitialized → LoadingTemplate → WarmingUp → Ready).
- **Initialization Sequence**: 
  1. LLM startup
  2. Mystery parsing and character extraction
  3. Character manager initialization
  4. Main scene loading
- **NPC Behavior States**: NPCs cycle between idle and movement states, with transitions to dialogue when players interact.

### Runtime Data & Asset Relationships
- **Mystery Data Structure**: Uses a complex object hierarchy starting with the `Mystery` class, which contains metadata, core mystery details, characters, and a node-based "constellation" structure.
- **Character Data Flow**: 
  1. Mystery JSON parsing → Character extraction → Individual character files
  2. Character files → LLMCharacter initialization → NPC behavior
- **Runtime Component References**: Characters spawned in the world contain references to their corresponding LLMCharacter components, which connect to the LLM system.

## Technical Implementation Details

### Key Systems
1. **Mystery Processing Pipeline**
   - Mystery JSON → `ParsingControl` → Character extraction → Individual character files
   - Core mystery data stored in `GameControl` singleton for game logic

2. **Character System**
   - Character files → `CharacterManager` → LLMCharacter objects
   - Robust initialization with retry mechanics and state tracking
   - Template generation for LLM via `CharacterPromptGenerator`

3. **Dialogue System**
   - `DialogueControl` manages UI interactions
   - `LLMDialogueManager` handles communication with LLM
   - Streaming text responses with callbacks

4. **NPC System**
   - `NPCManager` handles spawning and tracking NPCs
   - `NPCMovement` controls state-based behavior
   - Interaction zones for player-NPC dialogue

### Critical Dependencies
1. **LLM Integration**: Core to character dialogue and personality
2. **Mystery JSON Structure**: Drives all game content and logic
3. **Initialization Sequence**: Multi-step process with dependencies between systems

## Performance Considerations
- **Async Loading**: Mystery parsing and character initialization use async/await patterns
- **Progress Reporting**: Event-based progress tracking for UI feedback
- **Resource Management**: LLM context allocation across characters

## Architectural Patterns
- **Singleton Management**: Core managers use singleton pattern (GameControl, PersistentSystemsManager)
- **Event-Based Communication**: System coordination through events
- **State Machine Implementations**: Character and NPC state management
- **Component-Based Design**: Standard Unity component architecture