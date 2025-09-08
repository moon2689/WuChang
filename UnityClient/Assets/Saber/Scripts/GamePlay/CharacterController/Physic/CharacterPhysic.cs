using System;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    /// <summary>处理角色物理相关事务</summary>
    public partial class CharacterPhysic
    {
        private PhysicInfo m_PhysicInfo;
        private CapsuleCollider m_CapsuleCollider;
        private bool m_UseGravity;
        private Vector3 m_VectorSmoothDamp = Vector3.zero;
        private Vector3 m_LastPosition; // World Position on the last Frame
        private bool m_Active;
        private Quaternion? m_AlignForwardAddRot;


        private SActor Actor { get; set; }
        public Rigidbody RB { get; private set; }
        public bool ApplyRootMotion { get; set; }
        public Quaternion AdditiveRotation { get; set; }
        public Vector3 AdditivePosition { get; set; }
        private float DeltaTime => Actor.DeltaTime;
        public float Radius => m_PhysicInfo.m_Radius;
        public float Height => m_PhysicInfo.m_Height;
        public float CenterHeight => Height / 2f;
        public float GroundDistance { get; private set; }

        /// <summary>Difference from the Last Frame and the Current Frame</summary>
        public Vector3 DeltaPos { get; private set; }

        public PhysicMaterial ColliderPhysicMaterial => m_CapsuleCollider.sharedMaterial;


        public CharacterPhysic(SActor actor, PhysicInfo physicInfo)
        {
            Actor = actor;
            m_PhysicInfo = physicInfo;

            RB = actor.gameObject.GetComponent<Rigidbody>();
            if (!RB)
                RB = actor.gameObject.AddComponent<Rigidbody>();
            RB.constraints = RigidbodyConstraints.FreezeRotation;
            RB.useGravity = false;
            RB.isKinematic = false;
            RB.mass = physicInfo.m_Mass;
            RB.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            m_CapsuleCollider = actor.gameObject.GetComponent<CapsuleCollider>();
            if (!m_CapsuleCollider)
                m_CapsuleCollider = actor.gameObject.AddComponent<CapsuleCollider>();
            m_CapsuleCollider.radius = physicInfo.m_Radius;
            float hipHeight = Mathf.Clamp(physicInfo.m_HipHeight, 0.1f, physicInfo.m_Height - physicInfo.m_Radius);
            m_CapsuleCollider.height = physicInfo.m_Height - hipHeight;
            m_CapsuleCollider.center =
                new Vector3(0, hipHeight + physicInfo.m_Height * 0.5f, physicInfo.m_CapsuleOffsetZ);

            GameApp.Entry.Asset.LoadPhysicMaterial(physicInfo.m_PhysicMaterialType, pm => m_CapsuleCollider.sharedMaterial = pm);
            
            if (physicInfo.m_IsBodyHuge)
            {
                m_CapsuleCollider.excludeLayers = 1 << EStaticLayers.Actor.GetLayer();
            }

            UseGravity = true;
            ResetGravityValues();
            DefaultSlopeLimit();

            m_LastPosition = actor.transform.position;

            m_Active = true;
        }

        public void Active(bool active)
        {
            m_Active = active;
            UseGravity = active;
            m_CapsuleCollider.enabled = active;
        }

        public void ResetMotionValues()
        {
            AdditivePosition = ApplyRootMotion ? Actor.CAnim.AnimatorObj.deltaPosition : Vector3.zero;
            //Vector3.Lerp(AdditivePosition, Vector3.zero, 4 * DeltaTime);
            AdditivePosition = Quaternion.FromToRotation(Actor.transform.up, SlopeNormal) * AdditivePosition;
            if (Actor.TimeMultiplier > 0)
                AdditivePosition /= Actor.TimeMultiplier;

            AdditiveRotation = ApplyRootMotion ? Actor.CAnim.AnimatorObj.deltaRotation : Quaternion.identity;
            DeltaVelocity = RB.velocity * DeltaTime; //When is not grounded take the Up Vector this is the one!!!
        }

        public void Update()
        {
            if (!m_Active)
            {
                return;
            }

            DeltaPos = Actor.transform.position - m_LastPosition; //DeltaPosition from the last frame
            m_LastPosition = Actor.transform.position;

            CheckIfGrounded();

            ApplyExternalForce();

            if (m_PhysicInfo.m_OpenPlatformMovement && EnablePlatformMovement)
                PlatformMovement();

            if (m_PhysicInfo.m_OpenSlopeMovement && EnableSlopeMovement)
                SlopeMovement();

            //Debug.Log($"Grounded:{Grounded}, {GroundDistance}");

            if (Grounded)
            {
                // 紧贴地面
                AlignPosition();

                // 四足动物紧贴地面法线
                AlignRotation(!m_PhysicInfo.m_IsWalkingUpright);
            }
            else
            {
                //Reset the PosLerp
                if (UseGravity)
                    GravityLogic();

                AlignRotation(false); //Align to the Gravity Normal
            }

            // apply pos
            if (RB.isKinematic)
                Actor.transform.position += AdditivePosition * Actor.TimeMultiplier;
            else
                RB.velocity = (AdditivePosition / DeltaTime) * Actor.TimeMultiplier;

            if (m_AlignForwardAddRot != null)
            {
                AdditiveRotation *= m_AlignForwardAddRot.Value;
                m_AlignForwardAddRot = null;
            }

            // apply rot
            Actor.transform.rotation *= AdditiveRotation;
        }
    }
}