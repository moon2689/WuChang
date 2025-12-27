Shader "Hidden/Planar Reflections/Internal/VRCombine"
{
    Properties
    {
        _MainTex("Left Eye Texture", 2D) = "white"{}
        _RightEyeTex("Right Eye Texture", 2D) = "white"{}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert 
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _RightEyeTex;
            float4 _RightEyeTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 leftEyeUV = i.uv;
                leftEyeUV.x *= 2;
                float2 rightEyeUV = leftEyeUV;
                rightEyeUV.x -= 1;

                float eyeIndex = 0;

                if ( i.uv.x >= 0.5 ){
                    eyeIndex = 1;
                }

                float4 col = lerp( tex2D(_MainTex, leftEyeUV ), tex2D(_RightEyeTex, rightEyeUV ), eyeIndex );
                return col;
            }
            ENDCG
        }

        Pass
        {
        Cull Off ZWrite On ZTest Always


            CGPROGRAM
            #pragma vertex vert 
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST; 
            sampler2D _RightEyeTex;
            float4 _RightEyeTex_ST;

             SamplerState sampler_MainTex;  
            SamplerState sampler_RightEyeTex; 

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float frag (v2f i ) : SV_Depth
            {
                float2 leftEyeUV = i.uv;
                leftEyeUV.x *= 2;
                float2 rightEyeUV = leftEyeUV;
                rightEyeUV.x -= 1;

                float eyeIndex = 0;

                if ( i.uv.x >= 0.5 ){
                    eyeIndex = 1;
                }

                float leftDepth =  SAMPLE_DEPTH_TEXTURE( _MainTex, leftEyeUV);
                float rightDepth =  SAMPLE_DEPTH_TEXTURE( _RightEyeTex, rightEyeUV);

                return lerp( leftDepth, rightDepth, eyeIndex );
            }
            ENDCG
        }

        
        Pass
        {
        Cull Off ZWrite On ZTest Always


            CGPROGRAM
            #pragma vertex vert 
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST; 
            sampler2D _RightEyeTex;
            float4 _RightEyeTex_ST;

             SamplerState sampler_MainTex;  
            SamplerState sampler_RightEyeTex; 

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float frag (v2f i ) : SV_Depth
            {
                
                float leftDepth =  SAMPLE_DEPTH_TEXTURE( _RightEyeTex, i.uv);

                //outDepth = leftDepth;
                return leftDepth;
            }
            ENDCG
        }

    }
}
