# Character Spawning System Overview (Updated 2025-04-08)

## 1. Goal

The primary goal of the current character spawning system is to instantiate all Non-Player Characters (NPCs) defined in the loaded mystery during the initial game loading sequence (`SystemsTest` scene). Each NPC should be:

1.  **Spawned Centrally:** Instantiation is managed by `InitializationManager.cs` after core systems like `CharacterManager` and `NPCManager` are ready.
2.  **Placed by Location:** Positioned within the specific train car designated by the `initial_location` field in their respective character JSON file (`Assets/StreamingAssets/Characters/`). This includes handling duplicate car types using suffixes (e.g., `business_class_1`, `business_class_2`).
3.  **Positioned on NavMesh:** Placed at a valid point on the NavMesh surface within the target car, ideally near a designated grid point (`Anchor` -> `walkway` structure).
4.  **Visually Represented:** Assigned one of four available "Winchester" appearances cyclically, using `NPCAnimContainer` Scriptable Objects assigned in the Inspector.
5.  **AI-Enabled:** Linked to their corresponding `LLMCharacter` component for dialogue and behavior.
6.  **Mobile:** Able to navigate the baked NavMesh within the train cars using their `NavMeshAgent` component.

## 2. Current Implementation Details (Post-Refactor)

The spawning process involves several key scripts and assets, with recent modifications:

*   **`InitializationManager.cs`:** Orchestrates the overall game startup. After initializing `CharacterManager` and `NPCManager` (which caches LLM data), it calls `trainLayoutManager.ResetUsedAnchorTracking()` and then loops through available characters, calling a dedicated `SpawnAllNPCs()` method (or similar logic within its initialization coroutine).
*   **`SpawnAllNPCs()` (in `InitializationManager.cs`):**
    *   Retrieves the list of available character names (presumably from `CharacterManager` or `NPCManager`).
    *   Loops through each character name (getting index `i`).
    *   Calls `CharacterManager.GetCharacterStartingCar()` (or equivalent logic) to get the target car name key (e.g., `"business_class_1"`) from the character's JSON.
    *   Calls `TrainLayoutManager.GetCarTransform(startCarName)` to find the `Transform` of the target car GameObject using the potentially suffixed key.
    *   Calls `TrainLayoutManager.GetSpawnPointInCar(startCarName)` to determine a `Vector3` spawn position within that car.
    *   Calls `NPCManager.SpawnNPCInCar()`, passing the character name, spawn position, car transform, and the character's index `i` (for appearance assignment).
*   **`CharacterManager.cs` (or equivalent logic):**
    *   `GetCharacterStartingCar(string characterName)`: Reads the specified character's JSON file and parses the top-level `initial_location` field to return the target car name string (e.g., `"business_class_1"`). *(Note: The exact script handling this needs confirmation as `CharacterManager.cs` was not found at the documented path).*
*   **`TrainLayoutManager.cs`:**
    *   `NameCars()`: Populates an internal `Dictionary<string, GameObject> carInstanceMap`. It iterates through the `LayoutOrder` from the mystery JSON, tracks counts of duplicate car types (e.g., "business\_class"), and adds suffixed keys (e.g., `"business_class_1"`, `"business_class_2"`) to the map, linking them to the corresponding spawned car GameObjects.
    *   `GetCarTransform(string carNameKey)`: Performs a direct lookup in `carInstanceMap` using the provided key (e.g., `"business_class_1"`) to find the correct car `Transform`.
    *   `ResetUsedAnchorTracking()`: Clears a temporary dictionary that tracks which anchor points have been used during the current spawn sequence.
    *   `GetSpawnPointInCar(string carNameKey)`:
        *   Finds the car's transform using `GetCarTransform`.
        *   Finds the `RailCarFloor` child.
        *   Finds all `Anchor` children under the floor.
        *   Selects the first *unused* `Anchor` for this car in the current spawn sequence (tracked in `usedAnchorsThisSpawn`). If all are used, it reuses the first one.
        *   Finds the "walkway" child under the chosen `Anchor`.
        *   Uses `NavMesh.SamplePosition` with a radius of **5.0f** to find the closest point on the NavMesh to the *walkway's* position.
        *   If successful, marks the parent `Anchor` as used and returns the valid NavMesh point.
        *   If `NavMesh.SamplePosition` fails (walkway too far from mesh), it falls back to sampling near the car's center.
        *   If center sampling also fails, it returns the raw car center (likely resulting in NavMeshAgent failure).
*   **`NPCManager.cs`:**
    *   `availableAnimContainers` (Serialized Array): Holds references to the four `NPCAnimContainer` assets (`Winchester1.asset` to `Winchester4.asset`), **must be assigned manually in the Inspector**.
    *   `SpawnNPCInCar(string characterName, Vector3 position, Transform carTransform, int characterIndex)`:
        *   Instantiates the `NPC.prefab`.
        *   Checks if the instance has an `NPCAnimManager`. If not, it **adds the component dynamically** (`AddComponent<NPCAnimManager>()`) and logs a warning.
        *   Gets the `NPCAnimManager` component.
        *   Assigns an `NPCAnimContainer` from the `availableAnimContainers` array using `characterIndex % availableAnimContainers.Length`. Logs a warning if the array or specific container is null.
        *   Calls `SetAnimContainer` on the `NPCAnimManager`.
        *   Initializes the `Character.cs` component.
        *   Attempts to place the NPC using direct `transform.position` assignment (after temporarily disabling/re-enabling the NavMeshAgent), rather than `agent.Warp()`.
*   **`NPCAnimManager.cs`:**
    *   `SetAnimContainer(NPCAnimContainer container)`: Receives the assigned container from `NPCManager` and stores it in the `anims` field, setting the initial `currentAnim`.
    *   `Awake()`: Gets references and the initial local scale of the assigned `sprite` transform.
    *   `UpdateDirection()` / `DirectionOverride()`: Applies horizontal scale flipping (multiplying X scale by -1) to the **`sprite.transform.localScale`** to change the visual facing direction, **not** the root transform's scale. This prevents interference with the NavMeshAgent.
*   **`NPCMovement.cs`:**
    *   Contains safety checks (`agent.isOnNavMesh`) before attempting NavMeshAgent operations (`SetDestination`, `isStopped`).
    *   Calls to `animator.SetBool` and `animManager.ApplyAnim` are now **re-enabled**.
*   **`NPC.prefab` (`Assets/Prefabs/Characters/NPC.prefab`):** The template used for all NPCs.
    *   **Must** have `NavMeshAgent`, `Character`, `NPCMovement` components attached.
    *   **Should** have `NPCAnimManager` component attached (currently missing, added dynamically at runtime).
    *   **Should** have `Animator` component with "Apply Root Motion" **OFF**.
*   **Car Prefabs (e.g., `Assets/Resources/TrainCars/shell_car_new.prefab`):** Used by `TrainLayoutManager` (via `TrainManager`) to build the train.
    *   **Must** have a child structure like `RailCarFloor` -> `Anchor (x, y)` -> `walkway`.
    *   The `RailCarFloor` **must** have a correctly baked `NavMeshSurface`.
    *   The "walkway" child objects **must** be positioned such that `NavMesh.SamplePosition` (with a 5.0f radius) can find a point on the baked NavMesh. Ideally, they should be directly on or very close to the NavMesh surface.

## 3. System Dependencies & Requirements

*   **Character JSON:** Each character file in `Assets/StreamingAssets/Characters/` must have a top-level `"initial_location": "car_key"` field, where `"car_key"` exactly matches a key generated by `TrainLayoutManager.NameCars` (e.g., "business\_class", "business\_class\_1").
*   **Mystery Layout:** The `LayoutOrder` in the main mystery JSON (`transformed-mystery.json`) defines the sequence and base keys for the train cars.
*   **Prefab Configuration:**
    *   The `NPC.prefab` must have required components (`NavMeshAgent`, `Character`, `NPCMovement`). It *should* also have `NPCAnimManager` and `Animator` (with Root Motion OFF).
    *   The car prefabs used (currently `shell_car_new.prefab`) must have the `RailCarFloor` -> `Anchor` -> `walkway` structure and a correctly baked NavMesh on the floor. Walkways must be positioned near the NavMesh.
*   **Inspector Assignment:** The four `WinchesterX.asset` files **must** be manually assigned to the `Available Anim Containers` array on the `NPCManager` component in the `SystemsTest` scene Inspector.
*   **NavMesh:** A valid NavMesh must be baked for the train car interiors (specifically the `RailCarFloor` of the prefabs used).

## 4. Current Status & Remaining Issues (As of 2025-04-08 12:07 AM)

*   **Prefab Setup:** `NPC.prefab` now correctly includes `NPCAnimManager` with its `Sprite`, `Animator`, and `Movement Control` references assigned in the Inspector. `Animator` has "Apply Root Motion" OFF.
*   **Inspector Assignments:** `NPCManager` has its `availableAnimContainers` array correctly assigned.
*   **Script Errors:** Previous `NullReferenceException` errors and warnings related to missing components/references are resolved.
*   **Spawn Point Selection:** `TrainLayoutManager.GetSpawnPointInCar` correctly identifies central anchors (e.g., "Anchor (3, 7)") and finds valid NavMesh points near them.
*   **Initial Placement:** NPCs are placed correctly near the center of their designated car using direct transform assignment.
*   **[RESOLVED] Position Corruption & Glitchy Movement:** The previous issue where NPCs would snap to the edge of the NavMesh or have their X coordinate flipped upon stopping movement or starting idle has been resolved.
    *   **Cause:** The issue stemmed from `NPCAnimManager.cs` modifying the `transform.root.localScale` to visually flip the sprite. This interfered with the `NavMeshAgent`'s internal position calculations, causing it to believe it was off-mesh or in an invalid state when movement stopped, resulting in the snap to the nearest edge.
    *   **Fix:** The `NPCAnimManager` script was modified to apply the scale flip only to the child `sprite.transform.localScale` instead of the root transform. This isolates the visual flip from the NavMeshAgent's positioning logic.
*   **Movement:** NPCs now correctly navigate the NavMesh, stop, idle, and resume movement without position corruption.
*   **[Remaining Warning] `NPCAnimManager ... Awake: 'anims' ... is null`:** This warning persists due to script execution order (`Awake` runs before `SetAnimContainer` is called by `NPCManager`). It is considered cosmetic as the container *is* assigned correctly later, preventing runtime errors.

## 5. Known Issues / Future Considerations

*   The cosmetic `NPCAnimManager` Awake warning could be resolved by ensuring `SetAnimContainer` is called before the first frame where `anims` might be accessed (e.g., by adjusting script execution order or initialization timing), but it's low priority as it doesn't break functionality.
*   Ensure the `NPC.prefab` hierarchy remains consistent (i.e., the `SpriteRenderer` used for flipping stays as a child of the root object containing the `NavMeshAgent`).
