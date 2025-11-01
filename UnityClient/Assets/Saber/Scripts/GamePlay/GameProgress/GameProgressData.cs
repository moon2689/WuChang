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
    }
}