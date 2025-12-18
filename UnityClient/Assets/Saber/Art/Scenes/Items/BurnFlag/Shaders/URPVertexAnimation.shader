Shader "Saber/Unlit/Dissolve/URP Vertex Animation"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _boundingMax("Bounding Max", Float) = 1.0
        _boundingMin("Bounding Min", Float) = 1.0
        _numOfFrames("Number Of Frames", int) = 240
        _speed("Speed", Float) = 0.33
        [MaterialToggle] _pack_normal ("Pack Normal", Float) = 0
        _posTex ("Position Map (RGB)", 2D) = "white" {}
        _nTex ("Normal Map (RGB)", 2D) = "grey" {}
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
            #pragma target 3.5
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_posTex);
            SAMPLER(sampler_posTex);
            TEXTURE2D(_nTex);
            SAMPLER(sampler_nTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _Color;
                float _Glossiness;
                float _Metallic;
                float _boundingMax;
                float _boundingMin;
                float _numOfFrames;
                float _speed;
                float _pack_normal;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD1;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings UnlitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                //calcualte uv coordinates
                float timeInFrames = ((ceil(frac(-_Time.y * _speed) * _numOfFrames)) / _numOfFrames) + (1.0 / _numOfFrames);

                //get position and normal from textures
                float4 texturePos = SAMPLE_TEXTURE2D_LOD(_posTex, sampler_posTex, float2(input.uv1.x, (timeInFrames + input.uv1.y)), 0);
                float3 textureN = SAMPLE_TEXTURE2D_LOD(_nTex, sampler_nTex, float2(input.uv1.x, (timeInFrames + input.uv1.y)), 0);
                
                //expand normalised position texture values to world space
                float expand = _boundingMax - _boundingMin;
                texturePos.xyz *= expand;
                texturePos.xyz += _boundingMin;
                texturePos.x *= -1; //flipped to account for right-handedness of unity
                input.positionOS.xyz += texturePos.xzy; //swizzle y and z because textures are exported with z-up

                //calculate normal
                if (_pack_normal)
                {
                    //decode float to float2
                    float alpha = texturePos.w * 1024;
                    float2 f2;
                    f2.x = floor(alpha / 32.0) / 31.5;
                    f2.y = (alpha - (floor(alpha / 32.0) * 32.0)) / 31.5;

                    //decode float2 to float3
                    float3 f3;
                    f2 *= 4;
                    f2 -= 2;
                    float f2dot = dot(f2, f2);
                    f3.xy = sqrt(1 - (f2dot / 4.0)) * f2;
                    f3.z = 1 - (f2dot / 2.0);
                    f3 = clamp(f3, -1.0, 1.0);
                    f3 = f3.xzy;
                    f3.x *= -1;
                    output.normalWS = f3;
                }
                else
                {
                    textureN = textureN.xzy;
                    textureN *= 2;
                    textureN -= 1;
                    textureN.x *= -1;
                    output.normalWS = textureN;
                }
                
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;

                return output;
            }

            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half2 uv = input.uv;
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                half3 color = texColor.rgb * _Color.rgb;
                half alpha = texColor.a * _Color.a;

                half4 finalColor = half4(color, alpha);
                return finalColor;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}