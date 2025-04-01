classDiagram
    %% Core Data Structure
    class Mystery {
        +MysteryMetadata Metadata
        +MysteryCore Core
        +Dictionary~string,MysteryCharacter~ Characters
        +MysteryEnvironment Environment
        +MysteryConstellation Constellation
    }
    
    class MysteryCore {
        +string Perpetrator
        +string Victim
        +string Method
        +string Motive
    }
    
    class MysteryCharacter {
        +CharacterCore Core
        +CharacterMindEngine MindEngine
    }
    
    class MysteryConstellation {
        +Dictionary~string,MysteryNode~ Nodes
        +List~MysteryConnection~ Connections
    }
    
    class MysteryNode {
        +string Id
        +string Type
        +string Title
        +string Description
    }
    
    %% Manager Classes
    class GameControl {
        +GameState currentState
        +Mystery coreMystery
        +MysteryConstellation coreConstellation
    }
    
    class ParsingControl {
        +bool IsParsingComplete
        +void ParseMystery()
        +Task~Mystery~ ParseMysteryAsync()
    }
    
    class CharacterManager {
        +bool IsInitialized
        +Dictionary~string,LLMCharacter~ characterCache
        +LLMCharacter GetCharacterByName(string)
    }
    
    class NPCManager {
        +Dictionary~string,GameObject~ activeNPCs
        +GameObject SpawnNPCInCar(string,Vector3,Transform)
    }
    
    %% Runtime Components
    class LLMCharacter {
        +string AIName
        +Task Chat(string,Action~string~,Action)
    }
    
    class Character {
        -string characterName
        -LLMCharacter llmCharacter
        +LLMCharacter GetLLMCharacter()
    }
    
    class NPCMovement {
        +bool inDialogueRange
        +IEnumerator IdleState()
        +IEnumerator MovementState()
    }
    
    class DialogueControl {
        +void Activate(GameObject)
        +void Deactivate()
    }
    
    class LLMDialogueManager {
        +void SetCharacter(LLMCharacter)
        +void InitializeDialogue()
        +Task ResetDialogue()
    }
    
    %% Data Flow Connections
    Mystery <|-- GameControl : "Stores"
    MysteryCore <|-- Mystery : "Contains"
    MysteryCharacter <|-- Mystery : "Contains many"
    MysteryConstellation <|-- Mystery : "Contains"
    MysteryNode <|-- MysteryConstellation : "Contains many"
    
    ParsingControl --> Mystery : "Creates"
    ParsingControl --> GameControl : "Populates"
    
    Mystery --> CharacterManager : "Creates characters from"
    CharacterManager --> LLMCharacter : "Creates"
    
    NPCManager --> Character : "Creates"
    Character --> LLMCharacter : "References"
    
    NPCMovement --> DialogueControl : "Activates"
    DialogueControl --> LLMDialogueManager : "Uses"
    LLMDialogueManager --> LLMCharacter : "Communicates with"
    
    %% Asset Flow
    class AssetFlow {
        <<File>>
        Mystery JSON
        Character JSON files
        NPC Prefab
    }
    
    AssetFlow --> ParsingControl : "Reads Mystery JSON"
    AssetFlow --> CharacterManager : "Reads Character JSON"
    AssetFlow --> NPCManager : "Instantiates NPC Prefab"
    
    %% Styling
    class AssetFlow {
        <<File>>
    }
    class GameControl {
        <<Singleton>>
    }
    class CharacterManager {
        <<Manager>>
    }
    class NPCManager {
        <<Manager>>
    }
    class ParsingControl {
        <<Manager>>
    }