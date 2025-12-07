Shader "Saber/Unlit/URPScan"
{
    Properties
    {
		_MainTex ("Texture", 2D) = "white" {}
		_RimMin("RimMin",Range(-1,1)) = 0.0
		_RimMax("RimMax",Range(0,2)) = 1.0
		_InnerColor("Inner Color",Color) = (0.0,0.0,0.0,0.0)
		_RimColor("Rim Color",Color) = (1,1,1,1)
		_RimIntensity("Rim Intensity",Float) = 1.0
		_FlowTilling("Flow Tilling",Vector) = (1,1,0,0)
		_FlowSpeed("Flow Speed",Vector) = (1,1,0,0)
		_FlowTex("Flow Tex",2D) = "white"{}
		_FlowIntensity("Flow Intensity",Float) = 0.5
		_InnerAlpha("Inner Alpha",Range(0.0,1.0)) = 0.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
			"Queue" = "Transparent"
        }

        Pass
        {
            ZWrite Off
			Blend SrcAlpha One

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 pos_world : TEXCOORD1;
				float3 normal_world : TEXCOORD2;
				float3 pivot_world : TEXCOORD3;
			};

			TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
			float4 _MainTex_ST;
			float _RimMin;
			float _RimMax;
			float4 _InnerColor;
			float4 _RimColor;
			float _RimIntensity;
			float4 _FlowTilling;
			float4 _FlowSpeed;
			TEXTURE2D(_FlowTex); SAMPLER(sampler_FlowTex);
			float _FlowIntensity;
			float _InnerAlpha;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.normal_world = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
				o.pos_world = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.pivot_world = mul(unity_ObjectToWorld, float4(0.0, 0.0, 0.0, 1.0)).xyz;
				o.uv = v.texcoord;
				return o;
			}
			
			half4 frag (v2f i) : SV_Target
			{
				half3 normal_world = normalize(i.normal_world);
				half3 view_world = normalize(_WorldSpaceCameraPos.xyz - i.pos_world);
				
				//
				half NdotV = saturate(dot(normal_world, view_world));
				half fresnel = 1.0 - NdotV;
				fresnel = smoothstep(_RimMin, _RimMax, fresnel);
				half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
				half emiss = mainTex.r;
				emiss = pow(emiss, 5.0);

				half final_fresnel = saturate(fresnel + emiss);

				half3 final_rim_color = lerp(_InnerColor.xyz, _RimColor.xyz * _RimIntensity, final_fresnel);
				half final_rim_alpha = final_fresnel;
				//流光
				half2 uv_flow = (i.pos_world.xy - i.pivot_world.xy) * _FlowTilling.xy;
				uv_flow = uv_flow + _Time.y * _FlowSpeed.xy;
				float4 flow_rgba = SAMPLE_TEXTURE2D(_FlowTex, sampler_FlowTex, uv_flow) * _FlowIntensity;

				float3 final_col = final_rim_color + flow_rgba.xyz;
				float final_alpha = saturate(final_rim_alpha + flow_rgba.a + _InnerAlpha);
				return float4(final_col, final_alpha);
			}

            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
