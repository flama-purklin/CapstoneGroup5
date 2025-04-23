# Character Lifecycle & LLM Management Report

This document details the processes involved in managing character lifecycles within the LLM-integrated system, covering spawning, initial loading, dynamic warm-up, and parallel processing management. It includes references to key scripts, GameObjects, and relevant code snippets illustrating the flow.

## 1. Key Components & Locations

The following scripts and GameObjects are central to these processes:

*   **`InitializationManager.cs`**: Attached to the root `InitializationManager` GameObject. Orchestrates the entire game startup sequence.
*   **`ParsingControl.cs`**: Attached to the root `ParsingControl` GameObject. Handles loading the main mystery JSON in its `Awake` phase.
*   **`GameControl.cs`**: Attached to the root `GameController` GameObject. Acts as a singleton data holder, notably storing the parsed mystery data in `GameControl.coreMystery`.
*   **`LLM.cs`**: Attached to the root `LLM` GameObject. Manages the underlying C++ LLM server process and core configuration (model path, parallel slots, GPU layers).
*   **`CharacterManager.cs`**: Attached to the root `CharacterManager` GameObject. Manages the *logical* representation of characters.
    *   **Child GameObject:** Creates a child GameObject named `Characters` under itself.
*   **`LLMCharacter.cs`**: Component dynamically added by `CharacterManager` to GameObjects named after each character (e.g., `maxwell_porter`) parented under `CharacterManager/Characters`. Represents an individual character's AI state, history, and configuration.
*   **`TrainLayoutManager.cs`**: Likely attached to the root `TrainManager` GameObject. Responsible for instantiating train car prefabs.
    *   **Child GameObject:** Creates a child GameObject named `Rail Cars` under itself.
*   **`NPCManager.cs`**: Attached to the root `NPCManager` GameObject. Responsible for spawning the *physical* NPC prefabs.
*   **NPC Prefab (e.g., `NPC.prefab`)**: Instantiated by `NPCManager`. Contains components like:
    *   `Character.cs`: Links the physical NPC to its logical `LLMCharacter`.
    *   `NPCMovement.cs`: Handles movement and interaction triggers.
    *   `NPCAnimManager.cs`: Handles animations.
    *   `NavMeshAgent`: For pathfinding.
    *   **Runtime Location:** Parented under the appropriate train car GameObject (e.g., `TrainManager/Rail Cars/<CarName>/<CharacterName>`).
*   **`SimpleProximityWarmup.cs`**: Likely attached to the `CharacterManager` GameObject. Manages dynamic warm-up/cooldown based on player proximity.

## 2. Character Spawning (Initialization Phase)

This process creates both the logical AI representation and the physical NPC presence during the loading screen.

**Flow & Code Snippets:**

1.  **Data Parsing (`ParsingControl.Awake`)**: Reads `transformed-mystery.json` and populates `GameControl.coreMystery`. (Code not shown, but happens in `Awake`).
2.  **Orchestration (`InitializationManager.InitializeGame`)**: Begins the main sequence.
    ```csharp
    // InitializationManager.cs - InitializeGame()
    // ... (Wait LLM, Wait Parsing)
    Debug.Log("--- INIT STEP 3: Initialize CharacterManager ---");
    if (characterManager != null) {
         characterManager.Initialize(); // Starts TwoPhaseInitialization
         await WaitForCharacterManagerInitialization(); // Wait for it
    }
    // ... (Build Train)
    Debug.Log("--- INIT STEP 5: Spawn NPCs & Link Characters ---");
    SpawnAndLinkNPCs(); // Combined spawning and linking
    // ...
    ```
3.  **Logical Character Creation (`CharacterManager.Initialize` -> `TwoPhaseInitialization` -> `CreateSingleCharacterObject`)**:
    *   Triggered by `InitializationManager`.
    *   Iterates through `GameControl.GameController.coreMystery.Characters`.
    *   For each non-victim:
        ```csharp
        // CharacterManager.cs - CreateSingleCharacterObject()
        GameObject charObj = new GameObject($"{characterName}");
        charObj.transform.SetParent(charactersContainer, false);
        LLMCharacter character = charObj.AddComponent<LLMCharacter>();
        character.llm = sharedLLM;
        // ... configure character settings ...
        string systemPrompt = CharacterPromptGenerator.GenerateSystemPrompt(...);
        promptCache[characterName] = systemPrompt;
        character.SetPrompt(systemPrompt, true);
        characterCache[characterName] = character;
        stateTransitions[characterName] = new CharacterStateTransition(characterName, CharacterState.Uninitialized, charObj);
        ```
        *   Registration happens within `LLMCharacter.Awake()`:
        ```csharp
        // LLMCharacter.cs - Awake()
        base.Awake(); // Calls LLMCaller.Awake()
        if (!remote)
        {
            int slotFromServer = llm.Register(this); // Registers with LLM service
            if (slot == -1) slot = slotFromServer;
        }
        // ...
        ```
4.  **Physical NPC Spawning (`InitializationManager.SpawnAndLinkNPCs`)**:
    *   Triggered by `InitializationManager` *after* `CharacterManager` initialization.
    *   Iterates through `GameControl.coreMystery.Characters`.
    *   For each non-victim:
        ```csharp
        // InitializationManager.cs - SpawnAndLinkNPCs()
        string startCarName = charData.InitialLocation;
        Transform carTransform = trainLayoutManager.GetCarTransform(startCarName);
        Vector3 spawnPos = trainLayoutManager.GetSpawnPointInCar(startCarName);
        GameObject spawnedNPC = npcManager.SpawnNPCInCar(characterName, spawnPos, carTransform, i); // Instantiates prefab
        if (spawnedNPC != null) {
            Character characterComponent = spawnedNPC.GetComponent<Character>();
            LLMCharacter llmCharacterRef = characterManager.GetCharacterByName(characterName);
            if (characterComponent != null && llmCharacterRef != null) {
                characterComponent.Initialize(characterName, llmCharacterRef); // Links physical to logical
            }
            // ... error handling ...
        }
        ```

**Runtime State:** After this phase (still during loading), the scene contains the root managers, the `CharacterManager/Characters/` hierarchy with `LLMCharacter` objects, the `TrainManager/Rail Cars/` hierarchy, and the spawned NPC GameObjects under their respective cars, linked to their `LLMCharacter`.

## 3. Character Loading (Initialization Phase)

This prepares the LLM and character states while the loading overlay is visible.

**Flow & Code Snippets:**

1.  **LLM Server Start (`InitializationManager.InitializeGame` -> `WaitForLLMStartup`)**: Waits for `LLM.Awake()` to start the backend server.
2.  **Template Loading (`CharacterManager.InitializeLLMCharacters` -> `LLMCharacter.LoadTemplate`)**:
    *   Called during `CharacterManager`'s initialization.
    ```csharp
    // CharacterManager.cs - InitializeLLMCharacters()
    foreach (var kvp in characterCache) {
        // ...
        templateLoadTasks.Add(LoadTemplateWithTimeout(llmCharacterRef, characterName));
        // ...
    }
    // ... await Task.WhenAll(templateLoadTasks); ...

    // CharacterManager.cs - LoadTemplateWithTimeout() calls:
    // LLMCharacter.cs - LoadTemplate()
    public virtual async Task LoadTemplate()
    {
        string llmTemplate;
        if (remote) { llmTemplate = await AskTemplate(); }
        else { llmTemplate = llm.GetTemplate(); }
        // ... update internal template state ...
    }
    ```
3.  **Context Allocation (`CharacterManager.AllocateContext`)**:
    *   Called immediately after template loading.
    ```csharp
    // CharacterManager.cs - AllocateContext()
    int contextPerCharacter = sharedLLM.contextSize / sharedLLM.parallelPrompts;
    foreach (var kvp in characterCache) {
        LLMCharacter character = kvp.Value;
        if (character != null) { character.nKeep = contextPerCharacter; }
    }
    ```
4.  **History/Cache Loading (`LLMCharacter.Load`)**:
    *   Triggered by `LLMCharacter.Awake()` -> `InitHistory()` -> `LoadHistory()` if save files exist, OR later by `DialogueControl.Activate()`.
    ```csharp
    // LLMCharacter.cs - Load()
    public virtual async Task<string> Load(string filename)
    {
        // ... load json history ...
        List<ChatMessage> chatHistory = JsonUtility.FromJson<ChatListWrapper>(json).chat;
        ClearChat(); // Adds system prompt
        chat.AddRange(chatHistory); // Adds loaded history

        if (!remote && saveCache)
        {
            string cachepath = GetCacheSavePath(filename);
            if (File.Exists(GetSavePath(cachepath)))
            {
                string result = await Slot(cachepath, "restore"); // Calls LLM server endpoint
                return result;
            }
            // ...
        }
        // ...
    }

    // LLMCharacter.cs - Slot() helper calls LLM server
    protected virtual async Task<string> Slot(string filepath, string action)
    {
        SlotRequest slotRequest = new SlotRequest();
        slotRequest.id_slot = slot;
        slotRequest.filepath = filepath;
        slotRequest.action = action;
        string json = JsonUtility.ToJson(slotRequest);
        return await PostRequest<SlotResult, string>(json, "slots", SlotContent); // Sends request
    }
    ```

**Runtime State:** By the end of the `InitializationManager` sequence: LLM server running, `LLMCharacter` objects exist with templates loaded, context (`nKeep`) set, potentially history/cache loaded. Characters are in `LoadingTemplate` state.

## 4. Character Warm-up (Gameplay Phase)

Dynamically prepares characters for interaction *after* initial loading is complete.

**Flow & Code Snippets:**

1.  **Proximity Check (`SimpleProximityWarmup.Update` -> `UpdateWarmupState`)**: Runs periodically/on movement during gameplay, calculates distances, sorts NPCs.
2.  **Identify Targets**: Determines closest `maxWarmCharacters`.
3.  **Trigger Warmup (`UpdateWarmupState` -> `characterManager.WarmupCharacter`)**:
    ```csharp
    // SimpleProximityWarmup.cs - UpdateWarmupState()
    foreach (string name in charactersToWarm)
    {
        StartCoroutine(characterManager.WarmupCharacter(name));
    }

    // CharacterManager.cs - WarmupCharacter() Coroutine
    public IEnumerator WarmupCharacter(string characterName) {
        // ... checks ...
        if (!stateTransitions[characterName].TryTransition(CharacterState.WarmingUp)) { /* error */ yield break; }
        yield return StartCoroutine(WarmupWithRetries(character, characterName));
        // ... logging ...
    }
    ```
4.  **LLM Pre-Processing (`CharacterManager.WarmupWithRetries` -> `LLMCharacter.Warmup`)**:
    ```csharp
    // CharacterManager.cs - WarmupWithRetries() calls:
    // LLMCharacter.cs - Warmup()
    public virtual async Task Warmup(string query, EmptyCallback completionCallback = null)
    {
        // ... LoadTemplate, CheckTemplate, InitNKeep checks ...
        ChatRequest request;
        // ... generate request based on system prompt (or optional query) ...
        request.n_predict = 0; // KEY: Don't generate tokens
        string json = JsonUtility.ToJson(request);
        await CompletionRequest(json); // Sends request to LLM server
        completionCallback?.Invoke();
    }
    ```
5.  **Ready State (`WarmupWithRetries` in `CharacterManager`)**: Upon successful completion, transitions state to `Ready`.
    ```csharp
    // CharacterManager.cs - WarmupWithRetries()
    // ... after warmupTask completes successfully ...
    else {
        if (stateTransitions.ContainsKey(characterName)) stateTransitions[characterName].TryTransition(CharacterState.Ready);
        yield break;
    }
    ```
6.  **Cooldown (`UpdateWarmupState` -> `characterManager.CooldownCharacter`)**:
    ```csharp
    // SimpleProximityWarmup.cs - UpdateWarmupState()
    foreach (string name in charactersToCool)
    {
        characterManager.CooldownCharacter(name);
    }

    // CharacterManager.cs - CooldownCharacter()
    public void CooldownCharacter(string characterName) {
        // ... checks ...
        var character = characterCache[characterName];
        character.CancelRequests(); // Tells LLMCharacter to cancel ongoing server requests for its slot
        if (stateTransitions[characterName].TryTransition(CharacterState.LoadingTemplate)) { /* success log */ }
        else { /* error log */ }
    }

    // LLMCharacter.cs - CancelRequests() calls base which calls LLM
    protected override void CancelRequestsLocal()
    {
        if (slot >= 0) llm.CancelRequest(slot); // Calls LLM.cs method
    }

    // LLM.cs - CancelRequest()
    public void CancelRequest(int id_slot)
    {
        AssertStarted();
        llmlib?.LLM_Cancel(LLMObject, id_slot); // Calls native library function
        CheckLLMStatus();
    }
    ```

**Runtime State:** During gameplay, up to `maxWarmCharacters` are `Ready`, others are `LoadingTemplate`. State changes based on player movement.

## 5. Parallel Prompt Management

Allows multiple characters to process requests concurrently.

**Flow & Configuration:**

1.  **Slot Definition (`LLM.parallelPrompts` Inspector Setting)**: Configures the backend server via `-np` argument.
    ```csharp
    // LLM.cs - GetLlamaccpArguments()
    int slots = GetNumClients(); // Based on parallelPrompts or registered clients
    string arguments = $"-m \"{modelPath}\" ... -np {slots}"; // Sets number of parallel slots
    ```
2.  **Slot Assignment (`LLMCharacter.Awake` -> `llm.Register`)**: Assigns a cycling slot ID.
    ```csharp
    // LLM.cs - Register()
    public int Register(LLMCaller llmCaller)
    {
        clients.Add(llmCaller);
        int index = clients.IndexOf(llmCaller);
        if (parallelPrompts != -1) return index % parallelPrompts; // Assigns slot based on index and limit
        return index;
    }
    ```
3.  **Memory Allocation (`CharacterManager.AllocateContext`)**: Divides `LLM.contextSize` by `LLM.parallelPrompts` to set `LLMCharacter.nKeep`. (See snippet in Section 3).
4.  **Dynamic Slot Usage (`SimpleProximityWarmup`)**: Limits active (`Ready`) characters to `maxWarmCharacters`. (See Warmup/Cooldown snippets in Section 4).
5.  **Request Routing (`LLMCharacter` methods like `Chat`, `Warmup`)**: Includes `id_slot` in JSON requests.
    ```csharp
    // LLMCharacter.cs - GenerateRequest()
    ChatRequest chatRequest = new ChatRequest();
    // ...
    chatRequest.id_slot = slot; // Uses the assigned slot ID
    // ...
    return chatRequest;

    // LLMCharacter.cs - PostRequestLocal() (for completion)
    // Sends request JSON (containing id_slot) to llm.Completion()
    callResult = await llm.Completion(json, callbackString);

    // LLM.cs - Completion()
    // Sends request JSON to native library function llmlib.LLM_Completion()
    await Task.Run(() => llmlib.LLM_Completion(LLMObject, json, streamWrapper.GetStringWrapper()));
    ```

**Runtime State:** LLM server has a fixed number of slots (3). Each `LLMCharacter` has a fixed slot ID (0, 1, or 2). `SimpleProximityWarmup` keeps up to `maxWarmCharacters` (3) slots "warm" (`Ready`) based on proximity.
