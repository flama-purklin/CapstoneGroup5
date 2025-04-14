# Black-Box Mystery Interface Diagram

This diagram illustrates the flow of data from the `transformed-mystery.json` file through the parsing process into the central `Mystery` object instance, and how various game systems consume this data to initialize and run the simulation. It also highlights parts of the JSON currently unused and game elements not fully driven by the JSON.

```mermaid
graph TD
    subgraph Input
        A[transformed-mystery.json\n(File in StreamingAssets)]
    end

    subgraph Parsing & Centralization
        B(ParsingControl.cs);
        C(GameControl.cs);
        D[Mystery Object Instance\n(In Memory)];
        E[coreConstellation Reference];

        A -- Reads --> B;
        B -- Deserializes using C# Classes --> D;
        D -- Stored in --> C[GameControl.coreMystery];
        D -- Constellation copied to --> E;
        E -- Stored in --> C[GameControl.coreConstellation];

        style D fill:#ccf,stroke:#333,stroke-width:2px
    end

    subgraph Initialization & Simulation Setup
        F(InitializationManager.cs);
        G(CharacterManager.cs);
        H(TrainLayoutManager.cs);
        I(NPCManager.cs);
        J(TrainManager.cs);
        K[LLMCharacter Components\n(Children of CharacterManager GO)];
        L[NPC GameObjects\n(Instantiated Prefabs)];
        M[Train Car GameObjects\n(Children of TrainManager GO)];

        C -- Read by --> F;
        F -- Triggers --> G;
        F -- Triggers --> H;
        F -- Triggers --> I;
        F -- Triggers --> J;

        D[Characters] -- Read by --> G;
        G -- Creates --> K;
        D[Characters] -- InitialLocation Read by --> F;
        F -- Provides Spawn Info --> I;
        I -- Instantiates --> L;
        G -- Provides LLMCharacter Ref --> F;
        F -- Links Character Component on L --> K;

        D[Environment] -- Read by --> H;
        H -- Provides Layout Info --> J;
        J -- Instantiates --> M;

    end

    subgraph Gameplay Interaction
        N(Minigame Scripts\n e.g., LuggageControl on LuggageObj);
        O(MysteryCanvas / NodeHolder);
        P[EvidenceObj / LuggageObj\n(Placed in Scene)];

        E -- Read/Modified by --> N[DiscoverNode()];
        E -- Likely Read by --> O[For Visualization];
        P -- Triggers --> N;

        style P fill:#fcc,stroke:#333,stroke-width:1px,stroke-dasharray: 5 5;
    end

    subgraph Unused / Gaps
        U1[Mystery.Core Data\n(Victim, Perpetrator etc.)];
        U2[Mystery.Metadata];
        U3[MysteryCharacter Fields?\n(Appearance, Voice)];
        U4[Evidence/Minigame Definitions\n(Should be in JSON?)];

        D -- Contains --> U1;
        D -- Contains --> U2;
        D -- Characters Contain --> U3;
        P -.-> U4[Needs Link];

        style U1 fill:#ddd,stroke:#333,stroke-width:1px,stroke-dasharray: 2 2;
        style U2 fill:#ddd,stroke:#333,stroke-width:1px,stroke-dasharray: 2 2;
        style U3 fill:#ddd,stroke:#333,stroke-width:1px,stroke-dasharray: 2 2;
        style U4 fill:#fcc,stroke:#333,stroke-width:1px,stroke-dasharray: 5 5;
    end

    linkStyle 15 stroke:#f00,stroke-width:1px,color:red;
```

**Key:**

*   Blue Box: Central `Mystery` object instance in memory.
*   Grey Dashed Boxes: Parts of the JSON data model currently unused by the simulation.
*   Red Dashed Boxes/Arrows: Game elements or connections that are currently hardcoded or manually placed and need better integration with the JSON data structure.
