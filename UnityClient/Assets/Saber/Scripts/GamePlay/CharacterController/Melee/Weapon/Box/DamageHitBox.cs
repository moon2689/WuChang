using System;
using System.Collections.Generic;
using CombatEditor;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class DamageHitBox : HitBox
    {
        //private EmptyWeapon m_CurWeapon;
        private DamageInfo m_CurDmgInfo = new();
        private List<SActor> m_HurtedActors = new();

        /*
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
        */

        private void OnTriggerEnter(Collider other)
        {
            /*
            if (CurWeapon)
            {
                HurtBox hb = other.GetComponent<HurtBox>();
                if (hb)
                    CurWeapon.DoDamage(CurWeapon.CurWeaponPosition, CurWeapon.WeaponWaveDirection, hb);
            }
            else
            */

            HurtBox hurtBox = other.GetComponent<HurtBox>();
            if (hurtBox != null && !m_HurtedActors.Contains(hurtBox.Actor))
            {
                m_HurtedActors.Add(hurtBox.Actor);
                var dmgSetting = base.m_EventObj.EventObj.m_WeaponDamageSetting;
                DamageHelper.TryHit(other, this.Actor, dmgSetting, m_CurDmgInfo);
                //Debug.LogError($"No EmptyWeapon in bone:{base.m_EventObj.EventObj.m_WeaponDamageSetting.m_WeaponBone}");
            }
        }

        public override void Hide()
        {
            base.Hide();
            m_HurtedActors.Clear();
        }
    }
}