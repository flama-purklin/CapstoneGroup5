# Dynamic Train Layout System Documentation

## 1. Goals

The primary goal of this system is to establish a **robust, bidirectional connection** between the train layout defined in a loaded mystery JSON file and the actual train car GameObjects instantiated in the Unity scene.

This ensures that:
*   The sequence and type of train cars players experience **always** match the specification in the `environment.layout_order` section of the currently loaded mystery JSON.
*   The game can dynamically adapt the train environment based on different mystery files without requiring manual scene modifications for each mystery.
*   References to specific train cars within the mystery JSON (e.g., character whereabouts, evidence locations) remain consistent with the spawned environment.

## 2. Data Flow & Implementation

1.  **Mystery JSON Definition:**
    *   The mystery JSON file (e.g., `transformed-mystery.json`) defines the train structure within its `environment` section.
    *   `environment.layout_order`: An array of strings specifying the sequence of car keys.
    *   `environment.cars`: An object containing definitions for each car key (though this part is less critical now, the `layout_order` keys are the main driver).
    *   **Standardized Keys:** Specific keys (`first_class`, `business_class`, `dining_car`) are used in `layout_order` to represent car types that have dedicated prefab variants.
    *   **Fallback Keys:** Other keys (`storage_car`, `kitchen_car`, `second_class_car_1`, etc.) are used for car types that will default to a generic "shell" prefab.

2.  **Parsing:**
    *   `ParsingControl.cs` reads the mystery JSON during game initialization.
    *   The entire JSON structure, including `environment.layout_order`, is deserialized into the `GameControl.GameController.coreMystery` object (specifically `GameControl.GameController.coreMystery.Environment.LayoutOrder`).

3.  **Prefab Variants:**
    *   Specific Unity prefab variants exist for distinct car types (e.g., `Train Car (Demo Lux Pass).prefab`, `Train Car (Demo Pass Common).prefab`, `Train Car (Demo Dinning Common).prefab`).
    *   A fallback prefab (`Train Car (Demo Shell).prefab`) exists for generic or unmapped car types.
    *   These prefabs **must** be placed within a specific subfolder inside `Assets/Resources/` (e.g., `Assets/Resources/TrainCars/`).
    *   The prefab filenames **must exactly match** the corresponding standardized JSON key (e.g., `first_class.prefab`, `business_class.prefab`, `dining_car.prefab`, `shell_car.prefab`).

4.  **`TrainLayoutManager.cs`:**
    *   This script component (attached to a GameObject like "TrainController" in the scene) is responsible for building the train.
    *   **Configuration:** Requires the path within `Resources` (e.g., "TrainCars") and the filename of the fallback shell prefab (e.g., "shell_car") to be set (defaults provided).
    *   **Loading:** It dynamically loads prefabs using `Resources.Load<GameObject>()` based on the keys from the JSON `LayoutOrder`. It constructs the path using the configured `resourceFolderPath` and the `carKey`.
    *   **Fallback:** If `Resources.Load` fails to find a prefab matching a specific `carKey`, it uses the pre-loaded fallback `shellCarPrefab`.
    *   **Execution:** It's called by `InitializationManager.cs` *after* `ParsingControl` has finished loading the mystery data.
    *   **Logic:**
        *   Reads the `LayoutOrder` list from `GameControl.GameController.coreMystery.Environment`.
        *   Iterates through the list.
        *   For each key:
            *   Attempts to load `Resources/TrainCars/[carKey].prefab`.
            *   If successful, instantiates the loaded prefab.
            *   If unsuccessful, instantiates the fallback `shell_car.prefab`.
        *   Positions each instantiated car correctly relative to the previous one using the configured `carSpacing`.
        *   Names the instantiated GameObject clearly (e.g., `TrainCar_0_storage_car`, `TrainCar_1_first_class`).

5.  **Scene Setup:**
    *   The `SystemsTest` scene **must not** contain pre-placed static train car objects in its hierarchy. These must be deleted.
    *   The `TrainLayoutManager` component must be present in the scene (e.g., on a "TrainController" GameObject) and configured with a `Spawn Point` transform. The `Resource Folder Path` and `Shell Car Prefab Name` fields can usually be left as default ("TrainCars", "shell_car").

## 3. Considerations & Maintenance

*   **`Resources` Folder:** All required train car prefab variants **must** reside in the specified subfolder within `Assets/Resources/` (default: `Assets/Resources/TrainCars/`). Assets in the `Resources` folder are always included in the build, increasing build size. Consider using Addressables for larger projects if this becomes an issue.
*   **Prefab Naming Convention:** Prefab filenames within the `Resources` subfolder **must exactly match** the corresponding JSON keys (e.g., `first_class.prefab`, `shell_car.prefab`). Renaming a prefab requires updating its filename here.
*   **JSON Key Consistency:** Any new mystery JSON files must use the standardized keys (`first_class`, `business_class`, `dining_car`) for specific car types or use other keys (`storage_car`, `kitchen_car`, etc.) which will correctly fall back to the `shell_car.prefab`. All references to car locations within the JSON (whereabouts, evidence, etc.) must use these consistent keys.
*   **Static Car Deletion:** It is crucial that the static train cars are removed from the main gameplay scene (`SystemsTest`) to prevent duplication and ensure the dynamic system takes precedence.
*   **Error Handling:** The `TrainLayoutManager` includes basic checks for missing mystery data or unassigned prefabs, logging errors to the console. Further robustness could be added if needed.
*   **Car Spacing & Alignment:** The `carSpacing` value and spawn logic assume cars are aligned along a specific axis (currently `Vector3.left`). Adjustments may be needed if car pivot points or desired alignment change.
