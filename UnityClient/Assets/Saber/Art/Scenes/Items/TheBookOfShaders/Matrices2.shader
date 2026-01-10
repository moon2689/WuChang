Shader "Saber/Unlit/TheBookOfShaders/Matrices2"
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

            float2x2 rotate2d(float _angle)
            {
                return float2x2(cos(_angle), -sin(_angle),
                                sin(_angle), cos(_angle));
            }

            float box(in half2 _st, in half2 _size)
            {
                _size = 0.5 - _size * 0.5;
                half2 uv = smoothstep(_size, _size + 0.001, _st);
                uv *= smoothstep(_size, _size + 0.001, 1.0 - _st);
                return uv.x * uv.y;
            }

            float cross(in half2 _st, float _size)
            {
                return box(_st, half2(_size, _size / 4.)) +
                    box(_st, half2(_size / 4., _size));
            }

            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half2 st = input.uv; //gl_FragCoord.xy/u_resolution.xy;
                float t = _Time.y;

                // To move the cross we move the space
                //half2 translate = half2(cos(t), sin(t));
                //st += translate * 0.35;

                // move space from the center to the vec2(0.0)
                st -= 0.5;
                // rotate the space
                st = mul(rotate2d(sin(t) * PI), st);
                // move it back to the original place
                st += 0.5;

                half3 color = 0;
                // Show the coordinates of the space on the background
                //color += half3(st.x, st.y, 0.0);

                // Add the shape on the foreground
                color += cross(st, 0.25);

                return half4(color, 1);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}