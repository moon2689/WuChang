Shader "Saber/Unlit/Crystal/Crystal"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _NormalMap("Normal", 2D) = "bump"
        
        _InnerTexture("Inner Texture", 2D) = "black"
        _InnerTextureDistoration("Inner Texture Distoration", float) = 0.1
        
        _EmissionMap("Emission", 2D) = "black"
        
        _RefractionCube("Refraction Cube", Cube) = "white" {}
        _ReflectionCube("Reflection Cube", Cube) = "white" {}
        _RefractionIntensity("Refraction Intensity", float) = 1
        _ReflectionIntensity("Reflection Intensity", float) = 1
        
        _RimPower("Rim Power", float) = 5
        _RimScale("Rim Scale", float) = 1
        _RimBias("Rim Bias", float) = 0
        _RimColor("Rim Color", Color) = (0,0,0,0)
        
        _Alpha("Alpha", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }
        LOD 100
        
        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            //Blend SrcAlpha OneMinusSrcAlpha
            Cull Front
            ZWrite Off
            
            HLSLPROGRAM
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURECUBE(_RefractionCube);
            SAMPLER(sampler_RefractionCube);
            TEXTURECUBE(_ReflectionCube);
            SAMPLER(sampler_ReflectionCube);

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _RefractionIntensity;
                float _ReflectionIntensity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;

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
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);

                return output;
            }

            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half4 color;

                float3 V = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float3 N = normalize(input.normalWS);
                float3 reflectionDir = reflect(-V, N);
                half4 refractionCube = SAMPLE_TEXTURECUBE(_RefractionCube, sampler_RefractionCube, reflectionDir);
                half4 reflectionCube = SAMPLE_TEXTURECUBE(_ReflectionCube, sampler_ReflectionCube, reflectionDir);

                float rim = 1 - saturate(dot(N, V));

                color = refractionCube * _RefractionIntensity * _Color;
                color *= (1 + reflectionCube * rim * _ReflectionIntensity);
                return color;
            }
            ENDHLSL
        }


        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForwardOnly"
            }
            Blend One OneMinusSrcAlpha
            ZWrite Off
            
            HLSLPROGRAM
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURECUBE(_RefractionCube);
            SAMPLER(sampler_RefractionCube);
            TEXTURECUBE(_ReflectionCube);
            SAMPLER(sampler_ReflectionCube);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            TEXTURE2D(_InnerTexture);
            SAMPLER(sampler_InnerTexture);
            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _RefractionIntensity;
                float _ReflectionIntensity;
                float _RimPower;
                float _RimScale;
                float _RimBias;
                half _Alpha;
                half4 _RimColor;
                float4 _InnerTexture_ST;
                float _InnerTextureDistoration;
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
                float4 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float3 tangentWS : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings UnlitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                
                VertexNormalInputs data = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv.xy = input.uv;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = data.normalWS;
                output.tangentWS = data.tangentWS;
                output.bitangentWS = data.bitangentWS;
                
                float3 positionVS = TransformWorldToView(output.positionWS);
                float3 centerPosVS = TransformWorldToView(float3(0,0,0));
                float3 offsetPosVS = positionVS - centerPosVS;
                output.uv.zw = offsetPosVS.xy * _InnerTexture_ST.xy + _InnerTexture_ST.zw;

                return output;
            }

            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float3 NModel = normalize(input.normalWS);
                float3 T = normalize(input.tangentWS);
                float3 B = normalize(input.bitangentWS);
                
                half4 normalMap = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv.xy);
                float3 normalTS = UnpackNormal(normalMap);
                float3x3 TBN = float3x3(T,B,NModel);
                float3 N = TransformTangentToWorld(normalTS, TBN, true);

                float3 V = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float3 reflectionDir = reflect(-V, N);
                half4 refractionCube = SAMPLE_TEXTURECUBE(_RefractionCube, sampler_RefractionCube, reflectionDir);
                half4 reflectionCube = SAMPLE_TEXTURECUBE(_ReflectionCube, sampler_ReflectionCube, reflectionDir);
                half4 innerMap = SAMPLE_TEXTURE2D(_InnerTexture, sampler_InnerTexture, input.uv.zw + N.xy * _InnerTextureDistoration * 0.1);
                half4 emissionMap = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv.xy);

                float NoV = saturate(dot(N, V));
                float rim = 1 - NoV;

                float rimWeight = pow(rim, _RimPower) * _RimScale + _RimBias;
                half3 innerColor = innerMap.rgb;

                half4 color;
                color = refractionCube * _RefractionIntensity * _Color;
                color *= (1 + reflectionCube * rim * _ReflectionIntensity) * (1 + rimWeight * _RimColor);
                color.rgb = lerp(color.rgb, innerColor, NoV);
                color.rgb += color.rgb * emissionMap;
                color.a = _Alpha;
                return color;
            }
            ENDHLSL
        }

    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}