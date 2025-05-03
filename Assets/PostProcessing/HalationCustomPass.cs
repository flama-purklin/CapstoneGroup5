// Assets/PostProcessing/HalationCustomPass.cs
//-----------------------------------------------------------------------
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class HalationCustomPass : CustomPass
{
    const string PROFILER = "HalationPass";
    readonly Material mat;

    // tweakables
    [Range(0, 2)] public float intensity = 0.4f;
    [ColorUsage(false, true)] public Color tint = new(1f, .5f, .3f);
    [Range(0, 1)] public float threshold = 0.9f;
    [Range(0, 1)] public float blurSigma = 0.2f;

    public HalationCustomPass(Shader shader)
    {
        if (!shader)
        {
            Debug.LogError("Halation shader is missing! Please assign it in the Filmic60sSetup component.");
            return; // Exit early without setting up the material
        }
        mat = CoreUtils.CreateEngineMaterial(shader);
        name = PROFILER;
        targetColorBuffer = TargetBuffer.Camera;
    }

    protected override void Execute(CustomPassContext ctx)
    {
        if (!mat) return;

        mat.SetFloat("_Threshold", threshold);
        mat.SetFloat("_Sigma", blurSigma);
        mat.SetFloat("_Intensity", intensity);
        mat.SetColor("_Tint", tint);

        CoreUtils.DrawFullScreen(ctx.cmd, mat, ctx.cameraColorBuffer);
    }
}
