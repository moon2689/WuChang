using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RootMotion.FinalIK;

namespace Saber.CharacterController
{
    /// <summary>处理角色战斗相关事务</summary>
    public class CharacterMelee
    {
        private Dictionary<int, BaseSkill> m_DicSkills;
        private SkillConfig m_SkillConfig;
        private SkillItem[] m_Skills;
        private SkillExecute m_SkillExecute;
        private float m_TimerClearAttackedDamageInfo;
        private DamageInfo m_AttackedDamageInfo;
        private HurtBox[] m_HurtBoxes;
        private HitReaction m_IKHitReaction;

        public SActor Actor { get; private set; }
        public CharacterWeapon CWeapon { get; private set; }

        public BaseSkill CurSkill { get; private set; }
        public SkillConfig SkillConfig => m_SkillConfig;
        public EWeaponStyle CurWeaponStyle => m_SkillConfig.m_WeaponStyle;

        public SkillItem[] ValidSkills
        {
            get
            {
                if (m_Skills == null)
                {
                    List<SkillItem> list = new();
                    foreach (var item in SkillConfig.m_SkillItems)
                    {
                        if (item.m_FirstSkillOfCombo && item.m_TriggerCondition == ETriggerCondition.InGround)
                        {
                            list.Add(item);
                        }
                    }

                    m_Skills = list.ToArray();
                }

                return m_Skills;
            }
        }

        public BaseSkill this[int id]
        {
            get
            {
                m_DicSkills.TryGetValue(id, out var s);
                return s;
            }
        }

        public DamageInfo AttackedDamageInfo
        {
            get => m_AttackedDamageInfo;
            set
            {
                m_AttackedDamageInfo = value;
                if (value != null)
                {
                    m_TimerClearAttackedDamageInfo = 3;
                }
            }
        }

        public HurtBox[] HurtBoxes
        {
            get
            {
                if (m_HurtBoxes == null)
                {
                    m_HurtBoxes = Actor.transform.GetComponentsInChildren<HurtBox>();
                }

                return m_HurtBoxes;
            }
        }

        public HitReaction IKHitReaction
        {
            get
            {
                if (m_IKHitReaction == null)
                {
                    m_IKHitReaction = Actor.transform.GetComponentInChildren<HitReaction>();
                }

                return m_IKHitReaction;
            }
        }


        public CharacterMelee(SActor actor, SkillConfig skillConfig)
        {
            Actor = actor;
            CWeapon = new CharacterWeapon(actor);
            //Actor.CPhysic.Event_OnGrounded += OnGrounded;
            m_SkillConfig = skillConfig;
            InitSkills();
        }

        void InitSkills()
        {
            m_DicSkills = new();

            foreach (var item in m_SkillConfig.m_SkillItems)
            {
                BaseSkill skillObj;
                if (item.m_SkillType == ESkillType.Execute)
                {
                    skillObj = m_SkillExecute = new SkillExecute(Actor, item);
                }
                // else if (item.m_SkillType == ESkillType.MoveThenAttack)
                // {
                //     skillObj = new SkillMoveThenAttack(Actor, item);
                // }
                else
                {
                    skillObj = new SkillCommon(Actor, item);
                }

                m_DicSkills.Add(item.m_ID, skillObj);
            }
        }

        public void SetWeapon(WeaponPrefab[] weaponPrefabs)
        {
            CWeapon.CreateWeapons(weaponPrefabs);
            CWeapon.ResetParent();
        }

        public bool TryTriggerSkill(SkillItem skillItem)
        {
            // 状态机不可转换
            if (!Actor.CStateMachine.CanSwitchTo(EStateType.Skill))
            {
                return false;
            }

            BaseSkill skillObj = m_DicSkills[skillItem.m_ID];
            bool maybeCombo = CurSkill != null && CurSkill.IsTriggering && CurSkill.InComboTime;
            if (maybeCombo)
            {
                if (CurSkill.SkillConfig.m_ChainSkillIDs.Contains(skillItem.m_ID) && skillObj.CanEnter)
                {
                    ForceTriggerSkill(skillObj);
                    return true;
                }
            }

            // 当前没有任何技能释放，则触发起手技能
            if (CurSkill == null || !CurSkill.IsTriggering)
            {
                if (skillItem.m_FirstSkillOfCombo && skillObj.CanEnter && CanSwitchToSpecialSkill(skillItem))
                {
                    ForceTriggerSkill(skillObj);
                    return true;
                }
            }

            return false;
        }

        void ForceTriggerSkill(BaseSkill tarSkill)
        {
            SkillState skillState = Actor.CStateMachine.GetState<SkillState>(EStateType.Skill);
            if (skillState.IsTriggering)
            {
                skillState.Exit();
            }

            CurSkill = tarSkill;
            Actor.CStateMachine.ForceEnterState(skillState);
        }

        public bool TryTriggerSkill(ESkillType type)
        {
            // 当前技能正在触发中，并且未到连招时间或退出时间，则不可重新触发技能
            if (CurSkill != null && CurSkill.IsTriggering && !CurSkill.InComboTime && !CurSkill.CanExit)
            {
                return false;
            }

            // 状态机不可转换
            if (!Actor.CStateMachine.CanSwitchTo(EStateType.Skill))
            {
                return false;
            }

            // 尝试处决
            if (type == ESkillType.LightAttack && m_SkillExecute != null)
            {
                SActor target = m_SkillExecute.GetCanBeExecutedEnemy();
                if (target != null)
                {
                    m_SkillExecute.Target = target;
                    ForceTriggerSkill(m_SkillExecute);
                    return true;
                }
            }

            // 尝试连招
            bool canCombo = CurSkill != null && CurSkill.IsTriggering && CurSkill.InComboTime;
            if (canCombo)
            {
                foreach (var chainSkillID in CurSkill.SkillConfig.m_ChainSkillIDs)
                {
                    var chainSkill = m_DicSkills[chainSkillID];
                    if (chainSkill.SkillConfig.m_SkillType == type && chainSkill.CanEnter)
                    {
                        ForceTriggerSkill(chainSkill);
                        return true;
                    }
                }
            }

            // 当前没有任何技能释放，则触发起手技能
            foreach (var skillItem in SkillConfig.m_SkillItems)
            {
                BaseSkill skill = m_DicSkills[skillItem.m_ID];
                if (skillItem.m_FirstSkillOfCombo &&
                    skillItem.m_SkillType == type &&
                    skill.CanEnter &&
                    CanSwitchToSpecialSkill(skillItem))
                {
                    ForceTriggerSkill(skill);
                    return true;
                }
            }

            return false;
        }

        /// <summary>如闪避后放技能，冲刺后放技能等需要特殊判断</summary>
        bool CanSwitchToSpecialSkill(SkillItem skillItem)
        {
            EStateSwitchType canSwitchType = StateHelper.CanSwitchTo(Actor.CurrentStateType, EStateType.Skill);
            if (canSwitchType == EStateSwitchType.CanTriggerSkill &&
                Actor.CStateMachine.CurrentState is ISkillCanTrigger skillCanTrigger)
            {
                return skillCanTrigger.CanTriggerSkill(skillItem);
            }

            return true;
        }

        /*
        public SkillItem GetTargetSkill(ESkillType type)
        {
            if (m_SkillDecapitate != null && m_SkillDecapitate.GetDecapitateTarget())
            {
                return m_SkillDecapitate.SkillConfig;
            }

            bool maybeCombo = CurSkill != null && CurSkill.IsTriggering && !CurSkill.ComboTimePassed;
            if (maybeCombo)
            {
                foreach (var chainSkillID in CurSkill.SkillConfig.m_ChainSkillIDs)
                {
                    var chainSkill = m_DicSkills[chainSkillID];
                    if (chainSkill.SkillConfig.m_SkillType == type)
                    {
                        return chainSkill.SkillConfig;
                    }
                }
            }

            // 当前没有任何技能释放，则触发起手技能
            if (CurSkill == null || !CurSkill.IsTriggering)
            {
                foreach (var skill in SkillConfig.m_SkillItems)
                {
                    if (skill.m_FirstSkillOfCombo && skill.m_SkillType == type)
                    {
                        return skill;
                    }
                }
            }

            return null;
        }
        */

        public BaseSkill GetSkillObject(ESkillType type)
        {
            var tarSkill =
                SkillConfig.m_SkillItems.FirstOrDefault(a => a.m_SkillType == type && a.m_FirstSkillOfCombo);
            if (tarSkill != null)
            {
                m_DicSkills.TryGetValue(tarSkill.m_ID, out var tar);
                return tar;
            }

            return null;
        }

        public void Update()
        {
            if (m_TimerClearAttackedDamageInfo >= 0)
            {
                m_TimerClearAttackedDamageInfo -= Actor.DeltaTime;
                if (m_TimerClearAttackedDamageInfo <= 0)
                {
                    AttackedDamageInfo = null;
                }
            }
        }

        public void ToggleDamage(WeaponDamageSetting damage, bool enable)
        {
            CWeapon.ToggleDamage(damage, enable);
        }
    }
}