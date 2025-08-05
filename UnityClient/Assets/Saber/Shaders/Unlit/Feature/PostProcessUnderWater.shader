Shader "Saber/Feature/Post Process Under Water"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _ShallowColor("Shallow Color", Color) = (1,1,1,1)
        _DepthColor("Depth Color", Color) = (0,0,0,1)
        _DepthStart("Depth Start", float) = 0
        _DepthEnd("Depth End", float) = 20
        
        _NoiseTex("Noise Texture", 2D) = "black" {}
        _Distortion("Distortion", float) = 10
        
        _ScreenPosY("Screen Pos Y", float) = 0
        
        //_WaterLevelInCamera("Water Level In Camera", Range(0, 1)) = 1
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);
            
            half4 _ShallowColor;
            half4 _DepthColor;
            float _DepthStart;
            float _DepthEnd;
            float4 _NoiseTex_ST;
            float4 _CameraDepthTexture_TexelSize;
            //float _WaterLevelInCamera;
            float _Distortion;
            float _ScreenPosY;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.positionCS = mul(UNITY_MATRIX_MVP, v.vertex);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.positionCS);
                return o;
            }

            half4 frag(v2f i) : SV_TARGET
            {
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                
                float2 uv_NoiseTex = TRANSFORM_TEX(i.uv, _NoiseTex) * 10 + _Time.xx * 5;
                half4 noiseTex = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, uv_NoiseTex);
                float2 uvDistortion = i.uv + noiseTex.xx * _CameraDepthTexture_TexelSize.xy * _Distortion;
                
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half4 mainTexDistortion = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvDistortion);
                
                float rawDepth = SampleSceneDepth(screenUV);
                float depth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float rate = saturate((depth - _DepthStart) / (_DepthEnd - _DepthStart));
                half3 shallowColor = mainTexDistortion.rgb * _ShallowColor.rgb;
                half3 depthColor = lerp(mainTexDistortion.rgb, _DepthColor.rgb, _DepthColor.a);
                half3 waterColor = lerp(shallowColor, depthColor, rate);
                //color = rate;

                float mask = step(screenUV.y - _ScreenPosY, 0);
                half4 color = lerp(mainTex, half4(waterColor, 1), mask);
                
                return color;
            }

            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}