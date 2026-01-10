//https://www.shadertoy.com/view/wcyBD3
Shader "Saber/Unlit/TheBookOfShaders/ShaderToy2"
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

            //2d rotation matrix
            half2 r(half2 v, float t)
            {
                float s = sin(t), c = cos(t);
                return mul(v, float2x2(c, -s, s, c));
            }

            // ACES tonemap: https://www.shadertoy.com/view/Xc3yzM
            half3 a(half3 c)
            {
                float3x3 m1 = float3x3(0.59719, 0.07600, 0.02840, 0.35458, 0.90834, 0.13383, 0.04823, 0.01566, 0.83777);
                float3x3 m2 = float3x3(1.60475, -0.10208, -0.00327, -0.53108, 1.10813, -0.07276, -0.07367, -0.00605, 1.07602);
                half3 v = mul(c, m1), a = v * (v + 0.0245786) - 0.000090537, b = v * (0.983729 * v + 0.4329510) + 0.238081;
                return mul(a / b, m2);
            }

            //Xor's Dot Noise: https://www.shadertoy.com/view/wfsyRX
            float n(half3 p)
            {
                const float PHI = 1.618033988;
                const float3x3 GOLD = float3x3(
                    -0.571464913, +0.814921382, +0.096597072,
                    -0.278044873, -0.303026659, +0.911518454,
                    +0.772087367, +0.494042493, +0.399753815);
                return dot(cos(mul(p, GOLD)), sin(mul(GOLD, mul(p, PHI))));
            }


            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float s, t = _Time.x;
                half3 p, l, b, d;
                p.z = t;
                d = normalize(half3(2. * input.uv - 1, 1));
                for (int i=0; i < 10; i++)
                {
                    b = p;
                    b.xy = r(sin(b.xy), t * 1.5 + b.z * 3.);
                    s = .001 + abs(n(b * 12.) / 12. - n(b)) * .4;
                    s = max(s, 2. - length(p.xy));
                    s += abs(p.y * .75 + sin(p.z + t * .1 + p.x * 1.5)) * .2;
                    p += d * s;
                    l += (1. + sin(i + length(p.xy * .1) + half3(3, 1.5, 1))) / s;
                }
                half4 color;
                color.rgb = a(l * l / 6e2);
                color.a = 1;
                return color;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}