

using UnityEngine;

namespace Saber.CharacterController
{
    public class NPCMove : ActorStateBase
    {
        public override bool ApplyRootMotionSetWhenEnter => true;

        public NPCMove() : base(EStateType.Move)
        {
        }

        public override void Enter()
        {
            base.Enter();
            if (!Actor.CAnim.IsPlayingOrWillPlay("Move"))
                Actor.CAnim.Play("Move");
        }

        public override void OnStay()
        {
            base.OnStay();
            if (Actor.MovementAxisMagnitude > 0.1f)
            {
                // 转向
                Actor.CPhysic.AlignForwardTo(Actor.DesiredLookDir, angle => Mathf.Abs(angle) > 45 ? 1080 : 360);
                Actor.CAnim.SetSmoothFloat(EAnimatorParams.Horizontal, 0);
                Actor.CAnim.SetSmoothFloat(EAnimatorParams.Vertical, Actor.MovementAxisMagnitude * (int)Actor.MoveSpeedV);
            }
        }

        protected override void OnExit()
        {
            base.OnExit();
            Actor.CAnim.SetSmoothFloat(EAnimatorParams.Horizontal, 0);
            Actor.CAnim.SetSmoothFloat(EAnimatorParams.Vertical, 0);
        }
    }
}