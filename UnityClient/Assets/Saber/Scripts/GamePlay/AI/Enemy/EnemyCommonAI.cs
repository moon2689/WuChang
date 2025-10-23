using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using CombatEditor;
using Saber.CharacterController;

namespace Saber.AI
{
    public class EnemyCommonAI : EnemyAIBase
    {
        private List<SkillItem> m_ListSkills = new();
        private Queue<SkillItem> m_LastSkill = new();
        private SkillItem m_CurrentSkill;
        private List<SkillItem> m_CheckedWhetherDodgeSkills = new();

        
        protected override void OnStart()
        {
            base.OnStart();
            ToSearchEnemy();

            foreach (var e in Monster.m_MonsterInfo.m_AIInfo.m_EventsBeforeFighting)
            {
                TriggerFightingEvent(e);
            }
        }

        void TriggerFightingEvent(MonsterFightingEvent eventObj)
        {
            if (eventObj.EventType == EMonsterFightingEvent.HideWeapon)
            {
                var tarWeaponConfig = Monster.m_BaseActorInfo.m_WeaponPrefabs[eventObj.m_ParamInt];
                var weapon = Monster.CMelee.CWeapon.GetWeaponByPos(tarWeaponConfig.m_ArmBoneType);
                weapon.gameObject.SetActive(false);
            }
            else if (eventObj.EventType == EMonsterFightingEvent.ShowWeapon)
            {
                var tarWeaponConfig = Monster.m_BaseActorInfo.m_WeaponPrefabs[eventObj.m_ParamInt];
                var weapon = Monster.CMelee.CWeapon.GetWeaponByPos(tarWeaponConfig.m_ArmBoneType);
                weapon.gameObject.SetActive(true);
            }
        }

        /// <summary>当BOSS进入二阶段</summary>
        public override void OnEnterBossStageTwo()
        {
            base.OnEnterBossStageTwo();

            foreach (var e in Monster.m_MonsterInfo.m_AIInfo.m_EventsOnBossStageToTwo)
            {
                TriggerFightingEvent(e);
            }

            foreach (SkillItem skill in Actor.Skills)
            {
                if (skill.m_FirstSkillOfCombo &&
                    skill.m_AITriggerCondition == EAITriggerSkillCondition.OnEnterBossStageTwo &&
                    IsCDColldown(skill) &&
                    SatifyTriggerCondition(skill))
                {
                    m_CurrentSkill = skill;
                    SwitchCoroutine(AttackItor());
                    break;
                }
            }
        }

        protected override IEnumerator SearchEnemy()
        {
            Actor.StopMove();
            LockingEnemy = null;
            yield return null;

            if (Monster.m_MonsterInfo.m_SpecialIdls.Length > 0)
            {
                int ranIndex = UnityEngine.Random.Range(0, Monster.m_MonsterInfo.m_SpecialIdls.Length);
                string randomSpecialIdle = Monster.m_MonsterInfo.m_SpecialIdls[ranIndex];
                while (!Monster.PlayAction(PlayActionState.EActionType.SpecialIdle, randomSpecialIdle, null))
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }

            while (true)
            {
                //Debug.Log("SearchEnemy");

                if (TryLockEnemy(out var foundType))
                {
                    yield return null;
                    OnFoundEnemy(foundType);
                    yield break;
                }

                OnSearchEnemy();

                yield return new WaitForSeconds(0.3f);
            }
        }

        /// <summary>战斗开始</summary>
        protected override void OnFoundEnemy(EFoundEnemyType foundType)
        {
            ToStalemate();
        }

        IEnumerator MoveToOriginPosItor()
        {
            LockingEnemy = null;
            float timer = 0;

            while (true)
            {
                Vector3 dir = m_OriginPos - Actor.transform.position;

                if (dir.magnitude < 0.5f)
                {
                    ToSearchEnemy();
                    yield break;
                }

                Actor.DesiredLookDir = dir;
                Actor.StartMove(EMoveSpeedV.Walk, new Vector3(0, 0, 1));

                if (timer >= 0)
                {
                    timer -= Time.deltaTime;
                    if (timer < 0)
                    {
                        timer = 0.3f;
                        if (TryLockEnemy(out var foundType))
                        {
                            yield return null;
                            OnFoundEnemy(foundType);
                            yield break;
                        }
                    }
                }

                yield return null;
            }
        }

        // 对峙

        #region Stalemate

        void ToStalemate()
        {
            SwitchCoroutine(StalemateItor());
        }

        // 对峙
        IEnumerator StalemateItor()
        {
            Actor.StopMove();

            // 特殊IDLE切换成战斗IDLE状态
            bool wait;
            if (Monster.CurrentStateType == EStateType.PlayAction)
            {
                var actionState = (PlayActionState)Monster.CStateMachine.CurrentState;
                if (actionState.CurAction == PlayActionState.ECurAction.SpecialIdle)
                {
                    wait = true;
                    Monster.PlayAction(PlayActionState.EActionType.SpecialIdleTransition, null, () => wait = false);
                    while (wait)
                    {
                        yield return null;
                    }
                }
            }

            // 朝向敌人
            if (Monster.m_MonsterInfo.m_AIInfo.m_TurnDirWhenNotFaceToEnemy && !IsFaceToEnemy())
            {
                wait = true;
                if (Actor.PlayAction(PlayActionState.EActionType.FaceToLockingEnemy, () => wait = false))
                {
                    while (wait)
                    {
                        yield return null;
                    }
                }
            }

            Vector2 stayTime = Monster.m_MonsterInfo.m_AIInfo.m_StalemateStayTime;
            float timerStay = UnityEngine.Random.Range(stayTime.x, stayTime.y);
            float moveTimer = 0;
            m_CheckedWhetherDodgeSkills.Clear();
            while (true)
            {
                // 敌人死亡或敌人距离过远，则返回出生点
                if (base.LockingEnemy == null || LockingEnemy.IsDead ||
                    m_DistanceToEnemy > Monster.m_MonsterInfo.m_AIInfo.m_LostFocusRange)
                {
                    Actor.StopMove();
                    yield return new WaitForSeconds(3);
                    SwitchCoroutine(MoveToOriginPosItor());
                    yield break;
                }

                // 攻击
                timerStay -= Time.deltaTime;
                if (timerStay < 0 && !LockingEnemy.IsInSpecialStun)
                {
                    if (Monster.m_MonsterInfo.CanDodge &&
                        m_DistanceToEnemy < Monster.m_MonsterInfo.m_AIInfo.m_WarningRange &&
                        CalcProbability(Monster.m_MonsterInfo.m_AIInfo.m_DodgePercentAfterStalemate))
                    {
                        ToDodge();
                    }
                    else
                    {
                        ToAttack();
                    }

                    yield break;
                }

                // 如果敌人攻击中，则格挡或闪避
                if (LockingEnemy.CurrentStateType == EStateType.Skill &&
                    LockingEnemy.CurrentSkill.CurrentAttackState != EAttackStates.AfterAttack &&
                    Monster.m_MonsterInfo.CanDodge &&
                    !m_CheckedWhetherDodgeSkills.Contains(LockingEnemy.CurrentSkill.SkillConfig) &&
                    m_DistanceToEnemy < LockingEnemy.CurrentSkill.SkillConfig.m_AIPramAttackDistance.maxValue)
                {
                    if (CalcProbability(Monster.m_MonsterInfo.m_AIInfo.m_DodgeDamagePercent))
                    {
                        ToDodge();
                        yield break;
                    }

                    var skillItem = LockingEnemy.CurrentSkill.SkillConfig;
                    if (!m_CheckedWhetherDodgeSkills.Contains(skillItem))
                        m_CheckedWhetherDodgeSkills.Add(skillItem);
                }

                if (LockingEnemy.CurrentStateType != EStateType.Skill)
                {
                    m_CheckedWhetherDodgeSkills.Clear();
                }

                // 随机游走
                if (Actor.CurrentStateType == EStateType.Idle || moveTimer < 0)
                {
                    RandomMove();
                    Vector3 randomMoveTime = Monster.m_MonsterInfo.m_AIInfo.m_RandomMoveTime;
                    moveTimer = UnityEngine.Random.Range(randomMoveTime.x, randomMoveTime.y);
                }

                if (Actor.CurrentStateType == EStateType.Move)
                {
                    moveTimer -= Time.deltaTime;
                }

                yield return null;
            }
        }

        void RandomMove()
        {
            Vector3 axis;
            if (m_DistanceToEnemy > 5)
            {
                if (CalcProbability(50))
                {
                    axis = new Vector3(0, 0, 1);
                }
                else if (CalcProbability(50))
                {
                    axis = new Vector3(1, 0, 0);
                }
                else
                {
                    axis = new Vector3(-1, 0, 0);
                }
            }
            else if (m_DistanceToEnemy < 3)
            {
                axis = new Vector3(0, 0, -1);
            }
            else
            {
                axis = CalcProbability(50) ? new Vector3(1, 0, 0) : new Vector3(-1, 0, 0);
            }

            Actor.StartMove(EMoveSpeedV.Walk, axis);
        }

        #endregion


        // 攻击

        #region Attack

        bool IsRepeatSkill(SkillItem skill)
        {
            return m_LastSkill.Any(a => a == skill || (a.m_GroupID != 0 && a.m_GroupID == skill.m_GroupID));
        }

        bool IsCDColldown(SkillItem skill)
        {
            if (!Actor.CMelee[skill.m_ID].IsCDCooldown)
            {
                return false;
            }

            if (skill.m_GroupID != 0)
            {
                foreach (var s in Actor.Skills)
                {
                    if (s.m_GroupID == skill.m_GroupID && !Actor.CMelee[s.m_ID].IsCDCooldown)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        bool SatifyTriggerCondition(SkillItem skill)
        {
            if (skill.m_AITriggerCondition == EAITriggerSkillCondition.None)
            {
                return true;
            }
            else if (skill.m_AITriggerCondition == EAITriggerSkillCondition.BossStageOne)
            {
                return Monster.BossStage == 1;
            }
            else if (skill.m_AITriggerCondition == EAITriggerSkillCondition.OnEnterBossStageTwo ||
                     skill.m_AITriggerCondition == EAITriggerSkillCondition.BossStageTwo)
            {
                return Monster.BossStage == 2;
            }
            else
            {
                Debug.LogError($"Unknown condition:{skill.m_AITriggerCondition}");
                return false;
            }
        }

        void ToAttack()
        {
            if (LockingEnemy.IsDead)
            {
                ToStalemate();
                return;
            }


            m_CurrentSkill = GetInRangeRandomSkill();

            if (m_CurrentSkill != null)
            {
                SwitchCoroutine(AttackItor());
            }
            else
            {
                EAIAttackStyleWhenTooFar attackStyle = Monster.m_MonsterInfo.m_AIInfo.m_AIAttackStyleWhenTooFar;
                if (attackStyle == EAIAttackStyleWhenTooFar.UseLongestRangeSkill)
                {
                    m_CurrentSkill = GetLongestAttackRangeSkill(); //找攻击距离最大的技能
                }
                else if (attackStyle == EAIAttackStyleWhenTooFar.UseRandomSkill)
                {
                    m_CurrentSkill = GetRandomSkill();
                }
                else if (attackStyle == EAIAttackStyleWhenTooFar.ToStalemate)
                {
                    m_CurrentSkill = null;
                }
                else
                {
                    Debug.LogError($"Unknown attack style:{attackStyle}");
                }

                if (m_CurrentSkill != null)
                {
                    SwitchCoroutine(SprintAndAttackItor());
                }
                else
                {
                    ToStalemate();
                }
            }
        }

        SkillItem GetInRangeRandomSkill()
        {
            m_ListSkills.Clear();
            foreach (SkillItem skill in Actor.Skills)
            {
                if (skill.m_FirstSkillOfCombo &&
                    IsCDColldown(skill) &&
                    !IsRepeatSkill(skill) &&
                    SatifyTriggerCondition(skill) &&
                    skill.InRange(m_DistanceToEnemy))
                {
                    m_ListSkills.Add(skill);
                }
            }

            if (m_ListSkills.Count <= 0)
                return null;

            return m_ListSkills[UnityEngine.Random.Range(0, m_ListSkills.Count)];
        }

        SkillItem GetLongestAttackRangeSkill()
        {
            float maxAttackDistance = float.MinValue;
            SkillItem tarSkill = null;
            foreach (SkillItem skill in Actor.Skills)
            {
                if (skill.m_FirstSkillOfCombo &&
                    IsCDColldown(skill) &&
                    SatifyTriggerCondition(skill) &&
                    !IsRepeatSkill(skill))
                {
                    if (skill.m_AIPramAttackDistance.maxValue > maxAttackDistance)
                    {
                        maxAttackDistance = skill.m_AIPramAttackDistance.maxValue;
                        tarSkill = skill;
                    }
                }
            }

            if (tarSkill == null)
            {
                maxAttackDistance = float.MinValue;
                foreach (SkillItem skill in Actor.Skills)
                {
                    if (skill.m_FirstSkillOfCombo &&
                        IsCDColldown(skill) &&
                        SatifyTriggerCondition(skill))
                    {
                        if (skill.m_AIPramAttackDistance.maxValue > maxAttackDistance)
                        {
                            maxAttackDistance = skill.m_AIPramAttackDistance.maxValue;
                            tarSkill = skill;
                        }
                    }
                }
            }

            return tarSkill;
        }

        SkillItem GetRandomSkill()
        {
            m_ListSkills.Clear();
            foreach (SkillItem skill in Actor.Skills)
            {
                if (skill.m_FirstSkillOfCombo &&
                    IsCDColldown(skill) &&
                    SatifyTriggerCondition(skill) &&
                    !IsRepeatSkill(skill))
                {
                    m_ListSkills.Add(skill);
                }
            }

            if (m_ListSkills.Count <= 0)
            {
                foreach (SkillItem skill in Actor.Skills)
                {
                    if (skill.m_FirstSkillOfCombo &&
                        IsCDColldown(skill) &&
                        SatifyTriggerCondition(skill))
                    {
                        m_ListSkills.Add(skill);
                    }
                }
            }

            if (m_ListSkills.Count <= 0)
                return null;

            return m_ListSkills[UnityEngine.Random.Range(0, m_ListSkills.Count)];
        }

        /// <summary>冲刺后攻击</summary>
        IEnumerator SprintAndAttackItor()
        {
            Vector2 sprintTime = Monster.m_MonsterInfo.m_AIInfo.m_SprintTimeBeforeAttack;
            float timerSprint = UnityEngine.Random.Range(sprintTime.x, sprintTime.y);
            while (true)
            {
                Actor.StartMove(EMoveSpeedV.Sprint, new Vector3(0, 0, 1));
                if (m_DistanceToEnemy < m_CurrentSkill.m_AIPramAttackDistance.maxValue)
                {
                    SwitchCoroutine(AttackItor());
                    yield break;
                }

                timerSprint -= Time.deltaTime;
                if (timerSprint < 0)
                {
                    ToStalemate();
                    yield break;
                }

                yield return null;
            }
        }

        void SetLastSkill(SkillItem skill)
        {
            m_LastSkill.Enqueue(skill);
            if (m_LastSkill.Count > 2)
            {
                m_LastSkill.Dequeue();
            }
        }

        bool IsFaceToEnemy()
        {
            if (LockingEnemy == null)
            {
                return false;
            }

            Vector3 dirToEnemy = LockingEnemy.transform.position - Actor.transform.position;
            return Vector3.Dot(Actor.transform.forward, dirToEnemy) > 0;
        }

        /// <summary>攻击</summary>
        IEnumerator AttackItor()
        {
            Actor.StopMove();
            bool triggered = false;
            while (true)
            {
                if (!triggered)
                {
                    if (Actor.TryTriggerSkill(m_CurrentSkill))
                    {
                        triggered = true;
                        SetLastSkill(m_CurrentSkill);
                    }
                }
                else if (Actor.CurrentStateType == EStateType.Skill)
                {
                    if (Actor.CurrentSkill.SkillConfig.m_ChainSkills.Length > 0 && Actor.CurrentSkill.InComboTime)
                    {
                        if (LockingEnemy.IsInSpecialStun)
                        {
                            ToStalemate();
                            yield break;
                        }

                        m_ListSkills.Clear();
                        foreach (var item in Actor.CurrentSkill.SkillConfig.m_ChainSkills)
                        {
                            SkillItem skillConfig = Actor.CMelee.SkillConfig.GetSkillItemByID(item.m_SkillID);
                            if (m_DistanceToEnemy < skillConfig.m_AIPramAttackDistance.maxValue)
                                m_ListSkills.Add(skillConfig);
                        }

                        if (m_ListSkills.Count > 0)
                        {
                            int randomIndex = UnityEngine.Random.Range(0, m_ListSkills.Count);
                            SkillItem tarSkillItem = m_ListSkills[randomIndex];
                            Actor.TryTriggerSkill(tarSkillItem);
                        }
                        else
                        {
                            ToStalemate();
                            yield break;
                        }
                    }
                    else if (Actor.CurrentSkill.CanExit)
                    {
                        if (Monster.m_MonsterInfo.CanDodge &&
                            m_DistanceToEnemy < Monster.m_MonsterInfo.m_AIInfo.m_WarningRange &&
                            CalcProbability(Monster.m_MonsterInfo.m_AIInfo.m_DodgePercentAfterAttack))
                        {
                            ToDodge();
                            yield break;
                        }
                    }
                }
                else if (Monster.m_MonsterInfo.m_AIInfo.m_TurnDirWhenNotFaceToEnemy &&
                         (!IsFaceToEnemy() ||
                          LockingEnemy == null ||
                          LockingEnemy.IsDead ||
                          LockingEnemy.IsInSpecialStun))
                {
                    ToStalemate();
                    yield break;
                }
                else
                {
                    if (Monster.m_MonsterInfo.CanDodge &&
                        m_DistanceToEnemy < Monster.m_MonsterInfo.m_AIInfo.m_WarningRange &&
                        CalcProbability(Monster.m_MonsterInfo.m_AIInfo.m_DodgePercentAfterAttack))
                    {
                        ToDodge();
                        yield break;
                    }

                    if (CalcProbability(Monster.m_MonsterInfo.m_AIInfo.m_ContinueAttackPercentAfterAttack))
                    {
                        ToAttack();
                        yield break;
                    }

                    ToStalemate();
                    yield break;
                }


                yield return null;
            }
        }

        #endregion


        // 闪避

        #region Dodge

        void ToDodge()
        {
            SwitchCoroutine(DodgeItor());
        }

        IEnumerator DodgeItor()
        {
            if (!Monster.m_MonsterInfo.CanDodge)
            {
                throw new InvalidOperationException("Cann't dodge");
            }

            Vector3 axis;
            if (Monster.m_MonsterInfo.m_DodgeType.HasFlag(EDodgeType.Right) && CalcProbability(33))
                axis = new Vector3(1, 0, 0);
            else if (Monster.m_MonsterInfo.m_DodgeType.HasFlag(EDodgeType.Left) && CalcProbability(50))
                axis = new Vector3(-1, 0, 0);
            else
                axis = new Vector3(0, 0, -1);

            bool dodged = false;

            while (true)
            {
                if (Actor.CurrentStateType != EStateType.Dodge)
                {
                    if (!dodged)
                    {
                        if (Actor.Dodge(axis))
                            dodged = true;
                    }
                    else
                    {
                        ToStalemate();
                        yield break;
                    }
                }

                yield return null;
            }
        }

        #endregion
    }
}