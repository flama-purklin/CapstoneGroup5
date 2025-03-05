using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnchorPlacer : MonoBehaviour
{
    public GameObject placedObject; // The object placed on this anchor
    private static AnchorPlacerUI anchorUI;
    private Vector3 originalScale;

    void Start()
    {
        StartCoroutine(FindAnchorUICoroutine());
        originalScale = transform.localScale;
    }

    public void FindAnchorPlacerUI()
    { 
        
    }

    private IEnumerator FindAnchorUICoroutine()
    {
        while (anchorUI == null)
        {
            anchorUI = FindFirstObjectByType<AnchorPlacerUI>();

            if (anchorUI == null)
            {
                Debug.Log("AnchorPlacerUI not found in scene, retrying...");
                yield return new WaitForSeconds(0.5f); // Wait half a second before retrying
            }
            else
            {
                Debug.Log("AnchorPlacerUI found!");
            }
        }
    }

    // This method will run when the mouse enters the collider of this object
    void OnMouseEnter()
    {
        Debug.Log("Mouse is over " + this.transform.name);
        // Double the scale when mouse is over
        transform.localScale = originalScale * 2;
    }

    // This method will run when the mouse exits the collider of this object
    void OnMouseExit()
    {
        Debug.Log("Mouse left " + this.transform.name);
        // Reset the scale to original
        transform.localScale = originalScale;
    }

    void OnMouseDown()
    {
        Debug.Log(this.transform.parent.name + " selection registered!");
        if (anchorUI != null)
        {
            anchorUI.OpenUI(this);
        }
    }

    public void PlaceObject(GameObject prefab, int rotation)
    {
        if (placedObject != null)
        {
            Destroy(placedObject);
        }

        placedObject = Instantiate(prefab, transform.position, Quaternion.Euler(0, rotation, 0));
        placedObject.transform.SetParent(transform);
    }
}