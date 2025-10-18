using System;
using System.Collections.Generic;
using Saber.Config;
using UnityEngine;

namespace Saber.CharacterController
{
    /// <summary>Mode是一种能力，如走路时和休闲时都可以吃东西</summary>
    public class CharacterAbility
    {
        Dictionary<EAbilityType, AbilityBase> m_Abilities;


        public SCharacter Actor { get; private set; }
        public AbilityBase CurAbility { get; private set; }
        public EAbilityType CurAbilityType => CurAbility != null ? CurAbility.AbilityType : EAbilityType.None;


        #region Operation

        void RegisterAbilities()
        {
            RegisterAbility(new DrinkMedicine());
            RegisterAbility(new Eat());
            RegisterAbility(new EnchantWeapon());
        }

        public bool DrinkMedicine()
        {
            return TryEnterAbility<DrinkMedicine>(EAbilityType.DrinkMedicine, null);
        }

        public bool Eat(Action onEated)
        {
            return TryEnterAbility<Eat>(EAbilityType.Eat, a => { a.OnEated = onEated; });
        }

        public bool EnchantByItem(PropItemInfo item)
        {
            return TryEnterAbility<EnchantWeapon>(EAbilityType.EnchantWeapon, a =>
            {
                a.Magic = (EEnchantedMagic)((int)item.m_Param1);
                a.HoldSeconds = item.m_Param2;
            });
        }

        #endregion

        public CharacterAbility(SCharacter actor)
        {
            m_Abilities = new Dictionary<EAbilityType, AbilityBase>();
            Actor = actor;
            CurAbility = null;
            RegisterAbilities();

            Actor.CStateMachine.Event_OnStateChange += OnStateChange;
        }

        private void OnStateChange(EStateType arg1, EStateType arg2)
        {
            CurAbility?.OnStateChange(arg1, arg2);
        }

        void RegisterAbility(AbilityBase ability)
        {
            if (ability == null)
            {
                Debug.LogError("state == null");
                return;
            }

            if (m_Abilities.ContainsKey(ability.AbilityType))
            {
                Debug.LogError($"State type {ability.AbilityType} is already exits");
                return;
            }

            ability.Init(this);
            m_Abilities[ability.AbilityType] = ability;
        }

        AbilityBase GetAbility(EAbilityType type)
        {
            m_Abilities.TryGetValue(type, out AbilityBase state);
            return state;
        }

        public T GetAbility<T>(EAbilityType type) where T : AbilityBase
        {
            return GetAbility(type) as T;
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

        bool TryEnterAbility<T>(EAbilityType next, Action<T> onEnter) where T : AbilityBase
        {
            T nextState = GetAbility<T>(next);
            if (nextState == null)
            {
                Debug.LogError("Next state is null, type:" + next);
                return false;
            }

            return TryEnterAbility(nextState, onEnter);
        }

        bool TryEnterAbility<T>(T nextState, Action<T> onEnter) where T : AbilityBase
        {
            if (CurAbility == null && nextState != null && nextState.CanEnter)
            {
                CurAbility = nextState;
                CurAbility.Enter();
                onEnter?.Invoke(nextState);
                return true;
            }

            return false;
        }

        /*
        bool TryEnterAbility<T>(EAbilityType target, Action<T> beforeEnter) where T : AbilityBase
        {
            if (CurAbility == null)
            {
                T mode = GetAbility<T>(target);
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
        */

        public void Release()
        {
            foreach (var pair in m_Abilities)
                pair.Value.Release();
            m_Abilities = null;
        }
    }
}