using System;
using Saber.CharacterController;
using Saber.Config;
using Saber.Frame;
using Saber.UI;
using UnityEngine;

namespace Saber.World
{
    public class ShenKan : SceneInteractObjectBase
    {
        public interface IHandler
        {
            void OnPlayerEnter(ShenKan shenKan);
            void OnPlayerExit(ShenKan shenKan);
            void OnPlayerActiveFire(ShenKan shenKan);
            Coroutine OnPlayerRest(ShenKan shenKan);
        }

        [SerializeField] private GameObject m_FireObject;

        private int m_SceneID;
        private IHandler m_IHandler;

        public int SceneID => m_SceneID;
        public int ID => Point.m_ID;
        public ScenePointShenKan Point { get; private set; }

        public bool IsActived => GameApp.Entry.Game.ProgressMgr.IsShenKanActived(m_SceneID, ID);


        public void Init(int sceneID, ScenePoint scenePoint, Transform parent, IHandler handler)
        {
            m_SceneID = sceneID;
            Point = (ScenePointShenKan)scenePoint;
            m_IHandler = handler;

            transform.SetParent(parent);
            transform.position = scenePoint.transform.position;
            transform.rotation = scenePoint.transform.rotation;

            RefreshFire();
        }

        public void RefreshFire()
        {
            m_FireObject.SetActive(IsActived);
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
            m_IHandler.OnPlayerActiveFire(this);
        }

        public Coroutine Rest()
        {
            return m_IHandler.OnPlayerRest(this);
        }
    }
}