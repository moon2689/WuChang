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
half _Anisotropic;
half4 _AnisotropicColor;
half _NormalScale;
half4 _BumpMap_ST;
CBUFFER_END

TEXTURE2D(_MaskMap);            SAMPLER(sampler_MaskMap);

SurfaceData InitializeStandardLitSurfaceData(float2 uv)
{
    SurfaceData outSurfaceData = (SurfaceData)0;

    half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
    half4 maskMap = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, uv);
    half4 normalMap = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv * _BumpMap_ST.xy + _BumpMap_ST.zw);
    half3 normalTS = UnpackNormal(normalMap);

    outSurfaceData.albedo = baseMap.rgb * _BaseColor.rgb;
    outSurfaceData.alpha = baseMap.a * _BaseColor.a;

    outSurfaceData.metallic = maskMap.r * _Metallic;
    outSurfaceData.specular = half3(0.0, 0.0, 0.0);

    outSurfaceData.smoothness = 1 - maskMap.g * _Roughness;
    outSurfaceData.normalTS = normalTS;
    outSurfaceData.occlusion = maskMap.b * _Occlusion;
    outSurfaceData.emission = 0;

    outSurfaceData.clearCoatMask       = half(0.0);
    outSurfaceData.clearCoatSmoothness = half(0.0);

    return outSurfaceData;
}

#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED
