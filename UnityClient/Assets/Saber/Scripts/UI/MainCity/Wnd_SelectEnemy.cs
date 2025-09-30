using System;
using System.Collections.Generic;
using System.Linq;
using Saber.AI;
using Saber.Config;
using Saber.Frame;
using UnityEngine;
using UnityEngine.UI;

namespace Saber.UI
{
    public class Wnd_SelectEnemy : WndBase
    {
        public interface IHandler : IWndHandler
        {
            void OnClickConfirm(ActorItemInfo actor, int option);
        }

        [SerializeField] private Button m_btnConfirm, m_btnClose;
        [SerializeField] private Button m_tempActor;
        [SerializeField] private GridLayoutGroup m_gridActors;
        [SerializeField] private Dropdown m_dropOption;

        private ActorItemInfo m_selectedActor;
        private List<GameObject> m_goIcons;

        private IHandler m_Handler;

        public ActorItemInfo SelectedActor
        {
            get => m_selectedActor;
            set => m_selectedActor = value;
        }

        int Option => m_dropOption.value;
        

        protected override void OnAwake()
        {
            base.OnAwake();
            m_Handler = base.m_WndHandler as IHandler;

            m_goIcons = new List<GameObject>();

            m_btnConfirm.onClick.AddListener(OnClickConfirm);
            m_btnClose.onClick.AddListener(OnClickClose);

            // default selected actor
            SelectedActor = GameApp.Entry.Config.ActorInfo.m_Actors[0];

            LoadPlayerIcons();
            ResetWnd();
            LoadAIOptions();
        }

        void LoadAIOptions()
        {
            string[] names = Enum.GetNames(typeof(EAIType));
            m_dropOption.AddOptions(names.ToList());
        }

        void OnClickConfirm()
        {
            GameApp.Entry.Game.Audio.PlayCommonClick();
            m_Handler.OnClickConfirm(SelectedActor, Option);
            Destroy();
        }

        void OnClickClose()
        {
            GameApp.Entry.Game.Audio.PlayCommonClick();
            Destroy();
        }

        void LoadPlayerIcons()
        {
            m_tempActor.gameObject.SetActive(false);

            var array = GameApp.Entry.Config.ActorInfo.m_Actors;
            for (int i = 0; i < array.Length; i++)
            {
                var config = array[i];
                if (config.m_IsActive)
                {
                    GameObject go = UnityEngine.Object.Instantiate(m_tempActor.gameObject, m_gridActors.transform);
                    ResetPlayerIcon(go, config);
                    m_goIcons.Add(go);
                }
            }
        }

        void ResetPlayerIcon(GameObject go, ActorItemInfo config)
        {
            Button btn = go.GetComponent<Button>();
            RawImage imageIcon = go.transform.Find("Icon").GetComponent<RawImage>();
            GameObject goSelected = go.transform.Find("Selected").gameObject;
            Text textName = go.transform.Find("Name").GetComponent<Text>();

            go.SetActive(true);
            go.name = config.m_ID.ToString();
            goSelected.SetActive(false);
            imageIcon.color = Color.gray;

            // icon
            config.LoadIcon(t => imageIcon.texture = t);
            textName.text = config.m_Name;

            // click
            btn.onClick.AddListener(() =>
            {
                GameApp.Entry.Game.Audio.PlayCommonClick();
                SelectEnemy(config);
                ResetWnd();
            });
        }

        void SelectEnemy(ActorItemInfo config)
        {
            if (config == null)
            {
                SelectedActor = null;
                return;
            }

            if (!config.m_IsActive)
            {
                return;
            }

            SelectedActor = config;
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
                bool isSelected = SelectedActor != null && SelectedActor.m_ID.ToString() == go.name;
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
    }
}