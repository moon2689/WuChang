Shader "Saber/Unlit/Fire"
{
    Properties
    {
        _MainTex ("Noise Texture", 2D) = "white" {}
        _GradientTex ("Gradient Texture", 2D) = "white" {}
        _ShapeTex ("Shape Texture", 2D) = "white" {}
        [HDR] _ColorTop ("Color Top", Color) = (0,0,0,0)
        [HDR] _ColorBottom ("Color Bottom", Color) = (0,0,0,0)
        _FlowSpeed ("Flow Speed", float) = 1
        _StepA ("Step A", Range(0, 1)) = 0.5
        _Distortion ("Distortion", float) = 0.2
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


            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            half4 _ColorTop;
            half4 _ColorBottom;
            float _FlowSpeed;
            float _StepA;
            float _StepX;
            float _Distortion;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_GradientTex);
            SAMPLER(sampler_GradientTex);
            TEXTURE2D(_ShapeTex);
            SAMPLER(sampler_ShapeTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            

            Varyings Vertex(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.texcoord;

                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                
                float2 uvNoise = TRANSFORM_TEX(input.uv, _MainTex);
                uvNoise.y -= _Time.y * _FlowSpeed;
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvNoise);
                
                half4 gradientTex = SAMPLE_TEXTURE2D(_GradientTex, sampler_GradientTex, input.uv);
                
                float2 uvShape = input.uv;
                uvShape.x += (mainTex.r * 2 - 1) * _Distortion * (1 - gradientTex.r);
                uvShape = saturate(uvShape);
                half4 shapeTex = SAMPLE_TEXTURE2D(_ShapeTex, sampler_ShapeTex, uvShape);

                half4 color;
                color.rgb = lerp(_ColorTop.rgb, _ColorBottom.rgb, gradientTex.r);
                
                float alpha = smoothstep(mainTex.r * _StepA, mainTex.r, gradientTex.r * shapeTex.r);
                alpha *= shapeTex.r;
                color.a = alpha;
                
                return color;
            }
            ENDHLSL
        }
    }
}