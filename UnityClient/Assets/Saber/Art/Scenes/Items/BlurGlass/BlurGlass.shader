Shader "Omnigames/Blur Glass"
{
    Properties
    {
        _BaseColor("Color", Color) = (1, 1, 1, 1)
        _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalScale("Normal Scale", Range(0,2))=1
        _DistortStrength("Distort Strength", Range(0,1)) = 0.1
        _RefractionStrength("Refraction Strength", Range(-1,1)) = -0.1
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
            //Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);


            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _NormalScale;
                float4 _NormalMap_ST;
                float _DistortStrength;
                float _RefractionStrength;
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
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 tangentWS : TEXCOORD2;
                float3 bitangentWS : TEXCOORD3;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings UnlitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.normalWS = normalInputs.normalWS;
                output.tangentWS = normalInputs.tangentWS;
                output.bitangentWS = normalInputs.bitangentWS;

                return output;
            }

            half4 UnlitPassFragment(Varyings input) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float3 NModel = normalize(input.normalWS);
                float3 T = normalize(input.tangentWS);
                float3 B = normalize(input.bitangentWS);
                float3x3 TBN = float3x3(T, B, NModel);
                
                half4 normalMap = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv * _NormalMap_ST.xy);
                float3 normalTS = UnpackNormalScale(normalMap, _NormalScale);
                float3 N = TransformTangentToWorld(normalTS, TBN, true);

                Light mainLight = GetMainLight();
                float3 L = mainLight.direction;

                half3 albedo = _BaseColor.rgb;
                half alpha = _BaseColor.a;

                float NoL = max(0,dot(N, L));
                half3 diffuse = NoL * mainLight.color * albedo;

                // scene color
                float3 normalVS = TransformWorldToViewNormal(N);
                float2 screenUV = GetNormalizedScreenSpaceUV(input.positionCS);
                float2 uvSceneColor = screenUV + normalTS.xy * _DistortStrength + normalVS.xy * _RefractionStrength * 0.1;
                half3 sceneColor = SampleSceneColor(uvSceneColor);
                
                half4 color;
                color.rgb = lerp(sceneColor, diffuse, alpha);
                color.a = alpha;
                return color;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}