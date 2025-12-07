Shader "Saber/Unlit/Matcap"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
		_Matcap("Matcap",2D) = "white"{}
		_MatcapIntensity("Matcap Intensity",Float) = 1.0
		_RampTex("Ramp Tex",2D ) = "white"{}
		_MatcapAdd("MatcapAdd",2D ) = "white"{}
		_MatcapAddIntensity("MatcapAdd Intensity",Float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal :NORMAL;

			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normal_world : TEXCOORD1;
				float3 pos_world : TEXCOORD2;
			};

			TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
			float4 _MainTex_ST;
			TEXTURE2D(_Matcap); SAMPLER(sampler_Matcap);
			float _MatcapIntensity;
			TEXTURE2D(_RampTex); SAMPLER(sampler_RampTex);
			TEXTURE2D(_MatcapAdd); SAMPLER(sampler_MatcapAdd);
			float _MatcapAddIntensity;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normal_world = mul(float4(v.normal, 0.0), unity_WorldToObject).xyz;
				o.pos_world = mul(unity_ObjectToWorld, v.vertex).xyz;
				return o;
			}
			
			half4 frag (v2f i) : SV_Target
			{
				half3 normal_world = normalize(i.normal_world);
				
				// base matcap
				half3 normal_viewspace = mul(UNITY_MATRIX_V, float4(normal_world, 0.0)).xyz;
				half2 uv_matcap = normal_viewspace.xy * 0.5 + float2(0.5, 0.5);
				half4 matcap_color = SAMPLE_TEXTURE2D(_Matcap, sampler_Matcap, uv_matcap) * _MatcapIntensity;
				half4 diffuse_color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

				//Ramp
				half3 view_dir = GetWorldSpaceNormalizeViewDir(i.pos_world);
				half NdotV = saturate(dot(normal_world, view_dir));
				half fresnel = 1.0 - NdotV;
				half2 uv_ramp = half2(fresnel, 0.5);
				half4 ramp_color = SAMPLE_TEXTURE2D(_RampTex, sampler_RampTex, uv_ramp);

				//add matcap
				half4 matcap_add_color = SAMPLE_TEXTURE2D(_MatcapAdd, sampler_MatcapAdd, uv_matcap) * _MatcapAddIntensity;

				half4 combined_color = diffuse_color * matcap_color * ramp_color + matcap_add_color;
				return combined_color;
			}

            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
