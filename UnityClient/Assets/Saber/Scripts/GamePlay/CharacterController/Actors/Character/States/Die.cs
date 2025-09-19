using UnityEngine;

namespace Saber.CharacterController
{
    public class Die : ActorStateBase
    {
        public override bool ApplyRootMotionSetWhenEnter => true;

        public override bool CanEnter => Actor.IsDead;
        public string SpecialAnim { get; set; }


        public Die() : base(EStateType.Die)
        {
        }

        public override void Enter()
        {
            base.Enter();
            string anim = !string.IsNullOrEmpty(SpecialAnim) ? SpecialAnim : "Die";
            Actor.CAnim.Play(anim, force: true);
            Actor.CPhysic.UseGravity = true;
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