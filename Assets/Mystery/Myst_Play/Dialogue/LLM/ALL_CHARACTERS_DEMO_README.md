# All Characters Demo Setup

This README explains how to set up and run the All Characters Demo, which loads all character JSON files and spawns them in the SystemsTest scene.

## Installation

### Step 1: Configure LoadingScreen Scene

1. Open the "LoadingScreen" scene in the Unity Editor
2. Find the LLM GameObject in the scene hierarchy
3. Add the LLMConfig component to the LLM GameObject:
   - Click "Add Component" and search for "LLMConfig"
   - Make sure "Auto Configure For All Characters" is checked

### Step 2: Configure SystemsTest Scene

1. Open the "SystemsTest" scene in the Unity Editor
2. Drag the "AllCharacterDemoPrefab" prefab from the Project panel into the scene hierarchy
   - The prefab is located at "Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemoPrefab"
   
### Step 3: Tag Train Cars (Optional but Recommended)

1. In the SystemsTest scene, find the train car GameObjects
   - They are typically under "TrainManager > Rail Cars"
2. Select each train car and add the tag "Train" to them
   - Click on the Tag dropdown in the Inspector
   - Select "Add Tag..." if "Train" doesn't exist yet
   - Create the "Train" tag and apply it to all train cars

### Step 4: Verify Sprite Resources

1. Make sure the Winchester sprite sets in Assets/Sprites/Winchester are available
2. These sprite sets (1-4) will be used cyclically for all NPCs

## How It Works

The demo implementation modifies several key scripts to enable loading all characters at once:

1. **LLMConfig**: Sets the LLM's parallelPrompts field to match the number of characters
2. **CarCharacters**: Modified to include all characters rather than filtering by used characters
3. **GameInitializer**: Updated to trigger character spawning after the SystemsTest scene loads
4. **AllCharacterDemo**: Added to monitor character spawning and provide status information

## Testing the Implementation

1. Start from the "MainMenu" scene
2. Click "Start" to begin the game
3. The LoadingScreen will appear and initialize the LLM and characters
4. The system will automatically load all character JSON files and spawn them in the SystemsTest scene
5. Check the Unity Console for logs showing the character spawning process
6. When in the game, you can interact with any character to test the dialogue system

## Troubleshooting

If characters aren't spawning correctly:

1. Check the Unity Console for error messages
2. Verify that the LLMConfig component is attached to the LLM GameObject
3. Ensure the AllCharacterDemoPrefab is present in the SystemsTest scene
4. Check that all character JSON files are present in StreamingAssets/Characters

## Note for Developers

This implementation modifies the character spawning logic to:
- Configure the LLM to handle all characters concurrently
- Bypass the "used characters" filter in CarCharacters.cs
- Add a static SpawnAllAvailableCharacters() method for manual spawning
- Implement a helper to distribute characters evenly across train cars