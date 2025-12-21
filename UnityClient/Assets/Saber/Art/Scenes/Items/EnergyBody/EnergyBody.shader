Shader "Saber/Unlit/Energy/Energy Body"
{
    Properties
    {
        _NormalMap("Normal", 2D) = "bump" {}
        _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower("Rim Power", float) = 1
        _RimScale("Rim Scale", float) = 1
        _RimBias("Rim Bias", float) = 0
        _StarMap("Star Map", 2D) = "black" {}
        _StarDistoration("Star Distoration", float) = 0.05
        _StarIntensity("Star Intensity", float) = 1
        _StarPower("Star Power", float) = 1
        _FlowMap("Flow Map", 2D) = "black" {}
        [HDR] _FlowLightColor("Flow Light Color", Color) = (0,0,0,0)
        _FlowLightDistoration("Flow Light Distoration", float) = 0.3
        _FlowRimPower("Flow Rim Power", float) = 1
        _FlowRimScale("Flow Rim Scale", float) = 1
        _FlowRimBias("Flow Rim Bias", float) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            TEXTURE2D(_StarMap);
            SAMPLER(sampler_StarMap);
            TEXTURE2D(_FlowMap);
            SAMPLER(sampler_FlowMap);
            
            CBUFFER_START(UnityPerMaterial)
            half4 _RimColor;
            float _RimPower;
            float _RimScale;
            float _RimBias;
            float4 _StarMap_ST;
            float _StarDistoration;
            float _StarIntensity;
            float _StarPower;
            float4 _FlowMap_ST;
            float _FlowLightDistoration;
            half4 _FlowLightColor;
            float _FlowRimPower;
            float _FlowRimScale;
            float _FlowRimBias;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD1;
                float3 tangentWS : TEXCOORD2;
                float3 bitangentWS : TEXCOORD3;
                float3 positionWS : TEXCOORD4;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings UnlitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexNormalInputs data = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                
                output.normalWS = data.normalWS;
                output.tangentWS = data.tangentWS;
                output.bitangentWS = data.bitangentWS;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);

                return output;
            }

            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);
                
                float3 NModel = normalize(input.normalWS);
                float3 T = normalize(input.tangentWS);
                float3 B = normalize(input.bitangentWS);
                
                half4 normalMap = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv);
                float3 normalTS = UnpackNormal(normalMap);
                float3x3 TBN = float3x3(T,B,NModel);
                float3 N = TransformTangentToWorld(normalTS, TBN, true);
                
                float3 V = GetWorldSpaceNormalizeViewDir(input.positionWS);
                
                float NoV = dot(N,V);
                float rim = 1 - saturate(NoV);
                
                // rim
                half4 color = (pow(rim, _RimPower) * _RimScale + _RimBias) * _RimColor;
                
                // flow
                float2 uvFlow = input.uv * _FlowMap_ST.xy + _FlowMap_ST.zw * _Time.x + (NoV * 0.5 + 0.5) * _FlowLightDistoration;
                half4 flowMap = SAMPLE_TEXTURE2D(_FlowMap, sampler_FlowMap, uvFlow);
                half4 flowLightColor = flowMap.r * (pow(rim, _FlowRimPower) * _FlowRimScale + _FlowRimBias) * _FlowLightColor;
                color += flowLightColor;
                
                // star
                float3 offsetPosVS = TransformWorldToView(input.positionWS) - mul(UNITY_MATRIX_MV, float4(0,0,0,1)).xyz;
                float2 uvStar = offsetPosVS.xy * _StarMap_ST.xy + _StarMap_ST.zw + N.xy * _StarDistoration;
                half4 starMap = SAMPLE_TEXTURE2D(_StarMap, sampler_StarMap, uvStar);
                half4 starColor = starMap * flowMap.r; 
                color += starColor;
                color += pow(starColor * _StarIntensity, _StarPower);
                
                return color;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}