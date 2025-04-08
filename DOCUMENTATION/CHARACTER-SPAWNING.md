# Character Spawning System Overview (Updated 2025-04-07)

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
        *   Attempts to place the `NavMeshAgent` using `agent.Warp(position)`.
*   **`NPCAnimManager.cs`:**
    *   `SetAnimContainer(NPCAnimContainer container)`: Receives the assigned container from `NPCManager` and stores it in the `anims` field, setting the initial `currentAnim`.
    *   `Awake()`: Contains a null check for `anims` but may still log a warning if the component was added dynamically (as `Awake` runs before `SetAnimContainer`).
    *   `Update()`: Contains a null check for `movementControl` to prevent errors if the component was added dynamically and the reference wasn't set (though it attempts `GetComponent` as a fallback).
    *   `DirectionOverride()`: Contains a null check for `anims` to prevent errors if called before `SetAnimContainer`.
*   **`NPCMovement.cs`:**
    *   Contains safety checks (`agent.isOnNavMesh`) before attempting NavMeshAgent operations (`SetDestination`, `isStopped`).
    *   Calls to `animator.SetBool` and `animManager.ApplyAnim` are **currently commented out** for testing purposes.
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
*   **Initial Placement:** Debug logs confirm `NPCManager.SpawnNPCInCar` successfully warps NPCs to the correct central NavMesh point (`agent.isOnNavMesh` is true immediately after warp).
*   **[CORE ISSUE] Position Corruption & Glitchy Movement:** Despite correct initial placement, NPCs are observed visually spawning/snapping to the edge of the train car (often with a flipped positive X coordinate). Debug logs confirm their position is correct immediately after warp and at the start of `NPCMovement.IdleState`, but becomes incorrect (X flipped) by the time `MovementState` starts or shortly after. This leads to:
    *   Erratic "spasming" or "pulling back" movement as they try to pathfind from an invalid location.
    *   Frequent `NPCMovement (...): Failed to find valid NavMesh point near... Returning to Idle.` warnings, as pathfinding fails from the corrupted position.
*   **[Ruled Out Causes]:**
    *   Incorrect `NPCAnimContainer` assignment.
    *   Missing `NPCAnimManager` component or its internal references.
    *   `BillboardEffect` component interference (disabling it didn't fix the flip).
    *   Animation system calls during movement (disabling them didn't fix the flip).
    *   `NavMeshAgent.updatePosition = false` (caused NPCs to disappear).
    *   Simple `NavMesh.SamplePosition` failures during initial spawn (logs show it succeeds).
    *   Incorrect anchor selection logic in `TrainLayoutManager` (logs show correct anchor is chosen).
*   **[Remaining Warning] `NPCAnimManager ... Awake: 'anims' ... is null`:** This warning persists due to script execution order (`Awake` vs. `SetAnimContainer` call). It is considered cosmetic now as the container *is* assigned correctly later.

## 5. Troubleshooting / Next Steps

1.  **Investigate Position Corruption:** The primary goal is to find what modifies the NPC's `transform.position` (specifically flipping the X coordinate) *after* `NPCManager.SpawnNPCInCar` successfully warps it, but *before* or *during* the initial execution of `NPCMovement` coroutines. Possible causes:
    *   **Script Execution Order:** Another script's `Awake`, `OnEnable`, or `Start` modifying the transform.
    *   **Physics Interaction:** Immediate physics update conflict with `Rigidbody` or colliders after warp.
    *   **NavMeshAgent Internal State:** An issue with how the agent handles its position immediately after `Warp`.
    *   **Parenting Side Effects:** Unlikely, but the parenting to `characterContainer` could have a frame-delay issue.
2.  **Interactive Debugging:** Requires using the Unity Editor's debugger or frame-by-frame stepping to pinpoint the exact moment and script causing the position change after the initial warp. Inspecting `Transform` and `NavMeshAgent` properties during stepping is crucial.
3.  **Component Isolation:** Systematically disable other components on the `NPC.prefab` (e.g., `Rigidbody`, `Character.cs`) to see if the position flip stops.
