Shader "Saber/Unlit/Toon Water/Toon_Tree"
{
    Properties
    {
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_MainTex("MainTex", 2D) = "white" {}
		_Alpha("Alpha", Range( 0 , 1)) = 1
		_GlobalWindSpeed1("GlobalWindSpeed", Float) = 0.5
		_GlobalWindDirection1("GlobalWindDirection", Vector) = (0,0,-1,0)
		_GlobalWindStrength1("GlobalWindStrength", Float) = 1
		_SmallSpeed1("SmallSpeed", Float) = 0
		_SmallWeight1("SmallWeight", Float) = 0.65
		_WindWaveScale("WindWaveScale", Float) = 0
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
        Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float _GlobalWindSpeed1;
                float _GlobalWindStrength1;
                float3 _GlobalWindDirection1;
                float _SmallSpeed1;
                float _WindWaveScale;
                float _SmallWeight1;
                float4 _MainTex_ST;
                float _Alpha;
                float _Cutoff = 0.5;

            CBUFFER_END

			float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
			{
				original -= center;
				float C = cos( angle );
				float S = sin( angle );
				float t = 1 - C;
				float m00 = t * u.x * u.x + C;
				float m01 = t * u.x * u.y - S * u.z;
				float m02 = t * u.x * u.z + S * u.y;
				float m10 = t * u.x * u.y + S * u.z;
				float m11 = t * u.y * u.y + C;
				float m12 = t * u.y * u.z - S * u.x;
				float m20 = t * u.x * u.z - S * u.y;
				float m21 = t * u.y * u.z + S * u.x;
				float m22 = t * u.z * u.z + C;
				float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
				return mul( finalMatrix, original ) + center;
			}


			float3 ACESTonemap12( float3 linear_color )
			{
				float3 tonemapped_color = saturate((linear_color*(3 * linear_color + 0))/(linear_color*(2.0 * linear_color + 1.0) + 0.0));
				return tonemapped_color;
			}
            
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

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings UnlitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

				float temp_output_21_0 = ( ( ( ( _GlobalWindSpeed1 * 0.5 ) * _Time.y ) + input.color.b ) * ( 2.0 * PI ) );
				float temp_output_23_0 = ( _GlobalWindStrength1 * 0.1 );
				float3 temp_output_7_0_g2 = float3(0,0,1);
				float3 RotateAxis34_g2 = cross( temp_output_7_0_g2 , float3(0,1,0) );
				float3 wind_direction31_g2 = temp_output_7_0_g2;
				float3 wind_speed40_g2 = ( ( _Time.y * _SmallSpeed1 ) * float3(0.5,-0.5,-0.5) );
				float3 ase_worldPos = TransformObjectToWorld(input.positionOS.xyz);
				float temp_output_148_0_g2 = _WindWaveScale;
				float3 temp_cast_0 = (1.0).xxx;
				float3 temp_output_22_0_g2 = abs( ( ( frac( ( ( ( wind_direction31_g2 * wind_speed40_g2 ) +
					( ase_worldPos / ( 10.0 * temp_output_148_0_g2 ) ) ) + 0.5 ) ) * 2.0 ) - temp_cast_0 ) );
				float3 temp_cast_1 = (3.0).xxx;
				float dotResult30_g2 = dot( ( ( temp_output_22_0_g2 * temp_output_22_0_g2 ) * 
					( temp_cast_1 - ( temp_output_22_0_g2 * 2.0 ) ) ) , wind_direction31_g2 );
				float BigTriangleWave42_g2 = dotResult30_g2;
				float3 temp_cast_2 = (1.0).xxx;
				float3 temp_output_59_0_g2 = abs( ( ( frac( ( ( wind_speed40_g2 + ( ase_worldPos / ( 2.0 * temp_output_148_0_g2 ) ) ) + 0.5 ) ) * 2.0 ) - temp_cast_2 ) );
				float3 temp_cast_3 = (3.0).xxx;
				float SmallTriangleWave52_g2 = distance( ( ( temp_output_59_0_g2 * temp_output_59_0_g2 ) * ( temp_cast_3 - ( temp_output_59_0_g2 * 2.0 ) ) ) ,float3(0,0,0) );
				float3 rotatedValue72_g2 = RotateAroundAxis( ( ase_worldPos - float3(0,0.1,0) ), 
					ase_worldPos, normalize( RotateAxis34_g2 ), 
					( ( BigTriangleWave42_g2 + SmallTriangleWave52_g2 ) * ( 2.0 * PI ) ) );
				float3 worldToObj81_g2 = TransformWorldToObject(rotatedValue72_g2);
				float3 ase_vertex3Pos = input.positionOS.xyz;
				float3 TreeWindAnimVertex37 = ( ( input.color.r * ( sin( temp_output_21_0 ) * temp_output_23_0 ) * _GlobalWindDirection1 ) +
					( ( _GlobalWindDirection1 * ( cos( temp_output_21_0 ) * temp_output_23_0 ) * input.color.g ) + 
						( input.color.g * ( worldToObj81_g2 - ase_vertex3Pos ) * _SmallWeight1 ) ) );
				float3 positionOS = input.positionOS.xyz + TreeWindAnimVertex37;
            	
            	output.uv = input.uv;
            	output.positionCS = TransformObjectToHClip(positionOS);

                return output;
            }

            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);

				float2 uv_MainTex = input.uv * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 tex2DNode1 = SAMPLE_TEXTURE2D( _MainTex, sampler_MainTex, uv_MainTex );
				float3 linear_color12 = ( tex2DNode1 * tex2DNode1 ).rgb;
				float3 localACESTonemap12 = ACESTonemap12( linear_color12 );
            	half4 color;
				color.rgb = localACESTonemap12;
				color.a = _Alpha;
				clip( tex2DNode1.a - _Cutoff );
            	return color;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}