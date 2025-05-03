// Assets/PostProcessing/FilmicHalation.shader
// Wrapper Shader for Filmic Halation HLSL
Shader "Hidden/Filmic/Halation" // Keep the original name for consistency
{
    Properties
    {
        // Properties set by HalationCustomPass.cs
        _Threshold ("Threshold", Range(0, 1)) = 0.9
        _Sigma ("Blur Sigma", Range(0, 1)) = 0.2
        _Intensity ("Intensity", Range(0, 2)) = 0.4
        [HDR] _Tint ("Tint", Color) = (1, 0.5, 0.3, 1) // HDR Color

        // Texture implicitly used by the pass
        _CameraColorTexture ("Camera Color Texture", 2D) = "white" {}
    }

    SubShader
    {
        // Tags necessary for HDRP Custom Pass
        Tags { "RenderPipeline"="HighDefinitionRenderPipeline" }

        Pass
        {
            Name "Halation"
            ZTest Always Cull Off ZWrite Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            TEXTURE2D_X(_CameraColorTexture); SAMPLER(sampler_CameraColorTexture);
            float _Threshold, _Sigma, _Intensity; float3 _Tint;

            float4 Vert(float4 pos:POSITION) : SV_Position { return pos; }

            float4 Frag(float4 input : SV_Position) : SV_Target
            {
                float2 uv = input.xy / _ScreenSize.xy;
                float3 col = SAMPLE_TEXTURE2D_X(_CameraColorTexture, sampler_CameraColorTexture, uv).rgb;
                float lum = dot(col, float3(0.3,0.59,0.11));
                float mask = saturate((lum - _Threshold) / max(_Sigma,1e-4));
                float3 halo = col * mask * _Tint * _Intensity;
                return float4(col + halo, 1);
            }
            ENDHLSL
        }
    }
    Fallback Off // No fallback needed
}
