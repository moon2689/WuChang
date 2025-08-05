Shader "Saber/Unlit/Feature/Post Process Blur"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _BlurSpread("Blur Spread", float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
        float4 _MainTex_TexelSize;
        float _BlurSpread;
 
        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };
 
        struct v2f
        {
            float4 pos : SV_POSITION;
            float2 uv[5] : TEXCOORD0;
        };
 
        v2f vertHorizontal(appdata v)
        {
            v2f o;
            o.pos = TransformObjectToHClip(v.vertex.xyz);
            float tsx = _MainTex_TexelSize.x * _BlurSpread;
            o.uv[0] = v.uv + float2(tsx * -2, 0);
            o.uv[1] = v.uv + float2(tsx * -1, 0);
            o.uv[2] = v.uv;
            o.uv[3] = v.uv + float2(tsx * 1, 0);
            o.uv[4] = v.uv + float2(tsx * 2, 0);
            return o;
        }
 
        v2f vertVertical(appdata v)
        {
            v2f o;
            o.pos = TransformObjectToHClip(v.vertex.xyz);
            float tsy = _MainTex_TexelSize.y * _BlurSpread;
            o.uv[0] = v.uv + float2(0, tsy * -2);
            o.uv[1] = v.uv + float2(0, tsy * -1);
            o.uv[2] = v.uv;
            o.uv[3] = v.uv + float2(0, tsy * 1);
            o.uv[4] = v.uv + float2(0, tsy * 2);
            return o;
        }
 
        half4 frag(v2f i) : SV_TARGET
        {
            float g[3] = {0.0545, 0.2442, 0.4026};
            half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv[2]) * g[2];
            for(int k = 0; k < 2; k++)
            {
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv[k]) * g[k];
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv[4 - k]) * g[k];
            }
            return col;
        }
        ENDHLSL

        Pass
        {
            Name "HORIZONTAL"
            ZTest Always
            ZWrite Off
            Cull Off
 
            HLSLPROGRAM
            #pragma vertex vertHorizontal
            #pragma fragment frag
            ENDHLSL
        }
 
        Pass
        {
            Name "VERTICAL"
            ZTest Always
            ZWrite Off
            Cull Off
 
            HLSLPROGRAM
            #pragma vertex vertVertical
            #pragma fragment frag
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}