using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ScrollFadeManager : MonoBehaviour
{
    [Tooltip("ScrollRect containing the content")]
    [SerializeField] private ScrollRect scrollRect;
    [Tooltip("Gradient GameObject at bottom of viewport")]
    [SerializeField] private GameObject scrollFade;
    [Tooltip("Normalized position above which the fade shows (0 = bottom, 1 = top)")]
    [SerializeField, Range(0f, 1f)] private float fadeThreshold = 0.98f;

    private RectTransform contentRect;
    private RectTransform viewportRect;

    private void Awake()
    {
        if (scrollRect == null)
            scrollRect = GetComponent<ScrollRect>();
        contentRect = scrollRect.content;
        viewportRect = scrollRect.viewport;
    }

    private void OnEnable()
    {
        scrollRect.onValueChanged.AddListener(OnScrollChanged);
        UpdateFade();
    }

    private void OnDisable()
    {
        scrollRect.onValueChanged.RemoveListener(OnScrollChanged);
    }

    private void OnScrollChanged(Vector2 _) => UpdateFade();

    /// <summary>
    /// Call this after content size changes (e.g. new text) to refresh fade.
    /// </summary>
    public void ContentChanged()
    {
        UpdateFade();
    }

    private void UpdateFade()
    {
        if (scrollFade == null || contentRect == null || viewportRect == null)
            return;

        bool contentTooTall = contentRect.rect.height > viewportRect.rect.height + 0.5f;
        bool notAtBottom = scrollRect.verticalNormalizedPosition > (1f - fadeThreshold);

        scrollFade.SetActive(contentTooTall && notAtBottom);
    }
}
