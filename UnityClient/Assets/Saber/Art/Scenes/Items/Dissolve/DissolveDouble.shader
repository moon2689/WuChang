Shader "Saber/Unlit/Dissolve/Dissolve Double"
{
    Properties
    {
        [MainTexture] _BaseMap("Texture", 2D) = "white" {}
        _GradientMap("Gradient Map", 2D) = "white" {}
        _DissolveAmount("Dissolve Amount", Range(0, 1)) = 0
        [Toggle] _AutoPlay("Auto Play?", float) = 0
        _EdgeWidth("Edge Width", Range(0.01, 1)) = 1
        _EdgeColor("Edge Color", Color) = (0,0,0,0)
        _EdgeIntensity("Edge Intensity", float) = 1
        
        _NoiseMap("Noise Map", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "AlphaTest"
        }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #pragma multi_compile  _ _AUTOPLAY_ON
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_GradientMap);
            SAMPLER(sampler_GradientMap);
            TEXTURE2D(_EdgeRamp);
            SAMPLER(sampler_EdgeRamp);
            TEXTURE2D(_NoiseMap);
            SAMPLER(sampler_NoiseMap);

            CBUFFER_START(UnityPerMaterial)
            float _DissolveAmount;
            float _EdgeWidth;
            half4 _EdgeColor;
            float _EdgeIntensity;
            float4 _NoiseMap_ST;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings UnlitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;

                return output;
            }
            
            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half4 color;
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half4 gradientMap = SAMPLE_TEXTURE2D(_GradientMap, sampler_GradientMap, input.uv);
                half4 noiseMap = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, input.uv * _NoiseMap_ST.xy + _NoiseMap_ST.zw * _Time.x);
                
                color.rgb = baseMap.rgb;

                float changeAmount;
                #if _AUTOPLAY_ON
                changeAmount  = frac(_Time.x * 2);
                #else
                changeAmount = _DissolveAmount;
                #endif
                
                float gradient = gradientMap.r - (changeAmount * 2 - 1);
                gradient = gradient * 2 - noiseMap.r;
                float edgeWeight = saturate(1 - 2 * distance(gradient, 0.5) / _EdgeWidth);
                color.rgb += edgeWeight * _EdgeColor * _EdgeIntensity;
                
                float clipValue = step(0.5, gradient) * baseMap.a;
                color.a = clipValue;
                clip(clipValue - 0.5);
                
                return color;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}