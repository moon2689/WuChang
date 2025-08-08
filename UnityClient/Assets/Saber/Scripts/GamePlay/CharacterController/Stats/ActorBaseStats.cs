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
        public enum EStaminaRecoverSpeed
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

        public event Action OnHPPointCountChange;
        public event Action OnPowerChange;

        private const int k_MaxHPCount = 10;

        private IHandler m_Handler;

        private float m_CurrentHp,
            m_CurrentStamina;

        private float
            m_TimerRecoverSuperArmor,
            m_TimerStunBySuperArmorZero;

        private EffectObject m_HealingEffect;
        private int m_HPPotionCount;
        public int m_CurrentPower;


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

                CurrentHPRatio = m_CurrentHp / MaxHp;
                m_Handler.OnHpChange(m_CurrentHp);
            }
        }

        public int CurrentHPInt => Mathf.CeilToInt(CurrentHp);

        public float CurrentStamina
        {
            get => m_CurrentStamina;
            private set
            {
                m_CurrentStamina = Mathf.Clamp(value, 0, MaxStamina);
                CurrentStaminaRatio = m_CurrentStamina / MaxStamina;
            }
        }

        public int CurrentStaminaInt => Mathf.CeilToInt(CurrentStamina);

        public EStaminaRecoverSpeed StaminaRecoverSpeed { get; set; }
        public float CurrentSuperArmor { get; set; }
        public bool IsStaminaFull => CurrentStamina == MaxStamina;

        public int HPPotionCount
        {
            get => m_HPPotionCount;
            set
            {
                if (m_HPPotionCount == value)
                    return;

                int tar = Mathf.Clamp(value, 0, k_MaxHPCount);
                if (m_HPPotionCount == tar)
                    return;

                m_HPPotionCount = tar;
                OnHPPointCountChange?.Invoke();
            }
        }

        public bool IsHPFull => CurrentHp >= MaxHp;

        public int CurrentPower
        {
            get => m_CurrentPower;
            private set
            {
                m_CurrentPower = Mathf.Clamp(value, 0, MaxPower);
                OnPowerChange?.Invoke();
            }
        }

        public float CurrentHPRatio { get; private set; }
        public float CurrentStaminaRatio { get; private set; }
        public int MaxPower => Actor.StatsInfo.m_MaxPower;
        public int ParredTimesSum { get; set; }


        public ActorBaseStats(SActor actor)
        {
            Actor = actor;
            m_Handler = actor;
            Reset();
        }

        public virtual void Reset()
        {
            CurrentHp = MaxHp;
            CurrentStamina = MaxStamina;
            CurrentSuperArmor = Actor.StatsInfo.m_MaxSuperArmorValue;
            ResetHPPointCount();
            ResetPower();
        }

        public void ResetHPPointCount()
        {
            HPPotionCount = k_MaxHPCount;
        }

        public void ResetPower()
        {
            CurrentPower = MaxPower;
        }

        public void Update(float deltaTime)
        {
            UpdateSuperArmor(deltaTime);
            UpdateHp(deltaTime);
            UpdateStamina(deltaTime);
            UpdatePower(deltaTime);
        }

        private float m_TimerCheckPower;

        void UpdatePower(float deltaTime)
        {
            m_TimerCheckPower -= deltaTime;
            if (m_TimerCheckPower < 0)
            {
                m_TimerCheckPower = 20;
                if (CurrentPower <= 0)
                {
                    CurrentPower = MaxPower;
                }
            }
        }

        void UpdateHp(float deltaTime)
        {
            if (!Actor.IsDead && CurrentHp < MaxHp && Actor.StatsInfo.m_DefaultHpRecSpeed > 0)
            {
                CurrentHp += Actor.StatsInfo.m_DefaultHpRecSpeed * deltaTime;
            }
        }

        void UpdateStamina(float deltaTime)
        {
            if (!Actor.IsDead &&
                StaminaRecoverSpeed != EStaminaRecoverSpeed.Stop &&
                CurrentStamina < MaxStamina)
            {
                int speed = StaminaRecoverSpeed switch
                {
                    EStaminaRecoverSpeed.Stop => 0,
                    EStaminaRecoverSpeed.Slow => 30,
                    EStaminaRecoverSpeed.Medium => 50,
                    EStaminaRecoverSpeed.Fast => 80,
                    _ => 0,
                };

                CurrentStamina += speed * deltaTime;
            }
        }

        private void UpdateSuperArmor(float deltaTime)
        {
            if (CurrentSuperArmor <= 0)
            {
                if (m_TimerStunBySuperArmorZero > 0)
                {
                    m_TimerStunBySuperArmorZero -= deltaTime;
                }
                else
                {
                    Actor.IsStun = false;
                    CurrentSuperArmor = Actor.StatsInfo.m_MaxSuperArmorValue;
                }
            }
            else if (CurrentSuperArmor < Actor.StatsInfo.m_MaxSuperArmorValue)
            {
                if (m_TimerRecoverSuperArmor > 0)
                {
                    m_TimerRecoverSuperArmor -= deltaTime;
                }
                else
                {
                    CurrentSuperArmor = Actor.StatsInfo.m_MaxSuperArmorValue;
                }
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

            if (CurrentSuperArmor > 0)
            {
                m_TimerRecoverSuperArmor = 10;
                CurrentSuperArmor -= damage;
                if (CurrentSuperArmor <= 0)
                {
                    CurrentSuperArmor = 0;
                    m_TimerStunBySuperArmorZero = 3;
                    m_TimerRecoverSuperArmor = 30;
                    Actor.IsStun = true;
                }
            }
        }

        public void CostStamina(float value)
        {
            CurrentStamina -= value;
        }

        public void CostPower(int value)
        {
            CurrentPower -= value;
        }

        public void PlayHealingEffect(float hpValue)
        {
            if (IsHPFull && CurrentPower >= MaxPower)
            {
                return;
            }

            if (CurrentHp <= 0)
            {
                return;
            }

            if (m_HealingEffect == null)
            {
                GameObject effect = GameApp.Entry.Asset.LoadGameObject("Particles/Healing");
                effect.transform.parent = Actor.transform;
                effect.transform.localPosition = new Vector3(0, 0, 0);
                effect.transform.localRotation = Quaternion.identity;
                m_HealingEffect = effect.gameObject.GetComponent<EffectObject>();
            }

            m_HealingEffect.Show();
            CurrentHp += hpValue;
        }

        void OnDead()
        {
            CurrentPower = 0;
        }
    }
}