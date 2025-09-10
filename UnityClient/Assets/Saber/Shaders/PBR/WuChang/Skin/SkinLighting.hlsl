#ifndef UNIVERSAL_LIGHTING_INCLUDED
#define UNIVERSAL_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/BRDF.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging3D.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/AmbientOcclusion.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
#include "PCFShadows.hlsl"


struct SkinSurfaceData
{
    half4 sssFalloff;
    float2 uv;
    half3x3 tangentToWorld;
    float normalMapMipCount;
    half lightIntensity;
};

SkinSurfaceData InitializeSkinSurfaceData(half4 sssFalloff, float2 uv, half3x3 tangentToWorld, half lightIntensity)
{
    SkinSurfaceData skinSurfaceData = (SkinSurfaceData)0;
    skinSurfaceData.sssFalloff = sssFalloff;
    skinSurfaceData.uv = uv;
    skinSurfaceData.tangentToWorld = tangentToWorld;
    skinSurfaceData.normalMapMipCount = _NormalMapMipCount;
    skinSurfaceData.lightIntensity = lightIntensity;
    return skinSurfaceData;
}

///////////////////////////////////////////////////////////////////////////////
//                      Lighting Functions                                   //
///////////////////////////////////////////////////////////////////////////////

/// 次表面散射
half3 SubsurfaceScattering(half3x3 tangentSpaceTransform, half3 lightDir, half3 SSSFalloff, float2 uv, int mipCount, half intensity)
{
    half3 weights = 0.0;
    half scattering = 0.0;
    half NdotL = 0.0;
    half brdfLookup = 0.0;
    half directDiffuse = 0.0;
    half3 brdf = 0.0;
    half3 worldNormal = half3(0, 0, 1);

    // Ref: HDRP's DiffusionProfileSettings.cs #105
    // We importance sample the color channel with the widest scattering distance.
    half radius = max(max(SSSFalloff.x, SSSFalloff.y), SSSFalloff.z); 
    
    /////////////////////////////////////////////////////////////////////
    //	                        Skin Profile                           //
    /////////////////////////////////////////////////////////////////////

    half3 c = min(1.0, SSSFalloff.xyz);

    // Modified using Color Tint with weight from the highest color value of the human skin profile
    half3 profileWeights[6] = {
    (1 - c) * 0.649,
    (1 - c) * 0.366,
    c * 0.198,
    c * 0.113,
    c * 0.358,
    c * 0.078 };

    const half profileVariance[6] = {
    0.0064,
    0.0484,
    0.187,
    0.567,
    1.99,
    7.41 };

    const half profileVarianceSqrt[6] = {
    0.08,	    // sqrt(0.0064)
    0.219,	    // sqrt(0.0484)
    0.432,	    // sqrt(0.187)
    0.753,	    // sqrt(0.567)
    1.410,	    // sqrt(1.99)
    2.722 };	// sqrt(7.41)
    
    // mip count can be calculate in the shader editor and caches it in material property?
    //int mipCount = GetMipCount(TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
    
    // approximation mip level
    half blur = radius * PI * mipCount;
    
    half r = rcp(radius); // 1 / r
    half s = -r * r;

    /////////////////////////////////////////////////////////////////////
    //	              Six Layer Subsurface Scattering                  //
    /////////////////////////////////////////////////////////////////////
    [unroll]
    for (int i = 0; i < 6; i++)
    {
        weights = profileWeights[i];
        scattering = exp(s / profileVarianceSqrt[i]);

    // #ifdef _NORMALMAP
        // blur normal map via mip
        worldNormal = UnpackNormal( SAMPLE_TEXTURE2D_LOD(_BumpMap, sampler_BumpMap, uv, lerp(0.0, blur, profileVariance[i])));
        worldNormal = TransformTangentToWorld(worldNormal, tangentSpaceTransform);
    // #endif

        // Direct Diffuse Lookup
        NdotL = dot(worldNormal, lightDir);
        brdfLookup = mad(NdotL, 0.5, 0.5);
        
        directDiffuse = SAMPLE_TEXTURE2D(_PreIntegratedSSSMap, sampler_PreIntegratedSSSMap, float2(brdfLookup, scattering)).r;

        //brdf += weights * (directDiffuse + (pow(1 - dot(normalWS, viewDirectionWS), 3)) *  ao * shadow * half3(0.9, 0.3, 0.1));
        brdf += weights * directDiffuse * intensity;
    }

    return brdf;
}

half3 LightingPhysicallySSS(BRDFData brdfData, Light light, half3 normalWS, half3 viewDirectionWS, SkinSurfaceData skinSurfaceData)
{
    half3 lightColor = light.color;
    half3 lightDirectionWS = light.direction;
    half lightAttenuation = light.distanceAttenuation * light.shadowAttenuation;

    half3 brdf = brdfData.diffuse;
    brdf += brdfData.specular * DirectBRDFSpecular(brdfData, normalWS, lightDirectionWS, viewDirectionWS);

#if _SSS_ON
    // 次表面散射
    half3x3 tangentToWorld = skinSurfaceData.tangentToWorld;
    half3 sssFalloff = skinSurfaceData.sssFalloff.rgb;
    float2 uv = skinSurfaceData.uv;
    half3 sss = SubsurfaceScattering(tangentToWorld, lightDirectionWS, sssFalloff, uv, skinSurfaceData.normalMapMipCount, skinSurfaceData.lightIntensity);
    half3 radiance = sss * lightColor * lightAttenuation;
#else
    half NdotL = saturate(dot(normalWS, lightDirectionWS));
    half3 radiance = lightColor * (lightAttenuation * NdotL);
#endif
    
    return brdf * radiance;;
}

struct LightingData
{
    half3 giColor;
    half3 mainLightColor;
    half3 additionalLightsColor;
    half3 vertexLightingColor;
    half3 emissionColor;
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

    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_EMISSION))
    {
        lightingColor += lightingData.emissionColor;
    }

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
    lightingData.emissionColor = surfaceData.emission;
    lightingData.vertexLightingColor = 0;
    lightingData.mainLightColor = 0;
    lightingData.additionalLightsColor = 0;

    return lightingData;
}

half3 SkinGlobalIllumination(BRDFData brdfData, half3 bakedGI, half occlusion, float3 positionWS,
    half3 normalWS, half3 viewDirectionWS, float2 normalizedScreenSpaceUV)
{
    half3 reflectVector = reflect(-viewDirectionWS, normalWS);
    half NoV = saturate(dot(normalWS, viewDirectionWS));
    half fresnelTerm = Pow4(1.0 - NoV);

    half3 indirectDiffuse = bakedGI;
    half3 indirectSpecular = GlossyEnvironmentReflection(reflectVector, positionWS, brdfData.perceptualRoughness, 1.0h, normalizedScreenSpaceUV);
    half3 color = EnvironmentBRDF(brdfData, indirectDiffuse, indirectSpecular, fresnelTerm);
    return color * occlusion;
}

///////////////////////////////////////////////////////////////////////////////
//                      Fragment Functions                                   //
//       Used by ShaderGraph and others builtin renderers                    //
///////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////
/// PBR lighting...
////////////////////////////////////////////////////////////////////////////////
half4 UniversalFragmentPBR(InputData inputData, SurfaceData surfaceData, SkinSurfaceData skinSurfaceData)
{
    BRDFData brdfData;

    // NOTE: can modify "surfaceData"...
    InitializeBRDFData(surfaceData, brdfData);

    // Clear-coat calculation...
    half4 shadowMask = CalculateShadowMask(inputData);
    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
    uint meshRenderingLayers = GetMeshRenderingLayer();
#if _PCFSHADOW_ON
    Light mainLight = get_main_light_poisson(inputData.shadowCoord, inputData.positionWS); //pcf阴影
#else
    Light mainLight = GetMainLight(inputData, shadowMask, aoFactor);
#endif

    // NOTE: We don't apply AO to the GI here because it's done in the lighting calculation below...
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI);

    LightingData lightingData = CreateLightingData(inputData, surfaceData);

    lightingData.giColor = SkinGlobalIllumination(brdfData, inputData.bakedGI, aoFactor.indirectAmbientOcclusion, inputData.positionWS,
                                              inputData.normalWS, inputData.viewDirectionWS, inputData.normalizedScreenSpaceUV);
#ifdef _LIGHT_LAYERS
    if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
#endif
    {
        lightingData.mainLightColor = LightingPhysicallySSS(brdfData, mainLight, inputData.normalWS, inputData.viewDirectionWS, skinSurfaceData);
    }

    uint pixelLightCount = GetAdditionalLightsCount();

    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

#ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
#endif
        {
            lightingData.additionalLightsColor += LightingPhysicallySSS(brdfData, light, inputData.normalWS, inputData.viewDirectionWS, skinSurfaceData);
        }
    LIGHT_LOOP_END

#if REAL_IS_HALF
    // Clamp any half.inf+ to HALF_MAX
    return min(CalculateFinalColor(lightingData, surfaceData.alpha), HALF_MAX);
#else
    return CalculateFinalColor(lightingData, surfaceData.alpha);
#endif
}

#endif
