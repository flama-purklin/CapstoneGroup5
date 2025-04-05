flowchart TD
    %% Event Publishers
    ParsingControl["ParsingControl\n(Publisher)"]
    MysteryExtractor["MysteryCharacterExtractor\n(Publisher)"]
    CharManager["CharacterManager\n(Publisher)"]
    LLMDialogueManager["LLMDialogueManager\n(Publisher)"]
    
    %% Event Subscribers
    InitManager["InitializationManager\n(Subscriber)"]
    ParsingControlSub["ParsingControl\n(Subscriber)"]
    LoadingUI["LoadingOverlay/UI\n(Subscriber)"]
    DialogueUI["DialogueUI\n(Subscriber)"]
    
    %% Event Relationships with Data Payloads
    ParsingControl -->|"OnParsingProgress(float)"| LoadingUI
    ParsingControl -->|"OnMysteryParsed(Mystery)"| GameControl["GameControl"]
    ParsingControl -->|"OnCharactersExtracted(int)"| InitManager
    ParsingControl -->|"OnParsingComplete()"| InitManager
    
    MysteryExtractor -->|"OnExtractionProgress(float)"| ParsingControlSub
    MysteryExtractor -->|"OnCharactersExtracted(int)"| ParsingControlSub
    
    CharManager -->|"OnInitializationComplete()"| InitManager
    
    %% UI Events
    SubmitButton["Submit Button"] -->|"onClick.AddListener()"| LLMDialogueManager
    InputField["Input Field"] -->|"onSubmit.AddListener()"| LLMDialogueManager
    
    %% Game State Changes
    NPCMovement["NPCMovement"] -->|"GameState = DIALOGUE"| GameControl
    DialogueControl["DialogueControl"] -->|"GameState = DIALOGUE/DEFAULT"| GameControl
    
    %% LLM Callbacks
    LLMCharacter["LLMCharacter"] -->|"Chat(..., HandleReply, OnReplyComplete)"| LLMDialogueManager
    
    %% Connections between systems
    InitManager -.->|"Initialization Flow"| ParsingControl
    InitManager -.->|"After Parse Complete"| CharManager
    CharManager -.->|"Character Creation"| LLMCharacter
    
    %% Visual Styling
    classDef publisher fill:#f96,stroke:#333,stroke-width:2px
    classDef subscriber fill:#69f,stroke:#333,stroke-width:2px
    classDef state fill:#9e9,stroke:#333,stroke-width:1px
    classDef ui fill:#d9f,stroke:#333,stroke-width:1px
    
    class ParsingControl,MysteryExtractor,CharManager,LLMDialogueManager publisher
    class InitManager,ParsingControlSub,LoadingUI,DialogueUI subscriber
    class GameControl state
    class SubmitButton,InputField ui
