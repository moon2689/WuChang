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
float4 _BumpMap_ST;
CBUFFER_END

SurfaceData InitializeStandardLitSurfaceData(float2 uv)
{
    SurfaceData surfaceData = (SurfaceData)0;

    half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);

    float2 uvBumpMap = uv * _BumpMap_ST.xy + _BumpMap_ST.zw;
    float4 normalMap = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uvBumpMap);
    
    surfaceData.albedo = baseMap.r * _BaseColor.rgb;
    surfaceData.alpha = baseMap.r * _BaseColor.a;

    surfaceData.metallic = _Metallic;
    surfaceData.specular = half3(0.0, 0.0, 0.0);

    surfaceData.smoothness = 1 - _Roughness;
    surfaceData.normalTS = UnpackNormal(normalMap);
    surfaceData.occlusion = _Occlusion;
    surfaceData.emission = 0;

    surfaceData.clearCoatMask       = half(0.0);
    surfaceData.clearCoatSmoothness = half(0.0);

    return surfaceData;
}

#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED
