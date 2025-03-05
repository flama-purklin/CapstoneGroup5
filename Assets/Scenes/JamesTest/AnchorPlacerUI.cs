using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class AnchorPlacerUI : MonoBehaviour
{
    public GameObject uiPanel;
    public TMP_Dropdown typeDropdown;
    public TMP_Dropdown rotationDropdown;
    public Button placeButton;
    public List<PrefabEntry> objectPrefabsList;

    private AnchorPlacer selectedAnchor;

    void Start()
    {
        uiPanel.SetActive(false);
        placeButton.onClick.AddListener(PlaceObject);
    }

    public void OpenUI(AnchorPlacer anchor)
    {
        selectedAnchor = anchor;
        uiPanel.SetActive(true);
    }

    public void PlaceObject()
    {
        Debug.Log("Attempting to place object!");
        if (selectedAnchor == null) return;

        string selectedType = typeDropdown.options[typeDropdown.value].text.ToLower();
        int rotation = int.Parse(rotationDropdown.options[rotationDropdown.value].text);

        PrefabEntry prefabEntry = objectPrefabsList.Find(entry => entry.key.ToLower() == selectedType);
        if (prefabEntry != null && prefabEntry.prefab != null)
        {
            GameObject placedObject = Instantiate(prefabEntry.prefab, selectedAnchor.transform.position, Quaternion.Euler(0, rotation, 0));

            // Set parent of placed object anchor sibling
            placedObject.transform.SetParent(selectedAnchor.transform.parent);
            if (selectedAnchor.placedObject != null)
            {
                Destroy(selectedAnchor.placedObject);
            }

            selectedAnchor.placedObject = placedObject;
        }

        uiPanel.SetActive(false);
    }
}