Shader "Saber/Unlit/Toon Water/Water"
{
    Properties
    {
        [Header(Color)]
        _DeepRange("Deep Range", float) = 5
        _ShallowColor("Shallow Color", Color) = (1, 1, 1, 1)
        _DeepColor("Deep Color", Color) = (1, 1, 1, 1)
        _FresnelColor("Fresnel Color", Color) = (1, 1, 1, 1)
        _FresnelPower("Fresnel Power", float) = 10
        _FresnelScale("Fresnel Scale", float) = 1

        [Header(Normal)]
        _NormalMap("Normal Map", 2D) = "bump" {}
        _WaveStrength1("Wave Strength 1", Vector) = (1,1,0,0)
        _WaveStrength2("Wave Strength 2", Vector) = (1,1,0,0)
        _NormalScale("Normal Scale", Range(0, 2)) = 1
        _ReflectionNoise("Reflection Noise", float) = 1

        _UnderwaterNoise("Underwater Noise", float) = 1

        [Header(Caustics)]
        _CausticsMap("Caustics Map", 2D) = "black" {}
        _CausticsIntensity("Caustics Intensity", float) = 1
        _CausticsRange("Caustics Range", float) = 1

        [Header(Shore)]
        _ShoreRange("Shore Range", float) = 5
        _ShoreColor("Shore Edge Color", Color) = (1,1,1,1)
        _ShoreEdgeWidth("Shore Edge Width", Range(0,1)) = 0.1
        _ShoreEdgeIntensity("Shore Edge Intensity", Range(0,1)) = 0.2

        [Header(Foam)]
        _FoamColor("Foam Color", Color) = (1,1,1,1)
        _FoamRange("Foam Range", float) = 1
        _FoamSpeed("Foam Speed", float) = 1
        _FoamFrequency("Foam Frequency", float) = 20
        _FoamNoiseMap("Foam Noise Map", 2D) = "black" {}
        _FoamBlend("Foam Blend", Range(0,1)) = 0
        _FoamDissolve("Foam Dissolve", Range(0,2)) = 0
        _FoamWidth("Foam Width", Float) = 1

        [Header(Wave)]
        _WaveA ("Wave A (dir, steepness, wavelength)", Vector) = (1,0,0.5,10)
        _WaveB ("Wave B", Vector) = (0,1,0.25,20)
        _WaveC ("Wave C", Vector) = (1,1,0.15,10)
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            TEXTURE2D(_ReflectionTex);
            SAMPLER(sampler_ReflectionTex);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            TEXTURE2D(_CausticsMap);
            SAMPLER(sampler_CausticsMap);
            TEXTURE2D(_FoamNoiseMap);
            SAMPLER(sampler_FoamNoiseMap);

            CBUFFER_START(UnityPerMaterial)
                float _DeepRange;
                half4 _ShallowColor;
                half4 _DeepColor;
                half4 _FresnelColor;
                float _FresnelPower;
                float _FresnelScale;

                float _ReflectionNoise;
                float4 _WaveStrength1;
                float4 _WaveStrength2;
                float _NormalScale;

                float _UnderwaterNoise;

                float4 _CausticsMap_ST;
                float _CausticsIntensity;
                float _CausticsRange;

                float _ShoreRange;
                half4 _ShoreColor;
                float _ShoreEdgeWidth;
                float _ShoreEdgeIntensity;

                half4 _FoamColor;
                float _FoamRange;
                float _FoamSpeed;
                float _FoamFrequency;
                float4 _FoamNoiseMap_ST;
                float _FoamBlend;
                float _FoamDissolve;
                float _FoamWidth;

                float4 _WaveA, _WaveB, _WaveC;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 tangentWS : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // 参考：https://catlikecoding.com/unity/tutorials/flow/waves/
            float3 GerstnerWave(float4 wave, float3 p, inout float3 tangent, inout float3 binormal)
            {
                float steepness = wave.z * 0.01;
                float wavelength = wave.w;
                float k = 2 * PI / wavelength;
                float c = sqrt(9.8 / k);
                float2 d = normalize(wave.xy);
                float f = k * (dot(d, p.xz) - c * _Time.y);
                float a = steepness / k;

                //p.x += d.x * (a * cos(f));
                //p.y = a * sin(f);
                //p.z += d.y * (a * cos(f));

                tangent += float3(
                    -d.x * d.x * (steepness * sin(f)),
                    d.x * (steepness * cos(f)),
                    -d.x * d.y * (steepness * sin(f))
                );
                binormal += float3(
                    -d.x * d.y * (steepness * sin(f)),
                    d.y * (steepness * cos(f)),
                    -d.y * d.y * (steepness * sin(f))
                );
                return float3(
                    d.x * (a * cos(f)),
                    a * sin(f),
                    d.y * (a * cos(f))
                );
            }

            Varyings UnlitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 gridPoint = TransformObjectToWorld(input.positionOS.xyz);
                float3 tangentWS = float3(1, 0, 0);
                float3 binormalWS = float3(0, 0, 1);
                float3 positionWS = gridPoint;
                positionWS += GerstnerWave(_WaveA, gridPoint, tangentWS, binormalWS);
                positionWS += GerstnerWave(_WaveB, gridPoint, tangentWS, binormalWS);
                positionWS += GerstnerWave(_WaveC, gridPoint, tangentWS, binormalWS);
                float3 normalWS = normalize(cross(binormalWS, tangentWS));
                output.normalWS = normalWS;
                output.tangentWS = tangentWS;
                output.bitangentWS = binormalWS;

                output.positionCS = TransformWorldToHClip(positionWS);
                output.uv = input.uv;
                output.positionWS = positionWS;

                return output;
            }

            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float3 NModel = normalize(input.normalWS);
                float3 T = normalize(input.tangentWS);
                float3 B = normalize(input.bitangentWS);
                float3 positionWS = input.positionWS;
                float3 V = GetWorldSpaceNormalizeViewDir(positionWS);

                // normal
                float2 uvNormalMap1 = positionWS.xz * 0.01 * _WaveStrength1.xy + _Time.x * _WaveStrength1.zw;
                float2 uvNormalMap2 = positionWS.xz * 0.01 * _WaveStrength2.xy + _Time.x * _WaveStrength2.zw;
                half4 normalMap1 = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uvNormalMap1);
                half4 normalMap2 = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uvNormalMap2);
                float3 normalTS1 = UnpackNormalScale(normalMap1, _NormalScale);
                float3 normalTS2 = UnpackNormalScale(normalMap2, _NormalScale);
                float3 finalNormalTS = lerp(normalTS1, normalTS2, 0.5);
                float3x3 TBN = float3x3(T, B, NModel);
                float3 N = TransformTangentToWorld(finalNormalTS, TBN, true);

                // 水的深度
                float2 screenUV = GetNormalizedScreenSpaceUV(input.positionCS);
                float rawDepth = SampleSceneDepth(screenUV);
                /*
                #if UNITY_REVERSED_Z
                real rawDepth = SampleSceneDepth(screenUV);
                #else
                // Adjust z to match NDC for OpenGL
                real rawDepth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(screenUV));
                #endif
                */
                float3 positionUnderwater = ComputeWorldSpacePosition(screenUV, rawDepth, UNITY_MATRIX_I_VP);
                float waterDepth = positionWS.y - positionUnderwater.y;

                // 水深处和浅处的颜色
                float shallowWeight = saturate(exp(-waterDepth / _DeepRange));
                half4 waterColor = lerp(_DeepColor, _ShallowColor, shallowWeight);
                float fresnel = saturate(_FresnelScale * pow(1 - max(0, dot(NModel, V)), _FresnelPower));
                waterColor = lerp(waterColor, _FresnelColor, fresnel);
                half waterOpacity = waterColor.a;

                // 反射
                float2 uvRef = screenUV + N.xz * rcp(input.positionCS.w + 1) * _ReflectionNoise;
                half4 reflectionColor = SAMPLE_TEXTURE2D(_ReflectionTex, sampler_ReflectionTex, uvRef);
                reflectionColor *= fresnel;

                // 水底
                float2 underWaterNoise = _UnderwaterNoise * N.xz * 0.01;
                float2 uvUnderwater = screenUV + underWaterNoise;
                float3 sceneColor = SampleSceneColor(uvUnderwater);

                // 焦散
                float2 uvCausticsTilling = positionUnderwater.xz * _CausticsMap_ST.xy * 0.1;
                float2 uvCausticsMoving = _Time.x * _CausticsMap_ST.zw;
                float2 uvCaustics1 = float2(uvCausticsTilling + uvCausticsMoving + underWaterNoise);
                half4 causticsMap1 = SAMPLE_TEXTURE2D(_CausticsMap, sampler_CausticsMap, uvCaustics1);
                float2 uvCaustics2 = float2(uvCausticsTilling + uvCausticsMoving * float2(-1, -0.7) + underWaterNoise);
                half4 causticsMap2 = SAMPLE_TEXTURE2D(_CausticsMap, sampler_CausticsMap, uvCaustics2);
                float causticsWeight = saturate(exp(-waterDepth / _CausticsRange));
                half3 causticsColor = min(causticsMap1.rgb, causticsMap2.rgb) * _CausticsIntensity * causticsWeight;

                float3 underwaterColor = sceneColor + causticsColor;

                // 岸边缘颜色
                float waterShore = saturate(exp(-waterDepth / _ShoreRange));
                half3 shoreColor = _ShoreColor.rgb * sceneColor;
                float shoreEdge = smoothstep(_ShoreEdgeWidth, 1, waterShore) * _ShoreEdgeIntensity;

                // 泡沫
                float foam = saturate(waterDepth / _FoamRange);
                float foamMask = 1 - smoothstep(_FoamBlend, 1, foam + 0.01);
                foam = 1 - foam;
                float foamAlpha = foam;
                foam *= _FoamFrequency;
                foam += _Time.y * _FoamSpeed;
                foam = sin(foam);

                half4 foamNoiseMap = SAMPLE_TEXTURE2D(_FoamNoiseMap, sampler_FoamNoiseMap, positionWS.xz * _FoamNoiseMap_ST.xy * 0.01);
                foam = step(foamAlpha * _FoamWidth, foam + foamNoiseMap.x - _FoamDissolve);

                foam *= foamMask;

                // final color
                half4 color;
                color.rgb = lerp(underwaterColor, waterColor.rgb + reflectionColor.rgb, waterOpacity);
                color.rgb = lerp(color.rgb, shoreColor, waterShore);
                color.rgb += shoreEdge.xxx;
                color.rgb += foam * _FoamColor;
                color.a = 1;
                return color;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}