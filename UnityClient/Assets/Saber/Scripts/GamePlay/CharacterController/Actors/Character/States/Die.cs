namespace Saber.CharacterController
{
    public class Die : ActorStateBase
    {
        public override bool ApplyRootMotionSetWhenEnter => true;

        public override bool CanEnter => Actor.IsDead;

        public Die() : base(EStateType.Die)
        {
        }

        public override void Enter()
        {
            base.Enter();
            Actor.CAnim.Play("Die");
            Actor.CPhysic.UseGravity = true;
            Actor.CMelee.CWeapon.ToggleWeapon(false);
        }
    }
}