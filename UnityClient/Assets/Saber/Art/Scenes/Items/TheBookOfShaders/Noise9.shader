Shader "Saber/Unlit/TheBookOfShaders/Noise9"
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


            float shape(float2 st, int N)
            {
                st = st * 2. - 1.;
                float a = atan2(st.x, st.y) + PI;
                float r = TWO_PI / float(N);
                return cos(floor(.5 + a / r) * r - a) * length(st);
            }

            float box(float2 st, float2 size)
            {
                return shape(st * size, 4);
            }

            float hex(float2 st, bool a, bool b, bool c, bool d, bool e, bool f)
            {
                st = st * float2(2., 6.);

                float2 fpos = frac(st);
                float2 ipos = floor(st);

                if (ipos.x == 1.0) fpos.x = 1. - fpos.x;
                if (ipos.y < 1.0)
                {
                    return a ? box(fpos - float2(0.03, 0.), 1.) : box(fpos, float2(0.84, 1.));
                }
                else if (ipos.y < 2.0)
                {
                    return b ? box(fpos - float2(0.03, 0.), 1.) : box(fpos, float2(0.84, 1.));
                }
                else if (ipos.y < 3.0)
                {
                    return c ? box(fpos - float2(0.03, 0.), 1.) : box(fpos, float2(0.84, 1.));
                }
                else if (ipos.y < 4.0)
                {
                    return d ? box(fpos - float2(0.03, 0.), 1.) : box(fpos, float2(0.84, 1.));
                }
                else if (ipos.y < 5.0)
                {
                    return e ? box(fpos - float2(0.03, 0.), 1.) : box(fpos, float2(0.84, 1.));
                }
                else if (ipos.y < 6.0)
                {
                    return f ? box(fpos - float2(0.03, 0.), 1.) : box(fpos, float2(0.84, 1.));
                }
                return 0.0;
            }

            float hex(float2 st, float N)
            {
                bool b[6];
                float remain = floor(fmod(N, 64.));
                for (int i = 0; i < 6; i++)
                {
                    b[i] = fmod(remain, 2.) == 1. ? true : false;
                    remain = ceil(remain / 2.);
                }
                return hex(st, b[0], b[1], b[2], b[3], b[4], b[5]);
            }

            float3 random3(float3 c)
            {
                float j = 4096.0 * sin(dot(c, float3(17.0, 59.4, 15.0)));
                float3 r;
                r.z = frac(512.0 * j);
                j *= .125;
                r.x = frac(512.0 * j);
                j *= .125;
                r.y = frac(512.0 * j);
                return r - 0.5;
            }

            const float F3 = 0.3333333;
            const float G3 = 0.1666667;

            float snoise(float3 p)
            {
                float3 s = floor(p + dot(p, F3));
                float3 x = p - s + dot(s, G3);

                float3 e = step(0.0, x - x.yzx);
                float3 i1 = e * (1.0 - e.zxy);
                float3 i2 = 1.0 - e.zxy * (1.0 - e);

                float3 x1 = x - i1 + G3;
                float3 x2 = x - i2 + 2.0 * G3;
                float3 x3 = x - 1.0 + 3.0 * G3;

                float4 w, d;

                w.x = dot(x, x);
                w.y = dot(x1, x1);
                w.z = dot(x2, x2);
                w.w = dot(x3, x3);

                w = max(0.6 - w, 0.0);

                d.x = dot(random3(s), x);
                d.y = dot(random3(s + i1), x1);
                d.z = dot(random3(s + i2), x2);
                d.w = dot(random3(s + 1.0), x3);

                w *= w;
                w *= w;
                d *= w;

                return dot(d, 52.0);
            }


            //用 Simplex Noise 给你现在的作品添加更多的材质效果。
            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float2 st = input.uv; // gl_FragCoord.xy / u_resolution.xy;
                //st.y *= u_resolution.y / u_resolution.x;

                float t = _Time.y * 0.5;

                float df = 1.0;
                df = lerp(hex(st, t), hex(st, t + 1.), frac(t));
                df += snoise(float3(st * 75., t * 0.1)) * 0.03;
                return float4(lerp(0., 1., step(0.7, df)).xxx, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}