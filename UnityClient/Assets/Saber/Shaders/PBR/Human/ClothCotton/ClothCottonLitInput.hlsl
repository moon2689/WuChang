#ifndef UNIVERSAL_LIT_INPUT_INCLUDED
#define UNIVERSAL_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ParallaxMapping.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"

#if defined(_DETAIL_MULX2) || defined(_DETAIL_SCALED)
#define _DETAIL
#endif

// NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
half4 _BaseColor;
half _Metallic;
half _Roughness;
half _Occlusion;
half4 _SheenColor;
float4 _FuzzMap_ST;
CBUFFER_END

TEXTURE2D(_MaskMap);            SAMPLER(sampler_MaskMap);
TEXTURE2D(_FuzzMap);            SAMPLER(sampler_FuzzMap);

SurfaceData InitializeStandardLitSurfaceData(float2 uv, out half4 maskMap)
{
    SurfaceData surfaceData = (SurfaceData)0;
    half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
    maskMap = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, uv);
    half4 normalMap = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv);
    half3 normalTS = UnpackNormal(normalMap);
    
    surfaceData.albedo = baseMap.rgb * _BaseColor.rgb;
    surfaceData.alpha = baseMap.a * _BaseColor.a;
    surfaceData.albedo = AlphaModulate(surfaceData.albedo, surfaceData.alpha);

    surfaceData.metallic = _Metallic;
    surfaceData.specular = half3(0.0, 0.0, 0.0);

    surfaceData.smoothness = 1 - maskMap.g * _Roughness;
    surfaceData.normalTS = normalTS;
    surfaceData.occlusion = maskMap.b * _Occlusion;
    surfaceData.emission = 0;

    surfaceData.clearCoatMask       = half(0.0);
    surfaceData.clearCoatSmoothness = half(0.0);

    return surfaceData;
}

#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED
