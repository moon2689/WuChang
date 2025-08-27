using System.Collections;
using UnityEngine;

namespace Saber
{
    public class FPSCounter : MonoBehaviour
    {
        [SerializeField] private float m_UpdateInterval = 0.5f;
        private float m_FPS;
        private GUIStyle m_LabelStyle;

        public float FPS => m_FPS;
        public int FPSInt => Mathf.FloorToInt(m_FPS);


        public static void Create()
        {
            GameObject go = new(nameof(FPSCounter));
            go.AddComponent<FPSCounter>();
        }

        void Awake()
        {
            m_LabelStyle = new()
            {
                fontSize = 40,
                normal =
                {
                    textColor = Color.yellow
                }
            };

            StartCoroutine(UpdateCounter());

            DontDestroyOnLoad(gameObject);
        }

        IEnumerator UpdateCounter()
        {
            while (true)
            {
                var previousUpdateTime = Time.unscaledTime;
                var previousUpdateFrames = Time.frameCount;

                while (Time.unscaledTime < previousUpdateTime + m_UpdateInterval)
                {
                    yield return null;
                }

                var timeElapsed = Time.unscaledTime - previousUpdateTime;
                var framesChanged = Time.frameCount - previousUpdateFrames;

                m_FPS = framesChanged / timeElapsed;
            }
        }

        private void OnGUI()
        {
            GUI.Label(new(0, 0, 300, 10), $"帧率：{FPSInt}", m_LabelStyle);
        }
    }
}