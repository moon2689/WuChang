/*
* PIDI - Planar Reflections™ 6 - Copyright© 2017-2025
* PIDI - Planar Reflections is a trademark and copyrighted property of Jorge Pinal Negrete.

* You cannot sell, redistribute, share nor make public this code, modified or not, in part nor in whole, through any
* means on any platform except with the purposes of contacting the developers to request support and only when taking
* all pertinent measures to avoid its release to the public and / or any unrelated third parties.
* Modifications are allowed only for internal use within the limits of your Unity based projects and cannot be shared,
* published, redistributed nor made available to any unlicensed third parties.
*
* For more information, contact us at support@irreverent-software.com
*
*/
namespace PlanarReflections6 {

    using UnityEngine;


    [System.Serializable]
    public class ReflectionData {

        protected Camera _reflectionCam;

        public RenderTexture reflectionTex;

        public RenderTextureDescriptor rd;

        public RenderTexture reflectionDepth;

        public RenderTexture reflectionFog;

        public Vector2Int screenRes;


        public void ForceSetCamera( Camera cam ) {
            _reflectionCam = cam;
        }

        public ReflectionData( Camera cam, PlanarReflectionSettings settings ) {
            _reflectionCam = cam;
            reflectionTex = RenderTexture.GetTemporary( 1, 1 );
            reflectionDepth = RenderTexture.GetTemporary( 1, 1 );
            reflectionFog = RenderTexture.GetTemporary( 1, 1 );
            screenRes = new Vector2Int( Screen.width, Screen.height );
            RegenerateTextures( settings );
        }

        public void RegenerateTextures( PlanarReflectionSettings settings ) {

            RenderTexture.ReleaseTemporary( reflectionTex );
            RenderTexture.ReleaseTemporary( reflectionDepth );
            RenderTexture.ReleaseTemporary( reflectionFog );

            rd = new RenderTextureDescriptor( Mathf.RoundToInt( settings.outputResolutionMultiplier * ( settings.screenBasedResolution ? Screen.width : settings.explicitResolution.x ) ), Mathf.RoundToInt( settings.outputResolutionMultiplier * ( settings.screenBasedResolution ? Screen.height : settings.explicitResolution.y ) ) );
            rd.useMipMap = settings.useMipMaps;
            rd.msaaSamples = 1;
            rd.colorFormat = settings.forceHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
            rd.depthBufferBits = 16;
            rd.autoGenerateMips = true;
            rd.volumeDepth = 1;
            rd.vrUsage = VRTextureUsage.None;
            rd.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
            rd.mipCount = 6;



            screenRes = new Vector2Int( Screen.width, Screen.height );

            if ( rd.width < 1 ) {
                rd.width = 512;
                screenRes.x = 512;
            }

            if ( rd.height < 1 ) {
                rd.height = 512;
                screenRes.y = 512;
            }

            reflectionTex = RenderTexture.GetTemporary( rd );
            reflectionTex.filterMode = FilterMode.Bilinear;
            reflectionTex.wrapMode = TextureWrapMode.Repeat;

            if ( settings.renderDepth ) {
                rd.colorFormat = RenderTextureFormat.Depth;
                reflectionDepth = RenderTexture.GetTemporary( rd );
                reflectionDepth.filterMode = FilterMode.Bilinear;
                reflectionDepth.name = "_REFDEPTHP6";
            }
            else {
                RenderTexture.ReleaseTemporary( reflectionDepth );
            }

            if ( settings.renderFog ) {
                rd.colorFormat = RenderTextureFormat.Default;
                reflectionFog = RenderTexture.GetTemporary( rd );
                reflectionFog.filterMode = FilterMode.Bilinear;
                reflectionFog.name = "_REFFOGP6";
            }
            else {
                RenderTexture.ReleaseTemporary( reflectionFog );
            }

            rd.colorFormat = settings.forceHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
            reflectionTex.name = "_REFTEXP6";

        }


        public void Release() {
            RenderTexture.ReleaseTemporary( reflectionFog );
            RenderTexture.ReleaseTemporary( reflectionTex );
            RenderTexture.ReleaseTemporary( reflectionDepth );
        }


    }

}