# Mystery JSON Structure Specification

This document outlines the structural components of the mystery data format, explaining the purpose and organization of each section without specific implementation details.

## Mock JSON Structure (Commented)

```json
{
  // Unique identifier and versioning information
  "mystery_id": "string",  // Unique identifier for this mystery
  "version": "string",     // Version number (e.g., "1.0")
  "last_updated": "string", // Timestamp of last update

  // Detailed character information including personality, relationships, whereabouts, etc.
  "character_profiles": {
    "character_id": {  // Each character has its own entry keyed by a unique ID
      "core": {
        "involvement": {
          "role": "string",     // Character's role in the mystery (e.g., "Murderer", "Victim")
          "type": "string",     // Subtype of the role
          "mystery_attributes": ["string"] // Key character traits relevant to the mystery
        },
        "whereabouts": [  // Timeline of the character's locations and actions
          {
            "key": "string",    // Time period identifier
            "value": {
              "circumstance": "string", // Location and timeframe
              "action": "string",       // What the character was doing
              "events": ["string"]      // Significant events during this period
            }
          }
        ],
        "relationships": [  // Character's connections to other characters
          {
            "key": "string",     // Name of the related character
            "value": {
              "attitude": "string",     // Emotional stance toward other character
              "history": ["string"],    // Shared history points
              "known_secrets": ["string"] // Secrets known about other character
            }
          }
        ],
        "agenda": {
          "primary_goal": "string",    // Main objective
          "secondary_goal": "string"   // Secondary objective
        }
      },
      "mind_engine": {  // Character personality and behavioral data
        "identity": {
          "name": "string",       // Character's full name
          "occupation": "string", // Professional role
          "personality": {        // OCEAN model values
            "O": number,  // Openness (0.0-1.0)
            "C": number,  // Conscientiousness (0.0-1.0)
            "E": number,  // Extraversion (0.0-1.0)
            "A": number,  // Agreeableness (0.0-1.0)
            "N": number   // Neuroticism (0.0-1.0)
          }
        },
        "state_of_mind": {
          "worries": "string",     // Primary concern
          "feelings": "string",    // Emotional state
          "reasoning_style": "string" // Cognitive approach
        },
        "social_mechanics": {
          "affinity_triggers": ["string"],  // What makes character like someone
          "antipathy_triggers": ["string"], // What makes character dislike someone
          "vulnerabilities": ["string"]     // Psychological/social weaknesses
        },
        "speech_patterns": {
          "vocabulary_level": "string",    // Language complexity
          "sentence_style": ["string"],    // Speech structure
          "speech_quirks": ["string"],     // Distinctive verbal habits
          "common_phrases": ["string"]     // Recurring expressions
        }
      }
    }
  },

  // Core mystery details
  "core": {
    "type": "string",      // Mystery category (e.g., "Migratory Route")
    "subtype": "string",   // Specific variant (e.g., "Fashion Week")
    "theme": "string",     // Central narrative motif
    "victim": {
      "character_id": "string"  // Reference to victim character
    },
    "culprit": {
      "character_id": "string"  // Reference to culprit character
    },
    "manipulator": {
      "character_id": "string"  // Reference to manipulator character (optional)
    },
    "method": "string",    // How the crime was committed
    "motive": "string",    // Reason for the crime
    "circumstance": {
      "location_id": "string",  // Where crime occurred
      "time_minutes": number,   // When crime occurred (minutes into journey)
      "details": "string"       // Description of circumstances
    }
  },

  // Physical environment details
  "train_layout": {
    "cars": [  // Array of train cars
      {
        "id": "string",           // Unique identifier for car
        "name": "string",         // Display name
        "description": "string",  // Textual description
        "accessible": boolean,    // Whether player can access
        "points_of_interest": [   // Locations within car
          {
            "id": "string",               // Unique identifier
            "name": "string",             // Display name
            "description": "string",      // Textual description
            "associated_character": "string", // Optional reference to character
            "initial_state": "string",    // Optional state (e.g., "locked")
            "evidence_connection": "string", // Optional evidence relevance
            "evidence_items": ["string"]  // Optional array of evidence IDs
          }
        ]
      }
    ],
    "layout_order": ["string"]  // Array of car IDs defining sequential ordering
  },

  // Character gameplay data
  "characters": {
    "involved": [  // Array of character gameplay data
      {
        "character_id": "string",         // Reference to character profile
        "role": "string",                 // Investigation role
        "type": "string",                 // Role subtype
        "investigation_difficulty": number, // Interrogation challenge (0.0-1.0)
        "initial_location": "string",     // Reference to train location
        "starting_attitude": "string",    // Initial disposition toward player
        "key_testimonies": [              // Extractable information
          {
            "id": "string",                 // Unique identifier
            "content": "string",            // Actual testimony text
            "truthfulness": number,         // Reliability (0.0-1.0)
            "reveals_node_id": "string",    // Reference to node in constellation
            "requires": {                   // Conditions for testimony extraction
              "player_knowledge": ["string"], // Knowledge required
              "character_state": "string"     // Required character emotional state
            },
            "extraction_methods": ["string"] // Techniques to obtain testimony
          }
        ]
      }
    ]
  },

  // Mystery solution structure
  "constellation": {
    "nodes": [  // Array of knowledge units
      {
        "id": "string",                  // Unique identifier
        "type": "string",                // Knowledge type (FACT, EVIDENCE, TESTIMONY, etc.)
        "category": "string",            // Organizational grouping (MAIN_MYSTERY, MINI_MYSTERY_A, etc.)
        "content": "string",             // Descriptive text
        "discovered": boolean,           // Initial visibility
        "location_id": "string",         // Optional reference to train location
        "time_minutes": number,          // Optional temporal reference
        "related_characters": ["string"], // Array of character IDs
        "visual_properties": {           // Display attributes
          "icon": "string",              // Visual representation
          "color": "string"              // Color code (typically hex)
        },
        
        // Type-specific properties (only one of these will be present per node)
        "evidence_properties": {         // For EVIDENCE type
          "physical_description": "string", // Visual appearance
          "hidden_details": ["string"],    // Details revealed on inspection
          "can_be_picked_up": boolean,     // Whether item can be collected
          "examination_requirements": {     // Skills/tools needed
            "skills": ["string"],
            "tools": ["string"]
          }
        },
        "testimony_properties": {        // For TESTIMONY type
          "truthfulness": number,           // Reliability score (0.0-1.0)
          "delivery_style": "string",       // How testimony is communicated
          "extraction_difficulty": number,  // Challenge level (0.0-1.0)
          "prerequisites": ["string"]       // Node IDs required to unlock
        },
        "barrier_properties": {          // For BARRIER type
          "solution": "string",             // How to overcome barrier
          "blocking_access": ["string"],    // Node IDs blocked
          "solution_methods": [             // Solution requirements
            {
              "type": "string",            // Method type (e.g., "password")
              "hints": ["string"],         // Clues for solution
              "answer": "string"           // Correct solution
            }
          ]
        }
      }
    ],
    "edges": [  // Logical connections between nodes
      {
        "id": "string",                  // Unique identifier
        "source_node_id": "string",      // Origin node reference
        "target_node_id": "string",      // Destination node reference
        "type": "string",                // Connection type (REVEALS, SUGGESTS, etc.)
        "strength": number,              // Connection confidence (0.0-1.0)
        "is_hidden": boolean,            // Initial visibility
        "discovery_condition": "string"  // Optional text describing unlock condition
      }
    ],
    "minimysteries": [  // Sub-investigations
      {
        "id": "string",                  // Unique identifier
        "name": "string",                // Display name
        "description": "string",         // Textual description
        "entry_node_ids": ["string"],    // Starting node references
        "revelation_node_id": "string",  // Solution node reference
        "connects_to_main_mystery": ["string"] // Connections to main case
      }
    ],
    "story_phases": [  // Progression gates
      {
        "phase": number,                // Numeric identifier
        "required_node_ids": ["string"], // Nodes needed to advance
        "trigger_events": ["string"]     // Game events activated
      }
    ]
  }
}
```

## Key Data Relationships

1. **Character References**
   - Character profiles are referenced by ID in the core mystery setup
   - Characters are linked to locations through whereabouts and initial_location
   - Characters are connected to evidence and testimonies in the constellation

2. **Location References**
   - Train layout defines all physical locations in the mystery
   - Locations are referenced in evidence, character whereabouts, and the crime circumstance

3. **Knowledge Structure**
   - Nodes represent different types of information (facts, evidence, testimonies, etc.)
   - Edges create logical connections between nodes, forming the investigative path
   - Mini-mysteries are sub-plots that connect to the main mystery
   - Story phases control the progression of the mystery

4. **Investigation Mechanics**
   - Character testimonies unlock nodes in the constellation
   - Evidence examination reveals hidden details
   - Barriers block access to certain nodes until solved

This structure allows for complex mystery narratives with interconnected characters, locations, and evidence that players can investigate through various gameplay mechanics.
