Shader "Saber/Unlit/Slime/Slime Body"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _EmissionMap("Emission Map", 2D) = "black" {}
        _MatcapMap("Matcap Map", 2D) = "black" {}
        _MatcapIntensity("Matcap Intensity", float) = 1
        _MatcapNormalNoiseMap("Matcap Normal Noise Map", 2D) = "bump" {}
        _FlowNoiseTilling("Flow Noise Tilling", Vector) = (1,1,1,1)
        _FlowNoiseSpeed("Flow Noise Speed", Vector) = (0,0,0,0)
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
            //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);
            TEXTURE2D(_MatcapMap);
            SAMPLER(sampler_MatcapMap);
            TEXTURE2D(_MatcapNormalNoiseMap);
            SAMPLER(sampler_MatcapNormalNoiseMap);
            
            CBUFFER_START(UnityPerMaterial)
                float _MatcapIntensity;
            float3 _FlowNoiseTilling;
            float3 _FlowNoiseSpeed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 tangentOS : TANGENT;
                float3 normalOS : NORMAL;

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
                
                VertexNormalInputs normalData = GetVertexNormalInputs(input.normalOS);
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
                half4 emissionMap = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv);

                // 三平面映射
                float3 posOffset = input.positionWS - TransformObjectToWorld(float3(0,0,0));
                float3 uvPosOffset = posOffset * _FlowNoiseTilling.xyz + _Time.x * _FlowNoiseSpeed.xyz;
                half4 noiseXY = SAMPLE_TEXTURE2D(_MatcapNormalNoiseMap, sampler_MatcapNormalNoiseMap, uvPosOffset.xy);
                half4 noiseXZ = SAMPLE_TEXTURE2D(_MatcapNormalNoiseMap, sampler_MatcapNormalNoiseMap, uvPosOffset.xz);
                half4 noiseYZ = SAMPLE_TEXTURE2D(_MatcapNormalNoiseMap, sampler_MatcapNormalNoiseMap, uvPosOffset.yz);
                float3 powN = pow(abs(N), 5);
                float3 weightN = powN / (powN.x + powN.y + powN.z);
                half4 noiseMatcap = noiseXY * weightN.z + noiseXZ * weightN.y + noiseYZ * weightN.x;
                
                // matcap
                float3 NMatcap = TransformTangentToWorld(UnpackNormal(noiseMatcap), TBN, true);
                float3 NVS = TransformWorldToViewNormal(NMatcap, true);
                float3 fixNVS = NVS * 0.5 + 0.5;
                half4 matcapMap = SAMPLE_TEXTURE2D(_MatcapMap, sampler_MatcapMap, fixNVS.xy);
                
                half4 color;
                color.rgb = baseMap.rgb * matcapMap.rgb * _MatcapIntensity;
                
                // emission
                color.rgb += emissionMap.rgb;
                
                return color;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}