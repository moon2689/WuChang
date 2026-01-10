Shader "Saber/Unlit/TheBookOfShaders/Noise6"
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

            float2 random2(float2 st)
            {
                st = float2(dot(st, float2(127.1, 311.7)),
                            dot(st, float2(269.5, 183.3)));
                return -1.0 + 2.0 * frac(sin(st) * 43758.5453123);
            }

            // Gradient Noise by Inigo Quilez - iq/2013
            // https://www.shadertoy.com/view/XdXGW8
            float noise(float2 st)
            {
                float2 i = floor(st);
                float2 f = frac(st);

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(lerp(dot(random2(i + float2(0.0, 0.0)), f - float2(0.0, 0.0)),
                                 dot(random2(i + float2(1.0, 0.0)), f - float2(1.0, 0.0)), u.x),
                            lerp(dot(random2(i + float2(0.0, 1.0)), f - float2(0.0, 1.0)),
                                 dot(random2(i + float2(1.0, 1.0)), f - float2(1.0, 1.0)), u.x), u.y);
            }

            float2x2 rotate2d(float _angle)
            {
                return float2x2(cos(_angle), -sin(_angle),
                                  sin(_angle), cos(_angle));
            }

            float shape(float2 st, float radius)
            {
                st = 0.5 - st;
                float r = length(st) * 2.0;
                float a = atan2(st.y, st.x);
                float m = abs(fmod(a + _Time.y * 2., 3.14 * 2.) - 3.14) / 3.6;
                float f = radius;
                m += noise(st + _Time.y * 0.1) * .5;
                // a *= 1.+abs(atan(u_time*0.2))*.1;
                // a *= 1.+noise(st+u_time*0.1)*0.1;
                f += sin(a * 50.) * noise(st + _Time.y * .2) * .1;
                f += (sin(a * 20.) * .1 * pow(m, 2.));
                return 1. - smoothstep(f, f + 0.007, r);
            }

            float shapeBorder(float2 st, float radius, float width)
            {
                return shape(st, radius) - shape(st, radius - width);
            }

            //第三种方法是用 noise 函数来变换一个形状。
            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float2 st = input.uv; //gl_FragCoord.xy / u_resolution.xy;
                float3 color = 1.0 * shapeBorder(st, 0.8, 0.02);

                return float4(1. - color, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}