using System;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class MonsterGetHit : ActorStateBase
    {
        enum EFaceToAttackerType
        {
            None,
            FaceToFace,
            FaceToBack,
        }

        private string m_CurAnim;
        private float m_AngleFromAttacker;

        public DamageInfo Damage { get; set; }
        public override bool CanExit => false;


        public MonsterGetHit() : base(EStateType.GetHit)
        {
        }

        string Get4DirString()
        {
            if (m_AngleFromAttacker > -45 && m_AngleFromAttacker <= 45)
                return "Backward";
            else if (m_AngleFromAttacker > 45 && m_AngleFromAttacker <= 135)
                return "Right";
            else if (m_AngleFromAttacker > -135 && m_AngleFromAttacker <= -45)
                return "Left";
            else
                return "Forward";
        }

        string Get2DirString(out EFaceToAttackerType faceType)
        {
            bool isBack = m_AngleFromAttacker > -90 && m_AngleFromAttacker <= 90;
            faceType = isBack ? EFaceToAttackerType.FaceToBack : EFaceToAttackerType.FaceToFace;
            return isBack ? "Backward" : "Forward";
        }

        string GetHitAnim(out EFaceToAttackerType faceType)
        {
            m_AngleFromAttacker = Vector3.SignedAngle(Damage.DamageDirection, Actor.transform.forward, Vector3.up);
            bool forceLarge = Damage.DamageConfig.m_ForceWhenGround.x >= 5;
            faceType = EFaceToAttackerType.None;
            switch (Damage.DamageConfig.m_DamageLevel)
            {
                case DamageLevel.HitLight:
                    string dirStr = Get4DirString();
                    if (forceLarge)
                    {
                        return $"GetHit{dirStr}Far";
                    }
                    else
                    {
                        return $"GetHit{dirStr}";
                    }

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
                    dirStr = Get2DirString(out faceType);
                    return $"GetHitDownBy{dirStr}";

                case DamageLevel.HitFly:
                    dirStr = Get2DirString(out faceType);
                    return $"GetHitFlyBy{dirStr}";

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
            m_CurAnim = GetHitAnim(out var forceToAttackerType);
            //Actor.CAnim.Play(m_CurAnim, force: true);

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
        }

        public override void OnStay()
        {
            base.OnStay();
            if (Actor.IsDead)
            {
                if (Weak.IsLieOnGround(Actor))
                {
                    base.Exit();
                    return;
                }
            }

            if (Actor.CAnim.IsPlayingOrWillPlay("Idle"))
            {
                Exit();
                return;
            }
        }
    }
}