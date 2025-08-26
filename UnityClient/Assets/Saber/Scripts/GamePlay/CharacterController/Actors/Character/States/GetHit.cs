using System;
using System.Buffers;
using Saber.Frame;
using Saber.UI;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Saber.CharacterController
{
    public class GetHit : ActorStateBase, IHitRecovery
    {
        enum EState
        {
            Stun,
            BlockBroken,
            Executed,
            GetUpAfterExecuted,
        }

        enum EHitRecHurtType
        {
            Stun,
            BlockBroken,
        }


        private string m_CurAnim;
        private EHitRecHurtType m_HurtType;
        private EState m_State;
        private bool m_IsExecuteFromBack;
        private bool m_CanExecuteTimePassed;

        public DamageInfo Damage { get; set; }
        public override bool CanExit => false;
        public bool CanBeExecute { get; private set; }

        public bool IsBlockBrokenWaitExecute => m_State == EState.BlockBroken && !m_CanExecuteTimePassed;
        public bool IsBlockBrokenHurtType => m_HurtType == EHitRecHurtType.BlockBroken;
        public override bool ApplyRootMotionSetWhenEnter => true;


        public GetHit() : base(EStateType.GetHit)
        {
        }

        string GetHitAnim(out EHitRecHurtType hurtType)
        {
            hurtType = EHitRecHurtType.Stun;
            
            if (Damage.DamageConfig.m_HitRecover == EHitRecover.StunTanDao)
            {
                return "StunTanDao";
            }
            
            if (Damage.DamageConfig.m_HitRecover == EHitRecover.Backstab &&
                Vector3.Dot(Actor.transform.forward, Damage.Attacker.transform.forward) > 0 ||
                Actor.CStats.CurrentUnbalanceValue <= 0)
            {
                hurtType = EHitRecHurtType.BlockBroken;
                return "BlockBroken";
            }

            // stun
            //float angleFromAttacker = Vector3.SignedAngle(Damage.DamageDirection, Actor.transform.forward, Vector3.up);
            float angleFromAttacker = Vector3.SignedAngle(Damage.Attacker.transform.forward, Actor.transform.forward, Vector3.up);
            string dirStr;
            if (angleFromAttacker > -45 && angleFromAttacker <= 45)
                dirStr = "B";
            else if (angleFromAttacker > 45 && angleFromAttacker <= 135)
                dirStr = "R";
            else if (angleFromAttacker > -135 && angleFromAttacker <= -45)
                dirStr = "L";
            else
                dirStr = "F";
            return $"Stun{dirStr}";
        }

        public override void Enter()
        {
            base.Enter();
            OnEnter();
        }

        public override void ReEnter()
        {
            base.ReEnter();
            OnEnter();
        }

        void OnEnter()
        {
            // 受击动画
            m_CurAnim = GetHitAnim(out m_HurtType);
            Actor.CAnim.Play(m_CurAnim, force: true);

            CanBeExecute = false;
            m_CanExecuteTimePassed = false;

            m_State = m_HurtType switch
            {
                EHitRecHurtType.Stun => EState.Stun,
                EHitRecHurtType.BlockBroken => EState.BlockBroken,
                _ => throw new InvalidOperationException($"Unknown hurt type:{m_HurtType}"),
            };

            if (m_HurtType == EHitRecHurtType.BlockBroken)
            {
                Actor.CStats.DefaultUnbalanceValue();
            }
        }

        public override void OnStay()
        {
            base.OnStay();

            if (m_State == EState.Executed)
            {
                if (!Actor.CAnim.IsPlayingOrWillPlay(m_CurAnim, 0.95f))
                {
                    if (Actor.IsDead)
                    {
                        DieAfterExecuted();
                    }
                    else
                    {
                        GetUpAfterExecuted();
                    }
                }
            }
            else
            {
                if (!Actor.CAnim.IsPlayingOrWillPlay(m_CurAnim))
                {
                    Exit();
                }
            }
        }

        public override void OnTriggerRangeEvent(AnimRangeTimeEvent eventObj, bool enter)
        {
            base.OnTriggerRangeEvent(eventObj, enter);
            if (eventObj.EventType == EAnimRangeEvent.CanBeExecute)
            {
                CanBeExecute = enter;
                if (!enter)
                {
                    m_CanExecuteTimePassed = true;
                }
            }
        }

        public void BeExecuted(SActor executioner)
        {
            m_State = EState.Executed;

            Vector3 dirFromExe = Actor.transform.position - executioner.transform.position;
            m_IsExecuteFromBack = Vector3.Dot(Actor.transform.forward, dirFromExe) > 0;

            m_CurAnim = m_IsExecuteFromBack ? "ExecutedBack" : "ExecutedFront";
            Actor.CAnim.Play(m_CurAnim, force: true);
            CanBeExecute = false;

            Actor.CPhysic.ApplyRootMotion = true;
        }

        void GetUpAfterExecuted()
        {
            m_State = EState.GetUpAfterExecuted;
            m_CurAnim = m_IsExecuteFromBack ? "ExecutedBackUp" : "ExecutedFrontUp";
            Actor.CAnim.Play(m_CurAnim, force: true);

            Actor.CStats.DefaultUnbalanceValue();
        }

        void DieAfterExecuted()
        {
            m_CurAnim = m_IsExecuteFromBack ? "ExecutedBackDownDie" : "ExecutedFrontDownDie";
            Actor.CAnim.Play(m_CurAnim, force: true);
            Exit();
        }
    }

    public interface IHitRecovery
    {
        bool CanBeExecute { get; }
        void BeExecuted(SActor executioner);
        bool IsBlockBrokenWaitExecute { get; }
        bool IsBlockBrokenHurtType { get; }
    }
}