Shader "Saber/Unlit/Beer Glass Simple"
{
    Properties
    {
        _MatcapReflection ("Matcap Reflection", 2D) = "white" {}
        _MatcapRefraction ("Matcap Refraction", 2D) = "white" {}
        _RefractionIntensity("Refraction Intensity", float) = 1
        _RefractionColor ("Refraction Color", Color) = (0,0,0,0)
        _ThicknessMap ("Thickness Map", 2D) = "black" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }
        LOD 0

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            HLSLPROGRAM
            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            
            
            CBUFFER_START(UnityPerMaterial)
            float _RefractionIntensity;
            half4 _RefractionColor;
            CBUFFER_END
			
			TEXTURE2D(_MatcapReflection);   SAMPLER(sampler_MatcapReflection);
            TEXTURE2D(_MatcapRefraction);   SAMPLER(sampler_MatcapRefraction);
            TEXTURE2D(_ThicknessMap);       SAMPLER(sampler_ThicknessMap);
            

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : NORMAL;
                float4 positionWS : TEXCOORD0;
                float4 uv : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            

            // 十字叉积Matcap UV
            float2 CalcMatcapUV(float3 normalWS, float4 positionOS)
            {
                float3 normalVS = mul(UNITY_MATRIX_IT_MV, float4(normalWS, 0)).xyz;
                normalVS = normalize(normalVS);
                //float2 matcapUV = normalVS.xy * 0.5 + 0.5;
                float3 positionVS = mul(UNITY_MATRIX_MV, positionOS).xyz;
                float3 viewDirVS = normalize(-positionVS);
                float3 crossPN = cross(viewDirVS, normalVS);
                float2 matcapUV = float2(-crossPN.y, crossPN.x) * 0.5 + 0.5;
                return matcapUV;
            }
            
            Varyings Vertex(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv.xy = input.texcoord;
                output.normalWS = TransformObjectToWorldNormal(input.normal);
                output.positionWS.xyz = TransformObjectToWorld(input.positionOS.xyz);
                output.positionWS.z = input.positionOS.y;
                output.uv.zw = CalcMatcapUV(output.normalWS, input.positionOS);
                
                return output;
            }
            
            half4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                // 计算反射光
                float2 matcapUV = input.uv.zw;
                half4 matcapReflection = SAMPLE_TEXTURE2D(_MatcapReflection, sampler_MatcapReflection, matcapUV);

                // 采样厚度图
                half4 thickness = SAMPLE_TEXTURE2D(_ThicknessMap, sampler_ThicknessMap, input.uv.xy);

                // 计算菲涅尔
                float3 N = normalize(input.normalWS);
                float3 V = GetWorldSpaceNormalizeViewDir(input.positionWS.xyz);
                float NoV = dot(N, V);
                half rim = 1 - saturate(NoV);
                half refraction = (thickness + rim) * _RefractionIntensity;

                // 计算折射
                float2 refractionUV = saturate(matcapUV + refraction);
                half4 matcapRefraction = SAMPLE_TEXTURE2D(_MatcapRefraction, sampler_MatcapRefraction, refractionUV);
                //half3 refractionColor = matcapRefraction.rgb;
                half3 refractionColor = lerp(0.5, matcapRefraction.rgb, refraction) * _RefractionColor;

                // 融合反射与折射
                half4 color;
                color.rgb = matcapReflection.rgb + refractionColor;

                half a = max(matcapReflection.r, refraction);
                a = max(refractionColor.r, a);
                color.a = saturate(a);
                
                return color;
            }
            
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ZTest LEqual
            ColorMask 0
            //Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            // -------------------------------------
            // Material Keywords
            //#pragma shader_feature_local_fragment _ALPHATEST_ON
            //#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Universal Pipeline keywords

            // -------------------------------------
            // Unity defined keywords
            //#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }



    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
