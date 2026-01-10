Shader "Saber/Unlit/TheBookOfShaders/Noise7_SimplexNoise"
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


            // Some useful functions
            float3 mod289(float3 x)
            {
                return x - floor(x * (1.0 / 289.0)) * 289.0;
            }

            float2 mod289(float2 x)
            {
                return x - floor(x * (1.0 / 289.0)) * 289.0;
            }

            float3 permute(float3 x)
            {
                return mod289(((x * 34.0) + 1.0) * x);
            }

            //
            // Description : GLSL 2D simplex noise function
            //      Author : Ian McEwan, Ashima Arts
            //  Maintainer : ijm
            //     Lastmod : 20110822 (ijm)
            //     License :
            //  Copyright (C) 2011 Ashima Arts. All rights reserved.
            //  Distributed under the MIT License. See LICENSE file.
            //  https://github.com/ashima/webgl-noise
            //
            float snoise(float2 v)
            {
                // Precompute values for skewed triangular grid
                const float4 C = float4(0.211324865405187,
                                        // (3.0-sqrt(3.0))/6.0
                                        0.366025403784439,
                                        // 0.5*(sqrt(3.0)-1.0)
                                        -0.577350269189626,
                                        // -1.0 + 2.0 * C.x
                                        0.024390243902439);
                // 1.0 / 41.0

                // First corner (x0)
                float2 i = floor(v + dot(v, C.yy));
                float2 x0 = v - i + dot(i, C.xx);

                // Other two corners (x1, x2)
                float2 i1 = 0.0;
                i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
                float2 x1 = x0.xy + C.xx - i1;
                float2 x2 = x0.xy + C.zz;

                // Do some permutations to avoid
                // truncation effects in permutation
                i = mod289(i);
                float3 p = permute(
                    permute(i.y + float3(0.0, i1.y, 1.0))
                    + i.x + float3(0.0, i1.x, 1.0));

                float3 m = max(0.5 - float3(
                                   dot(x0, x0),
                                   dot(x1, x1),
                                   dot(x2, x2)
                               ), 0.0);

                m = m * m;
                m = m * m;

                // Gradients:
                //  41 pts uniformly over a line, mapped onto a diamond
                //  The ring size 17*17 = 289 is close to a multiple
                //      of 41 (41*7 = 287)

                float3 x = 2.0 * frac(p * C.www) - 1.0;
                float3 h = abs(x) - 0.5;
                float3 ox = floor(x + 0.5);
                float3 a0 = x - ox;

                // Normalise gradients implicitly by scaling m
                // Approximation of: m *= inversesqrt(a0*a0 + h*h);
                m *= 1.79284291400159 - 0.85373472095314 * (a0 * a0 + h * h);

                // Compute final noise value at P
                float3 g = 0.0;
                g.x = a0.x * x0.x + h.x * x0.y;
                g.yz = a0.yz * float2(x1.x, x2.x) + h.yz * float2(x1.y, x2.y);
                return 130.0 * dot(m, g);
            }


            /*
             * 对于 Ken Perlin 来说他的算法所取得的成功是远远不够的。他觉得可以更好。在 2001 年的 Siggraph
             * （译者注：Siggraph是由美国计算机协会「计算机图形专业组」组织的计算机图形学顶级年度会议）上，他展示了 “simplex noise”，simplex noise 比之前的算法有如下优化：
             * 它有着更低的计算复杂度和更少乘法计算。
             * 它可以用更少的计算量达到更高的维度。
             * 制造出的 noise 没有明显的人工痕迹。
             * 有着定义得很精巧的连续的 gradients（梯度），可以大大降低计算成本。
             * 特别易于硬件实现。
             */
            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float2 st = input.uv; //gl_FragCoord.xy / u_resolution.xy;
                //st.x *= u_resolution.x / u_resolution.y;

                float3 color = 0.0;

                // Scale the space in order to see the function
                st *= 10.;

                color = snoise(st) * .5 + .5;

                return float4(color, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}