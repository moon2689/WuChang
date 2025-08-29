using System;
using System.Collections.Generic;
using UnityEngine;

namespace Saber.CharacterController
{
    /// <summary>Mode是一种能力，如走路时和休闲时都可以吃东西</summary>
    public class CharacterAbility
    {
        Dictionary<EAbilityType, AbilityBase> m_Modes;


        public SCharacter Actor { get; private set; }
        public AbilityBase CurAbility { get; private set; }
        public EAbilityType CurAbilityType => CurAbility != null ? CurAbility.AbilityType : EAbilityType.None;


        #region Mode operation

        public bool DrinkMedicine()
        {
            return TryEnterMode(EAbilityType.DrinkMedicine, null);
        }

        #endregion

        public CharacterAbility(SCharacter actor)
        {
            m_Modes = new Dictionary<EAbilityType, AbilityBase>();
            Actor = actor;
            CurAbility = null;
            RegisterModes();

            Actor.CStateMachine.Event_OnStateChange += OnStateChange;
        }

        private void OnStateChange(EStateType arg1, EStateType arg2)
        {
            CurAbility?.OnStateChange(arg1, arg2);
        }

        void RegisterModes()
        {
            RegisterMode(new DrinkMedicine());
        }

        void RegisterMode(AbilityBase ability)
        {
            if (ability == null)
            {
                Debug.LogError("state == null");
                return;
            }

            if (m_Modes.ContainsKey(ability.AbilityType))
            {
                Debug.LogError($"State type {ability.AbilityType} is already exits");
                return;
            }

            ability.Init(this);
            m_Modes[ability.AbilityType] = ability;
        }

        AbilityBase GetMode(EAbilityType type)
        {
            m_Modes.TryGetValue(type, out AbilityBase state);
            return state;
        }

        public T GetMode<T>(EAbilityType type) where T : AbilityBase
        {
            return GetMode(type) as T;
        }

        public void Update()
        {
            if (CurAbility != null && CurAbility.IsTriggering)
            {
                CurAbility.OnStay();
            }
            else
            {
                CurAbility = null;
            }
            //Debug.Log($"player: {Actor.transform.name}  cur mode: {CurModeType}", Actor.gameObject);
        }

        bool TryEnterMode(EAbilityType next, Action onFinished)
        {
            AbilityBase nextState = GetMode<AbilityBase>(next);
            if (nextState == null)
            {
                Debug.LogError("Next state is null, type:" + next);
                return false;
            }

            return TryEnterMode(nextState, onFinished);
        }

        bool TryEnterMode(AbilityBase nextState, Action onFinished)
        {
            if (CurAbility == null && nextState != null && nextState.CanEnter)
            {
                CurAbility = nextState;
                CurAbility.Enter();
                CurAbility.OnFinisehd = onFinished;
                return true;
            }

            return false;
        }

        bool TryEnterMode<T>(EAbilityType target, Action<T> beforeEnter) where T : AbilityBase
        {
            if (CurAbility == null)
            {
                T mode = GetMode<T>(target);
                if (mode != null && mode.CanEnter)
                {
                    beforeEnter?.Invoke(mode);
                    CurAbility = mode;
                    CurAbility.Enter();
                    return true;
                }
            }

            return false;
        }

        public void Release()
        {
            foreach (var pair in m_Modes)
                pair.Value.Release();
            m_Modes = null;
        }
    }
}