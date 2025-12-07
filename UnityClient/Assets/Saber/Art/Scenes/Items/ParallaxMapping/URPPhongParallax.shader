Shader "Saber/Unlit/URP Phong Parallax"
{
    Properties
    {
		_MainTex ("Texture", 2D) = "white" {}
		_NormalMap("NormalMap",2D) = "bump"{}
		_NormalIntensity("Normal Intensity",Range(0.0,5.0)) = 1.0
		_AOMap("AO Map",2D) = "white"{}
		_SpecMask("Spec Mask",2D) = "white"{}
		_Shininess("Shininess",Range(0.01,100)) = 1.0
		_SpecIntensity("SpecIntensity",Range(0.01,5)) = 1.0
		_ParallaxMap("ParallaxMap",2D) = "black"{}		// �Ӳ���ͼ
		_Parallax("_Parallax",float) = 2
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

            struct appdata
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float3 normal  : NORMAL;
				float4 tangent : TANGENT;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normal_dir : TEXCOORD1;
				float3 pos_world : TEXCOORD2;
				float3 tangent_dir : TEXCOORD3;
				float3 binormal_dir : TEXCOORD4;
				float4 shadowCoord : TEXCOORD5;
			};

			TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
			float4 _MainTex_ST;
			float4 _LightColor0;
			float _Shininess;
			float _SpecIntensity;
			TEXTURE2D(_AOMap); SAMPLER(sampler_AOMap);
			TEXTURE2D(_SpecMask); SAMPLER(sampler_SpecMask);
			TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);
			float _NormalIntensity;
			TEXTURE2D(_ParallaxMap); SAMPLER(sampler_ParallaxMap);
			float _Parallax;
			
			float3 ACESFilm(float3 x)
			{
				float a = 2.51f;
				float b = 0.03f;
				float c = 2.43f;
				float d = 0.59f;
				float e = 0.14f;
				return saturate((x*(a*x + b)) / (x*(c*x + d) + e));
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.normal_dir = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
				o.tangent_dir = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0)).xyz);
				o.binormal_dir = normalize(cross(o.normal_dir,o.tangent_dir)) * v.tangent.w;
				o.pos_world = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.shadowCoord = TransformWorldToShadowCoord(o.pos_world);
				return o;
			}
			
			half4 frag (v2f i) : SV_Target
			{
				float3 worldPos = i.pos_world;
				half3 view_dir = GetWorldSpaceNormalizeViewDir(worldPos);
				half3 normal_dir = normalize(i.normal_dir);
				half3 tangent_dir = normalize(i.tangent_dir);
				half3 binormal_dir = normalize(i.binormal_dir);
				float3x3 TBN = float3x3(tangent_dir, binormal_dir, normal_dir);
				half3 view_tangentspace = normalize(mul(TBN, view_dir));		
				half2 uv_parallax = i.uv;

				// �����Ӳ���ͼ��uvƫ�ƣ�ע��Ҫ������ת�������߿ռ��ټ���
				for (int index = 0; index < 10; index++)
				{
					half height = SAMPLE_TEXTURE2D(_ParallaxMap, sampler_ParallaxMap, uv_parallax).r;
					uv_parallax = uv_parallax - (0.5 - height) * view_tangentspace.xy * _Parallax * 0.01f;
				}

				half4 base_color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv_parallax);
				base_color = pow(base_color, 2.2);		// ٤��ռ�->���Կռ�
				half4 ao_color = SAMPLE_TEXTURE2D(_AOMap, sampler_AOMap, uv_parallax);
				half4 spec_mask = SAMPLE_TEXTURE2D(_SpecMask, sampler_SpecMask, uv_parallax);

				// ��ȡ����
				half4 normalmap = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv_parallax);
				half3 normal_data = UnpackNormal(normalmap);
				normal_data.xy = normal_data.xy * _NormalIntensity;
				normal_dir = normalize(mul(normal_data.xyz, TBN));
				//normal_dir = normalize(tangent_dir * normal_data.x * _NormalIntensity + binormal_dir * normal_data.y * _NormalIntensity + normal_dir * normal_data.z);

				// ����Դ
				half3 final_color = half3(0, 0, 0);
				{
					// ������
					Light mainLight = GetMainLight(i.shadowCoord);
					half3 light_dir = mainLight.direction;
					half NdotL = max(0.0,dot(normal_dir, light_dir));
					half3 diffuse_color = NdotL *  mainLight.color * base_color.xyz * mainLight.shadowAttenuation;
				
					// ���淴��
					half3 half_dir = normalize(light_dir + view_dir);
					half NdotH = max(0,dot(normal_dir, half_dir));
					half3 spec_color = pow(NdotH,_Shininess) * mainLight.color * _SpecIntensity * spec_mask.rgb * mainLight.shadowAttenuation;

					final_color = diffuse_color + spec_color;
				}

				// ������Դ
				uint addLightCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0; lightIndex < addLightCount; lightIndex++)
                {
					Light addLight = GetAdditionalLight(lightIndex, worldPos, i.shadowCoord);
					half NdotL = saturate(dot(normal_dir, addLight.direction));
					half3 addLightDiffuse = NdotL * addLight.color * base_color.rgb * addLight.distanceAttenuation * addLight.shadowAttenuation;

					half3 halfDir = normalize(addLight.direction + view_dir);
					half NdotH = saturate(dot(normal_dir, halfDir));
					half3 addLightSpec = pow(NdotH, _Shininess) * addLight.color * _SpecIntensity * spec_mask.rgb * addLight.distanceAttenuation * addLight.shadowAttenuation;

					final_color += (addLightDiffuse + addLightSpec);
                }

				// ������
				half3 shColor = SampleSH(normal_dir);		// ��г����
				half3 ambient_color = shColor * base_color.xyz;

				final_color += ambient_color;
				final_color *= ao_color.rgb;
				half3 tone_color = ACESFilm(final_color);	// ɫ��ӳ��
				tone_color = pow(tone_color, 1.0 / 2.2);	// ���Կռ�->٤��ռ�

				return half4(tone_color, 1.0);
			}

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            // -------------------------------------
            // Universal Pipeline keywords

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
