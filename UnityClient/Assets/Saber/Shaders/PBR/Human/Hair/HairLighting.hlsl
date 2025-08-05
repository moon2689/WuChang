#ifndef UNIVERSAL_LIGHTING_INCLUDED
#define UNIVERSAL_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/BRDF.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging3D.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/AmbientOcclusion.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"

#if defined(LIGHTMAP_ON)
    #define DECLARE_LIGHTMAP_OR_SH(lmName, shName, index) float2 lmName : TEXCOORD##index
    #define OUTPUT_LIGHTMAP_UV(lightmapUV, lightmapScaleOffset, OUT) OUT.xy = lightmapUV.xy * lightmapScaleOffset.xy + lightmapScaleOffset.zw;
    #define OUTPUT_SH(normalWS, OUT)
#else
    #define DECLARE_LIGHTMAP_OR_SH(lmName, shName, index) half3 shName : TEXCOORD##index
    #define OUTPUT_LIGHTMAP_UV(lightmapUV, lightmapScaleOffset, OUT)
    #define OUTPUT_SH(normalWS, OUT) OUT.xyz = SampleSHVertex(normalWS)
#endif

#define DEFAULT_HAIR_SPECULAR_VALUE 0.0465 // Hair is IOR 1.55

//-----------------------------------------------------------------------------
// conversion function for forward
//-----------------------------------------------------------------------------
real RoughnessToBlinnPhongSpecularExponent(real roughness)
{
    return clamp(2 * rcp(roughness * roughness) - 2, FLT_EPS, rcp(FLT_EPS));
}

//-----------------------------------------------------------------------------
// Unity Hair Lighting functions (based on HDRP hair lighting model)
//-----------------------------------------------------------------------------
struct BSDFData
{
    half secondaryGrazingTerm;
    half ambientOcclusion;
    half3 diffuseColor;
    half3 fresnel0;
    half3 specularTint;
    half3 normalWS;
    half3 geomNormalWS;
    half perceptualRoughness;
    half3 transmittance;
    half rimTransmissionIntensity;
    half3 hairStrandDirectionWS;
    half anisotropy;
    half secondaryPerceptualRoughness;
    half3 secondarySpecularTint;
    half specularExponent;
    half secondarySpecularExponent;
    half specularShift;
    half secondarySpecularShift;
};

struct HairSurfaceData
{
    half3   albedo;
    half	occlusion;
    half3	emission;
    half    alpha;
    half3   normalWS;
    half3	geomNormalWS;
    half    smoothness;
    half3   transmittance;
    half    rimTransmissionIntensity;
    half3	hairStrandDirection;
    half    secondarySmoothness;
    half3   specularTint;
    half3   secondarySpecularTint;
    half    specularShift;
    half    secondarySpecularShift;
    half2   uv;
    half4    vertexColor;
};

// ref: HDRP ver.10 - High Definition RP/Runtime/Material/Hair/Hair.hlsl #130
BSDFData ConvertSurfaceDataToBSDFData(HairSurfaceData hairSurfaceData, SurfaceData surfaceData)
{
    BSDFData bsdfData;
    ZERO_INITIALIZE(BSDFData, bsdfData);

    bsdfData.ambientOcclusion               = surfaceData.occlusion;
    bsdfData.diffuseColor                   = surfaceData.albedo;

    bsdfData.normalWS                       = hairSurfaceData.normalWS;
    bsdfData.geomNormalWS                   = hairSurfaceData.geomNormalWS;

    half secondaryReflectivity              = ReflectivitySpecular(hairSurfaceData.secondarySpecularTint);
    bsdfData.secondaryGrazingTerm           = saturate(hairSurfaceData.secondarySmoothness + secondaryReflectivity);
    // Diffuse has no energy conservation, Balancing energy is left to artist
    bsdfData.specularTint                   = hairSurfaceData.specularTint;
    bsdfData.secondarySpecularTint          = hairSurfaceData.secondarySpecularTint;

    bsdfData.perceptualRoughness            = PerceptualSmoothnessToPerceptualRoughness(surfaceData.smoothness);
    bsdfData.secondaryPerceptualRoughness   = PerceptualSmoothnessToPerceptualRoughness(hairSurfaceData.secondarySmoothness);
    real roughness1                         = PerceptualRoughnessToRoughness(bsdfData.perceptualRoughness);
    real roughness2                         = PerceptualRoughnessToRoughness(bsdfData.secondaryPerceptualRoughness);

    bsdfData.specularExponent               = RoughnessToBlinnPhongSpecularExponent(roughness1);
    bsdfData.secondarySpecularExponent      = RoughnessToBlinnPhongSpecularExponent(roughness2);
    bsdfData.specularShift                  = hairSurfaceData.specularShift;
    bsdfData.secondarySpecularShift         = hairSurfaceData.secondarySpecularShift;
    // This value will be override by the value in diffusion profile
    bsdfData.fresnel0                       = DEFAULT_HAIR_SPECULAR_VALUE;
    bsdfData.transmittance                  = hairSurfaceData.transmittance;
    bsdfData.rimTransmissionIntensity       = hairSurfaceData.rimTransmissionIntensity;

    // This is the hair tangent (which represents the hair strand direction, root to tip).
    bsdfData.hairStrandDirectionWS          = hairSurfaceData.hairStrandDirection;

    bsdfData.anisotropy                     = 0.8; // For hair we fix the anisotropy

    return bsdfData;
}

// CBSDF Struct in: com.unity.render-pipeline.core/ShaderLibrary/BSDF.hlsl
// ref: HDRP ver.10.1.x - High Definition RP/Runtime/Material/Hair/Hair.hlsl #359
// (BSDF) Bidirectional Scattering Distribution Function
CBSDF EvaluateBSDF(half3 V, half3 L, BSDFData bsdfData)
{
    CBSDF cbsdf;
    ZERO_INITIALIZE(CBSDF, cbsdf);

    half3 T = bsdfData.hairStrandDirectionWS;
    half3 N = bsdfData.normalWS; // it is view facing normal

#if _USE_LIGHT_FACING_NORMAL
    // The Kajiya-Kay model has a "built-in" transmission, and the 'NdotL' is always positive.
    half cosTL = dot(T, L);
    half sinTL = sqrt(saturate(1.0 - cosTL * cosTL));
    half NdotL = sinTL; // Corresponds to the cosine w.r.t. the light-facing normal
#else
    // Double-sided Lambert.
    float NdotL = dot(N, L);
#endif

    half NdotV = dot(N, V);// preLightData.NdotV;
    half clampedNdotV = ClampNdotV(NdotV);
    half clampedNdotL = saturate(NdotL);

    half LdotV, NdotH, LdotH, invLenLV;
    GetBSDFAngle(V, L, NdotL, NdotV, LdotV, NdotH, LdotH, invLenLV);

    half3 t1 = ShiftTangent(T, N, bsdfData.specularShift);
    half3 t2 = ShiftTangent(T, N, bsdfData.secondarySpecularShift);

    half3 H = (L + V) * invLenLV;

    // Balancing energy between lobes, as well as between diffuse and specular is left to artists.
    half3 hairSpec1 = bsdfData.specularTint          * D_KajiyaKay(t1, H, bsdfData.specularExponent);
    half3 hairSpec2 = bsdfData.secondarySpecularTint * D_KajiyaKay(t2, H, bsdfData.secondarySpecularExponent);
    

    half3 F = F_Schlick(bsdfData.fresnel0, LdotH);

#if _USE_LIGHT_FACING_NORMAL
    // See "Analytic Tangent Irradiance Environment Maps for Anisotropic Surfaces".
    //cbsdf.diffR = rcp(PI * PI)* clampedNdotL;
    cbsdf.diffR = bsdfData.anisotropy * clampedNdotL; // Michael: modified multiplication to get correct brigthness in URP
    // Transmission is built into the model, and it's not exactly clear how to split it.
    cbsdf.diffT = 0;
#else
    // Double-sided Lambert.
    //cbsdf.diffR = Lambert() * clampedNdotL;
    // cbsdf.diffR = pow(clampedNdotL, 1.5);
    cbsdf.diffR = saturate(dot(N, L) + 0.5) / (1 + 0.5);
    float3 scatterColor = lerp(float3(0.992, 0.808, 0.518), bsdfData.diffuseColor, 0.5);
    cbsdf.diffR *= saturate(scatterColor + clampedNdotL);

    // cbsdf.diffR = clampedNdotL;
    // cbsdf.diffR = clampedNdotL * saturate(dot(T, L));
#endif
    // Bypass the normal map...
    half geomNdotV = dot(bsdfData.geomNormalWS, V);

    // G = NdotL * NdotV. // Michael: modified multiplication from 0.25 to 1 that match the brightness closer to the HDRP's hair specular result
    cbsdf.specR = F * (hairSpec1 + hairSpec2) * clampedNdotL * saturate(geomNdotV * FLT_MAX);

    // Yibing's and Morten's hybrid scatter model hack.
    half scatterFresnel1 = pow(saturate(-LdotV), 9.0) * pow(saturate(1.0 - geomNdotV * geomNdotV), 12.0);
    half scatterFresnel2 = saturate(PositivePow((1.0 - geomNdotV), 20.0));

    cbsdf.specT = scatterFresnel1 + bsdfData.rimTransmissionIntensity * scatterFresnel2;

    return cbsdf;
}

half3 EvaluateBSDF_Env(BSDFData bsdfData, InputData inputData, HairSurfaceData hairSurfaceData)
{
    half3 reflectVector = reflect(-inputData.viewDirectionWS, bsdfData.normalWS); // iblR
    half NoV = saturate(dot(bsdfData.normalWS, inputData.viewDirectionWS));
    half fresnelTerm = Pow4(1.0 - NoV);

    half3 indirectDiffuse = inputData.bakedGI * bsdfData.ambientOcclusion;

    // ref: HDRP ver.10.1.x - High Definition RP/Runtime/Material/Hair/Hair.hlsl #286
    // Note: For Kajiya hair we currently rely on a single cubemap sample instead of two, as in practice smoothness of both lobe aren't too far from each other.
    // and we take smoothness of the secondary lobe as it is often more rough (it is the colored one).
    half iblPerceptualRoughness = bsdfData.secondaryPerceptualRoughness;

    // Michael: We do an approximation of roughness only here, skip the GetPreIntegratedFGDGGXAndDisneyDiffuse() function for URP
    iblPerceptualRoughness *= saturate(1.2 - bsdfData.anisotropy); // constant 0.4
    half3 indirectSpecular = GlossyEnvironmentReflection(reflectVector, iblPerceptualRoughness, bsdfData.ambientOcclusion); // function in Lighting.hlsl

    // ref: HDRP ver.10.1.x - High Definition RP/Runtime/Material/Hair/Hair.hlsl #578
    // We tint the HDRI with the secondary lob specular as it is more representatative of indirect lighting on hair.
    indirectSpecular *= bsdfData.secondarySpecularTint;

    // Specular Occulsion From AO
    indirectSpecular *= bsdfData.ambientOcclusion * hairSurfaceData.vertexColor.b;

    half3 c = indirectDiffuse * bsdfData.diffuseColor;
    float surfaceReduction = 1.0 / (iblPerceptualRoughness * iblPerceptualRoughness + 1.0);

    half3 envReflection = GlossyEnvironmentReflection(reflectVector, 0.6, bsdfData.ambientOcclusion);
    float3 d_KajiyaKay = D_KajiyaKay(ShiftTangent(bsdfData.hairStrandDirectionWS, bsdfData.normalWS, 0.2 * _JitterStrength), inputData.viewDirectionWS, RoughnessToBlinnPhongSpecularExponent(0.5));
    c += surfaceReduction * indirectSpecular * lerp(bsdfData.fresnel0, bsdfData.secondaryGrazingTerm, fresnelTerm) + _ReflectanceStrength * hairSurfaceData.smoothness * envReflection * d_KajiyaKay * 0.2;

    /*
    c += surfaceReduction * indirectSpecular * lerp(bsdfData.fresnel0, bsdfData.secondaryGrazingTerm, fresnelTerm) +
            _ReflectanceStrength * 0.2 * (1 - SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, hairSurfaceData.uv).r) *
            GlossyEnvironmentReflection(reflectVector, 0.6, bsdfData.ambientOcclusion) *
            D_KajiyaKay(ShiftTangent(bsdfData.hairStrandDirectionWS, bsdfData.normalWS, 0.2 * _JitterStrength),
            inputData.viewDirectionWS, RoughnessToBlinnPhongSpecularExponent(0.5));
    */
    
    // return indirectSpecular * D_KajiyaKay(ShiftTangent(bsdfData.hairStrandDirectionWS, bsdfData.normalWS, 0.5), inputData.viewDirectionWS, RoughnessToBlinnPhongSpecularExponent(0.3));
    return c;
}

///////////////////////////////////////////////////////////////////////////////
//                      Fragment Functions                                   //
//       Used by ShaderGraph and others builtin renderers                    //
///////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////
/// PBR lighting...
////////////////////////////////////////////////////////////////////////////////
half4 UniversalFragmentPBR(InputData inputData, SurfaceData surfaceData, HairSurfaceData hairSurfaceData)
{
    #if defined(_SPECULARHIGHLIGHTS_OFF)
    bool specularHighlightsOff = true;
    #else
    bool specularHighlightsOff = false;
    #endif
    BRDFData brdfData;

    // NOTE: can modify "surfaceData"...
    InitializeBRDFData(surfaceData, brdfData);

    #if defined(DEBUG_DISPLAY)
    half4 debugColor;

    if (CanDebugOverrideOutputColor(inputData, surfaceData, brdfData, debugColor))
    {
        return debugColor;
    }
    #endif

    // Clear-coat calculation...
    BRDFData brdfDataClearCoat = CreateClearCoatBRDFData(surfaceData, brdfData);
    half4 shadowMask = CalculateShadowMask(inputData);
    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
    uint meshRenderingLayers = GetMeshRenderingLayer();
    Light mainLight = GetMainLight(inputData, shadowMask, aoFactor);

    // NOTE: We don't apply AO to the GI here because it's done in the lighting calculation below...
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI);

    BSDFData bsdfData = ConvertSurfaceDataToBSDFData(hairSurfaceData, surfaceData);
    half3 color = EvaluateBSDF_Env(bsdfData, inputData, hairSurfaceData);
    half3 diffuse = 0, specular = 0;
    half3 radiance = mainLight.color * (mainLight.distanceAttenuation * mainLight.shadowAttenuation);
    half3 transmittance = bsdfData.transmittance;

    #ifdef _LIGHT_LAYERS
    if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
        #endif
    {
        // Direct lighting
        // ref: HDRP ver.10 - High Definition RP/Runtime/Lighting/SurfaceShading.hlsl #39
        CBSDF cbsdf = EvaluateBSDF(inputData.viewDirectionWS, mainLight.direction, bsdfData);

        diffuse = (cbsdf.diffR) * radiance * surfaceData.occlusion;

        specular = (cbsdf.specR + cbsdf.specT * transmittance * hairSurfaceData.vertexColor.r) * radiance;
    }

    // additional light
    uint pixelLightCount = GetAdditionalLightsCount();
    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

    #ifdef _LIGHT_LAYERS
    if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
    #endif
    {
        CBSDF cbsdf = EvaluateBSDF(inputData.viewDirectionWS, light.direction, bsdfData);
        radiance = light.color * (light.distanceAttenuation * light.shadowAttenuation);
        diffuse += (cbsdf.diffR) * radiance;
        specular += (cbsdf.specR + cbsdf.specT * transmittance* hairSurfaceData.vertexColor.r) * radiance;
    }
    LIGHT_LOOP_END

    color += diffuse * bsdfData.diffuseColor + specular;
    color += surfaceData.emission;
    return half4(color, surfaceData.alpha);
}

HairSurfaceData BuildHairSurfaceData(InputData inputData, half4 maskMap, real3 hairStrandDirection, real4 vertexColor, float smoothness)
{
    HairSurfaceData surfaceData = (HairSurfaceData)0;

    surfaceData.transmittance = _Transmittance.rgb;
    surfaceData.rimTransmissionIntensity = 0.2;
    surfaceData.specularTint = _SpecularTint.rgb * (1-maskMap.r);
    surfaceData.specularShift = _SpecularShift + maskMap.g * _JitterStrength;
    surfaceData.secondarySmoothness = _SecondarySmoothness;
    surfaceData.secondarySpecularTint = _SecondarySpecularTint.rgb * (1-maskMap.r);
    surfaceData.secondarySpecularShift = _SecondarySpecularShift + maskMap.g * _JitterStrength;
    surfaceData.normalWS = inputData.normalWS;
    surfaceData.geomNormalWS = inputData.tangentToWorld[2];
    // surfaceData.hairStrandDirection = normalize(TransformTangentToWorld(hairStrandDirection, inputData.tangentToWorld));
    surfaceData.hairStrandDirection = hairStrandDirection;
    surfaceData.vertexColor = vertexColor;
    surfaceData.smoothness = smoothness;
    return surfaceData;
}

#endif