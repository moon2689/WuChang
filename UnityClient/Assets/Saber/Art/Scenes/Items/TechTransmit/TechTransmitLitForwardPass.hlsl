#ifndef UNIVERSAL_FORWARD_LIT_PASS_INCLUDED
#define UNIVERSAL_FORWARD_LIT_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#if defined(LOD_FADE_CROSSFADE)
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
#endif
#include "../TheBookOfShaders/Noise.hlsl"


// keep this file in sync with LitGBufferPass.hlsl

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 texcoord : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    float3 normalWS : TEXCOORD2;
    half3 tangentWS : TEXCOORD3; // xyz: tangent, w: sign
    half4 bitangentWS : TEXCOORD4; // xyz: bitangent, w: fog factor
    DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 5);
    float3 pivotPosWS : TEXCOORD6;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

InputData InitializeInputData(Varyings input, half3 normalTS)
{
    InputData inputData = (InputData)0;

    inputData.positionWS = input.positionWS;

    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
    float3 bitangent = input.bitangentWS.xyz;
    half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);

    inputData.tangentToWorld = tangentToWorld;
    inputData.normalWS = TransformTangentToWorld(normalTS, tangentToWorld);

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    inputData.viewDirectionWS = viewDirWS;

    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
    inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.bitangentWS.w);

    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);

    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);

    return inputData;
}

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Used in Standard (Physically Based) shader
Varyings LitPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

    // normalWS and tangentWS already normalize.
    // this is required to avoid skewing the direction during interpolation
    // also required for per-vertex lighting and SH evaluation
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    half fogFactor = 0;
    #if !defined(_FOG_FRAGMENT)
    fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
    #endif

    output.uv = input.texcoord;

    // already normalized from normal transform to WS.
    output.normalWS = normalInput.normalWS;
    output.tangentWS = normalInput.tangentWS;
    output.bitangentWS.xyz = normalInput.bitangentWS;
    output.bitangentWS.w = fogFactor;

    OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

    output.positionWS = vertexInput.positionWS;
    output.positionCS = vertexInput.positionCS;
    
    output.pivotPosWS = TransformObjectToWorld(float3(0,0,0));

    return output;
}

half3 Dissolve(Varyings input)
{
    float dissolveAmount;
    #if _AUTOPLAY_ON
    dissolveAmount = frac(_Time.x * 2);
    #else
    dissolveAmount = _DissolveAmount;
    #endif
    
    float noise = snoise(input.positionWS * _DissolveNoiseScale) * 0.5 + 0.5;
    
    float offsetPosY = input.pivotPosWS.y - input.positionWS.y;
    float dissolveValue = _DissolveAmountOffset + offsetPosY - (dissolveAmount * 2 - 1);
    dissolveValue -= noise * 0.3;
    dissolveValue /= _DissolveAmountSpread;
    
    clip(dissolveValue - 0.5);
    
    float edgeValue = 1 - saturate(distance(dissolveValue, 0.5) / _EdgeWidth);
    return edgeValue * _EdgeColor.rgb;
}

// Used in Standard (Physically Based) shader
half4 LitPassFragment(
    Varyings input
    #ifdef _WRITE_RENDERING_LAYERS
    , out float4 outRenderingLayers : SV_Target1
    #endif
) : SV_Target0
{
    UNITY_SETUP_INSTANCE_ID(input);

    SurfaceData surfaceData = InitializeStandardLitSurfaceData(input.uv);

    #ifdef LOD_FADE_CROSSFADE
    LODFadeCrossFade(input.positionCS);
    #endif

    InputData inputData = InitializeInputData(input, surfaceData.normalTS);
    SETUP_DEBUG_TEXTURE_DATA(inputData, input.uv, _BaseMap);

    #ifdef _DBUFFER
    ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
    #endif

    half4 color = UniversalFragmentPBR(inputData, surfaceData);
    color.rgb = MixFog(color.rgb, inputData.fogCoord);

    #ifdef _WRITE_RENDERING_LAYERS
    uint renderingLayers = GetMeshRenderingLayer();
    outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
    #endif

    half3 edgeColor = Dissolve(input);
    color.rgb += edgeColor;

    return color;
}

#endif