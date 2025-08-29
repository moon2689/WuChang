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
        private int m_StatueIndex;
        private IHandler m_IHandler;

        public GodStatuePoint GodStatueInfo { get; private set; }
        public int SceneID => m_SceneID;
        public int StatueIndex => m_StatueIndex;

        public Vector3 RestPos
        {
            get { return transform.position + transform.forward * 1.5f; }
        }


        public bool IsFired
        {
            get { return GameProgressManager.Instance.IsGodStatueFired(m_SceneID, m_StatueIndex); }
        }

        public void Init(int sceneID, int statueIndex, GodStatuePoint godStatueInfo, Transform parent, IHandler handler)
        {
            m_SceneID = sceneID;
            m_StatueIndex = statueIndex;
            m_IHandler = handler;
            GodStatueInfo = godStatueInfo;

            transform.parent = parent;
            transform.position = godStatueInfo.m_Position;
            transform.rotation = Quaternion.Euler(0, godStatueInfo.m_RotationY, 0);

            RefreshFire();

            godStatueInfo.IdolObject = this;
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