# Mystery Generator Components

## Overview
This directory contains two types of mystery generation systems:

1. **Primary Mystery Pipeline** (Used in production):
   - Uses the `transformed-mystery.json` file in `Assets/StreamingAssets/MysteryStorage/`
   - Follows the standardized pipeline described in CLAUDE.md
   - Managed by the ParsingControl system for character extraction and scene setup

2. **Legacy Test Generator** (MysteryGenBeta):
   - A prototype system that generates simpler test mysteries
   - Creates test characters like "Alice", "Bob", "Charlie" with randomly assigned roles
   - Produces debug output about gangs, train cars, and simplified evidence
   - **Not used in production** and should remain disabled unless testing

## Usage Guidelines

### Primary Mystery Pipeline
- This is the production system and should be the only active mystery generation system
- Do not modify the standard pipeline unless making intentional changes to the game design
- Character data is processed through ParsingControl and MysteryCharacterExtractor

### Legacy Test Generator
- The `MysteryGenTest` prefab contains the `MysteryGenRunner` component
- This component is now disabled by default via the `useTestGenerator` flag
- Only enable this flag when specifically testing the prototype generator
- Be aware that enabling it will create a secondary mystery that may conflict with the primary one

## Files

- `MysteryGenBeta.cs` - Contains the prototype mystery generator classes
- `MysteryGenRunner.cs` - MonoBehaviour that runs the prototype generator (disabled by default)
- `NodeControl.cs` - Helper for visualizing mystery nodes in the prototype system
- `ConstellationToggle.cs` - UI toggle for showing/hiding the mystery board

## Important Note

If you find both mysteries being generated simultaneously (character sets from both systems appearing in logs), check:

1. The `MysteryGenTest` prefab in any active scenes
2. Ensure `useTestGenerator` is set to false on all `MysteryGenRunner` components
3. Check for any manual calls to `GenerateMystery()` in active scripts

The standard mystery pipeline using transformed-mystery.json should be the only active system during normal gameplay.