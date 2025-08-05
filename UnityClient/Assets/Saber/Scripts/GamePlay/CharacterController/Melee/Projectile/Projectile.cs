using System;
using CombatEditor;
using UnityEngine;

namespace Saber.CharacterController
{
    [RequireComponent(typeof(SphereCollider))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float m_Speed = 10;
        [SerializeField] private Vector3 m_Direction;
        [SerializeField] private bool m_Flying;
        [SerializeField] private WeaponDamageSetting m_WeaponDamageSetting;

        private SphereCollider m_SphereCollider;
        private DamageInfo m_CurDmgInfo = new();
        private SActor m_Actor;
        private float m_TimerDestroy;

        private void Awake()
        {
            m_SphereCollider = GetComponent<SphereCollider>();
            m_SphereCollider.isTrigger = true;

            gameObject.layer = (int)EStaticLayers.Actor;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!m_Flying)
            {
                return;
            }

            // Debug.Log($"Projectile,{this.name} hit {other.name}", gameObject);
            bool succeed = DamageHelper.TryHit(other, m_Actor, m_WeaponDamageSetting, m_CurDmgInfo);
            if (succeed)
            {
                Hide();
            }
            else
            {
                if (other.gameObject.layer == (int)EStaticLayers.Default)
                {
                    Hide();
                }
            }
        }

        void Hide()
        {
            gameObject.SetActive(false);
            m_Actor = null;
            m_Flying = false;
        }

        public void Throw(SActor owner, SActor target)
        {
            NodeFollower nodeFollower = GetComponent<NodeFollower>();
            if (nodeFollower)
            {
                nodeFollower.enabled = false;
            }

            gameObject.SetActive(true);

            m_Actor = owner;
            m_Direction = target.transform.position + Vector3.up * target.CPhysic.CenterHeight - transform.position;
            m_Direction.Normalize();
            m_Flying = true;
            m_TimerDestroy = 10;
        }

        void Update()
        {
            if (m_Flying)
            {
                transform.position += m_Direction * m_Speed * Time.deltaTime;

                m_TimerDestroy -= Time.deltaTime;
                if (m_TimerDestroy < 0)
                {
                    Hide();
                }
            }
        }

        /// <summary>在完美闪避范围内</summary>
        public bool InPerfectDodgeRange(SActor target)
        {
            float radius = m_SphereCollider.radius;
            int layerMask = EStaticLayers.Actor.GetLayerMask();
            Vector3 point1 = transform.position + m_Direction * m_Speed * 0.2f;
            Collider[] colliders = Physics.OverlapCapsule(transform.position, point1, radius, layerMask,
                QueryTriggerInteraction.Ignore);
            
            //SDebug.DrawCapsule(transform.position, point1, Color.green, radius, 3);

            foreach (var col in colliders)
            {
                if (col.gameObject == target.gameObject)
                    return true;
            }

            return false;
        }

        public bool IsFlying(out SActor owner)
        {
            owner = m_Actor;
            return m_Flying;
        }
    }
}