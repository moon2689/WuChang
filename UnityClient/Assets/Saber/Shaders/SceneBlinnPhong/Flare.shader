Shader "Saber/FX/Flare"
{
    Properties
    {
        _MainTex ("Particle Texture", 2D) = "black" {}
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }
        Cull Off Lighting Off ZWrite Off Ztest Always
        Blend One One

        Pass
        {
            HLSLPROGRAM
            #pragma target 2.0
            
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			TEXTURE2D(_MainTex);  SAMPLER(sampler_MainTex);
			half4 _TintColor;
			float4 _MainTex_ST;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				half4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.color = v.color;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                return o;
            }

            half4 frag(v2f i) : SV_TARGET
            {
                half4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half4 color;
				color.rgb = i.color.rgb * baseMap.rgb;
				color.a = baseMap.a;
                return color;
            }
            ENDHLSL
        }
    }
}