using UnityEngine;

public class ConstellationToggle : MonoBehaviour
{
    private Canvas canvas;

    void Start()
    {
        // Try to get the Canvas component on this GameObject
        canvas = GetComponent<Canvas>();

        if (canvas == null)
        {
            Debug.LogWarning("No Canvas component found for Constellation.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (canvas != null)
            {
                // Toggle just the canvas visibility
                canvas.enabled = !canvas.enabled;
            }
            else
            {
                // Toggle the entire GameObject if no canvas is found
                gameObject.SetActive(!gameObject.activeSelf);
            }
        }
    }
}
