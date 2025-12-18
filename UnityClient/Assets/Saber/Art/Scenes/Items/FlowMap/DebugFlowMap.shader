Shader "Saber/Unlit/Debug Flow Map"
{
    Properties
    {
		_Noise("Noise", 2D) = "white" {}
		_FlowMap("FlowMap", 2D) = "white" {}
		_NoiseStrength("NoiseStrength", Vector) = (0,0,0,0)
		_NoiseSpeed("NoiseSpeed", Float) = 0
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

            TEXTURE2D(_Noise);
            SAMPLER(sampler_Noise);
            TEXTURE2D(_FlowMap);
            SAMPLER(sampler_FlowMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _Noise_ST;
                float4 _FlowMap_ST;
                float _NoiseSpeed;
                float2 _NoiseStrength;
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
            
            half4 Flow(float2 uvNoise, TEXTURE2D_PARAM(noise,samplernoise),float2 flowDir, float2 flowStrength, float flowSpeed)
            {
                float2 fixedFlowDir = 0.5 - flowDir.rg;
                float mulTime = _Time.y * flowSpeed;
                float fracMulTime = frac(mulTime);
                
                float2 uvNoise1 = uvNoise + fixedFlowDir * fracMulTime * flowStrength;
                half4 noiseMap1 = SAMPLE_TEXTURE2D(noise, samplernoise, uvNoise1);
                
                float2 uvNoise2 = uvNoise + fixedFlowDir * frac(mulTime + 0.5) * flowStrength;
                half4 noiseMap2 = SAMPLE_TEXTURE2D(noise, samplernoise, uvNoise2);
                
                float weight = abs(fracMulTime * 2 - 1);
                half4 color = lerp(noiseMap1, noiseMap2, weight);
                return color;
            }
            
            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);
                
                /*
                float2 uv_Noise = input.uv * _Noise_ST.xy + _Noise_ST.zw;
			    float2 uv_FlowMap = input.uv * _FlowMap_ST.xy + _FlowMap_ST.zw;
			    float4 flowMap = SAMPLE_TEXTURE2D( _FlowMap, sampler_FlowMap, uv_FlowMap );
			    float2 flowDir = 0.5 - flowMap.rg;
			    float mulTime = _Time.y * _NoiseSpeed;
			    float fracMulTime = frac(mulTime);
                
                float2 uvNoise1 = uv_Noise + flowDir * fracMulTime * _NoiseStrength;
                half4 noiseMap1 = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, uvNoise1);
                
                float2 uvNoise2 = uv_Noise + flowDir * _NoiseStrength * frac(mulTime + 0.5);
                half4 noiseMap2 = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, uvNoise2);
                
                float weight = abs(fracMulTime * 2 - 1);
			    float4 color = lerp(noiseMap1, noiseMap2, weight);
                */
                
                float2 uvNoise = TRANSFORM_TEX(input.uv, _Noise);
                half4 flowMap = SAMPLE_TEXTURE2D(_FlowMap, sampler_FlowMap, TRANSFORM_TEX(input.uv, _FlowMap));
                
                float4 color = Flow(uvNoise, TEXTURE2D_ARGS(_Noise, sampler_Noise), flowMap.rg, _NoiseStrength, _NoiseSpeed);
                
                return color;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}