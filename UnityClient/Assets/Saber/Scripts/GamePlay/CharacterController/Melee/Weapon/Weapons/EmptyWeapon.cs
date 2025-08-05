using System;
using UnityEngine;

namespace Saber.CharacterController
{
    public class EmptyWeapon : WeaponBase
    {
        private Vector3 m_LastWeaponPos, m_CurWeaponPos;
        private bool m_EnableDamage;

        public Vector3 WeaponWaveDirection => m_CurWeaponPos - m_LastWeaponPos;
        public Vector3 CurWeaponPosition => (m_CurWeaponPos + m_LastWeaponPos) * 0.5f;


        public override void ToggleDamage(WeaponDamageSetting damage, bool enable)
        {
            base.ToggleDamage(damage, enable);
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