using UnityEngine;
using Saber.CharacterController;

namespace Saber.AI
{
    /// <summary>预输入</summary>
    public class AheadInputData
    {
        enum EAheadInputType
        {
            None,
            Skill,
            Dodge,
        }

        private EAheadInputType m_AheadType;
        private ESkillType m_Key;
        private Vector3 m_DodgeAxis;

        public bool IsEnabled => m_AheadType != EAheadInputType.None;

        public bool TryTrigger(SActor owner)
        {
            if (m_AheadType == EAheadInputType.Skill)
            {
                return owner.TryTriggerSkill(m_Key);
            }
            else if (m_AheadType == EAheadInputType.Dodge)
            {
                return owner.Dodge(m_DodgeAxis);
            }
            else
            {
                Debug.LogError("Unknown type:" + m_AheadType);
            }

            return false;
        }

        public void SetData_Skill(ESkillType key)
        {
            //Debug.Log("SetData_Skill:" + key);
            m_AheadType = EAheadInputType.Skill;
            m_Key = key;
        }

        public void SetData_Dodge(Vector3 axis)
        {
            //Debug.Log("SetData_Dodge:" + axis);
            m_AheadType = EAheadInputType.Dodge;
            m_DodgeAxis = axis;
        }

        public void Clear()
        {
            //Debug.Log("Clear ahead data");
            m_AheadType = EAheadInputType.None;
        }
    }
}