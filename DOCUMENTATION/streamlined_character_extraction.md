# Refactoring: Streamlined Character Data Extraction & Initialization

## 1. Goals

*   **Eliminate Redundancy:** Remove the creation and use of individual character JSON files located in `Assets/StreamingAssets/Characters/`. The sole source of truth for character data during runtime should be the `GameControl.GameController.coreMystery.Characters` dictionary.
*   **Fix Initialization Order:** Resolve the race condition between `ParsingControl` (parsing the mystery) and `CharacterManager` (initializing LLM characters). Ensure `CharacterManager` only begins its initialization *after* `ParsingControl` has successfully loaded the mystery data into `GameControl`.
*   **Improve Maintainability:** Simplify the data flow by removing unnecessary file I/O steps and intermediate data representations.

## 2. Current State (Pre-Refactor)

1.  **Parsing:** `ParsingControl` reads `transformed-mystery.json`.
2.  **Data Population:** `ParsingControl` deserializes the JSON into `GameControl.GameController.coreMystery`.
3.  **Extraction (Redundant):** `ParsingControl` calls `MysteryCharacterExtractor`.
4.  **File Writing (Redundant):** `MysteryCharacterExtractor` iterates through `GameControl.coreMystery.Characters` and writes individual JSON files to `Assets/StreamingAssets/Characters/`.
5.  **Parsing Completion Signal:** `ParsingControl` fires `OnParsingComplete` *after* file extraction attempts (or timeout).
6.  **Character Initialization:** `CharacterManager` (triggered independently in `Awake`) reads the individual JSON files from `Assets/StreamingAssets/Characters/` to create and initialize `LLMCharacter` instances.
7.  **NPC Spawning:** `InitializationManager` calls `CharacterManager.GetCharacterStartingCar()`, which reads the individual JSON file again to find the `initial_location`.

**Problem:** Step 6 can start before Step 4 finishes, causing `CharacterManager` to fail reading files that don't exist yet (race condition). Step 4 itself is redundant. Step 7 involves redundant file reading.

## 3. Target State (Post-Refactor)

1.  **Parsing:** `ParsingControl` reads `transformed-mystery.json`.
2.  **Data Population:** `ParsingControl` deserializes the JSON into `GameControl.GameController.coreMystery`.
3.  **Parsing Completion Signal:** `ParsingControl` fires `OnParsingComplete` immediately after successfully populating `GameControl.coreMystery`.
4.  **Character Initialization Trigger:** `CharacterManager` listens for `ParsingControl.OnParsingComplete`.
5.  **Character Initialization:** Upon receiving the event, `CharacterManager` iterates directly through the `GameControl.GameController.coreMystery.Characters` dictionary.
6.  **Prompt Generation:** `CharacterManager` serializes the `MysteryCharacter` object from the dictionary into a JSON string and passes it to `CharacterPromptGenerator.GenerateSystemPrompt`.
7.  **LLMCharacter Creation:** `LLMCharacter` instances are created and initialized using the in-memory data.
8.  **NPC Spawning:** `InitializationManager` directly accesses the top-level `GameControl.GameController.coreMystery.Characters[characterName].InitialLocation` property to get the starting car name.

**Outcome:** No individual character files are created or read. Initialization sequence is guaranteed by the event system (Parsing → Character Init → NPC Spawn). Data flow is simplified.

## 4. Key Code Changes Summary

*   **`ParsingControl.cs`:** Remove all code related to `MysteryCharacterExtractor`. Signal `OnParsingComplete` immediately after populating `GameControl.coreMystery`.
*   **`CharacterManager.cs`:**
    *   Remove file system reading logic (`Directory.GetFiles`, `File.ReadAllText`).
    *   Iterate `GameControl.GameController.coreMystery.Characters` in `CreateCharacterObjects`.
    *   Serialize the `MysteryCharacter` object (value from the dictionary) back to a JSON string using `Newtonsoft.Json.JsonConvert.SerializeObject()` and pass this string to `CharacterPromptGenerator.GenerateSystemPrompt`.
    *   Remove `GetCharacterStartingCar()` method and its helper class `CharacterLocationData`.
    *   Remove the `Awake()` call to `StartCoroutine(TwoPhaseInitialization())`. Subscribe to `ParsingControl.OnParsingComplete` instead, triggering `TwoPhaseInitialization` from the event handler. Ensure `OnInitializationComplete` event is still fired.
*   **`InitializationManager.cs`:**
    *   Add logic to wait for `CharacterManager.OnInitializationComplete` (with timeout) *after* `ParsingControl.OnParsingComplete` and *before* spawning NPCs.
    *   Modify `SpawnAllNPCs` to get `startCarName` directly from the top-level `GameControl.GameController.coreMystery.Characters[characterName].InitialLocation` property.
*   **File System & Scene:** Delete `MysteryCharacterExtractor.cs` script, the `MysteryCharacterExtractor` GameObject from the `SystemTest` scene, and the directories `Assets/StreamingAssets/Characters/` and `Assets/StreamingAssets/CharacterBackups/`.
*   **Documentation:** Update `full-technical-documentation.md` to reflect the new data flow, event handling, and removal of redundant components/files.
