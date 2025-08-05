Shader "Saber/Feature/Post Process Witch Time"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _MaskTex("Mask (R:Color G:Distort)", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Cull Off
            ZWrite Off
            ZTest Always

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            //#include "../Old/Library/TACommon.hlsl"

            TEXTURE2D(_MainTex);            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MaskTex);            SAMPLER(sampler_MaskTex);
            half4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_TARGET
            {
                half4 maskTex = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv);
                float2 mainTexUV = float2(i.uv.x, i.uv.y + maskTex.g - 0.5);

                /*
                float distanceToCenter = distance(i.uv, 0.5);
                float2 mainTexUV = i.uv;
                if (distanceToCenter < 0.5)
                {
                    float weight = saturate((distanceToCenter - 0.4) / 0.1);
                    float angle = lerp(0, -10, weight);
                    mainTexUV = RotateAroundByDegree(angle, i.uv - 0.5);
                    mainTexUV += 0.5;
                }*/
                
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainTexUV);
                half4 blendColor = lerp(1, _Color, maskTex.r * _Color.a);
                return mainTex * blendColor;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}