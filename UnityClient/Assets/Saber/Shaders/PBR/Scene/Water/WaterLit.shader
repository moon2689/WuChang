Shader "Saber/Scene/Water Lit"
{
    Properties
    {
        [Header(Base)]
        [MainColor] _ShallowColor("Shallow Color", Color) = (0.6,1,0.8,1)
        _DepthColor("Depth Color", Color) = (0,0.26,0.4,1)
        _DepthStart("Depth Start", float) = 0
        _DepthEnd("Depth End", float) = 20
        _Distortion("Distortion", float) = 30
        _EdgeSize("Edge Size", float) = 0.2
        
        [Header(Normal)]
        _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpMapFlow2("Bump map flow 2", Vector) = (1,1,0,0)
        _BumpScale("Normal Scale", Range(0, 2)) = 1
        _ReflectionDistortion("Reflection Distortion", Range(0,1)) = 0.25

        _Smoothness("Smoothness", Range(0,1)) = 0.7
        
        [Header(Scattering)]
        [Toggle] _Scattering ("Scattering On?", Float) = 0
        _ScatteringColor("Scattering Color", Color) = (0.28,1,0.78,1)
        _ScatteringNormalScale("Scattering Normal Scale", Range(0, 1)) = 0.2
        _ScatteringPower("Scattering Power", float) = 1
        _ScatteringIntensity("Scattering Intensity", float) = 1
        
        [Header(Wave)]
        [Toggle] _Wave ("Wave On?", Float) = 0
        _WaveRT("Wave Render Texture", 2D) = "black" {}
        _WaveRTRect("Wave Render Texture Rect", Vector) = (0,0,1,1)
        
        [Header(Caustics)]
        [Toggle] _Caustics ("Caustics On?", Float) = 0
        _CausticsMap("Caustics Map", 2D) = "black" {}
        _CausticsMapFlow2("Caustics map flow 2", Vector) = (1,1,0,0)
        _CausticsFadeDepth("Caustics Fade Depth", float) = 4
        _CausticsIntensity("Caustics Intensity", float) = 1
    }

    SubShader
    {
        // Universal Pipeline tag is required. If Universal render pipeline is not set in the graphics settings
        // this Subshader will fail. One can add a subshader below or fallback to Standard built-in to make this
        // material work with both Universal Render Pipeline and Builtin Unity Pipeline
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit"
            "IgnoreProjector" = "True"
			"Queue" = "Transparent"
        }
        LOD 300

        // ------------------------------------------------------------------
        //  Forward pass. Shades all light in a single pass. GI + emission + Fog
        Pass
        {
            // Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
            // no LightMode tag are also rendered by Universal Render Pipeline
            Name "ForwardLit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            // -------------------------------------
            // Render State Commands
            Blend SrcAlpha OneMinusSrcAlpha
            //ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            // -------------------------------------
            // Material Keywords
            #pragma multi_compile_fragment _ _SCATTERING_ON
            #pragma multi_compile_fragment _ _WAVE_ON
            #pragma multi_compile_fragment _ _CAUSTICS_ON

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _LIGHT_LAYERS
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _FORWARD_PLUS
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"


            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            //#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
            #pragma multi_compile_fog
            #pragma multi_compile_fragment _ DEBUG_DISPLAY

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            #include "WaterLitInput.hlsl"
            #include "WaterLitForwardPass.hlsl"
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    //CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.LitShader"
}
