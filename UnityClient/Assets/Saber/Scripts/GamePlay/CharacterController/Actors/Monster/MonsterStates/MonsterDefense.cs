using System.Collections;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class MonsterDefense : DefenseBase
    {
        private float m_TimerAlign;
        private bool m_AutoExit;

        public override bool ApplyRootMotionSetWhenEnter => true;
        public override bool CanEnter => Actor.CPhysic.Grounded;
        public override bool CanExit => m_CurState == EState.DefenseLoop;


        public override void Enter()
        {
            base.Enter();
            m_CurState = EState.DefenseLoop;

            Actor.CAnim.StopMaskLayerAnims();

            m_TimerAlign = 0.1f;

            // fix weapon location
            Actor.CMelee.CWeapon.TryFixDefenseLocation(true);

            Actor.CAnim.Play($"DefenseStart");
        }

        public override void OnStay()
        {
            base.OnStay();
            if (m_TimerAlign > 0)
            {
                m_TimerAlign -= Time.deltaTime;
                Actor.CPhysic.AlignForwardTo(Actor.DesiredLookDir, 1080f);
            }
        }

        protected override void OnExit()
        {
            base.OnExit();
            // fix weapon location
            Actor.CMelee.CWeapon.TryFixDefenseLocation(false);
        }

        public void EndDefense()
        {
            if (m_CurState != EState.DefenseEnd)
            {
                m_CurState = EState.DefenseEnd;
                Actor.CAnim.Play($"DefenseEnd", exitTime: 0.9f, onFinished: Exit);
            }
        }

        public void OnHit(DamageInfo dmgInfo)
        {
            Actor.CAnim.Play("DefenseHit", force: true);

            // face to attacher
            Vector3 dir = dmgInfo.Attacker.transform.position - Actor.transform.position;
            dir.y = 0;
            Actor.transform.rotation = Quaternion.LookRotation(dir);

            // force
            if (dmgInfo.DamageConfig.m_ForceWhenGround.x > 0)
            {
                Actor.CPhysic.Force_Add(-dir, dmgInfo.DamageConfig.m_ForceWhenGround.x, 1, false);
            }
        }
    }
}