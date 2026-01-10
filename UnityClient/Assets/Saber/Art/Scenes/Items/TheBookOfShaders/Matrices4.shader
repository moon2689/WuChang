Shader "Saber/Unlit/TheBookOfShaders/Matrices4"
{
    Properties
    {
        [MainColor] _BaseColor("Color", Color) = (0, 1, 0, 1)
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings UnlitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;

                return output;
            }


            /*
             * YUV 是个用来模拟照片和视频的编码的色彩空间。这个色彩空间考虑人类的感知，减少色度的带宽。
             * 下面的代码展现一种利用GLSL中的矩阵操作来切换颜色模式的有趣可能。
             */
            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);
                // YUV to RGB matrix
                float3x3 yuv2rgb = float3x3(1.0, 0.0, 1.13983,
                                            1.0, -0.39465, -0.58060,
                                            1.0, 2.03211, 0.0);

                // RGB to YUV matrix
                float3x3 rgb2yuv = float3x3(0.2126, 0.7152, 0.0722,
                                    -0.09991, -0.33609, 0.43600,
                                    0.615, -0.5586, -0.05639);
                
                half2 st = input.uv; //gl_FragCoord.xy/u_resolution.xy;

                // UV values goes from -1 to 1
                // So we need to remap st (0.0 to 1.0)
                st -= 0.5; // becomes -0.5 to 0.5
                st *= 2.0; // becomes -1.0 to 1.0

                // we pass st as the y & z values of
                // a three dimensional vector to be
                // properly multiply by a 3x3 matrix
                half3 color = mul(float3(0.5, st.x, st.y), yuv2rgb);

                return half4(color, 1);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}