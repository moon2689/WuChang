using System;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class MonsterIdle : ActorStateBase
    {
        private SMonster m_Monster;
        private AudioPlayer m_AudioPlayer;
        private Action m_ActionOnLookArroundFinished;
        private Action m_ActionOnTurnDirectionFinished;


        private SMonster Monster => m_Monster ??= (SMonster)Actor;
        public override bool ApplyRootMotionSetWhenEnter => true;
        

        public MonsterIdle() : base(EStateType.Idle)
        {
        }

        public override void Enter()
        {
            base.Enter();
            m_ActionOnLookArroundFinished = null;
            m_ActionOnTurnDirectionFinished = null;
        }

        public override void OnStay()
        {
            base.OnStay();
            // 播放声音
            if (Monster.m_MonsterInfo.m_PlaySoundWhenIdle)
            {
                if (m_AudioPlayer == null || !m_AudioPlayer.AudioSource.isPlaying)
                {
                    var clip = Monster.m_MonsterInfo.GetRandomIdleAudio();
                    m_AudioPlayer = GameApp.Entry.Game.Audio.Play3DSound(clip, Monster.transform.position);
                }
            }
        }

        protected override void OnExit()
        {
            base.OnExit();
            OnLookAroundFinished();
            OnTurnDirectionFinished();
        }

        public void DoActionStartled(Action onFinished)
        {
            Actor.CAnim.Play("Startled", onFinished: onFinished);
        }

        public void LookAround(Action onFinished)
        {
            m_ActionOnLookArroundFinished = onFinished;
            Actor.CAnim.Play("LookArround");
        }

        public void PlayAction(string name, Action onFinished)
        {
            Actor.CAnim.Play(name, onFinished: onFinished);
        }

        public bool IsPlayingAction(string name)
        {
            return Actor.CAnim.IsPlayingOrWillPlay(name);
        }

        public override void OnTriggerAnimEvent(AnimPointTimeEvent eventObj)
        {
            base.OnTriggerAnimEvent(eventObj);
            if (eventObj.EventType == EAnimTriggerEvent.LookArroundFirstFinished)
            {
                OnLookAroundFinished();
            }
        }

        public void TurnDirection(Vector3 targetPos, Action onFinished)
        {
            Vector3 dir = targetPos - Actor.transform.position;
            float signedAngle = Vector3.SignedAngle(Actor.transform.forward, dir, Vector3.up);
            string animName;
            if (signedAngle > 30 && signedAngle <= 60)
            {
                animName = "TurnRight45";
            }
            else if (signedAngle > 60 && signedAngle <= 120)
            {
                animName = "TurnRight90";
            }
            else if (signedAngle > 120 && signedAngle <= 180)
            {
                animName = "TurnRight150";
            }
            else if (signedAngle > -60 && signedAngle <= -30)
            {
                animName = "TurnLeft45";
            }
            else if (signedAngle > -120 && signedAngle <= -60)
            {
                animName = "TurnLeft90";
            }
            else if (signedAngle > -180 && signedAngle <= -120)
            {
                animName = "TurnLeft150";
            }
            else
            {
                animName = null;
            }

            m_ActionOnTurnDirectionFinished = onFinished;
            if (animName != null)
            {
                //base.Actor.CPhysic.ApplyRootMotion = true;
                Actor.CAnim.Play(animName, onFinished: () =>
                {
                    //base.Actor.CPhysic.ApplyRootMotion = false;
                    OnTurnDirectionFinished();
                });
            }
            else
            {
                OnTurnDirectionFinished();
            }
        }

        void OnTurnDirectionFinished()
        {
            m_ActionOnTurnDirectionFinished?.Invoke();
            m_ActionOnTurnDirectionFinished = null;
        }

        void OnLookAroundFinished()
        {
            m_ActionOnLookArroundFinished?.Invoke();
            m_ActionOnLookArroundFinished = null;
        }
    }
}