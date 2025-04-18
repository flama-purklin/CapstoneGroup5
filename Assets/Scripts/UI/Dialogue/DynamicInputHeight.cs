using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_InputField))]
[RequireComponent(typeof(LayoutElement))]
public class DynamicInputHeight : MonoBehaviour
{
    [Tooltip("Minimum height for the input field")]
    [SerializeField] private float minHeight = 40f;
    [Tooltip("Maximum height for the input field")]
    [SerializeField] private float maxHeight = 150f;

    private TMP_InputField inputField;
    private LayoutElement layoutElement;
    private TMPro.TMP_Text textComponent;


    private void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        layoutElement = GetComponent<LayoutElement>();
        textComponent = inputField.textComponent;

        layoutElement.minHeight = minHeight;
        layoutElement.preferredHeight = minHeight;
    }

    private void OnEnable()
    {
        inputField.onValueChanged.AddListener(OnTextChanged);
    }

    private void OnDisable()
    {
        inputField.onValueChanged.RemoveListener(OnTextChanged);
    }

    private void OnTextChanged(string _)
    {
        UpdateHeight();
    }

    private void UpdateHeight()
    {
        if (textComponent == null || layoutElement == null)
            return;

        // Compute preferred total height for the text area
        float preferred = textComponent.preferredHeight
                        + (inputField.textViewport.rect.height - textComponent.rectTransform.rect.height);
        float clamped = Mathf.Clamp(preferred, minHeight, maxHeight);
        layoutElement.preferredHeight = clamped;
    }

    /// <summary>
    /// Reset input field to its minimum height.
    /// </summary>
    public void ResetHeightToMin()
    {
        if (layoutElement != null)
            layoutElement.preferredHeight = minHeight;
    }
}
