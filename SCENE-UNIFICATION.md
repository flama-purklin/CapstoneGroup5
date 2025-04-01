# Scene Unification Plan

## Overview
This document outlines the plan to unify the LoadingScreen and SystemsTest scenes in the Mystery Game project. The goal is to eliminate the artificial separation between loading and gameplay by incorporating the loading functionality directly into the SystemsTest scene as an overlay.

## Current Architecture
- **LoadingScreen Scene**: Entry point, handles initialization
- **SystemsTest Scene**: Main gameplay environment
- **PersistentSystemsManager**: Maintains objects across scene transitions

## Proposed Architecture

### 1. Architectural Overview

Rather than having two separate scenes (LoadingScreen and SystemsTest), we'll:
1. Keep only the SystemsTest scene as the main scene
2. Add a LoadingOverlay GameObject to the SystemsTest scene that handles initialization
3. Make the LoadingOverlay manage the initialization process before revealing the game

### 2. Component Structure

**LoadingOverlay GameObject:**
- Canvas (Screen Space - Overlay, highest sort order)
- LoadingUI script (repurposed from existing LoadingUI)
- Progress bar, spinner, and status text elements
- Background to block visibility of game elements

### 3. Game Initialization Flow

1. **Initial State:**
   - SystemsTest scene loads with LoadingOverlay active
   - Game elements exist but are blocked by the overlay
   - Player input is disabled during initialization

2. **Initialization Process:**
   - LLM initialization
   - Mystery parsing & character extraction
   - NPC initialization
   - All managed by a new InitializationManager component

3. **Transition to Gameplay:**
   - Fade out LoadingOverlay when initialization completes
   - Enable player input and interaction
   - Resume normal gameplay

## Implementation Status

### Completed Items ✅
1. Basic LoadingOverlay GameObject in SystemsTest scene
2. InitializationManager script created and attached
3. GameState.LOADING state added
4. GameInitializer updated to work with unified scene approach
5. LoadingUI script refactored for programmatic configuration
6. MainMenu updated to load SystemsTest directly
7. Added timeouts for all initialization steps 
8. Fixed the TMP_Text with TextMeshProUGUI
9. Added proper error handling for Mystery parsing
10. Added robust exception handling
11. Added recovery mechanisms for critical errors
12. Refactored MysteryCharacterExtractor to work within the CoreControl.MysteryParsing namespace
13. Fixed the LoadingOverlay references in the InitializationManager
14. Updated InitializationManager with proper references to LLM and CharacterManager
15. Created the LoadingOverlay GameObject with all required components
16. Eliminated namespace conflicts between different implementations

### Items Currently Implemented ✅

1. **Scene Setup**:
   - LoadingOverlay GameObject has been added to the scene
   - LoadingOverlay and LoadingUI scripts have been attached
   - Canvas component configured with proper render mode and sorting order
   - UI elements (background panel, progress bar, status text, spinner) added
   - InitializationManager created and configured with proper references

2. **Character Extraction System**:
   - MysteryCharacterExtractor implemented as a standalone component in the CoreControl.MysteryParsing namespace
   - Original extractor renamed to MysteryCharacterExtractorCore to avoid conflicts
   - ParsingControl updated to work with the namespaced extractor
   - Editor scripts updated to find components using the correct namespaces
   
3. **Component References**:
   - InitializationManager references properly set to LLM, CharacterManager, and LoadingOverlay
   - Editor tools created to set up and fix references automatically:
     - SetupInitializationReferences.cs: Sets up references for the InitializationManager
     - FixLoadingOverlay.cs: Ensures LoadingOverlay has the correct components

### Items Remaining

1. **Testing and Verification**:
   - Run full initialization cycle to verify all components work together
   - Test error handling for various failure scenarios
   - Verify proper transition from loading to gameplay state
   - Profile the loading process to identify any performance bottlenecks

2. **Final Integration**:
   - Update the build settings to use SystemsTest as the initial scene
   - Remove any remaining references to the separate LoadingScreen scene
   - Ensure MainMenu scene transitions directly to SystemsTest
   - Verify gameplay is properly disabled during initialization

3. **Cleanup**:
   - Remove redundant components that were needed for the two-scene approach
   - Remove or refactor PersistentSystemsManager
   - Update documentation to reflect the new scene architecture

## Testing Process
1. Verify LoadingOverlay displays correctly at startup
2. Confirm all initialization steps complete successfully
3. Ensure proper transition to gameplay
4. Test error conditions (missing LLM, parsing failures)
5. Measure load times compared to previous approach

## Error Handling
The updated implementation includes robust error handling:
1. Timeouts for all initialization steps to prevent hanging
2. Fallback mechanisms for critical failures
3. Recovery from most error states to ensure game remains usable
4. Exception catching and reporting for better debugging

## Redundant Components to Remove

With the unification of scenes, the following components become redundant and can be removed or refactored:

1. **PersistentSystemsManager**: No longer needed as there is no transition between scenes
2. **CoreSystemsManager**: Can be simplified as duplicate prevention is no longer necessary
3. **SceneManager References**: Remove any code related to loading scenes
4. **DontDestroyOnLoad Calls**: No longer necessary since everything is in one scene

## Implementation Notes

### MysteryCharacterExtractor Namespace Resolution
The project previously had two implementations of MysteryCharacterExtractor causing namespace conflicts:
1. The original in `Assets/Mystery/Myst_Play/Dialogue/LLM/MysteryCharacterExtractor.cs`
2. The new one in `Assets/Scripts/CoreControl/MysteryParsing/MysteryCharacterExtractor.cs`

To resolve this, we:
1. Renamed the original to `MysteryCharacterExtractorCore` to avoid naming conflicts
2. Placed the new implementation in the `CoreControl.MysteryParsing` namespace
3. Made the new implementation self-sufficient without dependencies on the original
4. Updated `ParsingControl.cs` to use the namespaced version
5. Updated editor tools to find and reference the correct implementations

These changes allow both implementations to coexist while eliminating compile errors and maintaining functionality.

### Editor Tools
Two editor tools were created to help with the scene unification process:
1. **SetupInitializationReferences**: Found in `Assets/Scripts/Editor/SetupInitializationReferences.cs`
   - Automatically finds and assigns references to the InitializationManager
   - Creates missing components when needed
   - Can be accessed via the Tools menu

2. **FixLoadingOverlay**: Found in `Assets/Scripts/Editor/FixLoadingOverlay.cs`
   - Sets up the LoadingOverlay with all required components
   - Configures Canvas, CanvasScaler, and UI elements
   - Assigns references to the InitializationManager
   - Can be accessed via the Tools menu