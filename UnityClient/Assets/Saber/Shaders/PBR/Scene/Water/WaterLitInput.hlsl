#ifndef UNIVERSAL_LIT_INPUT_INCLUDED
#define UNIVERSAL_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ParallaxMapping.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

// NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
CBUFFER_START(UnityPerMaterial)
half4 _ShallowColor;
half4 _DepthColor;
half _Smoothness;
float4 _BumpMap_ST;
float4 _BumpMapFlow2;
float _BumpScale;
float _DepthStart;
float _DepthEnd;
half4 _CameraOpaqueTexture_TexelSize;
half4 _CameraDepthTexture_TexelSize;
float _Distortion;
float _ReflectionDistortion;
float _EdgeSize;

half4 _ScatteringColor;
float _ScatteringNormalScale;
float _ScatteringPower;
float _ScatteringIntensity;

float4 _WaveRTRect;
float4 _CausticsMap_ST;
float4 _CausticsMapFlow2;
float _CausticsFadeDepth;
half _CausticsIntensity;
CBUFFER_END

TEXTURE2D(_WaveRT);             SAMPLER(sampler_WaveRT);
TEXTURE2D(_CausticsMap);        SAMPLER(sampler_CausticsMap);


struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS               : SV_POSITION;
    float2 uv                       : TEXCOORD0;
    float3 positionWS               : TEXCOORD1;
    float3 normalWS                 : TEXCOORD2;
    half4 tangentWS                 : TEXCOORD3; // xyz: tangent, w: sign
    half4 someFactor                : TEXCOORD4; // x:fogFactor,y:pixelDepth(distance to camera)
    half3 vertexSH                  : TEXCOORD5;
    //float4 screenPos                : TEXCOORD6;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct WaterBRDFData
{
    float3 normalWSMesh;
    float2 screenUV;
    float pixelDepth;
    half3 causticsColor;
};


SurfaceData InitializeStandardLitSurfaceData(Varyings input, out WaterBRDFData waterBRDF)
{
    SurfaceData surfaceData = (SurfaceData)0;
    
    waterBRDF = (WaterBRDFData)0;
    //float2 screenUV = screenPos.xy / screenPos.w;
    float2 screenUV = GetNormalizedScreenSpaceUV(input.positionCS);
    waterBRDF.normalWSMesh = input.normalWS;
    waterBRDF.screenUV = screenUV;
    waterBRDF.pixelDepth = input.someFactor.y;
    
    // normal
    float2 uv = input.uv;
    float2 uvBump = uv * _BumpMap_ST.xy + _BumpMap_ST.zw * _Time.x;
    half4 bumpMap = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uvBump);
    float2 uvBump2 = uv * _BumpMapFlow2.xy + _BumpMapFlow2.zw * _Time.x;
    half4 bumpMap2 = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uvBump2);
    half3 normalTS1 = UnpackNormalScale(bumpMap, _BumpScale);
    half3 normalTS2 = UnpackNormalScale(bumpMap2, _BumpScale);
    half3 blendNormalTS = BlendNormal(normalTS1, normalTS2);
    //blendNormalTS = half3(0,0,1);

    // wave
    #if _WAVE_ON
    float2 uvWave = (uv - _WaveRTRect.xy) / _WaveRTRect.zw;
    if (uvWave.x>=0&&uvWave.x<=1&&uvWave.y>=0&&uvWave.y<=1)
    {
        half4 waveRTMap = SAMPLE_TEXTURE2D(_WaveRT, sampler_WaveRT, uvWave);
        waveRTMap = pow(waveRTMap, 0.45); //伽马校正
        half3 waveNormal = half3(waveRTMap.rg * 2 - 1, 0);
        waveNormal.z = sqrt(1 - saturate(waveNormal.x * waveNormal.x + waveNormal.y * waveNormal.y)); //重构z分量，x*x+y*y+z*z=1
        waveNormal = normalize(waveNormal);
        blendNormalTS = BlendNormal(blendNormalTS, waveNormal);
    }
    #endif

    // depth
    //float3 clearColor = SampleSceneColor(screenUV);
    float rawDepth = SampleSceneDepth(screenUV);
    //float depth01 = Linear01Depth(rawDepth, _ZBufferParams);
    float depth = LinearEyeDepth(rawDepth, _ZBufferParams);
    float disToWaterSurface = max(depth - waterBRDF.pixelDepth, 0);

    float2 uvDistortionOffset = blendNormalTS.xy * _Distortion;
    float2 screenUVDepth = screenUV + uvDistortionOffset * _CameraDepthTexture_TexelSize.xy;
    float rawDepthDistortion = SampleSceneDepth(screenUVDepth);
    float depthDistortion = LinearEyeDepth(rawDepthDistortion, _ZBufferParams);
    float depthRate = saturate((depthDistortion - _DepthStart) / (_DepthEnd - _DepthStart));
    
    // water albedo
    float2 screenUVOpaque = screenUV + uvDistortionOffset * _CameraOpaqueTexture_TexelSize.xy;
    float3 sceneColorDistortion = SampleSceneColor(screenUVOpaque);
    half3 shallowColor = sceneColorDistortion.rgb * lerp(1, _ShallowColor.rgb, _ShallowColor.a);
    half3 depthColor = lerp(sceneColorDistortion, _DepthColor.rgb, _DepthColor.a);
    half3 albedo = lerp(shallowColor, depthColor, depthRate);

    // edge fade
    float alpha = saturate(disToWaterSurface / _EdgeSize);

    // caustics
    #if _CAUSTICS_ON
    float3 worldUV = ComputeWorldSpacePosition(screenUVOpaque, rawDepthDistortion, UNITY_MATRIX_I_VP);
    float2 uvCaustics1 = worldUV.xz * _CausticsMap_ST.xy + _CausticsMap_ST.zw * _Time.x;
    float2 uvCaustics2 = worldUV.xz * _CausticsMapFlow2.xy + _CausticsMapFlow2.zw * _Time.x;
    half4 causticsMap1 = SAMPLE_TEXTURE2D(_CausticsMap, sampler_CausticsMap, uvCaustics1);
    half4 causticsMap2 = SAMPLE_TEXTURE2D(_CausticsMap, sampler_CausticsMap, uvCaustics2);
    half3 causticsMin = min(causticsMap1.rgb, causticsMap2.rgb);
    float disToWaterSurfaceDistortion = max(depthDistortion - waterBRDF.pixelDepth, 0);
    float causticsDepthRate = 1 - saturate(disToWaterSurfaceDistortion / _CausticsFadeDepth);
    waterBRDF.causticsColor = causticsMin * causticsDepthRate * _CausticsIntensity;
    #endif

    surfaceData.alpha = alpha;
    surfaceData.albedo = albedo;
    surfaceData.metallic = 0;
    surfaceData.smoothness = _Smoothness;
    surfaceData.occlusion = 1;
    surfaceData.specular = 0;
    surfaceData.normalTS = blendNormalTS;
    return surfaceData;
}

#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED
