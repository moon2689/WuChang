Shader "Saber/Unlit/TheBookOfShaders/Noise2 ValueNoise"
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
            
            /*
             * Noise 算法的设计初衷是将难以言说的自然质感转化成数字图像。在目前我们看到的一维和二维的实践中，
             * 都是在random values（随机值）之间插值，所以它们才被叫做 Value Noise，但是还有很多很多获取 noise 的方法……
             */

            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float2 st = input.uv; //gl_FragCoord.xy / u_resolution.xy;
                //st.x *= u_resolution.x / u_resolution.y;

                float2 pos = float2(st * 10.0);

                float3 color = noise(pos) * .5 + .5;

                return half4(color, 1);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}