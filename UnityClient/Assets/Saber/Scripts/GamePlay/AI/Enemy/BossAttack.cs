using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Saber.CharacterController;

namespace Saber.AI
{
    public class BossAttack : EnemyAIBase
    {
        /*
        private List<SkillItem> m_ListSkills = new();
        private int m_LastSkillID;
        private SkillItem m_CurrentSkill;

        protected override void OnStart()
        {
            base.OnStart();
            ToSearchEnemy();
        }

        protected override void OnFoundEnemy(EFoundEnemyType foundType)
        {
            ToStalemate();
        }

        bool DodgeToOriginPos()
        {
            if (Actor.CurrentStateType == EStateType.Dodge)
            {
                return true;
            }

            Vector3 dir = m_OriginPos - Actor.transform.position;
            dir.y = 0;

            if (dir.magnitude > 5)
            {
                var edir4 = dir.Calc4Dir(Actor.transform.forward, out _);
                Vector3 axis = edir4 switch
                {
                    GameHelper.EDir4.Back => new Vector3(0, 0, -1),
                    GameHelper.EDir4.Front => new Vector3(0, 0, 1),
                    GameHelper.EDir4.Left => new Vector3(-1, 0, 0),
                    GameHelper.EDir4.Right => new Vector3(1, 0, 0),
                    _ => new Vector3(0, 0, -1),
                };
                return Actor.Dodge(axis);
            }

            return false;
        }

        IEnumerator MoveToOriginPosItor(bool dodgeFirst, Action onReached)
        {
            if (dodgeFirst && DodgeToOriginPos())
            {
                yield return new WaitForSeconds(1);
            }

            while (true)
            {
                if (LockingEnemy != null && LockingEnemy.CurrentStateType == EStateType.Skill && m_DistanceToEnemy < 3)
                {
                    ToStalemate();
                    yield break;
                }

                Vector3 dir = m_OriginPos - Actor.transform.position;
                dir.y = 0;
                dir.Calc4Dir(Actor.transform.forward, out float angle);
                Vector3 axis = Quaternion.Euler(0, angle, 0) * new Vector3(0, 0, 1);
                EMoveSpeedV speedV;

                if (dir.magnitude > 3)
                {
                    speedV = EMoveSpeedV.Run;
                }
                else if (dir.magnitude > 1)
                {
                    speedV = EMoveSpeedV.Walk;
                }
                else
                {
                    onReached?.Invoke();
                    yield break;
                }

                Actor.StartMove(speedV, axis);
                yield return null;
            }
        }

        void ToStalemate()
        {
            SwitchCoroutine(StalemateItor());
        }

        void ToDodge()
        {
            SwitchCoroutine(DodgeItor());
        }

        // 对峙
        IEnumerator StalemateItor()
        {
            // 如果离开出生点太远，则返回
            Vector3 distanceToOriginPos = m_OriginPos - Actor.transform.position;
            if (distanceToOriginPos.magnitude > Actor.m_BaseActorInfo.m_AIInfo.m_WarningRange)
            {
                SwitchCoroutine(MoveToOriginPosItor(true, ToStalemate));
                yield break;
            }

            // 敌人死亡或敌人距离过远，则返回出生点
            if (base.LockingEnemy == null || LockingEnemy.IsDead ||
                m_DistanceToEnemy > Actor.m_BaseActorInfo.m_AIInfo.m_WarningRange * 1.2f)
            {
                yield return new WaitForSeconds(3);
                SwitchCoroutine(MoveToOriginPosItor(false, ToSearchEnemy));
                yield break;
            }

            float timerStay = UnityEngine.Random.Range(0, 3);

            while (true)
            {
                // 攻击
                timerStay -= Time.deltaTime;
                if (timerStay < 0)
                {
                    if (CalcProbability(10))
                        ToDodge();
                    else
                        ToAttack();
                    yield break;
                }

                // 如果敌人攻击中，则格挡或闪避
                if (LockingEnemy.CurrentStateType == EStateType.Skill)
                {
                    if (m_DistanceToEnemy < LockingEnemy.CurrentSkill.SkillConfig.m_AIPramAttackDistance.maxValue)
                    {
                        if (Monster.m_MonsterInfo.m_CanDefense && CalcProbability(70))
                        {
                            SwitchCoroutine(DefenseItor());
                            yield break;
                        }
                        else if (Monster.m_MonsterInfo.m_CanDodge)
                        {
                            ToDodge();
                            yield break;
                        }
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

        IEnumerator DefenseItor()
        {
            if (!Monster.m_MonsterInfo.m_CanDefense)
            {
                ToStalemate();
                yield break;
            }

            while (true)
            {
                if (LockingEnemy.CurrentStateType != EStateType.Skill)
                {
                    yield return new WaitForSeconds(0.5f);
                    if (Actor.DefenseEnd())
                    {
                        ToStalemate();
                        yield break;
                    }
                }

                Vector3 dirToEnemy = LockingEnemy.transform.position - Monster.transform.position;
                if (Vector3.Dot(dirToEnemy, Monster.transform.forward) < 0)
                {
                    if (Actor.DefenseEnd())
                    {
                        ToDodge();
                        yield break;
                    }
                }

                if (Actor.CurrentStateType != EStateType.Defense)
                {
                    Actor.DefenseStart();
                }

                yield return new WaitForSeconds(0.1f);
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
                if (skill.m_FirstSkillOfCombo && skill.InRange(m_DistanceToEnemy) && m_LastSkillID != skill.m_ID)
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
                    if (skill.m_FirstSkillOfCombo && m_LastSkillID != skill.m_ID)
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
                    if (Actor.CurrentSkill.SkillConfig.m_ChainSkillIDs.Length > 0 && Actor.CurrentSkill.InComboTime)
                    {
                        m_ListSkills.Clear();
                        foreach (var chainID in Actor.CurrentSkill.SkillConfig.m_ChainSkillIDs)
                        {
                            SkillItem skillConfig = Actor.CMelee.SkillConfig.GetSkillItemByID(chainID);
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
                else if (CalcProbability(60))
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
        */


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
                    if (CalcProbability(20))
                        ToDodge();
                    else
                        ToAttack();
                    yield break;
                }

                // 如果敌人攻击中，则格挡或闪避
                if (LockingEnemy.CurrentStateType == EStateType.Skill)
                {
                    if (m_DistanceToEnemy < LockingEnemy.CurrentSkill.SkillConfig.m_AIPramAttackDistance.maxValue)
                    {
                        if (Monster.m_MonsterInfo.m_CanDodge)
                        {
                            ToDodge();
                            yield break;
                        }
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
                if (LockingEnemy.IsDead)
                {
                    ToStalemate();
                    yield break;
                }
                else if (!triggered)
                {
                    if (Actor.TryTriggerSkill(m_CurrentSkill))
                    {
                        triggered = true;
                        m_LastSkillID = m_CurrentSkill.m_ID;
                    }
                }
                else if (Actor.CurrentStateType == EStateType.Skill)
                {
                    if (Actor.CurrentSkill.SkillConfig.m_ChainSkillIDs.Length > 0 && Actor.CurrentSkill.InComboTime)
                    {
                        m_ListSkills.Clear();
                        foreach (var chainID in Actor.CurrentSkill.SkillConfig.m_ChainSkillIDs)
                        {
                            SkillItem skillConfig = Actor.CMelee.SkillConfig.GetSkillItemByID(chainID);
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
                else if (CalcProbability(60))
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