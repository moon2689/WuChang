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

            if (DodgeAxis != Vector3.zero)
            {
                if (Actor.AI.LockingEnemy != null)
                {
                    float angle = Vector3.SignedAngle(new Vector3(0, 0, 1), DodgeAxis, Vector3.up);
                    Vector3 moveDir = Quaternion.AngleAxis(angle, Vector3.up) * Actor.DesiredLookDir;
                    if ((angle >= 0 && angle <= 45f) || (angle <= 0 && angle >= -45f))
                    {
                        m_Dir = GameHelper.EDir4.Front;
                        m_ForwardDir = moveDir;
                    }
                    else if (angle > 45 && angle < 135)
                    {
                        m_Dir = GameHelper.EDir4.Right;
                        m_ForwardDir = Quaternion.AngleAxis(-90, Vector3.up) * moveDir;
                    }
                    else if (angle >= 135 || angle <= -135)
                    {
                        m_Dir = GameHelper.EDir4.Back;
                        m_ForwardDir = -moveDir;
                    }
                    else
                    {
                        m_Dir = GameHelper.EDir4.Left;
                        m_ForwardDir = Quaternion.AngleAxis(90, Vector3.up) * moveDir;
                    }
                }
                else
                {
                    m_Dir = GameHelper.EDir4.Front;
                    m_ForwardDir = Actor.DesiredMoveDir;
                }
            }
            else
            {
                m_Dir = GameHelper.EDir4.Back;
                m_ForwardDir = Actor.DesiredLookDir;
            }

            m_DodgeAnim = $"Dodge{m_Dir}";

            Actor.CAnim.Play(m_DodgeAnim);

            m_TimerAlign = 0.2f;
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
        }

        bool ISkillCanTrigger.CanTriggerSkill(SkillItem skill)
        {
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