Shader "Saber/Unlit/TheBookOfShaders/Shapes2"
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

                float2 st = input.uv; //gl_FragCoord.xy / u_resolution.xy;
                //st.x *= u_resolution.x / u_resolution.y;
                half3 color;

                // Remap the space to -1. to 1.
                st = st * 2. - 1.;

                // Make the distance field
                float d = length(abs(st) - .3);
                //d = length(min(abs(st) - .3, 0.));
                //d = length(max(abs(st) - .3, 0.));

                // Visualize the distance field
                color = frac(d * 10.0);

                // Drawing with the distance field
                //color = step(.3,d);
                //color = step(.3, d) * step(d, .4);
                //color = smoothstep(.3, .4, d) * smoothstep(.6, .5, d);

                return half4(color, 1);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}