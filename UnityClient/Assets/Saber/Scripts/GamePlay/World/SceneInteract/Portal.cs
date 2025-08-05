using System;
using System.Collections;
using System.Collections.Generic;
using Saber.Config;
using UnityEngine;

namespace Saber.World
{
    public class Portal : SceneInteractObjectBase
    {
        [SerializeField] private Collider[] m_GateCollider;

        public interface IHandler
        {
            void OnPlayerEnter(Portal portal);
            void OnPlayerExit(Portal portal);
            void OnPlayerTransmit(Portal portal);
        }

        private IHandler m_IHandler;

        public PortalPoint PortalInfo { get; private set; }


        public void Init(PortalPoint portalInfo, Transform parent, IHandler handler)
        {
            m_IHandler = handler;
            PortalInfo = portalInfo;
            transform.parent = parent;
            transform.position = portalInfo.m_Position;
            transform.rotation = Quaternion.Euler(0, portalInfo.m_RotationY, 0);
            portalInfo.PortalObject = this;
        }

        protected override void OnPlayerEnter()
        {
            m_IHandler.OnPlayerEnter(this);
        }

        protected override void OnPlayerExit()
        {
            m_IHandler.OnPlayerExit(this);
        }

        public void Transmit()
        {
            m_IHandler.OnPlayerExit(this);
            m_IHandler.OnPlayerTransmit(this);
        }

        public void EnableGateCollider(bool enable)
        {
            foreach (var c in m_GateCollider)
                c.enabled = enable;
        }
    }
}