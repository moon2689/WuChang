Shader "Saber/Particles/ChargeOrb"
{
    Properties
    {
		_FresnelScale("FresnelScale", Float) = 0
		_FresnelPower("FresnelPower", Float) = 0
		_TransFresnelScale("TransFresnelScale", Float) = 0
		_TransFresnelPower("TransFresnelPower", Float) = 0
		[HDR]_BaseColor("BaseColor", Color) = (0,0,0,0)
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

            CBUFFER_START(UnityPerMaterial)
			float _FresnelScale;
			float _FresnelPower;
			float4 _BaseColor;
			float _TransFresnelScale;
			float _TransFresnelPower;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                half4 color : COLOR;
            	float4 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
            	float4 positionWS : TEXCOORD1;
            	float4 normalWS : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };


            Varyings Vertex(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color;
            	output.positionWS.xyz = TransformObjectToWorld(input.positionOS.xyz);
            	output.normalWS.xyz = TransformObjectToWorldNormal(input.normalOS.xyz);
                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
            	half4 color;
				float3 ase_worldPos = input.positionWS.xyz;
				float3 ase_worldViewDir = GetWorldSpaceViewDir(ase_worldPos);
				float3 ase_worldNormal = input.normalWS.xyz;
				float fresnelNdotV1 = dot( ase_worldNormal, ase_worldViewDir );
				float fresnelNode1 = ( 0.0 + _FresnelScale * pow( abs(1.0 - fresnelNdotV1), _FresnelPower ) );
				color.rgb = ( input.color * fresnelNode1 * _BaseColor ).rgb;
				float fresnelNdotV6 = dot( ase_worldNormal, ase_worldViewDir );
				float fresnelNode6 = ( 0.0 + _TransFresnelScale * pow( abs(1.0 - fresnelNdotV6), _TransFresnelPower ) );
				float clampResult9 = clamp( fresnelNode6 , 0.0 , 1.0 );
				color.a = ( 1.0 - clampResult9 );
            	return color;
            }
            
            ENDHLSL
        }

    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
