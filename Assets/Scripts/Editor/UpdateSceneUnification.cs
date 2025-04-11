using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor tool to update the SCENE-UNIFICATION.md file with detailed instructions
/// </summary>
[ExecuteInEditMode]
public class UpdateSceneUnification : Editor
{
    [MenuItem("Tools/Update SCENE-UNIFICATION Documentation")]
    public static void UpdateDocumentation()
    {
        string mdPath = Path.Combine(Application.dataPath, "..", "SCENE-UNIFICATION.md");
        
        string content = @"# Scene Unification Implementation Guide

## Overview

This document outlines the implementation of a unified scene approach for the game, eliminating the artificial separation between loading and gameplay. Instead of using separate scenes, we incorporate the loading functionality directly into the SystemsTest scene as an overlay.

## Implementation Details

### 1. Scene Setup (SystemsTest scene):

The SystemsTest scene now contains the following key components:

- **InitializationManager**: Coordinates the entire initialization process (LLM startup, mystery parsing, character setup)
- **LoadingOverlay**: UI layer that shows progress during initialization
- **LLM**: Language model component for character dialogue
- **CharacterManager**: Manages LLM character creation and dialogue processing
- **NPCManager**: Handles NPC spawning and positioning in the scene

### 2. **Scene LLM Setup (CRITICAL)**:
- **Important**: The LLM must be set up as a scene GameObject for proper model loading
- Create a new GameObject named ""LLM"" in the Scene hierarchy
- Add the LLM component to it
- Configure the LLM component properly:
  - **Model Path**: Set this to a valid .gguf file in your project
  - **GPU Acceleration**: Configure as needed
  - **Num Threads**: Set to -1 (use all cores)
  - **Context Size**: Set to 12288 or higher

### 3. State Management:

- **GameState.LOADING**: Used during the initialization phase
- **GameState.DEFAULT**: Set once initialization is complete
- These states control what UI is shown and which gameplay systems are active

### 4. Initialization Process:

1. **LLM Startup**: Wait for the LLM to initialize with a timeout
2. **Mystery Parsing**: Extract character data from the mystery JSON
3. **Character Setup**: Initialize NPCs and LLM character templates
4. **Transition**: Hide loading overlay and enable player input

### 5. Error Handling:

- Timeout mechanisms prevent hanging in each initialization phase
- Recovery mechanisms ensure the game transitions to gameplay even if errors occur
- Detailed console logs provide diagnostic information

## Using the Editor Tools

Several editor tools have been created to help with the setup:

1. **Setup Scene References**: Configures all required references between components
2. **Fix LoadingOverlay References**: Ensures the LoadingOverlay is properly configured
3. **Update SCENE-UNIFICATION Documentation**: Updates this document with the latest information

## Common Issues and Solutions

### LLM Not Starting
- Ensure a valid model path is set in the LLM component
- Check console for error messages related to the model file
- Verify GPU acceleration is configured properly for your hardware

### Missing References
- Run ""Setup Scene References"" tool from the Tools menu
- Manually check that InitializationManager has references to:
  - LLM
  - CharacterManager
  - NPCManager
  - LoadingOverlay

### LoadingOverlay Not Working
- Run ""Fix LoadingOverlay References"" tool from the Tools menu
- Check that the Canvas and UI components are properly configured
- Ensure TextMeshProUGUI components are used (not TMP_Text abstract class)

### Character Creation Failing
- Verify that the StreamingAssets/Characters folder exists
- Check that the StreamingAssets/MysteryStorage folder contains a valid mystery JSON
- Look for errors in the character extraction process in the console

## Technical Details for Developers

### InitializationManager
The core class that orchestrates the initialization process. Uses a step-by-step approach with timeouts:
1. WaitForLLMStartup
2. WaitForParsingComplete
3. InitializeCharactersAndNPCs
4. CompleteInitialization

### LoadingOverlay
Manages the UI during loading, creating a ScreenSpace-Overlay canvas with a progress bar and status text.

### CharacterManager
Handles LLM character initialization and template loading. Works directly with the in-scene LLM component.

### NPCManager
Spawns NPCs in the scene and connects them to the appropriate LLM characters for dialogue.

";
        
        File.WriteAllText(mdPath, content);
        
    }
}
