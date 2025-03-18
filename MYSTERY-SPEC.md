# Mystery Data Model Specification: Actual Implementation

This specification documents the *actual implemented format* of mystery files as observed in `transformed-mystery.json`, noting where it diverges from the original design specification.

## Core Data Structure

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

## Detailed Field Specifications

### 1. Metadata
Simple key-value collection with basic information about the mystery:
```json
"metadata": {
  "id": "art_forgery_murder",
  "version": "1.0",
  "last_updated": "2025-03-05 11:30:46",
  "title": "Art Fraud on the Fashion Express"
}
```

### 2. Core Mystery Setup
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

### 3. Characters

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

#### 3.1 Character Core Data
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

#### 3.2 Mind Engine Data
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
  }
}
```

#### 3.3 Key Testimonies
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

### 4. Environment (Train Layout)

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

### 5. Constellation (Mystery Structure)

#### 5.1 Nodes
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

#### 5.2 Connections
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

#### 5.3 Mini Mysteries
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

#### 5.4 Scripted Events
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

## Major Implementation Divergences Summary

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

## Implementation Considerations

When working with these files, your system will need to:

1. **Handle Inconsistent Formats**: Your parser must be flexible enough to handle both the intended format from the spec and the actual format found in implemented files.

2. **Validate Mixed Structures**: Some parts follow object-based indexing while others use arrays - your validation must account for this mixture.

3. **Transform Data**: Consider building a normalization layer that standardizes these files to a consistent format before processing.

4. **Migration Path**: If you're updating existing content, you'll need a migration strategy that either updates all existing files or maintains backward compatibility with both formats.

5. **Consistency Enforcement**: Going forward, decide on one format and strictly enforce it to prevent further divergence.
