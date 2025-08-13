using System;
using System.Buffers;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class MonsterGetHit : ActorStateBase, IHitRecovery
    {
        enum EState
        {
            Stun,
            BlockBroken,
            Executed,
            GetUpAfterExecuted,
        }


        private string m_CurAnim;
        private float m_AngleFromAttacker;
        private EHitRecHurtType m_HurtType;
        private EState m_State;
        private bool m_IsExecuteFromBack;

        public DamageInfo Damage { get; set; }
        public override bool CanExit => false;
        public EHitRecHurtType HurtType => m_HurtType;
        public bool CanBeExecute { get; private set; }


        public MonsterGetHit() : base(EStateType.GetHit)
        {
        }

        string Get4DirString()
        {
            if (m_AngleFromAttacker > -45 && m_AngleFromAttacker <= 45)
                return "B";
            else if (m_AngleFromAttacker > 45 && m_AngleFromAttacker <= 135)
                return "R";
            else if (m_AngleFromAttacker > -135 && m_AngleFromAttacker <= -45)
                return "L";
            else
                return "F";
        }

        string Get2DirString()
        {
            bool isBack = m_AngleFromAttacker > -90 && m_AngleFromAttacker <= 90;
            //  faceType = isBack ? EFaceToAttackerType.FaceToBack : EFaceToAttackerType.FaceToFace;
            return isBack ? "Backward" : "Forward";
        }

        string GetHitAnim(out EHitRecHurtType hurtType)
        {
            m_AngleFromAttacker = Vector3.SignedAngle(Damage.DamageDirection, Actor.transform.forward, Vector3.up);
            bool forceLarge = Damage.DamageConfig.m_ForceWhenGround.x >= 5;
            // faceType = EFaceToAttackerType.None;
            hurtType = EHitRecHurtType.Stun;
            switch (Damage.DamageConfig.m_DamageLevel)
            {
                case DamageLevel.HitLight:
                    string dirStr = Get4DirString();
                    return $"Stun{dirStr}";

                case DamageLevel.HitHeavy:
                    dirStr = Get4DirString();
                    if (forceLarge)
                    {
                        return $"GetHit{dirStr}HeavyFar";
                    }
                    else
                    {
                        return $"GetHit{dirStr}Heavy";
                    }

                case DamageLevel.HitDown:
                    dirStr = Get2DirString();
                    return $"GetHitDownBy{dirStr}";

                case DamageLevel.HitFly:
                    dirStr = Get2DirString();
                    return $"GetHitFlyBy{dirStr}";

                case DamageLevel.Backstab:
                    if (Vector3.Dot(Actor.transform.forward, Damage.Attacker.transform.forward) > 0)
                    {
                        hurtType = EHitRecHurtType.BlockBroken;
                        return "BlockBroken";
                    }
                    else
                    {
                        dirStr = Get4DirString();
                        return $"Stun{dirStr}";
                    }


                default:
                    Debug.LogError($"Unknown hit level:{Damage.DamageConfig.m_DamageLevel}");
                    break;
            }

            return null;
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

            m_State = m_HurtType switch
            {
                EHitRecHurtType.Stun => EState.Stun,
                EHitRecHurtType.BlockBroken => EState.BlockBroken,
                _ => throw new InvalidOperationException($"Unknown hurt type:{m_HurtType}"),
            };
            /*
            // 对齐攻击者方向
            Vector3 directionToAttacker = Damage.Attacker.transform.position - Actor.transform.position;
            directionToAttacker.y = 0;
            if (forceToAttackerType == EFaceToAttackerType.FaceToFace)
            {
                Actor.transform.rotation = Quaternion.LookRotation(directionToAttacker);
            }
            else if (forceToAttackerType == EFaceToAttackerType.FaceToBack)
            {
                Actor.transform.rotation = Quaternion.LookRotation(-directionToAttacker);
            }
            */
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
                if (HurtType == EHitRecHurtType.BlockBroken)
                {
                    CanBeExecute = enter;
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
        EHitRecHurtType HurtType { get; }
        bool CanBeExecute { get; }
        void BeExecuted(SActor executioner);
    }

    public enum EHitRecHurtType
    {
        Stun,
        BlockBroken,
    }
}