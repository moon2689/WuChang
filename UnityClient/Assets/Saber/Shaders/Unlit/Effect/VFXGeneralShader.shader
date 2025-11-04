Shader "Unlit/VFXGeneralShader"
{
    Properties
    {
        //属性设置区域
        [HideInInspector]_BlendTemp("BlendTemp",int) =0
        [Header(setting)]
        [Enum(Twoside,0,Back,1,Front,2)] _CULLMODE("CULLMODE",float) = 2
        [Enum(ON,1,OFF,0)] _ZWrite("ZWrite",float) = 1
        [Enum(UnityEngine.Rendering.CullMode)]_CullMode("CullMode",Int) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("ZTest",Int) = 4
        [Enum(UnityEngine.Rendering.BlendMode)]_BlendModeSrc("BlendModeSrc",Int) = 5
        [Enum(UnityEngine.Rendering.BlendMode)]_BlendModeDst("BlendModeDst",Int) = 10
        [Toggle(_CLIP_ON)]_Clip_On("Clip_On",Int) = 0
        _clipRange("半透明剪裁",range(0,1)) = 0.5
        [Toggle(_PANNER_ON)]_Panner_ON("uv流动",int) = 0
        //[Enum(OFF,0, Panner,1, Disslove,2)]_CustomDataSwitch("CustomData模式",int)    =0
        
        [Enum(Off,0,Panner,1,Dissolve,2)]_ParticleMode("ParticleMode",Int) = 0
        _ParticleModeTemp01("mode01" , Int) =0
        _ParticleModeTemp02("mode02" , Int) =0

        [Space(20)]
        //BlendMode
        //主贴图
        [Enum(Alpha,0 , a,1)]_MainTexAlpha("透明通道选择",int) = 0
        _MainTex("MainTex",2D) = "white"{}
        [HDR]_MainTexColor("Tint",Color) = (1,1,1,1)
        _Brightness("主贴图强度",float) = 1
        _Pivot_MainTex("旋转中心",vector) = (0.5,0.5,0,0)
        _MainTexRotationAngle("旋转角度", range(0,360)) = 0
        _Uspeed_MainTex("Uspeed",float) = 0
        _Vspeed_MainTex("Vspeed",float) = 0
        
        [Space(10)]
        //mask贴图
        [Toggle(_MAINTEX_MASK_ON)]_MAINTEX_MASK_ON("遮罩",int)= 0
        [Enum(Alpha,0 ,a,1)]_MaskTexAlpha("透明通道选择",int) = 0
        _MaskTex("遮罩贴图",2D) = "white"{}
        _Uspeed_MaskTex("Uspeed",float) = 0
        _Vspeed_MaskTex("Vspeed",float) = 0
        _MaskTexRotationAngle("旋转角度",range(0,360)) = 0
        
        [Space(10)]
        //扭曲贴图
        [Toggle(_DISTORT_ON)]_DISTORT_ON("扰动开关",int) =0
        _DistortTex("扭曲贴图",2D) ="white"{}
        _DistortTexIntensity("扭曲强度",float) =0
        _Uspeed_DistortTex("Uspeed",float) = 0
        _Vspeed_DistortTex("Vspeed",float) = 0
        
        //溶解
        [Enum(Off,0,Dissolve,1,DissolvePlus,2)]_DissolveMode("DissolveMode",int) = 0
        [Toggle(ReverseDissolve)]_ReverseDissolve("ReverseDissolve",Int) = 0
        _DissolveTex("DissolveTex",2D) = "white"{}
        _DissolveFactor("DissolveFactor",Range(0,1)) = 0.5
        _HardnessFactor("HardnessFactor",Range(0,1)) = 0.9
        _DissolveWidth("DissolveWidth",Range(0,1)) = 0.1
        [HDR]_WidthColor("WidthColor",Color) = (1,1,1,1)
        _Uspeed_DissolveTex("DissolveTex_PannerSpeedU",float) = 0
        _Vspeed_DissolveTex("DissolveTex_PannerSpeedV",float) = 0

        _Stencil ("Stencil ID", Float) = 0
        _StencilComp ("Stencil Comparison", Float) = 8
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

    }
    SubShader
    {
        Tags
        {
            "IgnoreProject" = "True"
            "Queue" = "Transparent+100"
            "RenderType" = "Transparent"
        }

        Pass
        {
            Lighting Off
            Blend [_BlendModeSrc] [_BlendModeDst]
            Cull [_CullMode]
            Zwrite [_ZWrite]
            ZTest [_ZTest]

            Stencil
            {
                Ref [_Stencil]
                Comp [_StencilComp]
                Pass [_StencilOp]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
            }

            ColorMask [_ColorMask]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            //宏
            //#pragma shader_feature      _DIFFUSEROTATION_ON
            #pragma shader_feature      _PANNER_ON
            #pragma shader_feature      _MAINTEX_MASK_ON
            #pragma shader_feature      _DISTORT_ON
            #pragma shader_feature_local      _DISSOLVE
            #pragma shader_feature_local      _DISSOLVEPLUS
            #pragma shader_feature      _CLIP_ON


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct VertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                half4 vertexColor : COLOR;
                float4 normal : NORMAL;
                half4 customData1 : TEXCOORD1;
                half4 customData2 : TEXCOORD2;
            };

            struct VertexOutput
            {
                float4 vertex : SV_POSITION;
                half4 vertexColor : COLOR;
                float4 uv1 : TEXCOORD0;
                float4 customData1 : TEXCOORD1;
                float4 customData2 : TEXCOORD2;
                float4 uv2 : TEXCOORD3;
                float4 uv3 : TEXCOORD4;
                float4 worldPos : TEXCOORD5;
                float4 normal : TEXCOORD6;
            };

            int _CustomDataSwitch;
            int _ParticleMode;
            int _ParticleModeTemp01, _ParticleModeTemp02;
            int _DissolveMode;
            int _CullMode;
            int _Zwrite;
            int _Clip_On;
            float _clipRange;


            TEXTURE2D(_MainTex);  SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            half _MainTexRotationAngle;
            half _Brightness;
            half _Uspeed_MainTex;
            half _Vspeed_MainTex;
            float4 _Pivot_MainTex;
            half4 _MainTexColor;
            half _MainTexAlpha;

            TEXTURE2D(_MaskTex);  SAMPLER(sampler_MaskTex);
            float4 _MaskTex_ST;
            half _MaskTexAlpha;
            half _Uspeed_MaskTex;
            half _Vspeed_MaskTex;
            half _MaskTexRotationAngle;

            TEXTURE2D(_DistortTex);  SAMPLER(sampler_DistortTex);
            float4 _DistortTex_ST;
            float _DistortTexIntensity;
            half _Uspeed_DistortTex;
            half _Vspeed_DistortTex;

            TEXTURE2D(_DissolveTex);  SAMPLER(sampler_DissolveTex);
            float4 _DissolveTex_ST;
            half _DissolveFactor;
            half _HardnessFactor;
            half _DissolveWidth;
            float4 _WidthColor;
            half _ReverseDissolve;
            half _Uspeed_DissolveTex;
            half _Vspeed_DissolveTex;

            inline float Smoothstep_Simple(half c,half minValue, half maxValue)
            {
                c = (c - minValue) / (maxValue - minValue);
                c = saturate(c);
                return c;
            }

            //溶解函数
            inline float4 DissolveFunction(half4 c, half dissloveTex, half disslove,half hardness)
            {
                hardness = clamp(hardness, 0.00001, 0.99999);
                dissloveTex += disslove * (2 - hardness);
                dissloveTex = 2 - dissloveTex;
                dissloveTex = Smoothstep_Simple(dissloveTex, hardness, 1);
                c.a *= dissloveTex;
                return c;
            }

            //光边溶解函数
            inline float4 DoubleDissolveFunction(half4 c,half dissolveTex,half dissolve,half hardness,half width,half4 WidthColor)
            {
                hardness = clamp(hardness, 0.00001, 999999);
                dissolve *= (1 + width);
                half hardnessFactor = 2 - hardness;
                half dissolve01 = dissolve * hardnessFactor + dissolveTex;
                dissolve01 = Smoothstep_Simple((2 - dissolve01), hardness, 1);
                half dissolve02 = (dissolve - width) * hardnessFactor + dissolveTex;
                dissolve02 = Smoothstep_Simple((2 - dissolve02), hardness, 1);
                c.rgb = lerp(WidthColor, c.rgb, dissolve01);
                c.a *= dissolve02;
                return c;
            }


            //顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色顶点着色            
            VertexOutput vert(VertexInput v)
            {
                //VertexOutput o = (VertexOutput)0;  //初始化
                VertexOutput o = (VertexOutput)0;

                o.customData1 = v.customData1;
                o.customData2 = v.customData2;

                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.normal.xyz = TransformObjectToWorldNormal(v.normal.xyz);

                o.vertexColor = v.vertexColor; //顶点色

                o.uv1.xy = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv1.zw = TRANSFORM_TEX(v.uv, _MaskTex);
                o.uv2.xy = TRANSFORM_TEX(v.uv, _DistortTex);
                o.uv2.zw = TRANSFORM_TEX(v.uv, _DissolveTex);
                //是否开启Panner
                #ifdef _PANNER_ON
                o.uv1.xy += _Time.y * float2(_Uspeed_MainTex, _Vspeed_MainTex);
                o.uv1.zw += _Time.y * float2(_Uspeed_MaskTex, _Vspeed_MaskTex);
                o.uv2.xy += _Time.y * float2(_Uspeed_DistortTex, _Vspeed_DistortTex);
                o.uv2.zw += _Time.y * float2(_Uspeed_DissolveTex, _Vspeed_DissolveTex);
                #endif

                //customData控制  CustomData1 xy=maintexuvpanner  zw = maskuvpanner  CustomData2 xy= 
                o.uv1 += v.customData1 * _ParticleModeTemp01;
                o.uv2 += v.customData2 * _ParticleModeTemp01;
                o.uv1.xy += v.customData1.zw * _ParticleModeTemp02;
                return o;
            }

            //片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元片元
            half4 frag(VertexOutput i) : SV_Target
            {
                float4 finalColor;
                //扭曲贴图
                #ifdef _DISTORT_ON
                half4 distortTex = SAMPLE_TEXTURE2D(_DistortTex, sampler_DistortTex, i.uv2.xy);
                distortTex = (distortTex * 2 - 1) * lerp(_DistortTexIntensity, i.customData1.y, _ParticleModeTemp02);
                i.uv1.xy += distortTex;
                i.uv1.zw += distortTex;
                i.uv2.xy += distortTex;
                i.uv2.zw += distortTex;
                #endif

                //旋转主贴图UV
                //#ifdef _DIFFUSEROTATION_ON
                float2 uv_Main = i.uv1.xy;
                half mainTexAngle_cos;
                half mainTexAngle_sin;
                //sincos(float x, out float s, out float c)  *0.0174角度转弧度
                sincos(_MainTexRotationAngle * 0.0174, mainTexAngle_sin, mainTexAngle_cos);
                float2 rotatedUv = mul(uv_Main - _Pivot_MainTex.xy, float2x2(mainTexAngle_cos, -mainTexAngle_sin, mainTexAngle_sin, mainTexAngle_cos)); //旋转矩阵
                float2 mainTexPanner = rotatedUv + _Pivot_MainTex.xy;
                float4 mainTexFinal = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainTexPanner);
                mainTexFinal.a = lerp(mainTexFinal.a, mainTexFinal.r, _MainTexAlpha);
                finalColor = mainTexFinal * i.vertexColor * _Brightness * _MainTexColor;
                //#endif

                //溶解
                #ifdef  _DISSOLVE
                float2 uv_DissolveTex = i.uv2.zw;
                float dissolveTex = SAMPLE_TEXTURE2D(_DissolveTex, sampler_DissolveTex, uv_DissolveTex);
                dissolveTex = lerp(dissolveTex, 1 - dissolveTex, _ReverseDissolve);
                _DissolveFactor = lerp(_DissolveFactor, i.customData1.x, _ParticleModeTemp02);
                finalColor = DissolveFunction(finalColor, dissolveTex, _DissolveFactor, _HardnessFactor);
                #endif

                //光边溶解
                #ifdef  _DISSOLVEPLUS
                float2 uv_DissolveTexPlus = i.uv2.zw;
                float dissolveTexPlus = SAMPLE_TEXTURE2D(_DissolveTex, sampler_DissolveTex, uv_DissolveTexPlus);
                dissolveTexPlus = lerp(dissolveTexPlus, 1 - dissolveTexPlus, _ReverseDissolve);
                _WidthColor = lerp(_WidthColor, i.customData2, _ParticleModeTemp02);
                _DissolveFactor = lerp(_DissolveFactor, i.customData1.x, _ParticleModeTemp02);
                finalColor = DoubleDissolveFunction(finalColor, dissolveTexPlus, _DissolveFactor, _HardnessFactor, _DissolveWidth, _WidthColor);
                #endif

                #ifdef  _CLIP_ON
                clip(finalColor.a - _clipRange);
                //clip(finalColor.a - 0.5);
                #endif


                //主帖图mask同步旋转平移
                #ifdef _MAINTEX_MASK_ON
                float2 uv_MaskTex = i.uv1.zw;
                half maskTexAngle_cos;
                half maskTexAngle_sin;
                sincos(_MaskTexRotationAngle * 0.0174, maskTexAngle_sin, maskTexAngle_cos);
                float2 rotatedMaskUV = mul(uv_MaskTex - _Pivot_MainTex.xy, float2x2(maskTexAngle_cos, -maskTexAngle_sin, maskTexAngle_sin, maskTexAngle_cos));
                float2 maskTexPanner = rotatedMaskUV + _Pivot_MainTex.xy;
                half4 maskTex = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, maskTexPanner);
                half maskChannel = lerp(maskTex.a, maskTex.r, _MaskTexAlpha);
                finalColor.a *= maskChannel;

                #endif

                //输出
                return finalColor;
            }
            ENDHLSL
        }
    }
    CustomEditor "VFXGeneralShaderGUI"
}