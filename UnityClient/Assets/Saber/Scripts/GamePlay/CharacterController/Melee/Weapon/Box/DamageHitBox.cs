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
            bool canDoDmg = hurtBox != null && !m_HurtedActors.Contains(hurtBox.Actor);
            if (!canDoDmg)
            {
                return;
            }

            m_HurtedActors.Add(hurtBox.Actor);

            m_CurDmgInfo.DamagePosition = other.transform.position;
            Vector3 dmgDir;

            var dmgSetting = base.m_EventObj.EventObj.m_WeaponDamageSetting;
            if (dmgSetting.m_HitType == EHitType.Weapon)
            {
                var weaponBone = dmgSetting.m_WeaponBone;
                var weapon = base.Actor.CMelee.CWeapon.GetWeaponByPos(weaponBone);
                if (weapon == null)
                {
                    var eventData = m_EventObj.CurrentSkill.SkillConfig.m_AnimStates[0].m_EventData;
                    Debug.LogError($"weapon==null,bone:{weaponBone},data:{eventData.name}");
                }

                dmgDir = other.transform.position - weapon.m_PosEnd.transform.position;
            }
            else
            {
                dmgDir = other.transform.position - transform.position;
            }

            dmgDir.Normalize();
            m_CurDmgInfo.DamageDirection = dmgDir;
            DamageHelper.TryHit(other, this.Actor, dmgSetting, m_CurDmgInfo);

            //Debug.LogError($"No EmptyWeapon in bone:{base.m_EventObj.EventObj.m_WeaponDamageSetting.m_WeaponBone}");
        }

        public override void Hide()
        {
            base.Hide();
            m_HurtedActors.Clear();
        }
    }
}