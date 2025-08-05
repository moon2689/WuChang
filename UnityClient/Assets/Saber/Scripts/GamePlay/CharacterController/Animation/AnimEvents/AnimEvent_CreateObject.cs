using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace Saber.CharacterController
{
    public class AnimEvent_CreateObject : AnimRangeTimeEvent
    {
        [SerializeField] private GameObject m_Prefab;
        [SerializeField] private ENodeType m_ParentBone;
        [SerializeField] private Vector3 m_LocalPosition;
        [SerializeField] private Vector3 m_LocalRotation;

        private GameObject m_GameObject;

        public override EAnimRangeEvent EventType => EAnimRangeEvent.CreateObject;

        protected override void OnRangeEventEnter()
        {
            if (!m_GameObject)
            {
                Transform parent = base.m_Actor.GetNodeTransform(m_ParentBone);
                Transform t = parent.Find(m_Prefab.name);
                if (t)
                {
                    m_GameObject = t.gameObject;
                }
                else
                {
                    m_GameObject = GameObject.Instantiate(m_Prefab);
                    m_GameObject.transform.SetParent(parent);
                    m_GameObject.name = m_Prefab.name;
                }
            }

            m_GameObject.transform.localPosition = m_LocalPosition;
            m_GameObject.transform.localRotation = Quaternion.Euler(m_LocalRotation);
            m_GameObject.SetActive(true);
        }

        protected override void OnRangeEventExit()
        {
            if (m_GameObject)
                m_GameObject.SetActive(false);
        }
    }
}