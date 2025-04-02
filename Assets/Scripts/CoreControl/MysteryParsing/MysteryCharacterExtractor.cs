using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CoreControl.MysteryParsing
{
    /// <summary>
    /// This class handles direct character extraction from mystery data without relying on the LLM assets.
    /// This is a standalone implementation that can be used by ParsingControl.
    /// </summary>
    public class MysteryCharacterExtractor : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private string _charactersOutputFolder = "Characters";
        [SerializeField] private string _charactersBackupFolder = "CharacterBackups";
        [SerializeField] private bool _clearExistingCharacterFiles = true;
        [SerializeField] private bool _backupExistingFiles = true;
        
        [Header("Performance")]
        [SerializeField] private int _maxConcurrentExtractions = 3;
        [SerializeField] private float _extractionDelay = 0.1f;
    
        [Header("Debug")]
        [SerializeField] private bool _verboseLogging = false;
        
        // Events for progress tracking
        public event Action<float> OnExtractionProgress;
        public event Action<int> OnCharactersExtracted;
    
        private string _outputPath;
        private string _backupPath;
        private int _totalCharactersProcessed = 0;
        private int _totalCharactersToProcess = 0;
    
        private void Awake()
        {
            _outputPath = Path.Combine(Application.streamingAssetsPath, _charactersOutputFolder);
            _backupPath = Path.Combine(Application.streamingAssetsPath, _charactersBackupFolder);
            
            Debug.Log($"MysteryCharacterExtractor initializing with output path: {_outputPath}");
            
            // Ensure StreamingAssets exists
            string streamingAssetsPath = Application.streamingAssetsPath;
            if (!Directory.Exists(streamingAssetsPath))
            {
                Directory.CreateDirectory(streamingAssetsPath);
                Debug.Log($"Created StreamingAssets directory at: {streamingAssetsPath}");
            }
            
            // Create the output directory if it doesn't exist
            EnsureCharactersDirectoryExists();
            
            // Create the backup directory if it doesn't exist and backups are enabled
            if (_backupExistingFiles && !Directory.Exists(_backupPath))
            {
                Directory.CreateDirectory(_backupPath);
                Debug.Log($"Created characters backup directory at: {_backupPath}");
            }
            
            Debug.Log("MysteryCharacterExtractor initialization complete");
        }
        
        /// <summary>
        /// Ensures the Characters directory exists, creating it if needed
        /// </summary>
        private void EnsureCharactersDirectoryExists()
        {
            if (!Directory.Exists(_outputPath))
            {
                Directory.CreateDirectory(_outputPath);
                Debug.Log($"Created characters output directory at: {_outputPath}");
            }
            else
            {
                Debug.Log($"Using existing characters directory at: {_outputPath}");
                
                // If we're in verbose mode, list the existing files
                if (_verboseLogging)
                {
                    var existingFiles = Directory.GetFiles(_outputPath, "*.json");
                    if (existingFiles.Length > 0)
                    {
                        Debug.Log($"Found {existingFiles.Length} existing character files:");
                        foreach (var file in existingFiles)
                        {
                            Debug.Log($"  - {Path.GetFileName(file)}");
                        }
                    }
                }
            }
        }
    
        /// <summary>
        /// Extracts characters from the mystery JSON and creates individual character files
        /// </summary>
        /// <param name="mystery">The mystery object containing character data</param>
        public void ExtractCharactersFromMystery(Mystery mystery)
        {
            if (mystery == null || mystery.Characters == null || mystery.Characters.Count == 0)
            {
                Debug.LogError("Mystery object is invalid or contains no characters");
                Debug.Log("Attempting to restore character files from backups...");
                int restoredCount = RestoreBackupCharacterFiles();
                OnCharactersExtracted?.Invoke(restoredCount);
                return;
            }
    
            if (_backupExistingFiles)
            {
                BackupExistingCharacterFiles();
            }
    
            if (_clearExistingCharacterFiles)
            {
                ClearExistingCharacterFiles();
            }
    
            _totalCharactersToProcess = mystery.Characters.Count;
            _totalCharactersProcessed = 0;
    
            Debug.Log($"Starting extraction of {_totalCharactersToProcess} characters from mystery data");
            
            // Our approach is simple: Just directly extract character JSON from the transformed-mystery.json file
            string mysteryPath = Path.Combine(Application.streamingAssetsPath, "MysteryStorage", "transformed-mystery.json");
            
            if (!File.Exists(mysteryPath))
            {
                Debug.LogError($"Mystery file not found at: {mysteryPath}");
                Debug.Log("Attempting to restore character files from backups...");
                int restoredCount = RestoreBackupCharacterFiles();
                OnCharactersExtracted?.Invoke(restoredCount);
                return;
            }
            
            try
            {
                // Read the mystery file
                string mysteryJson = File.ReadAllText(mysteryPath);
                JObject mysteryObject = JObject.Parse(mysteryJson);
                
                // Check if it has a characters section
                if (mysteryObject["characters"] == null)
                {
                    Debug.LogError("Mystery file does not contain a 'characters' section");
                    Debug.Log("Attempting to restore character files from backups...");
                    int restoredCount = RestoreBackupCharacterFiles();
                    OnCharactersExtracted?.Invoke(restoredCount);
                    return;
                }
                
                // Make sure the output directory exists before extraction
                EnsureCharactersDirectoryExists();
                
                // Process each character in the mystery
                foreach (var characterEntry in mystery.Characters)
                {
                    string characterId = characterEntry.Key;
                    
                    try
                    {
                        // Get the character from the JSON
                        if (mysteryObject["characters"][characterId] != null)
                        {
                            // Extract the character JSON with proper formatting
                            string characterJson = mysteryObject["characters"][characterId].ToString(Formatting.Indented);
                            
                            // Save to file
                            string filePath = Path.Combine(_outputPath, $"{characterId}.json");
                            File.WriteAllText(filePath, characterJson);
                            
                            _totalCharactersProcessed++;
                            
                            // Report progress
                            float progress = (float)_totalCharactersProcessed / _totalCharactersToProcess;
                            OnExtractionProgress?.Invoke(progress);
                            
                            if (_verboseLogging)
                            {
                                Debug.Log($"Extracted character [{_totalCharactersProcessed}/{_totalCharactersToProcess}]: {characterId}");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Character {characterId} not found in the mystery JSON file");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error extracting character {characterId}: {ex.Message}");
                        Debug.LogException(ex);
                    }
                }
                
                // If no characters were extracted, try to use backups
                if (_totalCharactersProcessed == 0)
                {
                    Debug.LogWarning("No characters extracted from mystery. Attempting to restore from backups...");
                    int restoredCount = RestoreBackupCharacterFiles();
                    
                    if (restoredCount > 0)
                    {
                        Debug.Log($"Successfully restored {restoredCount} character files from backups");
                        _totalCharactersProcessed = restoredCount;
                    }
                }
                
                LogExtractionResults(_totalCharactersProcessed);
                OnCharactersExtracted?.Invoke(_totalCharactersProcessed);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error processing mystery JSON: {ex.Message}");
                Debug.LogException(ex);
                
                Debug.Log("Attempting to restore character files from backups...");
                int restoredCount = RestoreBackupCharacterFiles();
                
                if (restoredCount > 0)
                {
                    Debug.Log($"Successfully restored {restoredCount} character files from backups");
                    OnCharactersExtracted?.Invoke(restoredCount);
                }
                else
                {
                    OnCharactersExtracted?.Invoke(0);
                }
            }
        }
    
        /// <summary>
        /// Asynchronously extracts characters from the mystery JSON and creates individual character files
        /// </summary>
        /// <param name="mystery">The mystery object containing character data</param>
        /// <returns>The number of characters successfully extracted</returns>
        public async Task<int> ExtractCharactersAsync(Mystery mystery)
        {
            if (mystery == null || mystery.Characters == null || mystery.Characters.Count == 0)
            {
                Debug.LogError("Mystery object is invalid or contains no characters");
                OnCharactersExtracted?.Invoke(0);
                return 0;
            }
    
            if (_backupExistingFiles)
            {
                BackupExistingCharacterFiles();
            }
    
            if (_clearExistingCharacterFiles)
            {
                ClearExistingCharacterFiles();
            }
    
            _totalCharactersToProcess = mystery.Characters.Count;
            _totalCharactersProcessed = 0;
    
            Debug.Log($"Starting async extraction of {_totalCharactersToProcess} characters from mystery data");
            
            // Direct JSON extraction approach
            string mysteryPath = Path.Combine(Application.streamingAssetsPath, "MysteryStorage", "transformed-mystery.json");
            
            if (!File.Exists(mysteryPath))
            {
                Debug.LogError($"Mystery file not found at: {mysteryPath}");
                OnCharactersExtracted?.Invoke(0);
                return 0;
            }
            
            try
            {
                // Read the mystery file
                string mysteryJson = await File.ReadAllTextAsync(mysteryPath);
                JObject mysteryObject = JObject.Parse(mysteryJson);
                
                // Check if it has a characters section
                if (mysteryObject["characters"] == null)
                {
                    Debug.LogError("Mystery file does not contain a 'characters' section");
                    OnCharactersExtracted?.Invoke(0);
                    return 0;
                }
                
                // Ensure the output directory exists before any extraction
                MainThreadHelper.Instance.QueueOnMainThread(() => 
                {
                    EnsureCharactersDirectoryExists();
                });
                
                // Create a list of tasks for processing characters
                var tasks = new List<Task>();
                var semaphore = new System.Threading.SemaphoreSlim(_maxConcurrentExtractions);
    
                foreach (var characterEntry in mystery.Characters)
                {
                    // Limit concurrent processing
                    await semaphore.WaitAsync();
    
                    string characterId = characterEntry.Key;
    
                    // Process each character in a separate task
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            // Get the character from the JSON
                            if (mysteryObject["characters"][characterId] != null)
                            {
                                // Extract the character JSON with proper formatting
                                string characterJson = mysteryObject["characters"][characterId].ToString(Formatting.Indented);
                                
                                // Ensure the output directory exists before saving
                                if (!Directory.Exists(_outputPath))
                                {
                                    Directory.CreateDirectory(_outputPath);
                                }
                                
                                // Save to file
                                string filePath = Path.Combine(_outputPath, $"{characterId}.json");
                                await File.WriteAllTextAsync(filePath, characterJson);
                                
                                // Increment counter and report progress safely
                                lock (this)
                                {
                                    _totalCharactersProcessed++;
                                    float progress = (float)_totalCharactersProcessed / _totalCharactersToProcess;
                                    
                                    // Can't directly call Unity API from background thread, so use main thread dispatcher
                                    MainThreadHelper.Instance.QueueOnMainThread(() => 
                                    {
                                        OnExtractionProgress?.Invoke(progress);
                                        
                                        if (_verboseLogging)
                                        {
                                            Debug.Log($"Extracted character [{_totalCharactersProcessed}/{_totalCharactersToProcess}]: {characterId}");
                                        }
                                    });
                                }
                            }
                            else
                            {
                                MainThreadHelper.Instance.QueueOnMainThread(() =>
                                {
                                    Debug.LogWarning($"Character {characterId} not found in the mystery JSON file");
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            MainThreadHelper.Instance.QueueOnMainThread(() =>
                            {
                                Debug.LogError($"Error extracting character {characterId}: {ex.Message}");
                                Debug.LogException(ex);
                            });
                        }
                        finally
                        {
                            // Release semaphore to allow next task
                            semaphore.Release();
                        }
                        
                        // Small delay to prevent thread starvation
                        await Task.Delay((int)(_extractionDelay * 1000));
                    }));
                }
    
                // Wait for all extraction tasks to complete
                await Task.WhenAll(tasks);
    
                // Log results on the main thread
                MainThreadHelper.Instance.QueueOnMainThread(() => 
                {
                    LogExtractionResults(_totalCharactersProcessed);
                    OnCharactersExtracted?.Invoke(_totalCharactersProcessed);
                });
    
                return _totalCharactersProcessed;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error processing mystery JSON: {ex.Message}");
                Debug.LogException(ex);
                OnCharactersExtracted?.Invoke(0);
                return 0;
            }
        }
    
        /// <summary>
        /// Backs up existing character files before extraction
        /// </summary>
        private void BackupExistingCharacterFiles()
        {
            if (!Directory.Exists(_outputPath))
            {
                Debug.Log("No character files to back up - directory doesn't exist");
                return;
            }
    
            if (!Directory.Exists(_backupPath))
            {
                Directory.CreateDirectory(_backupPath);
            }
    
            var existingFiles = Directory.GetFiles(_outputPath, "*.json");
            if (existingFiles.Length == 0)
            {
                Debug.Log("No character files to back up - no files found");
                return;
            }
    
            Debug.Log($"Backing up {existingFiles.Length} character files...");
    
            foreach (var file in existingFiles)
            {
                string fileName = Path.GetFileName(file);
                string backupFilePath = Path.Combine(_backupPath, fileName);
                
                try
                {
                    File.Copy(file, backupFilePath, true);
                    
                    if (_verboseLogging)
                    {
                        Debug.Log($"Backed up {fileName} to {_backupPath}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error backing up file {fileName}: {ex.Message}");
                }
            }
    
            Debug.Log($"Backed up {existingFiles.Length} character files to {_backupPath}");
        }
    
        /// <summary>
        /// Clears existing character files before extraction
        /// </summary>
        private void ClearExistingCharacterFiles()
        {
            if (!Directory.Exists(_outputPath))
            {
                Debug.Log("No character files to clear - directory doesn't exist");
                return;
            }
    
            var existingFiles = Directory.GetFiles(_outputPath, "*.json");
            if (existingFiles.Length == 0)
            {
                Debug.Log("No character files to clear - no files found");
                return;
            }
    
            Debug.Log($"Clearing {existingFiles.Length} existing character files...");
    
            foreach (var file in existingFiles)
            {
                try
                {
                    File.Delete(file);
                    
                    if (_verboseLogging)
                    {
                        Debug.Log($"Deleted file: {Path.GetFileName(file)}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error deleting file {Path.GetFileName(file)}: {ex.Message}");
                }
            }
    
            Debug.Log($"Cleared {existingFiles.Length} character files from {_outputPath}");
        }
    
        /// <summary>
        /// Logs the results of the extraction process
        /// </summary>
        private void LogExtractionResults(int count)
        {
            if (count > 0)
            {
                string chars = count == 1 ? "character" : "characters";
                Debug.Log($"Successfully extracted {count} {chars} to {_outputPath}");
                
                // Log the files that were created
                if (_verboseLogging)
                {
                    var files = Directory.GetFiles(_outputPath, "*.json");
                    Debug.Log("Created the following character files:");
                    foreach (var file in files)
                    {
                        Debug.Log($"  - {Path.GetFileName(file)}");
                    }
                }
            }
            else
            {
                Debug.LogWarning("No characters were extracted - check for errors above");
            }
        }
        
        /// <summary>
        /// Tests extraction of a specific character - useful for debugging
        /// </summary>
        public void TestSpecificCharacter(string characterId)
        {
            Debug.Log($"Testing extraction of character: {characterId}");
            
            // Load mystery from StreamingAssets
            string mysteryPath = Path.Combine(Application.streamingAssetsPath, "MysteryStorage", "transformed-mystery.json");
            if (File.Exists(mysteryPath))
            {
                try
                {
                    // Read the mystery file
                    string mysteryJson = File.ReadAllText(mysteryPath);
                    JObject mysteryObject = JObject.Parse(mysteryJson);
                    
                    // Check if the character exists
                    if (mysteryObject["characters"] != null && mysteryObject["characters"][characterId] != null)
                    {
                        // Extract the character JSON
                        string characterJson = mysteryObject["characters"][characterId].ToString(Formatting.Indented);
                        
                        // Ensure the Characters directory exists
                        EnsureCharactersDirectoryExists();
                        
                        // Save to file
                        string filePath = Path.Combine(_outputPath, $"{characterId}.json");
                        File.WriteAllText(filePath, characterJson);
                        
                        Debug.Log($"Character '{characterId}' extracted successfully to: {filePath}");
                    }
                    else
                    {
                        Debug.LogError($"Character '{characterId}' not found in mystery JSON.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error processing mystery JSON: {ex.Message}");
                    Debug.LogException(ex);
                }
            }
            else
            {
                Debug.LogError($"Mystery file not found at: {mysteryPath}");
            }
        }
    
        /// <summary>
        /// Restores character files from backup if extraction fails
        /// </summary>
        /// <returns>Number of character files restored</returns>
        public int RestoreBackupCharacterFiles()
        {
            int restoredCount = 0;
            
            // First check if we have character backups
            string backupsFolderPath = Path.Combine(Application.streamingAssetsPath, _charactersBackupFolder);
            
            if (!Directory.Exists(backupsFolderPath))
            {
                Debug.LogError($"Character backups folder not found at: {backupsFolderPath}");
                
                // As a fallback, check CharacterBackups/UnusedChars folder which is known to contain files
                string fallbackPath = Path.Combine(Application.streamingAssetsPath, "CharacterBackups", "UnusedChars");
                if (Directory.Exists(fallbackPath))
                {
                    backupsFolderPath = fallbackPath;
                    Debug.Log($"Found fallback character backups at: {fallbackPath}");
                }
                else
                {
                    Debug.LogError("No backup character files found. Character initialization will likely fail.");
                    return 0;
                }
            }
            
            // Get all JSON files in the backup folder
            var backupFiles = Directory.GetFiles(backupsFolderPath, "*.json");
            if (backupFiles.Length == 0)
            {
                Debug.LogError("No backup character files found in backup folder.");
                return 0;
            }
            
            // Ensure output directory exists
            EnsureCharactersDirectoryExists();
            
            // Copy files from backup
            foreach (var backupFile in backupFiles)
            {
                try
                {
                    string fileName = Path.GetFileName(backupFile);
                    string destPath = Path.Combine(_outputPath, fileName);
                    
                    File.Copy(backupFile, destPath, true);
                    restoredCount++;
                    
                    Debug.Log($"Restored character file: {fileName}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error restoring backup file {Path.GetFileName(backupFile)}: {ex.Message}");
                }
            }
            
            return restoredCount;
        }
    }
}