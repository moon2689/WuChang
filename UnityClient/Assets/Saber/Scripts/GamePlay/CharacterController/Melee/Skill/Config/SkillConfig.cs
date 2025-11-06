using System;
using System.Collections.Generic;
using CombatEditor;
using Saber.AI;
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
        public bool m_Active = true;
        public int m_ID;
        public string m_SkillName;
        public SkillAnimStateMachine[] m_AnimStates;

        public float CostStrength = 5;
        public int m_CostPower;
        public bool m_CanTriggerWhenPowerNotEnough;
        public int m_PowerAddWhenHitted;
        public float m_CDSeconds;
        public ESkillType m_SkillType;
        public ETriggerCondition m_TriggerCondition;

        public bool UseGravityWhenInAir;
        // public bool CanBeTanFan = true;
        // public bool BreakByTanFan = true;

        public bool m_FirstSkillOfCombo;
        public ChainSkill[] m_ChainSkills;

        public EResilience m_ResilienceBeforeAttack = EResilience.Level2;
        public EResilience m_ResilienceAttacking = EResilience.Level2;
        public EResilience m_ResilienceAfterAttack = EResilience.Level1;

        public RangedFloat m_AIPramAttackDistance;
        public int m_GroupID;
        public EAITriggerSkillCondition m_AITriggerCondition;
        public float m_SkipSomeAnimWhenUsePower;
        public EEnchantedMagic m_EnchantedWhenUsePower;

        public bool IsAirSkill => m_TriggerCondition == ETriggerCondition.InAir;


        /// <summary>是否是法术</summary>
        public bool IsTheurgy
        {
            get
            {
                return m_SkillType == ESkillType.FaShu1 ||
                       m_SkillType == ESkillType.FaShu2 ||
                       m_SkillType == ESkillType.FaShu3 ||
                       m_SkillType == ESkillType.FaShu4 ||
                       m_SkillType == ESkillType.FaShu5 ||
                       m_SkillType == ESkillType.FaShu6 ||
                       m_SkillType == ESkillType.FaShu7 ||
                       m_SkillType == ESkillType.FaShu8 ||
                       m_SkillType == ESkillType.FaShu9 ||
                       m_SkillType == ESkillType.FaShu10;
            }
        }

        public bool InRange(float distance)
        {
            return distance >= m_AIPramAttackDistance.minValue && distance <= m_AIPramAttackDistance.maxValue;
        }
    }

    [Serializable]
    public class ChainSkill
    {
        public int m_SkillID;
        public float m_BlendTime = 0.1f;
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
        ChargeAttack,
        Skill1,
        Skill2,
        Skill3,
        Skill4,
        Skill5,
        Skill6,
        Skill7,
        Skill8,
        Skill9,
        Skill10,
        FaShu1,
        FaShu2,
        FaShu3,
        FaShu4,
        FaShu5,
        FaShu6,
        FaShu7,
        FaShu8,
        FaShu9,
        FaShu10,
    }

    [Serializable]
    public enum EMagicType
    {
        ClearDay,
        RecoverHP,
    }

    public enum EResilience
    {
        Level1,
        Level2,
        Level3,
        Level4,
        Level5,
    }
}