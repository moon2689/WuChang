using System;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class MonsterMove : ActorStateBase
    {
        //private float m_CurRotSpeed;

        //private SMonster m_Monster;
        //private AudioPlayer m_AudioPlayer;
        //private ActorFootstep[] m_ActorFootstep;


        //private SMonster Monster => m_Monster ??= (SMonster)Actor;
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

            Actor.CAnim.Play("Move", force: true);

            // for (int i = 0; i < m_ActorFootstep.Length; i++)
            // {
            //     m_ActorFootstep[i].ActiveSelf = true;
            // }
        }

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

        /*
        public override void OnStay()
        {
            base.OnStay();

            if (Actor.MovementAxisMagnitude > 0.1f)
            {
                bool isBack = Actor.MovementAxis.z < 0;
                Vector3 desiredForwardDir = isBack ? -Actor.DesiredMoveDir : Actor.DesiredMoveDir;

                // 转向
                float angle = Actor.CPhysic.AlighForwardTo(desiredForwardDir, angle =>
                {
                    float speed = Mathf.Abs(angle) > 60 ? 360 : 90;
                    m_CurRotSpeed = Mathf.Lerp(m_CurRotSpeed, speed, 0.1f);
                    return m_CurRotSpeed * Actor.PhysicInfo.TurnRotSpeedRate;
                });

                float speed;
                float paramH = 0, paramV;
                if (isBack)
                {
                    speed = -Monster.m_MonsterInfo.m_SpeedWalk;
                    paramV = -1;
                }
                else
                {
                    bool isWalk = Actor.MoveSpeedV == EMoveSpeedV.Walk;
                    if (angle > 0.1f)
                        paramH = Mathf.Clamp01(angle / 20f);
                    else if (angle < -0.1f)
                        paramH = -Mathf.Clamp01(Mathf.Abs(angle) / 20f);

                    paramV = isWalk ? 1 : 2;
                    speed = isWalk ? Monster.m_MonsterInfo.m_SpeedWalk : Monster.m_MonsterInfo.m_SpeedRun;
                }

                Actor.CAnim.SetSmoothFloat(EAnimatorParams.Horizontal, paramH);
                Actor.CAnim.SetSmoothFloat(EAnimatorParams.Vertical, paramV);

                // 位移
                Actor.CPhysic.AdditivePosition += Actor.transform.forward * speed * base.DeltaTime;

                if (!Actor.CPhysic.Grounded)
                    Actor.CPhysic.AdditivePosition += Vector3.down * 3 * base.DeltaTime;
                    

                // 播放声音
                if (Monster.m_MonsterInfo.m_FootstepType == MonsterInfo.EFootstepType.PlayWhenMove)
                {
                    if (m_AudioPlayer == null || !m_AudioPlayer.AudioSource.isPlaying)
                    {
                        var clip = Monster.m_MonsterInfo.GetRandomFootstepAudio();
                        m_AudioPlayer = GameApp.Entry.Game.Audio.Play3DSound(clip, Monster.transform.position);
                    }
                }
            }
            else
            {
                Exit();
            }
        }
        */

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