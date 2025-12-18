Shader "Saber/Feature/Post Process SceneScan"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _Color("test", Color) = (1,1,1,1)
        _CenterPos("Center Position", Vector) = (0,0,0,0)
        _ChangeAmount("Change Amount", Range(0,1)) = 0
        _Range("Range", float) = 50
        _EdgeWidth("Edge Width", Range(0.001, 0.1)) = 0.1
        [HDR] _EdgeColor("Edge Color", Color) = (0,0,0,0)
        _NoiseMap("Noise", 2D) = "white"
        _NoiseIntensity("Noise Intensity", float) = 0.1
        [Toggle] _AutoPlay("Auto Play", float) = 0
        _AutoPlaySpeed("Auto Play Speed", float) = 1
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
            #pragma multi_compile _ _AUTOPLAY_ON
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseMap);
            SAMPLER(sampler_NoiseMap);
            
            half4 _Color;
            float4 _CenterPos;
            float _ChangeAmount;
            float _Range;
            float _EdgeWidth;
            half4 _EdgeColor;
            float4 _NoiseMap_ST;
            float _NoiseIntensity;
            float _AutoPlaySpeed;

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
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                float2 screenUV = GetNormalizedScreenSpaceUV(i.pos);
                float rawDepth = SampleSceneDepth(screenUV);
                float3 depthWordPos = ComputeWorldSpacePosition(screenUV, rawDepth, UNITY_MATRIX_I_VP);
                float dis = distance(depthWordPos, _CenterPos.xyz);

                half4 noiseMap = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, i.uv * _NoiseMap_ST.xy + _NoiseMap_ST.zw * _Time.x);
                
                float changeAmount;
                #if _AUTOPLAY_ON
                changeAmount = frac(_Time.x * _AutoPlaySpeed);
                #else
                changeAmount = _ChangeAmount;
                #endif
                
                float gradient = saturate(dis / _Range - changeAmount * 2 + 1);
                gradient = gradient * 2 - noiseMap.r * _NoiseIntensity;

                float edgeWeight = saturate(1 - distance(gradient, 0.5) / _EdgeWidth);
                half3 edgeColor = edgeWeight * _EdgeColor.rgb;

                float clipValue = step(0.5, gradient);
                half grayColor = (mainTex.r + mainTex.g + mainTex.b) * 0.3333;
                
                half4 color;
                color.rgb = lerp(mainTex.rgb, grayColor, clipValue);
                color.rgb += edgeColor * color.rgb;
                color.a = mainTex.a;
                return color;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}