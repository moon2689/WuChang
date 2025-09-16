#ifndef UNIVERSAL_LIGHTING_INCLUDED
#define UNIVERSAL_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/BRDF.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging3D.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/AmbientOcclusion.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonLighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
#include "../Skin/PCFShadows.hlsl"

half4 _ScatteringColor;
half _Depth;
half _CausticIntensity;
half _CausticBlend;
TEXTURE2D(_EyeDirMap);
SAMPLER(sampler_EyeDirMap);
half _IrisRadius;

#if defined(LIGHTMAP_ON)
    #define DECLARE_LIGHTMAP_OR_SH(lmName, shName, index) float2 lmName : TEXCOORD##index
    #define OUTPUT_LIGHTMAP_UV(lightmapUV, lightmapScaleOffset, OUT) OUT.xy = lightmapUV.xy * lightmapScaleOffset.xy + lightmapScaleOffset.zw;
    #define OUTPUT_SH(normalWS, OUT)
#else
    #define DECLARE_LIGHTMAP_OR_SH(lmName, shName, index) half3 shName : TEXCOORD##index
    #define OUTPUT_LIGHTMAP_UV(lightmapUV, lightmapScaleOffset, OUT)
    #define OUTPUT_SH(normalWS, OUT) OUT.xyz = SampleSHVertex(normalWS)
#endif

struct EyeSurfaceData
{
    half3 scatteringColor;
    half  thickness;
    half3 specNormal;
    half3 causticNormal;
    float2 uv;
    float2 refractUV;
    float mask;
};

void InitializeEyeSurfaceData (half3x3 tangentToWorld, half3 specNormal, half3 normal, float2 uv, float3 viewDirectionTS, out EyeSurfaceData eyeSurface, inout SurfaceData surfaceData)
{
    eyeSurface      = (EyeSurfaceData) 0;

    float2 EyeUV = (uv - float2(0.5, 0.5)) / _IrisRadius * 0.5 + float2(0.5, 0.5);
    half4 refractData = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, EyeUV);
    eyeSurface.mask = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, EyeUV).a;
    
    // half2 refractUV = EyeRefraction(viewDirectionTS, uv, _Depth, refractData.a);
    half2 refractUV = EyeRefraction_float(uv, tangentToWorld[2], viewDirectionTS, 1.33, _IrisRadius, refractData.a * _Depth, TransformTangentToWorld(UnpackNormal(SAMPLE_TEXTURE2D(_EyeDirMap, sampler_EyeDirMap, uv)), tangentToWorld) , tangentToWorld[0]);
    half2 normalTmp = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, refractUV).rg * 2 - 1;
    half3 causticNormal = normalize(half3(normalTmp, sqrt(1 - saturate(dot(normalTmp, normalTmp)))));
    
    half4 refractedIrisData = SAMPLE_TEXTURE2D(_IrisMap, sampler_IrisMap, refractUV);
    
    surfaceData.albedo = lerp(surfaceData.albedo, refractedIrisData.a * _IrisColor.rgb, eyeSurface.mask);
    surfaceData.smoothness = lerp(surfaceData.smoothness, 1 - refractedIrisData.r, eyeSurface.mask);
    surfaceData.metallic = lerp(surfaceData.metallic, refractedIrisData.g, eyeSurface.mask);
    surfaceData.occlusion = lerp(surfaceData.occlusion, refractedIrisData.b, eyeSurface.mask);
    
    eyeSurface.scatteringColor     = _ScatteringColor.rgb,
    eyeSurface.thickness           = 1,
    #if defined(_SPECNORMALMAP)
    eyeSurface.specNormal          = normalize(TransformTangentToWorld(specNormal, tangentToWorld)),
    #else
    eyeSurface.specNormal          = normal,
    #endif
    eyeSurface.causticNormal       = causticNormal,
    eyeSurface.uv                  = uv;
    eyeSurface.refractUV           = refractUV;
}
///////////////////////////////////////////////////////////////////////////////
//                      Lighting Functions                                   //
///////////////////////////////////////////////////////////////////////////////
half3 ColorBleedAO(half occlusion, half3 colorBleed)
{
    return pow(abs(occlusion), 1.0 - colorBleed);
}
half3 SubsurfaceScattering(half NdotL, half shadowAttenuation, half distanceAttenuation, half curvature)
{
    // In HDRP, it uses Wrapped Power lighting + Subsurface Scattering, here we use regular wrapped light for LUT instead
    // HDRP Ref: Runtime/Material/Eye/Eye.hlsl #446
    // half clampedNdotL = ComputeWrappedPowerDiffuseLighting(NdotL, PI / 12.0, 2.0);
    half clampedNdotL = mad(NdotL, 0.5, 0.5);
    half shadow = min(shadowAttenuation + 0.1, 1.0);
    clampedNdotL *= mad(shadow, 0.5, 0.5);

    return SAMPLE_TEXTURE2D(_PreIntegratedSSSMap, sampler_PreIntegratedSSSMap, float2(clampedNdotL, curvature)).xyz * distanceAttenuation;
}

half3 LightingPhysicallyBased(BRDFData brdfData, BRDFData overlayBRDFData,
    Light light,
    half3 normalWS, half3 viewDirectionWS,
    bool specularHighlightsOff, EyeSurfaceData eyeSurfaceData)
{
    half3 lightColor        = light.color;
    half3 lightDirectionWS  = light.direction;
    half lightAttenuation   = light.distanceAttenuation * light.shadowAttenuation;
    
    half NdotL = saturate(dot(normalWS, lightDirectionWS));
    // half3 radiance = lightColor * (lightAttenuation * ComputeWrappedPowerDiffuseLighting(NdotL, PI / 12.0, 2.0));
    half3 radiance = lightColor * lightAttenuation * NdotL;
    half3 brdf = brdfData.diffuse;
#ifndef _SPECULARHIGHLIGHTS_OFF
    [branch] if (!specularHighlightsOff)
    {
        brdf += brdfData.specular * DirectBRDFSpecular(brdfData, normalWS, viewDirectionWS, viewDirectionWS);
        brdf += brdfData.specular * DirectBRDFSpecular(overlayBRDFData, eyeSurfaceData.specNormal, lightDirectionWS, viewDirectionWS);
    }
#endif // _SPECULARHIGHLIGHTS_OFF
    half3 subsurface = SubsurfaceScattering(NdotL, light.shadowAttenuation, light.distanceAttenuation, eyeSurfaceData.scatteringColor.r);
    subsurface *= lightColor * light.distanceAttenuation;
    radiance = lerp(subsurface, radiance, eyeSurfaceData.mask);

    // return normalWS;
    // return brdfData.specular * DirectBRDFSpecular(overlayBRDFData, eyeSurfaceData.specNormal, lightDirectionWS, viewDirectionWS);
    return brdf * radiance;
}

half3 VertexLighting(float3 positionWS, half3 normalWS)
{
    half3 vertexLightColor = half3(0.0, 0.0, 0.0);

#ifdef _ADDITIONAL_LIGHTS_VERTEX
    uint lightsCount = GetAdditionalLightsCount();
    LIGHT_LOOP_BEGIN(lightsCount)
        Light light = GetAdditionalLight(lightIndex, positionWS);
        half3 lightColor = light.color * light.distanceAttenuation;
        vertexLightColor += LightingLambert(lightColor, light.direction, normalWS);
    LIGHT_LOOP_END
#endif

    return vertexLightColor;
}

struct LightingData
{
    half3 giColor;
    half3 mainLightColor;
    half3 additionalLightsColor;
    half3 vertexLightingColor;
};

half3 CalculateLightingColor(LightingData lightingData, half3 albedo)
{
    half3 lightingColor = 0;

    if (IsOnlyAOLightingFeatureEnabled())
    {
        return lightingData.giColor; // Contains white + AO
    }

    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_GLOBAL_ILLUMINATION))
    {
        lightingColor += lightingData.giColor;
    }

    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_MAIN_LIGHT))
    {
        lightingColor += lightingData.mainLightColor;
    }

    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_ADDITIONAL_LIGHTS))
    {
        lightingColor += lightingData.additionalLightsColor;
    }

    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_VERTEX_LIGHTING))
    {
        lightingColor += lightingData.vertexLightingColor;
    }

    lightingColor *= albedo;

    return lightingColor;
}

half4 CalculateFinalColor(LightingData lightingData, half alpha)
{
    half3 finalColor = CalculateLightingColor(lightingData, 1);

    return half4(finalColor, alpha);
}


LightingData CreateLightingData(InputData inputData, SurfaceData surfaceData)
{
    LightingData lightingData;

    lightingData.giColor = inputData.bakedGI;
    lightingData.vertexLightingColor = 0;
    lightingData.mainLightColor = 0;
    lightingData.additionalLightsColor = 0;

    return lightingData;
}

///////////////////////////////////////////////////////////////////////////////
//                      Fragment Functions                                   //
//       Used by ShaderGraph and others builtin renderers                    //
///////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////
/// PBR lighting...
////////////////////////////////////////////////////////////////////////////////
half4 UniversalFragmentPBR(InputData inputData, SurfaceData surfaceData, EyeSurfaceData eyeSurfaceData)
{
    #if defined(_SPECULARHIGHLIGHTS_OFF)
    bool specularHighlightsOff = true;
    #else
    bool specularHighlightsOff = false;
    #endif
    BRDFData brdfData;

    // NOTE: can modify "surfaceData"...
    InitializeBRDFData(surfaceData, brdfData);

    half4 shadowMask = CalculateShadowMask(inputData);
    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
    // uint meshRenderingLayers = GetMeshRenderingLayer();
    // Light mainLight = GetMainLight(inputData, shadowMask, aoFactor);
    Light mainLight = get_main_light_poisson(inputData.shadowCoord, inputData.positionWS);
    // NOTE: We don't apply AO to the GI here because it's done in the lighting calculation below...
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI);

    LightingData lightingData = CreateLightingData(inputData, surfaceData);

    half3 reflectVector = reflect(-inputData.viewDirectionWS, eyeSurfaceData.causticNormal);
    half NoV = saturate(dot(eyeSurfaceData.causticNormal, inputData.viewDirectionWS * half3(-1,1,1)));
    half fresnelTerm = Pow4(1.0 - NoV);
    float surfaceReduction = 1.0 / (0.2 + 1.0);
    half3 indirectSpecular = GlossyEnvironmentReflection(reflectVector, 0.3, half(1.0));
    half3 caustic = lerp(brdfData.diffuse, half3(1,1,1), _CausticBlend) * lerp(eyeSurfaceData.mask, _CausticIntensity, _CausticBlend) * indirectSpecular * half3(surfaceReduction * lerp(brdfData.specular, brdfData.grazingTerm, fresnelTerm)) * step(0.9, eyeSurfaceData.mask) * SAMPLE_TEXTURE2D(_IrisMap, sampler_IrisMap, eyeSurfaceData.refractUV).g;

    BRDFData overlayBRDFData = brdfData;
    half4 maskData = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, eyeSurfaceData.uv);
    overlayBRDFData.perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(1 - maskData.r);
    overlayBRDFData.roughness = max(PerceptualRoughnessToRoughness(overlayBRDFData.perceptualRoughness), HALF_MIN_SQRT);
    overlayBRDFData.roughness2 = max(overlayBRDFData.roughness * overlayBRDFData.roughness, HALF_MIN);;
    overlayBRDFData.normalizationTerm   = overlayBRDFData.roughness * half(4.0) + half(2.0);
    overlayBRDFData.roughness2MinusOne  = overlayBRDFData .roughness2 - half(1.0);
    overlayBRDFData.specular = lerp(kDieletricSpec.rgb, surfaceData.albedo, maskData.g);

    lightingData.giColor = GlobalIllumination(overlayBRDFData, inputData.bakedGI, aoFactor.indirectAmbientOcclusion, inputData.positionWS,
    eyeSurfaceData.specNormal, inputData.viewDirectionWS) + caustic;
        
    // lightingData.giColor = GlobalIllumination(brdfData, inputData.bakedGI, aoFactor.indirectAmbientOcclusion, inputData.positionWS,
    // inputData.tangentToWorld[2], inputData.viewDirectionWS) + brdfData.diffuse * lerp(eyeSurfaceData.mask, _CausticIntensity, _CausticBlend) * indirectSpecular * half3(surfaceReduction * lerp(brdfData.specular, brdfData.grazingTerm, fresnelTerm)) * step(0.9, eyeSurfaceData.mask) * SAMPLE_TEXTURE2D(_IrisMap, sampler_IrisMap, eyeSurfaceData.refractUV).g;

    
    // lightingData.giColor = GlobalIllumination(brdfData, inputData.bakedGI, aoFactor.indirectAmbientOcclusion, inputData.positionWS,
    //                         inputData.normalWS, inputData.viewDirectionWS);
    
    lightingData.mainLightColor = LightingPhysicallyBased(brdfData, overlayBRDFData,
                                                          mainLight,
                                                          inputData.normalWS, inputData.viewDirectionWS,
                                                          specularHighlightsOff, eyeSurfaceData);


    #if defined(_ADDITIONAL_LIGHTS)
    uint pixelLightCount = GetAdditionalLightsCount();

    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

#ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
#endif
        {
            lightingData.additionalLightsColor += LightingPhysicallyBased(brdfData, overlayBRDFData, light,
                                                                          inputData.normalWS, inputData.viewDirectionWS,
                                                                          specularHighlightsOff, eyeSurfaceData);
        }
    LIGHT_LOOP_END
    #endif

    #if defined(_ADDITIONAL_LIGHTS_VERTEX)
    lightingData.vertexLightingColor += inputData.vertexLighting * brdfData.diffuse;
    #endif
// return half4(lightingData.mainLightColor, 1);
// return half4(lightingData.giColor, 1);
// return mainLight.shadowAttenuation;
// return half4(indirectSpecular * half3(surfaceReduction * lerp(brdfData.specular, brdfData.grazingTerm, fresnelTerm)) * eyeSurfaceData.mask, 1);
    return CalculateFinalColor(lightingData, surfaceData.alpha);
}

#endif
