using System;


namespace Saber.CharacterController
{
    public class ObstructStun : ObstructBase
    {
        public ObstructStun(SActor actor, Action actionExit) : base(actor, actionExit)
        {
        }

        public override void Enter(DamageInfo damageInfo)
        {
            Actor.CAnim.PlayClip("Animation/HumanObstruct/Stun", null);
        }

        public override void ReEnter(DamageInfo damageInfo)
        {
        }

        public override void OnStay(DamageInfo damageInfo, float deltaTime)
        {
            base.OnStay(damageInfo, deltaTime);
            if (!Actor.IsStun)
            {
                Exit();
            }
        }
    }
}