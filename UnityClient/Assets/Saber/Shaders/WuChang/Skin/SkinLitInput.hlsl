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
half4 _SSSColor;
half _DetailNormalMapScale;
float4 _DetailNormalMap_ST;
float _NormalMapMipCount;
CBUFFER_END

TEXTURE2D(_ParallaxMap);        SAMPLER(sampler_ParallaxMap);
TEXTURE2D(_MaskMap);            SAMPLER(sampler_MaskMap);
TEXTURE2D(_PreIntegratedSSSMap);SAMPLER(sampler_PreIntegratedSSSMap);
TEXTURE2D(_DetailNormalMap);    SAMPLER(sampler_DetailNormalMap);


SurfaceData InitializeStandardLitSurfaceData(float2 uv)
{
    SurfaceData surfaceData = (SurfaceData)0;
    
    half4 maskMap = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, uv);
    half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
    half4 normalMap = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv);
    half3 normalTS = UnpackNormal(normalMap);
    
    surfaceData.albedo = baseMap.rgb * _BaseColor.rgb;
    surfaceData.alpha = baseMap.a * _BaseColor.a;
    surfaceData.metallic = half(0.0);
    surfaceData.specular = half3(0.0, 0.0, 0.0);
    surfaceData.smoothness = 1 - maskMap.r;
    //surfaceData.normalTS = normalTS;
    surfaceData.occlusion = 1;;
    surfaceData.emission = half(0.0);
    surfaceData.clearCoatMask = half(0.0);
    surfaceData.clearCoatSmoothness = half(0.0);

    // normal
#if _DETAILNORMAL_ON
    float2 detailNormalUv = uv * _DetailNormalMap_ST.xy + _DetailNormalMap_ST.zw;
    half3 detailNormalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_DetailNormalMap, sampler_DetailNormalMap, detailNormalUv), _DetailNormalMapScale);
    detailNormalTS = normalize(detailNormalTS);
    surfaceData.normalTS = lerp(normalTS, BlendNormalRNM(normalTS, detailNormalTS), maskMap.r);
#else
    surfaceData.normalTS = normalTS;
#endif

    return surfaceData;
}

#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED
