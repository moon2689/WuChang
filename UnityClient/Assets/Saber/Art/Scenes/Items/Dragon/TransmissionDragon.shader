Shader "Saber/Unlit/Transmission Dragon"
{
    Properties
    {
        _DiffuseColor("Diffuse Color",Color) = (0,0.352,0.219,1)
		_AddColor("Add Color",Color) = (0,0.352,0.219,1)
		_Opacity("Opacity",Range(0,1)) = 0
		_ThicknessMap("Thickness Map",2D) = "black"{}
		
		[Header(BasePass)]
		_BasePassDistortion("Bass Pass Distortion", Range(0,1)) = 0.2
		_BasePassColor("BasePass Color",Color) = (1,1,1,1)
		_BasePassPower("BasePass Power",float) = 1
		_BasePassScale("BasePass Scale",float) = 2
		
		[Header(AddPass)]
		_AddPassDistortion("Add Pass Distortion", Range(0,1)) = 0.2
		_AddPassColor("AddPass Color",Color) = (0.56,0.647,0.509,1)
		_AddPassPower("AddPass Power",float) = 1
		_AddPassScale("AddPass Scale",float) = 1

		[Header(EnvReflect)]
		_EnvRotate("Env Rotate",Range(0,360)) = 0
		_EnvMap ("Env Map", Cube) = "white" {}
		_FresnelMin("Fresnel Min",Range(-2,2)) = 0
		_FresnelMax("Fresnel Max",Range(-2,2)) = 1
		_EnvIntensity("Env Intensity",float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM

            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS

            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_ThicknessMap); SAMPLER(sampler_ThicknessMap);
		    float4 _DiffuseColor;
		    float4 _AddColor;
		    float _Opacity;

		    float4 _BasePassColor;
		    float _BasePassDistortion;
		    float _BasePassPower;
		    float _BasePassScale;

            float _AddPassDistortion;
			float _AddPassPower;
			float _AddPassScale;
			float4 _AddPassColor;

 		    samplerCUBE _EnvMap;
		    float4 _EnvMap_HDR;
		    float _EnvRotate;
		    float _EnvIntensity;
		    float _FresnelMin;
		    float _FresnelMax;

            struct appdata
            {
                float4 vertex : POSITION;
			    float2 texcoord : TEXCOORD0;
			    float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
			    float2 uv : TEXCOORD0;
			    float4 posWorld : TEXCOORD1;
			    float3 normalDir : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;
			    o.posWorld = mul(unity_ObjectToWorld, v.vertex);
			    o.normalDir = mul(float4(v.normal, 0), unity_WorldToObject).xyz;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
			    o.uv = v.texcoord;
			    o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			    return o;
            }

            half4 frag(v2f i) : SV_TARGET
            {
                //info
			    float3 diffuse_color = _DiffuseColor.rgb;
			    float3 normalDir = normalize(i.normalDir);
			    float3 viewDir = GetWorldSpaceNormalizeViewDir(i.posWorld.xyz);
                Light mainLight = GetMainLight();

			    // diffuse
			    float diff_term = max(0.0, dot(normalDir, mainLight.direction));
			    float3 diffuselight_color = diff_term * diffuse_color * mainLight.color;

			    float sky_sphere = dot(normalDir,float3(0,1,0)) * 0.5 + 0.5;
			    float3 sky_light = sky_sphere * diffuse_color;
			    float3 final_diffuse = diffuselight_color + sky_light * _Opacity + _AddColor.xyz;

			    //trans light
			    float3 back_dir = -normalize(mainLight.direction + normalDir * _BasePassDistortion);
			    float VdotB = max(0.0, dot(viewDir, back_dir));
			    float backlight_term = max(0.0,pow(VdotB, _BasePassPower)) * _BasePassScale;
			    float thickness = 1.0 - SAMPLE_TEXTURE2D(_ThicknessMap, sampler_ThicknessMap, i.uv).r;
			    float3 backlight = backlight_term * thickness * mainLight.color * _BasePassColor.xyz;

                float3 addLightBackLight = float3(0, 0, 0);
                #if defined(_ADDITIONAL_LIGHTS)
                uint addLightCount = GetAdditionalLightsCount();
                for(uint lightIndex = 0;lightIndex < addLightCount;++lightIndex)
                {
                    Light addLight = GetAdditionalLight(lightIndex, i.posWorld.xyz);
                    float3 addLightBackDir = -normalize(addLight.direction + normalDir * _AddPassDistortion);
				    float addLightVdotB = max(0.0,dot(viewDir, addLightBackDir));
				    float addLightBacklightTerm = max(0.0, pow(addLightVdotB, _AddPassPower)) * _AddPassScale;
				    addLightBackLight += addLightBacklightTerm * thickness * addLight.color * _AddPassColor.xyz;
                }
                #endif

			    //ENV
			    float3 reflectDir = reflect(-viewDir,normalDir);

			    half theta = _EnvRotate * PI / 180.0f;
			    float2x2 m_rot = float2x2(cos(theta), -sin(theta), sin(theta),cos(theta));
			    float2 v_rot = mul(m_rot, reflectDir.xz);
			    reflectDir = half3(v_rot.x, reflectDir.y, v_rot.y);

			    float4 cubemap_color = texCUBE(_EnvMap,reflectDir);
			    half3 env_color = DecodeHDREnvironment(cubemap_color, _EnvMap_HDR);

			    float fresnel = 1.0 - saturate(dot(normalDir, viewDir));
			    fresnel = smoothstep(_FresnelMin, _FresnelMax, fresnel);

			    float3 final_env = env_color * _EnvIntensity * fresnel;
			    //combine
			    float3 combined_color = final_diffuse + final_env + backlight + addLightBackLight;
			    float3 final_color = combined_color;
			    return float4(final_color,1.0);
            }

            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
