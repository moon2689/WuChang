Shader "Un/BlinnPhong/Universal"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0
        [Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
        
        [Toggle(_ALPHATEST_ON)] _AlphaClip ("Alpha Clip", Float) = 0
        _Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _RampMap ("Toon Ramp (RGB)", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "False"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // URP 核心库
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Shader 特性
            #pragma multi_compile _ _ALPHATEST_ON
            #pragma multi_compile _ _UNLIT_ON _HALFLAMBERT_ON _RAMP_ON
            //#pragma shader_feature _ALPHAPREMULTIPLY_ON

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };

            TEXTURE2D(_BaseMap);        SAMPLER(sampler_BaseMap);
            TEXTURE2D(_RampMap);        SAMPLER(sampler_RampMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _Cutoff;
                float _Transparency;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.positionWS = vertexInput.positionWS;
                
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                output.normalWS.xyz = normalInput.normalWS;

                // fog
                half fogFactor = 0;
                #if !defined(_FOG_FRAGMENT)
                    fogFactor = ComputeFogFactor(o.positionCS.z);
                #endif
                output.normalWS.w = fogFactor;
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 采样纹理
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half3 albedo = baseMap.rgb * _BaseColor.rgb;
                half alpha = baseMap.a * _BaseColor.a;
                
                // Alpha 测试
                #if _ALPHATEST_ON
                    clip(alpha - _Cutoff);
                #endif

                half4 color;
                color.a = alpha;

                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                half3 L = mainLight.direction;
                float3 N = input.normalWS.xyz;

                //_UNLIT_ON _HALFLAMBERT_ON _RAMP_ON
                half3 diffuseLighting = albedo * mainLight.color * mainLight.shadowAttenuation;
                #if _UNLIT_ON
                #elif _HALFLAMBERT_ON
                    half NoL = dot(N, L) * 0.5 + 0.5;
                    diffuseLighting *= NoL;
                #elif _RAMP_ON
                    half NoL = dot(N, L) * 0.5 + 0.5;
                    half4 rampTex = SAMPLE_TEXTURE2D(_RampMap, sampler_RampMap, float2(NoL,0.5));
                    diffuseLighting *= rampTex.rgb;
                #endif
                
                half3 env = _GlossyEnvironmentColor.rgb * albedo; //half3 sh = SampleSH(N); or _GlossyEnvironmentColor ?
                
                half3 lighting = diffuseLighting + env;
                color.rgb = lighting;

                // fog
                half fogFactor = input.normalWS.w;
                float fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), fogFactor);
                color.rgb = MixFog(color.rgb, fogCoord);
                
                return color;
            }
            ENDHLSL
        }
    }
    
    CustomEditor "UnBlinnPhongUniversalShaderGUI"
}