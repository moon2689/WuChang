#ifndef UNIVERSAL_LIT_INPUT_INCLUDED
#define UNIVERSAL_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ParallaxMapping.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"


// NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
CBUFFER_START(UnityPerMaterial)
half4 _BaseColor;
half _Cutoff;
half _EmissionIntensity;

half _DissolveWeight;
half4 _DissolveNoiseMap_ST;
half _DissolveEdgeWidth;
half4 _DissolveEdgeColor;
CBUFFER_END

TEXTURE2D(_MaskMROMap);         SAMPLER(sampler_MaskMROMap);
TEXTURE2D(_DissolveNoiseMap);   SAMPLER(sampler_DissolveNoiseMap);


SurfaceData InitializeStandardLitSurfaceData(float2 uv)
{
    SurfaceData surfaceData = (SurfaceData)0;
    
    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    surfaceData.alpha = albedoAlpha.a;
    #if _ALPHATEST_ON
    clip(albedoAlpha.a - _Cutoff);
    #endif

    surfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;

    half4 maskMRO = SAMPLE_TEXTURE2D(_MaskMROMap, sampler_MaskMROMap, uv);
    surfaceData.metallic = maskMRO.b;
    surfaceData.smoothness = maskMRO.r;
    surfaceData.occlusion = maskMRO.g;

    surfaceData.specular = 0;

    surfaceData.normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv));
    #if _EMISSION_ON
    surfaceData.emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, uv).rgb * _EmissionIntensity;
    #endif

    // 溶解
    #if _DISSOLVE_ON
    float2 uvDissolve = uv * _DissolveNoiseMap_ST.xy + _DissolveNoiseMap_ST.zw;
    half4 dissolveNoiseMap = SAMPLE_TEXTURE2D(_DissolveNoiseMap, sampler_DissolveNoiseMap, uvDissolve);
    clip(dissolveNoiseMap.r - _DissolveWeight);
    half edgeRatio = step(_DissolveWeight, dissolveNoiseMap.r) - step(_DissolveWeight, dissolveNoiseMap.r - _DissolveEdgeWidth);
    surfaceData.albedo = lerp(surfaceData.albedo, _DissolveEdgeColor.rgb, edgeRatio);
    #endif

    //surfaceData.emission = maskMRO.aaa;

    return surfaceData;
}

#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED
