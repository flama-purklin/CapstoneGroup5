using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class QualityManager : MonoBehaviour
{
    [Header("Performance Settings")]
    [SerializeField] private bool useQualityLevelChange = false;  // Disabled by default as it's very noticeable
    [SerializeField] private int highQualityLevel = 5;
    [SerializeField] private int chatQualityLevel = 2;

    [Header("HDRP Volume Settings")]
    public Volume postProcessVolume;

    private VolumeProfile originalProfile;
    private VolumeProfile chatProfile;
    private HDRenderPipelineAsset originalPipelineAsset;
    private HDRenderPipelineAsset chatPipelineAsset;

    // Store original quality settings
    private int originalShadowResolution;
    private int originalVolumetricSteps;
    private float originalLODBias;

    private void Awake()
    {
        if (!Application.isPlaying) return;

        // Store original quality settings
        originalPipelineAsset = QualitySettings.renderPipeline as HDRenderPipelineAsset;
        originalLODBias = QualitySettings.lodBias;

        if (postProcessVolume != null)
        {
            originalProfile = postProcessVolume.profile;
            chatProfile = Instantiate(originalProfile);
            ConfigureChatProfile(chatProfile);
        }

        if (originalPipelineAsset != null)
        {
            chatPipelineAsset = Instantiate(originalPipelineAsset);
            StoreOriginalSettings();
            ConfigureChatPipelineAsset(chatPipelineAsset);
        }
    }

    private void StoreOriginalSettings()
    {
        var settings = originalPipelineAsset.currentPlatformRenderPipelineSettings;
        originalShadowResolution = settings.hdShadowInitParams.maxShadowRequests;
        // Store other original settings as needed
    }

    private void ConfigureChatProfile(VolumeProfile profile)
    {
        // Only modify subtle post-processing settings
        if (profile.TryGet<ScreenSpaceAmbientOcclusion>(out var ssao))
        {
            ssao.intensity.value *= 0.5f;  // Reduce SSAO intensity instead of disabling
        }

        if (profile.TryGet<Fog>(out var fog))
        {
            // Reduce fog quality but keep the visual effect
            fog.quality.value = 0;  // Lower quality but same appearance
        }

        // Don't modify visually important effects like DoF, SSR, or contact shadows
    }

    private void ConfigureChatPipelineAsset(HDRenderPipelineAsset asset)
    {
        var globalSettings = asset.currentPlatformRenderPipelineSettings;

        // Subtle optimizations that don't affect visual quality dramatically
        globalSettings.hdShadowInitParams.maxShadowRequests =
            Mathf.Max(globalSettings.hdShadowInitParams.maxShadowRequests / 2, 32);  // Reduce but don't eliminate shadows



        // Don't disable major features like SSR, SSAO, or volumetrics
        // Instead, reduce their quality slightly if needed

        asset.currentPlatformRenderPipelineSettings = globalSettings;
    }

    public void EnterChatMode()
    {
        if (!Application.isPlaying) return;

        // Only change quality level if explicitly enabled
        if (useQualityLevelChange)
        {
            QualitySettings.SetQualityLevel(chatQualityLevel, true);
        }

        // Reduce LOD bias slightly (makes objects use lower detail models a bit sooner)
        QualitySettings.lodBias = originalLODBias * 0.8f;

        if (postProcessVolume != null && chatProfile != null)
        {
            postProcessVolume.profile = chatProfile;
        }

        if (chatPipelineAsset != null)
        {
            QualitySettings.renderPipeline = chatPipelineAsset;
        }
    }

    public void ExitChatMode()
    {
        if (!Application.isPlaying) return;

        // Restore quality level if it was changed
        if (useQualityLevelChange)
        {
            QualitySettings.SetQualityLevel(highQualityLevel, true);
        }

        // Restore original LOD bias
        QualitySettings.lodBias = originalLODBias;

        if (postProcessVolume != null && originalProfile != null)
        {
            postProcessVolume.profile = originalProfile;
        }

        if (originalPipelineAsset != null)
        {
            QualitySettings.renderPipeline = originalPipelineAsset;
        }
    }

    private void OnDestroy()
    {
        ExitChatMode();
    }
}