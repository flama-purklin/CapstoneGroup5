using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

/// <summary>
/// Test harness for validating the Mystery Engine functionality.
/// Provides a UI for running tests and displays results.
/// </summary>
public class TestHarness : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private string testMysteryPath = "MysteryStorage/test_mystery.json";
    [SerializeField] private bool runTestsOnStart = false;
    
    [Header("UI References")]
    [SerializeField] private Text testResultsText;
    [SerializeField] private Button runTestsButton;
    [SerializeField] private Toggle verboseToggle;
    
    [Header("System References")]
    [SerializeField] private WorldCoordinator worldCoordinator;
    [SerializeField] private LocationRegistry locationRegistry;
    [SerializeField] private TrainGenerator trainGenerator;
    [SerializeField] private EntityPlacer entityPlacer;
    [SerializeField] private LoadingUIController loadingUI;
    
    private List<TestResult> testResults = new List<TestResult>();
    private bool isRunningTests = false;
    private bool verboseLogging = true;
    
    // Test counters
    private int totalTests = 0;
    private int passedTests = 0;
    private int failedTests = 0;
    
    private void Start()
    {
        // Initialize UI
        if (runTestsButton != null)
        {
            runTestsButton.onClick.AddListener(RunAllTests);
        }
        
        if (verboseToggle != null)
        {
            verboseToggle.onValueChanged.AddListener(SetVerboseLogging);
            verboseLogging = verboseToggle.isOn;
        }
        
        // Auto-run tests if configured
        if (runTestsOnStart)
        {
            RunAllTests();
        }
    }
    
    /// <summary>
    /// Runs all tests in the harness.
    /// </summary>
    public void RunAllTests()
    {
        if (isRunningTests) return;
        
        isRunningTests = true;
        ResetTestResults();
        
        // Show loading UI if available
        if (loadingUI != null)
        {
            loadingUI.Show("Running Tests...");
        }
        
        StartCoroutine(RunTestsCoroutine());
    }
    
    private IEnumerator RunTestsCoroutine()
    {
        LogTest("Starting test suite...");
        
        // Basic initialization tests
        yield return StartCoroutine(TestMysteryParsing());
        yield return StartCoroutine(TestWorldCoordinator());
        yield return StartCoroutine(TestTrainGeneration());
        yield return StartCoroutine(TestCharacterPlacement());
        yield return StartCoroutine(TestEvidencePlacement());
        
        // Display final results
        DisplayTestResults();
        
        // Hide loading UI if available
        if (loadingUI != null)
        {
            loadingUI.Hide();
        }
        
        isRunningTests = false;
    }
    
    private IEnumerator TestMysteryParsing()
    {
        LogTest("TESTING MYSTERY PARSING");
        
        // Get a reference to the parsing control
        ParsingControl parsingControl = FindFirstObjectByType<ParsingControl>();
        if (parsingControl == null)
        {
            LogTestResult("Mystery Parsing", "Failed to find ParsingControl component", false);
            yield break;
        }
        
        // Set test mystery path
        if (!string.IsNullOrEmpty(testMysteryPath))
        {
            parsingControl.mysteryFiles = System.IO.Path.GetDirectoryName(testMysteryPath);
        }
        
        // Wait for parsing to complete
        Mystery mystery = null;
        try
        {
            LogTest("Parsing mystery...");
            
            // Start parsing
            var parseTask = parsingControl.ParseMysteryAsync();
            
            // Wait for parsing to complete
            while (!parseTask.IsCompleted)
            {
                yield return null;
            }
            
            // Get the result
            mystery = parseTask.Result;
        }
        catch (System.Exception ex)
        {
            LogTestResult("Mystery Parsing", $"Exception: {ex.Message}", false);
            yield break;
        }
        
        // Verify the mystery was parsed
        if (mystery == null)
        {
            LogTestResult("Mystery Parsing", "Mystery is null", false);
            yield break;
        }
        
        // Verify basic mystery properties
        bool metadataValid = mystery.Metadata != null && !string.IsNullOrEmpty(mystery.Metadata.Title);
        bool charactersValid = mystery.Characters != null && mystery.Characters.Count > 0;
        bool constellationValid = mystery.Constellation != null && 
                                mystery.Constellation.Nodes != null && 
                                mystery.Constellation.Nodes.Count > 0;
        
        // Verify train layout if available
        bool trainLayoutValid = mystery.TrainLayout != null && 
                                mystery.TrainLayout.Cars != null && 
                                mystery.TrainLayout.Cars.Count > 0;
        
        // Log detailed results if verbose
        if (verboseLogging)
        {
            LogTest($"Mystery Title: {mystery.Metadata?.Title ?? "null"}");
            LogTest($"Character Count: {mystery.Characters?.Count ?? 0}");
            LogTest($"Node Count: {mystery.Constellation?.Nodes?.Count ?? 0}");
            LogTest($"Train Car Count: {mystery.TrainLayout?.Cars?.Count ?? 0}");
        }
        
        // Overall test result
        bool overallResult = metadataValid && charactersValid && constellationValid;
        string resultMessage = overallResult ? 
            "Mystery parsed successfully" : 
            "Mystery parsing issues detected";
            
        if (!trainLayoutValid)
        {
            resultMessage += " (Train layout missing or invalid)";
        }
        
        LogTestResult("Mystery Parsing", resultMessage, overallResult);
    }
    
    private IEnumerator TestWorldCoordinator()
    {
        LogTest("TESTING WORLD COORDINATOR");
        
        // Verify WorldCoordinator exists
        if (worldCoordinator == null)
        {
            worldCoordinator = FindFirstObjectByType<WorldCoordinator>();
        }
        
        if (worldCoordinator == null)
        {
            LogTestResult("World Coordinator", "Failed to find WorldCoordinator component", false);
            yield break;
        }
        
        // Get mystery data from game controller
        GameControl gameControl = FindFirstObjectByType<GameControl>();
        if (gameControl == null || gameControl.coreMystery == null)
        {
            LogTestResult("World Coordinator", "No mystery data available from GameControl", false);
            yield break;
        }
        
        // Test initialization
        LogTest("Initializing world...");
        worldCoordinator.InitializeWorld(gameControl.coreMystery);
        
        // Wait for initialization to complete
        float startTime = Time.time;
        float timeout = 10f; // 10 seconds timeout
        
        while (!worldCoordinator.IsInitialized && Time.time - startTime < timeout)
        {
            yield return null;
        }
        
        // Check if initialization completed successfully
        if (!worldCoordinator.IsInitialized)
        {
            LogTestResult("World Coordinator", "Initialization timed out", false);
            yield break;
        }
        
        LogTestResult("World Coordinator", "World initialized successfully", true);
    }
    
    private IEnumerator TestTrainGeneration()
    {
        LogTest("TESTING TRAIN GENERATION");
        
        // Verify TrainGenerator exists
        if (trainGenerator == null)
        {
            trainGenerator = FindFirstObjectByType<TrainGenerator>();
        }
        
        if (trainGenerator == null)
        {
            LogTestResult("Train Generation", "Failed to find TrainGenerator component", false);
            yield break;
        }
        
        // Get mystery data from game controller
        GameControl gameControl = FindFirstObjectByType<GameControl>();
        if (gameControl == null || gameControl.coreMystery == null || gameControl.coreMystery.TrainLayout == null)
        {
            LogTestResult("Train Generation", "No train layout data available", false);
            yield break;
        }
        
        // Get train cars
        var trainCars = trainGenerator.GetTrainCars();
        bool hasTrainCars = trainCars != null && trainCars.Count > 0;
        
        // Verify each train car has required components
        bool allCarsValid = true;
        int carCount = trainCars?.Count ?? 0;
        
        if (hasTrainCars)
        {
            foreach (var car in trainCars)
            {
                if (car == null)
                {
                    allCarsValid = false;
                    LogTest("Found null train car");
                    continue;
                }
                
                // Check for required components
                bool hasCarIdentifier = car.GetComponent<CarIdentifier>() != null;
                bool hasVisibility = car.GetComponent<CarVisibility>() != null;
                
                if (!hasCarIdentifier || !hasVisibility)
                {
                    allCarsValid = false;
                    LogTest($"Car {car.name} missing required components");
                }
            }
        }
        
        // Log results
        bool overallResult = hasTrainCars && allCarsValid;
        string resultMessage = overallResult ?
            $"Generated {carCount} valid train cars" :
            $"Train generation issues: {(hasTrainCars ? "Cars generated but some invalid" : "No cars generated")}";
            
        LogTestResult("Train Generation", resultMessage, overallResult);
        
        yield return null;
    }
    
    private IEnumerator TestCharacterPlacement()
    {
        LogTest("TESTING CHARACTER PLACEMENT");
        
        // Verify required components exist
        if (worldCoordinator == null)
        {
            worldCoordinator = FindFirstObjectByType<WorldCoordinator>();
        }
        
        if (worldCoordinator == null)
        {
            LogTestResult("Character Placement", "Failed to find WorldCoordinator component", false);
            yield break;
        }
        
        // Get mystery data
        GameControl gameControl = FindFirstObjectByType<GameControl>();
        if (gameControl == null || gameControl.coreMystery == null || gameControl.coreMystery.Characters == null)
        {
            LogTestResult("Character Placement", "No character data available", false);
            yield break;
        }
        
        // Wait for any initialization to complete
        while (!worldCoordinator.IsInitialized)
        {
            yield return null;
        }
        
        // Check if characters were placed
        int expectedCount = gameControl.coreMystery.Characters.Count;
        int actualCount = 0;
        
        // Count characters in scene
        foreach (var characterPair in gameControl.coreMystery.Characters)
        {
            GameObject characterObj = worldCoordinator.GetCharacterById(characterPair.Key);
            if (characterObj != null)
            {
                actualCount++;
                
                if (verboseLogging)
                {
                    string locationId = characterPair.Value.InitialLocation;
                    LogTest($"Character {characterPair.Key} placed at {locationId}");
                }
            }
            else if (verboseLogging)
            {
                LogTest($"Character {characterPair.Key} not found in scene");
            }
        }
        
        // Check if all characters were placed
        bool allCharactersPlaced = actualCount == expectedCount;
        
        // Log results
        string resultMessage = allCharactersPlaced ?
            $"All {expectedCount} characters placed successfully" :
            $"Character placement issues: {actualCount}/{expectedCount} placed";
            
        LogTestResult("Character Placement", resultMessage, allCharactersPlaced);
    }
    
    private IEnumerator TestEvidencePlacement()
    {
        LogTest("TESTING EVIDENCE PLACEMENT");
        
        // Verify required components exist
        if (worldCoordinator == null)
        {
            worldCoordinator = FindFirstObjectByType<WorldCoordinator>();
        }
        
        if (worldCoordinator == null)
        {
            LogTestResult("Evidence Placement", "Failed to find WorldCoordinator component", false);
            yield break;
        }
        
        // Get mystery data
        GameControl gameControl = FindFirstObjectByType<GameControl>();
        if (gameControl == null || gameControl.coreMystery == null || 
            gameControl.coreMystery.Constellation == null || 
            gameControl.coreMystery.Constellation.Nodes == null)
        {
            LogTestResult("Evidence Placement", "No evidence data available", false);
            yield break;
        }
        
        // Wait for any initialization to complete
        while (!worldCoordinator.IsInitialized)
        {
            yield return null;
        }
        
        // Count evidence nodes that should be physical evidence
        int expectedCount = 0;
        foreach (var nodePair in gameControl.coreMystery.Constellation.Nodes)
        {
            if (nodePair.Value.PhysicalEvidence && !string.IsNullOrEmpty(nodePair.Value.Location))
            {
                expectedCount++;
            }
        }
        
        if (expectedCount == 0)
        {
            LogTestResult("Evidence Placement", "No physical evidence to place", true);
            yield break;
        }
        
        // Check if evidence was placed
        int actualCount = 0;
        
        // Count evidence in scene
        foreach (var nodePair in gameControl.coreMystery.Constellation.Nodes)
        {
            if (!nodePair.Value.PhysicalEvidence || string.IsNullOrEmpty(nodePair.Value.Location))
                continue;
                
            GameObject evidenceObj = worldCoordinator.GetEvidenceById(nodePair.Key);
            if (evidenceObj != null)
            {
                actualCount++;
                
                if (verboseLogging)
                {
                    string locationId = nodePair.Value.Location;
                    LogTest($"Evidence {nodePair.Key} placed at {locationId}");
                }
            }
            else if (verboseLogging)
            {
                LogTest($"Evidence {nodePair.Key} not found in scene");
            }
        }
        
        // Check if all evidence was placed
        bool allEvidencePlaced = actualCount == expectedCount;
        
        // Log results
        string resultMessage = allEvidencePlaced ?
            $"All {expectedCount} evidence items placed successfully" :
            $"Evidence placement issues: {actualCount}/{expectedCount} placed";
            
        LogTestResult("Evidence Placement", resultMessage, allEvidencePlaced);
    }
    
    private void ResetTestResults()
    {
        testResults.Clear();
        totalTests = 0;
        passedTests = 0;
        failedTests = 0;
        
        if (testResultsText != null)
        {
            testResultsText.text = "Running tests...";
        }
    }
    
    private void LogTest(string message)
    {
        if (verboseLogging)
        {
            Debug.Log($"[TestHarness] {message}");
        }
    }
    
    private void LogTestResult(string testName, string message, bool passed)
    {
        totalTests++;
        
        if (passed)
        {
            passedTests++;
            Debug.Log($"[TEST PASSED] {testName}: {message}");
        }
        else
        {
            failedTests++;
            Debug.LogError($"[TEST FAILED] {testName}: {message}");
        }
        
        testResults.Add(new TestResult(testName, message, passed));
    }
    
    private void DisplayTestResults()
    {
        if (testResultsText == null) return;
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Test Results: {passedTests}/{totalTests} passed");
        sb.AppendLine();
        
        foreach (var result in testResults)
        {
            sb.AppendLine($"{(result.Passed ? "✅" : "❌")} {result.TestName}: {result.Message}");
        }
        
        testResultsText.text = sb.ToString();
    }
    
    private void SetVerboseLogging(bool verbose)
    {
        verboseLogging = verbose;
    }
    
    /// <summary>
    /// Simple class to store test results.
    /// </summary>
    private class TestResult
    {
        public string TestName { get; private set; }
        public string Message { get; private set; }
        public bool Passed { get; private set; }
        
        public TestResult(string testName, string message, bool passed)
        {
            TestName = testName;
            Message = message;
            Passed = passed;
        }
    }
}
