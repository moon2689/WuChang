namespace Saber.CharacterController
{
    public abstract class ActorStateBase
    {
        public virtual bool ApplyRootMotionSetWhenEnter => false;
        public virtual bool CanEnter => true;
        public virtual bool CanExit => true;

        public bool IsTriggering { get; private set; }

        protected ActorStateMachine StateMachine { get; private set; }

        protected SActor Actor => StateMachine.Actor;

        public EStateType StateType { private set; get; }
        public float DeltaTime => Actor.DeltaTime;

        protected virtual ActorBaseStats.EStaminaRecoverSpeed StaminaRecoverSpeed =>
            ActorBaseStats.EStaminaRecoverSpeed.Stop;


        public ActorStateBase(EStateType type)
        {
            StateType = type;
            IsTriggering = false;
        }

        public virtual void Init(ActorStateMachine parent)
        {
            StateMachine = parent;
        }

        public virtual void ReEnter()
        {
            // if (Actor is SCharacter)
            //     UnityEngine.Debug.Log("ReEnter:" + StateType);
        }

        public virtual void Enter()
        {
            IsTriggering = true;
            Actor.CPhysic.ApplyRootMotion = ApplyRootMotionSetWhenEnter;
            Actor.CStats.StaminaRecoverSpeed = StaminaRecoverSpeed;

            // if (Actor is SCharacter)
            //     UnityEngine.Debug.Log("Enter:" + StateType);
        }

        public virtual void OnStay()
        {
        }

        public void Exit()
        {
            if (IsTriggering)
            {
                IsTriggering = false;
                OnExit();
            }
        }

        protected virtual void OnExit()
        {
            // if (Actor is SCharacter)
            //     UnityEngine.Debug.Log("On Exit:" + StateType);
        }

        public virtual void OnTriggerAnimEvent(AnimPointTimeEvent eventObj)
        {
        }

        public virtual void OnTriggerRangeEvent(AnimRangeTimeEvent eventObj, bool enter)
        {
        }

        public virtual void OnTriggerAnimClipEvent(string str)
        {
        }

        public virtual void OnAnimEnter(int nameHash, int layer)
        {
        }

        public virtual void OnAnimExit(int nameHash, int layer)
        {
        }
    }
}