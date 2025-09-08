Shader "Saber/WuChang/Terrain Lit Simple"
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

            sampler2D _Texture0;
            sampler2D _Texture1;
            sampler2D _Texture2;
            sampler2D _Texture3;

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
                half4 t0 = tex2D(_Texture0, i.uv);
                half4 t1 = tex2D(_Texture1, i.uv);
                half4 t2 = tex2D(_Texture2, i.uv);
                half4 t3 = tex2D(_Texture3, i.uv);
                half4 baseMap = t0 * i.weight.x + t1 * i.weight.y + t2 * i.weight.z + t3 * i.weight.w;

                float4 shadowCoord = TransformWorldToShadowCoord(i.worldPos);
                Light mainLight = GetMainLight(shadowCoord);
                half NoL = dot(i.normal.xyz, mainLight.direction) * 0.5 + 0.5;
                half3 lighting = NoL * baseMap.rgb * mainLight.color * mainLight.shadowAttenuation;
                half4 color = half4(lighting, 1);

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