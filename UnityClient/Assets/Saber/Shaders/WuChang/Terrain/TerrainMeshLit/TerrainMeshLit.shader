Shader "Saber/WuChang/Terrain Mesh Lit"
{
    Properties
    {
        _Texture0 ("Texture 1", 2D) = "white" {}
        _Texture1 ("Texture 2", 2D) = "white" {}
        _Texture2 ("Texture 3", 2D) = "white" {}
        _Texture3 ("Texture 4", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        LOD 0

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

            
            TEXTURE2D(_Texture0);
            TEXTURE2D(_Texture1);
            TEXTURE2D(_Texture2);
            TEXTURE2D(_Texture3);
            SAMPLER(sampler_Texture0);

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 weight : TEXCOORD1;
                float4 normal : NORMAL;
                float3 worldPos : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.weight = v.tangent;
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
                half4 t0 = SAMPLE_TEXTURE2D(_Texture0, sampler_Texture0, i.uv);
                half4 t1 = SAMPLE_TEXTURE2D(_Texture1, sampler_Texture0, i.uv);
                half4 t2 = SAMPLE_TEXTURE2D(_Texture2, sampler_Texture0, i.uv);
                half4 t3 = SAMPLE_TEXTURE2D(_Texture3, sampler_Texture0, i.uv);
                half4 baseMap = t0 * i.weight.x + t1 * i.weight.y + t2 * i.weight.z + t3 * i.weight.w;

                float4 shadowCoord = TransformWorldToShadowCoord(i.worldPos);
                Light mainLight = GetMainLight(shadowCoord);
                half3 albedo = baseMap.rgb;
                float3 N = i.normal.xyz;
                half NoL = dot(N, mainLight.direction) * 0.5 + 0.5;
                half3 lighting = NoL * albedo * mainLight.color * mainLight.shadowAttenuation;
                half4 color = half4(lighting, 1);

                // sh
                half3 sh = SampleSH(N);
                color.rgb += sh * albedo;
                
                // fog
                half fogFactor = i.normal.w;
                float fogCoord = InitializeInputDataFog(float4(i.worldPos, 1.0), fogFactor);
                color.rgb = MixFog(color.rgb, fogCoord);
                
                return color;
            }
            ENDHLSL
        }
    }

    Fallback "Diffuse"
}