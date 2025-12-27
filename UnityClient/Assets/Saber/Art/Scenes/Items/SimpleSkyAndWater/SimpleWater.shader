Shader "Saber/Unlit/Simple Water"
{
    Properties
    {
        //_ReflectionTex("Reflection Texture", 2D) = "black" {}
        
        _NormalMap("Normal Map", 2D) = "bump" {}
        _WaveStrength1("Wave Strength 1", Vector) = (1,1,0,0)
        _WaveStrength2("Wave Strength 2", Vector) = (1,1,0,0)
        _NormalScale("Normal Scale", Range(0, 2)) = 1
        _ReflectionNoise("Reflection Noise", Range(0,1)) = 1
        [HDR] _SpecularTint("Specular Tint", Color) = (1,1,1,1)
        _Smoothness("Smoothness", Range(0.01, 1)) = 0.5
        _SpecularFadeStart("Specular Fade Start", float) = 50
        _SpecularFadeEnd("Specular Fade End", float) = 100
        
        [Header(UnderWater)]
        _UnderWaterMap("Under Water Map", 2D) = "black" {}
        _RimPower("Rim Power", float) = 1
        _UnderWaterDepth("Under Water depth", float) = -1
        
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_ReflectionTex);
            SAMPLER(sampler_ReflectionTex);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            TEXTURE2D(_UnderWaterMap);
            SAMPLER(sampler_UnderWaterMap);

            CBUFFER_START(UnityPerMaterial)
            float _ReflectionNoise;
            float4 _WaveStrength1;
            float4 _WaveStrength2;
            float _NormalScale;
            half4 _SpecularTint;
            float _Smoothness;
            float _SpecularFadeStart;
            float _SpecularFadeEnd;
            float4 _UnderWaterMap_ST;
            float _RimPower;
            float _UnderWaterDepth;
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
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                
                VertexNormalInputs normalData = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.normalWS = normalData.normalWS;
                output.tangentWS = normalData.tangentWS;
                output.bitangentWS = normalData.bitangentWS;
                
                return output;
            }

            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);
                
                float3 NModel = normalize(input.normalWS);
                float3 T = normalize(input.tangentWS);
                float3 B = normalize(input.bitangentWS);

                // normal
                float2 uvNormalMap1 = input.uv * _WaveStrength1.xy + _Time.x * _WaveStrength1.zw;
                float2 uvNormalMap2 = input.uv * _WaveStrength2.xy + _Time.x * _WaveStrength2.zw;
                half4 normalMap1 = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uvNormalMap1);
                half4 normalMap2 =  SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uvNormalMap2);
                float3 normalTS1 = UnpackNormalScale(normalMap1, _NormalScale);
                float3 normalTS2 = UnpackNormalScale(normalMap2, _NormalScale);
                float3 finalNormalTS = lerp(normalTS1, normalTS2, 0.5);
                float3x3 TBN = float3x3(T,B,NModel);
                float3 N = TransformTangentToWorld(finalNormalTS, TBN, true);
                
                float3 V = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float NoV = saturate(dot(N,V));
                float rim = 1 - pow(NoV, _RimPower);
                
                // 反射
                float2 screenUV = GetNormalizedScreenSpaceUV(input.positionCS);
                float2 uvRef = screenUV + N.xz * rcp(input.positionCS.w + 1) * _ReflectionNoise;
                half4 refCol = SAMPLE_TEXTURE2D(_ReflectionTex, sampler_ReflectionTex, uvRef);

                // 高光
                Light mainLight = GetMainLight();
                float3 L = normalize(mainLight.direction);
                float3 H = normalize(V + L);
                float NoH = max(0, dot(N,H));
                half3 specularLighting = pow(NoH, _Smoothness * 256) * mainLight.color * _SpecularTint.rgb;
                float specularAtten = saturate((_SpecularFadeEnd - input.positionCS.w) / (_SpecularFadeEnd - _SpecularFadeStart));
                specularLighting *= specularAtten;
                
                // under water
                float3 viewDirTS = TransformWorldToTangentDir(V, TBN);
                float2 parallaxOffset = _UnderWaterDepth * viewDirTS.xy / viewDirTS.z;
                float2 uvUnderWater = input.positionWS.xz * _UnderWaterMap_ST.xy + _UnderWaterMap_ST.zw * N.xz + parallaxOffset;
                half4 underWaterMap = SAMPLE_TEXTURE2D(_UnderWaterMap, sampler_UnderWaterMap, uvUnderWater);

                // final color
                half4 color;
                color.rgb = lerp(underWaterMap.rgb, refCol.rgb, rim) + specularLighting;
                color.a = 1;
                
                return color;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}