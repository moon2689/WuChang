#ifndef UNIVERSAL_SIMPLE_LIT_INPUT_INCLUDED
#define UNIVERSAL_SIMPLE_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

CBUFFER_START(UnityPerMaterial)
    half4 _BaseColor;
    half4 _SpecColor;
CBUFFER_END

SurfaceData InitializeSimpleLitSurfaceData(float2 uv)
{
    SurfaceData surfaceData = (SurfaceData)0;

    half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
    
    surfaceData.albedo = baseMap.rgb * _BaseColor.rgb;
    surfaceData.alpha = baseMap.a * _BaseColor.a;

    surfaceData.metallic = 0.0; // unused
    surfaceData.specular = _SpecColor.rgb;
    surfaceData.smoothness = _SpecColor.a;
    surfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
    surfaceData.occlusion = 1.0;
    surfaceData.emission = 0;
    return surfaceData;
}

#endif
