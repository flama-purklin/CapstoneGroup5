using UnityEngine;
using UnityEngine.Rendering;

// Drop this on a bootstrap GameObject that lives the whole session
[ExecuteAlways]
public class FixedInternalRes : MonoBehaviour
{
    [Tooltip("Desired INTERNAL resolution, will only down-scale")]
    public Vector2Int target = new(960, 540);

    void Start() => Apply();
    void OnValidate() => Apply();

    void Apply()
    {
        // ratio <1 means “shrink”; >1 would upscale – clamp it.
        float w = (float)target.x / Screen.width;
        float h = (float)target.y / Screen.height;

        // take the smaller axis to keep aspect, clamp to [0,1]
        float scale = Mathf.Clamp01(Mathf.Min(w, h));

        ScalableBufferManager.ResizeBuffers(scale, scale);
    }

    void OnDisable()               // restore normal rendering when script is off
        => ScalableBufferManager.ResizeBuffers(1f, 1f);
}
