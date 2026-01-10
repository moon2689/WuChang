//https://www.shadertoy.com/view/tsXBzS
Shader "Saber/Unlit/TheBookOfShaders/ShaderToy3"
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

            float3 palette(float d)
            {
                return lerp(float3(0.2, 0.7, 0.9), float3(1., 0., 1.), d);
            }

            float2 rotate(float2 p, float a)
            {
                float c = cos(a);
                float s = sin(a);
                return mul(float2x2(c, s, -s, c), p);
            }

            float map(float3 p)
            {
                for (int i = 0; i < 8; ++i)
                {
                    float t = _Time.y * 0.2;
                    p.xz = rotate(p.xz, t);
                    p.xy = rotate(p.xy, t * 1.89);
                    p.xz = abs(p.xz);
                    p.xz -= .5;
                }
                return dot(sign(p), p) / 5.;
            }

            float4 rm(float3 ro, float3 rd)
            {
                float t = 0.;
                float3 col = 0;
                float d;
                for (float i = 0.; i < 64.; i++)
                {
                    float3 p = ro + rd * t;
                    d = map(p) * .5;
                    if (d < 0.02)
                    {
                        break;
                    }
                    if (d > 100.)
                    {
                        break;
                    }
                    //col+=float3(0.6,0.8,0.8)/(400.*(d));
                    col += palette(length(p) * .1) / (400. * (d));
                    t += d;
                }
                return float4(col, 1. / (d * 100.));
            }


            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float2 uv = input.uv - 0.5;
                float3 ro = float3(0., 0., -50.);
                ro.xz = rotate(ro.xz, _Time.y);
                float3 cf = normalize(-ro);
                float3 cs = normalize(cross(cf, float3(0., 1., 0.)));
                float3 cu = normalize(cross(cf, cs));

                float3 uuv = ro + cf * 3. + uv.x * cs + uv.y * cu;

                float3 rd = normalize(uuv - ro);

                float4 col = rm(ro, rd);

                col = lerp(0, col, col.a);

                return col;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}