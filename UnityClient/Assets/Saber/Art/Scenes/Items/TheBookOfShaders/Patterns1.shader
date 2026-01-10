Shader "Saber/Unlit/TheBookOfShaders/Patterns1"
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

            float2 rotate2D(float2 _st, float _angle)
            {
                float2x2 mat = float2x2(cos(_angle), -sin(_angle),
                                        sin(_angle), cos(_angle));

                _st -= 0.5;
                _st = mul(_st, mat);
                _st += 0.5;
                return _st;
            }

            float2 tile(float2 _st, float _zoom)
            {
                _st *= _zoom;
                return frac(_st);
            }

            float box(float2 _st, float2 _size, float _smoothEdges)
            {
                _size = 0.5 - _size * 0.5;
                float2 aa = _smoothEdges * 0.5;
                float2 uv = smoothstep(_size, _size + aa, _st);
                uv *= smoothstep(_size, _size + aa, 1.0 - _st);
                return uv.x * uv.y;
            }

            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float2 st = input.uv; //gl_FragCoord.xy / u_resolution.xy;

                // Divide the space in 4
                st = tile(st, 4.);

                // Use a matrix to rotate the space 45 degrees
                st = rotate2D(st,PI * 0.25);

                // Draw a square
                half3 color = box(st, 0.7, 0.01);
                //color = float3(st, 0.0);

                return half4(color, 1);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}