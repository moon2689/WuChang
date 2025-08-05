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
half _Metallic;
half _Roughness;
half _Occlusion;
half _FurLen, _FurWindScale, _FurOcclusion, _FurEdgeFade;
half4 _FurNoiseMap_ST, _FurWindMap_ST, _FurDir;
half4 _SheenColor;
CBUFFER_END

TEXTURE2D(_MaskMap);            SAMPLER(sampler_MaskMap);
TEXTURE2D(_FurNoiseMap);        SAMPLER(sampler_FurNoiseMap);
TEXTURE2D(_FurWindMap);         SAMPLER(sampler_FurWindMap);

SurfaceData InitializeStandardLitSurfaceData(float2 uv, half4 vertexColor, out half4 maskMap)
{
    SurfaceData surfaceData = (SurfaceData)0;
    half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
    maskMap = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, uv);
    half4 normalMap = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv);
    half3 normalTS = UnpackNormal(normalMap);
    
    surfaceData.albedo = baseMap.rgb * _BaseColor.rgb;
    //surfaceData.alpha = baseMap.a * _BaseColor.a;

    surfaceData.metallic = _Metallic;
    surfaceData.specular = half3(0.0, 0.0, 0.0);

    surfaceData.smoothness = 1 - maskMap.g * _Roughness;
    surfaceData.normalTS = normalTS;
    //surfaceData.occlusion = maskMap.b;
    surfaceData.emission = 0;

    surfaceData.clearCoatMask       = half(0.0);
    surfaceData.clearCoatSmoothness = half(0.0);

    // fur
    float furLayer = vertexColor.r;
    float furLayerPow2 = furLayer * furLayer;

    float2 uv_furNoise = uv * _FurNoiseMap_ST.xy + _FurDir.xy * furLayerPow2;

    #if _FURWIND_ON
    float4 furWindMap = SAMPLE_TEXTURE2D(_FurWindMap, sampler_FurWindMap, uv * _FurWindMap_ST.xy);
    float2 furWindLen = (furWindMap.xy * 2 - 1) * furLayer * _FurWindScale;
    uv_furNoise += furWindLen;
    #endif
    
    float4 furNoiseMap = SAMPLE_TEXTURE2D(_FurNoiseMap, sampler_FurNoiseMap, uv_furNoise);
    float furAlpha = furLayer < 0.01 ? 1 : max(furNoiseMap.r * 2 - furLayerPow2 * _FurEdgeFade, 0);

    /*
    half furMask = 1 - surfaceData.metallic;
    half furAlpha;
    if (furLayer < 0.01)
    {
        furAlpha = 1;
    }
    else if(furMask > 0.5)
    {
        //furAlpha = furNoiseMap.r * (1 - furLayer); //最里层完全不透明
        furAlpha = furNoiseMap.r * 2 - furLayerPow2 * _FurEdgeFade;
    }
    else
    {
        furAlpha = 0;
    }
    */

    //clip(furAlpha - 0.8);

    float occlusion = lerp(_FurOcclusion, 1, furLayer);
    surfaceData.occlusion = occlusion * _Occlusion;
    surfaceData.alpha = furAlpha;

    return surfaceData;
}

#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED
