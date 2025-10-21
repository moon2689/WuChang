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
            void OnClickTransmit(int sceneID, int shenKanID);
            void OnClickDressUp();
        }

        [SerializeField] private Button m_BtnQuit;
        [SerializeField] private Button m_BtnTransmit;
        [SerializeField] private Button m_BtnDressUp;
        [SerializeField] private GameObject m_Root;
        [SerializeField] private GameObject m_TempTextItem;
        [SerializeField] private GameObject m_RootStatues;


        private IHandler m_Handler;


        public bool ActiveRoot
        {
            set => m_Root.SetActive(value);
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            m_Handler = base.m_WndHandler as IHandler;

            m_BtnQuit.onClick.AddListener(OnClickQuit);
            m_BtnTransmit.onClick.AddListener(OnClickTransmit);
            m_BtnDressUp.onClick.AddListener(OnClickDressUp);

            GameApp.Entry.Game.Audio.Play2DSound("Sound/UI/ActorInfoWndOpen");

            InitStatueTransmit();
        }

        void InitStatueTransmit()
        {
            int count = 0;
            foreach (var s in GameApp.Entry.Game.ProgressMgr.SceneProgressDatas)
            {
                var sceneInfo = GameApp.Entry.Config.SceneInfo.GetSceneInfoByID(s.m_SceneID);
                foreach (var statueID in s.m_ActivedShenKan)
                {
                    ++count;
                    var shenKanInfo = sceneInfo.m_ShenKans.FirstOrDefault(a => a.m_ID == statueID);
                    GameObject item = GameObject.Instantiate(m_TempTextItem);
                    item.SetActive(true);
                    item.name = item.ToString();
                    item.transform.SetParent(m_RootStatues.transform);
                    item.GetComponentInChildren<Text>().text = $"{sceneInfo.m_Name} {shenKanInfo.m_Name}";
                    item.GetComponentInChildren<Button>().onClick.AddListener(() =>
                    {
                        m_Handler.OnClickTransmit(sceneInfo.m_ID, statueID);
                        Destroy();
                        GameApp.Entry.Game.Audio.PlayCommonClick();
                    });
                }
            }

            m_RootStatues.SetActive(false);
        }

        /*
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
                    m_Handler.CreateEnemy(actorItemInfo.m_ID);
                    GameApp.Entry.Game.Audio.PlayCommonClick();
                    GameApp.Entry.UI.ShowTips("创建怪物成功！", 1);
                });
            }

            m_RootEnemyNames.SetActive(false);
        }
        */

        void OnClickDressUp()
        {
            GameApp.Entry.Game.Audio.PlayCommonClick();
            m_RootStatues.SetActive(false);
            m_Handler.OnClickDressUp();
        }

        void OnClickTransmit()
        {
            GameApp.Entry.Game.Audio.PlayCommonClick();
            m_RootStatues.SetActive(true);
        }

        void OnClickQuit()
        {
            Destroy();
            GameApp.Entry.Game.Audio.PlayCommonClick();
            m_Handler.OnClickQuit();
        }

        public override void Destroy()
        {
            base.Destroy();
            GameApp.Entry.Game.Audio.Play2DSound("Sound/UI/ActorInfoWndClose");
        }

#if UNITY_EDITOR
        protected override void Update()
        {
            base.Update();
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnClickQuit();
            }
        }
#endif
    }
}