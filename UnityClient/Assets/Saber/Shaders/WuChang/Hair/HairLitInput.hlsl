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

float4 _AnosioMap_ST;

half _SpecularShininess1;
half4 _SpecularTint1;
half _SpecularShiftOffset1;
CBUFFER_END

TEXTURE2D(_OpacityMap);         SAMPLER(sampler_OpacityMap);
TEXTURE2D(_AnosioMap);          SAMPLER(sampler_AnosioMap);
TEXTURE2D(_FlowMap);            SAMPLER(sampler_FlowMap);


half3 SampleNormalCommonLit(float2 uv, TEXTURE2D_PARAM(bumpMap, sampler_bumpMap))
{
    half4 n = SAMPLE_TEXTURE2D(bumpMap, sampler_bumpMap, uv);
    return UnpackNormal(n);
}

SurfaceData InitializeStandardLitSurfaceData(float2 uv)
{
    SurfaceData surfaceData = (SurfaceData)0;
    
    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    half4 opacityMap = SAMPLE_TEXTURE2D(_OpacityMap, sampler_OpacityMap, uv);
    
    surfaceData.alpha = opacityMap.r;

    surfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;
    surfaceData.metallic = 0;
    surfaceData.smoothness = 0.5;
    surfaceData.occlusion = 1;

    surfaceData.specular = 0;

    surfaceData.normalTS = half3(0,0,1);
    surfaceData.emission = 0;

    return surfaceData;
}

#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED
