using System;
using System.Collections.Generic;
using CombatEditor;
using UnityEngine;

namespace Saber.CharacterController
{
    [RequireComponent(typeof(CapsuleCollider))]
    public class Projectile : MonoBehaviour
    {
        enum EStage
        {
            Fly,
            Impact,
            Hide,
        }

        [SerializeField] private float m_Speed = 10;
        [SerializeField] private Vector3 m_Direction;
        [SerializeField] private GameObject m_EffectImpact;
        [SerializeField] private float m_LifeTime = 2;

        [SerializeField] private WeaponDamageSetting m_WeaponDamageSetting;

        private CapsuleCollider m_Collider;
        private DamageInfo m_CurDmgInfo = new();
        private SActor m_Actor;
        private float m_TimerHide;
        private EStage m_Stage;
        private List<SActor> m_HurtedActors = new();

        private void Awake()
        {
            m_Collider = GetComponent<CapsuleCollider>();
            m_Collider.isTrigger = true;

            gameObject.layer = (int)EStaticLayers.Actor;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (m_Stage != EStage.Fly)
            {
                return;
            }

            HurtBox hurtBox = other.GetComponent<HurtBox>();
            bool canDoDmg = hurtBox != null && !m_HurtedActors.Contains(hurtBox.Actor);
            if (!canDoDmg)
            {
                return;
            }

            m_HurtedActors.Add(hurtBox.Actor);

            // Debug.Log($"Projectile,{this.name} hit {other.name}", gameObject);
            m_CurDmgInfo.DamageDirection = transform.forward;
            m_CurDmgInfo.DamagePosition = transform.position;
            bool succeed = DamageHelper.TryHit(other, m_Actor, m_WeaponDamageSetting, m_CurDmgInfo);
            if (succeed)
            {
                Impact();
            }
            else
            {
                if (other.gameObject.layer == (int)EStaticLayers.Default)
                {
                    Impact();
                }
            }
        }

        void Impact()
        {
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

        void Hide()
        {
            gameObject.SetActive(false);
            m_Actor = null;
            m_Stage = EStage.Hide;
        }

        public void Throw(SActor owner, SActor target, float offsetAngle = 0)
        {
            NodeFollower nodeFollower = GetComponent<NodeFollower>();
            if (nodeFollower)
            {
                nodeFollower.enabled = false;
            }

            gameObject.SetActive(true);

            m_Actor = owner;
            if (target)
            {
                m_Direction = target.transform.position + Vector3.up * target.CPhysic.CenterHeight - transform.position;
            }
            else
            {
                m_Direction = owner.transform.forward;
            }

            if (offsetAngle != 0)
            {
                m_Direction = Quaternion.AngleAxis(offsetAngle, Vector3.up) * m_Direction;
            }

            m_Direction.Normalize();
            m_TimerHide = m_LifeTime;

            transform.rotation = Quaternion.LookRotation(m_Direction);

            if (m_EffectImpact)
                m_EffectImpact.SetActive(false);

            m_Collider.enabled = true;

            m_Stage = EStage.Fly;

            m_HurtedActors.Clear();
        }

        void Update()
        {
            if (m_Stage == EStage.Fly)
            {
                transform.position += m_Direction * m_Speed * Time.deltaTime;

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