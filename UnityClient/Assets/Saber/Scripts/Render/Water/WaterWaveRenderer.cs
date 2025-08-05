using System;
using System.Collections;
using System.Collections.Generic;
using Saber.Frame;
using UnityEngine;

namespace Saber
{
    public class WaterWaveRenderer : MonoBehaviour
    {
        private static int s_IDWaveRTRect = Shader.PropertyToID("_WaveRTRect");
        private static WaterWaveRenderer s_Instance;

        [SerializeField] private Transform m_FollowTarget;
        [SerializeField] private Transform m_CameraRoot;
        [SerializeField] private Camera m_Camera;
        [SerializeField] private MeshRenderer m_MRWater;
        [SerializeField] private BoxCollider m_ColliderWater;
        [SerializeField] private RenderTexture m_WaveRT;

        private float m_RTWidthMetre;
        private float m_RTHeightMetre;
        private float m_WaterWidth;
        private float m_WaterHeight;
        private Vector4 m_RectWaveRT;


        public static void SetActiveCamera(bool active)
        {
            if (s_Instance)
                s_Instance.m_CameraRoot.gameObject.SetActive(active);
        }


        void Awake()
        {
            s_Instance = this;

            m_Camera.targetTexture = m_WaveRT;
            m_ColliderWater.isTrigger = true;

            m_MRWater.material.SetTexture("_WaveRT", m_WaveRT);

            m_CameraRoot.transform.parent = null;
            m_CameraRoot.transform.localScale = Vector3.one;
            m_CameraRoot.rotation = Quaternion.identity;

            m_WaterWidth = m_ColliderWater.size.x * m_ColliderWater.transform.lossyScale.x;
            m_WaterHeight = m_ColliderWater.size.z * m_ColliderWater.transform.lossyScale.z;

            m_RTHeightMetre = m_Camera.orthographicSize * 2f;
            m_RTWidthMetre = m_RTHeightMetre;
            float uvHeight = m_RTHeightMetre / m_WaterHeight;
            float uvWidth = m_RTWidthMetre / m_WaterWidth;

            m_RectWaveRT = new Vector4(0, 0, uvWidth, uvHeight);
        }

        void Update()
        {
            if (m_CameraRoot.gameObject.activeSelf)
            {
                if (m_FollowTarget != null)
                {
                    m_CameraRoot.position = m_FollowTarget.position;
                }
                else if (GameApp.Entry.Game.Player)
                {
                    m_FollowTarget = GameApp.Entry.Game.Player.transform;
                }

                UpdateWaveRect();
            }
        }

        void UpdateWaveRect()
        {
            m_RectWaveRT.x = 0.5f - (m_Camera.transform.position.x - transform.position.x + m_RTWidthMetre / 2) /
                m_WaterWidth;
            m_RectWaveRT.y = 0.5f - (m_Camera.transform.position.z - transform.position.z + m_RTHeightMetre / 2) /
                m_WaterHeight;
            m_MRWater.material.SetVector(s_IDWaveRTRect, m_RectWaveRT);
        }

        private void OnDestroy()
        {
            s_Instance = null;
        }
    }
}