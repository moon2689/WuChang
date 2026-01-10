// Made with Amplify Shader Editor v1.9.9.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TestTechTransmit"
{
	Properties
	{
		_Scale( "Scale", Vector ) = ( 1, 1, 1, 0 )

	}

	SubShader
	{
		

		Tags { "RenderType"="Opaque" }

	LOD 0

		

		Blend Off
		AlphaToMask Off
		Cull Back
		ColorMask RGBA
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		

		CGINCLUDE
			#pragma target 3.5

			float4 ComputeClipSpacePosition( float2 screenPosNorm, float deviceDepth )
			{
				float4 positionCS = float4( screenPosNorm * 2.0 - 1.0, deviceDepth, 1.0 );
			#if UNITY_UV_STARTS_AT_TOP
				positionCS.y = -positionCS.y;
			#endif
				return positionCS;
			}
		ENDCG

		
		Pass
		{
			Name "Unlit"

			CGPROGRAM
				#define ASE_VERSION 19905

				#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
					#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
				#endif
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_instancing
				#include "UnityCG.cginc"

				

				struct appdata
				{
					float4 vertex : POSITION;
					
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float4 pos : SV_POSITION;
					float4 ase_texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
				};

				uniform float3 _Scale;


				float3 mod3D289( float3 x ) { return x - floor( x / 289.0 ) * 289.0; }
				float4 mod3D289( float4 x ) { return x - floor( x / 289.0 ) * 289.0; }
				float4 permute( float4 x ) { return mod3D289( ( x * 34.0 + 1.0 ) * x ); }
				float4 taylorInvSqrt( float4 r ) { return 1.79284291400159 - r * 0.85373472095314; }
				float snoise( float3 v )
				{
					const float2 C = float2( 1.0 / 6.0, 1.0 / 3.0 );
					float3 i = floor( v + dot( v, C.yyy ) );
					float3 x0 = v - i + dot( i, C.xxx );
					float3 g = step( x0.yzx, x0.xyz );
					float3 l = 1.0 - g;
					float3 i1 = min( g.xyz, l.zxy );
					float3 i2 = max( g.xyz, l.zxy );
					float3 x1 = x0 - i1 + C.xxx;
					float3 x2 = x0 - i2 + C.yyy;
					float3 x3 = x0 - 0.5;
					i = mod3D289( i);
					float4 p = permute( permute( permute( i.z + float4( 0.0, i1.z, i2.z, 1.0 ) ) + i.y + float4( 0.0, i1.y, i2.y, 1.0 ) ) + i.x + float4( 0.0, i1.x, i2.x, 1.0 ) );
					float4 j = p - 49.0 * floor( p / 49.0 );  // mod(p,7*7)
					float4 x_ = floor( j / 7.0 );
					float4 y_ = floor( j - 7.0 * x_ );  // mod(j,N)
					float4 x = ( x_ * 2.0 + 0.5 ) / 7.0 - 1.0;
					float4 y = ( y_ * 2.0 + 0.5 ) / 7.0 - 1.0;
					float4 h = 1.0 - abs( x ) - abs( y );
					float4 b0 = float4( x.xy, y.xy );
					float4 b1 = float4( x.zw, y.zw );
					float4 s0 = floor( b0 ) * 2.0 + 1.0;
					float4 s1 = floor( b1 ) * 2.0 + 1.0;
					float4 sh = -step( h, 0.0 );
					float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
					float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
					float3 g0 = float3( a0.xy, h.x );
					float3 g1 = float3( a0.zw, h.y );
					float3 g2 = float3( a1.xy, h.z );
					float3 g3 = float3( a1.zw, h.w );
					float4 norm = taylorInvSqrt( float4( dot( g0, g0 ), dot( g1, g1 ), dot( g2, g2 ), dot( g3, g3 ) ) );
					g0 *= norm.x;
					g1 *= norm.y;
					g2 *= norm.z;
					g3 *= norm.w;
					float4 m = max( 0.6 - float4( dot( x0, x0 ), dot( x1, x1 ), dot( x2, x2 ), dot( x3, x3 ) ), 0.0 );
					m = m* m;
					m = m* m;
					float4 px = float4( dot( x0, g0 ), dot( x1, g1 ), dot( x2, g2 ), dot( x3, g3 ) );
					return 42.0 * dot( m, px);
				}
				

				v2f vert ( appdata v )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID( v );
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
					UNITY_TRANSFER_INSTANCE_ID( v, o );

					float3 ase_positionWS = mul( unity_ObjectToWorld, float4( ( v.vertex ).xyz, 1 ) ).xyz;
					o.ase_texcoord.xyz = ase_positionWS;
					
					
					//setting value to unused interpolator channels and avoid initialization warnings
					o.ase_texcoord.w = 0;

					float3 vertexValue = float3( 0, 0, 0 );
					#if ASE_ABSOLUTE_VERTEX_POS
						vertexValue = v.vertex.xyz;
					#endif
					vertexValue = vertexValue;
					#if ASE_ABSOLUTE_VERTEX_POS
						v.vertex.xyz = vertexValue;
					#else
						v.vertex.xyz += vertexValue;
					#endif

					o.pos = UnityObjectToClipPos( v.vertex );
					return o;
				}

				half4 frag( v2f IN  ) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID( IN );
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );
					half4 finalColor;

					float4 ScreenPosNorm = float4( IN.pos.xy * ( _ScreenParams.zw - 1.0 ), IN.pos.zw );
					float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, IN.pos.z ) * IN.pos.w;
					float4 ScreenPos = ComputeScreenPos( ClipPos );

					float3 ase_positionWS = IN.ase_texcoord.xyz;
					float simplePerlin3D10 = snoise( ( ase_positionWS * _Scale ) );
					simplePerlin3D10 = simplePerlin3D10*0.5 + 0.5;
					float4 temp_cast_0 = (simplePerlin3D10).xxxx;
					

					finalColor = temp_cast_0;

					return finalColor;
				}
			ENDCG
		}
	}
	CustomEditor "AmplifyShaderEditor.MaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19905
Node;AmplifyShaderEditor.Vector3Node, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;14;-672,112;Inherit;False;Property;_Scale;Scale;0;0;Create;True;0;0;0;False;0;False;1,1,1;1,1,1;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;15;-656,-144;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;13;-448,64;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NoiseGeneratorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;10;-320,-96;Inherit;False;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;16;-48,-32;Float;False;True;-1;3;AmplifyShaderEditor.MaterialInspector;0;5;TestTechTransmit;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;RenderType=Opaque=RenderType;True;3;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;0;;0;0;Standard;1;Vertex Position;1;0;0;1;True;False;;False;0
WireConnection;13;0;15;0
WireConnection;13;1;14;0
WireConnection;10;0;13;0
WireConnection;16;0;10;0
ASEEND*/
//CHKSM=5BF4273A980A15D1476F052F44FAB68AB6581E33