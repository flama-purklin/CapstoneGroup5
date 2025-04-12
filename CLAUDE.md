# CLAUDE.md - Capstone Group 5 Guidelines

## Project Overview

This Unity project implements a mystery game built around a modular "black box" design that processes mystery JSON files to create unique gameplay experiences. The game leverages an LLM (Large Language Model) integration for dynamic character interactions and dialogue on a train setting where players solve a mystery through conversations with NPCs.

## Unity Project Commands
- **Run Game**: Open project in Unity Editor and press Play button
- **Build**: File > Build Settings > Build (Windows/Mac/Linux)
- **Test Scene**: Open specific scene and use Play button to test
- **Clean Project**: Delete Library/ and Temp/ folders, reopen Unity

## Unity MCP Setup
- **Connect Claude Code to Unity MCP**: Use the following command:
  ```bash
  claude mcp add unity-mcp -- python3 /mnt/c/Users/charl/AppData/Local/Programs/UnityMCP/UnityMcpServer/src/server.py
  ```

## Code Style Guidelines
- **Naming**: PascalCase for classes/public methods, camelCase for variables
- **Organization**: Keep scripts in Assets/Scripts/ under appropriate folders
- **Comments**: Document complex logic and public API methods
- **Error Handling**: Use try/catch for external dependencies, Debug.Log for errors
- **Structure**: MonoBehaviours for scene objects, static classes for utilities
- **Imports**: Group Unity imports first, then system imports, then project imports
- **LLM Integration**: Use LLMUnity for character dialogue, follow pattern in existing NPCs

## Core Architecture
- **Unified Scene**: The game uses a single scene architecture (SystemsTest) with all core systems initialized there
- **Singleton Managers**:
  - **GameControl**: Main game state and mystery data manager
  - **CoreSystemsManager**: Manages Unity systems
  - **ParsingControl**: Handles mystery JSON parsing 
  - **NPCManager**: Manages NPC creation and behavior
  - **CharacterManager**: Manages LLM character creation and interaction
  - **InitializationManager**: Orchestrates the startup sequence
- **Game State Machine**:
  - States: DEFAULT, DIALOGUE, PAUSE, FINAL, WIN, LOSE, MINIGAME, MYSTERY
  - Changed via `GameControl.GameController.currentState`

## LLM Integration & Character System
- Uses LLMUnity package for character dialogue
- Characters have state transitions: LoadingTemplate → WarmingUp → Ready
- **Proximity-Based Warmup System**: Only keeps nearby characters "warm" (active in memory)
- **Conversation History**: Persists between interactions
- **IMPORTANT**: LLM's `parallelPrompts` setting in Inspector controls how many characters can be processed simultaneously
- Character data comes from parsed mystery JSON in `GameControl.coreMystery.Characters`

## NPC System
- NPCs linked to LLMCharacters via Character component 
- Contains NavMeshAgent for movement
- NPCMovement handles state transitions (Idle ↔ Movement)
- NPCAnimManager manages animations and sprite flipping
- Spawn locations determined by character's InitialLocation field

## Dialogue System
- Player enters dialogue range, presses interaction key
- DialogueControl activates UI
- LLMDialogueManager handles player input and LLM responses
- Conversation history should persist between dialogues

## Initialization Sequence
1. Scene loads, managers awake (ParsingControl reads mystery JSON into GameControl)
2. InitializationManager coordinates startup:
   - Waits for LLM startup
   - Confirms parsing complete
   - Triggers CharacterManager initialization
   - Builds train layout
   - Waits for character initialization
   - Initializes NPCManager
   - Spawns NPCs based on location data
   - Completes initialization, enabling gameplay

## Asset Structure
- **Scripts**: `Assets/Scripts/` (CoreControl/, NPCs/, Player/, Train/, UI/)
- **Mystery**: `Assets/Mystery/` (Myst_Gen/, Myst_Play/)
- **StreamingAssets**: `StreamingAssets/MysteryStorage/` for mystery JSON

## Unity MCP Server Tools and Best Practices—Use whenever accessing or changing Unity-related elements like scenes, objects, components, and editor scripts:

1. **Editor Control**
   - `editor_action` - Performs editor-wide actions such as `PLAY`, `PAUSE`, `STOP`, `BUILD`, `SAVE`
   - `read_console(show_logs=True, show_warnings=True, show_errors=True, search_term=None)` - Read and filter Unity Console logs
2. **Scene Management**
   - `get_current_scene()`, `get_scene_list()` - Get scene details
   - `open_scene(path)`, `save_scene(path)` - Open/save scenes
   - `new_scene(path)`, `change_scene(path, save_current)` - Create/switch scenes

3. **Object Management**
   - ALWAYS use `find_objects_by_name(name)` to check if an object exists before creating or modifying it
   - `create_object(name, type)` - Create objects (e.g. `CUBE`, `SPHERE`, `EMPTY`, `CAMERA`)
   - `delete_object(name)` - Remove objects
   - `set_object_transform(name, location, rotation, scale)` - Modify object position, rotation, and scale
   - `add_component(name, component_type)` - Add components to objects (e.g. `Rigidbody`, `BoxCollider`)
   - `remove_component(name, component_type)` - Remove components from objects
   - `get_object_properties(name)` - Get object properties
   - `find_objects_by_name(name)` - Find objects by name
   - `get_hierarchy()` - Get object hierarchy
4. **Script Management**
   - ALWAYS use `list_scripts(folder_path)` or `view_script(path)` to check if a script exists before creating or updating it
   - `create_script(name, type, namespace, template)` - Create scripts
   - `view_script(path)`, `update_script(path, content)` - View/modify scripts
   - `attach_script(object_name, script_name)` - Add scripts to objects
   - `list_scripts(folder_path)` - List scripts in folder

5. **Asset Management**
   - ALWAYS use `get_asset_list(type, search_pattern, folder)` to check if an asset exists before creating or importing it
   - `import_asset(source_path, target_path)` - Import external assets
   - `instantiate_prefab(path, pos_x, pos_y, pos_z, rot_x, rot_y, rot_z)` - Create prefab instances
   - `create_prefab(object_name, path)`, `apply_prefab(object_name, path)` - Manage prefabs
   - `get_asset_list(type, search_pattern, folder)` - List project assets
   - Use relative paths for Unity assets (e.g., 'Assets/Models/MyModel.fbx')
   - Use absolute paths for external files

6. **Best Practices**
   - ALWAYS verify existence before creating or updating any objects, scripts, assets, or materials
   - Use meaningful names for objects and scripts
   - Keep scripts organized in folders with namespaces
   - Verify changes after modifications
   - Save scenes before major changes
   - Use full component names (e.g., 'Rigidbody', 'BoxCollider')
   - Provide correct value types for properties
   - Keep prefabs in dedicated folders
   - Regularly apply prefab changes
   - Monitor console logs for errors and warnings
   - Use search terms to filter console output when debugging