Shader "Saber/Unlit/KK/KK Aniso2"
{
	Properties
	{
		_Shininess("Shininess",Range(0.01,1000)) = 1.0
		_ShiftOffset("_ShiftOffset",Range(-1,1)) = 0
		_ShiftMap("ShitfMap",2D) = "white"{}
		_FlowMap("_FlowMap",2D) = "gray"{}
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
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normal_dir : TEXCOORD1;
				float3 tangent_dir : TEXCOORD2;
				float3 binormal_dir : TEXCOORD3;
				float3 pos_world : TEXCOORD4;
			};

			TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
			float4 _MainTex_ST;
			float4 _LightColor0;
			float _Shininess;
			float _ShiftOffset;
			TEXTURE2D(_ShiftMap); SAMPLER(sampler_ShiftMap);
			float4 _ShiftMap_ST;
			TEXTURE2D(_FlowMap); SAMPLER(sampler_FlowMap);


			v2f vert(appdata v)
			{
				v2f o;
				o.pos = TransformObjectToHClip(v.vertex.xyz);
				o.uv = v.texcoord;
				o.normal_dir = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
				o.tangent_dir = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0)).xyz);
				o.binormal_dir = normalize(cross(o.normal_dir, o.tangent_dir)) * v.tangent.w;
				o.pos_world = mul(unity_ObjectToWorld, v.vertex).xyz;
				return o;
			}

			half4 frag(v2f i) : SV_Target
			{
				Light mainLight = GetMainLight();
				half3 light_dir = mainLight.direction;
				half3 view_dir = GetWorldSpaceNormalizeViewDir(i.pos_world);
				half3 normal_dir = normalize(i.normal_dir);
				half3 tangent_dir = normalize(i.tangent_dir);
				half3 binormal_dir = normalize(i.binormal_dir);

				//half3 flowmap = SAMPLE_TEXTURE2D(_FlowMap, sampler_FlowMap, i.uv).rgb;
				//half2 aniso_dir = flowmap.rg * 2.0 - 1.0;
				//half3 offsetBinormal = normalize(tangent_dir * aniso_dir.x + binormal_dir * aniso_dir.y);

				half2 uv_shift = i.uv * _ShiftMap_ST.xy + _ShiftMap_ST.zw;
				half shiftnoise = SAMPLE_TEXTURE2D(_ShiftMap, sampler_ShiftMap, uv_shift).r;
				shiftnoise = 0;
				half3 offsetBinormal = binormal_dir;
				offsetBinormal += normal_dir * (_ShiftOffset + shiftnoise);

				half3 dirH = normalize(view_dir + light_dir);
				half TdotH = dot(offsetBinormal, dirH);
				half sinTH = sqrt(1 - TdotH * TdotH);
				half3 specCol = pow(sinTH, _Shininess) * mainLight.color;

				return half4(specCol, 1);
			}
			ENDHLSL
		}
	}
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
