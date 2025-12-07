Shader "Saber/Unlit/KK/KK Aniso"
{
	Properties
	{
		_Shininess("Shininess",Range(10,300)) = 1.0
		_ShiftOffset("_ShiftOffset",Range(-1,1)) = 0
		_ShiftMap("ShitfMap",2D) = "white"{}
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
			"RenderPipeline" = "UniversalPipeline"
		}
		LOD 100

		Pass
		{
			Tags
			{
				"LightMode" = "UniversalForward"
			}
			
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float3 normal  : NORMAL;
				float4 tangent : TANGENT;
			};

			struct v2f
			{
				float4 positionCS : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normalWS : TEXCOORD1;
				float3 tangentWS : TEXCOORD2;
				float3 binormalWS : TEXCOORD3;
				float3 positionWS : TEXCOORD4;
			};

			TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
			float4 _MainTex_ST;
			float4 _LightColor0;
			float _Shininess;
			float _ShiftOffset;
			TEXTURE2D(_ShiftMap); SAMPLER(sampler_ShiftMap);
			float4 _ShiftMap_ST;


			v2f vert(appdata v)
			{
				v2f o;
				o.positionCS = TransformObjectToHClip(v.vertex.xyz);
				o.uv = v.texcoord;
				
				/*
				o.normalWS = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
				o.tangentWS = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0)).xyz);
				o.binormalWS = normalize(cross(o.normalWS, o.tangentWS)) * v.tangent.w;
				o.positionWS = mul(unity_ObjectToWorld, v.vertex).xyz;
				*/
				
				VertexNormalInputs vertexNormal = GetVertexNormalInputs(v.normal, v.tangent);
				o.normalWS = vertexNormal.normalWS;
				o.tangentWS = vertexNormal.tangentWS;
				o.binormalWS = vertexNormal.bitangentWS;
				o.positionWS = TransformObjectToWorld(v.vertex).xyz;
				
				return o;
			}

			half4 frag(v2f i) : SV_Target
			{
				Light mainLight = GetMainLight();
				half3 L = mainLight.direction;
				half3 V = GetWorldSpaceNormalizeViewDir(i.positionWS);
				half3 N = normalize(i.normalWS);
				half3 T = normalize(i.tangentWS);
				half3 BT = normalize(i.binormalWS);

				half3 H = normalize(V + L);
				half2 uv_shift = i.uv * _ShiftMap_ST.xy + _ShiftMap_ST.zw;
				half shiftnoise = SAMPLE_TEXTURE2D(_ShiftMap, sampler_ShiftMap, uv_shift).r;
				half3 offsetBinormal = BT + (shiftnoise + _ShiftOffset) * N;
				half TdotH = dot(offsetBinormal, H);
				half sinTH = sqrt(1 - TdotH * TdotH);
				half3 specCol = pow(sinTH, _Shininess) * mainLight.color;

				//half NdotH = dot(normal_dir, H);
				//specCol = pow(NdotH, _Shininess) * mainLight.color;

				return half4(specCol, 1);
			}
			ENDHLSL
		}
	}
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
