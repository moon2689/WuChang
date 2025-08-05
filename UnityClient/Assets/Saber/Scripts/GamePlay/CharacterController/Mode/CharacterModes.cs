using System;
using System.Collections.Generic;
using UnityEngine;

namespace Saber.CharacterController
{
    /// <summary>Mode是一种能力，如走路时和休闲时都可以吃东西</summary>
    public class CharacterModes
    {
        Dictionary<EModeType, ModeBase> m_Modes;


        public SCharacter Actor { get; private set; }
        public ModeBase CurMode { get; private set; }
        public EModeType CurModeType => CurMode != null ? CurMode.ModeType : EModeType.None;


        #region Mode operation

        public bool DrinkMedicine()
        {
            return TryEnterMode(EModeType.DrinkMedicine, null);
        }

        #endregion

        public CharacterModes(SCharacter actor)
        {
            m_Modes = new Dictionary<EModeType, ModeBase>();
            Actor = actor;
            CurMode = null;
            RegisterModes();

            Actor.CStateMachine.Event_OnStateChange += OnStateChange;
        }

        private void OnStateChange(EStateType arg1, EStateType arg2)
        {
            CurMode?.OnStateChange(arg1, arg2);
        }

        void RegisterModes()
        {
            RegisterMode(new DrinkMedicine());
        }

        void RegisterMode(ModeBase mode)
        {
            if (mode == null)
            {
                Debug.LogError("state == null");
                return;
            }

            if (m_Modes.ContainsKey(mode.ModeType))
            {
                Debug.LogError($"State type {mode.ModeType} is already exits");
                return;
            }

            mode.Init(this);
            m_Modes[mode.ModeType] = mode;
        }

        ModeBase GetMode(EModeType type)
        {
            m_Modes.TryGetValue(type, out ModeBase state);
            return state;
        }

        public T GetMode<T>(EModeType type) where T : ModeBase
        {
            return GetMode(type) as T;
        }

        public void Update()
        {
            if (CurMode != null && CurMode.IsTriggering)
            {
                CurMode.OnStay();
            }
            else
            {
                CurMode = null;
            }
            //Debug.Log($"player: {Actor.transform.name}  cur mode: {CurModeType}", Actor.gameObject);
        }

        bool TryEnterMode(EModeType next, Action onFinished)
        {
            ModeBase nextState = GetMode<ModeBase>(next);
            if (nextState == null)
            {
                Debug.LogError("Next state is null, type:" + next);
                return false;
            }

            return TryEnterMode(nextState, onFinished);
        }

        bool TryEnterMode(ModeBase nextState, Action onFinished)
        {
            if (CurMode == null && nextState != null && nextState.CanEnter)
            {
                CurMode = nextState;
                CurMode.Enter();
                CurMode.OnFinisehd = onFinished;
                return true;
            }

            return false;
        }

        bool TryEnterMode<T>(EModeType target, Action<T> beforeEnter) where T : ModeBase
        {
            if (CurMode == null)
            {
                T mode = GetMode<T>(target);
                if (mode != null && mode.CanEnter)
                {
                    beforeEnter?.Invoke(mode);
                    CurMode = mode;
                    CurMode.Enter();
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