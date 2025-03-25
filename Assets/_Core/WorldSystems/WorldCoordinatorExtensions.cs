using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Extension methods for the WorldCoordinator class to support testing and debugging.
/// </summary>
public static class WorldCoordinatorExtensions
{
    /// <summary>
    /// Gets the number of train cars in the world.
    /// </summary>
    public static int GetTrainCarCount(this WorldCoordinator coordinator)
    {
        var trainGenerator = coordinator.GetComponentInChildren<TrainGenerator>();
        if (trainGenerator == null) return 0;
        
        List<GameObject> cars = trainGenerator.GetTrainCars();
        return cars != null ? cars.Count : 0;
    }
}
