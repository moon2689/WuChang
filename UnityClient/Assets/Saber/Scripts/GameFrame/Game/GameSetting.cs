using System.Linq;
using Saber.Frame;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Saber
{
    public static class GameSetting
    {
        static UniversalRenderPipelineAsset s_URPAsset;
        private static Volume m_Volume;

        public static UniversalRenderPipelineAsset URPAsset =>
            s_URPAsset ??= (UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset;

        public static Volume GlobalVolume => m_Volume ??= GameObject.FindObjectOfType<Volume>();

        public static void Init()
        {
            Application.runInBackground = true;

            Application.targetFrameRate = 60;

            Input.multiTouchEnabled = true;

            // urp
            URPAsset.shadowDistance = GameApp.Entry.Config.GameSetting.ShadowDistance;
            URPAsset.supportsCameraOpaqueTexture = true;
            URPAsset.renderScale = 1;
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
#if UNITY_EDITOR
            renderScale = 1;
#endif
            URPAsset.renderScale = renderScale;
        }

        static void OnCamDistanceChange(float dis)
        {
            float shadowDis;
            DepthOfFieldMode depthOfFieldMode;
            if (dis < 1)
            {
                shadowDis = Mathf.Max(3, dis + 0.5f);
                depthOfFieldMode = DepthOfFieldMode.Gaussian;
            }
            else
            {
                shadowDis = 50;
                depthOfFieldMode = DepthOfFieldMode.Off;
            }

            URPAsset.shadowDistance = shadowDis;

            //Debug.Log($"cam dis:{dis},shadowDis:{shadowDis}");

            DepthOfField dof = GlobalVolume?.sharedProfile.components.FirstOrDefault(a => a is DepthOfField) as DepthOfField;
            if (dof)
            {
                dof.mode.value = depthOfFieldMode;
            }
        }

        // 横竖屏
        // public static void SetScreenOrientation(bool toLandscape)
        // {
        //     Screen.orientation = toLandscape ? ScreenOrientation.LandscapeLeft : ScreenOrientation.Portrait;
        // }
    }
}