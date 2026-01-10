Shader "Saber/Unlit/TheBookOfShaders/Noise4"
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

            float random2(float2 st)
            {
                st = float2(dot(st, float2(127.1, 311.7)),
                            dot(st, float2(269.5, 183.3)));

                return -1.0 + 2.0 * frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            // Value Noise by Inigo Quilez - iq/2013
            // https://www.shadertoy.com/view/lsf3WH
            float noise(float2 st)
            {
                float2 i = floor(st);
                float2 f = frac(st);

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(lerp(random2(i + float2(0.0, 0.0)),
                                 random2(i + float2(1.0, 0.0)), u.x),
                            lerp(random2(i + float2(0.0, 1.0)),
                                 random2(i + float2(1.0, 1.0)), u.x), u.y);
            }

            float2x2 rotate2d(float angle)
            {
                return float2x2(cos(angle), -sin(angle),
                                sin(angle), cos(angle));
            }

            float lines(in float2 pos, float b)
            {
                float scale = 10.0;
                pos *= scale;
                return smoothstep(0.0, .5 + b * .5, abs((sin(pos.x * 3.1415) + b * 2.0)) * .5);
            }
            
            /*
             * 就像一个画家非常了解画上的颜料是如何晕染的，我们越了解 noise 是如何运作的，越能更好地使用 noise。
             * 比如，如果我们要用一个二维的 noise 来旋转空间中的直线，我们就可以制作下图的旋涡状效果，看起来就像木头表皮一样。
             */

            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float2 st = input.uv; //gl_FragCoord.xy/u_resolution.xy;
                //st.y *= u_resolution.y/u_resolution.x;

                float2 pos = st.yx * float2(10., 3.);

                // Add noise
                pos = mul(pos, rotate2d(noise(pos)));

                // Draw lines
                float pattern = lines(pos, .5);

                return half4(pattern.xxx, 1);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}