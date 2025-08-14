using System;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class GetHit : ActorStateBase
    {
        private string m_CurAnim;
        private float m_AngleFromAttacker;

        public DamageInfo Damage { get; set; }
        public override bool CanExit => false;


        public GetHit() : base(EStateType.GetHit)
        {
        }


        string GetHitAnim()
        {
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
            m_CurAnim = GetHitAnim();
            Actor.CAnim.Play(m_CurAnim, force: true);

            // 对齐攻击者方向


            // 击退的力
            if (Damage.DamageConfig.m_ForceWhenGround.x > 0)
            {
                //Actor.CPhysic.Force_Add(-directionToAttacker, Damage.DamageConfig.m_ForceWhenGround.x, 0, false);
            }

            // 骨骼受击抖动
           // float force = GameApp.Entry.Config.GameSetting.IKBoneForceOnHit;
            //this.Damage.m_HurtBox.OnHit(Damage.DamageDirection * force, Damage.DamagePosition);
        }

        public override void OnStay()
        {
            base.OnStay();

            if (Actor.CAnim.IsPlayingOrWillPlay($"IdleArmed") || Actor.CAnim.IsPlayingOrWillPlay($"IdleUnarmed"))
            {
                Exit();
            }
        }
    }
}