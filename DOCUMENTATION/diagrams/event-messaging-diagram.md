flowchart TD
    %% Event Publishers
    ParsingControl["ParsingControl\n(Publisher)"]
    MysteryExtractor["MysteryCharacterExtractor\n(Publisher)"]
    CharManager["CharacterManager\n(Publisher)"]
    LLMDialogueManager["LLMDialogueManager\n(Publisher)"]
    
    %% Event Subscribers
    GameInit["GameInitializer\n(Subscriber)"]
    ParsingControlSub["ParsingControl\n(Subscriber)"]
    LoadingUI["LoadingUI\n(Subscriber)"]
    DialogueUI["DialogueUI\n(Subscriber)"]
    
    %% Event Relationships with Data Payloads
    ParsingControl -->|"OnParsingProgress(float)"| LoadingUI
    ParsingControl -->|"OnMysteryParsed(Mystery)"| GameControl["GameControl"]
    ParsingControl -->|"OnCharactersExtracted(int)"| GameInit
    ParsingControl -->|"OnParsingComplete()"| GameInit
    
    MysteryExtractor -->|"OnExtractionProgress(float)"| ParsingControlSub
    MysteryExtractor -->|"OnCharactersExtracted(int)"| ParsingControlSub
    
    CharManager -->|"OnInitializationComplete()"| GameInit
    
    %% UI Events
    SubmitButton["Submit Button"] -->|"onClick.AddListener()"| LLMDialogueManager
    InputField["Input Field"] -->|"onSubmit.AddListener()"| LLMDialogueManager
    
    %% Game State Changes
    NPCMovement["NPCMovement"] -->|"GameState = DIALOGUE"| GameControl
    DialogueControl["DialogueControl"] -->|"GameState = DIALOGUE/DEFAULT"| GameControl
    
    %% LLM Callbacks
    LLMCharacter["LLMCharacter"] -->|"Chat(..., HandleReply, OnReplyComplete)"| LLMDialogueManager
    
    %% Connections between systems
    GameInit -.->|"Initialization Flow"| ParsingControl
    GameInit -.->|"After Parse Complete"| CharManager
    CharManager -.->|"Character Creation"| LLMCharacter
    
    %% Visual Styling
    classDef publisher fill:#f96,stroke:#333,stroke-width:2px
    classDef subscriber fill:#69f,stroke:#333,stroke-width:2px
    classDef state fill:#9e9,stroke:#333,stroke-width:1px
    classDef ui fill:#d9f,stroke:#333,stroke-width:1px
    
    class ParsingControl,MysteryExtractor,CharManager,LLMDialogueManager publisher
    class GameInit,ParsingControlSub,LoadingUI,DialogueUI subscriber
    class GameControl state
    class SubmitButton,InputField ui