Shader "ChargePathShader"
{
    Properties
    {
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_StartOffset("StartOffset", Range( -1 , 0)) = 0
		_Speed("Speed", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
             //"LightMode" = "AfterHair"
        }
        LOD 0

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			//ZWrite Off
            
            HLSLPROGRAM
            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_TextureSample0); SAMPLER(sampler_TextureSample0);
            CBUFFER_START(UnityPerMaterial)
			float _Speed;
			float _StartOffset;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            	float4 texcoord : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            	float4 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };


            Varyings Vertex(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
            	output.uv = input.texcoord;
                output.color = input.color;
                return output;
            }

            half4 Fragment(Varyings i) : SV_Target
            {
            	half4 color;
				float4 temp_cast_0 = (0.0).xxxx;
				float4 temp_cast_1 = (1.0).xxxx;
				float4 clampResult12 = clamp( ( i.uv + ( ( i.uv.w * _Speed ) + _StartOffset ) ) , temp_cast_0 , temp_cast_1 );
				float4 temp_output_14_0 = ( i.color * SAMPLE_TEXTURE2D( _TextureSample0, sampler_TextureSample0, clampResult12.xy ) );
				color.rgb = temp_output_14_0.rgb;
				color.a = temp_output_14_0.a;
            	return color;
            }
            
            ENDHLSL
        }

    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
