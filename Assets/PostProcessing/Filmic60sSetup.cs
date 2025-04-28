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

        // 9) Depth of Field with direct Manual mode
        depthOfField = GetOrAdd<DepthOfField>();
        
        if (depthOfField == null) {
            Debug.LogError("[DIAGNOSIS] CRITICAL ERROR: Could not get or add DepthOfField to volume profile!");
            return;
        }
        
        // Always start with active=true
        depthOfField.active = true;
        
        // CRUCIAL: Set the mode to Manual to allow direct focus control
        // This is different from "Manual Ranges" which uses fixed ranges 
        depthOfField.focusMode.overrideState = true;
        depthOfField.focusMode.value = DepthOfFieldMode.Manual;
        
        // Initial focus value (will be updated by camera script)
        depthOfField.focusDistance.overrideState = true;
        depthOfField.focusDistance.value = 5f; // Default starting value
        
        // EXTREME BLUR SETTINGS TO MAKE FOCUS OBVIOUS
        // Different HDRP versions use different property names, so we try both
        
        // Try setting "maxBlur" properties if they exist
        try {
            var propertyInfo = typeof(DepthOfField).GetProperty("nearMaxBlur");
            if (propertyInfo != null) {
                Debug.Log("[DEEP-DIAGNOSIS] Setting nearMaxBlur/farMaxBlur (older HDRP)");
                depthOfField.GetType().GetProperty("nearMaxBlur").GetValue(depthOfField, null).GetType().GetProperty("overrideState").SetValue(depthOfField.GetType().GetProperty("nearMaxBlur").GetValue(depthOfField, null), true);
                depthOfField.GetType().GetProperty("nearMaxBlur").GetValue(depthOfField, null).GetType().GetProperty("value").SetValue(depthOfField.GetType().GetProperty("nearMaxBlur").GetValue(depthOfField, null), 20f);
                
                depthOfField.GetType().GetProperty("farMaxBlur").GetValue(depthOfField, null).GetType().GetProperty("overrideState").SetValue(depthOfField.GetType().GetProperty("farMaxBlur").GetValue(depthOfField, null), true);
                depthOfField.GetType().GetProperty("farMaxBlur").GetValue(depthOfField, null).GetType().GetProperty("value").SetValue(depthOfField.GetType().GetProperty("farMaxBlur").GetValue(depthOfField, null), 20f);
            }
        }
        catch (System.Exception e) {
            Debug.Log($"[DEEP-DIAGNOSIS] Could not set nearMaxBlur/farMaxBlur: {e.Message}");
        }
        
        // Try setting "maxBlurSize" properties if they exist
        try {
            var propertyInfo = typeof(DepthOfField).GetProperty("nearMaxBlurSize");
            if (propertyInfo != null) {
                Debug.Log("[DEEP-DIAGNOSIS] Setting nearMaxBlurSize/farMaxBlurSize (newer HDRP)");
                depthOfField.GetType().GetProperty("nearMaxBlurSize").GetValue(depthOfField, null).GetType().GetProperty("overrideState").SetValue(depthOfField.GetType().GetProperty("nearMaxBlurSize").GetValue(depthOfField, null), true);
                depthOfField.GetType().GetProperty("nearMaxBlurSize").GetValue(depthOfField, null).GetType().GetProperty("value").SetValue(depthOfField.GetType().GetProperty("nearMaxBlurSize").GetValue(depthOfField, null), 20f);
                
                depthOfField.GetType().GetProperty("farMaxBlurSize").GetValue(depthOfField, null).GetType().GetProperty("overrideState").SetValue(depthOfField.GetType().GetProperty("farMaxBlurSize").GetValue(depthOfField, null), true);
                depthOfField.GetType().GetProperty("farMaxBlurSize").GetValue(depthOfField, null).GetType().GetProperty("value").SetValue(depthOfField.GetType().GetProperty("farMaxBlurSize").GetValue(depthOfField, null), 20f);
            }
        }
        catch (System.Exception e) {
            Debug.Log($"[DEEP-DIAGNOSIS] Could not set nearMaxBlurSize/farMaxBlurSize: {e.Message}");
        }
        
        Debug.Log("[DIAGNOSIS] DoF component details:");
        Debug.Log($"[DIAGNOSIS] - Active: {depthOfField.active}");
        Debug.Log($"[DIAGNOSIS] - Focus Mode: {depthOfField.focusMode.value}, Override: {depthOfField.focusMode.overrideState}");
        Debug.Log($"[DIAGNOSIS] - Focus Distance: {depthOfField.focusDistance.value}, Override: {depthOfField.focusDistance.overrideState}");
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
        // Log method call for immediate issues
        Debug.Log($"[DIAGNOSIS] UpdateFocusDistance called with distance = {distance}");
        
        // Verify profile is valid
        if (profile == null)
        {
            Debug.LogError("[DEEP-DIAGNOSIS] FATAL: Volume profile is null! Trying to recreate...");
            SetupVolume();
            
            if (profile == null) {
                Debug.LogError("[DEEP-DIAGNOSIS] FATAL: Failed to recreate profile!");
                return;
            }
        }
        
        // Check if profile still has the DoF component
        bool profileHasDof = profile.Has<DepthOfField>();
        Debug.Log($"[DEEP-DIAGNOSIS] Profile has DoF component: {profileHasDof}");
        
        if (depthOfField == null || !profileHasDof)
        {
            Debug.LogError("[DEEP-DIAGNOSIS] DoF component missing - attempting reconstruction");
            
            // Re-get or re-add the component
            depthOfField = GetOrAdd<DepthOfField>();
            
            if (depthOfField == null) {
                Debug.LogError("[DEEP-DIAGNOSIS] FATAL: Reconstruction failed, still null!");
                return;
            }
            else {
                Debug.Log("[DEEP-DIAGNOSIS] Successfully reconstructed DoF component");
                
                // Re-initialize key settings
                depthOfField.active = true;
                depthOfField.focusMode.value = DepthOfFieldMode.Manual;
                depthOfField.focusMode.overrideState = true;
            }
        }
        
        try
        {
            // Store old value to verify change
            float oldDistance = depthOfField.focusDistance.value;
            
            // Force critical override states - only parameters have overrideState
            depthOfField.focusMode.overrideState = true;
            depthOfField.focusDistance.overrideState = true;
            
            // Force mode to Manual again
            if (depthOfField.focusMode.value != DepthOfFieldMode.Manual)
            {
                depthOfField.focusMode.value = DepthOfFieldMode.Manual;
                Debug.LogError("[DEEP-DIAGNOSIS] Focus mode changed back to Manual - this shouldn't happen!");
            }
            
            // Ensure component is active
            if (!depthOfField.active)
            {
                depthOfField.active = true;
                Debug.LogError("[DEEP-DIAGNOSIS] DoF was inactive! Re-activated.");
            }
            
            // CORE FUNCTIONALITY: Set the focus distance based on player distance
            depthOfField.focusDistance.value = distance;
            
            // Log every update but with throttled details
            if (Time.frameCount % 30 == 0)
            {
                Debug.Log($"[DEEP-DIAGNOSIS] DoF update values:");
                Debug.Log($"[DEEP-DIAGNOSIS] - Old: {oldDistance:F2} → New: {distance:F2} (After set: {depthOfField.focusDistance.value:F2})");
                Debug.Log($"[DEEP-DIAGNOSIS] - Focus distance override state: {depthOfField.focusDistance.overrideState}");
                Debug.Log($"[DEEP-DIAGNOSIS] - Focus mode: {depthOfField.focusMode.value}, Override: {depthOfField.focusMode.overrideState}");
                Debug.Log($"[DEEP-DIAGNOSIS] - Component active: {depthOfField.active}");
                
                // Test volume component validity
                var vol = Object.FindFirstObjectByType<Volume>();
                if (vol)
                {
                    Debug.Log($"[DEEP-DIAGNOSIS] Volume: Found, Name: {vol.gameObject.name}, Weight: {vol.weight}, Priority: {vol.priority}");
                    Debug.Log($"[DEEP-DIAGNOSIS] Volume using Profile: {vol.sharedProfile.name}, ID: {vol.sharedProfile.GetInstanceID()}");
                    Debug.Log($"[DEEP-DIAGNOSIS] Our stored Profile ID: {profile.GetInstanceID()}");
                    
                    if (vol.sharedProfile.GetInstanceID() != profile.GetInstanceID())
                    {
                        Debug.LogError("[DEEP-DIAGNOSIS] CRITICAL ISSUE: We're modifying a different profile than what the Volume is using!!!");
                        // Try to fix by using the Volume's actual profile
                        profile = vol.sharedProfile;
                        depthOfField = GetOrAdd<DepthOfField>();
                        if (depthOfField != null)
                        {
                            Debug.Log("[DEEP-DIAGNOSIS] Recovered by switching to Volume's actual profile");
                            // Set this again after the switch
                            depthOfField.focusDistance.overrideState = true;
                            depthOfField.focusDistance.value = distance;
                        }
                    }
                }
                else
                {
                    Debug.LogError("[DEEP-DIAGNOSIS] No Volume found in scene!");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DEEP-DIAGNOSIS] EXCEPTION in UpdateFocusDistance: {e.Message}\n{e.StackTrace}");
        }
    }
}
