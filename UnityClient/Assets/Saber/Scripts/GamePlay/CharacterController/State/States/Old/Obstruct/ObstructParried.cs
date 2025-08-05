using System;


namespace Saber.CharacterController
{
    /// <summary>被弹反</summary>
    public class ObstructParried : ObstructBase
    {
        public ObstructParried(SActor actor, Action actionExit) : base(actor, actionExit)
        {
        }

        public override void Enter(DamageInfo damageInfo)
        {
            Actor.CAnim.Play("Parried", force: true, onFinished: Exit);
        }

        public override void ReEnter(DamageInfo damageInfo)
        {
        }
    }
}