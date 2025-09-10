using System;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public partial class CharacterPhysic
    {
        /// <summary> Delta Actor Velocity  </summary>
        public Vector3 DeltaVelocity { get; internal set; }

        /// <summary>Smoothness Position value when Entering from Non Grounded States </summary>
        public float AlignPosLerpDelta { get; internal set; }

        /// <summary>Align the Actor to Terrain</summary>
        /// <param name="alignTerrain">True: Aling to Surface Normal, False: Align to Up Vector</param>
        public void AlignRotation(bool alignTerrain)
        {
            Vector3 alignNormal = alignTerrain ? SurfaceNormal : UpVector;
            AlignRotation(alignNormal, AlignRotLerp);
        }

        /// <summary>Smoothness value to Snap to ground </summary>
        public float AlignPosLerp = 15f;

        /// <summary>Smoothness Position value when Entering from Non Grounded States</summary>
        public float AlignPosDelta = 2.5f;

        /// <summary>Smoothness Rotation value when Entering from Non Grounded States</summary>
        public float AlignRotDelta = 2.5f;

        /// <summary>Smoothness Rotation value when Entering from non Grounded States </summary>
        public float AlignRotLerpDelta { get; internal set; }

        /// <summary>Smoothness value to Snap to ground  </summary>
        public float AlignRotLerp = 15f;


        /// <summary>Align the Actor to a Custom </summary>
        public virtual void AlignRotation(Vector3 alignNormal, float Smoothness)
        {
            AlignRotLerpDelta = Mathf.Lerp(AlignRotLerpDelta, Smoothness, DeltaTime * AlignRotDelta * 4);
            Quaternion AlignRot =
                Quaternion.FromToRotation(Actor.transform.up, alignNormal) *
                Actor.transform.rotation; //Calculate the orientation to Terrain 
            Quaternion Inverse_Rot = Quaternion.Inverse(Actor.transform.rotation);
            Quaternion Target = Inverse_Rot * AlignRot;
            Quaternion Delta =
                Quaternion.Lerp(Quaternion.identity, Target,
                    DeltaTime * AlignRotLerpDelta); //Calculate the Delta Align Rotation

            //Actor.transform.rotation *= Delta;
            AdditiveRotation *= Delta;
        }

        public virtual void AlignRotation(Vector3 from, Vector3 to, float time, float Smoothness)
        {
            AlignRotLerpDelta = Mathf.Lerp(AlignRotLerpDelta, Smoothness, time * AlignRotDelta * 4);

            Quaternion AlignRot =
                Quaternion.FromToRotation(from, to) * Actor.transform.rotation; //Calculate the orientation to Terrain 
            Quaternion Inverse_Rot = Quaternion.Inverse(Actor.transform.rotation);
            Quaternion Target = Inverse_Rot * AlignRot;
            Quaternion Delta =
                Quaternion.Lerp(Quaternion.identity, Target,
                    time * AlignRotLerpDelta); //Calculate the Delta Align Rotation

            //Actor.transform.rotation *= Delta;
            AdditiveRotation *= Delta;
        }

        /*
        /// <summary>Snap to Ground with Smoothing</summary>
        private void AlignPosition()
        {
            if (!Grounded)
                return; //DO NOT ALIGN  IMPORTANT This caused the animals jumping upwards when falling down

            if (!Mathf.Approximately(m_RaycastHitChest.distance, 0))
            {
                AlignPosLerpDelta = Mathf.Lerp(AlignPosLerpDelta, 30, DeltaTime * 2.5f);

                float difference = CenterHeight - m_RaycastHitChest.distance;
                float DeltaDiference = difference * DeltaTime * AlignPosLerpDelta;
                Vector3 align = Actor.transform.rotation * new Vector3(0, DeltaDiference, 0); //Rotates with the Transform to better alignment
                AdditivePosition += align; //WORKS WITH THIS!! 

                m_RaycastHitChest.distance += DeltaDiference; //REMOVE the difference (PERFORMANCE!!!!!)
            }
        }
        */

        /// <summary>Snap to Ground with Smoothing</summary>
        private void AlignPosition()
        {
            if (!Grounded)
                return; //DO NOT ALIGN  IMPORTANT This caused the animals jumping upwards when falling down

            if (!UseGravity && GroundDistance > 0)
                return;

            if (!Mathf.Approximately(GroundDistance, 0))
            {
                AlignPosLerpDelta = Mathf.Lerp(AlignPosLerpDelta, 10, DeltaTime * 2.5f);
                float DeltaDiference = Mathf.Lerp(0, GroundDistance, DeltaTime * AlignPosLerpDelta);
                Vector3 align = Actor.transform.rotation *
                                new Vector3(0, DeltaDiference, 0); //Rotates with the Transform to better alignment
                AdditivePosition -= align; //WORKS WITH THIS!! 
            }
        }

        /// <summary>对齐前方方向</summary>
        /// <param name="dir">方向</param>
        /// <param name="GetTurnSpeed">angle => turnSpeed</param>
        public float AlignForwardTo(Vector3 dir, Func<float, float> GetTurnSpeed)
        {
            Vector3 forward = Actor.transform.forward;
            dir.y = 0;
            forward.y = 0;

            float angle = Vector3.SignedAngle(forward, dir, Vector3.up);
            Quaternion desiredRot = Quaternion.Euler(0, angle, 0);
            float turnSpeed = GetTurnSpeed(angle) * Actor.TimeMultiplier;
            float t = Mathf.Clamp01(turnSpeed * DeltaTime / Mathf.Abs(angle));
            Quaternion smoothRot = Quaternion.Slerp(Quaternion.identity, desiredRot, t);
            m_AlignForwardAddRot = smoothRot; //在 update 中应用，避免被 ResetMotionValues() 方法覆盖
            //AdditiveRotation *= smoothRot;
            return angle;
        }

        public bool AlignForwardTo(Vector3 dir, float turnSpeed)
        {
            float angle = AlignForwardTo(dir, a => turnSpeed);
            return Mathf.Abs(angle) < 1f;
        }

        public void ControlInAir()
        {
            AlignForwardTo(Actor.DesiredLookDir, 120);
            AdditivePosition += Actor.DesiredMoveDir * DeltaTime * 1f;
        }
    }
}