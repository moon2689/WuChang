Shader "Saber/Unlit/Dissolve/Dissolve Cube"
{
    Properties
    {
        [MainTexture] _BaseMap("Texture", 2D) = "gray" {}
        _GradientMap("Gradient Map", 2D) = "white" {}
        _DissolveAmount("Dissolve Amount", Range(0, 1)) = 0
        [Toggle] _AutoPlay("Auto Play?", float) = 0
        _EdgeWidth("Edge Width", Range(0.01, 1)) = 1
        _EdgeColor("Edge Color", Color) = (0,0,0,0)
        _EdgeIntensity("Edge Intensity", float) = 1
        _Spread("Spread", Range(0.1,2)) = 1
        _NoiseMap("Noise Map", 2D) = "white" {}
        _Height("Height", float) = 1
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
        Cull Off

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
            float _Spread;
            float4 _NoiseMap_ST;
            float _Height;
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
                float3 positionWS : TEXCOORD1;
                float3 centerPosWS : TEXCOORD2;

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
                output.centerPosWS = TransformObjectToWorld(float3(0,0,0));

                return output;
            }
            
            // Remap from range [oldMin, oldMax] to [newMin, newMax]
            float Remap(float value, float oldMin, float oldMax, float newMin, float newMax)
            {
                return newMin + (value - oldMin) / (oldMax - oldMin) * (newMax - newMin);
            }
            
            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half4 color;
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half4 gradientMap = SAMPLE_TEXTURE2D(_GradientMap, sampler_GradientMap, input.uv);
                half4 noiseMap = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, input.uv * _NoiseMap_ST.xy + _NoiseMap_ST.zw * _Time.x);

                float changeAmount;
                #if _AUTOPLAY_ON
                changeAmount  = frac(_Time.x * 2);
                #else
                changeAmount = _DissolveAmount;
                #endif
                
                float positionRate = -(input.positionWS.y - input.centerPosWS.y) / _Height;
                //positionRate = -distance(input.positionWS, input.centerPosWS)/1.5;
                float gradient = positionRate - Remap(changeAmount, 0, 1, -_Spread, 1) / _Spread;
                gradient = gradient * 2 - noiseMap.r * 0.2;
                float edgeWeight = saturate(1 - 2 * distance(gradient, 0.5) / _EdgeWidth);
                
                color.rgb = lerp(baseMap.rgb, _EdgeColor.rgb * _EdgeIntensity, edgeWeight);
                
                float clipValue = step(0.5, gradient) * baseMap.a;
                color.a = clipValue;
                clip(clipValue - 0.5);
                
                //color.rgb = positionRate;
                
                return color;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}