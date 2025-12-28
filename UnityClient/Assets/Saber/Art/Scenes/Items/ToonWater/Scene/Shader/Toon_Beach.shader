Shader "Saber/Unlit/Toon Water/Beach"
{
    Properties
    {
		_Sand_Water("Sand_Water", 2D) = "white" {}
		_Sand_Land("Sand_Land", 2D) = "white" {}
		_Tilling("Tilling", Float) = 1
		_ShadowColor("ShadowColor", Color) = (0,0,0,0)
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

            TEXTURE2D(_Sand_Water);
            SAMPLER(sampler_Sand_Water);
            
            TEXTURE2D(_Sand_Land);
            SAMPLER(sampler_Sand_Land);

            CBUFFER_START(UnityPerMaterial)
			uniform float _Tilling;
			uniform float4 _ShadowColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            	float3 color : COLOR;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            	float3 color : TEXCOORD1;
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
            	output.color = input.color;
            	output.positionWS = TransformObjectToWorld(input.positionOS.xyz);

                return output;
            }

			float3 ACESTonemap12( float3 linear_color )
			{
				float3 tonemapped_color = saturate((linear_color*(2.8 * linear_color + 0))/(linear_color*(2.0 * linear_color + 1.0) + 0.0));
				return tonemapped_color;
			}
            
            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);

				float2 uvTilling = ( input.uv * _Tilling );
				float4 tex2DNode1 = SAMPLE_TEXTURE2D(_Sand_Water, sampler_Sand_Water, uvTilling);
				float4 tex2DNode2 = SAMPLE_TEXTURE2D(_Sand_Land, sampler_Sand_Land, uvTilling);
				float4 lerpResult9 = lerp(tex2DNode1 * tex2DNode1, tex2DNode2 * tex2DNode2, input.color.r);
				float3 localACESTonemap12 = ACESTonemap12( lerpResult9.rgb );
            	
            	float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
            	Light mainLight = GetMainLight(shadowCoord);
            	float shadowAtten = mainLight.shadowAttenuation;
				float3 shadowColor = lerp( _ShadowColor , 1 , shadowAtten);
            	
            	half4 color;
            	color.rgb = localACESTonemap12 * shadowColor;
            	color.a = 1;
				return color;
            	
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}