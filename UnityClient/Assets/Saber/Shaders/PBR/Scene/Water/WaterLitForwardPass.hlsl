#ifndef UNIVERSAL_FORWARD_LIT_PASS_INCLUDED
#define UNIVERSAL_FORWARD_LIT_PASS_INCLUDED

#include "WaterLitLighting.hlsl"


///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Used in Standard (Physically Based) shader
Varyings LitPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

    // normalWS and tangentWS already normalize.
    // this is required to avoid skewing the direction during interpolation
    // also required for per-vertex lighting and SH evaluation
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    half fogFactor = 0;
    #if !defined(_FOG_FRAGMENT)
        fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
    #endif

    output.uv = input.texcoord;//TRANSFORM_TEX(input.texcoord, _BaseMap);

    // already normalized from normal transform to WS.
    output.normalWS = normalInput.normalWS;
    real sign = input.tangentOS.w * GetOddNegativeScale();
    half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
    output.tangentWS = tangentWS;

    OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);
    
    output.positionWS = vertexInput.positionWS;
    output.positionCS = vertexInput.positionCS;

    float pixelDepth = -TransformWorldToView(vertexInput.positionWS).z;
    output.someFactor = half4(fogFactor, pixelDepth, 0, 0);

    // float4 screenPos = ComputeScreenPos(vertexInput.positionCS);
    // screenPos.z = pixelDepth;
    // output.screenPos = screenPos;

    return output;
}

InputData InitializeInputData(Varyings input, half3 normalTS, float2 screenUV)
{
    InputData inputData = (InputData)0;

    inputData.positionWS = input.positionWS;

    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
    
    float sgn = input.tangentWS.w;      // should be either +1 or -1
    float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
    half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);
    inputData.tangentToWorld = tangentToWorld;
    inputData.normalWS = TransformTangentToWorld(normalTS, tangentToWorld);
    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    
    inputData.viewDirectionWS = viewDirWS;
    inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.someFactor.x);
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
    inputData.normalizedScreenSpaceUV = screenUV;//GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);
    return inputData;
}

half4 LitPassFragment(Varyings input) : SV_Target0
{
    UNITY_SETUP_INSTANCE_ID(input);

    WaterBRDFData waterBRDF;
    SurfaceData surfaceData = InitializeStandardLitSurfaceData(input, waterBRDF);
    InputData inputData = InitializeInputData(input, surfaceData.normalTS, waterBRDF.screenUV);
    half4 color = UniversalFragmentPBR(inputData, surfaceData, waterBRDF);
    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    return color;
}

#endif
