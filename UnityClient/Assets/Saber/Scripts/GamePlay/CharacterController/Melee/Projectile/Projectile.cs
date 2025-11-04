using System;
using System.Collections.Generic;
using CombatEditor;
using UnityEngine;

namespace Saber.CharacterController
{
    [RequireComponent(typeof(CapsuleCollider))]
    public abstract class Projectile : MonoBehaviour
    {
        enum EStage
        {
            Fly,
            Impact,
            Hide,
        }

        [SerializeField] private GameObject m_EffectFly;
        [SerializeField] private GameObject m_EffectImpact;
        [SerializeField] private float m_LifeTime = 2;
        [SerializeField] private WeaponDamageSetting m_WeaponDamageSetting;
        [SerializeField] protected float m_Speed = 15;
        [SerializeField] private bool m_DestroyOnHit = true;

        private CapsuleCollider m_Collider;
        private DamageInfo m_CurDmgInfo = new();
        private SActor m_Actor;
        private float m_TimerHide;
        private EStage m_Stage;
        private List<SActor> m_HurtedActors = new();


        protected abstract void Fly();

        private void Awake()
        {
            m_Collider = GetComponent<CapsuleCollider>();
            m_Collider.isTrigger = true;

            Rigidbody rb = GetComponent<Rigidbody>();
            if (!rb)
                rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.mass = 1;

            gameObject.layer = (int)EStaticLayers.Actor;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == (int)EStaticLayers.Default)
            {
                if (m_DestroyOnHit)
                    Impact();
                return;
            }

            HurtBox hurtBox = other.GetComponent<HurtBox>();
            bool canDoDmg = hurtBox && hurtBox.Actor != m_Actor && !m_HurtedActors.Contains(hurtBox.Actor);
            if (!canDoDmg)
            {
                return;
            }

            if (m_DestroyOnHit)
                Impact();

            m_HurtedActors.Add(hurtBox.Actor);

            // Debug.Log($"Projectile,{this.name} hit {other.name}", gameObject);
            m_CurDmgInfo.DamageDirection = transform.forward;
            m_CurDmgInfo.DamagePosition = transform.position;
            DamageHelper.TryHit(other, m_Actor, m_WeaponDamageSetting, m_CurDmgInfo);
        }

        protected void Impact()
        {
            m_EffectFly.SetActive(false);
            if (m_EffectImpact)
            {
                m_EffectImpact.SetActive(true);
                m_Collider.enabled = false;
                m_Stage = EStage.Impact;
                m_TimerHide = 0.5f;
            }
            else
            {
                Hide();
            }
        }

        protected void Hide()
        {
            gameObject.SetActive(false);
            m_Stage = EStage.Hide;
        }

        public virtual void Throw(SActor owner, SActor target)
        {
            NodeFollower nodeFollower = GetComponent<NodeFollower>();
            if (nodeFollower)
            {
                nodeFollower.enabled = false;
            }

            gameObject.SetActive(true);
            m_EffectFly.SetActive(true);

            m_Actor = owner;

            m_TimerHide = m_LifeTime;

            if (m_EffectImpact)
                m_EffectImpact.SetActive(false);

            m_Collider.enabled = true;

            m_Stage = EStage.Fly;

            m_HurtedActors.Clear();
        }

        protected virtual void Update()
        {
            if (m_Stage == EStage.Fly)
            {
                Fly();

                m_TimerHide -= Time.deltaTime;
                if (m_TimerHide < 0)
                {
                    Hide();
                }
            }
            else if (m_Stage == EStage.Impact)
            {
                m_TimerHide -= Time.deltaTime;
                if (m_TimerHide < 0)
                {
                    Hide();
                }
            }
            else if (m_Stage == EStage.Hide)
            {
            }
        }
    }
}