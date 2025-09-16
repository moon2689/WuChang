#ifndef UNIVERSAL_LIT_INPUT_INCLUDED
#define UNIVERSAL_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ParallaxMapping.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"


// NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
CBUFFER_START(UnityPerMaterial)
half4 _BaseColor;
float _IrisRadius;
CBUFFER_END

//_ScleraMap
TEXTURE2D(_ScleraMap);          SAMPLER(sampler_ScleraMap);
TEXTURE2D(_IrisMap);            SAMPLER(sampler_IrisMap);

half3 SampleNormalCommonLit(float2 uv, TEXTURE2D_PARAM(bumpMap, sampler_bumpMap))
{
    half4 n = SAMPLE_TEXTURE2D(bumpMap, sampler_bumpMap, uv);
    return UnpackNormal(n);
}

SurfaceData InitializeStandardLitSurfaceData(float2 uv)
{
    SurfaceData surfaceData = (SurfaceData)0;
    
    half4 scleraMap = SAMPLE_TEXTURE2D(_ScleraMap, sampler_ScleraMap, uv);

    float2 uvIris = (uv - float2(0.5, 0.5)) / _IrisRadius * 0.5 + float2(0.5, 0.5);
    half4 irisMap = SAMPLE_TEXTURE2D(_IrisMap, sampler_IrisMap, uvIris);
    
    surfaceData.alpha = 1;
    surfaceData.albedo = lerp(scleraMap.rgb, irisMap.rgb, irisMap.a);
    //surfaceData.albedo=irisMap.a;

    surfaceData.metallic = 0;
    surfaceData.smoothness = 1;
    surfaceData.occlusion = 1;

    surfaceData.specular = 0;

    surfaceData.normalTS = half3(0,0,1);
    surfaceData.emission = 0;

    return surfaceData;
}

#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED
