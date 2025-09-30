using System;
using System.Collections;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class PlayActionState : ActorStateBase
    {
        public enum EActionType
        {
            IdolActive,
            IdolRest,
            IdolRestEnd,
            TurnL180,
            TurnR180,
        }

        enum EState
        {
            WaitPlay,
            Playing,
            End,
        }

        private string m_CurActionName;
        private Action m_OnPlayActionEnd;
        private EState m_State;

        private bool m_IsSettingPosAndForward;
        private Vector3 m_ToSetForward;
        private float m_ToSetPosTime;
        private Vector3 m_ToSetPosSpeed;
        private Action m_SetPosAndForwardFinishedEvent;

        public override bool ApplyRootMotionSetWhenEnter => true;
        protected override ActorBaseStats.EStaminaRecSpeed StaminaRecSpeed => ActorBaseStats.EStaminaRecSpeed.Fast;


        public PlayActionState() : base(EStateType.PlayAction)
        {
        }

        public override void OnStay()
        {
            base.OnStay();

            if (m_IsSettingPosAndForward)
            {
                if (m_ToSetPosTime > 0)
                {
                    Actor.CPhysic.AdditivePosition += m_ToSetPosSpeed * base.DeltaTime;
                    m_ToSetPosTime -= DeltaTime;
                }

                if (Actor.CPhysic.AlignForwardTo(m_ToSetForward, 720))
                {
                    if (m_ToSetPosTime <= 0)
                    {
                        Exit();
                        m_IsSettingPosAndForward = false;
                        if (m_SetPosAndForwardFinishedEvent != null)
                        {
                            GameApp.Entry.Unity.DoActionOneFrameLater(m_SetPosAndForwardFinishedEvent);
                            m_SetPosAndForwardFinishedEvent = null;
                        }
                    }
                }
            }
            else if (!string.IsNullOrEmpty(m_CurActionName))
            {
                if (m_State == EState.WaitPlay)
                {
                    if (Actor.CAnim.IsReallyPlaying(m_CurActionName))
                    {
                        m_State = EState.Playing;
                    }
                }
                else if (m_State == EState.Playing)
                {
                    if (m_OnPlayActionEnd != null && !Actor.CAnim.IsPlayingOrWillPlay(m_CurActionName))
                    {
                        GameApp.Entry.Unity.DoActionOneFrameLater(m_OnPlayActionEnd);
                        m_OnPlayActionEnd = null;
                    }

                    if (Actor.CAnim.IsPlayingOrWillPlay("Idle"))
                    {
                        m_State = EState.End;
                        Exit();
                        m_CurActionName = null;
                    }
                }
                else if (m_State == EState.End)
                {
                }
            }
        }

        public void SetPosAndForward(Vector3 tarPos, Vector3 forward, Action onFinished)
        {
            Vector3 dis = tarPos - Actor.transform.position;
            dis.y = 0;
            float speed = 5f;
            m_ToSetPosSpeed = dis.normalized * speed;

            m_IsSettingPosAndForward = true;
            m_ToSetForward = forward;
            m_ToSetPosTime = dis.magnitude / speed;
            m_SetPosAndForwardFinishedEvent = onFinished;
        }

        public void PlayAction(EActionType actionType, Action onPlayFinish)
        {
            m_CurActionName = actionType.ToString();
            m_OnPlayActionEnd = onPlayFinish;

            Actor.CAnim.Play(m_CurActionName);
            m_State = EState.WaitPlay;
        }

        public override void OnTriggerAnimEvent(AnimPointTimeEvent eventObj)
        {
            base.OnTriggerAnimEvent(eventObj);
            if (eventObj.EventType == EAnimTriggerEvent.ShowWeapon)
            {
                Actor.CMelee.CWeapon.ShowOrHideWeapon(true);
            }
            else if (eventObj.EventType == EAnimTriggerEvent.HideWeapon)
            {
                Actor.CMelee.CWeapon.ShowOrHideWeapon(false);
            }
        }
    }
}