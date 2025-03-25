using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// Test harness for validating the Mystery Engine functionality.
/// </summary>
public class MysteryEngineTestHarness : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WorldCoordinator worldCoordinator;
    [SerializeField] private TextAsset testMysteryJson;
    [SerializeField] private string streamingAssetsMysteryPath = "MysteryStorage/test_mystery.json";
    
    [Header("Test Settings")]
    [SerializeField] private bool autoRunTests = true;
    [SerializeField] private bool verboseLogging = true;
    [SerializeField] private bool pauseOnFailure = true;
    
    [Header("Test UI")]
    [SerializeField] private TMPro.TextMeshProUGUI testResultsText;
    
    // Test state
    private bool testsPassed = false;
    private bool testsComplete = false;
    private List<string> testResults = new List<string>();
    private Mystery testMystery;
    
    private void Start()
    {
        if (autoRunTests)
        {
            RunTests();
        }
    }
    
    /// <summary>
    /// Run all tests.
    /// </summary>
    public void RunTests()
    {
        StartCoroutine(RunTestSequence());
    }
    
    private IEnumerator RunTestSequence()
    {
        testsComplete = false;
        testsPassed = true;
        testResults.Clear();
        
        LogTestStart("Running Mystery Engine Test Harness");
        
        // Test 1: Load mystery JSON
        LogTestStart("Test 1: Load Mystery JSON");
        bool loadSuccess = LoadTestMystery();
        yield return null;
        
        if (!loadSuccess)
        {
            LogTestFailure("Test 1: Failed to load mystery JSON");
            testsPassed = false;
            if (pauseOnFailure) yield break;
        }
        else
        {
            LogTestSuccess("Test 1: Successfully loaded mystery JSON");
        }
        
        // Test 2: Parse train layout
        LogTestStart("Test 2: Parse Train Layout");
        TrainLayout trainLayout = testMystery.GetTrainLayout();
        
        if (trainLayout == null || trainLayout.Cars == null || trainLayout.Cars.Count == 0)
        {
            LogTestFailure("Test 2: Failed to parse train layout");
            testsPassed = false;
            if (pauseOnFailure) yield break;
        }
        else
        {
            LogTestSuccess($"Test 2: Successfully parsed train layout with {trainLayout.Cars.Count} cars");
        }
        
        // Test 3: Get character locations
        LogTestStart("Test 3: Get Character Locations");
        Dictionary<string, string> characterLocations = testMystery.GetCharacterInitialLocations();
        
        if (characterLocations == null || characterLocations.Count == 0)
        {
            LogTestFailure("Test 3: Failed to get character locations");
            testsPassed = false;
            if (pauseOnFailure) yield break;
        }
        else
        {
            LogTestSuccess($"Test 3: Successfully got {characterLocations.Count} character locations");
            
            // Log all character locations
            if (verboseLogging)
            {
                foreach (var kvp in characterLocations)
                {
                    LogInfo($"  Character {kvp.Key} at location {kvp.Value}");
                }
            }
        }
        
        // Test 4: Initialize world
        LogTestStart("Test 4: Initialize World");
        
        if (worldCoordinator == null)
        {
            LogTestFailure("Test 4: WorldCoordinator reference is null");
            testsPassed = false;
            if (pauseOnFailure) yield break;
        }
        else
        {
            worldCoordinator.InitializeWorld(testMystery);
            
            // Wait for initialization to complete
            float timeout = Time.time + 10f; // 10 second timeout
            while (!worldCoordinator.IsInitialized && Time.time < timeout)
            {
                yield return null;
            }
            
            if (!worldCoordinator.IsInitialized)
            {
                LogTestFailure("Test 4: WorldCoordinator initialization timed out");
                testsPassed = false;
                if (pauseOnFailure) yield break;
            }
            else
            {
                LogTestSuccess("Test 4: Successfully initialized world");
            }
        }
        
        // Test 5: Verify train cars were created
        LogTestStart("Test 5: Verify Train Cars");
        
        TrainGenerator trainGenerator = FindFirstObjectByType<TrainGenerator>();
        if (trainGenerator == null)
        {
            LogTestFailure("Test 5: TrainGenerator not found");
            testsPassed = false;
            if (pauseOnFailure) yield break;
        }
        else
        {
            List<GameObject> trainCars = trainGenerator.GetTrainCars();
            
            if (trainCars == null || trainCars.Count == 0)
            {
                LogTestFailure("Test 5: No train cars were created");
                testsPassed = false;
                if (pauseOnFailure) yield break;
            }
            else if (trainCars.Count != trainLayout.Cars.Count)
            {
                LogTestFailure($"Test 5: Expected {trainLayout.Cars.Count} train cars, but found {trainCars.Count}");
                testsPassed = false;
                if (pauseOnFailure) yield break;
            }
            else
            {
                LogTestSuccess($"Test 5: Successfully created {trainCars.Count} train cars");
                
                // Log all train cars
                if (verboseLogging)
                {
                    foreach (var car in trainCars)
                    {
                        CarIdentifier carId = car.GetComponent<CarIdentifier>();
                        if (carId != null)
                        {
                            LogInfo($"  Car {carId.CarId} of type {carId.CarType}");
                        }
                        else
                        {
                            LogInfo($"  Car {car.name} (no identifier component)");
                        }
                    }
                }
            }
        }
        
        // Test 6: Verify characters were placed
        LogTestStart("Test 6: Verify Character Placement");
        
        LocationRegistry locationRegistry = FindFirstObjectByType<LocationRegistry>();
        if (locationRegistry == null)
        {
            LogTestFailure("Test 6: LocationRegistry not found");
            testsPassed = false;
            if (pauseOnFailure) yield break;
        }
        else
        {
            Dictionary<string, Transform> allLocations = locationRegistry.GetAllLocations();
            
            if (allLocations == null || allLocations.Count == 0)
            {
                LogTestFailure("Test 6: No locations were registered");
                testsPassed = false;
                if (pauseOnFailure) yield break;
            }
            else
            {
                LogTestSuccess($"Test 6: Found {allLocations.Count} registered locations");
                
                // Log some locations
                if (verboseLogging)
                {
                    int count = 0;
                    foreach (var kvp in allLocations)
                    {
                        LogInfo($"  Location {kvp.Key} at {kvp.Value.position}");
                        count++;
                        if (count >= 10) // Log at most 10 locations to avoid spam
                        {
                            LogInfo($"  ... and {allLocations.Count - 10} more");
                            break;
                        }
                    }
                }
                
                // Check if characters were placed at their locations
                foreach (var kvp in characterLocations)
                {
                    string characterId = kvp.Key;
                    string locationId = kvp.Value;
                    
                    GameObject character = worldCoordinator.GetCharacterById(characterId);
                    
                    if (character == null)
                    {
                        LogTestFailure($"Test 6: Character {characterId} was not placed");
                        testsPassed = false;
                        if (pauseOnFailure) yield break;
                    }
                    else
                    {
                        LogInfo($"  Character {characterId} was placed at {character.transform.position}");
                    }
                }
            }
        }
        
        // Complete
        testsComplete = true;
        LogTestComplete(testsPassed ? "All tests PASSED!" : "Some tests FAILED!");
        
        // Update UI
        UpdateResultsText();
    }
    
    /// <summary>
    /// Loads the test mystery from JSON.
    /// </summary>
    private bool LoadTestMystery()
    {
        try
        {
            string json;
            
            // Try to load from TextAsset first
            if (testMysteryJson != null)
            {
                json = testMysteryJson.text;
            }
            // Then try streaming assets
            else if (!string.IsNullOrEmpty(streamingAssetsMysteryPath))
            {
                string fullPath = Path.Combine(Application.streamingAssetsPath, streamingAssetsMysteryPath);
                
                if (File.Exists(fullPath))
                {
                    json = File.ReadAllText(fullPath);
                }
                else
                {
                    Debug.LogError($"Test mystery file not found at {fullPath}");
                    return false;
                }
            }
            else
            {
                Debug.LogError("No test mystery source specified");
                return false;
            }
            
            // Parse the JSON
            testMystery = JsonConvert.DeserializeObject<Mystery>(json);
            
            if (testMystery == null)
            {
                Debug.LogError("Failed to deserialize mystery JSON");
                return false;
            }
            
            // Verify essential components
            if (testMystery.Characters == null || testMystery.Characters.Count == 0)
            {
                Debug.LogWarning("Test mystery has no characters");
            }
            
            if (testMystery.Constellation == null || testMystery.Constellation.Nodes == null)
            {
                Debug.LogWarning("Test mystery has no constellation or nodes");
            }
            
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error loading test mystery: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Updates the UI with test results.
    /// </summary>
    private void UpdateResultsText()
    {
        if (testResultsText != null)
        {
            string resultText = string.Join("\n", testResults);
            testResultsText.text = resultText;
        }
    }
    
    private void LogTestStart(string message)
    {
        string formattedMessage = $"[TEST] {message}";
        testResults.Add(formattedMessage);
        Debug.Log(formattedMessage);
        UpdateResultsText();
    }
    
    private void LogTestSuccess(string message)
    {
        string formattedMessage = $"[PASS] {message}";
        testResults.Add(formattedMessage);
        Debug.Log(formattedMessage);
        UpdateResultsText();
    }
    
    private void LogTestFailure(string message)
    {
        string formattedMessage = $"[FAIL] {message}";
        testResults.Add(formattedMessage);
        Debug.LogError(formattedMessage);
        UpdateResultsText();
    }
    
    private void LogTestComplete(string message)
    {
        string formattedMessage = $"[COMPLETE] {message}";
        testResults.Add(formattedMessage);
        Debug.Log(formattedMessage);
        UpdateResultsText();
    }
    
    private void LogInfo(string message)
    {
        if (verboseLogging)
        {
            string formattedMessage = $"[INFO] {message}";
            testResults.Add(formattedMessage);
            Debug.Log(formattedMessage);
            UpdateResultsText();
        }
    }
}