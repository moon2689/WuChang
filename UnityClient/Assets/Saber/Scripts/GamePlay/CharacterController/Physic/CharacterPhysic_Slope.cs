using System;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public partial class CharacterPhysic
    {
        /// <summary>Angle on the terrain the animal can walk. If the Terrain Angle is higher than the Max value: the animal will slideDown</summary>
        private float m_SlopeLimit;


        public bool EnableSlopeMovement { get; set; } = true;

        public float SlopeLimit => m_SlopeLimit;


        public void SetSlopeLimit(float slopeLimit)
        {
            m_SlopeLimit = slopeLimit;
        }

        public void DefaultSlopeLimit()
        {
            SetSlopeLimit(45);
        }

        /// <summary> Slope momevent when the slope is big or small and where there's a ground changer component  </summary>
        private void SlopeMovement()
        {
            float slopeLimit = m_SlopeLimit;
            /*
            if (m_RaycastHitChest.collider && m_RaycastHitChest.collider.gameObject)
            {
                bool isTransparentFXLayer = ((1 << m_RaycastHitChest.collider.gameObject.layer) & EStaticLayers.BodyPart.GetLayerMask()) != 0;
                if (isTransparentFXLayer)
                {
                    slopeLimit = 70f;
                    Debug.Log($"isTransparentFXLayer,gameO:{m_RaycastHitChest.collider.name}", m_RaycastHitChest.collider);
                    return;
                }
            }
            */

            Vector3 slopDir = Vector3.zero;

            if (Grounded)
            {
                if (SlopeDirectionAngle > slopeLimit)
                {
                    float slopeAngleDifference = (SlopeDirectionAngle - slopeLimit) / 20;
                    //Clamp the Slope Movement so in higher angles does not get push that much
                    slopeAngleDifference = Mathf.Clamp01(slopeAngleDifference);
                    slopDir = slopeAngleDifference * SlopeDirection * 0.5f;
                }

                //Move in the direction of the Ground Normal,
                SlopeDirectionSmooth = Vector3.ProjectOnPlane(SlopeDirectionSmooth, SlopeNormal);
            }

            SlopeDirectionSmooth =
                Vector3.SmoothDamp(SlopeDirectionSmooth, slopDir, ref m_VectorSmoothDamp, DeltaTime * 15);

            if (SlopeDirectionSmooth != Vector3.zero)
            {
                AdditivePosition += SlopeDirectionSmooth;
                SDebug.DrawArrow(Actor.transform.position, SlopeDirectionSmooth * 10f, Color.yellow);
            }
        }

        void ResetSlopeValues()
        {
            SlopeDirection = Vector3.zero;
            SlopeDirectionSmooth = Vector3.ProjectOnPlane(SlopeDirectionSmooth, UpVector);
            SlopeDirectionAngle = 0;
        }
    }
}