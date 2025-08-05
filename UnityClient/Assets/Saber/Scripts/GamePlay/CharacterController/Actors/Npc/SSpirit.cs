using System.IO;
using Saber.Director;
using Saber.Frame;
using UnityEngine;

namespace Saber
{
    public class SSpirit : MonoBehaviour
    {
        private Transform m_Target;
        private Vector3 m_OldPosOfTarget, m_FollowOffset;
        private Light m_Light;
        private float m_TimerChangeLight;

        public static SSpirit Create(string resPath)
        {
            GameObject prefab = Resources.Load<GameObject>(resPath);
            GameObject go = GameObject.Instantiate<GameObject>(prefab);
            go.name = Path.GetFileNameWithoutExtension(resPath);
            SSpirit com = go.GetComponent<SSpirit>();
            return com;
        }

        void Awake()
        {
            m_Light = GetComponentInChildren<Light>();
        }

        public void SetFollowTarget(Transform target, Vector3 offset)
        {
            m_Target = target;
            transform.position = target.position + offset + new Vector3(0, 1);
            m_FollowOffset = offset;
        }

        // Update is called once per frame
        void Update()
        {
            if (m_Target != null && m_OldPosOfTarget != m_Target.position)
            {
                m_OldPosOfTarget = m_Target.position;
                Vector3 tarPos = m_Target.position + m_Target.rotation * m_FollowOffset;
                transform.position = Vector3.Lerp(transform.position, tarPos, 0.05f);
                transform.LookAt(tarPos);
            }

            if (m_TimerChangeLight >= 0)
            {
                m_TimerChangeLight -= Time.deltaTime;
                if (m_TimerChangeLight < 0)
                {
                    m_TimerChangeLight = 3;
                    if (GameApp.Entry.Game.World != null)
                    {
                        float intensity = 0.6f - GameApp.Entry.Game.World.MainLight.intensity;
                        intensity = Mathf.Max(0.2f, intensity);
                        if (intensity > 0)
                            m_Light.intensity = intensity;
                    }
                }
            }
        }
    }
}