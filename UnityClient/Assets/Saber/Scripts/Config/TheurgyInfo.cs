using System;
using Saber.Frame;
using UnityEngine;
using YooAsset;

namespace Saber.Config
{
    [CreateAssetMenu(menuName = "Saber/Theurgy Info", fileName = "TheurgyInfo", order = 1)]
    public class TheurgyInfo : ScriptableObject
    {
        public TheurgyItemInfo[] m_TheurgyArray;
    }

    [Serializable]
    public class TheurgyItemInfo
    {
        public int m_ID;
        public string m_Name;
        public string m_Description;
        public string m_Icon;
        public int m_SkillID;
    }
}