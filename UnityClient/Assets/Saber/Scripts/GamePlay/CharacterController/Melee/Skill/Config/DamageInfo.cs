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
        public SActor Attacker;
        public EWeaponType DamagingWeaponType;
        public WeaponDamageSetting DamageConfig;
        public HurtBox m_HurtBox;

        public float Time { get; set; }
    }

    [Serializable]
    public class WeaponDamageSetting
    {
        public ENodeType m_WeaponBone = ENodeType.RightHand;
        public float m_DamageValue = 5;
        public EImpactForce m_ImpactForce = EImpactForce.Level1;
        public EHitRecover m_HitRecover;
        public Vector2 m_ForceWhenGround;
    }

    public enum EHitRecover
    {
        Stun,
        Backstab,
        Uppercut,
        StrikeDown,
        StunTanDao,
    }
    

    public enum EImpactForce
    {
        Level0,
        Level1,
        Level2,
        Level3,
        Level4,
    }
}