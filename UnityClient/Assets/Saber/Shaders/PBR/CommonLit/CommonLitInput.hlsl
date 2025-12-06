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
CBUFFER_END

TEXTURE2D(_MaskMROMap);         SAMPLER(sampler_MaskMROMap);


half3 SampleNormalCommonLit(float2 uv, TEXTURE2D_PARAM(bumpMap, sampler_bumpMap))
{
    half4 n = SAMPLE_TEXTURE2D(bumpMap, sampler_bumpMap, uv);
    return UnpackNormal(n);
}

inline void InitializeStandardLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
{
    outSurfaceData = (SurfaceData)0;
    
    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    outSurfaceData.alpha = albedoAlpha.a;
    #if _ALPHATEST_ON
    clip(albedoAlpha.a - _Cutoff);
    #endif

    outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;

    half4 maskMRO = SAMPLE_TEXTURE2D(_MaskMROMap, sampler_MaskMROMap, uv);
    outSurfaceData.metallic = maskMRO.r;
    outSurfaceData.smoothness = (1 - maskMRO.g);
    outSurfaceData.occlusion = maskMRO.b;

    outSurfaceData.specular = 0;

    outSurfaceData.normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv));
    #if _EMISSION_ON
    outSurfaceData.emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, uv).rgb;
    #endif
}

#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED
