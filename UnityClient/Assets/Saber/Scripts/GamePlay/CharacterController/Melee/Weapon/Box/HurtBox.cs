using System.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;


namespace Saber.CharacterController
{
    /// <summary>伤害承受框</summary>
    [RequireComponent(typeof(CapsuleCollider))]
    public class HurtBox : MonoBehaviour
    {
        private CapsuleCollider m_Collider;

        private DamageInfo m_DamageInfo;
        //private float m_ColliderRadius, m_ColliderHeight;

        public SActor Actor { get; private set; }

        public Vector3 CenterPos => transform.position + transform.rotation * m_Collider.center;
        public EPhysicMaterialType PhysicMaterialType => Actor.PhysicInfo.m_PhysicMaterialType;
        public float ColliderRadius => m_Collider.radius;


        void Awake()
        {
            m_Collider = gameObject.GetComponent<CapsuleCollider>();
            m_Collider.enabled = true;

            // m_ColliderRadius = m_Collider.radius;
            // m_ColliderHeight = m_Collider.height;
        }

        void Start()
        {
            gameObject.layer = (int)EStaticLayers.Collider;
            Actor = GetComponentInParent<SActor>();
            m_Collider.sharedMaterial = Actor.CPhysic.ColliderPhysicMaterial;
            m_Collider.isTrigger = true;
        }

        /*
        public void SmallColliderSize()
        {
            m_Collider.radius = m_ColliderRadius / 5f;
            m_Collider.height = m_Collider.radius * 2;
        }

        public void RevertColliderSize()
        {
            m_Collider.radius = m_ColliderRadius;
            m_Collider.height = m_ColliderHeight;
        }
        */

        public void OnHit(Vector3 force, Vector3 point)
        {
            Actor.CMelee.IKHitReaction.Hit(m_Collider, force, point);
        }
    }
}