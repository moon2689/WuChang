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
    public class PlanarReflectionSettings {

        /// <summary>
        /// Enum listing the different ways in which the background of the reflection can be cleared
        /// </summary>
        public enum ReflectionBackground { CopyFromCamera, Skybox, SolidColor, Transparent }

        /// <summary>
        /// Enum that lists the ways in which the cameras can be filtered when deciding whether they should trigger reflections or not
        /// </summary>
        public enum CameraFilteringMode { AllCameras, ByComponentOnly, ByPrefix }

        /// <summary>
        /// The way in which the background of the reflection will be cleared
        /// </summary>
        [Tooltip( "The way in which the background of the reflection will be cleared" )]
        public ReflectionBackground reflectionBackground = ReflectionBackground.CopyFromCamera;

        /// <summary>
        /// The way in which the cameras can be filtered when deciding whether they should trigger reflections or not
        /// </summary>
        [Tooltip( "The way in which the cameras can be filtered when deciding whether they should trigger reflections or not" )] 
        public CameraFilteringMode cameraFilteringMode = CameraFilteringMode.AllCameras;


        /// <summary>
        /// The near clip plane of the reflection
        /// </summary>
        [Tooltip( "The near clip plane of the reflection" )] 
        public float nearClipPlane = 0.03f;

        /// <summary>
        /// The far clip plane of the reflection
        /// </summary>
        [Tooltip( "The far clip plane of the reflection")] 
        public float farClipPlane = 1000;

        /// <summary>
        /// The layers that can be reflected by this reflection
        /// </summary>
        [Tooltip("The layers that can be reflected by this reflection")]
        public LayerMask reflectLayers = 1;

        /// <summary>
        /// Assigns a custom Renderer index to the reflections, different from that of the main camera. Please make sure that your index is not larger than the amount of actual renderers in your URP asset
        /// </summary>
        [Tooltip( "Assigns a custom Renderer index to the reflections, different from that of the main camera. Please make sure that your index is not larger than the amount of actual renderers in your URP asset" )]
        [Min( 0 )]
        public int reflectionURPRendererIndex = 0;

        /// <summary>
        /// Custom LOD bias to be used within the reflection
        /// </summary>
        [Tooltip( "Custom LOD bias to be used within the reflection" )]
        [Range( 0, 1 )] 
        public float customLODBias = 1.0f;

        /// <summary>
        /// The maximum LOD level to be rendered by the reflection. Useful when working with high poly count scenes.
        /// </summary>
        [Tooltip( "The maximum LOD level to be rendered by the reflection. Useful when working with high poly count scenes." )]
        public int maxLODLevel;

        /// <summary>
        /// Whether this reflection will render shadows or not
        /// </summary>
        [Tooltip("Whether this reflection will render shadows or not")]
        public bool renderShadows = true;

        /// <summary>
        /// DO NOT ENABLE. It is unnecessary in most cases. It defines whether this reflection will use a custom Post FX volume mask and rendering workflow or not.
        /// </summary>
        [Tooltip( "DO NOT ENABLE. It is unnecessary in most cases. It defines whether this reflection will use a custom Post FX volume mask and rendering workflow or not." )]
        public bool usePostFX = false;

        /// <summary>
        /// If PostFX use is enabled, this provides the custom mask to track the PostFX volumes
        /// </summary>
        [Tooltip("If PostFX use is enabled, this provides the custom mask to track the PostFX volumes")]
        public LayerMask postFXVolumeMask;

        /// <summary>
        /// Whether the reflection will use an accurate (oblique) projection matrix or not. It is recommended to leave this setting enabled
        /// </summary>
        [Tooltip("Whether the reflection will use an accurate (oblique) projection matrix or not. It is recommended to leave this setting enabled")]
        [InspectorName("Use Oblique matrix")]
        public bool accurateMatrix = true;

        /// <summary>
        /// If the filtering mode is set to ByPrefix, the prefix that the reflection system will look for to decide whether a camera requires reflections or not.
        /// </summary>
        [Tooltip( "If the filtering mode is set to ByPrefix, the prefix that the reflection system will look for to decide whether a camera requires reflections or not." )]
        public string camerasPrefix;

        /// <summary>
        /// Whether the resolution of the reflection texture will be calculated as a factor of the screen's actual resolution or not.
        /// </summary>
        [Tooltip( "Whether the resolution of the reflection texture will be calculated as a factor of the screen's actual resolution or not." )]
        public bool screenBasedResolution = true;

        /// <summary>
        /// When specified, the explicit resolution that will be used to render the reflection texture.
        /// </summary>
        [Tooltip( "When specified, the explicit resolution that will be used to render the reflection texture." )]
        public Vector2 explicitResolution = new Vector2( 512, 512 );

        /// <summary>
        /// A factor that the reflection's resolution will be multiplied by. It is useful to downscale or supersample the reflection's resolution at will.
        /// </summary>
        [Tooltip( "A factor that the reflection's resolution will be multiplied by. It is useful to downscale or supersample the reflection's resolution at will." )]
        [Range( 0.1f, 2.0f )] 
        public float outputResolutionMultiplier = 1.0f;

        /// <summary>
        /// A custom framerate that the reflection will be rendered at. By default it is set to 0, meaning it will be updated on every frame.
        /// </summary>
        [Tooltip( "A custom framerate that the reflection will be rendered at. By default it is set to 0, meaning it will be updated on every frame." )]
        [Range( 0, 120 )] 
        public int reflectionFramerate = 0;

        /// <summary>
        /// Whether the reflection texture will generate mip-maps or not. It is recommended for PBR-Like reflections, as it is used internally by the Shader-based blur pass.
        /// </summary>
        [Tooltip( "Whether the reflection texture will generate mip-maps or not. It is recommended for PBR-Like reflections, as it is used internally by the Shader-based blur pass." )]
        public bool useMipMaps = true;

        /// <summary>
        /// Whether the reflection texture will use anti-aliasing or not. It is recommended to disable this unless the resolution of the reflections is too low.
        /// </summary>
        [Tooltip( "Whether the reflection texture will use anti-aliasing or not. It is recommended to disable this unless the resolution of the reflections is too low." )]
        public bool useAntialiasing = false;

        /// <summary>
        /// Whether the reflection will clear its background to a solid color or not. Legacy.
        /// </summary>
        public bool clearToColor = false;

        /// <summary>
        /// The color that the reflection will clear its background to.
        /// </summary>
        [Tooltip( "The color that the reflection will clear its background to.")]
        public Color backgroundColor = Color.blue;

        /// <summary>
        /// Whether this reflection requires a Depth pass or not.
        /// </summary>
        [Tooltip( "Whether this reflection requires a Depth pass or not." )]
        public bool renderDepth = false;

        /// <summary>
        /// OBSOLETE. Whether this reflection requires a Fog Pass or not.
        /// </summary>
        [Tooltip( "OBSOLETE. Whether this reflection requires a Fog Pass or not." )]
        public bool renderFog;

        /// <summary>
        /// The custom URP Renederer index that contains the Fog Pass
        /// </summary>
        [Tooltip( "The custom URP Renederer index that contains the Fog Pass" )]
        [Min(0)]
        public int fogRendererIndex = 1;

        /// <summary>
        /// Whether the reflection texture will be an HDR-ready texture or not. It is recommended to enable this if you are using Bloom or other HDR-dependant effects.
        /// </summary>
        [Tooltip( "Whether the reflection texture will be an HDR-ready texture or not. It is recommended to enable this if you are using Bloom or other HDR-dependant effects." )]
        public bool forceHDR = true;

        /// <summary>
        /// If enabled, the reflection will only be updated when a caster connected to this renderer is actually visible by any camera. It helps greatly to save performance.
        /// </summary>
        [Tooltip( "If enabled, the reflection will only be updated when a caster connected to this renderer is actually visible by any camera. It helps greatly to save performance." )]
        public bool updateOnCastOnly = true;

        /// <summary>
        /// Allows the reflections to render world-space canvas elements, but in turn disables some other features such as VR support
        /// </summary>
        [Tooltip("Allows the reflections to render world-space canvas elements, but in turn disables some other features such as VR support")]
        public bool worldCanvasCompatibilityMode = false;


        /// <summary>
        /// INTERNAL. The material used to combine / compose the VR-ready reflections
        /// </summary>
        public Material vrMat;


    }
}