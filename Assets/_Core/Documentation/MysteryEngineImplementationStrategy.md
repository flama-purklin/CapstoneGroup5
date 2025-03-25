# Mystery Engine Implementation Strategy

## Overview

This document outlines the implementation strategy for the Mystery Engine refactoring. The project aims to transform the current disconnected architecture into a cohesive, unified system that properly bridges the gap between mystery data (JSON) and the physical game world.

## Core Architecture

The new architecture is built around the concept of a central "reality model" that serves as the single source of truth for the game world:

1. **WorldCoordinator**: Central manager that bridges mystery data and the physical world
2. **LocationRegistry**: System for mapping location IDs to physical transforms
3. **TrainGenerator**: Procedural train generator based on mystery JSON
4. **EntityPlacer**: System for placing characters and evidence in the world

This architecture ensures a deterministic connection between mystery data and the physical game environment.

## Implementation Steps

### 1. Directory and Asset Organization

- Created a `_Core` directory with subdirectories for each subsystem
- Set up prefab directories for systems, train cars, and UI
- Organized scripts into logical folders by functionality

### 2. Core System Implementation

- Implemented `WorldCoordinator` as the central manager
- Implemented `LocationRegistry` for mapping location IDs to transforms
- Implemented `TrainGenerator` for procedurally generating train cars
- Implemented `EntityPlacer` for placing characters and evidence
- Created support components for identifying locations and spawn points

### 3. Integration with Existing Systems

- Created a modified `NPCManager` that works with the new architecture
- Implemented `EvidenceManager` for handling evidence objects
- Updated `TrainLayout` class to properly integrate with the Mystery model
- Created extension methods to extract train layout data from mystery JSON

### 4. Scene Consolidation

- Created a unified `MainScene` that replaces the separate loading and gameplay scenes
- Implemented `LoadingUIController` as an overlay instead of a separate scene
- Created scene setup utilities to ensure proper hierarchy and references

### 5. Testing Infrastructure

- Implemented a test harness for validating the implementation
- Created a test mystery JSON for verification
- Added detailed logging and test assertions for validation

## Key Components

### WorldCoordinator (Reality Model)

The `WorldCoordinator` serves as the central reality model, bridging the gap between mystery data and the physical world:

```csharp
public void InitializeWorld(Mystery mystery)
{
    // Generate train based on mystery layout
    trainGenerator.GenerateTrainFromLayout(mystery.GetTrainLayout());
    
    // Register all locations for later reference
    RegisterAllLocations();
    
    // Place characters in their initial positions
    PlaceCharactersInInitialLocations();
    
    // Place evidence objects
    PlaceEvidenceInLocations();
}
```

### LocationRegistry (Spatial Mapping)

The `LocationRegistry` maintains a mapping between location IDs in the mystery data and actual transforms in the world:

```csharp
public void RegisterLocation(string locationId, Transform locationTransform)
{
    locationTransforms[locationId] = locationTransform;
}

public Transform GetLocation(string locationId)
{
    if (locationTransforms.TryGetValue(locationId, out Transform locationTransform))
    {
        return locationTransform;
    }
    
    return null;
}
```

### TrainGenerator (Procedural World Generation)

The `TrainGenerator` creates the physical train layout based on the mystery specification:

```csharp
public void GenerateTrainFromLayout(TrainLayout layout)
{
    foreach (var carDef in layout.Cars)
    {
        GameObject carPrefab = GetCarPrefabByType(carDef.CarType);
        GameObject carInstance = Instantiate(carPrefab, position, rotation, trainParent);
        
        // Set up car identifier
        CarIdentifier carIdentifier = carInstance.AddComponent<CarIdentifier>();
        carIdentifier.CarId = carDef.CarId;
        carIdentifier.CarType = carDef.CarType;
        
        // Create location points within the car
        CreateLocationPoints(carInstance, carDef.AvailableLocations);
    }
}
```

### EntityPlacer (Character and Evidence Placement)

The `EntityPlacer` handles placing characters and evidence in their designated locations:

```csharp
public GameObject PlaceCharacter(string characterId, MysteryCharacter data, Transform location)
{
    // Find a suitable spawn point
    Transform spawnPoint = GetSpawnPointInLocation(location, "character_spawn");
    
    // Spawn the character at this location
    GameObject characterInstance = npcManager.SpawnNPCInCar(characterId, spawnPoint.position, location);
    
    return characterInstance;
}
```

## Integration with Mystery JSON

The system extracts train layout and initial character locations from the mystery JSON through extension methods:

```csharp
public static TrainLayout GetTrainLayout(this Mystery mystery)
{
    // First check if train_layout already exists directly
    if (mystery.TrainLayout != null)
    {
        return mystery.TrainLayout;
    }
    
    // If not, try to extract from environment data
    if (mystery.Environment != null)
    {
        return ExtractTrainLayoutFromEnvironment(mystery.Environment);
    }
    
    // Create a default layout as fallback
    return CreateDefaultTrainLayout();
}
```

## Migration Strategy

1. Set up the new scene with the new architecture
2. Run the test harness to validate core functionality
3. Update prefabs to include proper identifiers and spawn points
4. Gradually disable legacy systems (like CarCharacters.cs)
5. Verify integration with existing core systems (LLM, CharacterManager)

## Remaining Tasks

1. **Prefab Updates**:
   - Update train car prefabs to include proper identifiers and spawn points
   - Create standard location points for each car type

2. **Scene Integration**:
   - Set up the MainScene with all required components
   - Configure the loading UI overlay

3. **Legacy System Disabling**:
   - Disable the CarCharacters.cs random spawning
   - Ensure old systems don't interfere with the new architecture

4. **Testing**:
   - Run the test harness with a complete mystery JSON
   - Validate all core functionality works as expected

## Conclusion

This implementation strategy addresses the core issues in the Mystery Engine by creating a clear, deterministic path from mystery JSON to physical objects in the game world. By unifying the system around a central reality model, we ensure that all aspects of the mystery (train layout, characters, evidence) are properly represented in the game world.
