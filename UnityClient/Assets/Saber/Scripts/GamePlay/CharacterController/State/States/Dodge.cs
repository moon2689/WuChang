using System;
using Saber.AI;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class Dodge : ActorStateBase, ISkillCanTrigger
    {
        private string m_DodgeAnim;
        private bool m_CanExit;
        private float m_TimerAlign;
        private bool m_CanTriggerSkill;
        private GameHelper.EDir4 m_Dir;
        private Vector3 m_ForwardDir;
        private SCharacter m_Character;
        private bool m_PerfectDodged;

        public override bool ApplyRootMotionSetWhenEnter => true;
        public override bool CanExit => m_CanExit;

        public override bool CanEnter
        {
            get
            {
                return Actor.CStats.CurrentStamina >= GameApp.Entry.Config.GameSetting.DodgeCostStamina &&
                       Actor.CPhysic.Grounded;
            }
        }

        public Vector3 DodgeAxis { get; set; }
        public bool CanSwitchToSprint { get; private set; }


        public void OnPerfectDodge()
        {
            if (m_PerfectDodged)
            {
                return;
            }

            m_PerfectDodged = true;
            //m_Character.CRender.ShowOneChaShadow(0.5f);
            //var setting = GameApp.Entry.Config.GameSetting;
            //Character.CRender.ShowManyChaShadow(setting.PerfectDodgeShaEffCount, setting.PerfectDodgeShaInterval, setting.PerfectDodgeShaHoldTime);
            //GameApp.Entry.Game.Audio.Play3DSound("Sound/Skill/PerfectDodge", Actor.transform.position);

            Actor.AddYuMao(1);
        }

        public Dodge() : base(EStateType.Dodge)
        {
        }

        public override void Enter()
        {
            base.Enter();

            m_CanExit = false;
            m_CanTriggerSkill = false;
            CanSwitchToSprint = false;

            Actor.CStats.CostStamina(GameApp.Entry.Config.GameSetting.DodgeCostStamina);
            Actor.CPhysic.EnableSlopeMovement = false;
            Actor.Invincible = true;

            m_DodgeAnim = GetDodgeAnim(out m_ForwardDir);
            Actor.CAnim.Play(m_DodgeAnim);

            m_TimerAlign = 0.2f;
        }

        string GetDodgeAnim(out Vector3 forwardDir)
        {
            EDodgeType dodgeType = Actor.m_BaseActorInfo.m_AIInfo.m_DodgeType;

            if (DodgeAxis != Vector3.zero)
            {
                if (Actor.AI.LockingEnemy != null)
                {
                    float angle = Vector3.SignedAngle(new Vector3(0, 0, 1), DodgeAxis, Vector3.up);
                    Vector3 moveDir = Quaternion.AngleAxis(angle, Vector3.up) * Actor.DesiredLookDir;
                    if ((angle >= 0 && angle <= 45f) || (angle <= 0 && angle >= -45f))
                    {
                        if (dodgeType.HasFlag(EDodgeType.Front))
                        {
                            m_Dir = GameHelper.EDir4.Front;
                            forwardDir = moveDir;
                        }
                        else
                        {
                            m_Dir = GameHelper.EDir4.Back;
                            forwardDir = Actor.DesiredLookDir;
                        }
                    }
                    else if (angle > 45 && angle < 135)
                    {
                        if (dodgeType.HasFlag(EDodgeType.Right))
                        {
                            m_Dir = GameHelper.EDir4.Right;
                            forwardDir = Quaternion.AngleAxis(-90, Vector3.up) * moveDir;
                        }
                        else
                        {
                            m_Dir = GameHelper.EDir4.Back;
                            forwardDir = Actor.DesiredLookDir;
                        }
                    }
                    else if (angle >= 135 || angle <= -135)
                    {
                        m_Dir = GameHelper.EDir4.Back;
                        forwardDir = -moveDir;
                    }
                    else
                    {
                        if (dodgeType.HasFlag(EDodgeType.Left))
                        {
                            m_Dir = GameHelper.EDir4.Left;
                            forwardDir = Quaternion.AngleAxis(90, Vector3.up) * moveDir;
                        }
                        else
                        {
                            m_Dir = GameHelper.EDir4.Back;
                            forwardDir = Actor.DesiredLookDir;
                        }
                    }
                }
                else
                {
                    if (dodgeType.HasFlag(EDodgeType.Front))
                    {
                        m_Dir = GameHelper.EDir4.Front;
                        forwardDir = Actor.DesiredMoveDir;
                    }
                    else
                    {
                        m_Dir = GameHelper.EDir4.Back;
                        forwardDir = Actor.DesiredLookDir;
                    }
                }
            }
            else
            {
                m_Dir = GameHelper.EDir4.Back;
                forwardDir = Actor.DesiredLookDir;
            }

            return $"Dodge{m_Dir}";
        }

        public override void OnStay()
        {
            base.OnStay();
            if (m_TimerAlign > 0)
            {
                m_TimerAlign -= Time.deltaTime;
                Actor.CPhysic.AlignForwardTo(m_ForwardDir, 1080f);
            }

            if (!Actor.CAnim.IsPlayingOrWillPlay(m_DodgeAnim, 0.95f))
            {
                Exit();
            }
        }

        public override void OnTriggerRangeEvent(AnimRangeTimeEvent eventObj, bool enter)
        {
            if (eventObj.EventType == EAnimRangeEvent.Invincible)
            {
                Actor.Invincible = enter;
            }

            if (eventObj.EventType == EAnimRangeEvent.CanTriggerSkill)
            {
                m_CanTriggerSkill = enter;
            }
        }

        public override void OnTriggerAnimEvent(AnimPointTimeEvent eventObj)
        {
            base.OnTriggerAnimEvent(eventObj);
            if (eventObj.EventType == EAnimTriggerEvent.AnimCanExit)
            {
                m_CanExit = true;
                Actor.Invincible = false;
                CanSwitchToSprint = true;
            }
            else if (eventObj.EventType == EAnimTriggerEvent.DodgeToSprint)
            {
                CanSwitchToSprint = true;
            }
        }

        protected override void OnExit()
        {
            base.OnExit();

            Actor.Invincible = false;
            Actor.CPhysic.EnableSlopeMovement = true;
            m_PerfectDodged = false;
        }

        bool ISkillCanTrigger.CanTriggerSkill(SkillItem skill)
        {
            if (m_CanExit)
            {
                return true;
            }

            if (!m_CanTriggerSkill)
            {
                return false;
            }

            if (skill.m_TriggerCondition == ETriggerCondition.InDodgeForward)
            {
                return m_Dir == GameHelper.EDir4.Front;
            }
            else if (skill.m_TriggerCondition == ETriggerCondition.InDodgeNotForward)
            {
                return m_Dir != GameHelper.EDir4.Front;
            }

            return false;
        }
    }
}