using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasFade : MonoBehaviour
{
    [Tooltip("Seconds for fade in/out")]
    [SerializeField] private float fadeDuration = 0.3f;
    [Tooltip("Optional curve for non‑linear fades")]
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.Linear(0, 0, 1, 1);

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    public void FadeIn()
    {
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(0f, 1f));
    }

    public void FadeOut()
    {
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(1f, 0f));
    }

    private IEnumerator FadeRoutine(float start, float end)
    {
        float elapsed = 0f;
        if (end == 0f)
            canvasGroup.blocksRaycasts = false;
        else
            canvasGroup.blocksRaycasts = true;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            canvasGroup.alpha = Mathf.LerpUnclamped(start, end, fadeCurve.Evaluate(t));
            yield return null;
        }

        canvasGroup.alpha = end;
        if (end == 0f)
            canvasGroup.blocksRaycasts = false;
    }
}
