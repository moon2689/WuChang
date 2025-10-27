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
        [SerializeField] private GameObject m_EnchantedEffectFire;
        public Transform m_PosStart;
        public Transform m_PosEnd;

        private WeaponPrefab m_WeaponInfo;
        private DamageInfo m_CurDmgInfo = new();
        protected WeaponDamageSetting m_WeaponDamageSetting;
        private EEnchantedMagic m_CurrentHoldingEnchantedMagic;
        private EEnchantedMagic m_CurrentOnceEnchantedMagic;
        private float m_TimerHoldingEnchant;
        private bool m_Active;

        public SActor Actor { get; private set; }
        public EWeaponType WeaponType => m_WeaponType;
        public WeaponTrail[] WeaponTrails => m_WeaponTrails;

        public ENodeType WeaponBone => m_WeaponInfo.m_ArmBoneType;
        public Vector3 MiddlePos => (m_PosStart.position + m_PosEnd.position) / 2f;

        public bool IsEnchanted => m_CurrentHoldingEnchantedMagic != EEnchantedMagic.None ||
                                   m_CurrentOnceEnchantedMagic != EEnchantedMagic.None;

        public bool Active
        {
            get => m_Active;
            set
            {
                m_Active = value;
                gameObject.SetActive(value);
            }
        }


        private void Awake()
        {
            gameObject.SetRenderingLayerRecursive(ERenderingLayers.Actor);
        }

        public void Init(SActor actor, WeaponPrefab weaponInfo)
        {
            Actor = actor;
            m_WeaponInfo = weaponInfo;
            Active = true;
        }

        public virtual void ToggleDamage(WeaponDamageSetting damage, bool enable)
        {
            m_WeaponDamageSetting = damage;
        }

        public bool DoDamage(Vector3 position, Vector3 direction, HurtBox hurtBox)
        {
            m_CurDmgInfo.DamagePosition = position;
            m_CurDmgInfo.DamageDirection = direction.normalized;
            return DamageHelper.TryHit(hurtBox, this.Actor, m_WeaponDamageSetting, m_CurDmgInfo);
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

        #region 附魔

        public void StartEnchanted(EEnchantedMagic magic, EEnchantedStyle style, float holdSeconds)
        {
            if (magic == EEnchantedMagic.None)
            {
                return;
            }

            if (m_CurrentHoldingEnchantedMagic != EEnchantedMagic.None)
            {
                ActiveEnchantedEffect(m_CurrentHoldingEnchantedMagic, false);
            }

            if (m_CurrentOnceEnchantedMagic != EEnchantedMagic.None)
            {
                ActiveEnchantedEffect(m_CurrentOnceEnchantedMagic, false);
            }

            ActiveEnchantedEffect(magic, true);

            if (style == EEnchantedStyle.ByItem)
            {
                m_CurrentHoldingEnchantedMagic = magic;
                m_TimerHoldingEnchant = holdSeconds;
                m_CurrentOnceEnchantedMagic = EEnchantedMagic.None;
            }
            else if (style == EEnchantedStyle.ByPower)
            {
                m_CurrentOnceEnchantedMagic = magic;
            }
            else
            {
                throw new InvalidOperationException($"Unknown style:{style}");
            }
        }

        public void EndPowerEnchanted()
        {
            if (m_CurrentOnceEnchantedMagic == EEnchantedMagic.None)
            {
                return;
            }

            ActiveEnchantedEffect(m_CurrentOnceEnchantedMagic, false);
            m_CurrentOnceEnchantedMagic = EEnchantedMagic.None;

            if (m_CurrentHoldingEnchantedMagic != EEnchantedMagic.None)
            {
                ActiveEnchantedEffect(m_CurrentHoldingEnchantedMagic, true);
            }
        }

        void ActiveEnchantedEffect(EEnchantedMagic magic, bool active)
        {
            if (magic == EEnchantedMagic.None)
            {
                return;
            }

            GameObject effect = magic switch
            {
                EEnchantedMagic.Fire => m_EnchantedEffectFire,
                _ => throw new InvalidOperationException($"Unknown enchanted magic:{magic}"),
            };
            if (effect)
            {
                effect.SetActive(active);
            }
        }

        void Update()
        {
            if (m_CurrentHoldingEnchantedMagic != EEnchantedMagic.None)
            {
                m_TimerHoldingEnchant -= Time.deltaTime;
                if (m_TimerHoldingEnchant <= 0)
                {
                    ActiveEnchantedEffect(m_CurrentHoldingEnchantedMagic, false);
                    m_CurrentHoldingEnchantedMagic = EEnchantedMagic.None;
                }
            }
        }

        #endregion

        public void ShowOrHideWeapon(bool show)
        {
            if (Active)
            {
                gameObject.SetActive(show);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}