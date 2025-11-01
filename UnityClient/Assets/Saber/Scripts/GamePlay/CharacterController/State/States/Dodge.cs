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
        private bool m_AlignDirection;
        private bool m_CanTriggerSkill;
        private GameHelper.EDir4 m_Dir;
        private Vector3 m_ForwardDir;
        private SCharacter m_Character;

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
        public string[] SpecialDodgeBackAnims { get; set; }
        public string[] SpecialDodgeLeftAnims { get; set; }
        public string[] SpecialDodgeRightAnims { get; set; }
        public string[] SpecialDodgeFrontAnims { get; set; }


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
            Actor.AddYuMaoWhenHitted = true;

            m_DodgeAnim = GetDodgeAnim(out m_ForwardDir);
            Actor.CAnim.Play(m_DodgeAnim);
        }

        string GetDodgeAnim(out Vector3 forwardDir)
        {
            if (DodgeAxis != Vector3.zero)
            {
                if (Actor.AI.LockingEnemy != null)
                {
                    float angle = Vector3.SignedAngle(new Vector3(0, 0, 1), DodgeAxis, Vector3.up);
                    Vector3 moveDir = Quaternion.AngleAxis(angle, Vector3.up) * Actor.DesiredLookDir;
                    if ((angle >= 0 && angle <= 45f) || (angle <= 0 && angle >= -45f))
                    {
                        m_Dir = GameHelper.EDir4.Front;
                        forwardDir = moveDir;
                    }
                    else if (angle > 45 && angle < 135)
                    {
                        m_Dir = GameHelper.EDir4.Right;
                        forwardDir = Quaternion.AngleAxis(-90, Vector3.up) * moveDir;
                    }
                    else if (angle >= 135 || angle <= -135)
                    {
                        m_Dir = GameHelper.EDir4.Back;
                        forwardDir = -moveDir;
                    }
                    else
                    {
                        m_Dir = GameHelper.EDir4.Left;
                        forwardDir = Quaternion.AngleAxis(90, Vector3.up) * moveDir;
                    }
                }
                else
                {
                    m_Dir = GameHelper.EDir4.Front;
                    forwardDir = Actor.DesiredMoveDir;
                }
            }
            else
            {
                m_Dir = GameHelper.EDir4.Back;
                forwardDir = Actor.DesiredLookDir;
            }

            string[] specialAnims = m_Dir switch
            {
                GameHelper.EDir4.Back => SpecialDodgeBackAnims,
                GameHelper.EDir4.Left => SpecialDodgeLeftAnims,
                GameHelper.EDir4.Right => SpecialDodgeRightAnims,
                GameHelper.EDir4.Front => SpecialDodgeFrontAnims,
                _ => throw new InvalidOperationException()
            };

            if (specialAnims != null && specialAnims.Length > 0 && GameHelper.CalcProbability(30))
            {
                int ranIndex = UnityEngine.Random.Range(0, specialAnims.Length);
                return specialAnims[ranIndex];
            }

            return $"Dodge{m_Dir}";
        }

        public override void OnStay()
        {
            base.OnStay();
            if (m_AlignDirection)
            {
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
                if (!enter)
                {
                    Actor.AddYuMaoWhenHitted = false;
                }
            }
            else if (eventObj.EventType == EAnimRangeEvent.CanTriggerSkill)
            {
                m_CanTriggerSkill = enter;
            }
            else if (eventObj.EventType == EAnimRangeEvent.AlignDirection)
            {
                m_AlignDirection = enter;
            }
        }

        public override void OnTriggerAnimEvent(AnimPointTimeEvent eventObj)
        {
            base.OnTriggerAnimEvent(eventObj);
            if (eventObj.EventType == EAnimTriggerEvent.AnimCanExit)
            {
                m_CanExit = true;
                Actor.Invincible = false;
                Actor.AddYuMaoWhenHitted = false;
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
            Actor.AddYuMaoWhenHitted = false;
            Actor.CPhysic.EnableSlopeMovement = true;
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