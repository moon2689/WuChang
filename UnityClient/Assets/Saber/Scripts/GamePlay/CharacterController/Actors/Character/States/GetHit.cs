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

        public enum EHitRecHurtType
        {
            Stun,
            BlockBroken,
            SpecialStun,
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
        public EHitRecHurtType HitRecHurtType => m_HurtType;
        public override bool ApplyRootMotionSetWhenEnter => true;


        public static bool ToBlockBroken(DamageInfo dmg, SActor hurtedActor)
        {
            return dmg.DamageConfig.m_HitRecover == EHitRecover.Backstab &&
                   Vector3.Dot(hurtedActor.transform.forward, dmg.Attacker.transform.forward) > 0 ||
                   hurtedActor.CStats.CurrentUnbalanceValue <= 0;
        }

        string GetDir4()
        {
            float angleFromAttacker = Vector3.SignedAngle(Damage.Attacker.transform.forward, Actor.transform.forward, Vector3.up);
            if (angleFromAttacker > -45 && angleFromAttacker <= 45)
                return "B";
            else if (angleFromAttacker > 45 && angleFromAttacker <= 135)
                return "R";
            else if (angleFromAttacker > -135 && angleFromAttacker <= -45)
                return "L";
            else
                return "F";
        }

        string GetDir2()
        {
            float angleFromAttacker = Vector3.SignedAngle(Damage.Attacker.transform.forward, Actor.transform.forward, Vector3.up);
            return angleFromAttacker > -90 && angleFromAttacker <= 90 ? "B" : "F";
        }


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

            if (ToBlockBroken(Damage, Actor))
            {
                hurtType = EHitRecHurtType.BlockBroken;
                return "BlockBroken";
            }

            if (Damage.DamageConfig.m_HitRecover == EHitRecover.StrikeDown)
            {
                hurtType = EHitRecHurtType.SpecialStun;
                return $"StrikeDown{GetDir2()}";
            }

            if (Damage.DamageConfig.m_HitRecover == EHitRecover.KnockOffLongDis)
            {
                hurtType = EHitRecHurtType.SpecialStun;
                return $"KnockOffLongDis{GetDir2()}";
            }

            if (Damage.DamageConfig.m_HitRecover == EHitRecover.Uppercut)
            {
                hurtType = EHitRecHurtType.SpecialStun;
                return "Uppercut";
            }

            // stun
            return $"Stun{GetDir4()}";
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
                EHitRecHurtType.BlockBroken => EState.BlockBroken,
                _ => EState.Stun,
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
                if (Actor.CAnim.IsPlayingOrWillPlay("Idle"))
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
        public GetHit.EHitRecHurtType HitRecHurtType { get; }
    }
}