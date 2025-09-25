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

        private WeaponPrefab m_WeaponInfo;
        private DamageInfo m_CurDmgInfo = new();
        protected WeaponDamageSetting m_WeaponDamageSetting;

        public SActor Actor { get; private set; }
        public EWeaponType WeaponType => m_WeaponType;
        public WeaponTrail[] WeaponTrails => m_WeaponTrails;

        public ENodeType WeaponBone => m_WeaponInfo.m_ArmBoneType;
        public Vector3 MiddlePos => (m_PosStart.position + m_PosEnd.position) / 2f;


        private void Awake()
        {
            gameObject.SetRenderingLayerRecursive(ERenderingLayers.Actor);
        }

        public void Init(SActor actor, WeaponPrefab weaponInfo)
        {
            Actor = actor;
            m_WeaponInfo = weaponInfo;
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

        public void EquipWeapon()
        {
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            if (rb)
            {
                Destroy(rb);
            }

            var c = gameObject.GetComponent<Collider>();
            if (c)
            {
                c.enabled = false;
            }

            Transform parent = Actor.GetNodeTransform(m_WeaponInfo.m_ArmBoneType);
            transform.SetParent(parent);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
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

        public void FallToGround()
        {
            var c = gameObject.GetComponent<Collider>();
            c.enabled = true;

            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            if (!rb)
                rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 3;
            rb.useGravity = true;
            rb.isKinematic = false;
            transform.SetParent(null);
        }
    }
}