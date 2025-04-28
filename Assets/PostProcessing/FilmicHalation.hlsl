Shader "Hidden/Filmic/Halation"
{
    Properties {}
    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    TEXTURE2D_X(_CameraColorTexture); SAMPLER(sampler_CameraColorTexture);
    float _Threshold, _Sigma, _Intensity; float3 _Tint;
    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "Halation"
            ZTest Always Cull Off ZWrite Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            float4 Vert(float4 pos:POSITION) : SV_Position { return pos; }

            float4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.positionCS.xy / _ScreenSize.xy;
                float3 col = SAMPLE_TEXTURE2D_X(_CameraColorTexture, sampler_CameraColorTexture, uv).rgb;
                float lum = dot(col, float3(0.3,0.59,0.11));
                float mask = saturate((lum - _Threshold) / max(_Sigma,1e-4));
                float3 halo = col * mask * _Tint * _Intensity;
                return float4(col + halo, 1);
            }
            ENDHLSL
        }
    }
}
