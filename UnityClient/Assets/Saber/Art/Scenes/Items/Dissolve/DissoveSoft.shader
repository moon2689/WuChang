Shader "Saber/Unlit/Dissolve Soft"
{
    Properties
    {
        _MainTex ("Noise Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _EdgeColor("Edge Color", Color) = (0,0,0,0)
        _EdgeColorIntensity("Edge Color Intensity", float) = 1
        _EdgeWidth("Edge Width", Range(0.1, 2)) = 0.5
        _Softness("Softness", Range(0, 0.5)) = 0.3

        _ChangeAmount ("Change Amount", Range(0, 1)) = 0
        [Toggle] _AutoPlay ("Auto Play", float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }
        LOD 0

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
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

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


            CBUFFER_START(UnityPerMaterial)
            float _ChangeAmount;
            half4 _EdgeColor;
            float _EdgeWidth;
            float _Softness;
            float _EdgeColorIntensity;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

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

                float gradient = noiseTex.r - (changeAmount * 2 - 1);
                
                float edge = distance(gradient, _Softness);
                edge = saturate(1 - edge / _EdgeWidth);
                color.rgb = mainTex.rgb * lerp(1, _EdgeColor.rgb * _EdgeColorIntensity, edge);

                color.a = smoothstep(_Softness, 0.5, gradient) * mainTex.a;

                return color;
            }
            ENDHLSL
        }
    }
}