
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
    using UnityEditor;

    [CustomEditor( typeof( PlanarReflectionRenderer ) )]
    public class PlanarReflectionRenderer_Editor : Editor {


        public GUISkin pidiSkin2;

        public Texture2D reflectionsLogo;

        protected int _currentTab = 0;

        bool[] _folds = new bool[16];

        public override void OnInspectorGUI() {

            GUI.color = EditorGUIUtility.isProSkin ? new Color( 0.1f, 0.1f, 0.15f, 1 ) : new Color( 0.5f, 0.5f, 0.6f );
            GUILayout.BeginVertical( EditorStyles.helpBox );
            GUI.color = Color.white;

            GUILayout.Space( 8 );

            AssetLogoAndVersion();

            GUILayout.Space( 4 );

            GUILayout.BeginHorizontal();

            GUILayout.Space( 16 );

            if ( GUILayout.Button( "General Settings", _currentTab == 0 ? pidiSkin2.customStyles[6] : pidiSkin2.customStyles[5], GUILayout.MaxWidth( 240 ) ) ) {
                _currentTab = 0;
            }

            GUILayout.Space( 12 );

            if ( GUILayout.Button( "Performance", _currentTab == 1 ? pidiSkin2.customStyles[6] : pidiSkin2.customStyles[5], GUILayout.MaxWidth( 240 ) ) ) {
                _currentTab = 1;
            }

            GUILayout.Space( 12 );

            if ( GUILayout.Button( "Post FX Settings", _currentTab == 2 ? pidiSkin2.customStyles[6] : pidiSkin2.customStyles[5], GUILayout.MaxWidth( 240 ) ) ) {
                _currentTab = 2;
            }


            GUILayout.Space( 12 );

            if ( GUILayout.Button( "?", _currentTab == 3 ? pidiSkin2.customStyles[6] : pidiSkin2.customStyles[5], GUILayout.MaxWidth( 24 ) ) ) {
                _currentTab = 3;
            }


            GUILayout.Space( 16 );
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space( 16 );

            GUILayout.BeginVertical();

            if ( _currentTab == 0 ) {

                GUILayout.Space( 16 );

                if ( UnityEngine.XR.XRSettings.enabled ) {

                    EditorGUILayout.HelpBox( "This project is using VR cameras. Planar Reflections will now be rendered in VR-Stereoscopic Mode, which is still EXPERIMENTAL." +
                        "\n\nCompatibility is not guaranteed and the reflections will only work with Single-Pass Instanced mode, so please use with caution.", MessageType.Warning );
                    
                    GUILayout.Space( 16 );
                }

                CenteredLabel( "Basic Properties & Features" );

                GUILayout.Space( 16 );

                if ( serializedObject.FindProperty("_settings.cameraFilteringMode").enumValueIndex == (int)PlanarReflectionSettings.CameraFilteringMode.ByComponentOnly ) {
                    EditorGUILayout.HelpBox( "Only cameras with the Planar Reflection Camera component attached will trigger reflections from this Reflection Renderer", MessageType.Info );                    
                    GUILayout.Space( 16 );
                }

                EditorGUILayout.PropertyField( serializedObject.FindProperty( "showAdvancedSettings" ) );

                EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.reflectLayers" ), new GUIContent( "Reflect Layers" ) );

                EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.reflectionBackground" ) );

                EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.cameraFilteringMode" ) );


                if ( serializedObject.FindProperty( "_settings.reflectionBackground" ).enumValueIndex == (int)PlanarReflectionSettings.ReflectionBackground.SolidColor ) {
                    EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.backgroundColor" ), new GUIContent( "Background Color", "The color that this reflection will use as a solid background instead of the skybox" ) );
                }

                GUILayout.Space( 12 );

                EditorGUILayout.PropertyField( serializedObject.FindProperty( "_externalReflectionTex" ), new GUIContent("Ext. Reflection Texture", "An optional RenderTexture asset to which the reflection will be rendered to, instead of the internally managed resources. May not work accurately with multiple in-game cameras" ), GUILayout.Height( EditorGUIUtility.singleLineHeight ) );

                if ( serializedObject.FindProperty( "_settings.renderDepth" ).boolValue ) {
                    EditorGUILayout.PropertyField( serializedObject.FindProperty( "_externalReflectionDepth" ), new GUIContent( "Ext. Depth Texture", "An optional RenderTexture asset to which the reflection's depth will be rendered to, instead of the internally managed resources. May not work accurately with multiple in-game cameras. Must be in 'Depth' texture format." ), GUILayout.Height( EditorGUIUtility.singleLineHeight ) );
                }

                GUILayout.Space( 8 );

                //
                Toggle( "Reflection Depth", serializedObject, "_settings.renderDepth", 1 );

               
                GUILayout.Space( 16 );

                if ( serializedObject.FindProperty( "showAdvancedSettings" ).boolValue ) {

                    CenteredLabel( "Advanced Settings" );

                    GUILayout.Space( 16 );

                    EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.reflectionURPRendererIndex" ), new GUIContent( "Renderer Override", "Assigns a custom Renderer index to the reflections, different from that of the main camera. Please make sure that your index is not larger than the amount of actual renderers in your URP asset" ) );
                    
                    GUILayout.Space( 8 );

                    EditorGUILayout.PropertyField( serializedObject.FindProperty( "_previewInSceneView" ) );
                    EditorGUILayout.PropertyField( serializedObject.FindProperty( "_showPreviewReflector" ) );

                    if ( serializedObject.FindProperty( "_showPreviewReflector" ).boolValue ) {
                        serializedObject.FindProperty( "_previewInSceneView" ).boolValue = true;
                    }

                    GUILayout.Space( 4 );

                    EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.accurateMatrix" ) );

                    EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.nearClipPlane" ), new GUIContent( "Near Clip Plane" ) );
                    EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.farClipPlane" ), new GUIContent( "Far Clip Plane" ) );

                    GUILayout.Space( 12 );

                    if ( serializedObject.FindProperty( "_settings.renderFog" ).boolValue ) {
                        GUILayout.Space( 8 );

                        EditorGUILayout.HelpBox( "Fog Rendering is no longer needed in most cases for Unity 2022+ with URP as fog is rendered in the reflection by default.", MessageType.Info );

                        GUILayout.Space( 8 );
                    }

                    Toggle( "Reflection Fog", serializedObject, "_settings.renderFog", 1 );

                    if ( serializedObject.FindProperty( "_settings.renderFog" ).boolValue ) {
                        EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.fogRendererIndex" ), new GUIContent( "Fog Renderer Index" ) );
                    }


                    GUILayout.Space( 16 );
                }


            }


            if ( _currentTab == 1 ) {

                GUILayout.Space( 16 );

                CenteredLabel( "General Performance Settings" );

                GUILayout.Space( 16 );

                EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.updateOnCastOnly" ) );
                EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.renderShadows" ) );

                GUILayout.Space( 8 );

                EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.customLODBias" ) );
                EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.maxLODLevel" ) );

                GUILayout.Space( 8 );

                EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.reflectionFramerate" ) );

                GUILayout.Space( 16 );

                CenteredLabel( "Reflection Output Quality" );


                GUILayout.Space( 16 );


                Toggle( "Screen Based Resolution", serializedObject, "_settings.screenBasedResolution", 1 );

                if ( !serializedObject.FindProperty( "_settings.screenBasedResolution" ).boolValue ) {
                    EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.explicitResolution" ) );
                }

                EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.outputResolutionMultiplier" ) );

                GUILayout.Space( 4 );

                Toggle( "HDR Reflection", serializedObject, "_settings.forceHDR", 1 );
                Toggle( "Mip Maps", serializedObject, "_settings.useMipMaps", 1 );
                Toggle( "Anti-aliasing", serializedObject, "_settings.useAntialiasing", 1 );

                GUILayout.Space( 16 );
            }

            if ( _currentTab == 2 ) {

                GUILayout.Space( 16 );

                EditorGUILayout.HelpBox( "\nEnabling Post FX in the reflections is heavily discouraged, as it will often produce unexpected results and heavily impact performance. The reflections will automatically reflect most effects, including SSAO and bloom (as long as the reflection is marked as HDR enabled in the Performance tab).\n", MessageType.Warning );

                GUILayout.Space( 12 );

                Toggle( "PostFX Support", serializedObject, "_settings.usePostFX", 1 );

                if ( serializedObject.FindProperty( "_settings.usePostFX" ).boolValue ) {
                    GUILayout.Space( 8 );
                    EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.postFXVolumeMask" ) );
                }
                GUILayout.Space( 16 );

            }



            if ( _currentTab == 3 ) {

                GUILayout.Space( 16 );

                CenteredLabel( "Support & Assistance" );
                GUILayout.Space( 10 );

                EditorGUILayout.HelpBox( "Please make sure to include the following information with your request :\n - Invoice number\n- Unity version used\n- Universal RP version used (if any)\n- Target platform\n - Screenshots of the PlanarReflectionRenderer component and its settings\n - Steps to reproduce the issue.\n\nOur support service usually takes 2-4 business days to reply, so please be patient. We always reply to all emails and support requests as soon as possible.", MessageType.Info );

                GUILayout.Space( 8 );
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label( "For support, contact us at : support@irreverent-software.com" );
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Space( 24 );

                if ( CenteredButton( "Online Documentation", 500 ) ) {
                    Help.BrowseURL( "https://irreverent-software.com/docs/planar-reflections-5/" );
                }
                GUILayout.Space( 16 );

            }

            GUILayout.Space( 8 );


            GUILayout.EndVertical();
            GUILayout.Space( 16 );

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            var lStyle = new GUIStyle();
            lStyle.fontStyle = FontStyle.Italic;
            lStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            lStyle.fontSize = 8;

            GUILayout.Label( $"Copyright© 2017-{System.DateTime.Today.Year}, Jorge Pinal N.", lStyle );

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space( 24 );

            GUILayout.EndVertical();


            if ( serializedObject.hasModifiedProperties ) {
                ( (PlanarReflectionRenderer)target ).ApplySettings();
            }

            serializedObject.ApplyModifiedProperties();

        }



        private void AssetLogoAndVersion() {

            GUILayout.BeginVertical( reflectionsLogo, pidiSkin2 ? pidiSkin2.customStyles[1] : null );
            GUILayout.Space( 45 );
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label( (target as PlanarReflectionRenderer).Version, pidiSkin2.customStyles[2] );
            GUILayout.Space( 6 );
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }


        void CenteredLabel( string label ) {


            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            var tempStyle = new GUIStyle();
            tempStyle.fontStyle = FontStyle.Bold;
            tempStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

            GUILayout.Label( label, tempStyle );

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

        }


        bool CenteredButton( string label, float width = 400 ) {

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var btn = GUILayout.Button( label, GUILayout.MaxWidth( width ) );
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            return btn;
        }

        private bool BeginCenteredGroup( string label, ref bool groupFoldState ) {

            if ( GUILayout.Button( label, groupFoldState ? pidiSkin2.customStyles[6] : pidiSkin2.button ) ) {
                groupFoldState = !groupFoldState;
            }
            GUILayout.BeginHorizontal();
            GUILayout.Space( 12 );
            GUILayout.BeginVertical();
            return groupFoldState;
        }


        private void EndCenteredGroup() {
            GUILayout.EndVertical();
            GUILayout.Space( 12 );
            GUILayout.EndHorizontal();
            GUILayout.Space( 4 );
        }




        public static void PopupField( GUIContent label, SerializedObject serializedObject, string propertyID, string[] options ) {


            GUILayout.BeginHorizontal();


            var tempStyle = new GUIStyle();
            EditorGUILayout.PrefixLabel( label );


            var inValue = serializedObject.FindProperty( propertyID );

            if ( inValue.hasMultipleDifferentValues ) {
                var result = EditorGUILayout.Popup( -1, options );

                if ( result > -1 ) {
                    inValue.intValue = result;
                }
            }
            else {
                inValue.intValue = EditorGUILayout.Popup( inValue.intValue, options );
            }

            GUILayout.EndHorizontal();

        }



        private static void Toggle( string label, SerializedObject serializedObject, string propertyID, int toggleType = 0 ) {


            GUILayout.BeginHorizontal();

            var inValue = serializedObject.FindProperty( propertyID );


            GUIContent trueLabel = new GUIContent( label, inValue.tooltip );

            switch ( toggleType ) {

                case 0:
                    EditorGUILayout.PropertyField( inValue, trueLabel );
                    break;

                case 1:
                    if ( inValue.hasMultipleDifferentValues ) {
                        var result = EditorGUILayout.Popup( trueLabel, -1, new string[] { "Enabled", "Disabled" } );

                        if ( result > -1 ) {
                            inValue.boolValue = result == 0;
                        }
                    }
                    else {
                        inValue.boolValue = EditorGUILayout.Popup( trueLabel, inValue.boolValue ? 0 : 1, new string[] { "Enabled", "Disabled" } ) == 0;
                    }
                    break;

                case 2:
                    if ( inValue.hasMultipleDifferentValues ) {
                        var result = EditorGUILayout.Popup( trueLabel, -1, new string[] { "True", "False" } );

                        if ( result > -1 ) {
                            inValue.boolValue = result == 0;
                        }
                    }
                    else {
                        inValue.boolValue = EditorGUILayout.Popup( trueLabel, inValue.boolValue ? 0 : 1, new string[] { "True", "False" } ) == 0;
                    }
                    break;

            }

            GUILayout.EndHorizontal();
        }



    }

}