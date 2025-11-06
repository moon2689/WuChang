using System;
using System.Collections.Generic;
using Saber.Config;
using Saber.UI;
using UnityEngine.Serialization;

namespace Saber
{
    [Serializable]
    public class GameProgressData
    {
        public List<SceneProgressData> m_SceneProgress;
        public int m_LastStayingSceneID;
        public int m_LastStayingShenKanID;
        public int[] m_Clothes;
        public PlayerPropItemInfo[] m_Items;
        public MainWndSlotData[] m_Slots;
    }

    [Serializable]
    public class SceneProgressData
    {
        public int m_SceneID;
        public List<int> m_ActivedShenKan;
    }

    [Serializable]
    public class MainWndSlotData
    {
        public Widget_SlotObject.ESlotDataType m_SlotType = Widget_SlotObject.ESlotDataType.None;
        public int m_ID;

        public void Clear()
        {
            m_SlotType = Widget_SlotObject.ESlotDataType.None;
            m_ID = 0;
        }

        public void SwitchData(MainWndSlotData target)
        {
            var slotType = target.m_SlotType;
            var id = target.m_ID;
            target.m_SlotType = m_SlotType;
            target.m_ID = m_ID;
            m_SlotType = slotType;
            m_ID = id;
        }

        public bool IsEqual(MainWndSlotData target)
        {
            if (target == null)
            {
                return false;
            }

            return m_SlotType == target.m_SlotType && m_ID == target.m_ID;
        }
    }
}