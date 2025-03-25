using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Test runner for validating the Mystery Engine refactoring.
/// </summary>
public class MysteryEngineTestRunner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WorldCoordinator worldCoordinator;
    [SerializeField] private TextAsset testMysteryJson;
    [SerializeField] private GameObject testUIPrefab;
    
    [Header("Test Settings")]
    [SerializeField] private bool runTestsOnStart = true;
    [SerializeField] private bool verboseLogging = true;
    
    // Test UI references
    private Canvas testCanvas;
    private TextMeshProUGUI testOutputText;
    private ScrollRect scrollRect;
    private Button runTestsButton;
    
    // Internal state
    private Mystery testMystery;
    private int passedTests = 0;
    private int totalTests = 0;
    private List<string> failedTests = new List<string>();
    private List<string> logMessages = new List<string>();
    
    private void Start()
    {
        SetupTestUI();
        
        if (runTestsOnStart)
        {
            RunAllTests();
        }
    }
    
    /// <summary>
    /// Sets up the test UI for displaying results.
    /// </summary>
    private void SetupTestUI()
    {
        // Create UI if it doesn't exist
        if (testUIPrefab == null)
        {
            // Create canvas
            GameObject canvasObj = new GameObject("Test Canvas");
            testCanvas = canvasObj.AddComponent<Canvas>();
            testCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Create test output panel
            GameObject panelObj = new GameObject("Test Output Panel");
            panelObj.transform.SetParent(testCanvas.transform, false);
            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.7f, 0);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.offsetMin = new Vector2(0, 0);
            panelRect.offsetMax = new Vector2(0, 0);
            
            // Create scroll view
            GameObject scrollViewObj = new GameObject("Scroll View");
            scrollViewObj.transform.SetParent(panelObj.transform, false);
            scrollRect = scrollViewObj.AddComponent<ScrollRect>();
            RectTransform scrollRectTransform = scrollViewObj.GetComponent<RectTransform>();
            scrollRectTransform.anchorMin = new Vector2(0, 0);
            scrollRectTransform.anchorMax = new Vector2(1, 0.95f);
            scrollRectTransform.offsetMin = new Vector2(10, 10);
            scrollRectTransform.offsetMax = new Vector2(-10, 0);
            
            // Create content container
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(scrollViewObj.transform, false);
            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.offsetMin = new Vector2(0, 0);
            contentRect.offsetMax = new Vector2(0, 0);
            scrollRect.content = contentRect;
            
            // Create text
            GameObject textObj = new GameObject("Test Output Text");
            textObj.transform.SetParent(contentObj.transform, false);
            testOutputText = textObj.AddComponent<TextMeshProUGUI>();
            testOutputText.fontSize = 14;
            testOutputText.color = Color.white;
            testOutputText.text = "Mystery Engine Test Runner\n---------------------------\n";
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.offsetMin = new Vector2(5, 5);
            textRect.offsetMax = new Vector2(-5, -5);
            
            // Create run button
            GameObject buttonObj = new GameObject("Run Tests Button");
            buttonObj.transform.SetParent(panelObj.transform, false);
            Button button = buttonObj.AddComponent<Button>();
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.6f, 0.2f);
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0, 0.95f);
            buttonRect.anchorMax = new Vector2(1, 1);
            buttonRect.offsetMin = new Vector2(10, 0);
            buttonRect.offsetMax = new Vector2(-10, -5);
            
            // Button text
            GameObject buttonTextObj = new GameObject("Button Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "Run Tests";
            buttonText.fontSize = 16;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;
            
            // Hook up button
            runTestsButton = button;
            runTestsButton.onClick.AddListener(RunAllTests);
        }
        else
        {
            // Instantiate from prefab
            GameObject testUI = Instantiate(testUIPrefab);
            testCanvas = testUI.GetComponent<Canvas>();
            testOutputText = testUI.GetComponentInChildren<TextMeshProUGUI>();
            scrollRect = testUI.GetComponentInChildren<ScrollRect>();
            runTestsButton = testUI.GetComponentInChildren<Button>();
            
            if (runTestsButton != null)
            {
                runTestsButton.onClick.AddListener(RunAllTests);
            }
        }
    }
    
    /// <summary>
    /// Runs all tests and displays the results.
    /// </summary>
    public void RunAllTests()
    {
        ClearTestResults();
        LogMessage("Starting Mystery Engine tests...", LogLevel.Important);
        
        if (!LoadTestMystery())
        {
            LogMessage("Failed to load test mystery. Aborting tests.", LogLevel.Error);
            return;
        }
        
        // Initialize world with test mystery
        InitializeWorld();
        
        // Run tests
        StartCoroutine(RunTestCoroutine());
    }
    
    /// <summary>
    /// Clears previous test results.
    /// </summary>
    private void ClearTestResults()
    {
        totalTests = 0;
        passedTests = 0;
        failedTests.Clear();
        logMessages.Clear();
        
        if (testOutputText != null)
        {
            testOutputText.text = "Mystery Engine Test Runner\n---------------------------\n";
        }
    }
    
    /// <summary>
    /// Loads the test mystery JSON.
    /// </summary>
    private bool LoadTestMystery()
    {
        if (testMysteryJson == null)
        {
            // Try to find the test data
            string testDataPath = Path.Combine(Application.dataPath, "_Core/Testing/TestData/test_mystery.json");
            if (File.Exists(testDataPath))
            {
                string jsonText = File.ReadAllText(testDataPath);
                try
                {
                    testMystery = JsonConvert.DeserializeObject<Mystery>(jsonText);
                    LogMessage($"Loaded test mystery from file: {testMystery.Metadata.Title}", LogLevel.Info);
                    return true;
                }
                catch (System.Exception e)
                {
                    LogMessage($"Failed to parse test mystery from file: {e.Message}", LogLevel.Error);
                    return false;
                }
            }
            else
            {
                LogMessage("No test mystery JSON assigned and no test file found.", LogLevel.Error);
                return false;
            }
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
    
    /// <summary>
    /// Initializes the world with the test mystery.
    /// </summary>
    private void InitializeWorld()
    {
        if (worldCoordinator == null)
        {
            LogMessage("WorldCoordinator not assigned. Looking for one in the scene...", LogLevel.Warning);
            
            worldCoordinator = FindFirstObjectByType<WorldCoordinator>();
            if (worldCoordinator == null)
            {
                LogMessage("No WorldCoordinator found in the scene. Cannot run tests.", LogLevel.Error);
                return;
            }
        }
        
        LogMessage("Initializing world with test mystery...", LogLevel.Info);
        worldCoordinator.InitializeWorld(testMystery);
    }
    
    /// <summary>
    /// Coroutine for running tests with proper timing.
    /// </summary>
    private IEnumerator RunTestCoroutine()
    {
        // Wait for world initialization to complete
        while (!worldCoordinator.IsInitialized)
        {
            LogMessage("Waiting for world initialization to complete...", LogLevel.Info);
            yield return new WaitForSeconds(0.5f);
        }
        
        // Run the actual tests
        LogMessage("World initialization complete. Running tests...", LogLevel.Info);
        
        yield return new WaitForSeconds(0.5f);
        
        // Test train layout
        TestTrainLayout();
        
        yield return new WaitForSeconds(0.2f);
        
        // Test character placement
        TestCharacterPlacement();
        
        yield return new WaitForSeconds(0.2f);
        
        // Test evidence placement
        TestEvidencePlacement();
        
        yield return new WaitForSeconds(0.2f);
        
        // Display final results
        DisplayTestResults();
    }
    
    /// <summary>
    /// Tests if train cars are generated correctly.
    /// </summary>
    private void TestTrainLayout()
    {
        LogMessage("Testing train layout generation...", LogLevel.Info);
        
        // Test 1: Check if each car in the mystery is created
        foreach (var carDef in testMystery.TrainLayout.Cars)
        {
            string carId = carDef.CarId;
            GameObject carObj = FindCarById(carId);
            
            AssertNotNull(carObj, $"Car \"{carId}\" exists in scene");
            
            if (carObj != null)
            {
                // Test 2: Check if car has a CarIdentifier component
                CarIdentifier carIdentifier = carObj.GetComponent<CarIdentifier>();
                AssertNotNull(carIdentifier, $"Car \"{carId}\" has CarIdentifier component");
                
                if (carIdentifier != null)
                {
                    // Test 3: Check if car type matches
                    AssertEqual(carDef.CarType, carIdentifier.CarType, $"Car \"{carId}\" type is correct");
                    
                    // Test 4: Check if car has locations
                    if (carDef.AvailableLocations != null && carDef.AvailableLocations.Count > 0)
                    {
                        foreach (string locationId in carDef.AvailableLocations)
                        {
                            Transform locationTransform = FindLocationInCar(carObj, locationId);
                            AssertNotNull(locationTransform, $"Location \"{locationId}\" exists in car \"{carId}\"");
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Tests if characters are placed in their correct locations.
    /// </summary>
    private void TestCharacterPlacement()
    {
        LogMessage("Testing character placement...", LogLevel.Info);
        
        if (testMystery.Characters == null)
        {
            LogMessage("No characters defined in test mystery.", LogLevel.Warning);
            return;
        }
        
        foreach (var characterEntry in testMystery.Characters)
        {
            string characterId = characterEntry.Key;
            MysteryCharacter characterData = characterEntry.Value;
            
            if (string.IsNullOrEmpty(characterData.InitialLocation))
            {
                LogMessage($"Character \"{characterId}\" has no initial location specified.", LogLevel.Warning);
                continue;
            }
            
            // Test 1: Check if character exists
            GameObject characterObj = worldCoordinator.GetCharacterById(characterId);
            AssertNotNull(characterObj, $"Character \"{characterId}\" exists in scene");
            
            if (characterObj != null)
            {
                // Test 2: Check if character is in the correct location
                bool isInCorrectLocation = IsCharacterInLocation(characterObj, characterData.InitialLocation);
                AssertTrue(isInCorrectLocation, $"Character \"{characterId}\" is in location \"{characterData.InitialLocation}\"");
            }
        }
    }
    
    /// <summary>
    /// Tests if evidence is placed in the correct locations.
    /// </summary>
    private void TestEvidencePlacement()
    {
        LogMessage("Testing evidence placement...", LogLevel.Info);
        
        if (testMystery.Constellation == null || testMystery.Constellation.Nodes == null)
        {
            LogMessage("No evidence nodes defined in test mystery.", LogLevel.Warning);
            return;
        }
        
        foreach (var nodeEntry in testMystery.Constellation.Nodes)
        {
            string nodeId = nodeEntry.Key;
            MysteryNode nodeData = nodeEntry.Value;
            
            if (!nodeData.PhysicalEvidence || string.IsNullOrEmpty(nodeData.Location))
            {
                continue;
            }
            
            // Test 1: Check if evidence exists
            GameObject evidenceObj = worldCoordinator.GetEvidenceById(nodeId);
            AssertNotNull(evidenceObj, $"Evidence \"{nodeId}\" exists in scene");
            
            if (evidenceObj != null)
            {
                // Test 2: Check if evidence is in the correct location
                bool isInCorrectLocation = IsObjectInLocation(evidenceObj, nodeData.Location);
                AssertTrue(isInCorrectLocation, $"Evidence \"{nodeId}\" is in location \"{nodeData.Location}\"");
            }
        }
    }
    
    // Helper methods
    
    /// <summary>
    /// Finds a car GameObject by its ID.
    /// </summary>
    private GameObject FindCarById(string carId)
    {
        return worldCoordinator.GetTrainGenerator()?.GetCarById(carId);
    }
    
    /// <summary>
    /// Finds a location transform within a car.
    /// </summary>
    private Transform FindLocationInCar(GameObject carObj, string locationId)
    {
        // Look for a child with a LocationIdentifier component
        LocationIdentifier[] locationIdentifiers = carObj.GetComponentsInChildren<LocationIdentifier>();
        return locationIdentifiers
            .Where(li => li.LocationId == locationId)
            .Select(li => li.transform)
            .FirstOrDefault();
    }
    
    /// <summary>
    /// Checks if a character is in the specified location.
    /// </summary>
    private bool IsCharacterInLocation(GameObject characterObj, string locationId)
    {
        Transform locationTransform = worldCoordinator.GetLocationRegistry()?.GetLocation(locationId);
        return IsObjectInLocation(characterObj, locationTransform);
    }
    
    /// <summary>
    /// Checks if an object is in the specified location.
    /// </summary>
    private bool IsObjectInLocation(GameObject obj, string locationId)
    {
        Transform locationTransform = worldCoordinator.GetLocationRegistry()?.GetLocation(locationId);
        return IsObjectInLocation(obj, locationTransform);
    }
    
    /// <summary>
    /// Checks if an object is in the specified location.
    /// </summary>
    private bool IsObjectInLocation(GameObject obj, Transform locationTransform)
    {
        if (obj == null || locationTransform == null)
        {
            return false;
        }
        
        // Check if object is a child of the location
        return obj.transform.IsChildOf(locationTransform);
    }
    
    // Assertion methods
    
    /// <summary>
    /// Asserts that two values are equal.
    /// </summary>
    private void AssertEqual<T>(T expected, T actual, string testName)
    {
        totalTests++;
        bool passed = EqualityComparer<T>.Default.Equals(expected, actual);
        
        if (passed)
        {
            passedTests++;
            LogMessage($"✓ {testName}", LogLevel.Success);
        }
        else
        {
            failedTests.Add(testName);
            LogMessage($"✗ {testName}: Expected {expected}, got {actual}", LogLevel.Error);
        }
    }
    
    /// <summary>
    /// Asserts that a value is not null.
    /// </summary>
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
    
    /// <summary>
    /// Asserts that a condition is true.
    /// </summary>
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
    
    /// <summary>
    /// Displays the final test results.
    /// </summary>
    private void DisplayTestResults()
    {
        float successRate = totalTests > 0 ? (float)passedTests / totalTests * 100f : 0f;
        LogMessage($"\nTest Results: {passedTests}/{totalTests} tests passed ({successRate:F1}%)", LogLevel.Important);
        
        if (failedTests.Count > 0)
        {
            LogMessage("\nFailed Tests:", LogLevel.Error);
            foreach (string test in failedTests)
            {
                LogMessage($"  - {test}", LogLevel.Error);
            }
        }
        
        if (passedTests == totalTests && totalTests > 0)
        {
            LogMessage("\nAll tests passed! The refactoring is working correctly.", LogLevel.Success);
        }
        else if (totalTests > 0)
        {
            LogMessage("\nSome tests failed. See the details above for information.", LogLevel.Warning);
        }
        else
        {
            LogMessage("\nNo tests were run. Check for errors in the log.", LogLevel.Warning);
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
    
    /// <summary>
    /// Logs a message with the specified log level.
    /// </summary>
    private void LogMessage(string message, LogLevel level)
    {
        if (!verboseLogging && level == LogLevel.Info)
        {
            return;
        }
        
        // Save to log list
        logMessages.Add(message);
        
        // Update UI
        if (testOutputText != null)
        {
            string colorTag = "";
            
            switch (level)
            {
                case LogLevel.Success:
                    colorTag = "<color=green>";
                    break;
                case LogLevel.Warning:
                    colorTag = "<color=yellow>";
                    break;
                case LogLevel.Error:
                    colorTag = "<color=red>";
                    break;
                case LogLevel.Important:
                    colorTag = "<color=cyan>";
                    break;
            }
            
            string formattedMessage = level != LogLevel.Info ? $"{colorTag}{message}</color>\n" : $"{message}\n";
            testOutputText.text += formattedMessage;
            
            // Scroll to bottom
            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }
        
        // Log to console
        switch (level)
        {
            case LogLevel.Info:
            case LogLevel.Success:
                Debug.Log($"[TestRunner] {message}");
                break;
            case LogLevel.Warning:
                Debug.LogWarning($"[TestRunner] {message}");
                break;
            case LogLevel.Error:
            case LogLevel.Important:
                Debug.LogError($"[TestRunner] {message}");
                break;
        }
    }
}

/// <summary>
/// Extensions for the WorldCoordinator to facilitate testing.
/// </summary>
public static class WorldCoordinatorTestExtensions
{
    /// <summary>
    /// Gets the TrainGenerator component.
    /// </summary>
    public static TrainGenerator GetTrainGenerator(this WorldCoordinator coordinator)
    {
        return coordinator.GetComponentInChildren<TrainGenerator>();
    }
    
    /// <summary>
    /// Gets the LocationRegistry component.
    /// </summary>
    public static LocationRegistry GetLocationRegistry(this WorldCoordinator coordinator)
    {
        return coordinator.GetComponentInChildren<LocationRegistry>();
    }
}