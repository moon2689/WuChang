#ifndef UNIVERSAL_LIGHTING_INCLUDED
#define UNIVERSAL_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/BRDF.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging3D.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/AmbientOcclusion.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"

#define OUTPUT_SH(normalWS, OUT) OUT.xyz = SampleSHVertex(normalWS)

struct CottonSurfaceData
{
    half3 sheenColor;
    half sheenDFG;
    half sheenRoughness;
    half occlusion;
};

CottonSurfaceData InitializeCottonSurface(half4 maskMap, half occlusion)
{
    CottonSurfaceData surfaceData = (CottonSurfaceData)0;
    surfaceData.sheenColor = _SheenColor.rgb;
    surfaceData.sheenDFG = maskMap.b;
    surfaceData.sheenRoughness = maskMap.a;
    surfaceData.occlusion = occlusion;
    return surfaceData;
}

///////////////////////////////////////////////////////////////////////////////
//                      Lighting Functions                                   //
///////////////////////////////////////////////////////////////////////////////


/*
 * 参考：https://knarkowicz.wordpress.com/2018/01/04/cloth-shading/
 * 参考：https://google.github.io/filament/Filament.html#materialsystem/clothmodel
 */

float D_Charlie_Filament(float roughness, float NoH) // from Filament
{
    // Estevez and Kulla 2017, "Production Friendly Microfacet Sheen BRDF"
    float invAlpha  = 1.0 / roughness;
    float cos2h = NoH * NoH;
    float sin2h = max(1.0 - cos2h, 0.0078125); // 2^(-14/2), so sin2h^2 > 0 in fp16
    return (2.0 + invAlpha) * pow(sin2h, invAlpha * 0.5) / (2.0 * PI);
}

float Vis_Cloth( float NoV, float NoL )
{
    return rcp( 4 * ( NoL + NoV - NoL * NoV ) );
}

half3 LightingPhysicallyBased(BRDFData brdfData, Light light, half3 normalWS, half3 viewDirectionWS, CottonSurfaceData cottonSurfaceData)
{
    half3 lightColor = light.color;
    half3 lightDirectionWS = light.direction;
    half lightAttenuation = light.distanceAttenuation * light.shadowAttenuation;
    half NdotL = saturate(dot(normalWS, lightDirectionWS));
    half3 radiance = lightColor * (lightAttenuation * NdotL) * cottonSurfaceData.occlusion;

    half3 brdf = brdfData.diffuse;
    //brdf += brdfData.specular * DirectBRDFSpecular(brdfData, normalWS, lightDirectionWS, viewDirectionWS);

    // spec
    half3 H = normalize(lightDirectionWS + viewDirectionWS);
    float NoH = saturate(dot(normalWS, H));
    float NoV = saturate(abs(dot(normalWS, viewDirectionWS)) + 1e-5);
    float VoH = saturate(dot(viewDirectionWS, H));
    float D = D_Charlie_Filament(brdfData.roughness, NoH);
    float Vis = Vis_Cloth(NoV, NdotL);
    float3 F = F_Schlick(kDielectricSpec.rgb, VoH);
    float3 brdfSpec = (D * Vis * F) * PI;
    brdf += brdfSpec;

    // sheen color
    float sheenD = D_Charlie_Filament(cottonSurfaceData.sheenRoughness, NoH);
    half3 sheenF = cottonSurfaceData.sheenColor;
    half3 brdfSheenSpec = (sheenD * Vis * sheenF) * PI;
    brdf += brdfSheenSpec;
    //half lossEnergy = max(max(cottonSurfaceData.sheenColor.r, cottonSurfaceData.sheenColor.g), cottonSurfaceData.sheenColor.b) * cottonSurfaceData.sheenDFG;

    return brdf * radiance;
}

half3 GlobalIlluminationCotton(BRDFData brdfData,
    half3 bakedGI, half occlusion, float3 positionWS,
    half3 normalWS, half3 viewDirectionWS, float2 normalizedScreenSpaceUV,
    CottonSurfaceData cottonSurfaceData)
{
    half3 reflectVector = reflect(-viewDirectionWS, normalWS);
    //half NoV = saturate(dot(normalWS, viewDirectionWS));
    //half fresnelTerm = Pow4(1.0 - NoV);

    half3 indirectDiffuse = bakedGI;
    half3 indirectSpecular = GlossyEnvironmentReflection(reflectVector, positionWS, brdfData.perceptualRoughness, 1.0h, normalizedScreenSpaceUV);

    //half3 color = EnvironmentBRDF(brdfData, indirectDiffuse, indirectSpecular, fresnelTerm);
    //half3 color = indirectDiffuse * brdfData.diffuse;
    //color += indirectSpecular * EnvironmentBRDFSpecular(brdfData, fresnelTerm);

    half3 specDFG = kDielectricSpec.xxx * cottonSurfaceData.sheenDFG;
    half3 specLighting = indirectSpecular * specDFG;

    // DFG
    half3 sheenSpecDFG = cottonSurfaceData.sheenColor * cottonSurfaceData.sheenDFG;
    half3 sheenSpecLighting = indirectSpecular * sheenSpecDFG;
    //half lossEnergy = max(max(cottonSurfaceData.sheenColor.r, cottonSurfaceData.sheenColor.g), cottonSurfaceData.sheenColor.b) * cottonSurfaceData.sheenDFG;

    //half3 color = (indirectDiffuse * brdfData.diffuse + specLighting) * (1 - lossEnergy) + sheenSpecLighting;
    half3 color = indirectDiffuse * brdfData.diffuse + specLighting + sheenSpecLighting;
    
    return color * occlusion;
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

///////////////////////////////////////////////////////////////////////////////
//                      Fragment Functions                                   //
//       Used by ShaderGraph and others builtin renderers                    //
///////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////
/// PBR lighting...
////////////////////////////////////////////////////////////////////////////////
half4 UniversalFragmentPBR(InputData inputData, SurfaceData surfaceData, CottonSurfaceData cottonSurfaceData)
{
    BRDFData brdfData;

    // NOTE: can modify "surfaceData"...
    InitializeBRDFData(surfaceData, brdfData);

    // Clear-coat calculation...
    BRDFData brdfDataClearCoat = CreateClearCoatBRDFData(surfaceData, brdfData);
    half4 shadowMask = CalculateShadowMask(inputData);
    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
    uint meshRenderingLayers = GetMeshRenderingLayer();
    Light mainLight = GetMainLight(inputData, shadowMask, aoFactor);

    // NOTE: We don't apply AO to the GI here because it's done in the lighting calculation below...
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI);

    LightingData lightingData = CreateLightingData(inputData, surfaceData);

    lightingData.giColor = GlobalIlluminationCotton(brdfData, inputData.bakedGI, aoFactor.indirectAmbientOcclusion, inputData.positionWS,
                                              inputData.normalWS, inputData.viewDirectionWS, inputData.normalizedScreenSpaceUV, cottonSurfaceData);
#ifdef _LIGHT_LAYERS
    if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
#endif
    {
        lightingData.mainLightColor = LightingPhysicallyBased(brdfData, mainLight, inputData.normalWS, inputData.viewDirectionWS, cottonSurfaceData);
    }

    #if defined(_ADDITIONAL_LIGHTS)
    uint pixelLightCount = GetAdditionalLightsCount();

    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

#ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
#endif
        {
            lightingData.additionalLightsColor += LightingPhysicallyBased(brdfData, light, inputData.normalWS, inputData.viewDirectionWS, cottonSurfaceData);
        }
    LIGHT_LOOP_END
    #endif

#if REAL_IS_HALF
    // Clamp any half.inf+ to HALF_MAX
    return min(CalculateFinalColor(lightingData, surfaceData.alpha), HALF_MAX);
#else
    return CalculateFinalColor(lightingData, surfaceData.alpha);
#endif
}

#endif
