#ifndef UNIVERSAL_SIMPLE_LIT_INPUT_INCLUDED
#define UNIVERSAL_SIMPLE_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
half4 _BaseColor;
half4 _SpecColor;
half4 _EmissionColor;
half _Cutoff;
half _Surface;

half _DissolveWeight;
half4 _DissolveNoiseMap_ST;
half _DissolveEdgeWidth;
half4 _DissolveEdgeColor;
CBUFFER_END

TEXTURE2D(_SpecGlossMap);       SAMPLER(sampler_SpecGlossMap);
TEXTURE2D(_DissolveNoiseMap);   SAMPLER(sampler_DissolveNoiseMap);

half4 SampleSpecularSmoothness(float2 uv, half alpha, half4 specColor, TEXTURE2D_PARAM(specMap, sampler_specMap))
{
    half4 specularSmoothness = half4(0, 0, 0, 1);
#ifdef _SPECGLOSSMAP
    specularSmoothness = SAMPLE_TEXTURE2D(specMap, sampler_specMap, uv) * specColor;
#elif defined(_SPECULAR_COLOR)
    specularSmoothness = specColor;
#endif

#ifdef _GLOSSINESS_FROM_BASE_ALPHA
    specularSmoothness.a = alpha;
#endif

    return specularSmoothness;
}

SurfaceData InitializeSimpleLitSurfaceData(float2 uv)
{
    SurfaceData surfaceData = (SurfaceData)0;

    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    surfaceData.alpha = albedoAlpha.a * _BaseColor.a;
    //surfaceData.alpha = AlphaDiscard(surfaceData.alpha, _Cutoff);

    surfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;
    surfaceData.albedo = AlphaModulate(surfaceData.albedo, surfaceData.alpha);

    half4 specularSmoothness = SampleSpecularSmoothness(uv, surfaceData.alpha, _SpecColor, TEXTURE2D_ARGS(_SpecGlossMap, sampler_SpecGlossMap));
    surfaceData.metallic = 0.0; // unused
    surfaceData.specular = specularSmoothness.rgb;
    surfaceData.smoothness = specularSmoothness.a;
    surfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
    surfaceData.occlusion = 1.0;
    surfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));

    // 溶解
    #if _DISSOLVE_ON
    float2 uvDissolve = uv * _DissolveNoiseMap_ST.xy + _DissolveNoiseMap_ST.zw;
    half4 dissolveNoiseMap = SAMPLE_TEXTURE2D(_DissolveNoiseMap, sampler_DissolveNoiseMap, uvDissolve);
    clip(dissolveNoiseMap.r - _DissolveWeight);
    half edgeRatio = step(_DissolveWeight, dissolveNoiseMap.r) - step(_DissolveWeight, dissolveNoiseMap.r - _DissolveEdgeWidth);
    surfaceData.albedo = lerp(surfaceData.albedo, _DissolveEdgeColor.rgb, edgeRatio);
    #endif
    return surfaceData;
}

#endif
