using System;
using System.Collections.Generic;
using Saber.Config;
using Saber.Director;
using Saber.Frame;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Saber.UI
{
    public class Wnd_DressUp : WndBase
    {
        public class Content : WndContent
        {
            public List<int> m_ListClothes;
        }

        public interface IHandler : IWndHandler
        {
            void OnClickDressUp(int id, Action onFinished);
            bool IsDressing(int id);
            void OnCloseWnd();
        }

        enum EDressUpToggleType
        {
            All,
            Hat,
            Jacket,
            Hands,
            Pants,
        }

        private static EDressUpToggleType s_CurType;

        [SerializeField] private Button m_BtnConfirm;
        [SerializeField] private Button m_BtnClose;
        [SerializeField] private Button m_TempItem;
        [SerializeField] private GridLayoutGroup m_GridClothes;
        [SerializeField] private Button m_ToggleAll;
        [SerializeField] private RectTransform m_ToggleRoot;
        private List<GameObject> m_goIcons, m_ToggleButtons;
        private Content m_Content;
        private IHandler m_Handler;

        protected override bool PauseGame => false;


        protected override void OnAwake()
        {
            base.OnAwake();

            m_BtnConfirm.onClick.AddListener(OnClickClose);
            m_BtnClose.onClick.AddListener(OnClickClose);

            m_Content = base.m_WndContent as Content;
            m_Handler = base.m_WndHandler as IHandler;
            InitToggles();
            ResetIcons();
            ResetWnd();
        }

        string GetClothTypeString(EDressUpToggleType toggleType)
        {
            return toggleType switch
            {
                EDressUpToggleType.All => "全部",
                EDressUpToggleType.Hat => "头甲",
                EDressUpToggleType.Jacket => "胸甲",
                EDressUpToggleType.Hands => "臂甲",
                EDressUpToggleType.Pants => "腿甲",
                _ => throw new InvalidOperationException("Unknown cloth type:" + toggleType),
            };
        }

        void InitToggles()
        {
            Array clotheTypes = Enum.GetValues(typeof(EDressUpToggleType));
            m_ToggleButtons = new();
            m_ToggleAll.gameObject.SetActive(false);
            for (int i = 0; i < clotheTypes.Length; i++)
            {
                EDressUpToggleType toggleType = (EDressUpToggleType)clotheTypes.GetValue(i);
                GameObject toggleObj = GameObject.Instantiate(m_ToggleAll.gameObject, m_ToggleRoot.transform);
                toggleObj.SetActive(true);
                toggleObj.name = toggleType.ToString();
                Button btn = toggleObj.GetComponent<Button>();
                m_ToggleButtons.Add(toggleObj);
                Text[] text = toggleObj.GetComponentsInChildren<Text>();
                foreach (var t in text)
                {
                    t.text = GetClothTypeString(toggleType);
                }

                btn.onClick.AddListener(() =>
                {
                    s_CurType = toggleType;
                    ResetIcons();
                    ResetWnd();
                });
            }
        }

        void OnClickClose()
        {
            GameApp.Entry.Game.Audio.PlayCommonClick();
            Destroy();
            m_Handler.OnCloseWnd();
        }

        bool IsClothTypeRight(EClothType clothType)
        {
            switch (s_CurType)
            {
                case EDressUpToggleType.All:
                    return clothType == EClothType.Hat || clothType == EClothType.Pants ||
                           clothType == EClothType.Jacket || clothType == EClothType.Hands;
                case EDressUpToggleType.Hat:
                    return clothType == EClothType.Hat;
                case EDressUpToggleType.Pants:
                    return clothType == EClothType.Pants;
                case EDressUpToggleType.Jacket:
                    return clothType == EClothType.Jacket;
                case EDressUpToggleType.Hands:
                    return clothType == EClothType.Hands;
                default:
                    throw new InvalidOperationException("Unknown type:" + s_CurType);
            }
        }

        void ResetIcons()
        {
            m_TempItem.gameObject.SetActive(false);

            if (m_goIcons == null)
                m_goIcons = new List<GameObject>();
            int sum = 0;

            foreach (var clothID in m_Content.m_ListClothes)
            {
                ClothItemInfo clothInfo = GameApp.Entry.Config.ClothInfo.GetClothByID(clothID);
                bool isRight = clothInfo.m_IsActive && IsClothTypeRight(clothInfo.m_ClothType);
                if (!isRight)
                {
                    continue;
                }

                GameObject go;
                if (sum < m_goIcons.Count)
                {
                    go = m_goIcons[sum];
                }
                else
                {
                    go = UnityEngine.Object.Instantiate(m_TempItem.gameObject, m_GridClothes.transform);
                    m_goIcons.Add(go);
                }

                go.SetActive(true);
                ResetIcon(go, clothInfo);
                ++sum;
            }

            for (int i = sum; i < m_goIcons.Count; i++)
            {
                m_goIcons[i].SetActive(false);
            }
        }

        void ResetIcon(GameObject go, ClothItemInfo clothInfo)
        {
            Button btn = go.GetComponent<Button>();
            RawImage imageIcon = go.transform.Find("Icon").GetComponent<RawImage>();
            Text textName = go.transform.Find("Name").GetComponent<Text>();

            go.SetActive(true);
            go.name = clothInfo.m_ID.ToString();

            // icon
            clothInfo.LoadIcon(t => imageIcon.texture = t);

            // name
            textName.text = clothInfo.m_Name;

            // click
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                GameApp.Entry.Game.Audio.PlayCommonClick();
                m_Handler.OnClickDressUp(clothInfo.m_ID, ResetWnd);
            });
        }

        void ResetWnd()
        {
            // 刷新选中状态
            for (int i = 0; i < m_goIcons.Count; i++)
            {
                GameObject go = m_goIcons[i];
                int clothID = int.Parse(go.name);
                bool isSelected = m_Handler.IsDressing(clothID);
                GameObject goSelected = go.transform.Find("Selected").gameObject;
                goSelected.SetActive(isSelected);
            }

            ResetToggleButtonState(m_ToggleAll.gameObject, s_CurType == EDressUpToggleType.All);

            foreach (var toggleBtn in m_ToggleButtons)
            {
                bool isOn = s_CurType.ToString() == toggleBtn.name;
                ResetToggleButtonState(toggleBtn, isOn);
            }
        }

        void ResetToggleButtonState(GameObject obj, bool isOn)
        {
            obj.transform.Find("On").gameObject.SetActive(isOn);
            obj.transform.Find("Off").gameObject.SetActive(!isOn);
        }
    }
}