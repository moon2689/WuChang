Shader "Saber/Unlit/Toon Water/Toon_Water"
{
    Properties
    {
		_WaveASpeedXYSteepnesswavelength("WaveA(SpeedXY,Steepness,wavelength)", Vector) = (1,1,2,50)
		_WaveB("WaveB", Vector) = (1,1,2,50)
		_WaveC("WaveC", Vector) = (1,1,2,50)
		_WaveColor("WaveColor", Color) = (0,0,0,0)
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
            HLSLPROGRAM
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
		        float4 _WaveASpeedXYSteepnesswavelength;
		        float4 _WaveB;
		        float4 _WaveC;
		        float4 _WaveColor;
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
            	float3 normalWS : TEXCOORD1;
            	float3 positionWS : TEXCOORD2;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            float3 GerstnerWave188( float3 position, inout float3 tangent, inout float3 binormal, float4 wave )
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


			float3 GerstnerWave196( float3 position, inout float3 tangent, inout float3 binormal, float4 wave )
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


			float3 GerstnerWave203( float3 position, inout float3 tangent, inout float3 binormal, float4 wave )
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

            Varyings UnlitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
            	
            	float3 ase_worldPos = TransformObjectToWorld(input.positionOS.xyz);
				float3 position188 = ase_worldPos;
				float3 tangent188 = float3( 1,0,0 );
				float3 binormal188 = float3( 0,0,1 );
				float4 wave188 = _WaveASpeedXYSteepnesswavelength;
				float3 localGerstnerWave188 = GerstnerWave188( position188 , tangent188 , binormal188 , wave188 );
				float3 position196 = ase_worldPos;
				float3 tangent196 = tangent188;
				float3 binormal196 = binormal188;
				float4 wave196 = _WaveB;
				float3 localGerstnerWave196 = GerstnerWave196( position196 , tangent196 , binormal196 , wave196 );
				float3 position203 = ase_worldPos;
				float3 tangent203 = tangent196;
				float3 binormal203 = binormal196;
				float4 wave203 = _WaveC;
				float3 localGerstnerWave203 = GerstnerWave203( position203 , tangent203 , binormal203 , wave203 );
				float3 temp_output_191_0 = ( ase_worldPos + localGerstnerWave188 + localGerstnerWave196 + localGerstnerWave203 );
				float3 normalizeResult198 = normalize( cross( binormal203 , tangent203 ) );
				float3 worldToObjDir199 = mul( unity_WorldToObject, float4( normalizeResult198, 0 ) ).xyz;
				float3 WaveVertexNormal200 = worldToObjDir199;
				output.normalWS = WaveVertexNormal200;
            	
            	//float3 positionOS = input.positionOS.xyz;
            	output.positionCS = TransformWorldToHClip(temp_output_191_0);
            	output.positionWS = temp_output_191_0;//TransformObjectToWorld(positionOS);
            	
            	return output;
            }

            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);

				float3 ase_worldPos = input.positionWS;
				float3 position188 = ase_worldPos;
				float3 tangent188 = float3( 1,0,0 );
				float3 binormal188 = float3( 0,0,1 );
				float4 wave188 = _WaveASpeedXYSteepnesswavelength;
				float3 localGerstnerWave188 = GerstnerWave188( position188 , tangent188 , binormal188 , wave188 );
				float3 position196 = ase_worldPos;
				float3 tangent196 = tangent188;
				float3 binormal196 = binormal188;
				float4 wave196 = _WaveB;
				float3 localGerstnerWave196 = GerstnerWave196( position196 , tangent196 , binormal196 , wave196 );
				float3 position203 = ase_worldPos;
				float3 tangent203 = tangent196;
				float3 binormal203 = binormal196;
				float4 wave203 = _WaveC;
				float3 localGerstnerWave203 = GerstnerWave203( position203 , tangent203 , binormal203 , wave203 );
				float3 temp_output_191_0 = ( ase_worldPos + localGerstnerWave188 + localGerstnerWave196 + localGerstnerWave203 );
				float clampResult209 = clamp( (( temp_output_191_0 - ase_worldPos )).y , 0.0 , 1.0 );
				float4 WaveColor212 = ( clampResult209 * _WaveColor );
            	
            	half4 color;
				color.rgb = WaveColor212.rgb;
				color.a = 1;
            	return color;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}