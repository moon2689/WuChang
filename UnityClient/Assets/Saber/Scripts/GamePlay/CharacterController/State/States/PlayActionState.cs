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
            FaceToLockingEnemy,
        }

        enum ENormalActionState
        {
            WaitPlay,
            Playing,
            End,
        }

        enum ECurAction
        {
            None,
            SetPosAndForward,
            FaceToLockingEnemy,
            NormalAction,
        }

        private string m_CurActionName;
        private Action m_OnPlayActionEnd;
        private ENormalActionState m_NormalActionState;

        private Vector3 m_ToSetForward;
        private float m_ToSetPosTime;
        private Vector3 m_ToSetPosSpeed;
        private Action m_SetPosAndForwardFinishedEvent;

        private ECurAction m_CurAction;

        public override bool ApplyRootMotionSetWhenEnter => true;
        protected override ActorBaseStats.EStaminaRecSpeed StaminaRecSpeed => ActorBaseStats.EStaminaRecSpeed.Fast;


        public PlayActionState() : base(EStateType.PlayAction)
        {
        }

        public override void OnStay()
        {
            base.OnStay();

            if (m_CurAction == ECurAction.None)
            {
            }
            else if (m_CurAction == ECurAction.SetPosAndForward)
            {
                UpdateSetPosAndForward();
            }
            else if (m_CurAction == ECurAction.FaceToLockingEnemy)
            {
                UpdateFaceToLockingEnemy();
            }
            else if (m_CurAction == ECurAction.NormalAction)
            {
                UpdateNormalAction();
            }
            else
            {
                Debug.LogError($"Unknown current action:{m_CurAction}");
            }
        }

        public void SetPosAndForward(Vector3 tarPos, Vector3 forward, Action onFinished)
        {
            Vector3 dis = tarPos - Actor.transform.position;
            dis.y = 0;
            float speed = 5f;
            m_ToSetPosSpeed = dis.normalized * speed;

            m_CurAction = ECurAction.SetPosAndForward;
            m_ToSetForward = forward;
            m_ToSetPosTime = dis.magnitude / speed;
            m_SetPosAndForwardFinishedEvent = onFinished;
        }

        void UpdateSetPosAndForward()
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
                    m_CurAction = ECurAction.None;
                    if (m_SetPosAndForwardFinishedEvent != null)
                    {
                        GameApp.Entry.Unity.DoActionOneFrameLater(m_SetPosAndForwardFinishedEvent);
                        m_SetPosAndForwardFinishedEvent = null;
                    }
                }
            }
        }

        public void PlayAction(EActionType actionType, Action onPlayFinish)
        {
            if (actionType == EActionType.FaceToLockingEnemy)
            {
                if (Actor.AI == null || Actor.AI.LockingEnemy == null)
                {
                    m_CurAction = ECurAction.None;
                    return;
                }

                Vector3 dirToEnemy = Actor.AI.LockingEnemy.transform.position - Actor.transform.position;
                float angle = Vector3.SignedAngle(Actor.transform.forward, dirToEnemy, Vector3.up);

                if (angle > 135)
                {
                    m_CurActionName = "TurnL180";
                }
                else if (angle < -135)
                {
                    m_CurActionName = "TurnR180";
                }
                else if (angle > 0)
                {
                    m_CurActionName = "TurnL90";
                }
                else
                {
                    m_CurActionName = "TurnR90";
                }

                m_CurAction = ECurAction.FaceToLockingEnemy;
                Actor.CPhysic.ApplyRootMotion = false;
            }
            else
            {
                m_CurAction = ECurAction.NormalAction;
                m_CurActionName = actionType.ToString();
            }

            Actor.CAnim.Play(m_CurActionName);
            m_NormalActionState = ENormalActionState.WaitPlay;

            m_OnPlayActionEnd = onPlayFinish;
        }

        private void UpdateFaceToLockingEnemy()
        {
            if (m_NormalActionState == ENormalActionState.WaitPlay)
            {
                if (Actor.CAnim.IsReallyPlaying(m_CurActionName))
                {
                    m_NormalActionState = ENormalActionState.Playing;
                }
            }
            else if (m_NormalActionState == ENormalActionState.Playing)
            {
                Vector3 dirToEnemy = Actor.AI.LockingEnemy.transform.position - Actor.transform.position;
                if (Actor.CPhysic.AlignForwardTo(dirToEnemy, 360) || !Actor.CAnim.IsReallyPlaying(m_CurActionName))
                {
                    Exit();
                    m_CurAction = ECurAction.None;

                    if (m_OnPlayActionEnd != null)
                    {
                        GameApp.Entry.Unity.DoActionOneFrameLater(m_OnPlayActionEnd);
                        m_OnPlayActionEnd = null;
                    }
                }
            }
        }

        private void UpdateNormalAction()
        {
            if (m_NormalActionState == ENormalActionState.WaitPlay)
            {
                if (Actor.CAnim.IsReallyPlaying(m_CurActionName))
                {
                    m_NormalActionState = ENormalActionState.Playing;
                }
            }
            else if (m_NormalActionState == ENormalActionState.Playing)
            {
                if (m_OnPlayActionEnd != null && !Actor.CAnim.IsReallyPlaying(m_CurActionName))
                {
                    GameApp.Entry.Unity.DoActionOneFrameLater(m_OnPlayActionEnd);
                    m_OnPlayActionEnd = null;
                }

                if (Actor.CAnim.IsPlayingOrWillPlay("Idle"))
                {
                    Exit();
                    m_NormalActionState = ENormalActionState.End;
                    m_CurAction = ECurAction.None;
                }
            }
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