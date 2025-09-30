using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Saber.CharacterController;

namespace Saber.AI
{
    public class BossAttack : EnemyAIBase
    {
        private List<SkillItem> m_ListSkills = new();
        private Queue<SkillItem> m_LastSkill = new();
        private SkillItem m_CurrentSkill;


        protected override void OnStart()
        {
            base.OnStart();
            SwitchCoroutine(SearchEnemy());
        }

        void ToStalemate()
        {
            SwitchCoroutine(StalemateItor());
        }

        protected override void OnFoundEnemy(EFoundEnemyType foundType)
        {
            ToStalemate();
        }

        IEnumerator MoveToOriginPosItor(Action onReached)
        {
            LockingEnemy = null;
            float timer = 0;

            while (true)
            {
                Vector3 dir = m_OriginPos - Actor.transform.position;

                if (dir.magnitude < 1)
                {
                    onReached?.Invoke();
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
            float timerStay = UnityEngine.Random.Range(0, 3);

            while (true)
            {
                // 敌人死亡或敌人距离过远，则返回出生点
                if (base.LockingEnemy == null || LockingEnemy.IsDead ||
                    m_DistanceToEnemy > Actor.m_BaseActorInfo.m_AIInfo.m_WarningRange * 1.2f)
                {
                    Actor.StopMove();
                    yield return new WaitForSeconds(3);
                    SwitchCoroutine(MoveToOriginPosItor(ToSearchEnemy));
                    yield break;
                }

                /*
                // 面向玩家
                if (!IsFaceToEnemy())
                {
                    bool wait = true;
                    PlayActionState.EActionType turnDirAction = CalcProbability(50) ? PlayActionState.EActionType.TurnL180 : PlayActionState.EActionType.TurnR180;
                    if (Actor.PlayAction(turnDirAction, () => wait = false))
                    {
                        while (wait && !IsAlighingToEnemy())
                        {
                            yield return null;
                        }
                    }
                }
                */

                // 攻击
                timerStay -= Time.deltaTime;
                if (timerStay < 0 && !LockingEnemy.IsInSpecialStun)
                {
                    if (CalcProbability(20))
                        ToDodge();
                    else
                        ToAttack();
                    yield break;
                }

                // 如果敌人攻击中，则格挡或闪避
                if (LockingEnemy.CurrentStateType == EStateType.Skill && Monster.m_MonsterInfo.m_CanDodge)
                {
                    if (m_DistanceToEnemy < LockingEnemy.CurrentSkill.SkillConfig.m_AIPramAttackDistance.maxValue)
                    {
                        ToDodge();
                        yield break;
                    }
                }

                // 随机游走
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
                else if (Actor.CurrentStateType == EStateType.Idle)
                {
                    Vector3 axis = CalcProbability(50) ? new Vector3(1, 0, 0) : new Vector3(-1, 0, 0);
                    Actor.StartMove(EMoveSpeedV.Walk, axis);
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
            else if (skill.m_AITriggerCondition == EAITriggerSkillCondition.HPHalf)
            {
                return Actor.CStats.CurrentHp <= Actor.CStats.MaxHp * 0.5f;
            }
            else
            {
                Debug.LogError($"Unknown condition:{skill.m_AITriggerCondition}");
                return false;
            }
        }

        SkillItem GetCanTriggerSpecialSkill()
        {
            foreach (SkillItem skill in Actor.Skills)
            {
                if (skill.m_FirstSkillOfCombo &&
                    IsCDColldown(skill) &&
                    skill.m_AITriggerCondition != EAITriggerSkillCondition.None &&
                    SatifyTriggerCondition(skill))
                {
                    return skill;
                }
            }

            return null;
        }

        void ToAttack()
        {
            if (LockingEnemy.IsDead)
            {
                ToStalemate();
                return;
            }

            SkillItem specialSkill = GetCanTriggerSpecialSkill();
            if (specialSkill != null)
            {
                m_CurrentSkill = specialSkill;
                SwitchCoroutine(SprintAndAttackItor());
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

        /// <summary>冲刺后攻击</summary>
        IEnumerator SprintAndAttackItor()
        {
            while (true)
            {
                // 敌人死亡或敌人距离过远，则返回出生点
                if (base.LockingEnemy == null || LockingEnemy.IsDead ||
                    m_DistanceToEnemy > Actor.m_BaseActorInfo.m_AIInfo.m_WarningRange * 1.2f)
                {
                    Actor.StopMove();
                    yield return new WaitForSeconds(3);
                    SwitchCoroutine(MoveToOriginPosItor(ToSearchEnemy));
                    yield break;
                }

                Actor.StartMove(EMoveSpeedV.Sprint, new Vector3(0, 0, 1));

                if (m_DistanceToEnemy < m_CurrentSkill.m_AIPramAttackDistance.maxValue)
                {
                    Actor.StopMove();
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
            return Vector3.Dot(LockingEnemy.transform.position - Actor.transform.position, Actor.transform.forward) > 0;
        }

        bool IsAlighingToEnemy()
        {
            return Vector3.Angle(LockingEnemy.transform.position - Actor.transform.position, Actor.transform.forward) < 10;
        }

        /// <summary>攻击</summary>
        IEnumerator AttackItor()
        {
            Actor.StopMove();
            bool triggered = false;
            while (true)
            {
                if (LockingEnemy == null || LockingEnemy.IsDead)
                {
                    ToStalemate();
                    yield break;
                }
                else if (LockingEnemy.IsInSpecialStun)
                {
                    ToStalemate();
                    yield break;
                }
                else if (!triggered)
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
                        var specialSkill = GetCanTriggerSpecialSkill();
                        if (specialSkill != null)
                        {
                            m_CurrentSkill = specialSkill;
                            SwitchCoroutine(SprintAndAttackItor());
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
                }
                /*
                else if (!IsFaceToEnemy())
                {
                    ToStalemate();
                    yield break;
                }
                */
                else if (CalcProbability(90))
                {
                    ToAttack();
                    yield break;
                }
                else
                {
                    ToStalemate();
                    yield break;
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
            if (!Monster.m_MonsterInfo.m_CanDodge)
            {
                ToStalemate();
                yield break;
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