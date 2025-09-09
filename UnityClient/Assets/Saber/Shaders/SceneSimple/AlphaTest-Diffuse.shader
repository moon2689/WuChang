Shader "Saber/Legacy Shaders/Transparent/Cutout/Diffuse"
{
    Properties
    {
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        LOD 150

        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            
            HLSLPROGRAM
            #pragma target 2.0
            
            #pragma vertex vert
            #pragma fragment frag

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			TEXTURE2D(_MainTex);  SAMPLER(sampler_MainTex);
			half4 _Color;
			half _Cutoff;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 normal : NORMAL;
                float3 worldPos : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                o.normal.xyz = TransformObjectToWorldNormal(v.normal);
                o.worldPos = TransformObjectToWorld(v.vertex.xyz);

                // fog
                half fogFactor = 0;
                #if !defined(_FOG_FRAGMENT)
                    fogFactor = ComputeFogFactor(o.positionCS.z);
                #endif
                o.normal.w = fogFactor;
                
                return o;
            }

            half4 frag(v2f i) : SV_TARGET
            {
                half4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * _Color;

                float4 shadowCoord = TransformWorldToShadowCoord(i.worldPos);
                Light mainLight = GetMainLight(shadowCoord);
                float3 N = i.normal.xyz;
                half NoL = dot(N, mainLight.direction) * 0.5 + 0.5;
                half3 albedo = baseMap.rgb;
                half3 lighting = NoL * albedo * mainLight.color * mainLight.shadowAttenuation;

                
                half4 color;
                color.rgb = lighting;
                color.a = baseMap.a;

                half3 sh = SampleSH(N);
                color.rgb += sh * albedo;

                // fog
                half fogFactor = i.normal.w;
                float fogCoord = InitializeInputDataFog(float4(i.worldPos, 1.0), fogFactor);
                color.rgb = MixFog(color.rgb, fogCoord);
				
				clip(color.a - _Cutoff);
                
                return color;
            }
            ENDHLSL
        }
    }
}