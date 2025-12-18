Shader "Saber/Unlit/Dissolve/URP Burn Flag"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        [Header(VAT)]
        _boundingMax("Bounding Max", Float) = 1.0
        _boundingMin("Bounding Min", Float) = 1.0
        _numOfFrames("Number Of Frames", int) = 240
        _speed("Speed", Float) = 0.33
        [MaterialToggle] _pack_normal ("Pack Normal", Float) = 0
        _posTex ("Position Map (RGB)", 2D) = "white" {}
        _nTex ("Normal Map (RGB)", 2D) = "grey" {}

        [Header(Dissolve)]
        _ChangeAmount("Change Amount", Range(0, 1)) = 0
        [Toggle] _AutoDissolve("Auto Dissolve", float) = 0
        _FlagHeight("Flag Height", float) = 1
        _EdgeWidth("Edge Width", Range(0.001, 1)) = 0.1
        [HDR] _EdgeColor("Edge Color", Color) = (0,0,0,0)
        _DissolveNoiseMap("Dissolve Noise Map", 2D) = "white" {}
        _DissolveNoiseIntensity("Dissolve Noise Intensity", Range(0, 1)) = 0.2
        _DissolveSpread("Dissolve Spread", Range(0.001, 1)) = 1
        
        [Header(Charring)]
        _CharringOffset("Charring Offset", Range(0, 1)) = 0.5
        _CharringWidth("Charring Width", Range(0.001, 1)) = 0.5
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
            
            #pragma multi_compile _ _AUTODISSOLVE_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_posTex);
            SAMPLER(sampler_posTex);
            TEXTURE2D(_nTex);
            SAMPLER(sampler_nTex);
            TEXTURE2D(_DissolveNoiseMap);
            SAMPLER(sampler_DissolveNoiseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float _Glossiness;
                float _Metallic;
                float _boundingMax;
                float _boundingMin;
                float _numOfFrames;
                float _speed;
                float _pack_normal;

                // dissolve
                float _ChangeAmount;
                float _AutoDissolve;
                float _FlagHeight;
                float _EdgeWidth;
                half4 _EdgeColor;
                float4 _DissolveNoiseMap_ST;
                float _DissolveNoiseIntensity;
                float _DissolveSpread;
            
                float _CharringOffset;
                float _CharringWidth;
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
                float3 positionWS : TEXCOORD2;
                float3 dissolveStartPosWS : TEXCOORD3;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings UnlitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                //calcualte uv coordinates
                float timeInFrames = (ceil(frac(-_Time.y * _speed) * _numOfFrames) + 1) / _numOfFrames;

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
                
                // dissolve
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.dissolveStartPosWS = TransformObjectToWorld(float3(0,0,0));

                return output;
            }
            
            // Remap from range [oldMin, oldMax] to [newMin, newMax]
            float Remap(float value, float oldMin, float oldMax, float newMin, float newMax)
            {
                return newMin + (value - oldMin) * (newMax - newMin) / (oldMax - oldMin);
            }

            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                // dissolve
                float changeAmount;
                #if _AUTODISSOLVE_ON
                changeAmount = frac(_Time.x * 2);
                #else
                changeAmount  = _ChangeAmount;
                #endif
                
                float posYRate = (input.positionWS.y - input.dissolveStartPosWS.y) / _FlagHeight;
                float gradient = posYRate - Remap(changeAmount, 0, 1, -_DissolveSpread, 1) / _DissolveSpread;
                half4 dissolveNoiseMap = SAMPLE_TEXTURE2D(_DissolveNoiseMap, sampler_DissolveNoiseMap, input.uv * _DissolveNoiseMap_ST.xy + _DissolveNoiseMap_ST.zw * _Time.x);
                float gradientNoise = gradient * 2 - dissolveNoiseMap.r * _DissolveNoiseIntensity;
                
                float edgeWeight = saturate(1 - distance(gradientNoise, 0.5) / _EdgeWidth);
                half3 edgeColor = edgeWeight * _EdgeColor.rgb;
                
                float clipValue = step(0.5, gradientNoise);
                clip(clipValue - 0.5);
                
                // charring
                float charringWeight = saturate(distance(gradientNoise, _CharringOffset) / _CharringWidth);
                //charringWeight = charringWeight * 2 - 1;
                
                // 光照
                half3 albedo = mainTex.rgb * charringWeight;
                Light mainLight = GetMainLight();
                float3 N = normalize(input.normalWS);
                float3 V = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float3 L = mainLight.direction;
                float NoL = max(0,dot(N,L));
                half3 diffuse = albedo * mainLight.color * NoL;
                
                float3 H = normalize(V + L);
                float NoH = max(0,dot(N,H));
                float3 specular = albedo * mainLight.color * pow(NoH, 20);
                
                half4 color;
                color.rgb = diffuse + specular + edgeColor;
                color.a = 1;
                //color.rgb = charringWeight;
                return color;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}