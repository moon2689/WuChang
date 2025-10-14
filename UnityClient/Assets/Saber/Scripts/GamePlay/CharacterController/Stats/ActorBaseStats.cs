using System;
using System.Collections;
using System.Collections.Generic;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    /// <summary>存储角色属性值</summary>
    public class ActorBaseStats
    {
        public enum EStaminaRecSpeed
        {
            Stop,
            Slow,
            Medium,
            Fast,
        }

        public interface IHandler
        {
            void OnHpChange(float curHp);
            void OnDamaged(float damage);
        }

        public event Action EventOnHPPointCountChange;
        public event Action EventOnPowerChange;
        public event Action EventOnStaminaZero;

        private IHandler m_Handler;

        private float m_CurrentHp;
        private float m_CurrentStamina;
        private int m_HPPotionCount;
        public int m_CurrentPower;
        private float m_CurrentUnbalanceValue;
        private float m_TimerRecoverUnbalanceValue;


        SActor Actor { get; set; }
        public int MaxHp => Actor.StatsInfo.m_MaxHp;
        public int MaxStamina => Actor.StatsInfo.m_MaxStamina;

        public float CurrentHp
        {
            get => m_CurrentHp;
            private set
            {
                m_CurrentHp = Mathf.Clamp(value, 0, MaxHp);
                if (m_CurrentHp <= 0)
                {
                    OnDead();
                }

                m_Handler.OnHpChange(m_CurrentHp);
            }
        }

        public int CurrentHPInt => Mathf.CeilToInt(CurrentHp);

        public float CurrentStamina
        {
            get => m_CurrentStamina;
            private set => m_CurrentStamina = Mathf.Clamp(value, 0, MaxStamina);
        }

        public int CurrentStaminaInt => Mathf.CeilToInt(CurrentStamina);

        public EStaminaRecSpeed StaminaRecSpeed { get; set; }
        public bool EnableStamina { get; set; } = true;

        public int HPPotionCount
        {
            get => m_HPPotionCount;
            set
            {
                if (m_HPPotionCount == value)
                    return;

                int tar = Mathf.Clamp(value, 0, GameApp.Entry.Config.GameSetting.MaxHPPotionCount);
                if (m_HPPotionCount == tar)
                    return;

                m_HPPotionCount = tar;
                EventOnHPPointCountChange?.Invoke();
            }
        }

        public bool IsHPFull => CurrentHp >= MaxHp;

        public int CurrentPower
        {
            get => m_CurrentPower;
            private set
            {
                m_CurrentPower = Mathf.Clamp(value, 0, MaxPower);
                EventOnPowerChange?.Invoke();
            }
        }

        public int MaxPower => Actor.StatsInfo.m_MaxPower;

        public float CurrentUnbalanceValue
        {
            get => m_CurrentUnbalanceValue;
            set { m_CurrentUnbalanceValue = Mathf.Clamp(value, 0, Actor.StatsInfo.m_UnbalanceValue); }
        }


        public ActorBaseStats(SActor actor)
        {
            Actor = actor;
            m_Handler = actor;
            RecoverOrigin();
        }

        public virtual void RecoverOrigin()
        {
            CurrentHp = MaxHp;
            CurrentStamina = MaxStamina;
            DefaultHPPointCount();
            ClearPower();
            DefaultUnbalanceValue();
        }

        public void DefaultUnbalanceValue()
        {
            CurrentUnbalanceValue = Actor.StatsInfo.m_UnbalanceValue;
        }

        public void DefaultHPPointCount()
        {
            HPPotionCount = GameApp.Entry.Config.GameSetting.MaxHPPotionCount;
        }

        public void ClearPower()
        {
            CurrentPower = 0;
        }

        public void Update(float deltaTime)
        {
            if (Actor.IsDead)
            {
                return;
            }

            // 失衡值恢复
            if (m_TimerRecoverUnbalanceValue > 0)
            {
                m_TimerRecoverUnbalanceValue -= deltaTime;
            }
            else if (CurrentUnbalanceValue < Actor.StatsInfo.m_UnbalanceValue)
            {
                CurrentUnbalanceValue += deltaTime * GameApp.Entry.Config.GameSetting.RecoverUnbalanceSpeed;
            }

            // 恢复生命值
            if (CurrentHp < MaxHp && Actor.StatsInfo.m_DefaultHpRecSpeed > 0)
            {
                CurrentHp += Actor.StatsInfo.m_DefaultHpRecSpeed * deltaTime;
            }

            // 恢复体力
            if (EnableStamina && StaminaRecSpeed != EStaminaRecSpeed.Stop && CurrentStamina < MaxStamina)
            {
                int speed = StaminaRecSpeed switch
                {
                    EStaminaRecSpeed.Stop => 0,
                    EStaminaRecSpeed.Slow => 10,
                    EStaminaRecSpeed.Medium => 30,
                    EStaminaRecSpeed.Fast => 50,
                    _ => 0,
                };

                CurrentStamina += speed * deltaTime;
            }
        }


        public void TakeDamage(float damage)
        {
            if (damage <= 0)
            {
                return;
            }

            CurrentHp -= damage;
            m_Handler.OnDamaged(damage);

            CurrentUnbalanceValue -= damage;
            m_TimerRecoverUnbalanceValue = GameApp.Entry.Config.GameSetting.RecoverUnbalanceValueDelaySeconds;
        }

        public void CostStamina(float value)
        {
            if (EnableStamina)
            {
                CurrentStamina -= value;
                if (CurrentStamina <= 0)
                {
                    EventOnStaminaZero?.Invoke();
                }
            }
        }

        public void CostPower(int value)
        {
            CurrentPower -= value;
        }

        public void AddPower(int value)
        {
            CurrentPower += value;
        }

        public void AddHp(float value)
        {
            CurrentHp += value;
        }

        void OnDead()
        {
            CurrentPower = 0;
        }
    }
}