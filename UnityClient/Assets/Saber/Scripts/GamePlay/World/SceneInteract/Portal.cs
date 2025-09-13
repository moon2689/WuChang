using System;
using System.Collections;
using System.Collections.Generic;
using Saber.Config;
using UnityEngine;

namespace Saber.World
{
    public class Portal : SceneInteractObjectBase
    {
        public interface IHandler
        {
            void OnPlayerEnter(Portal portal);
            void OnPlayerExit(Portal portal);
            void OnPlayerTransmit(Portal portal);
        }

        private IHandler m_IHandler;

        public ScenePoint Point { get; private set; }
        public int TargetSceneID => Point.m_TargetSceneID;
        public int TargetPortalID => Point.m_TargetPortalID;


        public void Init(ScenePoint point, Transform parent, IHandler handler)
        {
            m_IHandler = handler;
            Point = point;
            transform.SetParent(parent);
            transform.position = point.transform.position;
            transform.rotation = point.transform.rotation;
            point.PortalObj = this;
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
            m_IHandler.OnPlayerTransmit(this);
        }
    }
}