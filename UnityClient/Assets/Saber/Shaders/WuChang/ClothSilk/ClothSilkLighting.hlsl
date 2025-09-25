#ifndef UNIVERSAL_LIGHTING_INCLUDED
#define UNIVERSAL_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/BRDF.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging3D.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/AmbientOcclusion.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"

#define OUTPUT_SH(normalWS, OUT) OUT.xyz = SampleSHVertex(normalWS)

struct SilkSurfaceData
{
    half3 tangentDir;
    half3 bitangentDir;
    half anisotropic;
    half4 anisotropicColor;
};

SilkSurfaceData InitializeSilkSurfaceData(half3 normalTS, half3x3 tangentToWorld)
{
    SilkSurfaceData surfaceData = (SilkSurfaceData)0;
    float3 tangentT = normalize(float3(1, 0, _NormalScale * normalTS.x));
    float3 tangentB = normalize(float3(0, 1, _NormalScale * normalTS.y));
    surfaceData.tangentDir = TransformTangentToWorld(tangentT, tangentToWorld);
    surfaceData.bitangentDir = TransformTangentToWorld(tangentB, tangentToWorld);
    surfaceData.anisotropic = _Anisotropic;
    surfaceData.anisotropicColor = _AnisotropicColor;
    return surfaceData;
}

///////////////////////////////////////////////////////////////////////////////
//                      Lighting Functions                                   //
///////////////////////////////////////////////////////////////////////////////

float Square( float x )
{
    return x*x;
}

// Anisotropic GGX
// [Burley 2012, "Physically-Based Shading at Disney"]
float D_GGXaniso( float ax, float ay, float NoH, float XoH, float YoH )
{
    // The two formulations are mathematically equivalent
    #if 1
    float a2 = ax * ay;
    float3 V = float3(ay * XoH, ax * YoH, a2 * NoH);
    float S = dot(V, V);

    return (1.0f / PI) * a2 * Square(a2 / S);
    #else
    float d = XoH*XoH / (ax*ax) + YoH*YoH / (ay*ay) + NoH*NoH;
    return 1.0f / ( PI * ax*ay * d*d );
    #endif
}

// [Heitz 2014, "Understanding the Masking-Shadowing Function in Microfacet-Based BRDFs"]
float Vis_SmithJointAniso(float ax, float ay, float NoV, float NoL, float XoV, float XoL, float YoV, float YoL)
{
    float Vis_SmithV = NoL * length(float3(ax * XoV, ay * YoV, NoV));
    float Vis_SmithL = NoV * length(float3(ax * XoL, ay * YoL, NoL));
    return 0.5 * rcp(Vis_SmithV + Vis_SmithL);
}

half3 LightingPhysicallyBased(BRDFData brdfData, Light light, half3 normalWS, half3 viewDirectionWS, SilkSurfaceData silkSurfaceData)
{
    half3 lightColor = light.color;
    half3 lightDirectionWS = light.direction;
    half lightAttenuation = light.distanceAttenuation * light.shadowAttenuation;
    half NdotL = saturate(dot(normalWS, lightDirectionWS));
    half3 radiance = lightColor * (lightAttenuation * NdotL);

    half3 brdf = brdfData.diffuse;
    //brdf += brdfData.specular * DirectBRDFSpecular(brdfData, normalWS, lightDirectionWS, viewDirectionWS);

    half3 H = normalize(lightDirectionWS + viewDirectionWS);
    half NoH = saturate(dot(normalWS, H));
    half VoH = saturate(dot(viewDirectionWS, H));
    half NoV = saturate(abs(dot(normalWS, viewDirectionWS)) + 1e-5);

    half XoH = dot(silkSurfaceData.tangentDir, H);
    half YoH = dot(silkSurfaceData.bitangentDir, H);
    half XoL = dot(silkSurfaceData.tangentDir, lightDirectionWS);
    half YoL = dot(silkSurfaceData.bitangentDir, lightDirectionWS);
    half XoV = dot(silkSurfaceData.tangentDir, viewDirectionWS);
    half YoV = dot(silkSurfaceData.bitangentDir, viewDirectionWS);
    
    // 这里参考UE4代码GetAnisotropicRoughness
    // Anisotropic parameters: ax and ay are the roughness along the tangent and bitangent	
    // Kulla 2017, "Revisiting Physically Based Shading at Imageworks"
    half ax = max(brdfData.roughness2 * (1 + silkSurfaceData.anisotropic), 0.001);
    half ay = max(brdfData.roughness2 * (1 - silkSurfaceData.anisotropic), 0.001);

    // 各向异性高光，参考UE4代码SpecularGGX
    half D = D_GGXaniso(ax, ay, NoH, XoH, YoH);
    half Vis = Vis_SmithJointAniso(ax, ay, NoV, NdotL, XoV, XoL, YoV, YoL);
    //half3 F = F_Schlick(kDielectricSpec.rgb, VoH);
    half3 F = F_Schlick(silkSurfaceData.anisotropicColor.rgb, VoH);
    half3 brdfSpec = D * Vis * F;
    brdf += brdfSpec;

    return brdf * radiance;
}

half3 GlobalIlluminationSilk(BRDFData brdfData, half3 bakedGI, half occlusion, float3 positionWS,
    half3 normalWS, half3 viewDirectionWS, float2 normalizedScreenSpaceUV, SilkSurfaceData silkSurfaceData)
{
    // 这里对Cube进行扭曲拉伸，参考Filament代码getReflectedVector
    float3 anisoDir = silkSurfaceData.anisotropic > 0 ? silkSurfaceData.bitangentDir : silkSurfaceData.tangentDir;
    float3 anisoTangent = cross(viewDirectionWS, anisoDir);
    float3 anisoNormal = cross(anisoTangent, anisoDir);
    float3 bentNormal = normalize(lerp(normalWS, anisoNormal, abs(silkSurfaceData.anisotropic)));
    
    //half3 reflectVector = reflect(-viewDirectionWS, normalWS);
    half3 reflectVector = reflect(-viewDirectionWS, bentNormal);
    
    //half NoV = saturate(dot(normalWS, viewDirectionWS));
    half NoV = saturate(abs(dot(bentNormal, viewDirectionWS))+1e-5);
    half fresnelTerm = Pow4(1.0 - NoV);

    half3 indirectDiffuse = bakedGI;
    half3 indirectSpecular = GlossyEnvironmentReflection(reflectVector, positionWS, brdfData.perceptualRoughness, 1.0h, normalizedScreenSpaceUV);

    half3 color = EnvironmentBRDF(brdfData, indirectDiffuse, indirectSpecular, fresnelTerm);

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
half4 UniversalFragmentPBR(InputData inputData, SurfaceData surfaceData, SilkSurfaceData silkSurfaceData)
{
    BRDFData brdfData;

    // NOTE: can modify "surfaceData"...
    InitializeBRDFData(surfaceData, brdfData);
    
    half4 shadowMask = CalculateShadowMask(inputData);
    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
    uint meshRenderingLayers = GetMeshRenderingLayer();
    Light mainLight = GetMainLight(inputData, shadowMask, aoFactor);

    // NOTE: We don't apply AO to the GI here because it's done in the lighting calculation below...
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI);

    LightingData lightingData = CreateLightingData(inputData, surfaceData);

    lightingData.giColor = GlobalIlluminationSilk(brdfData, inputData.bakedGI, aoFactor.indirectAmbientOcclusion, inputData.positionWS,
                                              inputData.normalWS, inputData.viewDirectionWS, inputData.normalizedScreenSpaceUV, silkSurfaceData);
    
#ifdef _LIGHT_LAYERS
    if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
#endif
    {
        lightingData.mainLightColor = LightingPhysicallyBased(brdfData, mainLight, inputData.normalWS, inputData.viewDirectionWS, silkSurfaceData);
    }

    #if defined(_ADDITIONAL_LIGHTS)
    uint pixelLightCount = GetAdditionalLightsCount();

    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

#ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
#endif
        {
            lightingData.additionalLightsColor += LightingPhysicallyBased(brdfData, light, inputData.normalWS, inputData.viewDirectionWS, silkSurfaceData);
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
