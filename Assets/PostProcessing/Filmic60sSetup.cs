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
        // Set target frame rate to 24 fps for cinematic look and consistent performance
        Application.targetFrameRate = 24;
        
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
            Debug.LogError("[DEEP-DIAGNOSIS] No Volume found in scene! Creating one...");
            var go = new GameObject("Filmic60s_Volume");
            vol = go.AddComponent<Volume>();
            vol.isGlobal = true;
        }
        else
        {
            Debug.Log($"[DEEP-DIAGNOSIS] Found Volume: '{vol.gameObject.name}', Is Global: {vol.isGlobal}, Priority: {vol.priority}");
        }
        
        if (!vol.sharedProfile)
        {
            Debug.LogError("[DEEP-DIAGNOSIS] Volume has no profile! Creating one...");
            vol.sharedProfile = ScriptableObject.CreateInstance<VolumeProfile>();
        }
        else
        {
            Debug.Log($"[DEEP-DIAGNOSIS] Using Volume Profile: '{vol.sharedProfile.name}', Instance ID: {vol.sharedProfile.GetInstanceID()}");
            
            // Check if there's already a DoF component in the profile
            if (vol.sharedProfile.TryGet(out DepthOfField existingDof))
            {
                Debug.Log($"[DEEP-DIAGNOSIS] Profile already has DoF component with Focus Mode: {existingDof.focusMode.value}");
            }
            else
            {
                Debug.Log("[DEEP-DIAGNOSIS] No existing DoF component in profile");
            }
        }
        
        profile = vol.sharedProfile;
        Debug.Log($"[DEEP-DIAGNOSIS] Stored profile reference. Has DoF: {profile.Has<DepthOfField>()}");
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
        vig.intensity.value  = 0.33f;
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

        // 9) Explicitly disable Motion Blur (added to fix motion blur artifact at low framerates)
        var motionBlur = GetOrAdd<MotionBlur>();
        motionBlur.active = false;
        motionBlur.intensity.value = 0f;
        
        // 10) Depth of Field with direct Manual mode
        depthOfField = GetOrAdd<DepthOfField>();
        
        // Always start with active=true
        depthOfField.active = true;
        
        // CRUCIAL: Set the mode to Manual to allow direct focus control
        // This is different from "Manual Ranges" which uses fixed ranges 
        depthOfField.focusMode.overrideState = true;
        depthOfField.focusMode.value = DepthOfFieldMode.Manual;
        
        // Initial focus value (will be updated by camera script)
        depthOfField.focusDistance.overrideState = true;
        depthOfField.focusDistance.value = 5f; // Default starting value
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
}
