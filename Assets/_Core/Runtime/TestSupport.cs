using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Helper class for testing the Mystery Engine.
/// </summary>
public class TestSupport : MonoBehaviour
{
    [Header("Test Data")]
    [SerializeField] private TextAsset testMysteryJson;
    
    [Header("References")]
    [SerializeField] private MysteryRealityModel realityModel;
    
    [Header("Settings")]
    [SerializeField] private bool runTestsOnStart = true;
    [SerializeField] private bool verboseLogging = true;
    
    // Track test results
    private int totalTests = 0;
    private int passedTests = 0;
    private List<string> failedTests = new List<string>();
    
    private void Start()
    {
        if (runTestsOnStart)
        {
            RunTests();
        }
    }
    
    /// <summary>
    /// Runs basic tests to validate the mystery engine.
    /// </summary>
    public void RunTests()
    {
        Debug.Log("=== Running Mystery Engine Tests ===");
        
        totalTests = 0;
        passedTests = 0;
        failedTests.Clear();
        
        // Test 1: Check if reality model exists
        Assert(realityModel != null, "Reality model exists");
        
        if (realityModel == null)
        {
            LogTestResults();
            return;
        }
        
        // Test 2: Load test mystery
        Mystery testMystery = LoadTestMystery();
        Assert(testMystery != null, "Test mystery loaded");
        
        if (testMystery == null)
        {
            LogTestResults();
            return;
        }
        
        // Test 3: Initialize world
        realityModel.InitializeWorld(testMystery);
        Assert(realityModel.IsInitialized, "World initialization completed");
        
        // Test 4: Check train car count (mock value)
        int carCount = realityModel.GetTrainCarCount();
        Assert(carCount > 0, $"Train has {carCount} cars");
        
        // Display results
        LogTestResults();
    }
    
    /// <summary>
    /// Loads the test mystery.
    /// </summary>
    private Mystery LoadTestMystery()
    {
        if (testMysteryJson == null)
        {
            Debug.LogError("No test mystery JSON assigned");
            return null;
        }
        
        try
        {
            // Create a basic Mystery object for testing
            return new Mystery
            {
                Metadata = new MysteryMetadata
                {
                    Title = "Test Mystery"
                }
            };
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading test mystery: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Asserts that a condition is true.
    /// </summary>
    private void Assert(bool condition, string testName)
    {
        totalTests++;
        
        if (condition)
        {
            passedTests++;
            Debug.Log($"✓ {testName}");
        }
        else
        {
            failedTests.Add(testName);
            Debug.LogError($"✗ {testName}");
        }
    }
    
    /// <summary>
    /// Logs the test results.
    /// </summary>
    private void LogTestResults()
    {
        Debug.Log($"\nTest Results: {passedTests}/{totalTests} tests passed");
        
        if (failedTests.Count > 0)
        {
            Debug.LogError("\nFailed Tests:");
            foreach (string test in failedTests)
            {
                Debug.LogError($"  - {test}");
            }
        }
        else if (passedTests > 0)
        {
            Debug.Log("\nAll tests passed!");
        }
    }
}