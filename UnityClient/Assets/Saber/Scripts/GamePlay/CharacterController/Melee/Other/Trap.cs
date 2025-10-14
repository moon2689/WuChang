using System;
using System.Collections.Generic;
using CombatEditor;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class Trap : MonoBehaviour, IDamageMaker
    {
        [SerializeField] private WeaponDamageSetting m_WeaponDamageSetting;
        [SerializeField] private float m_DamageInterval = 1;

        private DamageInfo m_CurDmgInfo = new();
        private Dictionary<SActor, float> m_HurtedActors = new();


        public EActorCamp Camp => EActorCamp.Chaos;

        public WeaponBase GetWeaponByPos(ENodeType bone)
        {
            return null;
        }

        private void OnTriggerStay(Collider other)
        {
            HurtBox hurtBox = other.GetComponent<HurtBox>();
            bool canDoDmg = hurtBox != null &&
                            (!m_HurtedActors.ContainsKey(hurtBox.Actor) ||
                             Time.time - m_HurtedActors[hurtBox.Actor] >= m_DamageInterval);
            if (!canDoDmg)
            {
                return;
            }

            m_HurtedActors[hurtBox.Actor] = Time.time;

            m_CurDmgInfo.DamagePosition = other.transform.position;
            Vector3 dmgDir = other.transform.position - transform.position;
            dmgDir.Normalize();
            m_CurDmgInfo.DamageDirection = dmgDir;
            DamageHelper.TryHit(other, this, m_WeaponDamageSetting, m_CurDmgInfo);

            //Debug.LogError($"No EmptyWeapon in bone:{base.m_EventObj.EventObj.m_WeaponDamageSetting.m_WeaponBone}");
        }
    }
}