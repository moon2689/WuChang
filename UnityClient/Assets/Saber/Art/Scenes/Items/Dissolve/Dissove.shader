Shader "Saber/Unlit/Dissolve"
{
    Properties
    {
        _MainTex ("Noise Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _EdgeColor("Edge Color", Color) = (0,0,0,0)
        _EdgeColorIntensity("Edge Color Intensity", float) = 1
        _EdgeWidth("Edge Width", Range(0.1, 2)) = 0.5
        _Spread("Spread", Range(0, 1)) = 0.3

        _ChangeAmount ("Change Amount", Range(0, 1)) = 0
        [Toggle] _AutoPlay ("Auto Play", float) = 0

        [KeywordEnum(Easy,Double,DoubleRamp)] _DissolveStyle("Dissolve Style", float) = 0

        _DoubleNoiseTex ("Double Noise Texture", 2D) = "white" {}
        _EdgeDistoration("Edge Distoration", Range(0, 1)) = 0.5
        _DoubleRampTex ("Double Ramp Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "AlphaTest"
        }
        LOD 0

        Pass
        {
            //ZWrite Off

            HLSLPROGRAM
            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma multi_compile _ _AUTOPLAY_ON
            #pragma multi_compile _ _DISSOLVESTYLE_EASY _DISSOLVESTYLE_DOUBLE _DISSOLVESTYLE_DOUBLERAMP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


            CBUFFER_START(UnityPerMaterial)
            float _ChangeAmount;
            half4 _EdgeColor;
            float _EdgeColorIntensity;
            float _EdgeWidth;
            float _Spread;
            float _EdgeDistoration;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);
            TEXTURE2D(_DoubleNoiseTex);
            SAMPLER(sampler_DoubleNoiseTex);
            TEXTURE2D(_DoubleRampTex);
            SAMPLER(sampler_DoubleRampTex);


            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };


            Varyings Vertex(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.texcoord;

                return output;
            }

            // 标准remap函数
            float Remap(float value, float inMin, float inMax, float outMin, float outMax)
            {
                return outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 noiseTex = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, input.uv);

                half4 color;

                float changeAmount;
                #if _AUTOPLAY_ON
                changeAmount = frac(_Time.x * 5);
                #else
                changeAmount = _ChangeAmount;
                #endif
                float remapAmount = Remap(changeAmount, 0, 1, -_Spread, 1);
                remapAmount /= _Spread;

                #if _DISSOLVESTYLE_EASY
                float gradient = noiseTex.r - remapAmount;
                
                float edge = distance(gradient, 0.5);
                edge = saturate(1 - edge / _EdgeWidth);
                
                color.rgb = mainTex.rgb * lerp(1, _EdgeColor.rgb * _EdgeColorIntensity, edge);
                color.a = step(0.5, gradient) * mainTex.a;
                
                clip(color.a - 0.5);
                #elif _DISSOLVESTYLE_DOUBLE
                half4 doubleNoiseTex = SAMPLE_TEXTURE2D(_DoubleNoiseTex, sampler_DoubleNoiseTex, input.uv);
                
                float gradient = doubleNoiseTex.r - noiseTex.r * _EdgeDistoration - remapAmount;
                color.a = step(0.5, gradient) * mainTex.a;

                float edge = distance(gradient, 0.5);
                edge = saturate(1 - edge / _EdgeWidth);
                
                color.rgb = mainTex.rgb * lerp(1, _EdgeColor.rgb * _EdgeColorIntensity, edge);
                
                clip(color.a - 0.5);
                #elif _DISSOLVESTYLE_DOUBLERAMP
                half4 doubleNoiseTex = SAMPLE_TEXTURE2D(_DoubleNoiseTex, sampler_DoubleNoiseTex, input.uv);
                
                float gradient = doubleNoiseTex.r - noiseTex.r * _EdgeDistoration - remapAmount;
                color.a = step(0.5, gradient) * mainTex.a;

                float edge = distance(gradient, 0.5);
                edge = saturate(1 - edge / _EdgeWidth);

                float rampU = saturate(1 - edge);
                half3 edgeColor = SAMPLE_TEXTURE2D(_DoubleRampTex, sampler_DoubleRampTex, float2(rampU, 0.5));
                color.rgb = mainTex.rgb * lerp(1, edgeColor * _EdgeColorIntensity, edge);
                
                clip(color.a - 0.5);
                //color.rgb = rampU;
                #endif


                return color;
            }
            ENDHLSL
        }
    }
}