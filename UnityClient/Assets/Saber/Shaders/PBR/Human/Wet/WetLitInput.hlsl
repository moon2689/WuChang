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
half _Smoothness;
CBUFFER_END

SurfaceData InitializeStandardLitSurfaceData(float2 uv)
{
    half4 normalMap = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv);
    half3 normalTS = UnpackNormal(normalMap);
    
    SurfaceData surfaceData = (SurfaceData)0;
    surfaceData.albedo = _BaseColor.rgb;
    surfaceData.alpha = _BaseColor.a;
    surfaceData.metallic = 0;
    surfaceData.specular = half3(0.0, 0.0, 0.0);
    surfaceData.smoothness = _Smoothness;
    surfaceData.normalTS = normalTS;
    surfaceData.occlusion = half(1.0);
    surfaceData.emission = 0;
    surfaceData.clearCoatMask       = half(0.0);
    surfaceData.clearCoatSmoothness = half(0.0);
    return surfaceData;
}

#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED
