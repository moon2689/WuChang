using System;
using System.Collections.Generic;
using System.Linq;
using CombatEditor;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    /// <summary>蓄力重击</summary>
    public class SkillHeavyAttack : SkillCommon
    {
        public interface IHandler
        {
            bool IsPressingKey { get; }
        }

        private IHandler m_IHandler;
        private bool m_SlowingAnim;
        private float m_SlowAnimSpeed;
        private float m_TimerPressing;
        private float m_TimerAlign;

        public SkillHeavyAttack(SActor actor, SkillItem skillConfig, IHandler handler) : base(actor, skillConfig)
        {
            m_IHandler = handler;
        }

        public override void OnTriggerRangeEvent(AnimRangeTimeEvent eventObj, bool enter)
        {
            base.OnTriggerRangeEvent(eventObj, enter);
            if (eventObj.EventType == EAnimRangeEvent.HeavyAttackSlowAnim)
            {
                m_SlowingAnim = enter;
                m_SlowAnimSpeed = ((AnimEvent_HeavyAttackSlowAnim)eventObj).m_SlowAnimSpeed;
            }
        }

        public override void Enter()
        {
            base.Enter();
            base.Actor.TimeMultiplier = 1;
            m_TimerPressing = 1f;
            m_SlowingAnim = false;
            m_TimerAlign = 0.2f;
        }

        public override void Exit()
        {
            base.Exit();
            base.Actor.TimeMultiplier = 1;
        }

        public override void OnStay()
        {
            base.OnStay();
            if (m_TimerAlign > 0)
            {
                m_TimerAlign -= Time.deltaTime;
                if (Actor.AI.LockingEnemy != null)
                    Actor.CPhysic.AlignForwardTo(Actor.DesiredLookDir, 1080f);
            }

            if (m_SlowingAnim && m_IHandler.IsPressingKey)
            {
                base.Actor.TimeMultiplier = m_SlowAnimSpeed;
                m_TimerPressing -= Actor.DeltaTime;

                if (m_TimerPressing <= 0)
                {
                    TriggerNextSkill();
                }
            }
            else
            {
                base.Actor.TimeMultiplier = 1;
            }

            if (Actor.CAnim.GetAnimNormalizedTime(0) >= 0.95f)
            {
                TriggerNextSkill();
            }
        }

        void TriggerNextSkill()
        {
            base.Actor.TimeMultiplier = 1;
            int animIndex = m_TimerPressing <= 0 ? 2 : 1;
            string animName = SkillConfig.m_AnimStates[animIndex].m_Name;
            Actor.CAnim.Play(animName);
        }
    }
}