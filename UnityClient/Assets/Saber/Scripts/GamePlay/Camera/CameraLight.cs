using Saber.Frame;
using UnityEngine;

namespace Saber
{
    [RequireComponent(typeof(Light))]
    public class CameraLight : MonoBehaviour
    {
        private Light m_Light;
        private float m_TimerChangeLight;

        private Light MainLight => GameApp.Entry.Game.World != null ? GameApp.Entry.Game.World.MainLight : null;

        private void Awake()
        {
            gameObject.SetActive(GameApp.Entry.Config.GameSetting.OpenCameraLight);
            m_Light = GetComponent<Light>();
        }

        void Update()
        {
            m_TimerChangeLight -= Time.deltaTime;
            if (m_TimerChangeLight < 0)
            {
                m_TimerChangeLight = 3;
                if (MainLight != null)
                    m_Light.intensity = Mathf.Lerp(0.05f, 0.8f, (MainLight.intensity - 0.6f) / 1.5f);
                else
                    m_Light.intensity = 0.5f;
            }
        }
    }
}