
namespace PlanarReflections6 {

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

    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.XR;

   

    [HelpURL( "https://irreverent-software.com/docs/planar-reflections-6/getting-started/planar-reflection-renderer/" )]
    [ExecuteAlways]
    public class PlanarReflectionRenderer : MonoBehaviour {


        protected List<XRNodeState> _nodeStates = new List<XRNodeState>();

#if UNITY_2023_3_OR_NEWER
        protected UniversalRenderPipeline.SingleCameraRequest rq = new UniversalRenderPipeline.SingleCameraRequest();
#endif

        [SerializeField] protected RenderTexture _externalReflectionTex;

        [SerializeField] protected RenderTexture _externalReflectionDepth;

        [SerializeField] protected Vector3Int _currentVersion = new Vector3Int( 6, 0, 0 );

        protected readonly Vector3Int _targetVersion = new Vector3Int( 6, 1, 0 );

#if UNITY_EDITOR

        public bool showAdvancedSettings;

        public Mesh defaultReflectorMesh;

        public Material defaultReflectorMaterial;

        [SerializeField]
        [InspectorName("Preview Reflective Plane")]
        [Tooltip("Renders a simple display plane where the reflection can be previewed.")]
        private bool _showPreviewReflector;

        [SerializeField]
        [Tooltip("If enabled, the reflection will be rendered and visible in the Scene View both in Play Mode and Edit Mode")]
        private bool _previewInSceneView = true;

        public string Version { get { return $"v{_currentVersion.x}.{_currentVersion.y}.{_currentVersion.z}"; } }

#endif

        protected int _currentActiveCasters;

        protected float _internalTimer;



        [SerializeField] protected PlanarReflectionSettings _settings = new PlanarReflectionSettings();

        public PlanarReflectionSettings Settings { get { return _settings; } }

        protected Camera _reflectionCam;

        protected ReflectionData _gameReflection;


#if UNITY_EDITOR

        protected ReflectionData _sceneReflection;

        private MaterialPropertyBlock _sceneReflectorMatBlock;

#endif


        public delegate void ReflectionRenderCallback( Camera forCamera, Camera withReflectionCamera, RenderTexture reflectionTexture );

        public ReflectionRenderCallback OnReflectionRendered;

        public ReflectionRenderCallback OnWillRenderReflection;


        private Plane[] _visibilityPlanes = new Plane[6];

        private int _updateTestCounter = 3;


#if UNITY_EDITOR
        private void OnValidate() {

            if ( !_settings.vrMat ) {
                _settings.vrMat = new Material( Shader.Find( "Hidden/Planar Reflections/Internal/VRCombine" ) );
            }


        }

        private void SelfUpdate() {
            UnityEditor.Undo.RecordObject( this, GetInstanceID() + "_versionChange" );
            Debug.Log( $"Updated from version {Version} to version v{_targetVersion.x}.{_targetVersion.y}.{_targetVersion.z}" );
            _currentVersion = _targetVersion;
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        }

#endif

        public Texture GetReflection( Camera cam ) {

            var planarCam = cam.GetComponent<PlanarReflectionCamera>();

            if ( cam.cameraType == CameraType.SceneView ) {

#if UNITY_EDITOR
                if ( !_externalReflectionTex && !_sceneReflection.reflectionTex ) {
                    return Texture2D.blackTexture;
                }
                return _externalReflectionTex ? (Texture)_externalReflectionTex : _sceneReflection.reflectionTex;
#else
                return Texture2D.blackTexture;
#endif
            }
            else {

                switch ( _settings.cameraFilteringMode ) {

                    case PlanarReflectionSettings.CameraFilteringMode.AllCameras:
                        if ( planarCam && planarCam.isReflectionCamera ) {
                            return Texture2D.blackTexture;
                        }
                        break;

                    case PlanarReflectionSettings.CameraFilteringMode.ByComponentOnly:
                        if ( !planarCam || planarCam.isReflectionCamera ) {
                            return Texture2D.blackTexture;
                        }
                        break;

                    case PlanarReflectionSettings.CameraFilteringMode.ByPrefix:
                        if (planarCam && planarCam.isReflectionCamera || ( !string.IsNullOrEmpty( _settings.camerasPrefix) && !cam.gameObject.name.Contains(_settings.camerasPrefix) ) ) {
                            return Texture2D.blackTexture;
                        }
                        break;

                } 

                if ( !_externalReflectionTex && !_gameReflection.reflectionTex ) {
                    return Texture2D.blackTexture;
                }

                return _externalReflectionTex ? (Texture)_externalReflectionTex : _gameReflection.reflectionTex;

            }


        }



        public Texture GetReflectionDepth( Camera cam ) {

            var planarCam = cam.GetComponent<PlanarReflectionCamera>();

            if ( cam.cameraType == CameraType.SceneView ) {
#if UNITY_EDITOR
                if ( !_externalReflectionDepth && !_sceneReflection.reflectionDepth ) {
                    return Texture2D.blackTexture;
                }
                return _externalReflectionDepth ? (Texture)_externalReflectionDepth : _sceneReflection.reflectionDepth;
#else
                return Texture2D.blackTexture;
#endif
            }
            else {
                switch ( _settings.cameraFilteringMode ) {

                    case PlanarReflectionSettings.CameraFilteringMode.AllCameras:
                        if ( planarCam && planarCam.isReflectionCamera ) {
                            return Texture2D.blackTexture;
                        }
                        break;

                    case PlanarReflectionSettings.CameraFilteringMode.ByComponentOnly:
                        if ( !planarCam || planarCam.isReflectionCamera ) {
                            return Texture2D.blackTexture;
                        }
                        break;

                    case PlanarReflectionSettings.CameraFilteringMode.ByPrefix:
                        if ( planarCam && planarCam.isReflectionCamera || ( !string.IsNullOrEmpty( _settings.camerasPrefix ) && !cam.gameObject.name.Contains( _settings.camerasPrefix ) ) ) {
                            return Texture2D.blackTexture;
                        }
                        break;

                }

                if ( !_externalReflectionDepth && !_gameReflection.reflectionDepth ) {
                    return Texture2D.blackTexture;
                }

                return _externalReflectionDepth ? (Texture)_externalReflectionDepth : _gameReflection.reflectionDepth;
                
            }
            
        }


        public Texture GetReflectionFog( Camera cam ) {

            var planarCam = cam.GetComponent<PlanarReflectionCamera>();

            if ( cam.cameraType == CameraType.SceneView ) {
#if UNITY_EDITOR
                return _sceneReflection != null && _sceneReflection.reflectionFog != null ? (Texture)_sceneReflection.reflectionFog : Texture2D.blackTexture;
#else
                return Texture2D.blackTexture;
#endif
            }
            else{
                switch ( _settings.cameraFilteringMode ) {

                    case PlanarReflectionSettings.CameraFilteringMode.AllCameras:
                        if ( planarCam && planarCam.isReflectionCamera ) {
                            return Texture2D.blackTexture;
                        }
                        break;

                    case PlanarReflectionSettings.CameraFilteringMode.ByComponentOnly:
                        if ( !planarCam || planarCam.isReflectionCamera ) {
                            return Texture2D.blackTexture;
                        }
                        break;

                    case PlanarReflectionSettings.CameraFilteringMode.ByPrefix:
                        if ( planarCam && planarCam.isReflectionCamera || ( !string.IsNullOrEmpty( _settings.camerasPrefix ) && !cam.gameObject.name.Contains( _settings.camerasPrefix ) ) ) {
                            return Texture2D.blackTexture;
                        }
                        break;

                }

                if ( !_gameReflection.reflectionFog ) {
                    return Texture2D.blackTexture;
                }

                return _gameReflection.reflectionFog;
            }

        }


        public void RegisterCaster() {
            _currentActiveCasters++;
        }


#if UNITY_EDITOR
        [UnityEditor.MenuItem( "GameObject/Effects/Planar Reflections 6/Create Reflections Renderer", priority = -99 )]
        public static void CreateReflectionsRendererObject() {

            var reflector = new GameObject( "Reflection Renderer", typeof( PlanarReflectionRenderer ) );
            reflector.transform.position = Vector3.zero;
            reflector.transform.rotation = Quaternion.identity;

        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded() {
            var cams = Resources.FindObjectsOfTypeAll<Camera>();

            foreach ( Camera cam in cams ) {
                if ( cam.name.Contains( "_RefCamera" ) ) {
#if !UNITY_2018_3_OR_NEWER
                    DestroyImmediate( cam.targetTexture );
#else
                    RenderTexture.ReleaseTemporary( cam.targetTexture );
#endif
                    cam.targetTexture = null;
                    DestroyImmediate( cam.gameObject );
                }
            }
        }

#endif




        public void OnEnable() {

            if ( !_reflectionCam ) {
                var rCam = new GameObject( "_RefCamera", typeof( Camera ), typeof( UniversalAdditionalCameraData ), typeof(PlanarReflectionCamera) );

                _reflectionCam = rCam.GetComponent<Camera>();

                rCam.hideFlags = HideFlags.HideAndDontSave;
                rCam.GetComponent<Camera>().enabled = false;
                rCam.GetComponent<Camera>().cameraType = CameraType.Game;
            }


            _gameReflection = new ReflectionData( _reflectionCam, _settings );
            _gameReflection.RegenerateTextures( _settings );

#if UNITY_EDITOR

            _sceneReflection = new ReflectionData( _reflectionCam, _settings );
            _sceneReflection.RegenerateTextures( _settings );

#if UNITY_2018_1_OR_NEWER
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            UnityEditor.SceneView.RepaintAll();
#endif


#endif


#if UPDATE_PLANAR3
            if ( GetComponent<PlanarReflections3.PlanarReflectionsRenderer>() ) {
                _settings = GetComponent<PlanarReflections3.PlanarReflectionsRenderer>().Settings;
            }
#endif


            //Camera.onPreCull += RenderReflection;

            RenderPipelineManager.beginContextRendering -= RenderAllCameras;
            RenderPipelineManager.beginContextRendering += RenderAllCameras;
            RenderPipelineManager.beginCameraRendering -= DrawPreview;
            RenderPipelineManager.beginCameraRendering += DrawPreview;

        }

        public void OnDisable() {

#if UNITY_EDITOR
            UnityEditor.Undo.undoRedoPerformed -= ApplySettings;

#endif
            RenderPipelineManager.beginContextRendering -= RenderAllCameras;


            RenderPipelineManager.beginCameraRendering -= DrawPreview;
#if UNITY_EDITOR
            _sceneReflection.Release();
#endif
            _gameReflection.Release();

            if ( _reflectionCam ) {
                DestroyImmediate( _reflectionCam.gameObject );
            }

#if UNITY_EDITOR
            _sceneReflection = default;
#endif

        }


        public void ApplySettings() {

            _gameReflection.RegenerateTextures( _settings );

#if UNITY_EDITOR
            if ( _sceneReflection != null )
                _sceneReflection.RegenerateTextures( _settings );
#endif

        }

        void RenderAllCameras( ScriptableRenderContext context, List<Camera> cams ) {

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

            if ( !_reflectionCam ) {
                var rCam = new GameObject( "_RefCamera", typeof( Camera ), typeof( UniversalAdditionalCameraData ), typeof( PlanarReflectionCamera ) );

                _reflectionCam = rCam.GetComponent<Camera>();
                rCam.hideFlags = HideFlags.HideAndDontSave;
                rCam.GetComponent<Camera>().enabled = false;
                rCam.GetComponent<Camera>().cameraType = CameraType.Game;
                return;
            }

            _reflectionCam.GetComponent<PlanarReflectionCamera>().isReflectionCamera = true;

            for ( int i = 0; i < cams.Count; i++ ) {
                if ( !cams[i] ) {
                    continue;
                }
                _reflectionCam.transform.position = cams[i].transform.position;
                _reflectionCam.transform.rotation = cams[i].transform.rotation;
#if UNITY_2023_3_OR_NEWER
                RenderURPReflection( cams[i] );
#else
                RenderURPReflection( context, cams[i] );
#endif
            }

            _currentActiveCasters = 0;

        }


        void DrawPreview( ScriptableRenderContext context, Camera cam ) {
#if UNITY_EDITOR
            if ( cam.cameraType == CameraType.SceneView && _showPreviewReflector ) {
                DrawReflectorMesh( cam, _sceneReflection );
            }
#endif
        }



        /// <summary>
        /// Calculates a reflection matrix for this Reflection Renderer's plane
        /// </summary>
        /// <param name="reflectionMat"></param>
        private void CalculateReflectionMatrix( ref Matrix4x4 reflectionMat ) {

            Vector4 plane = new Vector4( transform.up.x, transform.up.y, transform.up.z, -Vector3.Dot( transform.position, transform.up ) );

            reflectionMat.m00 = ( 1F - 2F * plane[0] * plane[0] );
            reflectionMat.m01 = ( -2F * plane[0] * plane[1] );
            reflectionMat.m02 = ( -2F * plane[0] * plane[2] );
            reflectionMat.m03 = ( -2F * plane[3] * plane[0] );

            reflectionMat.m10 = ( -2F * plane[1] * plane[0] );
            reflectionMat.m11 = ( 1F - 2F * plane[1] * plane[1] );
            reflectionMat.m12 = ( -2F * plane[1] * plane[2] );
            reflectionMat.m13 = ( -2F * plane[3] * plane[1] );

            reflectionMat.m20 = ( -2F * plane[2] * plane[0] );
            reflectionMat.m21 = ( -2F * plane[2] * plane[1] );
            reflectionMat.m22 = ( 1F - 2F * plane[2] * plane[2] );
            reflectionMat.m23 = ( -2F * plane[3] * plane[2] );

            reflectionMat.m30 = 0F;
            reflectionMat.m31 = 0F;
            reflectionMat.m32 = 0F;
            reflectionMat.m33 = 1F;
        }



        /// <summary>
        /// Transforms the WorldToCamera matrix of a given camera by reflecting it around this Reflection Renderer's plane
        /// </summary>
        /// <param name="fromCamera"></param>
        /// <returns></returns>
        public Matrix4x4 ReflectWorldToCameraMatrix( Camera fromCamera ) {

            Matrix4x4 reflectedMatrix = Matrix4x4.zero;

            CalculateReflectionMatrix( ref reflectedMatrix );

            return fromCamera.worldToCameraMatrix * reflectedMatrix;

        }

        /// <summary>
        /// Taking a transform as a reference it reflects its position and rotation based on this Reflection Renderer's Plane
        /// and applies the new values to another transform.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="reflected"></param>
        public void ReflectTransform( Transform original, Transform reflected ) {

            Vector3 relativePos = original.position - transform.position;
            Vector3 forward = original.TransformPoint( 0, 0, 1 ) - transform.position;
            Vector3 up = original.TransformPoint( 0, 1, 0 ) - transform.position;
            Vector3 right = original.TransformPoint( 1, 0, 0 ) - transform.position;

            reflected.position = Vector3.Reflect( relativePos, transform.up ) + transform.position;
            
            forward = Vector3.Reflect( forward, transform.up ) + transform.position;
            up = Vector3.Reflect( up, transform.up ) + transform.position;
            right = Vector3.Reflect( right, transform.up ) + transform.position;

            up = ( up - reflected.position ).normalized;

            reflected.LookAt( forward, up );

        }


        /// <summary>
        /// Reflects a position around this Reflection Renderer's plane
        /// </summary>
        /// <param name="original"></param>
        /// <returns>The reflected position</returns>
        public Vector3 ReflectPosition( Vector3 original ) {

            original = original - transform.position;
            return Vector3.Reflect( original, transform.up ) + transform.position;

        }


        /// <summary>
        /// Tests whether the given renderer is visible for the given camera through this reflection. If provided, it will test for visibility confined to the
        /// reflective area of the reflection caster mesh
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="fromCamera"></param>
        /// <returns></returns>
        public bool IsVisible( Renderer renderer, Camera fromCamera, Renderer onCaster ) {

            Vector3 oldPos = renderer.transform.position;
            Quaternion oldRot = renderer.transform.rotation;

            ReflectTransform( renderer.transform, renderer.transform );

            Bounds visBounds = renderer.bounds;
            float scale = visBounds.extents.magnitude;
            Vector3 refScreenPos = renderer.transform.position;
            refScreenPos = fromCamera.WorldToViewportPoint( refScreenPos );


            Ray cameraRay = fromCamera.ViewportPointToRay( refScreenPos );
            Plane refPlane = new Plane( transform.up, transform.position );

            renderer.transform.position = oldPos;
            renderer.transform.rotation = oldRot;

            GeometryUtility.CalculateFrustumPlanes( fromCamera, _visibilityPlanes );


            if ( refPlane.Raycast( cameraRay, out float intersectionPoint ) ) {

                Vector3 bMax = fromCamera.WorldToScreenPoint( visBounds.max );
                Vector3 bMin = fromCamera.WorldToScreenPoint( visBounds.min );

                visBounds.center = cameraRay.GetPoint(intersectionPoint);

                Vector3 bMax2 = fromCamera.WorldToScreenPoint( visBounds.max );
                Vector3 bMin2 = fromCamera.WorldToScreenPoint( visBounds.min );

                float dist1 = Vector2.Distance( bMax, bMin );
                float dist2 = Vector2.Distance( bMax2, bMin2 );
                scale = ( dist1 / dist2 );
                visBounds.extents = visBounds.extents * (dist1/dist2);

                if (!GeometryUtility.TestPlanesAABB(_visibilityPlanes, visBounds ) ) {
                    return false;
                }

                return onCaster.bounds.Intersects( visBounds );    

            }

            return false;

        }


        /// <summary>
        /// Renders a URP-based reflection (with support for VR) for the given camera.
        /// </summary>
#if UNITY_2023_3_OR_NEWER
        public void RenderURPReflection( Camera cam ) {
#else
        public void RenderURPReflection( ScriptableRenderContext context, Camera cam ) {
#endif


            var planarCam = cam.GetComponent<PlanarReflectionCamera>();

            if ( cam.cameraType == CameraType.Preview || cam.GetUniversalAdditionalCameraData().renderType == CameraRenderType.Overlay ) {
                return;
            }

            if ( _currentActiveCasters < 1 && _settings.updateOnCastOnly && cam.cameraType != CameraType.SceneView ) {
                return;
            }



            var isSceneCam = cam.cameraType == CameraType.SceneView;

            if ( !isSceneCam && !planarCam && cam.gameObject.hideFlags.HasFlag( HideFlags.HideInHierarchy ) ) {
                return;
            }

            if ( !isSceneCam ) {

                switch ( _settings.cameraFilteringMode ) {

                    case PlanarReflectionSettings.CameraFilteringMode.ByPrefix:
                        if ( !string.IsNullOrEmpty( _settings.camerasPrefix ) ) {
                            if ( cam.cameraType != CameraType.SceneView && !cam.name.Contains( _settings.camerasPrefix ) ) {
                                return;
                            }
                        }
                        break;

                    case PlanarReflectionSettings.CameraFilteringMode.ByComponentOnly:

                        if ( !planarCam || planarCam.isReflectionCamera ) {
                            return;
                        }

                        break;

                    case PlanarReflectionSettings.CameraFilteringMode.AllCameras:

                        if ( planarCam && planarCam.isReflectionCamera ) {
                            return;
                        }

                        break;
                }
            
            }
            else {
#if UNITY_EDITOR
                if ( !_previewInSceneView ) {
                    return;
                }
#endif
            }


            bool isVRCamera = cam.cameraType == CameraType.Game && cam.GetUniversalAdditionalCameraData().allowXRRendering && XRSettings.enabled;

            Vector3 leftEyePos = cam.transform.position;
            Vector3 rightEyePos = cam.transform.position;

            if ( isVRCamera ) {
                InputTracking.GetNodeStates( _nodeStates );

                for ( int i = 0; i < _nodeStates.Count; i++ ) {
                    if ( _nodeStates[i].nodeType == XRNode.LeftEye ) {
                        _nodeStates[i].TryGetPosition( out leftEyePos );
                    }
                    else if ( _nodeStates[i].nodeType == XRNode.RightEye ) {
                        _nodeStates[i].TryGetPosition( out rightEyePos );
                    }
                }
            }

            float eyeDist = Vector3.Distance( leftEyePos, rightEyePos ) / 2;


            Plane reflectionPlane = new Plane( transform.up, transform.position );

            if ( Mathf.Abs( Vector3.Dot( transform.up, cam.transform.forward ) ) < 0.01f && ( cam.orthographic || reflectionPlane.GetDistanceToPoint( cam.transform.position ) < 0.025f ) ) {
                return;
            }

#if UNITY_EDITOR



            if ( isSceneCam ) {
                if ( Screen.width != _sceneReflection.screenRes.x || Screen.height != _sceneReflection.screenRes.y ) {
                    _sceneReflection.RegenerateTextures( _settings );
                }
            }
            else {
                if ( Screen.width != _gameReflection.screenRes.x || Screen.height != _gameReflection.screenRes.y ) {
                    _gameReflection.RegenerateTextures( _settings );
                }
            }



            var currentData = isSceneCam ? _sceneReflection : _gameReflection;

#else
            if ( Screen.width != _gameReflection.screenRes.x || Screen.height != _gameReflection.screenRes.y ) {
                _gameReflection.RegenerateTextures( _settings );
            }

            var currentData = _gameReflection;
#endif
            var refCamera = _reflectionCam;

            refCamera.CopyFrom( cam );

            refCamera.depth = cam.depth + 99;
            refCamera.allowHDR = cam.allowHDR;
            refCamera.allowMSAA = cam.allowMSAA;
            refCamera.useOcclusionCulling = false;
            refCamera.cullingMask = _settings.reflectLayers;

            switch ( _settings.reflectionBackground ) {
                case PlanarReflectionSettings.ReflectionBackground.CopyFromCamera:
                    refCamera.clearFlags = cam.clearFlags;
                    break;

                case PlanarReflectionSettings.ReflectionBackground.Skybox:
                    refCamera.clearFlags = CameraClearFlags.Skybox;
                    break;

                case PlanarReflectionSettings.ReflectionBackground.SolidColor:
                    refCamera.clearFlags = CameraClearFlags.SolidColor;
                    refCamera.backgroundColor = _settings.backgroundColor;
                    break;

                case PlanarReflectionSettings.ReflectionBackground.Transparent:
                    refCamera.clearFlags = CameraClearFlags.SolidColor;
                    _settings.backgroundColor.a = 0;
                    refCamera.backgroundColor = _settings.backgroundColor;

                    if ( !_settings.renderDepth ) {
                        _settings.forceHDR = false;
                    }

                    break;


            }


            refCamera.cameraType = CameraType.Game;
            refCamera.renderingPath = cam.renderingPath;

            refCamera.useOcclusionCulling = false;

            refCamera.targetTexture = _externalReflectionTex ? _externalReflectionTex : currentData.reflectionTex;


            var uData = refCamera.GetUniversalAdditionalCameraData();

            uData.renderType = CameraRenderType.Base;

            refCamera.worldToCameraMatrix = ReflectWorldToCameraMatrix( cam );

            refCamera.nearClipPlane = _settings.nearClipPlane;
            refCamera.farClipPlane = _settings.farClipPlane;

            refCamera.rect = new Rect( 0, 0, 1, 1 );
            refCamera.aspect = cam.aspect;

            ReflectTransform( cam.transform, refCamera.transform );

            uData.SetRenderer( _settings.reflectionURPRendererIndex );

            if ( _settings.accurateMatrix ) {
                refCamera.projectionMatrix = refCamera.CalculateObliqueMatrix( CameraSpacePlane( refCamera, transform.position, transform.up ) );
            }

            var tempLOD = QualitySettings.lodBias;
            var maxLod = QualitySettings.maximumLODLevel;

            QualitySettings.lodBias *= _settings.customLODBias;
            QualitySettings.maximumLODLevel = _settings.maxLODLevel;


            uData.renderShadows = _settings.renderShadows;
            uData.volumeLayerMask = _settings.postFXVolumeMask;
            uData.antialiasing = _settings.useAntialiasing ? AntialiasingMode.FastApproximateAntialiasing : AntialiasingMode.None;

            OnWillRenderReflection?.Invoke( cam, refCamera, null );

            var oldCulling = GL.invertCulling;
            GL.invertCulling = true;

            if (
#if UNITY_EDITOR
            !Application.isPlaying ||
#endif
            ( Time.realtimeSinceStartup > _internalTimer ) ) {


                if ( _settings.renderFog ) {
                    refCamera.targetTexture = currentData.reflectionFog;
                    refCamera.depthTextureMode = DepthTextureMode.None;
                    uData.renderPostProcessing = false;

                    uData.SetRenderer( _settings.fogRendererIndex );
                    var clearF = refCamera.clearFlags;
                    var clearCol = refCamera.backgroundColor;
                    refCamera.clearFlags = CameraClearFlags.Color;
                    refCamera.backgroundColor = Color.clear;


#if UNITY_2023_3_OR_NEWER

                    if ( Application.isPlaying && cam.cameraType == CameraType.Game && isVRCamera ) {

                        GL.invertCulling = true;

                        refCamera.worldToCameraMatrix = ReflectWorldToCameraMatrix( cam );

                        var rendTex = _externalReflectionDepth ? _externalReflectionDepth : _gameReflection.reflectionDepth;

                        rq.destination = rendTex;
                        refCamera.projectionMatrix = cam.GetStereoProjectionMatrix( Camera.StereoscopicEye.Left );

                        if ( _settings.accurateMatrix ) {
                            refCamera.projectionMatrix = refCamera.CalculateObliqueMatrix( CameraSpacePlane( refCamera, transform.position, transform.up, Camera.StereoscopicEye.Left, eyeDist ) );
                        }

                        UniversalRenderPipeline.SubmitRenderRequest( refCamera, rq );

                        GL.invertCulling = oldCulling;

                        var depthRD = _gameReflection.rd;

                        var tempBufferL = RenderTexture.GetTemporary( depthRD );

                        Graphics.Blit( rendTex, tempBufferL );

                        GL.invertCulling = true;

                        refCamera.worldToCameraMatrix = ReflectWorldToCameraMatrix( cam );

                        rq.destination = rendTex;
                        refCamera.projectionMatrix = cam.GetStereoProjectionMatrix( Camera.StereoscopicEye.Right );

                        if ( _settings.accurateMatrix ) {
                            refCamera.projectionMatrix = refCamera.CalculateObliqueMatrix( CameraSpacePlane( refCamera, transform.position, transform.up, Camera.StereoscopicEye.Right, eyeDist ) );
                        }

                        UniversalRenderPipeline.SubmitRenderRequest( refCamera, rq );


                        var tempBuffer = RenderTexture.GetTemporary( depthRD );

                        GL.invertCulling = oldCulling;

                        Graphics.Blit( tempBufferL, tempBuffer );
                        _settings.vrMat.SetTexture( "_RightEyeTex", tempBuffer );

                        Graphics.Blit( tempBuffer, rendTex, _settings.vrMat );

                        RenderTexture.ReleaseTemporary( tempBuffer );
                        RenderTexture.ReleaseTemporary( tempBufferL );

                        GL.invertCulling = true;
                    }
                    else {
                        rq.destination = currentData.reflectionFog;
                        UniversalRenderPipeline.SubmitRenderRequest( refCamera, rq );
                    }

#else

                    if ( Application.isPlaying && cam.cameraType == CameraType.Game && isVRCamera ) {

                        GL.invertCulling = true;

                        refCamera.worldToCameraMatrix = ReflectWorldToCameraMatrix( cam );

                        var rendTex = currentData.reflectionFog;

                        refCamera.targetTexture = rendTex;

                        refCamera.projectionMatrix = cam.GetStereoProjectionMatrix( Camera.StereoscopicEye.Left );

                        if ( _settings.accurateMatrix ) {
                            refCamera.projectionMatrix = refCamera.CalculateObliqueMatrix( CameraSpacePlane( refCamera, transform.position, transform.up, Camera.StereoscopicEye.Left, eyeDist ) );
                        }

                        UniversalRenderPipeline.RenderSingleCamera( context, refCamera );

                        GL.invertCulling = oldCulling;

                        var tempBufferL = RenderTexture.GetTemporary( currentData.rd );

                        Graphics.Blit( rendTex, tempBufferL );

                        GL.invertCulling = true;

                        refCamera.worldToCameraMatrix = ReflectWorldToCameraMatrix( cam );

                        refCamera.targetTexture = rendTex;
                        refCamera.projectionMatrix = cam.GetStereoProjectionMatrix( Camera.StereoscopicEye.Right );

                        if ( _settings.accurateMatrix ) {
                            refCamera.projectionMatrix = refCamera.CalculateObliqueMatrix( CameraSpacePlane( refCamera, transform.position, transform.up, Camera.StereoscopicEye.Right, eyeDist ) );
                        }

                        UniversalRenderPipeline.RenderSingleCamera( context, refCamera );


                        var tempBuffer = RenderTexture.GetTemporary( currentData.rd );

                        GL.invertCulling = oldCulling;

                        Graphics.Blit( tempBufferL, tempBuffer );
                        _settings.vrMat.SetTexture( "_RightEyeTex", tempBuffer );

                        Graphics.Blit( tempBuffer, rendTex, _settings.vrMat );

                        RenderTexture.ReleaseTemporary( tempBuffer );
                        RenderTexture.ReleaseTemporary( tempBufferL );

                        GL.invertCulling = true;
                    }
                    else {
                        UniversalRenderPipeline.RenderSingleCamera( context, refCamera );
                    }
#endif
                   
                

                    refCamera.clearFlags = clearF;
                    refCamera.backgroundColor = clearCol;
                    uData.SetRenderer( 0 );
                }

#if !UNITY_2023_OR_NEWER
                if ( _settings.renderDepth && !isSceneCam ) {
#else
                    if ( _settings.renderDepth ) {

#endif
                    refCamera.targetTexture = _externalReflectionDepth ? _externalReflectionDepth : currentData.reflectionDepth;
                    refCamera.depthTextureMode = DepthTextureMode.Depth;
                    refCamera.aspect = cam.aspect;
                    uData.renderPostProcessing = false;

                    uData.requiresDepthOption = CameraOverrideOption.On;
                    uData.requiresColorOption = CameraOverrideOption.Off;
                    var clearF = refCamera.clearFlags;
                    refCamera.clearFlags = CameraClearFlags.Depth;
                    uData.renderShadows = false;


#if UNITY_2023_3_OR_NEWER

                    if ( Application.isPlaying && cam.cameraType == CameraType.Game && isVRCamera ) {

                        GL.invertCulling = true;

                        refCamera.worldToCameraMatrix = ReflectWorldToCameraMatrix( cam );

                        var rendTex = _externalReflectionDepth ? _externalReflectionDepth : _gameReflection.reflectionDepth;

                        rq.destination = rendTex;
                        refCamera.projectionMatrix = cam.GetStereoProjectionMatrix( Camera.StereoscopicEye.Left );

                        if ( _settings.accurateMatrix ) {
                            refCamera.projectionMatrix = refCamera.CalculateObliqueMatrix( CameraSpacePlane( refCamera, transform.position, transform.up, Camera.StereoscopicEye.Left, eyeDist ) );
                        }

                        UniversalRenderPipeline.SubmitRenderRequest( refCamera, rq );

                        GL.invertCulling = oldCulling;

                        var depthRD = _gameReflection.rd;
                        depthRD.colorFormat = RenderTextureFormat.Depth;

                        var tempBufferL = RenderTexture.GetTemporary( depthRD );

                        _settings.vrMat.SetTexture( "_RightEyeTex", rendTex );

                        Graphics.Blit( rendTex, tempBufferL, _settings.vrMat, 2 );

                        GL.invertCulling = true;

                        refCamera.worldToCameraMatrix = ReflectWorldToCameraMatrix( cam );

                        rq.destination = rendTex;
                        refCamera.projectionMatrix = cam.GetStereoProjectionMatrix( Camera.StereoscopicEye.Right );

                        if ( _settings.accurateMatrix ) {
                            refCamera.projectionMatrix = refCamera.CalculateObliqueMatrix( CameraSpacePlane( refCamera, transform.position, transform.up, Camera.StereoscopicEye.Right, eyeDist ) );
                        }

                        UniversalRenderPipeline.SubmitRenderRequest( refCamera, rq );


                        var tempBuffer = RenderTexture.GetTemporary( depthRD );

                        _settings.vrMat.SetTexture( "_RightEyeTex", rendTex );

                        GL.invertCulling = oldCulling;

                        Graphics.Blit( tempBufferL, tempBuffer, _settings.vrMat, 1 );
                        _settings.vrMat.SetTexture( "_RightEyeTex", tempBuffer );

                        Graphics.Blit( tempBuffer, rendTex, _settings.vrMat, 2 );

                        RenderTexture.ReleaseTemporary( tempBuffer );
                        RenderTexture.ReleaseTemporary( tempBufferL );

                        GL.invertCulling = true;
                    }
                    else {
                        rq.destination = _externalReflectionDepth ? _externalReflectionDepth : currentData.reflectionDepth;
                        UniversalRenderPipeline.SubmitRenderRequest( refCamera, rq );
                    }

#else

                    if ( Application.isPlaying && cam.cameraType == CameraType.Game && isVRCamera ) {

                        GL.invertCulling = true;

                        refCamera.worldToCameraMatrix = ReflectWorldToCameraMatrix( cam );

                        var rendTex = _externalReflectionDepth ? _externalReflectionDepth : _gameReflection.reflectionDepth;

                        refCamera.targetTexture = rendTex;

                        refCamera.projectionMatrix = cam.GetStereoProjectionMatrix( Camera.StereoscopicEye.Left );

                        if ( _settings.accurateMatrix ) {
                            refCamera.projectionMatrix = refCamera.CalculateObliqueMatrix( CameraSpacePlane( refCamera, transform.position, transform.up, Camera.StereoscopicEye.Left, eyeDist ) );
                        }

                        UniversalRenderPipeline.RenderSingleCamera( context, refCamera );

                        GL.invertCulling = oldCulling;

                        var depthRD = _gameReflection.rd;
                        depthRD.colorFormat = RenderTextureFormat.Depth;

                        var tempBufferL = RenderTexture.GetTemporary( depthRD );

                        _settings.vrMat.SetTexture( "_RightEyeTex", rendTex );

                        Graphics.Blit( rendTex, tempBufferL, _settings.vrMat, 2 );

                        GL.invertCulling = true;

                        refCamera.worldToCameraMatrix = ReflectWorldToCameraMatrix( cam );

                        refCamera.targetTexture = rendTex;
                        refCamera.projectionMatrix = cam.GetStereoProjectionMatrix( Camera.StereoscopicEye.Right );

                        if ( _settings.accurateMatrix ) {
                            refCamera.projectionMatrix = refCamera.CalculateObliqueMatrix( CameraSpacePlane( refCamera, transform.position, transform.up, Camera.StereoscopicEye.Right, eyeDist ) );
                        }

                        UniversalRenderPipeline.RenderSingleCamera( context, refCamera );


                        var tempBuffer = RenderTexture.GetTemporary( depthRD );

                        _settings.vrMat.SetTexture( "_RightEyeTex", rendTex );

                        GL.invertCulling = oldCulling;

                        Graphics.Blit( tempBufferL, tempBuffer, _settings.vrMat, 1 );
                        _settings.vrMat.SetTexture( "_RightEyeTex", tempBuffer );

                        Graphics.Blit( tempBuffer, rendTex, _settings.vrMat, 2 );

                        RenderTexture.ReleaseTemporary( tempBuffer );
                        RenderTexture.ReleaseTemporary( tempBufferL );

                        GL.invertCulling = true;
                    }
                    else {
                        UniversalRenderPipeline.RenderSingleCamera( context, refCamera );
                    }
#endif
                    uData.renderPostProcessing = false;
                    uData.volumeLayerMask = _settings.postFXVolumeMask;

                    refCamera.clearFlags = clearF;

                }

                refCamera.targetTexture = _externalReflectionTex ? _externalReflectionTex : currentData.reflectionTex;


                uData.renderPostProcessing = _settings.usePostFX;
                uData.volumeLayerMask = _settings.postFXVolumeMask;

                uData.requiresDepthOption = CameraOverrideOption.Off;
                uData.requiresColorOption = CameraOverrideOption.Off;


                refCamera.enabled = _settings.usePostFX;

#if UNITY_2023_3_OR_NEWER

                

                if ( Application.isPlaying && cam.cameraType == CameraType.Game && isVRCamera ) {

                    uData.allowXRRendering = false;
                    uData.renderPostProcessing = _settings.usePostFX;
                    uData.volumeLayerMask = _settings.postFXVolumeMask;

                    GL.invertCulling = true;

                    refCamera.worldToCameraMatrix = ReflectWorldToCameraMatrix( cam );

                    refCamera.targetTexture = _externalReflectionTex?_externalReflectionTex:_gameReflection.reflectionTex;
                    rq.destination = _externalReflectionTex?_externalReflectionTex:_gameReflection.reflectionTex;

                    refCamera.projectionMatrix = cam.GetStereoProjectionMatrix( Camera.StereoscopicEye.Left );

                    if ( _settings.accurateMatrix ) {
                        refCamera.projectionMatrix = refCamera.CalculateObliqueMatrix( CameraSpacePlane( refCamera, transform.position, transform.up, Camera.StereoscopicEye.Left, eyeDist ) );
                    }


                    UniversalRenderPipeline.SubmitRenderRequest( refCamera, rq );

                    GL.invertCulling = oldCulling;

                    var tempBufferL = RenderTexture.GetTemporary( _gameReflection.rd );

                    Graphics.Blit( _externalReflectionTex?_externalReflectionTex:_gameReflection.reflectionTex, tempBufferL );

                    GL.invertCulling = true;

                    refCamera.worldToCameraMatrix = ReflectWorldToCameraMatrix( cam );

                    refCamera.targetTexture = _externalReflectionTex?_externalReflectionTex:_gameReflection.reflectionTex;
                    refCamera.projectionMatrix = cam.GetStereoProjectionMatrix( Camera.StereoscopicEye.Right );

                    if ( _settings.accurateMatrix ) {
                        refCamera.projectionMatrix = refCamera.CalculateObliqueMatrix( CameraSpacePlane( refCamera, transform.position, transform.up, Camera.StereoscopicEye.Right, eyeDist ) );
                    }
                    rq.destination = _externalReflectionTex?_externalReflectionTex:_gameReflection.reflectionTex;
                    UniversalRenderPipeline.SubmitRenderRequest( refCamera, rq );

                    ;
                    var tempBuffer = RenderTexture.GetTemporary( _gameReflection.rd );

                    _settings.vrMat.SetTexture( "_RightEyeTex", _externalReflectionTex?_externalReflectionTex:_gameReflection.reflectionTex );

                    GL.invertCulling = oldCulling;

                    Graphics.Blit( tempBufferL, tempBuffer, _settings.vrMat );
                    Graphics.Blit( tempBuffer, _externalReflectionTex ? _externalReflectionTex : _gameReflection.reflectionTex );

                    RenderTexture.ReleaseTemporary( tempBuffer );
                    RenderTexture.ReleaseTemporary( tempBufferL );

                    GL.invertCulling = true;


                }
                else {

                    rq.destination = _externalReflectionTex ? _externalReflectionTex : currentData.reflectionTex;
                    refCamera.cullingMask = refCamera.cullingMask = _settings.reflectLayers;

                    if ( isSceneCam ) {
                        refCamera.cameraType = CameraType.Preview;
                    }
                    else {
                        refCamera.cameraType = CameraType.Game;
                    }

                    UniversalRenderPipeline.SubmitRenderRequest( refCamera, rq );


                }

                refCamera.targetTexture = rq.destination;
#else


                if ( Application.isPlaying && cam.cameraType == CameraType.Game && isVRCamera ) {

                    uData.allowXRRendering = false;
                    uData.renderPostProcessing = _settings.usePostFX;
                    uData.volumeLayerMask = _settings.postFXVolumeMask;

                    GL.invertCulling = true;

                    var rendTex = _externalReflectionTex ? _externalReflectionTex : _gameReflection.reflectionTex;

                    refCamera.worldToCameraMatrix = ReflectWorldToCameraMatrix( cam );

                    refCamera.targetTexture = rendTex;

                    refCamera.projectionMatrix = cam.GetStereoProjectionMatrix( Camera.StereoscopicEye.Left );

                    if ( _settings.accurateMatrix ) {
                        refCamera.projectionMatrix = refCamera.CalculateObliqueMatrix( CameraSpacePlane( refCamera, transform.position, transform.up, Camera.StereoscopicEye.Left, eyeDist ) );
                    }

                    UniversalRenderPipeline.RenderSingleCamera( context, refCamera );

                    GL.invertCulling = oldCulling;

                    var tempBufferL = RenderTexture.GetTemporary( _gameReflection.rd );

                    Graphics.Blit( rendTex, tempBufferL );

                    GL.invertCulling = true;

                    refCamera.worldToCameraMatrix = ReflectWorldToCameraMatrix( cam );

                    refCamera.targetTexture = rendTex;
                    refCamera.projectionMatrix = cam.GetStereoProjectionMatrix( Camera.StereoscopicEye.Right );

                    if ( _settings.accurateMatrix ) {
                        refCamera.projectionMatrix = refCamera.CalculateObliqueMatrix( CameraSpacePlane( refCamera, transform.position, transform.up, Camera.StereoscopicEye.Right, eyeDist ) );
                    }

                    UniversalRenderPipeline.RenderSingleCamera( context, refCamera );


                    var tempBuffer = RenderTexture.GetTemporary( _gameReflection.rd );

                    _settings.vrMat.SetTexture( "_RightEyeTex", rendTex );

                    GL.invertCulling = oldCulling;

                    Graphics.Blit( tempBufferL, tempBuffer, _settings.vrMat );
                    Graphics.Blit( tempBuffer, rendTex );
                    RenderTexture.ReleaseTemporary( tempBuffer );
                    RenderTexture.ReleaseTemporary( tempBufferL );

                    GL.invertCulling = true;

                }
                else {
                    UniversalRenderPipeline.RenderSingleCamera( context, refCamera );
                }

#endif

            }


            QualitySettings.maximumLODLevel = maxLod;
            QualitySettings.lodBias = tempLOD;



            if (
#if UNITY_EDITOR
                Application.isPlaying &&
#endif
                Time.realtimeSinceStartup > _internalTimer && _settings.reflectionFramerate > 0 ) {
                _internalTimer = Time.realtimeSinceStartup + ( 1.0f / _settings.reflectionFramerate );
            }

#if UNITY_EDITOR && !UNITY_2024_3_OR_NEWER
            if ( cam.cameraType == CameraType.SceneView && _showPreviewReflector )
                DrawReflectorMesh( cam, currentData );
#endif

            GL.invertCulling = oldCulling;
            OnReflectionRendered?.Invoke( cam, refCamera, currentData.reflectionTex );
        
        
        }


        private Vector4 CameraSpacePlane( Camera forCamera, Vector3 planeCenter, Vector3 planeNormal ) {
            Vector3 offsetPos = planeCenter;
            Matrix4x4 mtx = forCamera.worldToCameraMatrix;
            Vector3 cPos = mtx.MultiplyPoint( offsetPos );
            Vector3 cNormal = mtx.MultiplyVector( planeNormal ).normalized * 1;
            return new Vector4( cNormal.x, cNormal.y, cNormal.z, -Vector3.Dot( cPos, cNormal ) );
        }



        private Vector4 CameraSpacePlane( Camera forCamera, Vector3 planeCenter, Vector3 planeNormal, Camera.StereoscopicEye stereoEye, float eyeDistance ) {
            Vector3 offsetPos = planeCenter;
            Matrix4x4 mtx = forCamera.worldToCameraMatrix;

            mtx[12] += stereoEye == Camera.StereoscopicEye.Left ? eyeDistance : -eyeDistance;

            Vector3 cPos = mtx.MultiplyPoint( offsetPos );
            Vector3 cNormal = mtx.MultiplyVector( planeNormal ).normalized * 1;
            return new Vector4( cNormal.x, cNormal.y, cNormal.z, -Vector3.Dot( cPos, cNormal ) );
        }


#if UNITY_EDITOR
        private void DrawReflectorMesh( Camera sceneCam, ReflectionData data ) {

            if ( !sceneCam || data == null )
                return;

            var matrix = new Matrix4x4();
            matrix.SetTRS( transform.position, transform.rotation, Vector3.one * 10 );

            if ( _sceneReflectorMatBlock == null ) {
                _sceneReflectorMatBlock = new MaterialPropertyBlock();
            }

            _sceneReflectorMatBlock.SetTexture( "_ReflectionTex", data.reflectionTex ? (Texture)data.reflectionTex : (Texture)Texture2D.blackTexture );
            Graphics.DrawMesh( defaultReflectorMesh, matrix, defaultReflectorMaterial, 0, sceneCam, 0, _sceneReflectorMatBlock );

        }


        public void OnDrawGizmos() {
            Gizmos.matrix = Matrix4x4.TRS( transform.position, transform.rotation, Vector3.one );
            Gizmos.color = Color.clear;
            Gizmos.DrawCube( Vector3.zero, new Vector3( 1, 0.01f, 1 ) * 10 );
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube( Vector3.zero, new Vector3( 1, 0, 1 ) * 10 );
            Gizmos.matrix = Matrix4x4.TRS( Vector3.zero, Quaternion.identity, Vector3.one );
        }


#endif



    }

}