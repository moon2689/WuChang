#ifndef UNIVERSAL_LIT_INPUT_INCLUDED
#define UNIVERSAL_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ParallaxMapping.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"

// NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
CBUFFER_START(UnityPerMaterial)
half4 _BaseColor;
half _Cutoff;
half _Smoothness;
half _BumpScale;
half _OcclusionStrength;


half _VertexOcclusionStrength;

half4 _SpecularTint;
half4 _SecondarySpecularTint;
half _SecondarySmoothness;
half _SpecularShift;
half _SecondarySpecularShift;
half4 _Transmittance;

half _ReflectanceStrength;
half _JitterStrength;
CBUFFER_END


TEXTURE2D(_MaskMap);            SAMPLER(sampler_MaskMap);
TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);
TEXTURE2D(_BumpMap);            SAMPLER(sampler_BumpMap);

SurfaceData InitializeStandardLitSurfaceData(float2 uv, float vertexOcclusion, half4 maskMap)
{
    SurfaceData surfaceData = (SurfaceData)0;
    
    half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
    half4 bumpMap = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv);
    
    surfaceData.albedo = baseMap.rgb * _BaseColor.rgb;
    surfaceData.alpha = baseMap.a * _BaseColor.a;
    surfaceData.smoothness = _Smoothness * (1 - maskMap.r);
    surfaceData.normalTS = UnpackNormalScale(bumpMap, _BumpScale);
    surfaceData.occlusion = min(maskMap.b * _OcclusionStrength, LerpWhiteTo(vertexOcclusion, _VertexOcclusionStrength));
    return surfaceData;
}

#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED
