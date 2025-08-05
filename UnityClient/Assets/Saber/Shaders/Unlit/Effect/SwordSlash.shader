Shader "Saber/Unlit/Particles/SwordSlash"
{
    Properties
    {
        _MaskTex ("Mask (R)", 2D) = "white" {}
    	_DispMap ("Displacement Map (RG)", 2D) = "white" {}
        [MainColor]   _BaseColor("Base Color", Color) = (1, 1, 1, 1)
	    _DispScrollSpeedX  ("Map Scroll Speed X", Float) = 0
	    _DispScrollSpeedY  ("Map Scroll Speed Y", Float) = 0
	    _StrengthX  ("Displacement Strength X", Float) = 1
	    _StrengthY  ("Displacement Strength Y", Float) = -1
    	_ColorIntensity("Color Intensity", Float) = 20
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
			Cull Off
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

            TEXTURE2D(_CameraOpaqueTexture); SAMPLER(sampler_CameraOpaqueTexture);
            TEXTURE2D(_MaskTex); SAMPLER(sampler_MaskTex);
            TEXTURE2D(_DispMap); SAMPLER(sampler_DispMap);
            
            CBUFFER_START(UnityPerMaterial)
			half4 _BaseColor;
            half _StrengthX;
            half _StrengthY;
            float4 _DispMap_ST;
            half _DispScrollSpeedY;
            half _DispScrollSpeedX;
            half _ColorIntensity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
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
                output.uv.xy = input.texcoord;
                output.uv.zw = TRANSFORM_TEX(input.texcoord, _DispMap);
                output.color = input.color;
                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                //scroll displacement map.
	            half2 mapoft = half2(_Time.y * _DispScrollSpeedX, _Time.y * _DispScrollSpeedY);

	            //get displacement color
	            half4 offsetColor = SAMPLE_TEXTURE2D(_DispMap, sampler_DispMap, input.uv.zw + mapoft);

	            //get offset
	            half oftX =  offsetColor.r * _StrengthX;// * i.param.x;
	            half oftY =  offsetColor.g * _StrengthY;// * i.param.x;
                
                half2 screenUV = GetNormalizedScreenSpaceUV(input.positionCS);
				screenUV.x += oftX;
				screenUV.y += oftY;
                
                half4 screenMap =  SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenUV);
            	half4 maskMap =  SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, input.uv.xy);
            	half4 color = half4(_BaseColor.rgb * screenMap.rgb * _ColorIntensity, maskMap.r * _BaseColor.a);
				return color;
            }
            
            ENDHLSL
        }

    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
