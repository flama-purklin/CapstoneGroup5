# All Characters Demo Mode

This feature enables all characters from the mystery to spawn simultaneously across the train cars.

## Setup Instructions

1. **Enable Demo Mode**
   - In the SystemsTest scene, ensure the `AllCharacterDemo` component is active
   - This will trigger the global character spawning service

2. **How It Works**
   - The `CharacterSpawnerService` manages all character spawning and validation
   - Characters are spawned once at game start and distributed across train cars
   - Each character maintains a consistent appearance based on its name
   - Prevents duplicate character spawning when moving between cars

3. **Troubleshooting**
   - If characters aren't appearing, check the console for errors
   - Ensure the `NPCManager` and `CharacterManager` are properly initialized
   - Look for "All Characters Demo Mode" log messages to confirm it's active
   - If characters appear with incorrect names, run validation with `CharacterSpawnerService.ValidateAllCharacters()`

4. **Implementation Details**
   - The system uses a centralized service architecture to manage character spawning
   - Characters are tracked in a registry to prevent duplicates
   - NavMesh positions are validated to prevent characters from getting stuck
   - All character GameObjects are named consistently as NPC_{characterName} 
   - Animation is assigned based on character name hash for consistency

## Setting Up a New Scene

To add All Characters Demo mode to a new scene:

1. Add an empty GameObject named "AllCharacterDemo"
2. Add the `AllCharacterDemo` component to it
3. Make sure train cars have `CarCharacters` components 
4. Each car should have a valid `carFloor` property set

The system will handle the rest automatically!