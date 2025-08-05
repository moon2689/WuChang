Shader "Saber/Scene/Water Back Lit"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (0.6,1,0.8,1)
        _MainTex ("Main Texture", 2D) = "white" {}
        _MainTexFlow2("Main Tex flow 2", Vector) = (1,1,0,0)
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
			Cull Front
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            half4 _BaseColor;
            float4 _MainTexFlow2;
            CBUFFER_END

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
                
                float2 uv1 = input.uv * _MainTex_ST.xy + _MainTex_ST.zw * _Time.x;
                half4 mainTex1 =  SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv1);

                float2 uv2 = input.uv * _MainTexFlow2.xy + _MainTexFlow2.zw * _Time.x;
                half4 mainTex2 =  SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv2);

                half4 baseColor = mainTex1 + mainTex2;
                
                half4 color = baseColor.r * _BaseColor;
				return color;
            }
            
            ENDHLSL
        }

    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
