using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

[Serializable, VolumeComponentMenu("Post-process/VintageFilm")]
public sealed class VintageFilm : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Halation threshold (linear HDR)")] public ClampedFloatParameter threshold = new(8f, 0f, 20f);
    [Tooltip("Halation spread (pixels @1080p)")] public ClampedFloatParameter radius    = new(6f, 1f, 20f);
    [Tooltip("Overall intensity")]             public ClampedFloatParameter intensity = new(0.75f, 0f, 2f);
    [Tooltip("Gate-weave amplitude (pixels)")]public ClampedFloatParameter weave     = new(0.5f, 0f, 2f);
    [Tooltip("Dust texture (RGBA)")]           public TextureParameter        dustTex   = new(null);
    [Tooltip("Dust strength")]                 public ClampedFloatParameter dust      = new(0.4f, 0f, 1f);

    static readonly int _Params   = Shader.PropertyToID("_VintageParams");
    static readonly int _DustTex  = Shader.PropertyToID("_DustTex");
    static readonly int _JitterID = Shader.PropertyToID("_Jitter");
    Material m_Mat;

    public bool IsActive() => m_Mat != null && intensity.value > 0f;

    public override void Setup()
    {
        var shader = Shader.Find("Hidden/Custom/VintageFilm");
        if (shader != null)
            m_Mat = new Material(shader);
    }

    public override void Render(CommandBuffer cmd, HDCamera cam, RTHandle src, RTHandle dst)
    {
        if (m_Mat == null)
        {
            HDUtils.BlitCameraTexture(cmd, src, dst);
            return;
        }

        // corrected gate-weave jitter:
        Vector2 jitter = weave.value * ((new Vector2(UnityEngine.Random.value, UnityEngine.Random.value) - new Vector2(0.5f, 0.5f)) * 2f);


        m_Mat.SetVector(_Params, new Vector4(threshold.value, radius.value, intensity.value, Time.time));
        m_Mat.SetTexture(_DustTex, dustTex.value);
        m_Mat.SetVector(_JitterID, jitter);

        // apply jitter via UV offset in the blit
        cmd.SetGlobalVector("_BlitScaleBias", new Vector4(1, 1, jitter.x / Screen.width, jitter.y / Screen.height));
        HDUtils.DrawFullScreen(cmd, m_Mat, dst, shaderPassId: 0);
    }

    public override void Cleanup()
    {
        CoreUtils.Destroy(m_Mat);
    }
}
