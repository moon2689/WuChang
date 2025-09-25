using System;
using System.Collections.Generic;
using System.Linq;
using CombatEditor;
using Saber.Config;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class BaseSkill
    {
        private float m_LastTriggerTime = float.MinValue;
        private int m_LastAnimHash;
        private Vector2? m_Speed;
        private bool m_AnimParamToGounedTriggered;
        private bool m_InComboTime;
        private bool m_PowerAdded;
        private EAttackStates m_CurrentAttackState;


        public event Action OnSkillTrigger;

        public SActor Actor { get; private set; }

        public bool IsTriggering { get; private set; }

        public bool InComboTime
        {
            get => m_InComboTime;
            set
            {
                m_InComboTime = value;
                if (value)
                {
                    ComboTimeEntered = true;
                }
            }
        }

        public bool ComboTimeEntered { get; private set; }

        public SkillItem SkillConfig { get; private set; }
        public bool HasCD => SkillConfig.m_CDSeconds > 0;

        public bool CanEnter
        {
            get
            {
                if (SkillConfig.CostStrength > 0 && Actor.CStats.CurrentStamina <= 0)
                {
                    return false;
                }

                if (HasCD && !IsCDCooldown)
                {
                    return false;
                }

                if (SkillConfig.m_CostPower > 0 &&
                    !SkillConfig.m_CanTriggerWhenPowerNotEnough &&
                    Actor.CStats.CurrentPower < SkillConfig.m_CostPower)
                {
                    return false;
                }

                ETriggerCondition condition = SkillConfig.m_TriggerCondition;
                switch (condition)
                {
                    case ETriggerCondition.InGround:
                        return Actor.CPhysic.Grounded;

                    case ETriggerCondition.InAir:
                        return !Actor.CPhysic.Grounded &&
                               Actor.CPhysic.GroundDistance > 1 &&
                               (Actor.CurrentStateType == EStateType.Fall ||
                                Actor.CurrentStateType == EStateType.Skill);

                    case ETriggerCondition.InSprint:
                    case ETriggerCondition.InDodgeForward:
                    case ETriggerCondition.InDodgeNotForward:
                    case ETriggerCondition.AfterTanFanSucceed:
                        // 在状态机中判断
                        return true;

                    default:
                        throw new InvalidOperationException($"Unknown condition:{condition}");
                }
            }
        }

        public bool IsCDCooldown
        {
            get
            {
                if (!HasCD)
                {
                    return true;
                }

                if (GameApp.Entry.Config.TestGame.DebugFight)
                {
                    return true;
                }

                return Time.time - m_LastTriggerTime > SkillConfig.m_CDSeconds;
            }
        }

        public bool ComboTimePassed { get; set; }
        public bool CanExit { get; set; }

        public EAttackStates CurrentAttackState
        {
            get => m_CurrentAttackState;
            set
            {
                m_CurrentAttackState = value;
                Actor.CurrentResilience = value switch
                {
                    EAttackStates.BeforeAttack => SkillConfig.m_ResilienceBeforeAttack,
                    EAttackStates.Attacking => SkillConfig.m_ResilienceAttacking,
                    EAttackStates.AfterAttack => SkillConfig.m_ResilienceAfterAttack,
                    _ => throw new InvalidOperationException($"Unknown attack state:{value}"),
                };
            }
        }

        public float CDProgress
        {
            get
            {
                if (!HasCD)
                {
                    return 0;
                }

                float p = CDLeftSeconds / SkillConfig.m_CDSeconds;
                p = Mathf.Clamp01(p);
                return p;
            }
        }

        public float CDLeftSeconds
        {
            get
            {
                if (!HasCD)
                {
                    return 0;
                }

                float cooldownTime = m_LastTriggerTime + SkillConfig.m_CDSeconds;
                return cooldownTime - Time.time;
            }
        }

        public virtual bool InPerfectDodgeTime => false;
        //public virtual bool InTanDaoTime => false;

        /// <summary>是否安静，不破除潜行状态</summary>
        public virtual bool IsQuiet => false;

        public bool IsPowerEnough { get; private set; }
        public float BlendTime { get; set; } = 0.1f;


        public BaseSkill(SActor actor, SkillItem skillConfig)
        {
            Actor = actor;
            SkillConfig = skillConfig;
        }

        protected virtual void PlayAnimOnEnter(string firstAnim, string endAnim)
        {
            m_LastAnimHash = endAnim.GetAnimatorHash();
            Actor.CAnim.Play(firstAnim, force: true, blendTime: BlendTime);
        }

        public virtual void Enter()
        {
            InComboTime = false;
            IsTriggering = true;
            CanExit = false;
            ComboTimePassed = false;
            CurrentAttackState = EAttackStates.BeforeAttack;
            m_LastTriggerTime = Time.time;
            m_Speed = null;
            m_AnimParamToGounedTriggered = false;
            m_PowerAdded = false;

            IsPowerEnough = Actor.CStats.CurrentPower >= SkillConfig.m_CostPower;
            if (SkillConfig.m_CostPower > 0 && IsPowerEnough)
            {
                Actor.CostYuMao(SkillConfig.m_CostPower);
            }

            // play anim
            if (SkillConfig.m_AnimStates.Length > 0)
            {
                string firstAnimName = SkillConfig.m_AnimStates[0].m_Name;
                string lastAnimName = SkillConfig.m_AnimStates[^1].m_Name;
                PlayAnimOnEnter(firstAnimName, lastAnimName);
            }

            if (SkillConfig.IsAirSkill)
            {
                Actor.CPhysic.UseGravity = SkillConfig.UseGravityWhenInAir;
            }
            else
            {
                Actor.CPhysic.UseGravity = true;
            }

            Actor.CStats.CostStamina(SkillConfig.CostStrength);

            /*
            if (Actor.CurrentWeapons != null)
            {
                for (int i = 0; i < Actor.CurrentWeapons.Length; i++)
                    Actor.CurrentWeapons[i].OnSKillEnter();
            }
            */

            // Debug.Log($"enter skill,{SkillConfig.m_ID}");

            OnSkillTrigger?.Invoke();
        }

        void UpdateOwnerForce()
        {
            if (!SkillConfig.IsAirSkill)
                return;

            if (m_Speed != null)
            {
                float y = m_Speed.Value.y - Time.deltaTime * Actor.CPhysic.GravityPower;
                Actor.CPhysic.AdditivePosition += Vector3.up * y * Actor.DeltaTime;
                Actor.CPhysic.AdditivePosition += Actor.transform.forward * m_Speed.Value.x * Actor.DeltaTime;
                m_Speed = new Vector2(m_Speed.Value.x, y);
            }

            if (!m_AnimParamToGounedTriggered && Actor.CPhysic.Grounded)
            {
                if (m_Speed != null)
                {
                    m_Speed = new Vector2(m_Speed.Value.x, 0);
                }

                m_AnimParamToGounedTriggered = true;
                Actor.CAnim.SetTrigger(EAnimatorParams.ToGround);
            }
        }

        public void AddOwnerForce(Vector2 force)
        {
            m_Speed = force;
        }

        public virtual void OnStay()
        {
            /*
            if (Actor.CurrentWeapons != null)
            {
                for (int i = 0; i < Actor.CurrentWeapons.Length; i++)
                    Actor.CurrentWeapons[i].OnSkillStay();
            }
            */

            UpdateOwnerForce();
        }

        public virtual void OnAnimEnter(int nameHash, int layer)
        {
        }

        public virtual void OnAnimExit(int nameHash, int layer)
        {
            if (layer != 0)
                return;

            if (nameHash == m_LastAnimHash)
            {
                Exit();
            }
        }

        public virtual void Exit()
        {
            if (!IsTriggering)
            {
                return;
            }

            IsTriggering = false;
            Actor.CPhysic.UseGravity = true;

            /*
            if (Actor.CurrentWeapons != null)
            {
                for (int i = 0; i < Actor.CurrentWeapons.Length; i++)
                    Actor.CurrentWeapons[i].OnSkillExit();
            }
            */

            // Debug.Log($"exit skill,{SkillConfig.m_ID}, time:{Actor.CAnim.GetAnimNormalizedTime(0)}");
        }

        /// <summary>在完美闪避范围内</summary>
        public virtual bool InPerfectDodgeRange(SActor target)
        {
            return false;
        }

        public virtual void OnTriggerAnimEvent(AnimPointTimeEvent eventObj)
        {
        }

        public virtual void OnTriggerRangeEvent(AnimRangeTimeEvent eventObj, bool enter)
        {
        }

        public void OnDamageDone()
        {
            if (!m_PowerAdded && SkillConfig.m_PowerAddWhenHitted > 0)
            {
                m_PowerAdded = true;
                Actor.AddYuMao(SkillConfig.m_PowerAddWhenHitted);
            }
        }
    }
}