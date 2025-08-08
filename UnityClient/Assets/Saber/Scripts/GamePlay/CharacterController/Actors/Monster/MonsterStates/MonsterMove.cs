using System;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class MonsterMove : ActorStateBase
    {
        private float m_CurRotSpeed;
        private SMonster m_Monster;

        private AudioPlayer m_AudioPlayer;
        //private ActorFootstep[] m_ActorFootstep;


        private SMonster Monster => m_Monster ??= (SMonster)Actor;
        public override bool ApplyRootMotionSetWhenEnter => true;


        public MonsterMove() : base(EStateType.Move)
        {
        }

        // public override void Init(ActorStateMachine parent)
        // {
        //     base.Init(parent);
        //     m_ActorFootstep = Actor.GetComponentsInChildren<ActorFootstep>();
        // }

        public override void Enter()
        {
            base.Enter();

            Actor.CAnim.ResetSmoothFloat(EAnimatorParams.Horizontal, 0);
            Actor.CAnim.ResetSmoothFloat(EAnimatorParams.Vertical, 0);

            //Actor.CAnim.Play("Move", force: true);

            // for (int i = 0; i < m_ActorFootstep.Length; i++)
            // {
            //     m_ActorFootstep[i].ActiveSelf = true;
            // }
        }

        /*
        public override void OnStay()
        {
            base.OnStay();

            if (Actor.MovementAxisMagnitude > 0.1f)
            {
                if (Actor.MoveSpeedV == EMoveSpeedV.Walk)
                {
                    Actor.CAnim.SetSmoothFloat(EAnimatorParams.Horizontal, Actor.MovementAxis.x);
                    Actor.CAnim.SetSmoothFloat(EAnimatorParams.Vertical, Actor.MovementAxis.z);

                    // 转向
                    Actor.CPhysic.AlignForwardTo(Actor.DesiredLookDir, 1080);
                }
                else if (Actor.MoveSpeedV == EMoveSpeedV.Run)
                {
                    Actor.CAnim.SetSmoothFloat(EAnimatorParams.Horizontal, 0);
                    Actor.CAnim.SetSmoothFloat(EAnimatorParams.Vertical, Actor.MovementAxis.z * 2);

                    // 转向
                    Actor.CPhysic.AlignForwardTo(Actor.DesiredMoveDir, 1080);
                }
            }
            else
            {
                Exit();
            }
        }
        */


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
                speed = Mathf.Lerp(0, Monster.m_MonsterInfo.m_SpeedWalk, curSmoothFloat);
            }
            else if (curSmoothFloat <= 2)
            {
                speed = Mathf.Lerp(Monster.m_MonsterInfo.m_SpeedWalk, Monster.m_MonsterInfo.m_SpeedRun,
                    curSmoothFloat - 1);
            }
            else if (curSmoothFloat <= 3)
            {
                speed = Mathf.Lerp(Monster.m_MonsterInfo.m_SpeedRun, Monster.m_MonsterInfo.m_SpeedSprint,
                    curSmoothFloat - 2);
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

            // for (int i = 0; i < m_ActorFootstep.Length; i++)
            // {
            //     m_ActorFootstep[i].ActiveSelf = false;
            // }
        }
    }
}