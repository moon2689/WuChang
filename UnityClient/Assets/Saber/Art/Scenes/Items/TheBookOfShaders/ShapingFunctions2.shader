Shader "Saber/Unlit/TheBookOfShaders/ShapingFunctions2"
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

            float Plot(float2 uv, float pct)
            {
                return smoothstep(pct - 0.02, pct, uv.y) - smoothstep(pct, pct + 0.02, uv.y);
            }

            //  Function from Iñigo Quiles
            //  www.iquilezles.org/www/articles/functions/functions.htm
            float Impulse(float k, float x)
            {
                float h = k * x;
                return h * exp(1.0 - h);
            }

            //  Function from Iñigo Quiles
            //  www.iquilezles.org/www/articles/functions/functions.htm
            float CubicPulse(float c, float w, float x)
            {
                x = abs(x - c);
                if (x > w)
                    return 0.0;
                x /= w;
                return 1.0 - x * x * (3.0 - 2.0 * x);
            }

            //  Function from Iñigo Quiles
            //  www.iquilezles.org/www/articles/functions/functions.htm
            float ExpStep(float x, float k, float n)
            {
                return exp(-k * pow(x, n));
            }

            //  Function from Iñigo Quiles
            //  www.iquilezles.org/www/articles/functions/functions.htm
            float Parabola(float x, float k)
            {
                return pow(4.0 * x * (1.0 - x), k);
            }

            //  Function from Iñigo Quiles
            //  www.iquilezles.org/www/articles/functions/functions.htm
            float Pcurve(float x, float a, float b)
            {
                float k = pow(a + b, a + b) / (pow(a, a) * pow(b, b));
                return k * pow(x, a) * pow(1.0 - x, b);
            }

            float Function(float x)
            {
                float y = 0;
                //y = x;
                //y = pow(x, 5);
                //y = step(0.5, x);
                //y = smoothstep(0.1, 0.9, x);
                //y = smoothstep(0.2,0.5,x) - smoothstep(0.5,0.8,x);
                //y = exp(-x*3);
                //y = sin((x+_Time.x)*30) * 0.2 + 0.5;
                //y = sin(x*_Time.y*10) * 0.2 + 0.5;
                //y = abs(sin(x * 10));
                //y = frac(sin(x * 10)) * 0.2 + 0.5;
                //y = ceil(sin((x+_Time.x) * 10)) * 0.2 + 0.5;
                //y = fmod(x*10, 0.5);
                //y = frac(x * 10);
                //y = floor(x*10)/20;
                //y = sign(x);

                //y = Impulse(16, x);
                //y = CubicPulse(0.5, 0.2, x);
                //y = ExpStep(x, 10, 1);
                //y = Parabola(x, 1);
                //y = Pcurve(x, 3, 1);
                
                //y = smoothstep(-.5, 1., cos(x * 30.)) * 0.2 + 0.5;

                //y = abs(cos(x * 12.) * sin(x * 3.)) * .8 + .1;
                y = frac(sin(x*10));

                return y;
            }

            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float2 uv = input.uv;
                float x = uv.x;
                float y = Function(x);

                float pct = Plot(uv, y);
                half3 color = lerp(y.xxx, _BaseColor, pct);

                return half4(color, 1);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}