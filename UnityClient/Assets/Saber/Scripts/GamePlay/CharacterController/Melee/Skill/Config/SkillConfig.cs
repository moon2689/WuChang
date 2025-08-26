using System;
using System.Collections.Generic;
using CombatEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Saber.CharacterController
{
    [CreateAssetMenu(menuName = "Saber/Melee skill config", fileName = "NewSkill", order = 1)]
    public class SkillConfig : ScriptableObject
    {
        public EWeaponStyle m_WeaponStyle;
        public SkillItem[] m_SkillItems;

        private Dictionary<int, SkillItem> m_DicSkills;

        public SkillItem GetSkillItemByID(int id)
        {
            if (m_DicSkills == null)
            {
                m_DicSkills = new();
                for (int i = 0; i < m_SkillItems.Length; i++)
                {
                    m_DicSkills.Add(m_SkillItems[i].m_ID, m_SkillItems[i]);
                }
            }

            m_DicSkills.TryGetValue(id, out var tar);
            return tar;
        }
    }

    [Serializable]
    public class SkillItem
    {
        public int m_ID;
        public SkillAnimStateMachine[] m_AnimStates;

        public float CostStrength = 5;
        public int m_CostPower;
        public int m_PowerAddWhenHitted;
        public float m_CDSeconds;
        public ESkillType m_SkillType;
        public ETriggerCondition m_TriggerCondition;
        public bool UseGravityWhenInAir;

        public bool m_FirstSkillOfCombo;
        public int[] m_ChainSkillIDs;

        public RangedFloat m_AIPramAttackDistance;

        public float m_AttackTriggerDistance;
        public EResilience m_Resilience = EResilience.Level1;

        public bool IsAirSkill => m_TriggerCondition == ETriggerCondition.InAir;

        public bool InRange(float distance)
        {
            return distance >= m_AIPramAttackDistance.minValue && distance <= m_AIPramAttackDistance.maxValue;
        }
    }

    [Serializable]
    public class SkillAnimStateMachine
    {
        public string m_Name;
        public AbilityScriptableObject m_EventData;

        public AbilityEventWithEffects EventWithEffects { get; set; }
    }

    [Serializable]
    public enum ETriggerCondition
    {
        InGround,
        InAir,
        InDodgeForward,
        InDodgeNotForward,
        InSprint,
        AfterTanFanSucceed,
    }

    [Serializable]
    public enum ESkillType
    {
        LightAttack,
        HeavyAttack,
        Execute,
        MoveThenAttack,
        Skill1,
        Skill2,
        Skill3,
        Skill4,
        Skill5,
        ChargeAttack,
    }

    [Serializable]
    public enum EMagicType
    {
        ClearDay,
        RecoverHP,
    }

    public enum EResilience
    {
        Level0,
        Level1,
        Level2,
        Level3,
        Level4,
    }
}