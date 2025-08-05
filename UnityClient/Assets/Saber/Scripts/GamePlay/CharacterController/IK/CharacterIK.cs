using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Saber.CharacterController
{
    public class CharacterIK
    {
        // Specifies the limb affected by IK.
        public enum IKGoal
        {
            LeftHand, // The character's left hand.
            LeftElbow, // The character's left elbow.
            RightHand, // The character's right hand.
            RightElbow, // The character's right elbow.
            LeftFoot, // The character's left foot.
            LeftKnee, // The character's left knee.
            RightFoot, // The character's right foot.
            RightKnee, // The character's right knee.
            Last // The last entry in the enum - used to detect the number of values.
        }

        [Serializable]
        public class IKInfo
        {
            public bool m_Enable;

            [Tooltip("The speed at which the hips position should adjust to using IK and not using IK.")]
            public float m_HipsPositionAdjustmentSpeed = 4;

            [Tooltip("An extra offset applied to the hips position.")]
            public float m_HipsPositionOffset;

            [Tooltip("The speed at which the foot weight should adjust to when foot IK is inactive.")]
            public float m_FootWeightInactiveAdjustmentSpeed = 2;

            [Tooltip("The speed at which the foot weight should adjust to when foot IK is active.")]
            public float m_FootWeightActiveAdjustmentSpeed = 10;

            [Tooltip("The offset of the foot between the foot bone and the base of the foot.")]
            public float m_FootOffsetAdjustment = 0.005f;

            [Tooltip("高跟鞋高度")] public float m_HighHeelsHeight;
        }

        public event Func<IKGoal, Vector3, Quaternion, Vector3> Event_OnUpdateIKPosition;
        public event Func<IKGoal, Quaternion, Vector3, Quaternion> Event_OnUpdateIKRotation;

        protected SCharacter m_Character;
        protected Animator m_Animator;

        private IKInfo m_IKInfo;
        private LayerMask m_LayerMask = EStaticLayers.Default.GetLayerMask();
        private RaycastHit m_RaycastHit;
        private Vector3 m_HipsPosition;
        private float m_HipsOffset;
        private float[] m_FootOffset = new float[2];
        private float[] m_FootIKWeight = new float[2];
        private float[] m_MaxLegLength = new float[2];
        private float[] m_RaycastDistance = new float[2];
        private float[] m_GroundDistance = new float[2];
        private Vector3[] m_GroundPoint = new Vector3[2];
        private Vector3[] m_GroundNormal = new Vector3[2];

        private Transform[] m_IKTarget;

        private Transform m_Transform => m_Character.transform;
        private Transform m_LeftFoot => m_Character.GetNodeTransform(ENodeType.LeftFoot);
        private Transform m_RightFoot => m_Character.GetNodeTransform(ENodeType.RightFoot);
        private Transform m_LeftToes => m_Character.GetNodeTransform(ENodeType.LeftToes);
        private Transform m_RightToes => m_Character.GetNodeTransform(ENodeType.RightToes);
        private Transform m_LeftLowerLeg => m_Character.GetNodeTransform(ENodeType.LeftLowerLeg);
        private Transform m_RightLowerLeg => m_Character.GetNodeTransform(ENodeType.RightLowerLeg);
        private Transform m_Hips => m_Character.GetNodeTransform(ENodeType.Hips);

        public bool Enable
        {
            get => m_IKInfo.m_Enable;
        }


        public CharacterIK(SCharacter character, Animator animator, IKInfo ikInfo)
        {
            m_Character = character;
            m_Animator = animator;
            m_IKInfo = ikInfo;

            InitializeBones();
        }

        private void InitializeBones()
        {
            // Perform measurements during initialization while in a T-Pose so they can be compared against during the IK pass.
            for (int i = 0; i < 2; ++i)
            {
                var foot = i == 0 ? m_LeftFoot : m_RightFoot;
                m_FootOffset[i] = m_Transform.InverseTransformPoint(foot.position).y - m_IKInfo.m_FootOffsetAdjustment;
                m_MaxLegLength[i] = m_Transform.InverseTransformPoint(i == 0 ? m_LeftLowerLeg.position : m_RightLowerLeg.position).y - m_IKInfo.m_FootOffsetAdjustment;
            }

            m_HipsPosition = m_Transform.InverseTransformPoint(m_Hips.position);
        }


        public void OnAnimatorIK(int layerIndex)
        {
            if (!Enable)
                return;

            PositionLowerBody();
        }

        /// <summary>
        /// Returns the position that the raycast should start at when determining if the foot is near the ground.
        /// </summary>
        /// <param name="targetTransform">The Transform of the foot or toe.</param>
        /// <param name="lowerLeg">The Transform of the lower leg.</param>
        /// <param name="distance">The vertical distance between the hip and target Transform.</param>
        /// <returns>The position that the raycast should start at when determining if the foot is near the ground.</returns>
        private Vector3 GetFootRaycastPosition(Transform targetTransform, Transform lowerLeg, out float distance)
        {
            // The relative y position should be the same as the lower leg so the raycast can detect any objects between the lower leg position and current foot position.
            var raycastPosition = m_Transform.InverseTransformPoint(targetTransform.position);
            var localLowerLegPosition = m_Transform.InverseTransformPoint(lowerLeg.position);
            distance = (localLowerLegPosition.y - raycastPosition.y);
            //raycastPosition.y = localLowerLegPosition.y;
            // 解决陡坡可能穿模问题
            distance *= 2f;
            raycastPosition.y += distance;

            return m_Transform.TransformPoint(raycastPosition);
        }

        // Smoothly position the hips.
        private void CalcSmoothHipsPosition(float hipsOffset)
        {
            m_HipsOffset = Mathf.Lerp(m_HipsOffset, hipsOffset, m_IKInfo.m_HipsPositionAdjustmentSpeed * Time.deltaTime);
            m_HipsPosition = m_Transform.InverseTransformPoint(m_Hips.position);
            m_HipsPosition.y -= m_HipsOffset + m_IKInfo.m_HipsPositionOffset;
            m_HipsPosition.y += m_IKInfo.m_HighHeelsHeight;
        }

        /// <summary>Positions the lower body so the legs are always on the ground.</summary>
        private void PositionLowerBody()
        {
            float hipsOffset = 0;
            if (m_Character.CPhysic.Grounded)
            {
                // There are two passes for positioning the feet. The hips need to be positioned first and then the feet can be positioned.
                for (int i = 0; i < 2; ++i)
                {
                    // Worship the first raycast from the foot.
                    var target = (i == 0 ? m_LeftFoot : m_RightFoot);
                    var lowerLeg = (i == 0 ? m_LeftLowerLeg : m_RightLowerLeg);
                    if (Physics.Raycast(GetFootRaycastPosition(target, lowerLeg, out var distance), Vector3.down, out m_RaycastHit,
                            distance + m_FootOffset[i] + m_MaxLegLength[i], m_LayerMask, QueryTriggerInteraction.Ignore) &&
                        m_Transform.InverseTransformPoint(m_RaycastHit.point).y < m_Character.CPhysic.Radius)
                    {
                        m_RaycastDistance[i] = distance * m_Transform.lossyScale.y;
                        m_GroundDistance[i] = m_RaycastHit.distance;
                        m_GroundPoint[i] = m_RaycastHit.point;
                        m_GroundNormal[i] = m_RaycastHit.normal;
                    }
                    else
                    {
                        m_GroundDistance[i] = float.MaxValue;
                    }

                    if (m_GroundDistance[i] != float.MaxValue)
                    {
                        // If the foot is at the same relative height then the hip offset should be set. This is most useful when the character is standing on uneven ground.
                        // As an example, imagine that the character is standing on a set of stairs. The stairs have two sets of colliders: one collider which covers each step 
                        // and another plane collider at the same slope as the stairs. The character’s collider is going to be resting on the plane collider while standing on the 
                        // stairs and the IK system will be trying to ensure the feet are resting on the stairs collider. In some cases the plane collider may be relatively far 
                        // above the stair collider so the hip needs to be moved down to allow the character’s foot to hit the stair collider.
                        float offset;
                        var foot = (i == 0 ? m_LeftFoot : m_RightFoot);
                        if ((offset = m_GroundDistance[i] - m_RaycastDistance[i] - m_Transform.InverseTransformPoint(foot.position).y) > hipsOffset)
                        {
                            hipsOffset = offset;
                        }
                    }
                }
            }

            // Smoothly position the hips.
            CalcSmoothHipsPosition(hipsOffset);

            /*
            Vector3 bodyPos = m_Animator.bodyPosition;
            bodyPos.y -= m_HipsOffset + m_HipsPositionOffset;
            m_Animator.bodyPosition = bodyPos;
            */

            // Move the feet into the correct position/rotation.
            for (int i = 0; i < 2; ++i)
            {
                var ikGoal = (i == 0 ? AvatarIKGoal.LeftFoot : AvatarIKGoal.RightFoot);
                var position = m_Animator.GetIKPosition(ikGoal);
                var rotation = m_Animator.GetIKRotation(ikGoal);
                var targetWeight = 0f;

                float adjustmentSpeed = m_IKInfo.m_FootWeightInactiveAdjustmentSpeed;
                // Determine the position and rotation of the foot if on the ground.
                if (m_Character.CPhysic.Grounded)
                {
                    // IK should only be used if the foot position would be underneath the ground position.
                    if (m_GroundDistance[i] != float.MaxValue && m_GroundDistance[i] > 0 &&
                        m_Transform.InverseTransformDirection(position - m_GroundPoint[i]).y - m_IKInfo.m_HighHeelsHeight - m_FootOffset[i] - m_HipsOffset < 0.01f)
                    {
                        var localFootPosition = m_Transform.InverseTransformPoint(position);
                        localFootPosition.y = m_Transform.InverseTransformPoint(m_GroundPoint[i]).y;
                        position = m_Transform.TransformPoint(localFootPosition) + Vector3.up * (m_FootOffset[i] + m_HipsOffset);
                        rotation = Quaternion.LookRotation(Vector3.Cross(m_GroundNormal[i], rotation * -Vector3.right), Vector3.up);
                        targetWeight = 1f;
                        adjustmentSpeed = m_IKInfo.m_FootWeightActiveAdjustmentSpeed;
                    }
                    /*
                    else
                    {
                        float offset = m_Transform.InverseTransformDirection(position - m_GroundPoint[i]).y - m_FootOffset[i] - m_HipsOffset;
                        Debug.Log($"{i} offset:{offset}");
                    }
                    */
                }


                if (adjustmentSpeed == 0)
                {
                    m_FootIKWeight[i] = Mathf.MoveTowards(m_FootIKWeight[i], 0, adjustmentSpeed * Time.fixedTime);
                    //m_FootIKWeight[i] = 0;
                }
                else
                {
                    m_FootIKWeight[i] = Mathf.MoveTowards(m_FootIKWeight[i], targetWeight, adjustmentSpeed * Time.fixedTime);
                    //m_FootIKWeight[i] = Mathf.Clamp01(m_FootIKWeight[i]);
                }

                // Other objects have the chance of modifying the final position and rotation value.
                if (Event_OnUpdateIKPosition != null)
                {
                    position = Event_OnUpdateIKPosition(i == 0 ? IKGoal.LeftFoot : IKGoal.RightFoot, position, rotation);
                }

                if (Event_OnUpdateIKRotation != null)
                {
                    rotation = Event_OnUpdateIKRotation(i == 0 ? IKGoal.LeftFoot : IKGoal.RightFoot, rotation, position);
                }

                // Apply the IK position and rotation.
                m_Animator.SetIKPosition(ikGoal, position);
                m_Animator.SetIKRotation(ikGoal, rotation);
                m_Animator.SetIKPositionWeight(ikGoal, m_FootIKWeight[i]);
                m_Animator.SetIKRotationWeight(ikGoal, m_FootIKWeight[i]);
            }

            /*
            // The knees can be positioned manually.
            if (m_IKTarget[(int)IKGoal.LeftKnee] != null)
            {
                m_Animator.SetIKHintPosition(AvatarIKHint.LeftKnee, m_IKTarget[(int)IKGoal.LeftKnee].position);
                m_Animator.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, m_FootIKWeight[0]);
            }

            if (m_IKTarget[(int)IKGoal.RightKnee] != null)
            {
                m_Animator.SetIKHintPosition(AvatarIKHint.RightKnee, m_IKTarget[(int)IKGoal.RightKnee].position);
                m_Animator.SetIKHintPositionWeight(AvatarIKHint.RightKnee, m_FootIKWeight[1]);
            }
            */
        }

        /// <summary>
        /// Updates the IK component after the animator has updated.
        /// </summary>
        public void AfterFixedUpdate()
        {
            if (!Enable)
                return;
            m_Hips.position = m_Transform.TransformPoint(m_HipsPosition);
        }
    }
}