Shader "Saber/Unlit/Crystal/AhriTail"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _RefractionCube("Refraction Cube", Cube) = "white" {}
        _ReflectionCube("Reflection Cube", Cube) = "white" {}
        _RefractionIntensity("Refraction Intensity", float) = 1
        _ReflectionIntensity("Reflection Intensity", float) = 1
        _RimPower("Rim Power", float) = 5
        _RimScale("Rim Scale", float) = 1
        _RimBias("Rim Bias", float) = 0
        _RimColor("Rim Color", Color) = (0,0,0,0)
        _Ramp("Ramp Map", 2D) = "black" {}
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
            //Blend SrcAlpha One
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
            TEXTURE2D(_Ramp);
            SAMPLER(sampler_Ramp);

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
                half4 rampMap = SAMPLE_TEXTURE2D(_Ramp, sampler_Ramp, input.uv);

                float rim = 1 - saturate(dot(N, V));

                color = rampMap * _Color * refractionCube * _RefractionIntensity * (1 + reflectionCube * rim * _ReflectionIntensity);
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
            Blend SrcAlpha One
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURECUBE(_RefractionCube);
            SAMPLER(sampler_RefractionCube);
            TEXTURECUBE(_ReflectionCube);
            SAMPLER(sampler_ReflectionCube);
            TEXTURE2D(_Ramp);
            SAMPLER(sampler_Ramp);

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _RefractionIntensity;
                float _ReflectionIntensity;
                float _RimPower;
                float _RimScale;
                float _RimBias;
                half4 _RimColor;
                half _Alpha;
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
                half4 rampMap = SAMPLE_TEXTURE2D(_Ramp, sampler_Ramp, input.uv);

                float rim = 1 - saturate(dot(N, V));

                float rimWeight = pow(rim, _RimPower) * _RimScale + _RimBias;

                color = refractionCube * _RefractionIntensity * _Color * rampMap;
                color *= (1 + color) * reflectionCube * rim * _ReflectionIntensity * (1 + rimWeight * _RimColor);
                color.a = _Alpha;
                return color;
            }
            ENDHLSL
        }

    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}