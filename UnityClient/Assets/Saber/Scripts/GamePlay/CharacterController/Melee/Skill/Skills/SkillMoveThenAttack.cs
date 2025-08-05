using System;
using System.Collections.Generic;
using System.Linq;
using CombatEditor;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    /// <summary>移动后攻击</summary>
    public class SkillMoveThenAttack : SkillCommon
    {
        private bool m_Attacked;
        private float m_Timer;

        public SkillMoveThenAttack(SActor actor, SkillItem skillConfig) : base(actor, skillConfig)
        {
        }

        public override void Enter()
        {
            base.Enter();
            m_Attacked = false;
            m_Timer = 3;
        }

        public override void OnStay()
        {
            base.OnStay();

            if (!m_Attacked && Actor.CAnim.IsPlayingOrWillPlay(base.SkillConfig.m_AnimStates[1].m_Name))
            {
                Vector3 dir;
                if (Actor.AI.LockingEnemy != null)
                {
                    dir = Actor.AI.LockingEnemy.transform.position - Actor.transform.position;
                    if (dir.magnitude < SkillConfig.m_AttackTriggerDistance)
                    {
                        Actor.CAnim.Play(base.SkillConfig.m_AnimStates.Last().m_Name);
                        m_Attacked = true;
                    }
                }
                else
                {
                    dir = Actor.transform.forward;
                    if (m_Timer > 0)
                    {
                        m_Timer -= Actor.DeltaTime;
                        if (m_Timer <= 0)
                        {
                            Actor.CAnim.Play(base.SkillConfig.m_AnimStates.Last().m_Name);
                            m_Attacked = true;
                        }
                    }
                }

                Actor.CPhysic.AlignForwardTo(dir, 360);
            }
        }
    }
}