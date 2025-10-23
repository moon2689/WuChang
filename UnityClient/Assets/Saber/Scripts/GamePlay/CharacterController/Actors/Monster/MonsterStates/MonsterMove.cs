using System;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class MonsterMove : ActorStateBase
    {
        private float m_CurRotSpeed;
        private SMonster m_Monster;


        private SMonster Monster => m_Monster ??= (SMonster)Actor;


        public MonsterMove() : base(EStateType.Move)
        {
        }

        public override void Enter()
        {
            base.Enter();

            Actor.CAnim.ResetSmoothFloat(EAnimatorParams.Horizontal, 0);
            Actor.CAnim.ResetSmoothFloat(EAnimatorParams.Vertical, 0);
        }

        public override void OnStay()
        {
            base.OnStay();

            if (Actor.MovementAxisMagnitude > 0.1f)
            {
                int moveSpeed = (int)Actor.MoveSpeedV;
                Actor.CAnim.SetSmoothFloat(EAnimatorParams.Horizontal, Actor.MovementAxis.x * moveSpeed);
                Actor.CAnim.SetSmoothFloat(EAnimatorParams.Vertical, Actor.MovementAxis.z * moveSpeed);

                // 位移
                float speed = GetSpeed2D();
                Actor.CPhysic.AdditivePosition += Actor.DesiredMoveDir * speed * base.DeltaTime;
                Actor.CPhysic.AlignForwardTo(Actor.DesiredLookDir, 720);

                // 播放移动动画
                Actor.CAnim.Play("Move");
            }
            else
            {
                Exit();
            }
        }

        float GetSpeed(float curSmoothFloat)
        {
            float speed = 0;
            if (curSmoothFloat <= 1)
            {
                speed = Mathf.Lerp(0, Monster.m_BaseActorInfo.m_SpeedWalk, curSmoothFloat);
            }
            else if (curSmoothFloat <= 2)
            {
                speed = Mathf.Lerp(Monster.m_BaseActorInfo.m_SpeedWalk, Monster.m_BaseActorInfo.m_SpeedRun, curSmoothFloat - 1);
            }
            else if (curSmoothFloat <= 3)
            {
                speed = Mathf.Lerp(Monster.m_BaseActorInfo.m_SpeedRun, Monster.m_BaseActorInfo.m_SpeedSprint, curSmoothFloat - 2);
            }

            return speed;
        }

        float GetSpeed2D()
        {
            float curSmoothFloatH = Actor.CAnim.GetCurSmoothFloat(EAnimatorParams.Horizontal);
            float curSmoothFloatV = Actor.CAnim.GetCurSmoothFloat(EAnimatorParams.Vertical);
            float curSmoothFloat = Mathf.Sqrt(curSmoothFloatH * curSmoothFloatH + curSmoothFloatV * curSmoothFloatV);
            // float curSmoothFloat = 2 * GameHelper.GetStickLength(curSmoothFloatH * 0.5f, curSmoothFloatV * 0.5f);
            return GetSpeed(curSmoothFloat);
        }

        protected override void OnExit()
        {
            base.OnExit();
            Actor.CAnim.SetSmoothFloat(EAnimatorParams.Horizontal, 0);
            Actor.CAnim.SetSmoothFloat(EAnimatorParams.Vertical, 0);
        }
    }
}