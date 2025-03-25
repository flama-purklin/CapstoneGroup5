# Mystery Engine Refactoring

## Current Status and Issues

The project currently has serious integration issues caused by multiple copies of the same scripts in different locations:

1. Duplicate scripts in:
   - `_Core/Systems/`
   - `_Core/World/`
   - `_Core/WorldSystems/`

2. Multiple class definitions for key components:
   - `WorldCoordinator`
   - `LocationRegistry`
   - `TrainGenerator`
   - `EntityPlacer`
   - etc.

3. Compilation errors due to these duplicates that prevent the scripts from being attached to GameObjects.

## Consolidation Plan

1. Use the `DuplicateScriptFixer` tool:
   - Menu: "Mystery Engine/Fix Duplicate Scripts"
   - This will consolidate all duplicate scripts into the `_Core/WorldSystems/` directory
   - It will add the `MysteryEngine` namespace to scripts to prevent conflicts

2. Use the `SceneComponentFixer` tool:
   - Menu: "Mystery Engine/Fix Scene Components"
   - This will attach the correct components to the objects in the scene
   - It will set up references between components

3. Run the Test Harness:
   - Add the `TestRunnerPrefab` to the scene
   - Assign the `WorldCoordinator` and test data
   - Run the tests to validate the refactoring

## Architecture Overview

The refactored architecture is built around a central "reality model" that serves as the single source of truth for the game state:

1. **WorldCoordinator**: The central manager that bridges mystery data and the physical world.
   - Manages train generation, character placement, and evidence placement
   - Acts as the central reference point for all game systems

2. **LocationRegistry**: Maps location IDs from the JSON to actual transforms in the scene.
   - Allows finding locations by ID: `GetLocation("car_id.location_id")`
   - Maintains hierarchy information for locations

3. **TrainGenerator**: Creates train cars based on the train layout specification from the mystery.
   - Dynamically spawns cars based on the `TrainLayout` data
   - Creates location points within cars for character/evidence placement

4. **EntityPlacer**: Places characters and evidence in their designated locations.
   - Uses the `LocationRegistry` to find the correct transforms
   - Interfaces with `NPCManager` and `EvidenceManager` for actual entity creation

## Next Steps

1. Fix duplicate script issues using the provided tools
2. Set up test train car prefabs with proper components
3. Configure the scene with the correct hierarchy
4. Run tests to validate the refactoring