stateDiagram-v2
    %% Game States
    state "Game States" as GameStates {
        [*] --> DEFAULT
        DEFAULT --> DIALOGUE: Player interacts with NPC
        DIALOGUE --> DEFAULT: ESC key pressed
        DEFAULT --> PAUSE: ESC key pressed
        PAUSE --> DEFAULT: Resume selected
        DEFAULT --> MINIGAME: Minigame activated
        MINIGAME --> DEFAULT: Minigame completed
        DEFAULT --> MYSTERY: Mystery UI opened
        MYSTERY --> DEFAULT: Mystery UI closed
        DEFAULT --> FINAL: Timer expires
        FINAL --> WIN: Mystery solved
        FINAL --> LOSE: Failed to solve
    }
    
    %% Character States
    state "Character Initialization" as CharInit {
        [*] --> Uninitialized
        Uninitialized --> LoadingTemplate
        LoadingTemplate --> WarmingUp: Template loaded
        LoadingTemplate --> Failed: Timeout/Error
        WarmingUp --> Ready: Warmup complete
        WarmingUp --> Failed: Timeout/Error
        Failed --> LoadingTemplate: Retry
    }
    
    %% NPC Behavior States
    state "NPC Behavior" as NPCStates {
        [*] --> IdleState
        IdleState --> MovementState: Idle timer expires
        MovementState --> IdleState: Reached destination
        IdleState --> DialogueActivate: Player in range & E key
        DialogueActivate --> IdleState: Dialogue ends
    }
    
    %% Game Initialization Sequence
    state "Game Initialization" as GameInit {
        [*] --> LLMStartup
        LLMStartup --> MysteryParsing: LLM started
        MysteryParsing --> CharacterExtraction: Mystery parsed
        CharacterExtraction --> NPCInitialization: Characters extracted
        NPCInitialization --> SceneLoading: NPCs initialized
        SceneLoading --> [*]: Main scene loaded
    }
    
    %% Dialogue System States
    state "Dialogue System" as DialogueSystem {
        [*] --> DialogueActive
        DialogueActive --> PlayerInput
        PlayerInput --> ProcessingResponse: Submit clicked
        ProcessingResponse --> StreamingResponse: LLM generates
        StreamingResponse --> PlayerInput: Response complete
        PlayerInput --> DialogueExit: ESC pressed
        DialogueExit --> [*]
    }
    
    %% Connect the state machines at high level
    [*] --> GameInit
    GameInit --> GameStates
    GameStates --> NPCStates
    GameStates --> DialogueSystem: DIALOGUE state
    GameStates --> CharInit: During initialization
