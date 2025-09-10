using System;
using System.Collections.Generic;
using CombatEditor;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    /// <summary>处理角色状态机相关事务，每个时刻必然有一个状态</summary>
    public abstract class ActorStateMachine
    {
        public event Action<EStateType, EStateType> Event_OnStateChange;


        private readonly Dictionary<EStateType, ActorStateBase> m_DicStates;

        public SActor Actor { get; private set; }
        public ActorStateBase CurrentState { get; private set; }
        public ActorStateBase PreviousState { get; private set; }
        public EStateType CurrentStateType => CurrentState != null ? CurrentState.StateType : EStateType.None;
        public EStateType PreviousStateType { get; private set; }


        protected abstract void RegisterStates();

        public ActorStateMachine(SActor actor)
        {
            m_DicStates = new Dictionary<EStateType, ActorStateBase>();
            Actor = actor;
            CurrentState = null;
            PreviousStateType = EStateType.None;
            RegisterStates();
            GameApp.Entry.Unity.DoActionOneFrameLater(() => TryEnterState(EStateType.Idle));
        }

        protected void RegisterState(ActorStateBase characterState)
        {
            if (characterState == null)
            {
                Debug.LogError("state == null");
                return;
            }

            if (m_DicStates.ContainsKey(characterState.StateType))
            {
                Debug.LogError($"State type {characterState.StateType} is already exits");
                return;
            }

            characterState.Init(this);
            m_DicStates[characterState.StateType] = characterState;
        }

        protected ActorStateBase GetState(EStateType type)
        {
            m_DicStates.TryGetValue(type, out ActorStateBase state);
            return state;
        }

        public T GetState<T>(EStateType type) where T : ActorStateBase
        {
            return GetState(type) as T;
        }

        public virtual void Update()
        {
            if (Actor.IsDead)
            {
                if (Actor.CurrentStateType != EStateType.Die)
                    Die();
            }
            /*
            else if (Actor.IsStun)
            {
                ToStun();
            }
            else if (GameApp.Entry.Config.GameSetting.TiredWhenStaminaZero && Actor.CStats.CurrentStamina <= 0)
            {
                ToTired();
            }
            */

            if (CurrentState != null && CurrentState.IsTriggering)
            {
                CurrentState.OnStay();
            }
            else
            {
                TryEnterState(EStateType.Idle);
            }
            //Debug.Log($"player: {Actor.transform.name}  cur state: {CurStateType}", Actor.gameObject);
        }

        protected bool TryEnterState(EStateType next)
        {
            ActorStateBase nextCharacterState = GetState<ActorStateBase>(next);
            if (nextCharacterState == null)
            {
                Debug.LogError($"State is null:{next}");
                return false;
            }

            return TryEnterState(nextCharacterState);
        }

        protected bool TryEnterState(ActorStateBase nextCharacterState)
        {
            if (nextCharacterState == null)
            {
                Debug.LogError("Next state is null");
                return false;
            }

            // 可以转换
            bool canSwitch = CanSwitchTo(nextCharacterState.StateType) && nextCharacterState.CanEnter;
            if (canSwitch)
            {
                if (nextCharacterState == CurrentState)
                    CurrentState.ReEnter();
                else
                    ForceEnterState(nextCharacterState);
                return true;
            }

            return false;
        }

        public void ForceEnterState(ActorStateBase nextCharacterState)
        {
            if (CurrentStateType != nextCharacterState.StateType)
            {
                PreviousStateType = CurrentStateType;
                if (CurrentState != null && CurrentState.IsTriggering)
                    CurrentState.Exit();
            }

            PreviousState = CurrentState;
            CurrentState = nextCharacterState;
            CurrentState.Enter();

            Event_OnStateChange?.Invoke(PreviousStateType, CurrentStateType);
        }

        public void ForceEnterState(EStateType nextStateType)
        {
            ForceEnterState(GetState(nextStateType));
        }

        public bool CanSwitchTo(EStateType to)
        {
            if (to != EStateType.Die && this.Actor.IsDead)
            {
                return false;
            }

            if (Actor.CAbility != null && Actor.CAbility.CurAbility != null && !Actor.CAbility.CurAbility.CanSwitchTo(to))
            {
                return false;
            }

            if (CurrentState == null || !CurrentState.IsTriggering)
            {
                return true;
            }

            EStateSwitchType canSwitchType = StateHelper.CanSwitchTo(CurrentStateType, to);
            if (canSwitchType == EStateSwitchType.CannotSwitch)
            {
                return false;
            }
            else if (canSwitchType == EStateSwitchType.CanSwitchAnyTime)
            {
                return true;
            }
            else if (canSwitchType == EStateSwitchType.WaitStateCanExit)
            {
                return CurrentState.CanExit || !CurrentState.IsTriggering;
            }
            /*
            else if (canSwitchType == EStateSwitchType.SkillCanBreak)
            {
                if (CurrentState.CanExit || !CurrentState.IsTriggering)
                {
                    return true;
                }

                if (CurrentState is SkillState skillState && skillState.IsTriggering)
                {
                    return skillState.CurSkill.CurrentAttackState == EAttackStates.BeforeAttack;
                }

                return false;
            }
            */
            else if (canSwitchType == EStateSwitchType.CanTriggerSkill)
            {
                return true;
                /*
                return to == EStateType.Skill &&
                       CurrentState is ISkillCanTrigger skillCanTrigger &&
                       skillCanTrigger.CanTriggerSkill();
                */
            }
            else if (canSwitchType == EStateSwitchType.DodgeToSprint)
            {
                if (CurrentState.CanExit || !CurrentState.IsTriggering)
                {
                    return true;
                }
                else if (Actor.MoveSpeedV == EMoveSpeedV.Sprint && CurrentState is Dodge dodge)
                {
                    return dodge.CanSwitchToSprint;
                }
                else
                {
                    return false;
                }
            }
            /*
            else if (canSwitchType == EStateSwitchType.WaitSkillCanCombo)
            {
                if (CurrentState.CanExit || !CurrentState.IsTriggering)
                {
                    return true;
                }
                else if (CurrentState is SkillState skillState)
                {
                    return skillState.CurSkill.ComboTimeEntered;
                }
                else
                {
                    return false;
                }
            }
            */
            else
            {
                throw new InvalidOperationException("Unknown switch type: " + canSwitchType);
            }
        }

        protected bool TryEnterState<T>(EStateType target, Action<T> beforeEnter) where T : ActorStateBase
        {
            bool canSwitchTo = CanSwitchTo(target);
            if (!canSwitchTo)
                return false;

            T nextCharacterState = GetState<T>(target);
            if (nextCharacterState == null)
            {
                Debug.LogError($"State is null, type:{target}");
                return false;
            }

            // 可以转换
            if (nextCharacterState.CanEnter)
            {
                beforeEnter?.Invoke(nextCharacterState);
                if (nextCharacterState == CurrentState)
                    CurrentState.ReEnter();
                else
                    ForceEnterState(nextCharacterState);
                return true;
            }

            return false;
        }

        // ------------------------------------------------------------------------------------->以下为公用状态触发调用

        public List<BaseSkill> ParriedSuccssSkills { get; set; } = new();

        public bool StartMove()
        {
            if (CurrentStateType != EStateType.Move)
                return TryEnterState(EStateType.Move);
            return false;
        }

        public virtual bool Fall(bool playFallAnim = true)
        {
            return false;
        }

        public virtual bool OnHit(DamageInfo dmgInfo)
        {
            return false;
            /*
            if (dmgInfo == null)
            {
                return false;
            }

            return TryEnterState<ObstructStateBase>(EStateType.GetHit, state => state.Damage = dmgInfo);
            */
        }

        /*
        void ToTired()
        {
            if (CurrentStateType == EStateType.GetHit)
            {
                return;
            }

            TryEnterState<ObstructStateBase>(EStateType.GetHit, state =>
            {
                state.Damage = new DamageInfo()
                {
                    ObstructType = EObstructType.Tired
                };
            });
        }

        void ToStun()
        {
            if (CurrentStateType == EStateType.GetHit)
            {
                return;
            }

            TryEnterState<ObstructStateBase>(EStateType.GetHit, state =>
            {
                state.Damage = new DamageInfo()
                {
                    ObstructType = EObstructType.Stun
                };
            });
        }
        */

        private bool Die()
        {
            return TryEnterState(EStateType.Die);
        }

        public virtual bool Dodge(Vector3 axis)
        {
            return false;
        }

        public virtual void OnParried()
        {
        }

        public virtual bool DefenseStart()
        {
            return false;
        }

        public virtual bool DefenseEnd()
        {
            if (CurrentStateType == EStateType.Defense)
            {
                if (CurrentState.CanExit)
                {
                    CurrentState.Exit();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public virtual void DefenseHit(DamageInfo dmgInfo)
        {
            if (CurrentStateType == EStateType.Defense)
            {
                Defense defense = (Defense)CurrentState;
                defense.OnHit(dmgInfo);
            }
        }

        public virtual void ForceFall(bool playFallAnim = true)
        {
            Actor.CPhysic.UseGravity = true;
        }

        public virtual bool PlayAction_IdolActive(Action onPlayed)
        {
            return false;
        }

        public virtual bool PlayAction_IdolRest(Action onPlayFinish)
        {
            return false;
        }

        public virtual bool PlayAction_IdolRestEnd(Action onPlayFinish)
        {
            return false;
        }

        public virtual bool PlayAction_BranchTeleport(Action onPlayFinish)
        {
            return false;
        }

        public virtual bool PlayAction_GoHome(Action onPlayFinish)
        {
            return false;
        }

        public virtual bool PlayAction_LookAround(Action onFinished)
        {
            onFinished?.Invoke();
            return false;
        }

        public virtual bool PlayActionWhenIdle(string name, Action onFinished)
        {
            onFinished?.Invoke();
            return false;
        }

        public virtual bool IsPlayingActionWhenIdle(string name)
        {
            return false;
        }

        public virtual bool PlayAction_TurnDirection(Vector3 targetPos, Action onFinished)
        {
            onFinished?.Invoke();
            return false;
        }

        public virtual bool Startled(Action onFinished)
        {
            return false;
        }

        /// <summary>被处决</summary>
        public bool BeExecute(SActor executioner)
        {
            IHitRecovery hitRec = (IHitRecovery)CurrentState;
            hitRec.BeExecuted(executioner);
            return true;
        }

        public virtual bool SetPosAndForward(Vector3 tarPos, Vector3 forward, Action onFinished)
        {
            return false;
        }
    }
}