using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Saber.CharacterController
{
    public class DamageInfo
    {
        public float DamageValue;
        public Vector3 DamagePosition;
        public Vector3 DamageDirection;
        public EObstructType ObstructType;
        public SActor Attacker;
        public EWeaponType DamagingWeaponType;
        public WeaponDamageSetting DamageConfig;
        public HurtBox m_HurtBox;
        
        public float Time { get; set; }

        public void Clear()
        {
            DamageValue = 0;
            DamagePosition = Vector3.zero;
            DamageDirection = Vector3.zero;
            ObstructType = EObstructType.Normal;
            Attacker = null;
            DamageConfig = null;
        }
    }

    public enum EObstructType
    {
        Normal,
        Execute,
        DefenseBroken,
        Parried,
        Tired,
        Stun,
    }

    [Serializable]
    public class WeaponDamageSetting
    {
        public ENodeType m_WeaponBone = ENodeType.RightHand;
        public float m_DamageValue = 5;
        public DamageLevel m_DamageLevel;
        public Vector2 m_ForceWhenGround;
        public Vector2 m_ForceWhenAir;
        public Vector2 m_OwnerForce;
        public bool m_AlignEnemyPosWhenAirAttack;
        public Vector2 m_AlignEnemyOffsetWhenAirAttack = new Vector2(1, 0);
        public float m_AlignEnemyPosSecondsWhenAirAttack = 0.3f;
    }

    // 冲击力等级
    public enum DamageLevel
    {
        HitLight,
        HitHeavy,
        HitDown,
        HitFly,

        Normal, //普通攻击
        HitToAir, //挑飞到空中
        KnockDownSpin, //打飞并在空中翻转
        Large, //大的攻击
        StrikeDown, //砸趴
    }
}