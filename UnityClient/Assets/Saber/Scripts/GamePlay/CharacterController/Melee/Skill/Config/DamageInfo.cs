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
        public IDamageMaker Attacker;
        public EHitType HitType;
        public EWeaponType DamagingWeaponType;
        public WeaponDamageSetting DamageConfig;
        public HurtBox m_HurtBox;
    }

    [Serializable]
    public class WeaponDamageSetting
    {
        public EHitType m_HitType = EHitType.Weapon;
        public ENodeType m_WeaponBone = ENodeType.WeaponRightHand;
        public float m_DamageValue = 30;
        public EImpactForce m_ImpactForce = EImpactForce.Level2;
        public EHitRecover m_HitRecover;
        public Vector2 m_ForceWhenGround;
        public bool CanBeTanFan = true;
        public bool BreakByTanFan = true;
    }

    public enum EHitType
    {
        Weapon,
        Boxing,
        Leg,
        Magic,
        FeiDao,
    }

    public enum EHitRecover
    {
        Stun, //普通击晕，四方向
        Backstab, //背刺
        Uppercut, //挑飞
        StrikeDown, //砸趴
        StunTanDao, //被弹反
        KnockOffLongDis, //打飞
        StunLarge, //重度击晕，二方向
    }

    public enum EImpactForce
    {
        Level1,
        Level2,
        Level3,
        Level4,
        Level5,
    }
}