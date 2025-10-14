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
        private bool m_CanExit;
        private float m_AngleFromAttacker;

        public DamageInfo Damage { get; set; }
        public override bool CanExit => m_CanExit;
        public bool CanBeExecute { get; private set; }

        public bool IsBlockBrokenWaitExecute => m_State == EState.BlockBroken && !m_CanExecuteTimePassed;
        public EHitRecHurtType HitRecHurtType => m_HurtType;
        public override bool ApplyRootMotionSetWhenEnter => true;


        public static bool ToBlockBroken(DamageInfo dmg, SActor hurtedActor)
        {
            return dmg.DamageConfig.m_HitRecover == EHitRecover.Backstab &&
                   (int)GameApp.Entry.Config.SkillCommon.BackStabPower > (int)hurtedActor.CurrentResilience &&
                   Vector3.Dot(hurtedActor.GetNodeTransform(ENodeType.Back).right, dmg.Attacker.transform.forward) > 0 ||
                   !hurtedActor.IsPlayer && hurtedActor.CStats.CurrentUnbalanceValue <= 0;
        }

        string GetDir4()
        {
            m_AngleFromAttacker = Vector3.SignedAngle(Damage.Attacker.transform.forward, Actor.transform.forward, Vector3.up);
            if (m_AngleFromAttacker > -45 && m_AngleFromAttacker <= 45)
                return "B";
            else if (m_AngleFromAttacker > 45 && m_AngleFromAttacker <= 135)
                return "R";
            else if (m_AngleFromAttacker > -135 && m_AngleFromAttacker <= -45)
                return "L";
            else
                return "F";
        }

        string GetDir2(out Vector3 faceDir)
        {
            m_AngleFromAttacker = Vector3.SignedAngle(Damage.Attacker.transform.forward, Actor.transform.forward, Vector3.up);
            bool isBack = m_AngleFromAttacker > -90 && m_AngleFromAttacker <= 90;

            Vector3 directionToAttacker = Damage.Attacker.transform.position - Actor.transform.position;
            directionToAttacker.y = 0;
            faceDir = isBack ? -directionToAttacker : directionToAttacker;

            return isBack ? "B" : "F";
        }


        public GetHit() : base(EStateType.GetHit)
        {
        }

        string GetHitAnim(out EHitRecHurtType hurtType, out Vector3 faceDir)
        {
            hurtType = EHitRecHurtType.Stun;
            faceDir = Vector3.zero;

            EHitRecover hitRec = Damage.DamageConfig.m_HitRecover;
            var hitRecInfo = Actor.m_BaseActorInfo.m_HitRecInfo;
            if (hitRec == EHitRecover.StunTanDao)
            {
                return "StunTanDao";
            }

            if (hitRecInfo.CanBeBackstab && ToBlockBroken(Damage, Actor))
            {
                hurtType = EHitRecHurtType.BlockBroken;
                return "BlockBroken";
            }

            if (hitRecInfo.CanBeStrikeDown && hitRec == EHitRecover.StrikeDown)
            {
                hurtType = EHitRecHurtType.SpecialStun;
                return $"StrikeDown{GetDir2(out faceDir)}";
            }

            if (hitRecInfo.CanBeKnockOffLongDis && (hitRec == EHitRecover.KnockOffLongDis || hitRec == EHitRecover.Backstab))
            {
                hurtType = EHitRecHurtType.SpecialStun;
                return $"KnockOffLongDis{GetDir2(out faceDir)}";
            }

            if (hitRecInfo.CanBeStunLarge && hitRec == EHitRecover.StunLarge)
            {
                return $"StunLarge{GetDir2(out faceDir)}";
            }

            if (hitRecInfo.CanBeUppercut && hitRec == EHitRecover.Uppercut)
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
            m_CurAnim = GetHitAnim(out m_HurtType, out Vector3 faceDir);
            Actor.CAnim.Play(m_CurAnim, force: true);

            if (m_HurtType == EHitRecHurtType.BlockBroken)
            {
                GameApp.Entry.Game.Audio.Play3DSound(GameApp.Entry.Config.SkillCommon.BlockBrokenSound, Actor.transform.position);
            }

            CanBeExecute = false;
            m_CanExecuteTimePassed = false;
            m_CanExit = false;

            m_State = m_HurtType switch
            {
                EHitRecHurtType.BlockBroken => EState.BlockBroken,
                _ => EState.Stun,
            };

            if (m_HurtType == EHitRecHurtType.BlockBroken)
            {
                Actor.CStats.DefaultUnbalanceValue();
            }

            // 对齐攻击者方向
            if (faceDir != Vector3.zero)
            {
                Actor.transform.rotation = Quaternion.LookRotation(faceDir);
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

        public override void OnTriggerAnimEvent(AnimPointTimeEvent eventObj)
        {
            base.OnTriggerAnimEvent(eventObj);
            if (eventObj.EventType == EAnimTriggerEvent.AnimCanExit)
            {
                m_CanExit = true;
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
            Exit();
            string dieAnim = m_IsExecuteFromBack ? "ExecutedBackDownDie" : "ExecutedFrontDownDie";
            Actor.Die(dieAnim);
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