using System;
using Saber.Frame;
using UnityEngine;
using YooAsset;

namespace Saber.Config
{
    [CreateAssetMenu(menuName = "Saber/Prop Info", fileName = "PropInfo", order = 1)]
    public class PropInfo : ScriptableObject
    {
        public PropItemInfo[] m_Props;
    }

    [Serializable]
    public class PropItemInfo
    {
        public int m_ID;
        public string m_Name;
        public string m_Description;
        public string m_Icon;
        public EPropType m_PropType;

        public float m_Param1;
        public float m_Param2;
    }

    [Serializable]
    public enum EPropType
    {
        HealHp,
        BackToShenKan,
        HealHpContinuous,
        AddSoul,
        Enchant,
        AddPower,
    }

    [Serializable]
    public class PlayerPropItemInfo
    {
        public int m_ID;
        public int m_Count = 1;
    }
}