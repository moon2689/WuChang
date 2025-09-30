using System;
using System.Collections.Generic;
using UnityEngine;

namespace Saber.CharacterController
{
    public class Die : ActorStateBase
    {
        private List<string> m_Anims = new();
        private int m_CurAnimIndex;


        //public DamageInfo Damage { get; set; }
        public override bool ApplyRootMotionSetWhenEnter => true;

        public override bool CanEnter => Actor.IsDead;
        public string SpecialAnim { get; set; }


        public Die() : base(EStateType.Die)
        {
        }

        public override void Enter()
        {
            base.Enter();
            GetDieAnim(out Vector3 faceDir);
            Actor.CAnim.Play(m_Anims[0], force: true);
            m_CurAnimIndex = 0;

            Actor.CPhysic.UseGravity = true;

            // 对齐攻击者方向
            if (faceDir != Vector3.zero)
            {
                Actor.transform.rotation = Quaternion.LookRotation(faceDir);
            }
        }

        /*
        string GetDir2(out Vector3 faceDir, out bool isBack)
        {
            float angleFromAttacker = Vector3.SignedAngle(Damage.Attacker.transform.forward, Actor.transform.forward, Vector3.up);
            isBack = angleFromAttacker > -90 && angleFromAttacker <= 90;

            Vector3 directionToAttacker = Damage.Attacker.transform.position - Actor.transform.position;
            directionToAttacker.y = 0;
            faceDir = isBack ? -directionToAttacker : directionToAttacker;

            return isBack ? "B" : "F";
        }
        */

        void GetDieAnim(out Vector3 faceDir)
        {
            m_Anims.Clear();
            faceDir = Vector3.zero;

            if (!string.IsNullOrEmpty(SpecialAnim))
            {
                m_Anims.Add(SpecialAnim);
            }
            /*
            else if (Damage.DamageConfig.m_HitRecover == EHitRecover.StrikeDown)
            {
                m_Anims.Add($"StrikeDown{GetDir2(out faceDir, out bool isBack)}");
                m_Anims.Add(isBack ? "ExecutedBackDownDie" : "ExecutedFrontDownDie");
            }
            else if (Damage.DamageConfig.m_HitRecover == EHitRecover.KnockOffLongDis)
            {
                m_Anims.Add($"KnockOffLongDis{GetDir2(out faceDir, out bool isBack)}");
                m_Anims.Add(isBack ? "ExecutedBackDownDie" : "ExecutedFrontDownDie");
            }
            else if (Damage.DamageConfig.m_HitRecover == EHitRecover.Uppercut)
            {
                m_Anims.Add("Uppercut");
            }
            */
            else
            {
                m_Anims.Add("Die");
            }
        }

        public override void OnStay()
        {
            base.OnStay();

            if (m_CurAnimIndex < m_Anims.Count)
            {
                if (!Actor.CAnim.IsPlayingOrWillPlay(m_Anims[m_CurAnimIndex], 0.95f))
                {
                    ++m_CurAnimIndex;
                    if (m_CurAnimIndex < m_Anims.Count)
                    {
                        Actor.CAnim.Play(m_Anims[m_CurAnimIndex], force: true);
                    }
                    else
                    {
                        Actor.OnDieAnimPlayFinished();
                    }
                }
            }
        }

        protected override void OnExit()
        {
            base.OnExit();
            SpecialAnim = null;
        }

        public override void OnTriggerAnimEvent(AnimPointTimeEvent eventObj)
        {
            base.OnTriggerAnimEvent(eventObj);
            if (eventObj.EventType == EAnimTriggerEvent.WeaponFallToGround)
            {
                Actor.CMelee.CWeapon.WeaponFallToGround();
            }
        }
    }
}