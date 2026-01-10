#ifndef UNIVERSAL_LIT_INPUT_INCLUDED
#define UNIVERSAL_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ParallaxMapping.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"

// NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
CBUFFER_START(UnityPerMaterial)
float _DissolveAmount;
float _DissolveAmountOffset;
float _DissolveAmountSpread;
float _EdgeWidth;
float3 _DissolveNoiseScale;
half4 _EdgeColor;
CBUFFER_END

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);
TEXTURE2D(_BumpMap);
SAMPLER(sampler_BumpMap);
TEXTURE2D(_MaskMap);
SAMPLER(sampler_MaskMap);


SurfaceData InitializeStandardLitSurfaceData(float2 uv)
{
    SurfaceData surfaceData = (SurfaceData)0;
    
    half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
    half4 normalMap = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv);
    half4 maskMap = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, uv);
    
    float3 normalTS = UnpackNormal(normalMap);
    
    surfaceData.albedo = baseMap.rgb;
    surfaceData.metallic = maskMap.r;
    surfaceData.specular = half3(0.0, 0.0, 0.0);
    surfaceData.smoothness = 1 - maskMap.g;
    surfaceData.normalTS = normalTS;
    surfaceData.occlusion = maskMap.b;
    surfaceData.emission = 0;
    return surfaceData;
}

#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED