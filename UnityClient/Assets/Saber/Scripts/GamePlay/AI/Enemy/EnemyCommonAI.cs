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

        void ToStalemate()
        {
            SwitchCoroutine(StalemateItor());
        }

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
        IEnumerator StalemateItor()
        {
            bool wait;
            if (Monster.CurrentStateType == EStateType.PlayAction)
            {
                var actionState = (PlayActionState)Monster.CStateMachine.CurrentState;
                if (actionState.CurAction == PlayActionState.ECurAction.SpecialIdle)
                {
                    wait = true;
                    Monster.PlayAction(PlayActionState.EActionType.SpecialIdleTransition, null, () => wait = false);
                    while (wait && Monster.CurrentStateType == EStateType.PlayAction)
                    {
                        yield return null;
                    }
                }
            }

            // 朝向敌人
            if (!IsFaceToEnemy())
            {
                wait = true;
                Actor.StopMove();
                if (Actor.PlayAction(PlayActionState.EActionType.FaceToLockingEnemy, () => wait = false))
                {
                    while (wait)
                    {
                        yield return null;
                    }
                }
            }

            float minStayTime = Mathf.Lerp(3, 0.3f, Monster.m_MonsterInfo.AttackDesireRatio);
            float maxStayTime = Mathf.Lerp(10f, 3f, Monster.m_MonsterInfo.AttackDesireRatio);
            float timerStay = UnityEngine.Random.Range(minStayTime, maxStayTime);
            float moveTimer = 0;
            m_CheckedWhetherDodgeSkills.Clear();
            while (true)
            {
                // 敌人死亡或敌人距离过远，则返回出生点
                if (base.LockingEnemy == null || LockingEnemy.IsDead ||
                    m_DistanceToEnemy > Actor.m_BaseActorInfo.m_AIInfo.m_LostFocusRange)
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
                    if (Monster.m_BaseActorInfo.m_AIInfo.CanDodge)
                    {
                        int dodgePercent = (int)Mathf.Lerp(60f, 20, Monster.m_MonsterInfo.AttackDesireRatio);
                        if (CalcProbability(dodgePercent))
                        {
                            ToDodge();
                        }
                        else
                        {
                            ToAttack();
                        }
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
                    Monster.m_BaseActorInfo.m_AIInfo.CanDodge &&
                    !m_CheckedWhetherDodgeSkills.Contains(LockingEnemy.CurrentSkill.SkillConfig))
                {
                    if (m_DistanceToEnemy < LockingEnemy.CurrentSkill.SkillConfig.m_AIPramAttackDistance.maxValue)
                    {
                        int dodgePercent = (int)Mathf.Lerp(10f, 90, Monster.m_MonsterInfo.DodgeDamageRatio);
                        if (CalcProbability(dodgePercent))
                        {
                            ToDodge();
                            yield break;
                        }

                        var skillItem = LockingEnemy.CurrentSkill.SkillConfig;
                        if (!m_CheckedWhetherDodgeSkills.Contains(skillItem))
                            m_CheckedWhetherDodgeSkills.Add(skillItem);
                    }
                }

                if (LockingEnemy.CurrentStateType != EStateType.Skill)
                {
                    m_CheckedWhetherDodgeSkills.Clear();
                }

                // 随机游走
                if (Actor.CurrentStateType == EStateType.Idle || moveTimer < 0)
                {
                    if (m_DistanceToEnemy > 5)
                    {
                        Vector3 axis;
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

                        Actor.StartMove(EMoveSpeedV.Walk, axis);
                    }
                    else if (m_DistanceToEnemy < 3)
                    {
                        Actor.StartMove(EMoveSpeedV.Walk, new Vector3(0, 0, -1));
                    }
                    else
                    {
                        Vector3 axis = CalcProbability(50) ? new Vector3(1, 0, 0) : new Vector3(-1, 0, 0);
                        Actor.StartMove(EMoveSpeedV.Walk, axis);
                    }

                    moveTimer = UnityEngine.Random.Range(0.3f, 3f);
                }

                if (Actor.CurrentStateType == EStateType.Move)
                {
                    moveTimer -= Time.deltaTime;
                }

                yield return null;
            }
        }

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

            if (m_ListSkills.Count > 0)
            {
                m_CurrentSkill = m_ListSkills[UnityEngine.Random.Range(0, m_ListSkills.Count)];
                SwitchCoroutine(AttackItor());
            }
            else
            {
                // 找攻击距离最大的技能
                float maxAttackDistance = float.MinValue;
                m_CurrentSkill = null;
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
                            m_CurrentSkill = skill;
                        }
                    }
                }

                if (m_CurrentSkill == null)
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
                                m_CurrentSkill = skill;
                            }
                        }
                    }
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

        public override void OnEnterBossStageTwo()
        {
            base.OnEnterBossStageTwo();

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

        /// <summary>冲刺后攻击</summary>
        IEnumerator SprintAndAttackItor()
        {
            while (true)
            {
                Actor.StartMove(EMoveSpeedV.Sprint, new Vector3(0, 0, 1));
                if (m_DistanceToEnemy < m_CurrentSkill.m_AIPramAttackDistance.maxValue)
                {
                    SwitchCoroutine(AttackItor());
                    yield break;
                }

                yield return new WaitForSeconds(0.1f);
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
                }
                else if (!IsFaceToEnemy() || LockingEnemy == null || LockingEnemy.IsDead || LockingEnemy.IsInSpecialStun)
                {
                    ToStalemate();
                    yield break;
                }
                else
                {
                    int attackPercent = (int)Mathf.Lerp(10, 90, Monster.m_MonsterInfo.AttackDesireRatio);
                    if (CalcProbability(attackPercent))
                    {
                        ToAttack();
                        yield break;
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

        void ToDodge()
        {
            SwitchCoroutine(DodgeItor());
        }

        IEnumerator DodgeItor()
        {
            if (!Monster.m_BaseActorInfo.m_AIInfo.CanDodge)
            {
                throw new InvalidOperationException("Cann't dodge");
            }

            Vector3 axis;
            if (m_DistanceToEnemy < 2)
            {
                axis = new Vector3(0, 0, -1);
            }
            else
            {
                int v = UnityEngine.Random.Range(0, 100);
                if (v < 40)
                    axis = new Vector3(1, 0, 0);
                else if (v < 80)
                    axis = new Vector3(-1, 0, 0);
                else
                    axis = new Vector3(0, 0, -1);
            }

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

                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}