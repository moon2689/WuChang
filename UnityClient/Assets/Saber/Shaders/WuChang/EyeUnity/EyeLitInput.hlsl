#ifndef UNIVERSAL_LIT_INPUT_INCLUDED
#define UNIVERSAL_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "EyeSurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ParallaxMapping.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"

// #if defined(_DETAIL_MULX2) || defined(_DETAIL_SCALED)
// #define _DETAIL
// #endif

// NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
// float4 _DetailAlbedoMap_ST;
float4 _PreIntegratedSSSMap_ST;
float4 _NoiseMap_ST;
float4 _SpecNormalMap_ST;
half4 _BaseColor;
half4 _SpecColor;
half _Cutoff;
half _Smoothness;
half _Metallic;
half _BumpScale;
half _Parallax;
half _OcclusionStrength;
// half _ClearCoatMask;
// half _ClearCoatSmoothness;
// half _DetailAlbedoMapScale;
// half _DetailNormalMapScale;
half _Surface;
CBUFFER_END

// NOTE: Do not ifdef the properties for dots instancing, but ifdef the actual usage.
// Otherwise you might break CPU-side as property constant-buffer offsets change per variant.
// NOTE: Dots instancing is orthogonal to the constant buffer above.
#ifdef UNITY_DOTS_INSTANCING_ENABLED

UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
    UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
    UNITY_DOTS_INSTANCED_PROP(float4, _SpecColor)
    UNITY_DOTS_INSTANCED_PROP(float , _Cutoff)
    UNITY_DOTS_INSTANCED_PROP(float , _Smoothness)
    UNITY_DOTS_INSTANCED_PROP(float , _Metallic)
    UNITY_DOTS_INSTANCED_PROP(float , _BumpScale)
    UNITY_DOTS_INSTANCED_PROP(float , _Parallax)
    UNITY_DOTS_INSTANCED_PROP(float , _OcclusionStrength)
    // UNITY_DOTS_INSTANCED_PROP(float , _ClearCoatMask)
    // UNITY_DOTS_INSTANCED_PROP(float , _ClearCoatSmoothness)
    // UNITY_DOTS_INSTANCED_PROP(float , _DetailAlbedoMapScale)
    // UNITY_DOTS_INSTANCED_PROP(float , _DetailNormalMapScale)
    UNITY_DOTS_INSTANCED_PROP(float , _Surface)
UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

#define _BaseColor              UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4 , _BaseColor)
#define _SpecColor              UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4 , _SpecColor)
#define _Cutoff                 UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _Cutoff)
#define _Smoothness             UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _Smoothness)
#define _Metallic               UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _Metallic)
#define _BumpScale              UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _BumpScale)
#define _Parallax               UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _Parallax)
#define _OcclusionStrength      UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _OcclusionStrength)
// #define _ClearCoatMask          UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _ClearCoatMask)
// #define _ClearCoatSmoothness    UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _ClearCoatSmoothness)
// #define _DetailAlbedoMapScale   UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _DetailAlbedoMapScale)
// #define _DetailNormalMapScale   UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _DetailNormalMapScale)
#define _Surface                UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _Surface)
#endif

// TEXTURE2D(_ParallaxMap);        SAMPLER(sampler_ParallaxMap);
TEXTURE2D(_OcclusionMap);       SAMPLER(sampler_OcclusionMap);
TEXTURE2D(_MaskMap);            SAMPLER(sampler_MaskMap);
TEXTURE2D(_SpecNormalMap);       SAMPLER(sampler_SpecNormalMap);

half2 EyeRefraction(half3 viewDirectionTS, half2 uv, half depthScale, half height)
{
    return uv - ParallaxOffset1Step(height, depthScale, viewDirectionTS);
}

float3 RefractDirection(float internalIoR, float3 WorldNormal, float3 incidentVector)
{
    float airIoR = 1.00029;
    float n = airIoR / internalIoR;
    float facing = dot(WorldNormal, incidentVector);
    float w = n * facing;
    float k = sqrt(1 + (w - n) * (w + n));
    float3 t = -normalize((w - k) * WorldNormal - n * incidentVector);
    return t;
}
float2 EyeRefraction_float(float2 UV, float3 NormalDir, float3 ViewDir, half IOR,
                                  float IrisUVRadius, float IrisDepth, float3 EyeDirection, float3 WorldTangent)
{
    // ģ������ͨ����Ĥ������
    float3 RefractedViewDir = RefractDirection(IOR, NormalDir, ViewDir);
    float cosAlpha = dot(ViewDir, EyeDirection); // EyeDirection���۾���ǰ������
    cosAlpha = lerp(0.325, 1, cosAlpha * cosAlpha); //������������ļн�
    RefractedViewDir = RefractedViewDir * (IrisDepth / cosAlpha); //��Ĥ���Խ������Խǿ��������������н�Խ������Խǿ��

    //����WorldTangent�����EyeDirection��ֱ��������Ҳ���Ǻ�Ĥƽ���Tangent��BiTangent����,Ҳ����UV��ƫ�Ʒ���
    float3 TangentDerive = normalize(WorldTangent - dot(WorldTangent, EyeDirection) * EyeDirection);
    float3 BiTangentDerive = normalize(cross(EyeDirection, TangentDerive));
    float RefractUVOffsetX = dot(RefractedViewDir, TangentDerive);
    float RefractUVOffsetY = dot(RefractedViewDir, BiTangentDerive);
    float2 RefractUVOffset = float2(-RefractUVOffsetX, RefractUVOffsetY);
    float2 UVRefract = UV + IrisUVRadius * RefractUVOffset;
    //UVRefract = lerp(UV,UVRefract,IrisMask);
    return (UVRefract - float2(0.5, 0.5)) / IrisUVRadius * 0.5 + float2(0.5, 0.5);
}


inline void InitializeStandardLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
{
    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    outSurfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);

    outSurfaceData.specular = (half3)0.0;
    outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;

    half4 maskData = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, uv);
    outSurfaceData.smoothness = 1 - maskData.r;
    outSurfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
    // outSurfaceData.normalTS = half3(0,0,1);
    outSurfaceData.occlusion = maskData.b;
    outSurfaceData.emission = 0;
    outSurfaceData.metallic = maskData.g;

    outSurfaceData.clearCoatMask       = half(0.0);
    outSurfaceData.clearCoatSmoothness = half(0.0);
// #endif

// #if defined(_DETAIL)
//     half detailMask = SAMPLE_TEXTURE2D(_DetailMask, sampler_DetailMask, uv).a;
//     float2 detailUv = uv * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
//     outSurfaceData.albedo = ApplyDetailAlbedo(detailUv, outSurfaceData.albedo, detailMask);
//     outSurfaceData.normalTS = ApplyDetailNormal(detailUv, outSurfaceData.normalTS, detailMask);
// #endif
}

#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED
