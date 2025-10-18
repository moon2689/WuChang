using CombatEditor;
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
            DrinkPotion,
        }

        private SActor m_Actor;
        private EAheadInputType m_AheadType;
        private ESkillType m_Key;
        private Vector3 m_DodgeAxis;

        public bool IsEnabled => m_AheadType != EAheadInputType.None;

        bool CanAheadInput
        {
            get
            {
                var s = m_Actor.CurrentStateType;
                if (s == EStateType.Skill)
                {
                    return m_Actor.CurrentSkill.CurrentAttackState != EAttackStates.BeforeAttack;
                }

                return s == EStateType.Idle || s == EStateType.Move || s == EStateType.Defense ||
                       s == EStateType.Dodge;
            }
        }

        public AheadInputData(SActor actor)
        {
            m_Actor = actor;
        }

        public bool TryTrigger()
        {
            if (m_AheadType == EAheadInputType.None)
            {
                return false;
            }
            else if (m_AheadType == EAheadInputType.Skill)
            {
                return m_Actor.TryTriggerSkill(m_Key);
            }
            else if (m_AheadType == EAheadInputType.Dodge)
            {
                return m_Actor.Dodge(m_DodgeAxis);
            }
            else if (m_AheadType == EAheadInputType.DrinkPotion)
            {
                return m_Actor.DrinkPotion();
            }
            else
            {
                Debug.LogError("Unknown type:" + m_AheadType);
            }

            return false;
        }

        public void SetData_Skill(ESkillType key)
        {
            if (CanAheadInput)
            {
                //Debug.Log("SetData_Skill:" + key);
                m_AheadType = EAheadInputType.Skill;
                m_Key = key;
            }
        }

        public void SetData_Dodge(Vector3 axis)
        {
            if (CanAheadInput)
            {
                //Debug.Log("SetData_Dodge:" + axis);
                m_AheadType = EAheadInputType.Dodge;
                m_DodgeAxis = axis;
            }
        }

        public void SetData_DrinkPotion()
        {
            if (CanAheadInput)
            {
                if (m_Actor.CAbility.CurAbilityType != EAbilityType.DrinkMedicine)
                    m_AheadType = EAheadInputType.DrinkPotion;
            }
        }

        public void Clear()
        {
            //Debug.Log("Clear ahead data");
            m_AheadType = EAheadInputType.None;
        }
    }
}