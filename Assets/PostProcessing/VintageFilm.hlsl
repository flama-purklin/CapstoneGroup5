#pragma fragment frag
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
TEXTURE2D_X(_InputTexture); SAMPLER(sampler_InputTexture);
TEXTURE2D(_DustTex);         SAMPLER(sampler_DustTex);
float4  _VintageParams; // x=threshold, y=radius, z=intensity, w=time
float2  _Jitter;        // set from C#

float4 frag (Varyings i) : SV_Target
{
    float2 uv = i.texcoord.xy - _Jitter;          // gate-weave shift
    float3 col = SAMPLE_TEXTURE2D_X(_InputTexture, sampler_InputTexture, uv).rgb;

    // isolate bright pixels
    float luma = Luminance(col);
    float mask = saturate((luma - _VintageParams.x) / _VintageParams.x);

    // cheap radial blur – 4 taps
    float3 halo = 0;
    [unroll] for (int t = 0; t < 4; t++)
    {
        float2 dir = float2(cos(t*PI/2), sin(t*PI/2));
        halo += SAMPLE_TEXTURE2D_X(_InputTexture, sampler_InputTexture,
            uv + dir * _VintageParams.y / _ScreenParams.xy).rgb;
    }
    halo = (halo / 4) * float3(1.1,0.6,0.2);      // warm tint

    // composite
    col += halo * mask * _VintageParams.z;

    // subtle exposure flicker (±1 %)
    col *= 1.0 + (sin(_VintageParams.w*43.3)*0.01);

    // dust overlay (scrolling)
    if (_DustTex.IsValid())
    {
        float2 duv = frac(uv*1.02 + _VintageParams.w*float2(0.05,0.03));
        float a = SAMPLE_TEXTURE2D(_DustTex,sampler_DustTex, duv).r;
        col = lerp(col, col + a, saturate(_VintageParams.z*0.5));
    }

    return float4(col,1);
}
