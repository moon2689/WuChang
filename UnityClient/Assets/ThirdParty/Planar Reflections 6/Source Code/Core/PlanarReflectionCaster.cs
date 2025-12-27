namespace PlanarReflections6 {

    /*
    * PIDI - Planar Reflections™ 6 - Copyright© 2017-2025
    * PIDI - Planar Reflections is a trademark and copyrighted property of Jorge Pinal Negrete.

    * You cannot sell, redistribute, share nor make public this code, modified or not, in part nor in whole, through any
    * means on any platform except with the purpose of contacting the developers to request support and only when taking
    * all pertinent measures to avoid its release to the public and / or any unrelated third parties.
    * Modifications are allowed only for internal use within the limits of your Unity based projects and cannot be shared,
    * published, redistributed nor made available to any third parties unrelated to Irreverent Software by any means.
    *
    * For more information, contact us at support@irreverent-software.com
    *
    */

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [RequireComponent( typeof( Renderer ) )]
    [ExecuteAlways]
    [DefaultExecutionOrder(99)]
    [HelpURL( "https://irreverent-software.com/docs/planar-reflections-6/getting-started/planar-reflection-caster/" )]
    public class PlanarReflectionCaster : MonoBehaviour {

        public static readonly int _reflectionTex = Shader.PropertyToID( "_ReflectionTex" );
        public static readonly int _reflectionDepth = Shader.PropertyToID( "_ReflectionDepth" );
        public static readonly int _reflectionFog = Shader.PropertyToID( "_ReflectionFog" );
        public static readonly int _blurReflectionTex = Shader.PropertyToID( "_BlurReflectionTex" );
        public static readonly int _vrMode = Shader.PropertyToID( "_VRMode" );


        [System.Serializable]
        public struct BlurSettings {

            [System.NonSerialized] public RenderTexture blurredMap;
            [System.NonSerialized] public RenderTexture blurredDepth;
            public bool useBlur;
            public bool forceFakeBlur;
            public int blurPassMode;
            [Range( 0, 1 )] public float blurRadius;
            [Range( 1, 4 )] public int blurDownscale;

#if UPDATE_PLANAR3

            public static implicit operator BlurSettings( PlanarReflections3.PlanarReflectionsCaster.BlurSettings source ) {

                var newBlur = new BlurSettings();
                newBlur.useBlur = source.useBlur;
                newBlur.blurPassMode = source.blurPassMode;
                newBlur.blurDownscale = source.blurDownscale;
                newBlur.blurRadius = source.blurRadius;

                return newBlur;

            }

#endif

        }


        public PlanarReflectionRenderer castFromRenderer;
        public Material BlurMaterial;
        public bool[] castDepth = new bool[0];
        public bool[] castFog = new bool[0];
        public bool[] castReflection = new bool[0];
        public BlurSettings[] blurSettings = new BlurSettings[0];

        [SerializeField] protected Renderer rend;
         
        protected MaterialPropertyBlock mBlock;

        [SerializeField] protected Vector3Int _currentVersion = new Vector3Int( 6, 0, 0 );

        protected readonly Vector3Int _targetVersion = new Vector3Int( 6, 1, 0 );

#if UNITY_EDITOR

        private int _updateTestCounter = 3;
        
        public string Version { get { return $"v{_currentVersion.x}.{_currentVersion.y}.{_currentVersion.z}"; } }
#endif

#if UNITY_EDITOR

        private void OnValidate() {

            if ( _currentVersion != _targetVersion ) {
                SelfUpdate();
            }

        }

        private void SelfUpdate() {

            UnityEditor.Undo.RecordObject( this, this.GetInstanceID() + "_versionChanged" );
            Debug.Log( $"Updated from version {Version} to version v{_targetVersion.x}.{_targetVersion.y}.{_targetVersion.z}" );
            _currentVersion = _targetVersion;
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();

        }
#endif

        /// <summary>
        /// Tests whether the given renderer is visible through the reflection used in this caster from the given camera's point of view
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="fromCamera"></param>
        /// <returns></returns>
        public bool IsVisible( Renderer renderer, Camera fromCamera ) {


            return castFromRenderer.IsVisible( renderer, fromCamera, rend );

        }

        private void OnEnable() {

#if UNITY_EDITOR

            if ( !BlurMaterial ) {
                BlurMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>( UnityEditor.AssetDatabase.GUIDToAssetPath( UnityEditor.AssetDatabase.FindAssets( "PlanarReflections_InternalBlur" )[0] ) );
            }


#endif

            rend = GetComponent<Renderer>();


            if ( castDepth.Length != rend.sharedMaterials.Length )
                castDepth = new bool[rend.sharedMaterials.Length];

            if ( castReflection.Length != castDepth.Length )
                castReflection = new bool[castDepth.Length];

            if ( castFog.Length != castDepth.Length )
                castFog = new bool[castDepth.Length];

            if ( blurSettings.Length != castDepth.Length ) {
                blurSettings = new BlurSettings[castDepth.Length];
                for ( int i = 0; i < blurSettings.Length; i++ ) {
                    blurSettings[i].blurDownscale = Mathf.Clamp( blurSettings[i].blurDownscale, 1, 4 );
                }

            }


#if UPDATE_PLANAR3

            if ( GetComponent<PlanarReflections3.PlanarReflectionsCaster>() ) {
                var cast = GetComponent<PlanarReflections3.PlanarReflectionsCaster>();


                for ( int i = 0; i < blurSettings.Length; i++ ) {
                    RenderTexture.ReleaseTemporary( blurSettings[i].blurredMap );
                }

                blurSettings = new BlurSettings[cast.blurSettings.Length];

                for ( int i = 0; i < cast.blurSettings.Length; i++ ) {
                    blurSettings[i] = cast.blurSettings[i];
                }

                castReflection = cast.castReflection;
                castDepth = cast.castDepth;
            }



#endif

            RenderPipelineManager.beginCameraRendering -= AssignReflections;
            RenderPipelineManager.beginCameraRendering += AssignReflections;
            RenderPipelineManager.endCameraRendering -= BlackReflection;
            RenderPipelineManager.endCameraRendering += BlackReflection;

        }


        private void OnDisable() {

            RenderPipelineManager.beginCameraRendering -= AssignReflections;
            RenderPipelineManager.endCameraRendering -= BlackReflection;

            for ( int i = 0; i < blurSettings.Length; i++ ) {
                RenderTexture.ReleaseTemporary( blurSettings[i].blurredMap );
                RenderTexture.ReleaseTemporary( blurSettings[i].blurredDepth );
            }

            if ( rend ) {
                rend.SetPropertyBlock( null, 0 );
            }

        }


        private void OnWillRenderObject() {

            if ( castFromRenderer ) {
                castFromRenderer.RegisterCaster();
            }


#if UNITY_EDITOR

            if ( !Application.isPlaying ) {
                if ( _updateTestCounter > 0 ) {
                    _updateTestCounter--;
                }
                else {

                    if ( _currentVersion != _targetVersion ) {
                        SelfUpdate();
                    }
                }
            }

#endif


        }

        void BlackReflection( ScriptableRenderContext context, Camera cam ) {
            for ( int i = 0; i < castReflection.Length; i++ ) {
                mBlock.SetTexture( _blurReflectionTex, Texture2D.blackTexture );
                mBlock.SetTexture( _reflectionTex, Texture2D.blackTexture );
                rend.SetPropertyBlock( mBlock, i );
            }
        }


        void AssignReflections( ScriptableRenderContext context, Camera cam ) {

            if ( mBlock == null ) {
                mBlock = new MaterialPropertyBlock();
            }


            if ( !Application.isPlaying ) {

                if ( castDepth.Length != rend.sharedMaterials.Length )
                    castDepth = new bool[rend.sharedMaterials.Length];

                if ( castReflection.Length != castDepth.Length )
                    castReflection = new bool[castDepth.Length];

                if ( castFog.Length != castDepth.Length )
                    castFog = new bool[castDepth.Length];

                if ( blurSettings.Length != castDepth.Length ) {
                    blurSettings = new BlurSettings[castDepth.Length];
                    for ( int i = 0; i < blurSettings.Length; i++ ) {
                        blurSettings[i].blurDownscale = Mathf.Clamp( blurSettings[i].blurDownscale, 1, 4 );
                    }

                }
            }

            var uData = cam.GetUniversalAdditionalCameraData();

            if ( cam.cameraType == CameraType.Game && Application.isPlaying ) {
                mBlock.SetFloat( _vrMode, uData.allowXRRendering && UnityEngine.XR.XRSettings.enabled ? 1 : 0 );
            }
            else {
                mBlock.SetFloat( _vrMode, 0 );
            }

            if ( cam.TryGetComponent( out PlanarReflectionCamera refCam ) && refCam.isReflectionCamera ) {
                return;
            }

            for ( int i = 0; i < castReflection.Length; i++ ) {
                mBlock.SetTexture( _blurReflectionTex, Texture2D.blackTexture );
                mBlock.SetTexture( _reflectionTex, Texture2D.blackTexture );
                rend.SetPropertyBlock( mBlock, i );
            }


            if ( !castFromRenderer || !castFromRenderer.isActiveAndEnabled ) {
                return;
            }

            for ( int i = 0; i < castReflection.Length; i++ ) {

                rend.GetPropertyBlock( mBlock, i );

                if ( castReflection[i] || castDepth[i] ) {

                    Texture rTex = castFromRenderer.GetReflection( cam );
                    

                    if ( rTex != Texture2D.blackTexture && blurSettings[i].useBlur ) {

                        if ( blurSettings[i].forceFakeBlur ) {
                            RenderTexture.ReleaseTemporary( blurSettings[i].blurredMap );
                            var rd = new RenderTextureDescriptor( Mathf.Max( rTex.width / (blurSettings[i].blurDownscale * 2), 16 ), Mathf.Max( rTex.height / (blurSettings[i].blurDownscale * 2), 16 ), RenderTextureFormat.DefaultHDR, 0 );
                            rd.msaaSamples = 8;
                            blurSettings[i].blurredMap = RenderTexture.GetTemporary( rd );
                            var quality = BlurMaterial.GetFloat( "_KernelSize" );
                            BlurMaterial.SetFloat( "_KernelSize", 8 );
                            BlurMaterial.SetFloat( "_Radius", ( blurSettings[i].blurRadius + 0.01f ) * 16 );
                            Graphics.Blit( rTex, blurSettings[i].blurredMap, BlurMaterial );
                            BlurMaterial.SetFloat( "_KernelSize", quality );
                        }
                        else {

                            var rd = new RenderTextureDescriptor( Mathf.Max( rTex.width / blurSettings[i].blurDownscale, 1 ), Mathf.Max( rTex.height / blurSettings[i].blurDownscale, 1 ), RenderTextureFormat.Default, 0 );
                            rd.sRGB = false;

                            if ( !blurSettings[i].blurredMap ) {
                                blurSettings[i].blurredMap = RenderTexture.GetTemporary( rd );
                                rd.colorFormat = RenderTextureFormat.Depth;
                                rd.depthBufferBits = 16;
                            }
                            else if ( blurSettings[i].blurredMap.width != rTex.width / blurSettings[i].blurDownscale || blurSettings[i].blurredMap.height != rTex.height / blurSettings[i].blurDownscale ) {


                                RenderTexture.ReleaseTemporary( blurSettings[i].blurredMap );
                                RenderTexture.ReleaseTemporary( blurSettings[i].blurredDepth );

                                rd.depthBufferBits = 0;
                                rd.colorFormat = RenderTextureFormat.Default;
                                blurSettings[i].blurredMap = RenderTexture.GetTemporary( rd );
                                rd.colorFormat = RenderTextureFormat.Depth;
                                rd.depthBufferBits = 16;
                                blurSettings[i].blurredDepth = RenderTexture.GetTemporary( rd );
                            }

                            rd.colorFormat = RenderTextureFormat.Default;

                            BlurMaterial.SetFloat( "_Radius", ( blurSettings[i].blurRadius + 0.01f ) * 8 );
                            var tempRT = RenderTexture.GetTemporary( rd );
                            Graphics.Blit( rTex, blurSettings[i].blurredMap, BlurMaterial );
                            Graphics.Blit( blurSettings[i].blurredMap, tempRT, BlurMaterial );
                            Graphics.Blit( tempRT, blurSettings[i].blurredMap, BlurMaterial );
                            RenderTexture.ReleaseTemporary( tempRT );
                        }

                        if ( blurSettings[i].blurredMap ) {

                            if ( blurSettings[i].blurPassMode == 0 ) {
                                mBlock.SetTexture( _reflectionTex, blurSettings[i].blurredMap );
                            }
                            else {
                                mBlock.SetTexture( _blurReflectionTex, blurSettings[i].blurredMap );
                                mBlock.SetTexture( _reflectionTex, rTex );
                            }
                        }
                    }
                    else {
                        mBlock.SetTexture( _blurReflectionTex, rTex );
                        mBlock.SetTexture( _reflectionTex, rTex );
                        rend.SetPropertyBlock( mBlock, i );
                    }

                    if ( castDepth[i] && castFromRenderer.Settings.renderDepth ) {
                        mBlock.SetTexture( _reflectionDepth, castFromRenderer.GetReflectionDepth( cam ) );
                    }
                    else {
                        mBlock.SetTexture( _reflectionDepth, (Texture)Texture2D.whiteTexture );
                    } 
                    
                    if ( castFog[i] && castFromRenderer.Settings.renderFog ) {
                        rend.sharedMaterials[i].EnableKeyword( "_USE_FOG" );
                        mBlock.SetTexture( _reflectionFog, castFromRenderer.GetReflectionFog( cam ) );
                    }
                    else {
                        rend.sharedMaterials[i].DisableKeyword( "_USE_FOG" );
                        mBlock.SetTexture( _reflectionFog, (Texture)Texture2D.blackTexture );
                    }
                }
                else {
                    mBlock.SetTexture( _blurReflectionTex, (Texture)Texture2D.blackTexture );
                    mBlock.SetTexture( _reflectionTex, (Texture)Texture2D.blackTexture );
                }

                rend.SetPropertyBlock( mBlock, i );

            }
        }


    }

}