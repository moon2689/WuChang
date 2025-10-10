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

        public int m_Value;
    }

    [Serializable]
    public enum EPropType
    {
        HealHp,
        BackToIdol,
        AddSoul,
        Enchant,
    }

    [Serializable]
    public class PlayerPropItemInfo
    {
        public int m_ID;
        public int m_Count = 1;
    }
}