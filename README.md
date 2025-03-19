## Build & Run Commands
- Open project in Unity Editor (version 6000.0.34f1)
- Build project: Build > Build Solution (Select Windows Standalone)
- Run project: Play button in Unity Editor
- Debug: Use Debug.Log(), Debug.LogWarning(), Debug.LogError()

## Project Configuration
- Unity HDRP (High Definition Render Pipeline)
- Input System: New Input System (InputSystem_Actions.inputactions)
- Main scenes: MainMenu, LoadingScreen, GAME, SystemsTest, ResultScreen

## Code Style Guidelines
- Class naming: PascalCase (CoreSystemsManager, DialogueControl)
- Private variables: camelCase with initial underscore (_myVariable)
- Properties: PascalCase (CurrentState)
- Use [SerializeField] for inspector-visible private fields
- Group related fields with [Header("Group Name")]
- Keep MonoBehaviour methods in lifecycle order (Awake, Start, Update)
- Use region tags for logical grouping of code sections
- Prefer component-based architecture over inheritance
- Use singleton pattern for manager classes
- Check for null references before accessing objects
- Use coroutines for time-dependent operations
- Document complex algorithms or non-obvious code

## Project Overview
This is a detective mystery game set on a train. Players investigate by exploring train cars, talking with NPCs (powered by LLM dialogue), examining evidence, and solving puzzles to uncover the mystery.

### Mystery Engine Architecture
The game is built around a "mystery engine" architecture that separates content from generation methods:
- **Mystery Abstraction**: Mysteries are represented in a standardized template format
- **Generation Agnosticism**: Content can come from handcrafted design, procedural generation, or user creation
- **Mystery Simulator**: Takes mystery blueprints and instantiates them as game objects
- **Black Box Approach**: Regardless of source, all mysteries use the same data model and simulation system

### Roguelike Mystery Elements
- Each gameplay loop produces a completely different mystery to solve
- Varying cast of characters, evidence, locations, and logical structure
- Procedurally generated train layouts tailored to each mystery
- Enhanced social deduction through free-form LLM dialogue

## Directory Structure
- `/Assets/Scripts/CoreControl/`: Core game systems and state management
- `/Assets/Scripts/NPCs/`: NPC behavior, animations, and dialogue
- `/Assets/Scripts/Player/`: Player movement and interaction
- `/Assets/Scripts/Train/`: Train car management and environment
- `/Assets/Scripts/Camera/`: Camera control and visual effects
- `/Assets/Scripts/UI/`: UI components, minigames, and mystery board
- `/Assets/Scripts/UI/Mystery/`: Mystery board UI and inspection tools
- `/Assets/Scripts/UI/HUD/`: HUD notifications and UI elements
- `/Assets/Mystery/`: Mystery generation and gameplay elements
- `/Assets/Mystery/Myst_Gen/`: Mystery generation components 
- `/Assets/Mystery/Myst_Gen/NodeTypes/`: Visual node implementation
- `/Assets/Mystery/Myst_Gen/Connections/`: Node connections system
- `/Assets/Mystery/Myst_Play/Dialogue/LLM/`: LLM dialogue management scripts
- `/Assets/LLMUnity-release-v2.4.2/`: LLM integration framework
- `/Assets/StreamingAssets/Characters/`: Character JSON definitions
- `/Assets/StreamingAssets/Mysteries/`: Mystery template and blueprint JSON files
- `/Assets/StreamingAssets/TrainLayouts/`: Train car template JSON definitions

## Key Script Files

### Core Systems
- **CoreSystemsManager.cs**: Singleton for core systems (EventSystem, AudioListener)
- **GameControl.cs**: Central game state management via GameState enum
- **GameInitializer.cs**: Initializes game systems and handles scene transitions
- **PersistentSystemsManager.cs**: Manages objects that persist across scene loads

### Mystery System
- **Mystery.cs**: Data structure containing all mystery information
- **ParsingControl.cs**: Parses mystery JSON into game objects
- **MysteryGenBeta.cs**: Handles mystery generation mechanics
- **MysteryNavigation.cs**: Controls mystery board UI and navigation
- **NodeControl.cs**: Manages visual nodes and their connections in the mystery board
- **VisualNode.cs**: Base class for visual node implementations
- **Connection.cs**: Handles visual connections between nodes
- **EvidenceInspect.cs**: Displays detailed information when inspecting a node
- **MysteryTemplateManager.cs**: Manages mystery templates and validates their structure
- **MysteryBlueprintGenerator.cs**: Converts templates into complete mystery blueprints
- **MysteryScoreCalculator.cs**: Compares player board to blueprint for scoring
- **ConstellationValidator.cs**: Ensures mystery solvability and logical integrity

### Mystery Data Model
- **MysteryData.cs**: Root class containing all mystery components
  - **Metadata**: Basic information (ID, version, title)
  - **Core**: Fundamental setup (type, theme, victim, culprit, method, motive)
  - **CharacterProfiles**: Complete character data with core and mind engine data
  - **TrainLayout**: Physical environment definition including cars and points of interest
  - **Characters**: Investigation-specific character data
  - **Constellation**: Mystery solution structure (nodes, edges, mini-mysteries)
- **ConstellationNode.cs**: Base class for all node types
  - Extended by specialized types (Fact, Evidence, Testimony, Lead, Barrier, etc.)
- **ConstellationEdge.cs**: Connection between nodes with type and strength properties
- **MiniMystery.cs**: Sub-investigation component of the main mystery

## Mystery Data Model Specification: Actual Implementation

This section documents the *actual implemented format* of mystery files as observed in `transformed-mystery.json`, noting where it diverges from the original design specification.

### Core Data Structure

```
Mystery
│
├── Metadata (id, version, last_updated, title)
│
├── Core (fundamental mystery setup)
│   ├── Type/Subtype/Theme specification
│   ├── Victim/Culprit/Manipulator references
│   └── Method/Motive/Circumstance
│
├── Characters (complete character data)
│   ├── Character 1 (eleanor_verne)
│   ├── Character 2 (gideon_marsh)
│   └── ...
│
├── Environment (physical train layout)
│   ├── Cars
│   └── Points of Interest
│
└── Constellation (mystery solution structure)
    ├── Nodes (facts, evidence, testimonies, etc.)
    ├── Connections (logical relationships between nodes)
    ├── Mini Mysteries (sub-investigations)
    └── Scripted Events (triggers for game progression)
```

### Detailed Field Specifications

#### 1. Metadata
Simple key-value collection with basic information about the mystery:
```json
"metadata": {
  "id": "art_forgery_murder",
  "version": "1.0",
  "last_updated": "2025-03-05 11:30:46",
  "title": "Art Fraud on the Fashion Express"
}
```

#### 2. Core Mystery Setup
Defines fundamental aspects of the mystery:
```json
"core": {
  "type": "Migratory Route",
  "subtype": "Fashion Week",
  "theme": "Art Fraud and Paranoia",
  "victim": "victoria_blackwood",
  "culprit": "maxwell_porter",
  "manipulator": "gregory_crowe",
  "method": "Staged Suicide",
  "motive": "Protecting Art Forgery Ring",
  "circumstance": {
    "location": "lounge_bathroom",
    "time_minutes": 123,
    "details": "Maxwell sedated Victoria and staged a suicide by hanging using her scarf and bathroom fixtures"
  }
}
```

**Note:** Character references are simple string IDs rather than nested objects.

#### 3. Characters

Each character is defined by a complex nested structure with the character ID as the key:

```json
"characters": {
  "eleanor_verne": {
    "core": { ... },
    "mind_engine": { ... },
    "initial_location": "business_class_car_1",
    "key_testimonies": { ... }
  }
}
```

##### 3.1 Character Core Data
```json
"core": {
  "involvement": {
    "role": "Red Herring",
    "type": "Former authenticator with damaged reputation",
    "mystery_attributes": [
      "Unknowingly authenticated forged artworks for Victoria",
      "Currently works for Gregory's gallery",
      "Can identify Maxwell's artistic style",
      "Possesses knowledge of Gregory's unusual interest in forgery detection"
    ]
  },
  "whereabouts": [
    {
      "key": "0",
      "value": {
        "location": "platform",
        "action": "reviewing art catalog from Montreu exhibition",
        "events": [
          "Briefly acknowledged Victoria with professional courtesy"
        ]
      }
    },
    // Additional whereabouts...
  ],
  "relationships": [
    {
      "key": "Victoria Blackwood",
      "value": {
        "attitude": "complex mix of respect, resentment, and guilt",
        "history": [
          "Worked as her trusted art authenticator for years",
          "Career devastated when forgeries she authenticated were discovered",
          "Victoria distanced herself professionally after the scandal"
        ],
        "known_secrets": [
          "Victoria was genuinely unaware of the forgery operation"
        ]
      }
    },
    // Additional relationships...
  ],
  "agenda": {
    "primary_goal": "Rebuild her reputation as an art authenticator"
  }
}
```

**DIVERGENCE ALERT:** The `whereabouts` structure uses key-value pair objects rather than the direct structure specified in the streamlined model. It maintains the old format from the original complex model.

##### 3.2 Mind Engine Data
```json
"mind_engine": {
  "identity": {
    "name": "Eleanor Verne",
    "occupation": "Art authenticator currently working for Gregory's gallery",
    "personality": {
      "O": 0.8,
      "C": 0.9,
      "E": 0.5,
      "A": 0.6,
      "N": 0.7
    }
  },
  "state_of_mind": {
    "worries": "never recovering from the forgery scandal",
    "feelings": "conflicted about Victoria's death, guilty yet relieved",
    "reasoning_style": "detail-oriented and analytical"
  },
  "speech_patterns": {
    "vocabulary_level": "scholarly and precise",
    "sentence_style": [
      "speaks methodically with technical specificity"
    ],
    "speech_quirks": [
      "often references specific artistic techniques when making comparisons",
      "unconsciously analyzes the authenticity of objects around her",
      "hesitates before making definitive statements about authenticity",
      "occasionally lapses into art history lectures when nervous"
    ],
    "common_phrases": [
      "The brushwork is distinctive",
      "Upon closer examination",
      "The provenance suggests"
    ]
  },
  "gender": "F" // New field for sprite assignment
}
```

**NEW FIELD ALERT:** The `gender` field has been added to the mind_engine section to support character sprite assignment. It accepts values "F" for female or "M" for male characters.

##### 3.3 Key Testimonies
```json
"key_testimonies": {
  "eleanor_maxwell_skills": {
    "content": "Maxwell has an extraordinary eye for detail and technical skill. His ability to mimic artistic styles is... well, it's remarkable. I've seen him perfectly recreate a Monet brushstroke technique during a demonstration at Gregory's gallery.",
    "reveals": "testimony-maxwell-artistic-ability",
    "requires": ["basic_investigation", "maxwell_introduction"],
    "state": "default",
    "methods": ["discuss_art_techniques", "show_appreciation_for_expertise"]
  }
}
```

**DIVERGENCE ALERT:** This structure differs significantly from the streamlined model's intent. It adds a character-specific "key_testimonies" collection rather than embedding this in the constellation nodes.

#### 4. Environment (Train Layout)

The environment documents the physical spaces of the mystery:

```json
"environment": {
  "cars": {
    "storage_car": {
      "name": "Storage Car",
      "description": "Filled with luggage and supplies for the journey",
      "points_of_interest": {
        "luggage_section": {
          "name": "Luggage Section",
          "description": "Organized storage for passenger luggage",
          "evidence_items": [],
          "initial_state": "normal"
        },
        "maintenance_closet": {
          "name": "Maintenance Closet",
          "description": "Contains cleaning supplies and train equipment",
          "evidence_items": [],
          "initial_state": "locked"
        }
      }
    },
    // Additional cars...
  },
  "layout_order": ["storage_car", "second_class_car_1", "second_class_car_2", "kitchen_car", "business_class_car_1", "bar_car", "business_class_car_2", "lounge_car", "first_class_car", "engine_room"]
}
```

#### 5. Constellation (Mystery Structure)

##### 5.1 Nodes
```json
"constellation": {
  "nodes": {
    "fact-murder": {
      "type": "FACT",
      "category": "MAIN_MYSTERY",
      "content": "Victoria Blackwood was found dead in the bathroom of the Lounge Car, appearing to have hanged herself with her scarf",
      "discovered": true,
      "location": "lounge_bathroom",
      "time": 123,
      "characters": ["victoria_blackwood"]
    },
    // Example evidence node
    "evidence-suicide-note": {
      "type": "EVIDENCE",
      "category": "MAIN_MYSTERY",
      "content": "Suicide note found beside Victoria",
      "discovered": false,
      "location": "lounge_bathroom",
      "time": 123,
      "characters": ["victoria_blackwood", "maxwell_porter"],
      "description": "A handwritten note on Victoria's personal stationery",
      "hidden_details": [
        "The handwriting is suspiciously perfect",
        "It mentions 'taking full responsibility for the art fraud'",
        "Mimics Victoria's writing style from magazine editorials"
      ],
      "can_pickup": true
    },
    // Additional nodes...
  }
}
```

##### 5.2 Connections
```json
"connections": [
  {
    "source": "fact-murder",
    "target": "evidence-suicide-note",
    "type": "REVEALS"
  },
  {
    "source": "evidence-suicide-note",
    "target": "lead-suicide-question",
    "type": "SUGGESTS"
  },
  // Additional connections...
]
```

**MAJOR DIVERGENCE ALERT:** Connections are implemented as an array of objects rather than an object with connection IDs as keys. This is a significant structural difference from the streamlined template.

##### 5.3 Mini Mysteries
```json
"mini_mysteries": {
  "mini-a": {
    "name": "The Art Forgery Ring",
    "description": "Uncovering the forgery operation run by Gregory and executed by Maxwell",
    "entry_points": ["testimony-maxwell-artistic-ability", "fact-forgery-ring", "barrier-camera"],
    "revelation": "revelation-forgery-ring",
    "connects_to_main": ["lead-suicide-question", "lead-two-men"]
  },
  "mini-b": {
    "name": "Victoria's Desperate Measures",
    "description": "Investigating why Victoria was creating industry scandals",
    "entry_points": ["testimony-victoria-penelope-argument", "evidence-financial-records"],
    "revelation": "revelation-victoria-desperate",
    "connects_to_main": ["lead-suicide-question"]
  }
}
```

##### 5.4 Scripted Events
```json
"scripted_events": {
  "phase_1": {
    "character": null,
    "trigger": "fact-murder AND evidence-suicide-note AND lead-suicide-question AND lead-two-men",
    "description": "Maxwell begins pacing nervously in the second class car"
  },
  "phase_2": {
    "character": null,
    "trigger": "testimony-maxwell-artistic-ability AND fact-forgery-ring AND testimony-victoria-threats",
    "description": "Maxwell becomes visibly agitated; Gregory starts checking his watch repeatedly"
  },
  "phase_3": {
    "character": null,
    "trigger": "revelation-forgery-ring AND revelation-victoria-desperate",
    "description": "Gregory attempts to leave the train; Maxwell has a complete breakdown"
  }
}
```

**DIVERGENCE ALERT:** The `scripted_events` structure is completely different from the `solvability` structure in the streamlined template. It uses a simplified trigger-based approach rather than the more complex progression gate system.

### Major Implementation Divergences Summary

1. **Whereabouts Format**: Uses key-value pairs (`{"key": "0", "value": {...}}`) rather than direct objects with start/end times.

2. **Connections Structure**: Implemented as an array of objects rather than an object with connection IDs as keys:
   ```json
   // Implemented:
   "connections": [{"source": "X", "target": "Y", "type": "Z"}, ...]
   
   // Original template:
   "connections": {"connection_id": {"source": "X", "target": "Y", "type": "Z"}, ...}
   ```

3. **Key Testimonies**: Added directly to character objects rather than being part of constellation nodes as originally intended.

4. **Scripted Events**: Uses a simplified trigger-based system instead of the more complex progression gates in the original specification:
   ```json
   // Implemented:
   "scripted_events": {"event_id": {"trigger": "condition", "description": "..."}, ...}
   
   // Original template:
   "solvability": {
     "starting_nodes": [...],
     "solution_node": "node_id",
     "progression_gates": [...]
   }
   ```

5. **Property Naming**: Several properties have been renamed or structured differently:
   - `characters` instead of `related_characters` 
   - `can_pickup` instead of `can_be_picked_up`
   - `description` field for evidence instead of `physical_description`

6. **New Fields**: 
   - `gender` field added to `mind_engine` section for character sprite assignment
   - Values are "F" for female characters or "M" for male characters

### Implementation Considerations

When working with these files, your system will need to:

1. **Handle Inconsistent Formats**: Your parser must be flexible enough to handle both the intended format from the spec and the actual format found in implemented files.

2. **Validate Mixed Structures**: Some parts follow object-based indexing while others use arrays - your validation must account for this mixture.

3. **Transform Data**: Consider building a normalization layer that standardizes these files to a consistent format before processing.

4. **Migration Path**: If you're updating existing content, you'll need a migration strategy that either updates all existing files or maintains backward compatibility with both formats.

5. **Consistency Enforcement**: Going forward, decide on one format and strictly enforce it to prevent further divergence.

6. **Gender Field Handling**: The parsing system must extract and preserve the gender field for sprite assignment.

7. **Fallback Strategy**: For characters without an explicit gender field, the system should default to "F" (female).

### Character System
- **Character.cs**: Base class for NPCs with LLM integration
- **NPCManager.cs**: Manages NPC spawning and initialization
- **NPCMovement.cs**: Controls NPC movement and pathfinding
- **DialogueControl.cs**: Handles dialogue interaction and UI
- **CharacterProfile.cs**: Structured character data with two main components:
  - **CoreProfile**: Mystery-relevant data (relationships, whereabouts, involvement)
  - **MindEngine**: Personality and behavior (identity, state of mind, speech patterns)
- **CharacterProfileLoader.cs**: Loads and validates character profile JSON

### Player System
- **PlayerMovement.cs**: Controls player character movement
- **CarDetection.cs**: Detects player entry into different train cars

### Train Environment System
- **TrainLayoutManager.cs**: Handles procedural generation of train environment
- **TrainCarTemplate.cs**: Template definition for different car types
- **TrainCarInstance.cs**: Runtime instantiation of train car with contents
- **PointOfInterest.cs**: Interactive locations within train cars

### UI System
- **MinigameControl.cs**: Base class for mini-games (evidence, puzzles)
- **FinalSubmission.cs**: Handles end-game accusation and win/loss state
- **MainMenu.cs**: Main menu UI management
- **NodeNotif.cs**: Notification pop-up when new nodes are discovered

## Mystery Board & Node Inspection

### Key Components
- **MysteryNavigation.cs**: Controls camera movement and zoom for the mystery board
- **VisualNode.cs**: Base class for interactable node visualizations with:
  - Drag functionality (left mouse button)
  - Inspection activation (right mouse button)
  - Node discovery and connection management
- **Connection.cs**: Visual line between related nodes showing relationship type
- **EvidenceInspect.cs**: Detailed node inspection panel showing:
  - Node type, category, and content
  - Optional description, location, time
  - Associated characters
  - Hidden details when discovered
- **NodeNotif.cs**: Notification animation when new nodes are unlocked

### Node Types
- **Fact**: Established truth about the mystery
- **Evidence**: Physical or tangible clue requiring examination
- **Testimony**: Character statement that may contain truths or falsehoods
- **Lead**: Unanswered question driving investigation forward
- **Barrier**: Progress obstacle requiring specific solution
- **Deduction**: Player-formed connection between existing evidence
- **Revelation**: Key turning point in investigation
- Custom node visualizations based on node type and category

### Connection Types
- **Reveals**: Direct exposure of new information
- **Suggests**: Implies possible connection
- **Contradicts**: Conflicts with other evidence
- **Confirms**: Supports other evidence
- **Implies**: Indirectly suggests connection
- **Unlocks**: Removes barrier to progress
- Visual presentation varies based on connection type and strength

### Player Scoring System
- Player's mystery board is compared to the "blueprint board" that was used to generate the mystery
- Scoring evaluates:
  - Correct nodes discovered (percentage of total available)
  - Correct connections made between nodes
  - Correct identification of key players (victim, culprit, witnesses)
  - Accurate deductions made based on evidence
- End-of-game results screen displays score breakdown and missed elements
- Higher scores unlock additional gameplay features and mysteries

### Node Discovery System
1. Nodes start in undiscovered state and remain hidden
2. When a node is discovered, it calls `DiscoverNode()` on its visual object
3. Connected nodes are checked for discovery eligibility
4. Visual connections appear when both connecting nodes are discovered
5. User receives notification via NodeNotif when a new node is discovered
6. Discovered nodes can be dragged for organization and inspected for details

### Node Inspection Flow
1. Right-click on a node activates the inspection panel
2. EvidenceInspect fetches node data and populates UI sections
3. Only relevant sections are displayed based on available node data
4. Users can examine node connections to understand relationships
5. UI provides contextual details about evidence, time, location, and characters

## LLM Dialogue System

### Key Components
- **LLMUnity Framework**: Core integration with large language models
- **LLMCharacter.cs**: Connects Unity characters with LLM capabilities
- **CharacterManager.cs**: Manages character lifecycle and state transitions
- **BaseDialogueManager.cs**: Abstract base class for dialogue management
- **LLMDialogueManager.cs**: Concrete implementation handling dialogue UI and flow
- **CharacterPromptGenerator.cs**: Converts JSON data to LLM prompts

### Character Definition and Management
Characters are defined in JSON files with structured fields:
- **Core**: Demographics, personality, speech patterns, backstory
  - **Involvement**: Role in mystery, type, and relevant attributes
  - **Whereabouts**: Movement record with locations and times
  - **Relationships**: Connections to other characters
  - **Agenda**: Goals and motivations
- **Mind Engine**: Goals, reasoning style, vulnerabilities 
  - **Identity**: Name, occupation, personality (OCEAN model)
  - **State of Mind**: Current worries, feelings, reasoning style
  - **Social Mechanics**: Affinity/antipathy triggers, vulnerabilities
  - **Speech Patterns**: Vocabulary, sentence style, quirks
- **Case Info**: Memory of events, relationships, leads, uncertainties

CharacterManager handles:
- Two-phase initialization (loading templates, warming up LLM)
- Character state transitions (Uninitialized→LoadingTemplate→WarmingUp→Ready)
- Context window allocation between characters
- Switching between active characters
- Character reset and cleanup operations

### Dialogue System Architecture
1. **BaseDialogueManager**: Abstract class defining core dialogue functionality
   - Manages response streaming and processing
   - Handles errors and dialogue state
   - Provides abstract methods for UI-specific implementations

2. **LLMDialogueManager**: Concrete implementation
   - Handles UI elements for dialogue display
   - Processes user input submission
   - Manages dialogue activation/deactivation
   - Connects with LLMCharacter for LLM interactions

3. **CharacterPromptGenerator**: Static utility class
   - Parses character JSON data into structured objects
   - Generates formatted system prompts for LLM context
   - Includes character identity, traits, goals, memories, and relationships

### Dialogue Flow
1. Player initiates dialogue with NPC
2. DialogueControl activates character's LLMCharacter component
3. LLMDialogueManager processes user input via UI
4. Input is sent to LLM with character context via LLMCharacter.Chat()
5. Response is streamed back character-by-character with HandleReply()
6. UI is updated incrementally as response arrives
7. Dialogue state is maintained in conversation history

### LLM Configuration
- Temperature: 0.8 (default for characters)
- Top-K sampling: 55 tokens
- Top-P sampling: 0.9
- Repeat penalty: 1.0
- Frequency penalty: 1.0
- Context preservation: Custom windowing for character memory
- Streaming: Enabled for natural conversation flow

## Mystery Generation System

### Generation Process Overview
1. **Template Selection**: Choose mystery template (currently focused on murder mysteries)
2. **Theme Selection**: Select thematic elements for consistent narrative
3. **Motive Generation**: Determine compelling reasons for the crime
4. **Character Assignment**: Select victim, culprit, and supporting characters
5. **Crime Generation**: Define method, circumstances, and culprit's plan
6. **Constellation Construction**: Build solvable network of evidence and connections
   - Start with core facts and final solution
   - Create multiple valid paths to solution
   - Add appropriate red herrings and misdirections
7. **Validation**: Ensure mystery is solvable and narratively coherent

### Mystery Template Components
- **Core**: Fundamental mystery information (type, victim, culprit, method)
- **Characters**: Essential roles (victim, culprit, witnesses, red herrings)
- **Environment**: Train layout and significant locations
- **Constellation**: Structure of evidence and how it connects

### Narrative Coherence Rules
- All character motivations must be psychologically plausible
- Evidence must form multiple coherent paths to solution
- Time and location constraints must be logically consistent
- Red herrings must be plausible but ultimately discardable

## State Management Flow
The game uses a central state machine managed by GameControl.cs with these states:
- **DEFAULT**: Normal gameplay/exploration
- **DIALOGUE**: Active NPC conversation
- **PAUSE**: Game paused
- **FINAL**: Final accusation stage
- **WIN/LOSE**: End game states
- **MINIGAME**: Active puzzle interaction
- **MYSTERY**: Viewing mystery board

State transitions are handled by:
- DialogueControl.cs (DIALOGUE state)
- MinigameControl.cs (MINIGAME state)
- FinalSubmission.cs (WIN/LOSE states)
- PauseMenu.cs (PAUSE state)

## Class Architecture

### Core Hierarchy
- **GameControl** (Singleton)
  - Contains **Mystery**: Core mystery data
    - Contains **MysteryConstellation**: Network of evidence
      - Contains **MysteryNodes**: Individual evidence pieces

### Mystery Hierarchy
- **MysteryData** (Root container)
  - **Metadata**: Basic information
  - **Core**: Core mystery setup
  - **CharacterProfiles**: Character data
    - **CoreProfile**: Mystery-relevant data
    - **MindEngine**: Personality and behavior
  - **TrainLayout**: Physical environment
    - **Cars**: Train car definitions
    - **PointsOfInterest**: Interactive locations
  - **Constellation**: Mystery solution structure
    - **Nodes**: Individual pieces of evidence/information
    - **Edges**: Connections between nodes
    - **MiniMysteries**: Sub-investigations

### NPC System
- **NPCManager** manages all NPCs
  - Spawns **Character** objects
    - Contains **LLMCharacter**: LLM dialogue integration
    - Contains **NPCMovement**: Movement behavior
    - Contains **NPCAnimManager**: Animation control

### UI System
Multiple controllers for different states:
- **DialogueControl**: Dialogue UI
- **MinigameControl**: Parent for mini-games
  - **LuggageControl**: Luggage puzzle
  - **EvidenceControl**: Evidence examination
- **FinalSubmission**: End-game accusation UI

### Mystery Visualization System
- **MysteryNavigation**: Controls mystery board camera
  - Contains visual nodes for each mystery element
    - **VisualNode**: Base class for node visualization
      - Extended by specialized node types
    - **Connection**: Visual links between related nodes
  - **EvidenceInspect**: Detail panel for node examination