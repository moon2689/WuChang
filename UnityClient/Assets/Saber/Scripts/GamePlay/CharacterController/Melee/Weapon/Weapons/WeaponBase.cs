using System;
using System.Collections;
using System.Collections.Generic;
using Saber.Frame;
using UnityEngine;
using UnityEngine.Serialization;

namespace Saber.CharacterController
{
    public abstract class WeaponBase : MonoBehaviour
    {
        [SerializeField] private EWeaponType m_WeaponType;
        [SerializeField] private WeaponTrail[] m_WeaponTrails;
        public Transform m_PosStart;
        public Transform m_PosEnd;

        private WeaponParentInfo m_WeaponParentInfo;
        private DamageInfo m_CurDmgInfo = new();
        private WeaponDamageSetting m_WeaponDamageSetting;

        public SActor Actor { get; private set; }
        public EWeaponType WeaponType => m_WeaponType;
        public bool ResetLocation { get; set; }
        public WeaponTrail[] WeaponTrails => m_WeaponTrails;


        public ENodeType WeaponBone => m_WeaponParentInfo.m_ArmBoneType;


        private void Awake()
        {
            gameObject.SetRenderingLayerRecursive(ERenderingLayers.Actor);
        }

        public void Init(SActor actor, WeaponParentInfo weaponParentInfo)
        {
            Actor = actor;
            m_WeaponParentInfo = weaponParentInfo;
        }

        public virtual void ToggleDamage(WeaponDamageSetting damage, bool enable)
        {
            m_WeaponDamageSetting = damage;
        }

        public void DoDamage(Vector3 position, Vector3 direction, HurtBox hurtBox)
        {
            m_CurDmgInfo.DamagePosition = position;
            m_CurDmgInfo.DamageDirection = direction.normalized;
            DamageHelper.TryHit(hurtBox, this.Actor, m_WeaponDamageSetting, m_CurDmgInfo);
        }

        public void ResetParent()
        {
            if (!ResetLocation)
            {
                return;
            }

            Transform parent = m_WeaponParentInfo.m_ArmBone;
            transform.parent = parent;
            transform.localPosition = m_WeaponParentInfo.m_ArmPos;
            transform.localRotation = Quaternion.Euler(m_WeaponParentInfo.m_ArmRot);
            transform.localScale = new Vector3(1f / parent.lossyScale.x, 1f / parent.lossyScale.y, 1f / parent.lossyScale.z);
        }

        protected virtual void LateUpdate()
        {
        }

        public void ShowTrail()
        {
            for (int i = 0; i < m_WeaponTrails.Length; i++)
            {
                m_WeaponTrails[i].Show();
            }
        }

        public void HideTrail()
        {
            for (int i = 0; i < m_WeaponTrails.Length; i++)
            {
                m_WeaponTrails[i].Hide();
            }
        }
    }
}