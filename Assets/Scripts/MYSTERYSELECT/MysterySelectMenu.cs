using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class MysterySelectMenu : MonoBehaviour
{
    public GameObject buttonPrefab;        // assign in Inspector (Button with TMP_Text child)
    public Transform buttonContainer;      // assign in Inspector (Content of Scroll View)
    public GameObject mysteryMenuOverlay;  // assign in Inspector (Canvas to enable/disable)

    public ParsingControl parser;          // assign in Inspector (ParsingControl from SystemTest Scene)
    public string nextScene = "SystemsTest";

    private void Start()
    {
        mysteryMenuOverlay.SetActive(false); // hidden at start
    }

    public void ShowMenu()
{
    if (parser == null)
        parser = FindObjectOfType<ParsingControl>();

    mysteryMenuOverlay.SetActive(true);

    // Clear previous buttons
    foreach (Transform child in buttonContainer)
        Destroy(child.gameObject);

    string folderPath = Path.Combine(Application.streamingAssetsPath, "MysteryStorage");
    string[] jsonFiles = Directory.GetFiles(folderPath, "*.json");

    foreach (string file in jsonFiles)
    {
        GameObject newButton = Instantiate(buttonPrefab, buttonContainer);
        newButton.GetComponentInChildren<TMP_Text>().text = Path.GetFileNameWithoutExtension(file);

        newButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            Debug.Log("🟡 Button clicked: starting parse");

            parser.OnParseComplete = () =>
            {
                Debug.Log("✅ PARSING COMPLETE, LOADING SCENE...");
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
            };

            parser.ParseMystery(file);
        });
    }
}

}
