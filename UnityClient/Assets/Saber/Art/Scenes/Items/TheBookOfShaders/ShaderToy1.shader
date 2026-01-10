Shader "Saber/Unlit/TheBookOfShaders/ShaderToy1"
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


            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);
                
                // https://www.shadertoy.com/view/XsXXDn
                half3 c;
                float l, z = _Time.y;
                for (int i = 0; i < 3; ++i)
                {
                    half2 uv, p = input.uv;
                    uv = p;
                    p -= .5;
                    //p.x *= r.x / r.y;
                    z += .07;
                    l = length(p);
                    uv += p / l * (sin(z) + 1.) * abs(sin(l * 9. - z - z));
                    c[i] = .01 / length(fmod(uv, 1.) - .5);
                }

                return half4(c / l, 1);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}