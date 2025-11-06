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
    public class Widget_DressUp : WidgetBase
    {
        enum EDressUpToggleType
        {
            All,
            Hat,
            Jacket,
            Hands,
            Pants,
        }

        private static EDressUpToggleType s_CurType;

        [SerializeField] private Button m_BtnRefresh;
        [SerializeField] private Button m_TempItem;
        [SerializeField] private GridLayoutGroup m_GridClothes;
        [SerializeField] private Button m_ToggleAll;
        [SerializeField] private RectTransform m_ToggleRoot;
        [SerializeField] private Transform m_ClassfyItems;
        [SerializeField] private Button m_TempClassfyItem;

        private List<GameObject> m_goIcons = new(), m_ToggleButtons = new(), m_goClassfyIcons = new();


        protected override void Awake()
        {
            base.Awake();

            m_BtnRefresh.onClick.AddListener(OnClickRefresh);

            InitToggles();
            ResetIcons();
            ResetWnd();
        }

        private void OnClickRefresh()
        {
            TakeOffAllClothes();
            ResetWnd();
            m_ClassfyItems.gameObject.SetActive(false);
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
            int sum = 0;

            var allClothes = GameApp.Entry.Config.ClothInfo.GetAllClothesID();
            foreach (var clothID in allClothes)
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
                ResetIcon(go, clothInfo, false);
                ++sum;
            }

            for (int i = sum; i < m_goIcons.Count; i++)
            {
                m_goIcons[i].SetActive(false);
            }
        }

        void ResetIcon(GameObject go, ClothItemInfo clothInfo, bool isClassfy)
        {
            Button btn = go.GetComponent<Button>();
            RawImage imageIcon = go.transform.Find("Icon").GetComponent<RawImage>();
            Text textName = go.transform.Find("Name").GetComponent<Text>();

            go.SetActive(true);
            go.name = clothInfo.m_ID.ToString();

            // icon
            clothInfo.LoadIcon(t => imageIcon.texture = t);

            // name
            textName.text = clothInfo.ClothName;

            // selected
            bool isSelected = IsDressing(clothInfo.m_ID);
            GameObject goSelected = go.transform.Find("Selected").gameObject;
            goSelected.SetActive(isSelected);

            // click
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                GameApp.Entry.Game.Audio.PlayCommonClick();
                OnClickDressUp(clothInfo.m_ID, ResetWnd);
                if (!isClassfy)
                    ResetClassfyItems(clothInfo);
            });
        }

        void ResetClassfyItems(ClothItemInfo selectedCloth)
        {
            m_TempClassfyItem.gameObject.SetActive(false);
            m_ClassfyItems.gameObject.SetActive(true);

            if (m_goClassfyIcons == null)
                m_goClassfyIcons = new List<GameObject>();
            int sum = 0;

            var allClothes = GameApp.Entry.Config.ClothInfo.GetAllClothesID();
            foreach (var clothID in allClothes)
            {
                ClothItemInfo clothInfo = GameApp.Entry.Config.ClothInfo.GetClothByID(clothID);
                if (!clothInfo.m_IsActive || selectedCloth.m_ClassifyID != clothInfo.m_ClassifyID)
                {
                    continue;
                }

                GameObject go;
                if (sum < m_goClassfyIcons.Count)
                {
                    go = m_goClassfyIcons[sum];
                }
                else
                {
                    go = UnityEngine.Object.Instantiate(m_TempClassfyItem.gameObject, m_ClassfyItems.transform);
                    m_goClassfyIcons.Add(go);
                }

                go.SetActive(true);
                ResetIcon(go, clothInfo, true);
                ++sum;
            }

            for (int i = sum; i < m_goClassfyIcons.Count; i++)
            {
                m_goClassfyIcons[i].SetActive(false);
            }
        }

        void ResetWnd()
        {
            // 刷新选中状态
            for (int i = 0; i < m_goIcons.Count; i++)
            {
                GameObject go = m_goIcons[i];
                int clothID = int.Parse(go.name);
                bool isSelected = IsDressing(clothID);
                GameObject goSelected = go.transform.Find("Selected").gameObject;
                goSelected.SetActive(isSelected);
            }

            for (int i = 0; i < m_goClassfyIcons.Count; i++)
            {
                GameObject go = m_goClassfyIcons[i];
                int clothID = int.Parse(go.name);
                bool isSelected = IsDressing(clothID);
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

        void OnClickDressUp(int id, Action onFinished)
        {
            if (GameApp.Entry.Game.Player.CDressUp.IsDressing(id))
            {
                GameApp.Entry.Game.Player.CDressUp.UndressCloth(id);
                onFinished?.Invoke();
            }
            else
            {
                GameApp.Entry.Game.Player.CDressUp.DressCloth(id, onFinished);
            }
        }

        bool IsDressing(int id)
        {
            return GameApp.Entry.Game.Player.CDressUp.IsDressing(id);
        }

        void TakeOffAllClothes()
        {
            GameApp.Entry.Game.Player.CDressUp.UndressAll();
        }
    }
}