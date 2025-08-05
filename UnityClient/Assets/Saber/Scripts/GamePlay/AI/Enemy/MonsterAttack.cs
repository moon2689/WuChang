using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Saber.CharacterController;

namespace Saber.AI
{
    public class MonsterAttack : EnemyAIBase
    {
        private List<SkillItem> m_ListSkills = new();


        protected override void OnStart()
        {
            base.OnStart();
            SwitchCoroutine(SearchEnemy());
        }

        protected override void OnFoundEnemy(EFoundEnemyType foundType)
        {
            if (Actor.CAnim.HasAnim("Startled", 0))
            {
                SwitchCoroutine(StartledItor());
            }
            else
            {
                ToAttack();
            }
        }

        IEnumerator StartledItor()
        {
            while (true)
            {
                Vector3 dirToEnemy = base.LockingEnemy.transform.position - Actor.transform.position;
                if (Vector3.Angle(dirToEnemy, Actor.transform.forward) < Actor.m_BaseActorInfo.m_AIInfo.m_WarningRange)
                    break;

                Actor.DesiredLookDir = Vector3.ProjectOnPlane(dirToEnemy, Vector3.up).normalized;
                Actor.StartMove(EMoveSpeedV.Walk, new Vector3(0, 0, -1));
                yield return null;
            }

            yield return new WaitForSeconds(1);

            Actor.StopMove();

            while (Actor.CurrentStateType != EStateType.Idle)
            {
                yield return null;
            }

            bool startledFinished = false;
            Actor.CStateMachine.Startled(() => startledFinished = true);
            while (!startledFinished)
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.2f);

            while (true)
            {
                Vector3 dirToEnemy = base.LockingEnemy.transform.position - Actor.transform.position;
                if (Vector3.Angle(dirToEnemy, Actor.transform.forward) < Actor.m_BaseActorInfo.m_AIInfo.m_WarningRange)
                    break;

                Actor.DesiredLookDir = Vector3.ProjectOnPlane(dirToEnemy, Vector3.up).normalized;
                Actor.StartMove(EMoveSpeedV.Walk, new Vector3(0, 0, -1));
                yield return null;
            }

            Actor.StopMove();

            ToAttack();
        }

        void ToAttack()
        {
            SwitchCoroutine(AttackItor());
        }


        IEnumerator AttackItor()
        {
            float timer = 0;
            BaseSkill currentSkill = null;
            Vector3 moveAxis = GetMoveAxisByDis(m_DistanceToEnemy);
            while (true)
            {
                bool triggeredSkill = false;
                if (Actor.CurrentStateType == EStateType.Skill)
                {
                    if (LockingEnemy.CurrentStateType == EStateType.GetHit &&
                        currentSkill != null &&
                        currentSkill.IsTriggering &&
                        currentSkill.InComboTime &&
                        currentSkill.SkillConfig.m_ChainSkillIDs.Length > 0)
                    {
                        int randomIndex = UnityEngine.Random.Range(0, currentSkill.SkillConfig.m_ChainSkillIDs.Length);
                        int tarSkillItemID = currentSkill.SkillConfig.m_ChainSkillIDs[randomIndex];
                        SkillItem tarSkillItem = Actor.CMelee.SkillConfig.GetSkillItemByID(tarSkillItemID);
                        if (tarSkillItem.InRange(m_DistanceToEnemy))
                        {
                            Actor.StopMove();
                            triggeredSkill = Actor.TryTriggerSkill(tarSkillItem);
                            if (triggeredSkill)
                            {
                                currentSkill = Actor.CurrentSkill;
                                timer = UnityEngine.Random.Range(0, 3);
                                moveAxis = GetMoveAxisByDis(m_DistanceToEnemy);
                            }
                        }
                    }
                }
                else
                {
                    timer -= Actor.DeltaTime;
                    if (timer > 0)
                    {
                        Actor.StartMove(EMoveSpeedV.Walk, moveAxis);
                    }
                    else
                    {
                        if (CalcProbability(20))
                        {
                            if (m_DistanceToEnemy > 2)
                                SwitchCoroutine(WanderItor());
                            else
                                SwitchCoroutine(MoveBackItor());
                            yield break;
                        }

                        m_ListSkills.Clear();
                        foreach (var skill in Actor.Skills)
                        {
                            if (skill.m_FirstSkillOfCombo && skill.InRange(m_DistanceToEnemy))
                            {
                                m_ListSkills.Add(skill);
                            }
                        }

                        if (m_ListSkills.Count > 0)
                        {
                            Actor.StopMove();
                            int ranIndex = UnityEngine.Random.Range(0, m_ListSkills.Count);
                            SkillItem tarSkill = m_ListSkills[ranIndex];
                            triggeredSkill = Actor.TryTriggerSkill(tarSkill);
                            if (triggeredSkill)
                            {
                                currentSkill = Actor.CurrentSkill;
                                timer = UnityEngine.Random.Range(0, 3);
                                moveAxis = GetMoveAxisByDis(m_DistanceToEnemy);
                            }
                        }
                    }
                }

                yield return null;
            }
        }

        IEnumerator WanderItor()
        {
            float timer = UnityEngine.Random.Range(1f, 3f);
            Vector3 moveAxis = GetMoveAxisByDis(m_DistanceToEnemy);
            EMoveSpeedV speedV = EMoveSpeedV.Run;

            while (true)
            {
                if (m_DistanceToEnemy < 2.8f)
                {
                    ToAttack();
                    yield break;
                }

                timer -= Time.deltaTime;
                if (timer < 0)
                {
                    if (CalcProbability(80))
                    {
                        ToAttack();
                        yield break;
                    }

                    moveAxis = GetMoveAxisByDis(m_DistanceToEnemy);
                    speedV = CalcProbability(50) ? EMoveSpeedV.Run : EMoveSpeedV.Walk;
                    timer = UnityEngine.Random.Range(1f, 3f);
                }

                Actor.StartMove(speedV, moveAxis);

                yield return null;
            }
        }

        Vector3 GetMoveAxisByDis(float dis)
        {
            if (dis > 8)
            {
                return new Vector3(0, 0, 1);
            }
            else if (dis < Actor.CPhysic.Radius + LockingEnemy.CPhysic.Radius + 0.1f)
            {
                return new Vector3(0, 0, -1);
            }
            else
            {
                float randValue = Random.Range(0, 100);
                if (randValue < 33)
                {
                    return new Vector3(0, 0, -1);
                }
                else if (randValue < 66)
                {
                    return new Vector3(0, 0, 1);
                }
                else if (randValue < 83)
                {
                    return new Vector3(-1, 0, 0);
                }
                else
                {
                    return new Vector3(1, 0, 0);
                }
            }
        }

        IEnumerator MoveBackItor()
        {
            float timer = UnityEngine.Random.Range(0.5f, 2f);

            while (true)
            {
                if (base.LockingEnemy == null)
                {
                    SwitchCoroutine(SearchEnemy());
                    yield break;
                }

                if (LockingEnemy.IsDead)
                {
                    SwitchCoroutine(SearchEnemy());
                    yield break;
                }

                Vector3 dirToEnemy = base.LockingEnemy.transform.position - Actor.transform.position;
                float dis = dirToEnemy.magnitude;

                if (dis > Actor.m_BaseActorInfo.m_AIInfo.m_WarningRange)
                {
                    SwitchCoroutine(SearchEnemy());
                    yield break;
                }

                Actor.DesiredLookDir = Vector3.ProjectOnPlane(dirToEnemy, Vector3.up).normalized;

                timer -= Time.deltaTime;
                if (timer < 0)
                {
                    if (CalcProbability(80))
                    {
                        ToAttack();
                    }
                    else
                    {
                        SwitchCoroutine(WanderItor());
                    }

                    yield break;
                }

                Actor.StartMove(EMoveSpeedV.Run, new Vector3(0, 0, -1));
                yield return null;
            }
        }
    }
}