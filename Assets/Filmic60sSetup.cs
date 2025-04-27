using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[ExecuteAlways, DisallowMultipleComponent]
public sealed class Filmic60sSetup : MonoBehaviour
{
    [Header("Textures")]
    public Texture lensDirt;
    public Texture grainTex;

    [Header("Gate-Weave (fallback)")]
    [Tooltip("Pixel amplitude of the jitter")]
    public float jitterAmp = 0.15f;
    [Tooltip("Oscillation speed in Hertz")]
    public float jitterFreq = 2f;

    // Volume & overrides
    Volume vol;
    Bloom bloom;
    ScreenSpaceLensFlare flare;
    FilmGrain grain;
    Vignette vig;
    LensDistortion dist;
    ChromaticAberration ca;
    DepthOfField dof;

    Vector3 baseLocalPos;

    void OnEnable()
    {
        //--------------------------------------------------
        // 1. Create / grab global Volume on this camera
        //--------------------------------------------------
        vol = GetComponent<Volume>() ?? gameObject.AddComponent<Volume>();
        vol.isGlobal = true;
        vol.priority = 999f;                                  // win every blend
        if (vol.profile == null)
            vol.profile = ScriptableObject.CreateInstance<VolumeProfile>();

        // Local helper to add or fetch an override
        T Add<T>() where T : VolumeComponent, new()
            => vol.profile.TryGet(out T c) ? c : vol.profile.Add<T>(true);

        //--------------------------------------------------
        // 2. Configure built-in overrides
        //--------------------------------------------------
        bloom = Add<Bloom>();
        bloom.intensity.value = 0.7f;
        bloom.threshold.value = 1.1f;
        bloom.dirtTexture.value = lensDirt;

        flare = Add<ScreenSpaceLensFlare>();
        flare.intensity.value = 0.4f;

        grain = Add<FilmGrain>();
        grain.type.value = FilmGrainLookup.Custom;  // <— fixed enum
        grain.intensity.value = 0.35f;
        grain.response.value = 0.8f;
        grain.texture.value = grainTex;

        vig = Add<Vignette>();
        vig.intensity.value = 0.25f;

        dist = Add<LensDistortion>();
        dist.intensity.value = -0.05f;

        ca = Add<ChromaticAberration>();
        ca.intensity.value = 0.06f;

        dof = Add<DepthOfField>();
        dof.active = true;
        dof.focusMode.value = DepthOfFieldMode.UsePhysicalCamera;

        // store for gate-weave
        baseLocalPos = transform.localPosition;
    }

    void Update()
    {
        //--------------------------------------------------
        // 3. Gate-weave fallback if no Cinemachine VC
        //--------------------------------------------------
        // check by name so there's zero compile-time dep
        bool hasCineVC = GetComponent("CinemachineVirtualCamera") != null;
        if (!hasCineVC)
        {
            float t = Time.time * jitterFreq * 2f * Mathf.PI;
            float x = Mathf.PerlinNoise(t, 0f) - 0.5f;
            float y = Mathf.PerlinNoise(0f, t) - 0.5f;
            transform.localPosition = baseLocalPos + new Vector3(x, y) * jitterAmp;
        }
    }
}
