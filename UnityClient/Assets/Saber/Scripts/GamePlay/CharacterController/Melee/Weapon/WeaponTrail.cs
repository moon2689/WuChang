using System;
using System.Collections;
using System.Collections.Generic;
using CombatEditor;
using Saber.Frame;
using UnityEngine;
using UnityEngine.Serialization;

namespace Saber.CharacterController
{
    public class WeaponTrail : MonoBehaviour
    {
        [SerializeField] private Transform m_Base;
        [SerializeField] private Transform m_Tip;
        [SerializeField] private Material m_TrailMaterial;
        [SerializeField] private int m_MaxFrame = 50;
        [SerializeField] private int m_StopMultiplier = 4;
        [Range(2, 8)] [SerializeField] private int m_TrailSubs = 2;

        DynamicTrailGenerator m_TrailGenerator;
        DynamicTrailExecutor m_Executor;

        public DynamicTrailGenerator TrailGenerator => m_TrailGenerator;
        public GameObject TrailObj { get; private set; }

        public void Show()
        {
            InitTrailMesh();
            m_Executor = m_TrailGenerator._trailMeshObj.AddComponent<DynamicTrailExecutor>();
            m_Executor.trail = m_TrailGenerator;
            m_Executor.StartTrail();
        }

        public void InitTrailMesh()
        {
            m_TrailGenerator = new DynamicTrailGenerator(m_Base, m_Tip, m_MaxFrame, m_TrailSubs, m_StopMultiplier, m_TrailMaterial, DynamicTrailGenerator.TrailBehavior.FlowUV);
            TrailObj = m_TrailGenerator.InitTrailMesh();
        }

        public void Hide()
        {
            if (m_Executor != null)
            {
                m_Executor.StopTrail();
            }
        }
    }
}