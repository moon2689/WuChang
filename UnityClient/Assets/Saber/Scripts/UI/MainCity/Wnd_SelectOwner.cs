using System;
using System.Collections.Generic;
using Saber.Config;
using Saber.Director;
using Saber.Frame;
using UnityEngine;
using UnityEngine.UI;

namespace Saber.UI
{
    public class Wnd_SelectOwner : WndBase
    {
        public interface IHandler : IWndHandler
        {
            void OnClickConfirm(int characterID, int sceneIndex);
        }

        [SerializeField] Button m_btnConfirm, m_btnClose;
        [SerializeField] Button m_tempActor;
        [SerializeField] GridLayoutGroup m_gridActors;
        [SerializeField] Dropdown m_dropOption;
        List<GameObject> m_goIcons;
        private IHandler m_Handler;
        private int m_SelectedActorID;

        protected override bool PauseGame => true;
        int Option => m_dropOption.value;


        protected override void OnAwake()
        {
            base.OnAwake();
            m_Handler = base.m_WndHandler as IHandler;

            m_btnConfirm.onClick.AddListener(OnClickConfirm);
            m_btnClose.onClick.AddListener(OnClickClose);

            LoadPlayerIcons();
            ResetWnd();
            ResetSceneOptions();
        }

        void ResetSceneOptions()
        {
            List<string> options = new();
            foreach (var scene in GameApp.Entry.Config.SceneInfo.m_Scenes)
            {
                options.Add(scene.m_SceneName);
            }

            m_dropOption.AddOptions(options);
        }

        void OnClickConfirm()
        {
            GameApp.Entry.Game.Audio.PlayCommonClick();
            if (m_SelectedActorID > 0)
            {
                m_Handler.OnClickConfirm(m_SelectedActorID, Option);
                Destroy();
            }
            else
            {
                GameApp.Entry.UI.CreateMsgBox("请选择角色", null);
            }
        }

        void OnClickClose()
        {
            GameApp.Entry.Game.Audio.PlayCommonClick();
            Destroy();
        }

        void LoadPlayerIcons()
        {
            m_tempActor.gameObject.SetActive(false);

            m_goIcons = new List<GameObject>();

            //m_gridActors.cellSize = m_Content.CellSize;

            foreach (var item in GameApp.Entry.Config.ActorInfo.m_Actors)
            {
                GameObject go = UnityEngine.Object.Instantiate(m_tempActor.gameObject, m_gridActors.transform);
                ResetPlayerIcon(go, item);
                m_goIcons.Add(go);
            }
        }

        void ResetPlayerIcon(GameObject go, ActorItemInfo actorInfo)
        {
            Button btn = go.GetComponent<Button>();
            RawImage imageIcon = go.transform.Find("Icon").GetComponent<RawImage>();
            GameObject goSelected = go.transform.Find("Selected").gameObject;
            Text textName = go.transform.Find("Name").GetComponent<Text>();

            go.SetActive(true);
            go.name = actorInfo.m_ID.ToString();
            goSelected.SetActive(false);
            imageIcon.color = Color.gray;

            // icon
            imageIcon.texture = actorInfo.LoadIcon();

            // name
            textName.text = actorInfo.m_Name;

            // click
            btn.onClick.AddListener(() =>
            {
                GameApp.Entry.Game.Audio.PlayCommonClick();
                m_SelectedActorID = actorInfo.m_ID;
                ResetWnd();
            });
        }

        void ResetWnd()
        {
            RefreshActorSelectionState(); // 刷新角色选中状态
        }

        void RefreshActorSelectionState()
        {
            for (int i = 0; i < m_goIcons.Count; i++)
            {
                GameObject go = m_goIcons[i];
                bool isSelected = m_SelectedActorID.ToString() == go.name;
                RefreshSelectionState(go, isSelected);
            }
        }

        void RefreshSelectionState(GameObject go, bool isSelected)
        {
            Transform transIcon = go.transform.Find("Icon");
            RawImage textureIcon = transIcon.GetComponent<RawImage>();
            if (textureIcon)
                textureIcon.color = isSelected ? Color.white : Color.gray;
            else
                transIcon.GetComponent<Image>().color = isSelected ? Color.white : Color.gray;

            GameObject goSelected = go.transform.Find("Selected").gameObject;
            goSelected.SetActive(isSelected);
        }

        public void HideCloseButton()
        {
            m_btnClose.gameObject.SetActive(false);
        }
    }
}