using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

/// <summary>
/// Test harness for validating the Mystery Engine functionality.
/// Runs a series of tests to ensure the system is working as expected.
/// </summary>
public class TestHarnessManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WorldCoordinator worldCoordinator;
    [SerializeField] private LocationRegistry locationRegistry;
    [SerializeField] private TextAsset testMysteryJson;
    
    [Header("Test Settings")]
    [SerializeField] private bool runTestsOnStart = false;
    [SerializeField] private bool verboseLogging = true;
    
    // Test statistics
    private int totalTests = 0;
    private int passedTests = 0;
    private List<string> failedTests = new List<string>();
    
    private Mystery testMystery;
    
    void Start()
    {
        if (runTestsOnStart)
        {
            RunAllTests();
        }
    }
    
    /// <summary>
    /// Runs all tests and logs the results.
    /// </summary>
    public void RunAllTests()
    {
        // Reset statistics
        totalTests = 0;
        passedTests = 0;
        failedTests.Clear();
        
        LogMessage("Starting test suite...", LogLevel.Important);
        
        // Load test mystery
        bool mysteryLoaded = LoadTestMystery();
        if (!mysteryLoaded)
        {
            LogMessage("Failed to load test mystery. Aborting tests.", LogLevel.Error);
            return;
        }
        
        // Initialize world with test mystery
        InitializeWorld();
        
        // Run tests
        StartCoroutine(RunTestSequence());
    }
    
    private IEnumerator RunTestSequence()
    {
        // Wait a few frames to ensure everything is initialized
        yield return new WaitForSeconds(0.5f);
        
        // Run train layout tests
        TestTrainLayout();
        
        // Run character placement tests
        TestCharacterPlacement();
        
        // Run evidence placement tests
        TestEvidencePlacement();
        
        // Run location registry tests
        TestLocationRegistry();
        
        // Log results
        LogTestResults();
    }
    
    private bool LoadTestMystery()
    {
        if (testMysteryJson == null)
        {
            LogMessage("No test mystery JSON assigned.", LogLevel.Error);
            return false;
        }
        
        try
        {
            testMystery = JsonConvert.DeserializeObject<Mystery>(testMysteryJson.text);
            LogMessage($"Loaded test mystery: {testMystery.Metadata.Title}", LogLevel.Info);
            return true;
        }
        catch (System.Exception e)
        {
            LogMessage($"Failed to parse test mystery: {e.Message}", LogLevel.Error);
            return false;
        }
    }
    
    private void InitializeWorld()
    {
        if (worldCoordinator == null)
        {
            LogMessage("WorldCoordinator not assigned.", LogLevel.Error);
            return;
        }
        
        LogMessage("Initializing world with test mystery...", LogLevel.Info);
        worldCoordinator.InitializeWorld(testMystery);
    }
    
    private void TestTrainLayout()
    {
        LogMessage("Testing train layout...", LogLevel.Info);
        
        // Test 1: Correct number of train cars
        int expectedCarCount = testMystery.TrainLayout.Cars.Count;
        int actualCarCount = worldCoordinator.GetTrainCarCount();
        
        AssertEqual(expectedCarCount, actualCarCount, "Train car count");
        
        // Test 2: Cars have correct IDs and types
        foreach (var carDef in testMystery.TrainLayout.Cars)
        {
            GameObject car = worldCoordinator.GetCarById(carDef.CarId);
            AssertNotNull(car, $"Car with ID {carDef.CarId} exists");
            
            if (car != null)
            {
                CarIdentifier carId = car.GetComponent<CarIdentifier>();
                AssertNotNull(carId, $"Car {carDef.CarId} has CarIdentifier component");
                
                if (carId != null)
                {
                    AssertEqual(carDef.CarType, carId.CarType, $"Car {carDef.CarId} type");
                }
            }
        }
    }
    
    private void TestCharacterPlacement()
    {
        LogMessage("Testing character placement...", LogLevel.Info);
        
        // Test: Characters are in their initial locations
        foreach (var characterEntry in testMystery.Characters)
        {
            string characterId = characterEntry.Key;
            MysteryCharacter characterData = characterEntry.Value;
            
            if (string.IsNullOrEmpty(characterData.InitialLocation))
            {
                LogMessage($"Character {characterId} has no initial location specified. Skipping test.", LogLevel.Warning);
                continue;
            }
            
            GameObject character = worldCoordinator.GetCharacterById(characterId);
            AssertNotNull(character, $"Character {characterId} exists");
            
            if (character != null)
            {
                // Check if character is in the correct location
                Transform locationTransform = locationRegistry.GetLocation(characterData.InitialLocation);
                AssertNotNull(locationTransform, $"Location {characterData.InitialLocation} exists");
                
                if (locationTransform != null)
                {
                    bool isInLocation = IsObjectInLocation(character, locationTransform);
                    AssertTrue(isInLocation, $"Character {characterId} is in location {characterData.InitialLocation}");
                }
            }
        }
    }
    
    private void TestEvidencePlacement()
    {
        LogMessage("Testing evidence placement...", LogLevel.Info);
        
        if (testMystery.Constellation == null || testMystery.Constellation.Nodes == null)
        {
            LogMessage("No constellation/node data in test mystery. Skipping evidence tests.", LogLevel.Warning);
            return;
        }
        
        // Test: Evidence items are in their locations
        foreach (var nodeEntry in testMystery.Constellation.Nodes)
        {
            string nodeId = nodeEntry.Key;
            MysteryNode nodeData = nodeEntry.Value;
            
            // Only test physical evidence
            if (nodeData.PhysicalEvidence && !string.IsNullOrEmpty(nodeData.Location))
            {
                GameObject evidence = worldCoordinator.GetEvidenceById(nodeId);
                AssertNotNull(evidence, $"Evidence {nodeId} exists");
                
                if (evidence != null)
                {
                    Transform locationTransform = locationRegistry.GetLocation(nodeData.Location);
                    AssertNotNull(locationTransform, $"Location {nodeData.Location} exists");
                    
                    if (locationTransform != null)
                    {
                        bool isInLocation = IsObjectInLocation(evidence, locationTransform);
                        AssertTrue(isInLocation, $"Evidence {nodeId} is in location {nodeData.Location}");
                    }
                }
            }
        }
    }
    
    private void TestLocationRegistry()
    {
        LogMessage("Testing location registry...", LogLevel.Info);
        
        if (locationRegistry == null)
        {
            LogMessage("LocationRegistry not assigned.", LogLevel.Error);
            return;
        }
        
        // Test 1: All train cars are registered
        foreach (var carDef in testMystery.TrainLayout.Cars)
        {
            Transform carTransform = locationRegistry.GetLocation(carDef.CarId);
            AssertNotNull(carTransform, $"Car location {carDef.CarId} is registered");
        }
        
        // Test 2: All locations within cars are registered
        foreach (var carDef in testMystery.TrainLayout.Cars)
        {
            if (carDef.AvailableLocations != null)
            {
                foreach (string locationId in carDef.AvailableLocations)
                {
                    string fullLocationId = $"{carDef.CarId}.{locationId}";
                    Transform locationTransform = locationRegistry.GetLocation(fullLocationId);
                    AssertNotNull(locationTransform, $"Sub-location {fullLocationId} is registered");
                }
            }
        }
    }
    
    // Helper methods for tests
    
    private bool IsObjectInLocation(GameObject obj, Transform location)
    {
        if (obj == null || location == null)
            return false;
            
        // Check if object is a child of location
        return obj.transform.IsChildOf(location);
    }
    
    // Assertion methods
    
    private void AssertEqual<T>(T expected, T actual, string testName)
    {
        totalTests++;
        bool passed = EqualityComparer<T>.Default.Equals(expected, actual);
        
        if (passed)
        {
            passedTests++;
            LogMessage($"✓ {testName}: {actual} equals expected {expected}", LogLevel.Success);
        }
        else
        {
            failedTests.Add(testName);
            LogMessage($"✗ {testName}: {actual} does not equal expected {expected}", LogLevel.Error);
        }
    }
    
    private void AssertNotNull(object obj, string testName)
    {
        totalTests++;
        bool passed = obj != null;
        
        if (passed)
        {
            passedTests++;
            LogMessage($"✓ {testName}", LogLevel.Success);
        }
        else
        {
            failedTests.Add(testName);
            LogMessage($"✗ {testName}: Object is null", LogLevel.Error);
        }
    }
    
    private void AssertTrue(bool condition, string testName)
    {
        totalTests++;
        
        if (condition)
        {
            passedTests++;
            LogMessage($"✓ {testName}", LogLevel.Success);
        }
        else
        {
            failedTests.Add(testName);
            LogMessage($"✗ {testName}: Condition is false", LogLevel.Error);
        }
    }
    
    private void LogTestResults()
    {
        LogMessage($"\nTest Results: {passedTests}/{totalTests} tests passed", LogLevel.Important);
        
        if (failedTests.Count > 0)
        {
            LogMessage("\nFailed Tests:", LogLevel.Error);
            foreach (string test in failedTests)
            {
                LogMessage($"  - {test}", LogLevel.Error);
            }
        }
    }
    
    // Logging
    
    private enum LogLevel
    {
        Info,
        Success,
        Warning,
        Error,
        Important
    }
    
    private void LogMessage(string message, LogLevel level)
    {
        if (!verboseLogging && level == LogLevel.Info)
            return;
            
        string prefix = "[TestHarness] ";
        
        switch (level)
        {
            case LogLevel.Info:
                Debug.Log($"{prefix}{message}");
                break;
            case LogLevel.Success:
                Debug.Log($"{prefix}<color=green>{message}</color>");
                break;
            case LogLevel.Warning:
                Debug.LogWarning($"{prefix}{message}");
                break;
            case LogLevel.Error:
                Debug.LogError($"{prefix}{message}");
                break;
            case LogLevel.Important:
                Debug.Log($"{prefix}<color=yellow><b>{message}</b></color>");
                break;
        }
    }
}
