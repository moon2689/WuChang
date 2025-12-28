Shader "Saber/Unlit/Toon Water/Toon_Ship"
{
    Properties
    {
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_MainTex("MainTex", 2D) = "white" {}
		_Alpha("Alpha", Range( 0 , 1)) = 1
		_Pivot("Pivot", Vector) = (0,0,0,0)
		_WaveASpeedXYSteepnesswavelength1("WaveA(SpeedXY,Steepness,wavelength)", Vector) = (1,1,2,50)
		_WaveB1("WaveB", Vector) = (1,1,2,50)
		_WaveC1("WaveC", Vector) = (1,1,2,50)
		_WaveIntensity("WaveIntensity", Float) = 1
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

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float3 _Pivot;
                float4 _WaveASpeedXYSteepnesswavelength1;
                float4 _WaveB1;
                float4 _WaveC1;
                float _WaveIntensity;
                float4 _MainTex_ST;
                float _Alpha;
                float _Cutoff = 0.5;

            CBUFFER_END
            
            float3 GerstnerWave(float3 position, inout float3 tangent, inout float3 binormal, float4 wave)
			{
				float steepness = wave.z * 0.01;
				float wavelength = wave.w;
				float k = 2 * PI / wavelength;
				float c = sqrt(9.8 / k);
				float2 d = normalize(wave.xy);
				float f = k * (dot(d, position.xz) - c * _Time.y);
				float a = steepness / k;
							
				tangent += float3(
				-d.x * d.x * (steepness * sin(f)),
				d.x * (steepness * cos(f)),
				-d.x * d.y * (steepness * sin(f))
				);
				binormal += float3(
				-d.x * d.y * (steepness * sin(f)),
				d.y * (steepness * cos(f)),
				-d.y * d.y * (steepness * sin(f))
				);
				return float3(
				d.x * (a * cos(f)),
				a * sin(f),
				d.y * (a * cos(f))
				);
			}
            
			float3 ACESTonemap( float3 linear_color )
			{
				float3 tonemapped_color = saturate((linear_color*(2.8 * linear_color + 0))/(linear_color*(2.0 * linear_color + 1.0) + 0.0));
				return tonemapped_color;
			}
            

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

            	float3 pivotWorldPos = TransformObjectToWorld(_Pivot);
				float3 tangentWS = float3( 1,0,0 );
				float3 binormalWS = float3( 0,0,1 );
				float3 localGerstnerWave12 = GerstnerWave( pivotWorldPos , tangentWS , binormalWS , _WaveASpeedXYSteepnesswavelength1 );
				float3 localGerstnerWave14 = GerstnerWave( pivotWorldPos , tangentWS , binormalWS , _WaveB1 );
				float3 localGerstnerWave16 = GerstnerWave( pivotWorldPos , _WaveB1.xyz , binormalWS , _WaveC1 );
				float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
            	worldPos += ( localGerstnerWave12 + localGerstnerWave14 + localGerstnerWave16 ) * _WaveIntensity;
				output.positionCS = TransformWorldToHClip(worldPos);
            		
                output.uv = input.uv;

                return output;
            }
	            
            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);

			    float2 uv_MainTex = input.uv * _MainTex_ST.xy + _MainTex_ST.zw;
			    float4 tex2DNode1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv_MainTex);
			    float3 linear_color8 = ( tex2DNode1 * tex2DNode1 ).rgb;
			    float3 localACESTonemap8 = ACESTonemap( linear_color8 );
            	
            	half4 color;
			    color.rgb = localACESTonemap8;
			    color.a = _Alpha;
			    clip( tex2DNode1.a - _Cutoff );
            	
                return color;
            }
            ENDHLSL
        }

		
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}