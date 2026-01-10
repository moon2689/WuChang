Shader "Saber/Unlit/TheBookOfShaders/Random2"
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

            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            float2 truchetPattern(in float2 _st, in float _index)
            {
                _index = frac(((_index - 0.5) * 2.0));
                if (_index > 0.75)
                {
                    _st = 1.0 - _st;
                }
                else if (_index > 0.5)
                {
                    _st = float2(1.0 - _st.x, _st.y);
                }
                else if (_index > 0.25)
                {
                    _st = 1.0 - float2(1.0 - _st.x, _st.y);
                }
                return _st;
            }

            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float2 st = input.uv; //gl_FragCoord.xy / u_resolution.xy;
                st *= 10.0;
                //st = (st - 5.0) * (abs(sin(_Time.y * 0.2)) * 5.);
                //st.x += _Time.y * 3.0;

                float2 ipos = floor(st); // integer
                float2 fpos = frac(st); // fraction

                float2 tile = truchetPattern(fpos, random(ipos));

                float color = 0.0;

                // Maze
                color = smoothstep(tile.x - 0.3, tile.x, tile.y) - smoothstep(tile.x, tile.x + 0.3, tile.y);

                // Circles
                //color = (step(length(tile), 0.6) - step(length(tile), 0.4)) + (step(length(tile - 1.), 0.6) - step(length(tile - 1.), 0.4));

                // Truchet (2 triangles)
                //color = step(tile.x, tile.y);

                return half4(color.xxx, 1);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}