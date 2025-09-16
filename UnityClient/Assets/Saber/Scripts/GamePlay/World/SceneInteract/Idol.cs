using System;
using Saber.CharacterController;
using Saber.Config;
using Saber.Frame;
using Saber.UI;
using UnityEngine;

namespace Saber.World
{
    public class Idol : SceneInteractObjectBase
    {
        public interface IHandler
        {
            void OnPlayerEnter(Idol idol);
            void OnPlayerExit(Idol idol);
            void OnPlayerWorship(Idol idol);
            Coroutine OnPlayerRest(Idol idol);
        }

        [SerializeField] private GameObject m_FireObject;

        private int m_SceneID;
        private IHandler m_IHandler;

        public int SceneID => m_SceneID;
        public int ID => Point.m_ID;
        public ScenePoint Point { get; private set; }

        public bool IsFired => GameApp.Entry.Game.ProgressMgr.IsIdolFired(m_SceneID, ID);


        public void Init(int sceneID, ScenePoint scenePoint, Transform parent, IHandler handler)
        {
            m_SceneID = sceneID;
            Point = scenePoint;
            m_IHandler = handler;

            transform.SetParent(parent);
            transform.position = scenePoint.transform.position;
            transform.rotation = scenePoint.transform.rotation;

            RefreshFire();

            scenePoint.IdolObj = this;
        }

        public void RefreshFire()
        {
            m_FireObject.SetActive(IsFired);
        }

        protected override void OnPlayerEnter()
        {
            m_IHandler.OnPlayerEnter(this);
        }

        protected override void OnPlayerExit()
        {
            m_IHandler.OnPlayerExit(this);
        }

        public void Active()
        {
            m_IHandler.OnPlayerWorship(this);
        }

        public Coroutine Rest()
        {
            return m_IHandler.OnPlayerRest(this);
        }
    }
}