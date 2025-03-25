# Mystery Engine Integration Guide

## Overview

This guide provides step-by-step instructions for integrating the new World Coordinator architecture with existing game components. The integration process is designed to be minimally invasive while providing a clear path from the old random-spawning system to the new deterministic mystery-based system.

## Integration Steps

### 1. Update Train Car Prefabs

Each train car prefab needs to be updated with the following components:

1. **Add CarIdentifier Component**:
   ```csharp
   // Add to each train car
   CarIdentifier carIdentifier = carInstance.AddComponent<CarIdentifier>();
   carIdentifier.CarId = "[UNIQUE_CAR_ID]"; // e.g., "dining_car_01"
   carIdentifier.CarType = "[CAR_TYPE]";    // e.g., "dining", "passenger", "engine"
   carIdentifier.CarClass = "[CAR_CLASS]";  // e.g., "first", "standard"
   ```

2. **Create Location Points**:
   Create a "Locations" child GameObject with location points as children, each having:
   - LocationIdentifier component
   - SpawnPoint component

3. **Disable CarCharacters Script**:
   ```csharp
   // Find and disable the component
   CarCharacters carCharacters = carInstance.GetComponent<CarCharacters>();
   if (carCharacters != null)
   {
       carCharacters.enabled = false;
   }
   ```

### 2. Scene Setup

1. **Add Core System Objects**:
   - Create `WorldCoordinator` GameObject
   - Create `LocationRegistry` GameObject (child of WorldCoordinator)
   - Create `TrainGenerator` GameObject (child of WorldCoordinator)
   - Create `EntityPlacer` GameObject (child of WorldCoordinator)

2. **Configure Train Generator**:
   - Assign car prefabs to the TrainGenerator component
   - Set up the train parent transform
   - Configure car spacing

3. **Configure Entity Placer**:
   - Reference the NPCManager
   - Reference the EvidenceManager

### 3. Integration with GameInitializer

Modify `GameInitializer.cs` to use the WorldCoordinator for initialization:

```csharp
// In GameInitializer.cs
private WorldCoordinator worldCoordinator;

private async void InitializeGame()
{
    // Existing initialization code...
    
    // After mystery parsing is complete
    if (parsingControl != null && parsingControl.ParsedMystery != null)
    {
        Mystery mystery = parsingControl.ParsedMystery;
        
        // Initialize character manager
        await characterManager.InitializeCharactersAsync(mystery.Characters);
        
        // Initialize the world with the mystery data
        worldCoordinator.InitializeWorld(mystery);
    }
    
    // Continue with scene loading
    SceneManager.LoadScene("SystemsTest");
}
```

### 4. Scene Transition

Replace the separate loading screen with an overlay UI:

1. **Create LoadingUI Prefab**:
   - Canvas with CanvasGroup component
   - Progress bar
   - Status text
   - Background panel

2. **Add to Main Scene**:
   - Add LoadingUI to the main scene
   - Configure the LoadingUIController component

3. **Modify Scene Loading Logic**:
   ```csharp
   // Instead of loading a new scene
   loadingUI.Show();
   
   // After initialization is complete
   loadingUI.CompleteLoading();
   ```

### 5. Legacy System Compatibility

To ensure compatibility with existing systems:

1. **Update NPCManager**:
   ```csharp
   // Register spawned NPCs with the original NPCManager
   worldCoordinator.OnCharacterSpawned += (characterId, npcInstance) => {
       npcManager.RegisterNPC(characterId, npcInstance);
   };
   ```

2. **Update Character Interaction**:
   ```csharp
   // Register a lookup method with the dialogue system
   DialogueControl.GetCharacterByName = (characterName) => {
       return worldCoordinator.GetCharacterById(characterName);
   };
   ```

## Testing Integration

To validate the integration:

1. **Use MysteryEngineTestHarness**:
   - Configure the test harness with the WorldCoordinator reference
   - Run with the test_mystery.json file

2. **Validation Criteria**:
   - Train cars are generated according to the mystery JSON
   - Characters are placed at their initial locations
   - Evidence items are placed correctly
   - Legacy systems still function correctly

## Troubleshooting

### Common Issues

1. **CarCharacters Still Spawning**:
   - Check that the CarCharacters component is properly disabled
   - Verify CarCharacters.Start() isn't being called elsewhere

2. **Location Not Found**:
   - Verify location naming convention (e.g., "car_id.location_id")
   - Check that all locations are registered in the LocationRegistry

3. **Characters Not Appearing**:
   - Verify NPCManager initialization is complete
   - Check that character data is properly extracted from Mystery

### Logging

Add verbose logging to diagnose issues:

```csharp
// In WorldCoordinator.cs
[SerializeField] private bool debugMode = true;

private void LogDebug(string message)
{
    if (debugMode)
    {
        Debug.Log($"[WorldCoordinator] {message}");
    }
}
```

## Final Validation Checklist

- [ ] Train cars spawn with correct IDs and types
- [ ] Locations are properly registered
- [ ] Characters appear in their initial locations
- [ ] Evidence items are placed correctly
- [ ] UI transitions work smoothly
- [ ] Legacy systems still function correctly
