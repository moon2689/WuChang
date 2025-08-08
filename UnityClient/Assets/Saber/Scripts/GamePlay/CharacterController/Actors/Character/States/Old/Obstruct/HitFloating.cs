using System;
using System.Collections;
using System.Collections.Generic;

using Saber.Frame;

using UnityEngine;

namespace Saber.CharacterController
{
    /// <summary>浮空</summary>
    public class HitFloating : ObstructBase
    {
        enum EState
        {
            None,
            HitToAir,
            HitAirToTop,
            HitFall,
            HitFallGroundAndRise,
            HitFallBounce,
            HitAirHit,
        }

        private EState m_State;
        private Vector2 m_Speed;
        private Vector3 m_DirFromAttacker;
        private float m_GravityExtraPower;
        private float m_TimerGravityExtra;
        private Vector3 m_AdjustPosSpeed;
        private float m_TimerAdjustPos;

        public bool IsRunning => m_State != EState.None;

        public HitFloating(SActor actor, Action actionExit) : base(actor, actionExit)
        {
        }

        public override void Enter(DamageInfo damageInfo)
        {
            CalcDir(damageInfo);
            HitToAir(damageInfo);
            Actor.CAnim.StopMaskLayerAnims();
        }

        void PlayAnim(string clipName, Action onFinished = null)
        {
            Actor.CAnim.PlayClip($"Animation/Hit/HitAir/{clipName}", onFinished);
        }

        void HitToAir(DamageInfo damageInfo)
        {
            PlayAnim("HitToAir");
            m_State = EState.HitToAir;
            m_Speed = Actor.CPhysic.Grounded ? damageInfo.DamageConfig.m_ForceWhenGround : damageInfo.DamageConfig.m_ForceWhenAir;

            Actor.CPhysic.UseGravity = false;
            m_GravityExtraPower = 1;
        }

        void CalcDir(DamageInfo damageInfo)
        {
            m_DirFromAttacker = Actor.transform.position - damageInfo.Attacker.transform.position;
            m_DirFromAttacker.y = 0;
            m_DirFromAttacker.Normalize();
        }

        public override void ReEnter(DamageInfo damageInfo)
        {
            CalcDir(damageInfo);

            if (damageInfo.DamageConfig.m_DamageLevel == DamageLevel.HitToAir)
            {
                HitToAir(damageInfo);
            }
            else
            {
                HitInAir(damageInfo);
            }
        }

        void HitInAir(DamageInfo damageInfo)
        {
            if (m_State == EState.HitFallGroundAndRise)
                return;

            m_Speed = damageInfo.DamageConfig.m_ForceWhenAir;
            if (m_Speed.y > 0)
            {
                PlayAnim("HitAirHit", () => PlayAnim("HitFallLoop"));
                m_State = EState.HitAirHit;
                m_TimerGravityExtra = 1;
                m_GravityExtraPower = 0.5f;

                if (damageInfo.DamageConfig.m_AlignEnemyPosWhenAirAttack)
                {
                    m_TimerAdjustPos = Mathf.Max(0.3f, damageInfo.DamageConfig.m_AlignEnemyPosSecondsWhenAirAttack);
                    float offsetX = damageInfo.Attacker.CPhysic.Radius + damageInfo.DamageConfig.m_AlignEnemyOffsetWhenAirAttack.x;
                    Vector3 offsetHor = damageInfo.Attacker.transform.forward * offsetX;
                    Vector3 offsetVer = Vector3.up * damageInfo.DamageConfig.m_AlignEnemyOffsetWhenAirAttack.y;
                    Vector3 newPos = damageInfo.Attacker.transform.position + offsetHor + offsetVer;
                    Vector3 vDistance = newPos - Actor.transform.position;
                    m_AdjustPosSpeed = vDistance / m_TimerAdjustPos;
                }
            }
        }

        public override void OnStay(DamageInfo damageInfo, float deltaTime)
        {
            if (m_TimerGravityExtra > 0)
            {
                m_TimerGravityExtra -= deltaTime;
                if (m_TimerGravityExtra <= 0)
                    m_GravityExtraPower = 1;
            }

            bool inAir = m_State == EState.HitToAir ||
                         m_State == EState.HitAirToTop ||
                         m_State == EState.HitFall;
            if (inAir)
            {
                m_Speed.y -= Time.deltaTime * Actor.CPhysic.GravityPower * m_GravityExtraPower;
                Actor.CPhysic.AdditivePosition += Vector3.up * m_Speed.y * deltaTime;
                Actor.CPhysic.AdditivePosition += m_DirFromAttacker * m_Speed.x * deltaTime;
            }

            if (m_State == EState.HitToAir)
            {
                if (m_Speed.y <= 1.5f)
                {
                    m_State = EState.HitAirToTop;
                    PlayAnim("HitAirToTop");
                }
            }
            else if (m_State == EState.HitAirToTop)
            {
                if (m_Speed.y <= 0)
                {
                    m_State = EState.HitFall;
                    PlayAnim("HitStartFall", () => PlayAnim("HitFallLoop"));
                }
            }
            else if (m_State == EState.HitFall)
            {
                if (Actor.CPhysic.Grounded)
                {
                    if (damageInfo.DamageConfig.m_ForceWhenAir.y < -30)
                    {
                        m_State = EState.HitFallBounce;
                        PlayAnim("HitFallBounceHigh", () => { PlayAnim("HitFallBounceRise", PlayGetUpAnim); });
                    }
                    else if (damageInfo.DamageConfig.m_ForceWhenAir.y < -5)
                    {
                        m_State = EState.HitFallBounce;
                        PlayAnim("HitFallBounce", () => { PlayAnim("HitFallBounceRise", PlayGetUpAnim); });
                    }
                    else
                    {
                        m_State = EState.HitFallGroundAndRise;
                        PlayAnim("HitFallToGround", PlayGetUpAnim);
                    }

                    //GameApp.Entry.Game.Audio.Play3DSound("Sound/Actor/FallLand", Actor.transform.position);
                }
            }
            else if (m_State == EState.HitFallGroundAndRise)
            {
                if (Actor.CAnim.IsPlayingOrWillPlay("IdleArmed"))
                {
                    Exit();
                }
            }
            else if (m_State == EState.HitFallBounce)
            {
                if (Actor.CAnim.IsTheClipPlaying("HitFallBounceRise"))
                {
                    m_State = EState.HitFallGroundAndRise;
                }
            }
            else if (m_State == EState.HitAirHit)
            {
                if (damageInfo.DamageConfig.m_AlignEnemyPosWhenAirAttack)
                {
                    m_TimerAdjustPos -= deltaTime;
                    Actor.CPhysic.AdditivePosition += m_AdjustPosSpeed * deltaTime;

                    if (m_TimerAdjustPos <= 0)
                    {
                        m_State = EState.HitFall;
                    }
                }
                else
                {
                    m_Speed.y -= Time.deltaTime * Actor.CPhysic.GravityPower * m_GravityExtraPower;
                    Actor.CPhysic.AdditivePosition += Vector3.up * m_Speed.y * deltaTime;
                    Actor.CPhysic.AdditivePosition += m_DirFromAttacker * m_Speed.x * deltaTime;

                    if (Actor.CAnim.IsTheClipPlaying("HitAirHit"))
                    {
                        m_State = EState.HitFall;
                    }
                }
            }
        }

        void PlayGetUpAnim()
        {
            PlayAnim("HitGetUp", Exit);
        }

        protected override void Exit()
        {
            base.Exit();
            m_State = EState.None;
        }
    }
}