Shader "Saber/Unlit/Beer Glass"
{
    Properties
    {
        _MatcapReflection ("Matcap Reflection", 2D) = "white" {}
        _MatcapRefraction ("Matcap Refraction", 2D) = "white" {}
        _RefractionIntensity("Refraction Intensity", float) = 1
        _RefractionColor ("Refraction Color", Color) = (0,0,0,0)
        _ThicknessMap ("Thickness Map", 2D) = "black" {}
        /*
        _DirtyMap ("Dirty Map", 2D) = "black" {}
        _DirtyIntensity("Dirty Intensity", Range(0, 1)) = 1
        _LogoMap ("Logo Map", 2D) = "black" {}
        _LogoIntensity("Logo Intensity", Range(0, 1)) = 1
        */
        
        _Distortion("Distortion", float) = 1
        
        _BlurSpread("Blur Spread", float) = 1
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
            //Blend SrcAlpha OneMinusSrcAlpha
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
            // float _DirtyIntensity;
            // float _LogoIntensity;
            half4 _CameraOpaqueTexture_TexelSize;
            float _Distortion;
            float _BlurSpread;
            CBUFFER_END
			
			TEXTURE2D(_MatcapReflection);   SAMPLER(sampler_MatcapReflection);
            TEXTURE2D(_MatcapRefraction);   SAMPLER(sampler_MatcapRefraction);
            TEXTURE2D(_ThicknessMap);       SAMPLER(sampler_ThicknessMap);
            // TEXTURE2D(_DirtyMap);           SAMPLER(sampler_DirtyMap);
            // TEXTURE2D(_LogoMap);            SAMPLER(sampler_LogoMap);
            

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

            /*
            // 改进的叉积Matcap UV计算
            float2 CalculateCrossMatcapUV(float3 worldPos, float3 worldNormal)
            {
                // 转换为视图空间
                float3 normalVS = mul(UNITY_MATRIX_IT_MV, float4(worldNormal, 0)).xyz;
                normalVS = normalize(normalVS);
                
                // 视图空间位置（从顶点到相机）
                float3 positionVS = mul(UNITY_MATRIX_MV, float4(worldPos, 1)).xyz;
                
                // 计算视图方向（从顶点指向相机）
                float3 viewDirVS = normalize(-positionVS);
                
                // 十字叉积
                float3 crossPN = cross(viewDirVS, normalVS);
                
                // 可选：根据法线z分量调整强度（减少边缘失真）
                float normalZ = normalVS.z;
                float edgeFactor = saturate(abs(normalZ) * 2);
                
                // 映射到UV
                float2 uv = float2(-crossPN.y, crossPN.x);
                uv *= lerp(1.0, 0.5, edgeFactor); // 边缘处缩小
                
                return uv * 0.5 + 0.5;
            }
            */

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
                //half4 dirtyMap = SAMPLE_TEXTURE2D(_DirtyMap, sampler_DirtyMap, input.uv.xy);
                //half4 logoMap = SAMPLE_TEXTURE2D(_LogoMap, sampler_LogoMap, input.uv.xy);

                // 计算菲涅尔
                float3 N = normalize(input.normalWS);
                float3 V = GetWorldSpaceNormalizeViewDir(input.positionWS.xyz);
                float NoV = dot(N, V);
                half rim = 1 - saturate(NoV);
                half refraction = (thickness + rim) * _RefractionIntensity;// + dirtyMap.r * _DirtyIntensity + logoMap.r * _LogoIntensity;

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

                //color = half4(refractionColor, 1);
                //color = half4(refraction.xxx, 1);

                // 扭曲效果
                float2 screenUV = GetNormalizedScreenSpaceUV(input.positionCS);
                screenUV += N.xy * _CameraOpaqueTexture_TexelSize.xy * _Distortion * refraction;

                float3 sceneColor;
                for (int i = -2; i <= 2; ++i)
                {
                    for (int j = -2; j <= 2; ++j)
                    {
                        float2 uvOffset = _CameraOpaqueTexture_TexelSize.xy * float2(i,j) * _BlurSpread;
                        sceneColor += SampleSceneColor(screenUV + uvOffset);      
                    }    
                }
                sceneColor /= 25;
                //float3 sceneColor = SampleSceneColor(screenUV);
                
                color.rgb = lerp(sceneColor.rgb, color.rgb, color.a);
                
                return color;
            }
            
            ENDHLSL
        }

    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
