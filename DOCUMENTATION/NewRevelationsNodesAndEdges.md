### Revised `revelations` Sections (Formatted for JSON)

**For `character_profiles.maxwell_porter.core`:**

```json
        "revelations": {
          "maxwell_deflects_gideon_topic": {
            "content": "That man? Marsh? Just... observing. Makes one uneasy... when people watch so intently.",
            "reveals": "lead-investigator-purpose",
            "trigger_type": "conversation_topic",
            "trigger_value": "Asking about Gideon Marsh or being watched",
            "accessibility": "medium"
          },
          "maxwell_hints_sedative_knowledge": {
            "content": "Steady hands... critical for detail. Certain... substances... artists discuss them. Theoretical aids for precision.",
            "reveals": "testimony-sedative-art-use", 
            "trigger_type": "evidence_presentation",
            "trigger_value": "evidence-sedative", 
            "accessibility": "hard"
          },
          "maxwell_downplays_gregory_meeting": {
            "content": "Mr. Crowe? We spoke briefly. About art, naturally. The Pylsian masters... Nothing significant.",
            "reveals": "lead-two-men", 
            "trigger_type": "conversation_topic",
            "trigger_value": "Asking about his meeting with Gregory before the murder",
            "accessibility": "medium"
          },
          "maxwell_redirects_disposal_query": {
            "content": "Waste? The train staff handles all that. Kitchen car, I suppose. They have procedures.",
            "reveals": "lead-check-incinerator", 
            "trigger_type": "conversation_topic",
            "trigger_value": "Asking about medical waste or syringe disposal",
            "accessibility": "medium"
          }
        }
```

**For `character_profiles.gregory_crowe.core`:**

```json
        "revelations": {
          "gregory_establishes_alibi_via_expertise": {
            "content": "Tragic. Fortuitously, I was preoccupied discussing Pylsian authentication – binding agents! – in the dining car during the critical time. Colleagues can attest.",
            "reveals": "testimony-gregory-forgery-interest", 
            "trigger_type": "conversation_topic",
            "trigger_value": "Asking about his whereabouts during the murder",
            "accessibility": "easy"
          },
          "gregory_critiques_note_as_expert": {
            "content": "Hmm, it *mimics* Victoria's style, but perhaps *too* neat? Genuine distress often affects penmanship. Interesting.",
            "reveals": "lead-suicide-question", 
            "trigger_type": "evidence_presentation",
            "trigger_value": "evidence-suicide-note",
            "accessibility": "medium"
          },
          "gregory_dismisses_maxwell_reliability": {
            "content": "Maxwell? Brilliant artist, undeniably. But his nerves... his perceptions can be somewhat... heightened. I wouldn't rely solely on his account.",
            "reveals": "deduction-maxwell-paranoia", 
            "trigger_type": "conversation_topic",
            "trigger_value": "Mentioning Maxwell acting strangely or accusingly",
            "accessibility": "medium"
          },
          "gregory_denies_specific_manipulation": {
            "content": "Me, mention photos? Good heavens, no. A misunderstanding on his part. I merely noted Victoria speaking with Mr. Marsh. Maxwell does get easily flustered.",
            "reveals": "deduction-maxwell-paranoia", 
            "trigger_type": "conversation_topic", 
            "trigger_value": "Accusing Gregory of manipulating Maxwell about photos",
            "accessibility": "hard"
          }
        }
```

**For `character_profiles.nova_winchester.core`:**

```json
        "revelations": {
          "nova_describes_two_men_sighting": {
            "content": "Yeah, saw two blokes head into the lounge car right before... well, before. Crowe, the dealer. And some twitchy artist type. Got deadlines, didn't care.",
            "reveals": "testimony-two-men",
            "trigger_type": "conversation_topic",
            "trigger_value": "Asking about observations near the lounge car before the incident",
            "accessibility": "medium"
          },
          "nova_confirms_victoria_threat_dismissively": {
            "content": "That old hag? Yeah, threatened rubbish about me and Pen-- Ms. Valor. Headlines. Anything for attention. Bloody desperate.",
            "reveals": "testimony-victoria-threats",
            "trigger_type": "evidence_presentation",
            "trigger_value": "evidence-financial-records",
            "accessibility": "medium"
          },
          "nova_reacts_to_relationship_leverage_aggressively": {
            "content": "Fuck off, mate. Ours is none of your bloody business. Trying blackmail? Piss off.",
            "reveals": "testimony-victoria-threats", 
            "trigger_type": "conversation_topic",
            "trigger_value": "Accusing or directly mentioning her relationship with Penelope",
            "accessibility": "hard"
          }
        }
```

**For `character_profiles.eleanor_verne.core`:**

```json
        "revelations": {
          "eleanor_confirms_maxwell_artistic_skill": {
            "content": "Maxwell Porter's skill is... undeniable. His mimicry of Pylsian brushwork is remarkable. Unsettlingly precise.",
            "reveals": "testimony-maxwell-artistic-ability",
            "trigger_type": "conversation_topic",
            "trigger_value": "Asking about Maxwell Porter's art or skills",
            "accessibility": "medium"
          },
          "eleanor_reveals_gregory_forgery_interest": {
            "content": "Mr. Crowe has taken a keen interest in forgery detection since I joined. We discuss authentication weaknesses often. He has... deep understanding.",
            "reveals": "testimony-gregory-forgery-interest",
            "trigger_type": "conversation_topic",
            "trigger_value": "Asking about Gregory Crowe or art forgery",
            "accessibility": "medium"
          },
          "eleanor_analyzes_note_quality": {
            "content": "This is very close to Victoria's hand. But the consistency... under duress, one expects variation. This feels... studied. Almost *too* perfect.",
            "reveals": "deduction-forged-note",
            "trigger_type": "evidence_presentation",
            "trigger_value": "evidence-suicide-note",
            "accessibility": "hard"
          },
          "eleanor_expresses_mixed_feelings": {
            "content": "Victoria... didn't deserve this. She distanced herself after the scandal... understandable. Working for Gregory now feels... complicated.",
            "reveals": "lead-forgery-connection", 
            "trigger_type": "conversation_topic",
            "trigger_value": "Asking about her relationship with Victoria or Gregory",
            "accessibility": "easy"
          }
        }
```

**For `character_profiles.timmy_seol.core`:**

```json
        "revelations": {
          "timmy_confesses_hiring_gideon": {
            "content": "*Whispering* Please don't tell Ms. Valor! Yes, I hired Mr. Marsh. Victoria was threatening her... I thought if he could find dirt on Victoria... about forgery... it would protect Penelope!",
            "reveals": "testimony-timmy-hired-gideon",
            "trigger_type": "evidence_presentation",
            "trigger_value": "testimony-gideon-role", 
            "accessibility": "medium"
          },
          "timmy_gossips_about_penelope_nova_threat": {
            "content": "Ms. Valor and Nova? Oh, incredibly close! Penelope is *so* protective. Victoria was horrid, threatening them personally...",
            "reveals": "testimony-victoria-threats",
            "trigger_type": "conversation_topic",
            "trigger_value": "Asking about Penelope and Nova's relationship",
            "accessibility": "medium"
          },
          "timmy_reports_seeing_syringe": {
            "content": "Oh gosh, earlier I bumped into Mr. Porter! Spilled coffee... he seemed so jumpy. And I'm almost sure I saw something shiny... like a syringe needle... in his pocket! Maybe?",
            "reveals": "testimony-timmy-saw-syringe", 
            "trigger_type": "conversation_topic",
            "trigger_value": "Asking about Maxwell Porter or seeing anything unusual",
            "accessibility": "hard"
          }
        }
```

**For `character_profiles.gideon_marsh.core`:**

```json
        "revelations": {
          "gideon_states_official_purpose": {
            "content": "My purpose is professional. Investigating potential art fraud connections related to Ms. Blackwood's circle.",
            "reveals": "testimony-gideon-role",
            "trigger_type": "conversation_topic",
            "trigger_value": "Asking about his purpose on the train",
            "accessibility": "easy"
          },
          "gideon_reveals_client_when_pressed": {
            "content": "My client requires discretion. However... It was Mr. Timmy Seol, acting independently.",
            "reveals": "testimony-timmy-hired-gideon", 
            "trigger_type": "evidence_presentation",
            "trigger_value": "testimony-timmy-hired-gideon", 
            "accessibility": "hard"
          },
          "gideon_shares_observation_maxwell_fear": {
            "content": "Mr. Porter reacted... strongly when our paths crossed. Visible apprehension. Unusual.",
            "reveals": "lead-maxwell-paranoia",
            "trigger_type": "conversation_topic",
            "trigger_value": "Asking about Maxwell Porter's behavior",
            "accessibility": "medium"
          }
        }
```

**For `character_profiles.mira_sanchez.core`:**

```json
        "revelations": {
          "mira_offers_photos_for_info": {
            "content": "Photos? Oh, I have *plenty*. Caught the argument, faces near the lounge... But info isn't free. What exclusive can *you* offer *me*?",
            "reveals": "barrier-camera", 
            "trigger_type": "conversation_topic",
            "trigger_value": "Asking about photos or evidence she might have",
            "accessibility": "easy"
          },
          "mira_testifies_photo_content": {
            "content": "Okay, look - timestamped. Vicky/Penny arguing. Then, Crowe and the twitchy artist heading into the lounge pre-scream. And *this*... blurry figure leaving.",
            "reveals": "testimony-mira-photos-confirm-two-men", 
            "trigger_type": "evidence_presentation", 
            "trigger_value": "barrier-camera", 
            "accessibility": "medium"
          }
        }
```

**For `character_profiles.penelope_valor.core`:**

```json
        "revelations": {
          "penelope_downplays_argument": {
            "content": "Victoria and I had words. Industry tension about designers. Nothing more, darling.",
            "reveals": "testimony-victoria-penelope-argument",
            "trigger_type": "conversation_topic",
            "trigger_value": "Asking about her argument with Victoria",
            "accessibility": "easy"
          },
          "penelope_reveals_victoria_threat_defensively": {
            "content": "Victoria was spiraling. Threatened personal attacks... about my team, about Nova. Lashing out because Couture was sinking.",
            "reveals": "testimony-victoria-threats",
            "trigger_type": "evidence_presentation",
            "trigger_value": "evidence-financial-records",
            "accessibility": "hard"
          },
          "penelope_asserts_victoria_not_suicidal": {
            "content": "Suicide? Victoria? Absolutely not. She was a fighter. Cornered, yes. Desperate, perhaps. But never that.",
            "reveals": "lead-suicide-question",
            "trigger_type": "conversation_topic",
            "trigger_value": "Asking if Victoria seemed suicidal",
            "accessibility": "medium"
          }
        }
```

---

### New Nodes to Add (`constellation.nodes`)

*(Add these definitions within your existing `nodes` object)*

```json
    "testimony-sedative-art-use": {
      "type": "TESTIMONY",
      "category": "MAIN_MYSTERY",
      "content": "Maxwell hinted that artists might use sedatives for precision/steady hands",
      "discovered": false,
      "characters": ["maxwell_porter"]
    },
    "lead-check-incinerator": {
      "type": "LEAD",
      "category": "MAIN_MYSTERY",
      "content": "Should the kitchen incinerator be checked for disposed medical items?",
      "discovered": false,
      "location": "kitchen_car", 
      "characters": ["maxwell_porter"]
    },
    "testimony-gregory-manipulation-photos": {
      "type": "TESTIMONY",
      "category": "MAIN_MYSTERY",
      "content": "Maxwell claims Gregory told him Victoria was showing art photos to the investigator",
      "discovered": false,
      "characters": ["maxwell_porter", "gregory_crowe"]
    },
     "testimony-timmy-saw-syringe": {
      "type": "TESTIMONY",
      "category": "MAIN_MYSTERY",
      "content": "Timmy thinks he saw something like a syringe needle in Maxwell's pocket after spilling coffee on him",
      "discovered": false,
      "characters": ["timmy_seol", "maxwell_porter"]
    },
    "testimony-mira-photos-confirm-two-men": {
      "type": "TESTIMONY",
      "category": "MAIN_MYSTERY",
      "content": "Mira's photos confirm Gregory and Maxwell entered the lounge car together shortly before the murder was discovered",
      "discovered": false,
      "characters": ["mira_sanchez", "gregory_crowe", "maxwell_porter"]
    },
    "lead-maxwell-paranoia": {
        "type": "LEAD",
        "category": "MAIN_MYSTERY",
        "content": "Does Maxwell have reason to be paranoid, or is he misinterpreting events?",
        "discovered": false,
        "characters": ["maxwell_porter", "gideon_marsh", "gregory_crowe"]
    },
    "lead-gregory-downplays-maxwell": {
        "type": "LEAD",
        "category": "MAIN_MYSTERY",
        "content": "Is Gregory telling the truth about Maxwell's instability, or is he trying to discredit him?",
        "discovered": false,
        "characters": ["gregory_crowe", "maxwell_porter"]
    },
    "testimony-gregory-claims-maxwell-unstable": {
        "type": "TESTIMONY",
        "category": "MAIN_MYSTERY",
        "content": "Gregory claims Maxwell is unstable and prone to misinterpreting things",
        "discovered": false,
        "characters": ["gregory_crowe", "maxwell_porter"]
    }
```

*(Self-correction: Added missing nodes `lead-maxwell-paranoia`, `lead-gregory-downplays-maxwell`, `testimony-gregory-claims-maxwell-unstable` that were used in reveals)*

---

### New Connections to Add (`constellation.connections`)

*(Add these connections to your existing `connections` array)*

```json
    {
      "source": "evidence-sedative", 
      "target": "testimony-sedative-art-use", 
      "type": "IMPLIES" 
    },
     { 
      "source": "testimony-sedative-art-use", 
      "target": "lead-forgery-connection", 
      "type": "SUGGESTS" 
    },
    { 
      "source": "lead-check-incinerator", 
      "target": "evidence-incinerator-ash", 
      "type": "REVEALS" // Checking the incinerator reveals the ash
    },
     { 
      "source": "testimony-gregory-manipulation-photos", 
      "target": "deduction-maxwell-paranoia", 
      "type": "SUPPORTS" // Maxwell's account supports the paranoia deduction
    },
     { 
      "source": "testimony-timmy-saw-syringe", 
      "target": "evidence-sedative", 
      "type": "SUGGESTS" // Timmy's sighting suggests the origin of the sedative
    },
     { 
      "source": "testimony-mira-photos-confirm-two-men", 
      "target": "testimony-two-men", 
      "type": "CONFIRMS" // Mira's photos confirm Nova's testimony
    },
    {
      "source": "testimony-gregory-claims-maxwell-unstable",
      "target": "deduction-maxwell-paranoia",
      "type": "CONFIRMS" // Gregory confirming instability supports the paranoia deduction
    },
    {
      "source": "lead-gregory-downplays-maxwell",
      "target": "lead-forgery-connection",
      "type": "SUGGESTS" // Gregory discrediting Maxwell raises suspicion about Gregory
    }
    // Add connection from Gideon's observation to lead-maxwell-paranoia if not already present
    {
        "source": "testimony-gideon_shares_observation_maxwell_fear", // Assuming Gideon's revelation node is named this
        "target": "lead-maxwell-paranoia",
        "type": "SUPPORTS"
    }
```

*(Self-correction: Added necessary connections for the new nodes and ensured links make logical sense within the graph. Added a connection for Gideon's observation to the paranoia lead node.)*