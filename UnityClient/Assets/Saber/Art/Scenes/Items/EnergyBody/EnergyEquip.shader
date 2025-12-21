Shader "Saber/Unlit/Energy/Energy Equip"
{
    Properties
    {
        [MainTexture] _BaseMap("Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1, 1, 1, 1)
        _NormalMap("Normal", 2D) = "bump" {}
        _MaskMap("Mask", 2D) = "black" {}
        _EnvCube("Env Cube", Cube) = "black" {}
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

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            TEXTURE2D(_MaskMap);
            SAMPLER(sampler_MaskMap);
            TEXTURECUBE(_EnvCube);
            SAMPLER(sampler_EnvCube);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
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
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 tangentWS : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings UnlitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                
                VertexNormalInputs normalData = GetVertexNormalInputs(input.normalOS, input.positionOS);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = normalData.normalWS;
                output.tangentWS = normalData.tangentWS;
                output.bitangentWS = normalData.bitangentWS;

                return output;
            }

            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);
                
                float3 NModel = normalize(input.normalWS);
                float3 V = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float3 T = normalize(input.tangentWS);
                float3 B = normalize(input.bitangentWS);
                
                half4 normalMap = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv);
                float3 normalTS = UnpackNormal(normalMap);
                float3x3 TBN = float3x3(T,B,NModel);
                float3 N = TransformTangentToWorld(normalTS, TBN, true);
                
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half4 maskMap = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, input.uv);
                
                half4 color;
                
                float roughness = maskMap.g;
                float mipLevel = roughness * (1.7 - 0.7 * roughness) * 6;
                
                half3 specColor = baseMap.rgb * _BaseColor.rgb;
                float3 reflectDir = reflect(-V, N);
                float4 envCubeMap = SAMPLE_TEXTURECUBE_LOD(_EnvCube, sampler_EnvCube, reflectDir, mipLevel);

                color.rgb = specColor * envCubeMap.rgb;
                color.a = 1;
                return color;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}