using UnityEngine;

namespace Saber.CharacterController
{
    public class MonsterDodge : ActorStateBase
    {
        private string m_DodgeAnim;
        private bool m_CanExit;
        private float m_TimerAlign;

        public override bool ApplyRootMotionSetWhenEnter => true;
        public override bool CanExit => m_CanExit;

        public override bool CanEnter
        {
            get { return Actor.CStats.CurrentStamina > 0 && Actor.CPhysic.Grounded; }
        }

        public Vector3 DodgeAxis { get; set; }


        public MonsterDodge() : base(EStateType.Dodge)
        {
        }

        public override void Enter()
        {
            base.Enter();

            m_CanExit = false;

            Actor.CStats.CostStamina(5);

            GameHelper.EDir4 dir = GameHelper.EDir4.Back;
            if (DodgeAxis != Vector3.zero)
            {
                dir = DodgeAxis.Calc4Dir(new Vector3(0, 0, 1), out _);
            }

            if (dir == GameHelper.EDir4.Front)
            {
                dir = GameHelper.EDir4.Left;
            }

            m_DodgeAnim = $"Dodge{dir}";

            Actor.CAnim.Play(m_DodgeAnim, onFinished: Exit);

            m_TimerAlign = 0.1f;
        }

        public override void OnStay()
        {
            base.OnStay();
            if (m_TimerAlign > 0)
            {
                m_TimerAlign -= Time.deltaTime;
                Actor.CPhysic.AlignForwardTo(Actor.DesiredLookDir, 1080f);
            }
        }

        public override void OnTriggerRangeEvent(AnimRangeTimeEvent eventObj, bool enter)
        {
            if (eventObj.EventType == EAnimRangeEvent.Invincible)
                Actor.Invincible = enter;
        }

        public override void OnTriggerAnimEvent(AnimPointTimeEvent eventObj)
        {
            base.OnTriggerAnimEvent(eventObj);
            if (eventObj.EventType == EAnimTriggerEvent.AnimCanExit)
            {
                m_CanExit = true;
                Actor.Invincible = false;
            }
        }

        protected override void OnExit()
        {
            base.OnExit();
            Actor.Invincible = false;
        }
    }
}