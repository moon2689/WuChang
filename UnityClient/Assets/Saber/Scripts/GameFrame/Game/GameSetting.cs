using System.Linq;
using System.Net;
using Saber.Frame;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Saber
{
    public static class GameSetting
    {
        private static UniversalRenderPipelineAsset s_URPAsset;
        private static Volume m_Volume;


        public static UniversalRenderPipelineAsset URPAsset =>
            s_URPAsset ??= (UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset;

        public static Volume GlobalVolume => m_Volume ??= GameObject.FindObjectOfType<Volume>();

        public static bool ActiveDepthOfField
        {
            set
            {
                DepthOfField dof =
                    GlobalVolume.sharedProfile.components.FirstOrDefault(a => a is DepthOfField) as DepthOfField;
                if (dof)
                    dof.active = value;
            }
        }

        public static bool ActiveVignette
        {
            set
            {
                Vignette v = GlobalVolume.sharedProfile.components.FirstOrDefault(a => a is Vignette) as Vignette;
                if (v)
                    v.active = value;
            }
        }


        public static void Init()
        {
            Application.runInBackground = true;
#if UNITY_EDITOR
            Application.targetFrameRate = 60;
#else
            Application.targetFrameRate = 30;
#endif

            Input.multiTouchEnabled = true;

            // urp
            URPAsset.shadowDistance = GameApp.Entry.Config.GameSetting.ShadowDistance;
            URPAsset.supportsCameraOpaqueTexture = true;
            //URPAsset.renderScale = 1;
            // URPAsset.supportsCameraDepthTexture = true;

            // camera
            PlayerCamera.Instance.OnDistanceChange += OnCamDistanceChange;

            // render scale
            ResetResolution(1);
        }

        static void ResetResolution(float ratio)
        {
            float minRatio = ratio * 0.5f;
            float maxRatio = ratio;
            float renderScale = (1920 * 1080 * ratio) / (Screen.width * Screen.height);
            renderScale = Mathf.Clamp(renderScale, minRatio, maxRatio);
            renderScale = Mathf.Clamp01(renderScale);
            Debug.Log($"Set render scale:{renderScale}");
            URPAsset.renderScale = renderScale;
        }

        static void OnCamDistanceChange(float dis)
        {
            bool isClose = dis < 1;
            float shadowDis = isClose ? Mathf.Max(3, dis + 0.5f) : GameApp.Entry.Config.GameSetting.ShadowDistance;
            URPAsset.shadowDistance = shadowDis;
        }

        // 横竖屏
        // public static void SetScreenOrientation(bool toLandscape)
        // {
        //     Screen.orientation = toLandscape ? ScreenOrientation.LandscapeLeft : ScreenOrientation.Portrait;
        // }
    }
}