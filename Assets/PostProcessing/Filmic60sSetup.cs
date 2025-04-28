// Assets/PostProcessing/Filmic60sSetup.cs
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[ExecuteAlways, RequireComponent(typeof(Camera))]
public class Filmic60sSetup : MonoBehaviour
{
    [Header("Core Assets")]
    [Tooltip("3D LUT (import your .CUBE as Texture3D)")]
    public Texture   lutTexture;
    [Tooltip("Halation shader (e.g. Hidden/Filmic/Halation)")]
    public Shader    halationShader;

    [Header("Optional Lens Dirt")]
    [Tooltip("Warm lens-dirt texture, sRGB ON")]
    public Texture   dirtTexture;

    VolumeProfile    profile;
    CustomPassVolume passVolume;
    Coroutine        breathRoutine;
    DepthOfField     depthOfField; // Store reference for dynamic updates

    void OnEnable()
    {
        SetupVolume();
        ConfigureOverrides();
        SetupHalationPass();
        SetupPhysicalCamera();

        // start film-breath flicker
        breathRoutine = StartCoroutine(FilmBreathCoroutine());
    }

    void OnDisable()
    {
        if (breathRoutine != null) StopCoroutine(breathRoutine);
    }

    void SetupVolume()
    {
        var vol = Object.FindFirstObjectByType<Volume>();
        if (!vol)
        {
            var go = new GameObject("Filmic60s_Volume");
            vol = go.AddComponent<Volume>();
            vol.isGlobal = true;
        }
        if (!vol.sharedProfile)
            vol.sharedProfile = ScriptableObject.CreateInstance<VolumeProfile>();
        profile = vol.sharedProfile;
    }

    T GetOrAdd<T>() where T : VolumeComponent
        => profile.TryGet(out T comp) ? comp : profile.Add<T>(true);

    void ConfigureOverrides()
    {
        // 1) Tonemapping → External LUT
        var ton = GetOrAdd<Tonemapping>();
        ton.active               = true;
        ton.mode.value           = TonemappingMode.External;
        ton.lutTexture.value     = lutTexture;
        ton.lutContribution.value= 1f;

        // 2) Color Adjustments
        var ca = GetOrAdd<ColorAdjustments>();
        ca.postExposure.value = 0.1f;
        ca.contrast.value     = 15f;
        ca.saturation.value   = 20f;

        // 3) Bloom + optional lens dirt
        var bloom = GetOrAdd<Bloom>();
        bloom.threshold.value     = 1.1f;
        bloom.intensity.value     = 0.7f;
        bloom.scatter.value       = 0.7f;
        bloom.dirtTexture.value   = dirtTexture;
        bloom.dirtIntensity.value = dirtTexture ? 0.5f : 0f;

        // 4) Film Grain
        var fg = GetOrAdd<FilmGrain>();
        fg.type.value      = FilmGrainLookup.Medium1;
        fg.intensity.value = 0.35f;
        fg.response.value  = 0.8f;

        // 5) Vignette
        var vig = GetOrAdd<Vignette>();
        vig.intensity.value  = 0.25f;
        vig.smoothness.value = 0.5f;

        // 6) Chromatic Aberration
        var cab = GetOrAdd<ChromaticAberration>();
        cab.intensity.value = 0.1f;

        // 7) Lens Distortion
        var ld = GetOrAdd<LensDistortion>();
        ld.intensity.value = -0.02f;
        ld.scale.value     = 1.02f;

        // 8) Screen‐Space Lens Flare (HDRP 17+)
        #if UNITY_2024_2_OR_NEWER
        var ssfl = GetOrAdd<ScreenSpaceLensFlare>();
        ssfl.intensity.value = 0.5f;
        #endif

        // 9) Depth of Field (Manual focus mode for dynamic control)
        depthOfField = GetOrAdd<DepthOfField>();
        depthOfField.active            = true;
        depthOfField.focusMode.value   = DepthOfFieldMode.Manual;
        depthOfField.focusDistance.value = 10f; // Default starting value
    }

    void SetupHalationPass()
    {
        passVolume = Object.FindFirstObjectByType<CustomPassVolume>();
        if (!passVolume)
        {
            var go = new GameObject("Filmic60s_HalationPass");
            passVolume = go.AddComponent<CustomPassVolume>();
            passVolume.isGlobal       = true;
            passVolume.injectionPoint = CustomPassInjectionPoint.AfterPostProcess;
        }
        if (passVolume.customPasses.Count == 0)
            passVolume.customPasses.Add(new HalationCustomPass(halationShader));
    }

    void SetupPhysicalCamera()
    {
        var cam = GetComponent<Camera>();
        cam.usePhysicalProperties = true;
        cam.aperture              = 4f;  // f/4
        cam.bladeCount            = 6;   // hexagonal bokeh
    }

    // Film-breath: small sin-wave exposure flicker
    IEnumerator FilmBreathCoroutine()
    {
        var ca = GetOrAdd<ColorAdjustments>();
        while (true)
        {
            float t = Time.time * Mathf.PI;
            ca.postExposure.value = 0.1f + Mathf.Sin(t) * 0.02f;
            yield return null;
        }
    }

    // Public method to update DoF focus distance
    public void UpdateFocusDistance(float distance)
    {
        if (depthOfField != null)
        {
            depthOfField.focusDistance.value = distance;
        }
    }
}
