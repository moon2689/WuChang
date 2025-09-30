using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Saber.CharacterController;
using UnityEngine.Rendering.UI;

namespace Saber.AI
{
    public class SimpleAttack : EnemyAIBase
    {
        private List<SkillItem> m_ListSkills = new();
        private int m_LastSkillID;
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
            SwitchCoroutine(OnFoundEnemyItor(foundType));
        }

        IEnumerator OnFoundEnemyItor(EFoundEnemyType foundType)
        {
            Actor.StopMove();
            while (Actor.CurrentStateType != EStateType.Idle)
            {
                yield return null;
            }

            /*
            bool wait;
            if (foundType == EFoundEnemyType.NotInStealth)
            {
                wait = true;
                Actor.CStateMachine.PlayAction_LookAround(() => wait = false);
                while (wait)
                {
                    yield return null;
                }
            }

            wait = true;
            Actor.CStateMachine.PlayAction_TurnDirection(LockingEnemy.transform.position, () => wait = false);
            while (wait)
            {
                yield return null;
            }
            */

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

                // 攻击
                timerStay -= Time.deltaTime;
                if (timerStay < 0)
                {
                    // if (CalcProbability(20))
                    //     ToJump();
                    // else
                        ToAttack();
                    yield break;
                }

                // 如果敌人攻击中，则格挡或闪避
                if (LockingEnemy.CurrentStateType == EStateType.Skill)
                {
                    if (m_DistanceToEnemy < LockingEnemy.CurrentSkill.SkillConfig.m_AIPramAttackDistance.maxValue)
                    {
                        // if (Monster.m_MonsterInfo.m_CanJump)
                        // {
                        //     ToJump();
                        //     yield break;
                        // }
                    }
                }

                // 随机游走
                if (m_DistanceToEnemy > 5)
                {
                    Actor.StartMove(EMoveSpeedV.Walk, new Vector3(0, 0, 1));
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

        void ToAttack()
        {
            if (LockingEnemy.IsDead)
            {
                ToStalemate();
                return;
            }

            m_CurrentSkill = null;
            foreach (var skill in Actor.Skills)
            {
                if (skill.m_FirstSkillOfCombo && skill.InRange(m_DistanceToEnemy) && m_LastSkillID != skill.m_ID &&
                    Actor.CMelee[skill.m_ID].IsCDCooldown)
                {
                    m_CurrentSkill = skill;
                    break;
                }
            }

            if (m_CurrentSkill != null)
            {
                SwitchCoroutine(AttackItor());
            }
            else
            {
                m_ListSkills.Clear();
                foreach (var skill in Actor.Skills)
                {
                    if (skill.m_FirstSkillOfCombo && m_LastSkillID != skill.m_ID &&
                        Actor.CMelee[skill.m_ID].IsCDCooldown)
                    {
                        m_ListSkills.Add(skill);
                    }
                }

                if (m_ListSkills.Count == 0)
                {
                    foreach (var skill in Actor.Skills)
                    {
                        if (skill.m_FirstSkillOfCombo)
                        {
                            m_ListSkills.Add(skill);
                        }
                    }
                }

                m_CurrentSkill = m_ListSkills[UnityEngine.Random.Range(0, m_ListSkills.Count)];
                SwitchCoroutine(SprintAndAttackItor());
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
                        m_LastSkillID = m_CurrentSkill.m_ID;
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
                /*
                else if (CalcProbability(60))
                {
                    ToAttack();
                    yield break;
                }
                */
                else
                {
                    ToStalemate();
                    yield break;
                }

                yield return null;
            }
        }
    }
}