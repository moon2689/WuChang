#ifndef UNIVERSAL_SIMPLE_LIT_INPUT_INCLUDED
#define UNIVERSAL_SIMPLE_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
half4 _BaseColor;
float _ExpandIntensity;
CBUFFER_END


SurfaceData InitializeSimpleLitSurfaceData(float2 uv)
{
    SurfaceData outSurfaceData = (SurfaceData)0;

    half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);

    outSurfaceData.albedo = baseMap.rgb * _BaseColor.rgb;
    outSurfaceData.alpha = baseMap.a * _BaseColor.a;
    outSurfaceData.metallic = 0.0; // unused
    outSurfaceData.specular = 0;
    outSurfaceData.smoothness = 0;
    outSurfaceData.normalTS = 0;
    outSurfaceData.occlusion = 1.0;
    outSurfaceData.emission = 0;
    return outSurfaceData;
}

#endif
