using System;
using System.Collections.Generic;
using System.Linq;
using Saber.Config;
using Saber.Frame;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


namespace Saber.UI
{
    public class Wnd_Rest : WndBase
    {
        public interface IHandler : IWndHandler
        {
            void OnClickQuit();
            void OnClickTransmit(int sceneID, int statueIndex);
            void OnSelectEnemy(int actorID);
        }

        [SerializeField] private Button m_BtnQuit;
        [SerializeField] private Button m_BtnTransmit;
        [SerializeField] private Button m_BtnSelectEnemy;

        [SerializeField] private GameObject m_TempTextItem;

        [SerializeField] private GameObject m_RootStatues;
        [SerializeField] private GameObject m_RootEnemyNames;


        private IHandler m_Handler;


        protected override bool PauseGame => false;

        protected override void OnAwake()
        {
            base.OnAwake();
            m_Handler = base.m_WndHandler as IHandler;

            m_BtnQuit.onClick.AddListener(OnClickQuit);
            m_BtnTransmit.onClick.AddListener(OnClickTransmit);
            m_BtnSelectEnemy.onClick.AddListener(OnClickSelectEnemy);

            GameApp.Entry.Game.Audio.Play2DSound("Sound/UI/ActorInfoWndOpen");

            InitStatueTransmit();
            InitEnemies();
        }

        void InitStatueTransmit()
        {
            int count = 0;
            foreach (var s in GameProgressManager.Instance.SceneProgressDatas)
            {
                var sceneInfo = GameApp.Entry.Config.SceneInfo.GetSceneInfoByID(s.m_SceneID);
                foreach (var statueIndex in s.m_FiredGodStatues)
                {
                    ++count;
                    var statueInfo = sceneInfo.m_GodStatuePoint[statueIndex];
                    GameObject item = GameObject.Instantiate(m_TempTextItem);
                    item.SetActive(true);
                    item.name = item.ToString();
                    item.transform.SetParent(m_RootStatues.transform);
                    item.GetComponentInChildren<Text>().text = $"{sceneInfo.m_Name} {statueInfo.m_Name}";
                    item.GetComponentInChildren<Button>().onClick.AddListener(() =>
                    {
                        m_Handler.OnClickTransmit(sceneInfo.m_ID, statueIndex);
                        Destroy();
                        GameApp.Entry.Game.Audio.PlayCommonClick();
                    });
                }
            }

            m_RootStatues.SetActive(false);
        }

        void InitEnemies()
        {
            foreach (var actorItemInfo in GameApp.Entry.Config.ActorInfo.m_Actors)
            {
                if (actorItemInfo.m_ActorType != EActorType.Monster)
                {
                    continue;
                }

                GameObject item = GameObject.Instantiate(m_TempTextItem);
                item.SetActive(true);
                item.name = item.ToString();
                item.transform.SetParent(m_RootEnemyNames.transform);
                item.GetComponentInChildren<Text>().text = actorItemInfo.m_Name;
                item.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    m_Handler.OnSelectEnemy(actorItemInfo.m_ID);
                    GameApp.Entry.Game.Audio.PlayCommonClick();
                    GameApp.Entry.UI.ShowTips("创建怪物成功！", 1);
                });
            }

            m_RootEnemyNames.SetActive(false);
        }

        void OnClickSelectEnemy()
        {
            GameApp.Entry.Game.Audio.PlayCommonClick();
            m_RootStatues.SetActive(false);
            m_RootEnemyNames.SetActive(true);
        }

        void OnClickTransmit()
        {
            GameApp.Entry.Game.Audio.PlayCommonClick();
            m_RootStatues.SetActive(true);
            m_RootEnemyNames.SetActive(false);
        }

        void OnClickQuit()
        {
            Destroy();
            GameApp.Entry.Game.Audio.PlayCommonClick();
            m_Handler.OnClickQuit();
        }

        protected override void OnDestroy()
        {
            GameApp.Entry.Game.Audio.Play2DSound("Sound/UI/ActorInfoWndClose");
            base.OnDestroy();
        }

#if UNITY_EDITOR
        protected override void Update()
        {
            base.Update();
            if (Input.GetKeyDown(KeyCode.F))
            {
                OnClickQuit();
            }
        }
#endif
    }
}