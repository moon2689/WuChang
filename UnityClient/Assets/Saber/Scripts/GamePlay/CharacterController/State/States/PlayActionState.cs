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
            ShenKanActive,
            ShenKanRest,
            ShenKanRestEnd,
            FaceToLockingEnemy,
            SpecialIdle,
            SpecialIdleTransition,
            UseItemNone,
            ToDressUp,
        }

        enum ENormalActionState
        {
            WaitPlay,
            Playing,
            End,
        }

        public enum ECurAction
        {
            None,
            SetPosAndForward,
            FaceToLockingEnemy,
            NormalAction,
            SpecialIdle,
            SpecialIdleTransition,
            ToDressUp,
        }

        private string m_CurActionName;
        private Action m_OnPlayActionEnd;
        private ENormalActionState m_NormalActionState;

        private Vector3 m_ToSetForward;
        private float m_ToSetPosTime;
        private Vector3 m_ToSetPosSpeed;

        private ECurAction m_CurAction;
        private bool m_CanExit;


        public override bool ApplyRootMotionSetWhenEnter => true;
        protected override ActorBaseStats.EStaminaRecSpeed StaminaRecSpeed => ActorBaseStats.EStaminaRecSpeed.Fast;
        public ECurAction CurAction => m_CurAction;
        public override bool CanExit => m_CanExit;


        public PlayActionState() : base(EStateType.PlayAction)
        {
        }

        public override void Enter()
        {
            base.Enter();
            m_CanExit = false;
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
            else if (m_CurAction == ECurAction.SpecialIdle)
            {
            }
            else if (m_CurAction == ECurAction.SpecialIdleTransition)
            {
                UpdateNormalAction();
            }
            else if (m_CurAction == ECurAction.ToDressUp)
            {
                
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
            m_OnPlayActionEnd = onFinished;
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
                }
            }
        }

        public void PlayAction(EActionType actionType, string animName, Action onPlayFinish)
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
            else if (actionType == EActionType.SpecialIdle)
            {
                m_CurActionName = animName;
                m_CurAction = ECurAction.SpecialIdle;
            }
            else if (actionType == EActionType.SpecialIdleTransition)
            {
                if (m_CurAction != ECurAction.SpecialIdle)
                {
                    Exit();
                    return;
                }

                m_CurAction = ECurAction.NormalAction;
                m_CurActionName = $"Transition{m_CurActionName}";
            }
            else if (actionType == EActionType.ToDressUp)
            {
                ToDressUpItor(onPlayFinish).StartCoroutine();
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
                if (Actor.CPhysic.AlignForwardTo(dirToEnemy, 360))// || !Actor.CAnim.IsReallyPlaying(m_CurActionName))
                {
                    Exit();
                    m_CurAction = ECurAction.None;
                }
            }
        }

        IEnumerator ToDressUpItor(Action onPlayFinish)
        {
            Vector3 dir = GameApp.Entry.Game.World.CurrentStayingShenKan.transform.right;

            Actor.CAnim.Play("ShenKanRestEnd", force: true);
            while (Actor.CAnim.IsPlayingOrWillPlay("ShenKanRestEnd", 0.95f))
            {
                yield return null;
            }

            Actor.CAnim.Play("IdleUnarmed", force: true);
            while (!Actor.CPhysic.AlignForwardTo(dir, 360))
            {
                yield return null;
            }
            
            onPlayFinish?.Invoke();
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
            else if (eventObj.EventType == EAnimTriggerEvent.AnimCanExit)
            {
                m_CanExit = true;
            }
        }

        protected override void OnExit()
        {
            base.OnExit();
            if (m_OnPlayActionEnd != null)
            {
                GameApp.Entry.Unity.DoActionOneFrameLater(m_OnPlayActionEnd);
                m_OnPlayActionEnd = null;
            }
            Actor.CMelee.CWeapon.ShowOrHideWeapon(true);
        }
    }
}