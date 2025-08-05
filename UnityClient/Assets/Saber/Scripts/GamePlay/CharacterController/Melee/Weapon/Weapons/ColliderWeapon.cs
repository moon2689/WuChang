using System;
using UnityEngine;

namespace Saber.CharacterController
{
    public class ColliderWeapon : WeaponBase
    {
        private CapsuleCollider m_CapsuleCollider;
        private Vector3 m_LastWeaponPos, m_CurWeaponPos;
        private bool m_EnableDamage;

        Vector3 WeaponWaveDirection => m_CurWeaponPos - m_LastWeaponPos;
        Vector3 CurWeaponPosition => (m_CurWeaponPos + m_LastWeaponPos) * 0.5f;


        void Awake()
        {
            m_CapsuleCollider = GetComponentInChildren<CapsuleCollider>();
            m_CapsuleCollider.isTrigger = true;
            m_CapsuleCollider.enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            //Debug.Log($"OnTriggerEnter {other.name}", other);
            HurtBox hb = other.GetComponent<HurtBox>();
            if (hb)
                base.DoDamage(CurWeaponPosition, WeaponWaveDirection, hb);
        }

        public override void ToggleDamage(WeaponDamageSetting damage, bool enable)
        {
            base.ToggleDamage(damage, enable);
            m_CapsuleCollider.enabled = enable;
            m_EnableDamage = enable;
            if (enable)
            {
                m_LastWeaponPos = m_CurWeaponPos = Vector3.Lerp(m_PosStart.position, m_PosEnd.position, 0.5f);
            }
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            if (m_EnableDamage)
            {
                m_LastWeaponPos = m_CurWeaponPos;
                m_CurWeaponPos = Vector3.Lerp(m_PosStart.position, m_PosEnd.position, 0.5f);
                //SDebug.DrawArrow(m_LastWeaponPos, WeaponWaveDirection, Color.red, 1);
            }
        }
    }
}