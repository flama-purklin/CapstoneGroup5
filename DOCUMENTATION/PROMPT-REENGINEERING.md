# Prompt Re-engineering & Revelations Integration

## Goal
This task focuses on evolving the character interaction system by:
1.  Integrating the `revelations` data structure into the mystery JSON and C# data models.
2.  Removing the `mystery_attributes` field from the JSON and C# models, replacing its role with `revelations`.
3.  Performing a holistic redesign of the system prompt generation logic in `CharacterPromptGenerator.cs` to effectively utilize `revelations` and other character data.
4.  Incorporating function calling capabilities into the prompt structure, enabling characters to call `end_conversation` and `announce_evidence_revelation` as defined in `FUNCTION-CALLING.md`.

## Context
- The core JSON structure (`character_profiles`, dictionaries for whereabouts/relationships, `initial_location` placement) and C# code were previously updated for compatibility.
- `mystery_attributes` will now be fully replaced by `revelations`.
- The prompt generation logic requires significant updates to leverage `revelations` and support function calls.

## `revelations` Data Structure
This field should be added inside the `core` object for each character profile in `transformed-mystery.json`. The `mystery_attributes` field should be removed concurrently.

```json
// Example structure within a character profile's "core" object:
// "mystery_attributes": [...] <-- REMOVE THIS FIELD
"revelations": {
  // Key: Unique ID for the revelation
  "revelation_id_1": { 
    "content": "The actual text the character might say or imply.",
    "reveals": "node_id_or_fact_revealed", // Link to a node in the constellation
    "trigger_type": "conversation_topic | evidence_presentation | character_interaction | state_change", // How is this revealed?
    "trigger_value": "specific_topic_keyword | evidence_id | character_id | required_state", // Value associated with the trigger
    "accessibility": "easy | medium | hard | plot_critical" // Difficulty for player to uncover
  },
  "revelation_id_2": { ... } 
},
```

### Full `revelations` Data (from reference `character-profiles.json`)

**Maxwell Porter:**
```json
        "revelations": {
          "maxwell_paranoia": {
            "content": "I can feel them watching me... Victoria hired them. That investigator taking notes... they're closing in.",
            "reveals": "lead-maxwell-paranoia",
            "trigger_type": "conversation_topic",
            "trigger_value": "victoria_investigation",
            "accessibility": "medium"
          },
          "maxwell_sedative": {
            "content": "Artists like me... we need steady hands. Special techniques, special... tools. Gregory understands.",
            "reveals": "evidence-sedative",
            "trigger_type": "evidence_presentation",
            "trigger_value": "art_tools",
            "accessibility": "hard"
          }
        },
```

**Gregory Crowe:**
```json
        "revelations": {
          "gregory_alibi": {
            "content": "At the time in question, I was in the dining car discussing authentication techniques with some colleagues. Several passengers can confirm my whereabouts.",
            "reveals": "lead-gregory-alibi",
            "trigger_type": "conversation_topic",
            "trigger_value": "murder_time",
            "accessibility": "medium"
          },
          "gregory_forgery_knowledge": {
            "content": "In my extensive work on forgery detection, I've found that the key is in the binding agents. Modern forgers never get the chemical composition quite right.",
            "reveals": "lead-forgery-techniques",
            "trigger_type": "evidence_presentation",
            "trigger_value": "art_authentication",
            "accessibility": "hard"
          }
        },
```

**Victoria Blackwood:**
```json
        "revelations": {},
```

**Eleanor Verne:**
```json
        "revelations": {
          "eleanor_maxwell_skills": {
            "content": "Maxwell has an extraordinary eye for detail and technical skill. His ability to mimic artistic styles is... well, it's remarkable. I've seen him perfectly recreate a Monet brushstroke technique during a demonstration at Gregory's gallery.",
            "reveals": "testimony-maxwell-artistic-ability",
            "trigger_type": "conversation_topic",
            "trigger_value": "maxwell_introduction",
            "accessibility": "medium"
          }
        },
```

**Nova Winchester:**
```json
        "revelations": {
          "nova_sighting": {
            "content": "Yeah, I saw two blokes enter the lounge car right before I found the blood. One was that art dealer with the ridiculous pocket square, the other was some jumpy artist type. Didn't think much of it at the time.",
            "reveals": "testimony-two-men",
            "trigger_type": "conversation_topic",
            "trigger_value": "murder_scene",
            "accessibility": "medium"
          }
        },
```

**Penelope Valor:**
```json
        "revelations": {
          "penelope_argument": {
            "content": "Yes, Victoria and I had words in the lounge car. It was a professional disagreement about designer exclusivity contracts, nothing more.",
            "reveals": "testimony-victoria-penelope-argument",
            "trigger_type": "conversation_topic",
            "trigger_value": "victoria_argument",
            "accessibility": "easy"
          },
          "penelope_relationship": {
            "content": "Victoria threatened to expose my relationship with Nova as some kind of industry scandal. She was desperate for attention, trying to save her failing magazine.",
            "reveals": "testimony-victoria-threats",
            "trigger_type": "evidence_presentation",
            "trigger_value": "nova_relationship_hint",
            "accessibility": "hard"
          }
        },
```

**Gideon Marsh:**
```json
        "revelations": {
          "gideon_investigation": {
            "content": "I was hired to investigate Victoria's connections to art fraud, but not by her. Mr. Seol engaged my services without Ms. Valor's knowledge.",
            "reveals": "testimony-gideon-hired",
            "trigger_type": "evidence_presentation",
            "trigger_value": "timmy_connection",
            "accessibility": "hard"
          }
        },
```

**Mira Sanchez:**
```json
        "revelations": {
          "mira_photos": {
            "content": "I might have some... relevant photos on my camera. But this is exclusive material for my story. What's it worth to you?",
            "reveals": "barrier-camera",
            "trigger_type": "conversation_topic",
            "trigger_value": "murder_evidence",
            "accessibility": "medium"
          }
        },
```

**Timmy Seol:**
```json
        "revelations": {
          "timmy_hired_gideon": {
            "content": "Please don't tell Penelope, but I hired Mr. Marsh to investigate Victoria! I thought if we could prove she was involved in those art forgeries, it would protect Penelope's magazine from Victoria's threats.",
            "reveals": "testimony-timmy-hired-gideon",
            "trigger_type": "evidence_presentation",
            "trigger_value": "gideon_role",
            "accessibility": "medium"
          }
        },
```

## Function Calling Reference
Refer to `DOCUMENTATION/FUNCTION-CALLING.md` for the specific function definitions (`end_conversation`, `announce_evidence_revelation`) and required formatting to be integrated into the system prompt.

## Implementation Steps

1.  **Update JSON:** Add the `revelations` field (using the data above) to each character profile within `character_profiles` in `Assets/StreamingAssets/MysteryStorage/transformed-mystery.json`. Remove the `mystery_attributes` field from each character's `involvement` object.
2.  **Update C# Data Models:**
    *   Define a `Revelation` class (and any nested classes needed for triggers, etc.) in `MysteryCharacter.cs`.
    *   Add a `public Dictionary<string, Revelation> Revelations { get; set; }` property to the `CharacterCore` class.
    *   Remove the `MysteryAttributes` property from the `Involvement` class. Remove the `MysteryAttribute` class definition.
3.  **Redesign Prompt Generation (`CharacterPromptGenerator.cs`):**
    *   Remove the logic that processes `mystery_attributes`.
    *   Collaboratively design and implement new logic to incorporate `revelations` data meaningfully into the system prompt. Consider how trigger types, accessibility, and revealed nodes should influence the prompt.
    *   Integrate the function calling definitions from `FUNCTION-CALLING.md` into the prompt structure.
    *   Perform a holistic review and refinement of the entire prompt generation process based on the new data and function calling requirements.
4.  **Update Function Calling Handler (If necessary):** Ensure the system receiving function calls (likely within LLMUnity or related scripts) can handle `end_conversation` and `announce_evidence_revelation`. *(This might be outside the scope of prompt generation but needs consideration)*.
5.  **Testing & Iteration:** Thoroughly test NPC dialogue, ensuring `revelations` are used appropriately, function calls work as expected, and the overall character persona is consistent and effective. Debug and refine prompts and logic based on test results.
6.  **Documentation:** Update `full-technical-documentation.md` and any other relevant documents.
