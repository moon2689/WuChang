using System;
using UnityEngine;

namespace Saber.Config
{
    [CreateAssetMenu(menuName = "Saber/Skill Info", fileName = "SkillInfo", order = 1)]
    public class SkillInfo : ScriptableObject
    {
        public SkillItemInfo[] m_Skills;
    }

    [Serializable]
    public class SkillItemInfo
    {
        public int m_ID;
        public string m_Name;
        public Texture2D m_Icon;
        public string m_ResPath;
    }
}