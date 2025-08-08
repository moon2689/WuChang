using System;
using System.Collections.Generic;


using UnityEngine;

namespace Saber.CharacterController
{
    /// <summary>打击恢复</summary>
    public class HitRecovery : ObstructBase
    {
        enum EFaceToAttackerType
        {
            None,
            FaceToFace,
            FaceToBack,
            FaceToRight,
            FaceToLeft,
        }

        private float m_AngleFromAttacker;
        private Queue<string> m_QueueAnims = new();


        public HitRecovery(SActor actor, Action actionExit) : base(actor, actionExit)
        {
        }

        public override void Enter(DamageInfo damageInfo)
        {
            Vector3 directionToAttacker = damageInfo.Attacker.transform.position - Actor.transform.position;
            directionToAttacker.y = 0;

            m_AngleFromAttacker = Vector3.SignedAngle(damageInfo.Attacker.transform.forward, Actor.transform.forward, Vector3.up);

            // play anim
            var forceToAttackerType = GetAnimInfo(damageInfo);
            PlayAnim();

            // face to attacker
            if (forceToAttackerType == EFaceToAttackerType.FaceToFace)
            {
                Actor.transform.rotation = Quaternion.LookRotation(directionToAttacker);
            }
            else if (forceToAttackerType == EFaceToAttackerType.FaceToBack)
            {
                Actor.transform.rotation = Quaternion.LookRotation(-directionToAttacker);
            }
            else if (forceToAttackerType == EFaceToAttackerType.FaceToLeft)
            {
                Vector3 left = Vector3.Cross(Vector3.up, directionToAttacker);
                Actor.transform.rotation = Quaternion.LookRotation(left);
            }
            else if (forceToAttackerType == EFaceToAttackerType.FaceToRight)
            {
                Vector3 right = Vector3.Cross(Vector3.up, -directionToAttacker);
                Actor.transform.rotation = Quaternion.LookRotation(right);
            }

            // add force
            if (damageInfo.DamageConfig.m_ForceWhenGround.x > 0)
            {
                Actor.CPhysic.Force_Add(-directionToAttacker, damageInfo.DamageConfig.m_ForceWhenGround.x, 0, false);
            }
        }

        public override void ReEnter(DamageInfo damageInfo)
        {
            Enter(damageInfo);
        }

        void PlayAnim()
        {
            if (m_QueueAnims.Count > 0)
            {
                string animName = m_QueueAnims.Dequeue();
                Actor.CAnim.PlayClip($"Animation/Hit/{animName}", PlayAnim);
            }
            else
            {
                Exit();
            }
        }

        EFaceToAttackerType GetAnimInfo(DamageInfo damageInfo)
        {
            m_QueueAnims.Clear();
            DamageLevel dmgLevel = damageInfo.DamageConfig.m_DamageLevel;
            EFaceToAttackerType forceToAttackerType;
            bool isDamageFromBack = m_AngleFromAttacker > -90 && m_AngleFromAttacker <= 90;
            if (dmgLevel == DamageLevel.Normal)
            {
                string strDir;
                if (m_AngleFromAttacker > -45 && m_AngleFromAttacker <= 45)
                    strDir = "Back";
                else if (m_AngleFromAttacker > 45 && m_AngleFromAttacker <= 135)
                    strDir = "Right";
                else if (m_AngleFromAttacker > -135 && m_AngleFromAttacker <= -45)
                    strDir = "Left";
                else
                {
                    float angle = Vector3.SignedAngle(Actor.transform.up, damageInfo.DamageDirection, Actor.transform.right);
                    // Debug.Log($"angle:{angle}");
                    if (angle < -60 && angle > -120)
                    {
                        strDir = "FrontLeft";
                    }
                    else if (angle > 60 && angle < 120)
                    {
                        strDir = "FrontRight";
                    }
                    else if (angle > -30 && angle < 30)
                    {
                        strDir = "FrontUp";
                    }
                    else
                    {
                        strDir = "Front";
                    }
                }

                m_QueueAnims.Enqueue($"HitNormal{strDir}");
                forceToAttackerType = EFaceToAttackerType.None;
            }
            else if (dmgLevel == DamageLevel.Large)
            {
                if (isDamageFromBack)
                {
                    m_QueueAnims.Enqueue("HitLargeBack");
                    forceToAttackerType = EFaceToAttackerType.FaceToBack;
                }
                else
                {
                    m_QueueAnims.Enqueue("HitLargeFront");
                    forceToAttackerType = EFaceToAttackerType.FaceToFace;
                }
            }
            else if (dmgLevel == DamageLevel.StrikeDown)
            {
                if (isDamageFromBack)
                {
                    m_QueueAnims.Enqueue("StrikeDownBack");
                    m_QueueAnims.Enqueue("GetUpFromLieOnStomach");
                    forceToAttackerType = EFaceToAttackerType.FaceToBack;
                }
                else
                {
                    m_QueueAnims.Enqueue("StrikeDownFront");
                    m_QueueAnims.Enqueue("GetUpFromLieOnBack");
                    forceToAttackerType = EFaceToAttackerType.FaceToFace;
                }
            }
            else if (dmgLevel == DamageLevel.KnockDownSpin)
            {
                forceToAttackerType = EFaceToAttackerType.FaceToRight;
                m_QueueAnims.Enqueue($"Hit{dmgLevel}");
                m_QueueAnims.Enqueue("GetUpFromLieOnStomach");
            }
            else
            {
                m_QueueAnims.Enqueue($"Hit{dmgLevel}");
                forceToAttackerType = EFaceToAttackerType.FaceToFace;
            }

            return forceToAttackerType;
        }
    }
}