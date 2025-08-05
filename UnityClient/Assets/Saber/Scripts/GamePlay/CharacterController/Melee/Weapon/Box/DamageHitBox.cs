using System;
using CombatEditor;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class DamageHitBox : HitBox
    {
        private EmptyWeapon m_CurWeapon;
        private DamageInfo m_CurDmgInfo;

        EmptyWeapon CurWeapon
        {
            get
            {
                if (m_CurWeapon == null)
                {
                    var weaponBone = base.m_EventObj.EventObj.m_WeaponDamageSetting.m_WeaponBone;
                    var weapon = base.Actor.CMelee.CWeapon.GetWeaponByPos(weaponBone);
                    m_CurWeapon = weapon as EmptyWeapon;
                }

                return m_CurWeapon;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (CurWeapon)
            {
                HurtBox hb = other.GetComponent<HurtBox>();
                if (hb)
                    CurWeapon.DoDamage(CurWeapon.CurWeaponPosition, CurWeapon.WeaponWaveDirection, hb);
            }
            else
            {
                if (m_CurDmgInfo == null)
                    m_CurDmgInfo = new();
                DamageHelper.TryHit(other, this.Actor, base.m_EventObj.EventObj.m_WeaponDamageSetting, m_CurDmgInfo);
                //Debug.LogError($"No EmptyWeapon in bone:{base.m_EventObj.EventObj.m_WeaponDamageSetting.m_WeaponBone}");
            }
        }
    }
}