# All Characters Demo

This feature allows all characters extracted from the mystery JSON to be spawned and active simultaneously across all train cars, with each character having their own unique appearance and personality.

## Implementation Details

### Overview
The ALL_CHARACTERS_DEMO implementation modifies the existing character spawning system to:
1. Load all available character JSON files from StreamingAssets/Characters
2. Configure the LLM to handle multiple characters concurrently via parallel prompts
3. Spawn all characters across available train cars with even distribution
4. Ensure each character has a unique visual appearance based on their name
5. Fix character name assignment to properly set GameObject names

### Key Components

#### 1. CharacterSpawnerService (New)
- Central service that manages the entire character spawning process
- Maintains a registry of all spawned characters with validation capability
- Handles character distribution across train cars
- Ensures consistent parent-child hierarchy and naming
- Performs automatic validation of character setup

#### 2. CarCharacters (Modified)
- `SpawnAllAvailableCharacters()` - Static method to trigger spawning all characters
- `DistributeCharactersEvenly()` - Helper method to spread characters across train cars
- Increased `maxCharacters` from 3 to 10 per car to accommodate more characters
- Added `AllCharacterDemo` detection for modified spawning behavior

#### 3. NPCManager (Modified)
- Enhanced `SpawnNPCInCar()` with proper parent-child hierarchies
- Improved NavMesh position validation with multiple sampling attempts
- Creates consistent GameObject naming structure:
  - Parent: "NPC_{characterName}"
  - Child: "CharacterBody_{characterName}"
- Added robust NavMesh position validation

#### 4. NPCAnimManager (Modified)
- Prioritized Character component as source of character name
- Enhanced sprite assignment logic using consistent name hashing
- Added better error detection and recovery for animations
- Better logging to track sprite assignment issues

#### 5. Character (Modified)
- Simplified `Initialize()` to directly set parent GameObject name
- Removed ambiguous hierarchy validation checks
- Improved error detection for character initialization
- Added reliable reflection-based animation forcing

#### 6. AllCharacterDemo (Modified)
- Integrated with CharacterSpawnerService
- Enhanced monitoring with automatic validation triggers
- Improved diagnostics about character hierarchy issues
- Added comprehensive sprite assignment debugging

#### 7. CharacterDebugger (New)
- Provides in-scene visualization of character placement issues
- Performs complete diagnostic checks on demand
- Validates character hierarchy, names, and NavMesh placement
- Helps identify specific character issues with detailed logging

### Setup Instructions

1. **Scene Configuration:**
   - Add the `CharacterSpawnerService` component to any persistent GameObject
   - Add the `AllCharacterDemo` component to any GameObject in your scene
   - Set `continuousMonitoring` to true for ongoing status updates
   - Optional: Add `CharacterDebugger` for visual debugging

2. **LLM Configuration:**
   - Add the `LLMConfig` component to your LLM GameObject
   - Set `autoConfigureForAllCharacters` to true for automatic setup
   - OR manually set `parallelPrompts` to match your character count

3. **Train Car Configuration:**
   - Increase `maxCharacters` to 10 on all CarCharacters components
   - Ensure all train cars are active in the scene hierarchy
   - OR the system will attempt to activate them automatically
   
4. **NavMesh Configuration:**
   - Verify all train car floors have NavMesh surfaces baked
   - Check that NavMesh Agent components are present on character prefabs
   - If characters still get stuck, try rebaking NavMesh with lower agent radius

## Troubleshooting

### Using the Character Debugger

The CharacterDebugger component provides visual and diagnostic tools:

1. Add the CharacterDebugger component to any GameObject
2. Click "Run Diagnostics" in the inspector or call `PerformFullDiagnostics()`
3. Check the console for a complete diagnostic report
4. In scene view, problematic characters are marked with red gizmos
5. The component tracks counts of different issue types

### Common Issues

1. **Characters have same appearance:**
   - This is now fixed by consistent sprite hash assignment
   - If still occurring, check logs for sprite assignment errors
   - Verify Character.CharacterName is being set correctly
   - Check that parent name is correct via CharacterDebugger

2. **Not all characters spawning:**
   - Check NPCManager initialization status in logs
   - Verify character container is created correctly
   - Confirm all train cars are active and have CarCharacters component
   - Look for NavMesh errors - characters need valid NavMesh positions

3. **Characters getting stuck:**
   - Check NavMesh configuration on train car floors
   - Verify character NavMeshAgent components are enabled
   - Try larger NavMesh Agent radius in prefab
   - Use CharacterDebugger to identify off-NavMesh characters

4. **Parent name mismatches:**
   - These should be automatically fixed by the new system
   - If still occurring, check the initialization order
   - Run CharacterSpawnerService.ValidateAllCharacters()
   - Inspect hierarchy to ensure parent objects follow "NPC_{characterName}" pattern

5. **LLM context issues:**
   - If characters share the same responses, context size may be too small
   - Reduce other LLM parameters to free up memory
   - Consider using a smaller language model for all-character demos

### Logging Output

The system logs detailed status information:
- Characters active in scene (count and names)
- Cars with characters (distribution stats)
- Available vs. spawned character counts
- Character name issues and inconsistencies
- Sprite assignment duplications

## Technical Notes

1. **Parent-Child Hierarchy**:
   The system now uses a consistent parent-child structure:
   - Parent GameObject: "NPC_{characterName}" - Contains NavMeshAgent
   - Child GameObject: "CharacterBody_{characterName}" - Contains actual character
   - This structure is enforced by the CharacterSpawnerService

2. **Object Naming**:
   - Consistent naming is critical for the animation and sprite assignment
   - The parent object name drives the character's visual identity
   - Character components reference the base character name without "NPC_" prefix
   - This naming scheme is enforced by validation systems

3. **NavMesh Positioning**:
   - Characters must be placed on valid NavMesh surfaces
   - The system now uses multiple attempts with increasing radius
   - If NavMesh issues persist, rebake NavMesh with more generous settings

4. **Performance considerations:**
   - Multiple characters increase memory usage (especially LLM context)
   - Full character set may impact framerate on lower-end devices
   - Train car visibility system works with this feature to limit active characters

## Character Files

Character JSON files should be located in:
`StreamingAssets/Characters/*.json`

Each file contains character personality, dialogue traits, and memory, following this structure:
```json
{
  "core": {
    "involvement": { ... },
    "whereabouts": [ ... ],
    "relationships": [ ... ]
  },
  "mind_engine": {
    "identity": { ... },
    "state_of_mind": { ... },
    "speech_patterns": { ... }
  },
  "initial_location": "car_id",
  "key_testimonies": { ... }
}
```

For the demo to work properly, ensure all character JSONs are correctly structured and extracted.

## Implementation History

This feature was created to resolve persistent issues with characters in the game:
1. Characters would spawn, disappear, and respawn in different train cars
2. Characters would get stuck on walls due to NavMesh positioning errors
3. Characters would have parent name mismatches causing sprite assignment issues
4. Multiple characters would share the same sprites

The original implementation used a loosely coupled system where each train car was responsible for spawning characters. This led to inconsistencies when characters were spawned multiple times. The updated implementation uses a centralized CharacterSpawnerService that manages the entire character lifecycle.