using System;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public partial class CharacterPhysic
    {
        /// <summary>Add an External Force to the Actor</summary>
        public float ExternalForceValue { get; set; }

        public Vector3 ExternalForceDir { get; set; }
        private float m_ExternalForceAttenuation;

        /// <summary>Current External Force the animal current has</summary>
        public Vector3 CurrentExternalForce { get; set; }

        /// <summary>External Force Aceleration /summary>
        public float ExternalForceAcel { get; set; }

        /// <summary>External Force Air Control, Can it be controlled while on the air?? </summary>
        public bool ExternalForceAirControl { get; set; }

        public bool HasExternalForce => ExternalForceValue != 0;


        /// <summary> Adds a Custom Force to the Actor </summary>
        /// <param name="Direction">Direction of the Force Applied to the Actor</param>
        /// <param name="Force"> Amount of Force aplied to the Direction</param>
        /// <param name="Aceleration">Smoothens value to apply the force. Higher values faster the force is appled</param>
        /// <param name="ResetGravity">Every time a force is applied, the Gravity Aceleration will be reseted</param>
        /// <param name="ForceAirControl">The animal can move while is been pushed by the force if this parameter is true. </param>
        /// <param name="LimitForce">Limits the magnitude of the Force to a value </param>
        public virtual void Force_Add(Vector3 Direction, float Force, float Aceleration, bool ResetGravity,
            float attenuation = 20, bool ForceAirControl = true, float LimitForce = 0)
        {
            m_ExternalForceAttenuation = attenuation;
            var CurrentForce = CurrentExternalForce + GravityStoredVelocity; //Calculate the Starting force

            if (LimitForce > 0 && CurrentForce.magnitude > LimitForce)
                CurrentForce = CurrentForce.normalized * LimitForce; //Add the Bounce

            CurrentExternalForce = CurrentForce;
            ExternalForceValue = Force;
            ExternalForceDir = Direction.normalized;
            ExternalForceAcel = Aceleration;

            // if (ActiveState.ID == StateEnum.Fall) //If we enter to a zone from the Fall state.. Reset the Fall Current Distance
            // {
            //     var fall = ActiveState as Fall;
            //     fall.FallCurrentDistance = 0;
            // }

            if (ResetGravity)
                ResetGravityValues();

            ExternalForceAirControl = ForceAirControl;
        }

        /// <summary> Removes the current active force applied to the animal  </summary>
        /// <param name="Aceleration"> Current aceleration to remove the force. When set to Zero then the force will be removed instantly</param>
        public virtual void Force_Remove(float Aceleration = 0)
        {
            ExternalForceAcel = Aceleration;
            ExternalForceValue = 0;
            ExternalForceDir = Vector3.zero;
            m_ExternalForceAttenuation = 0;
        }

        /// <summary> Removes every force applied to the animal </summary>
        internal void Force_Reset()
        {
            CurrentExternalForce = Vector3.zero;
            ExternalForceValue = 0;
            ExternalForceDir = Vector3.zero;
            ExternalForceAcel = 0;
            m_ExternalForceAttenuation = 0;
        }

        /// <summary> This is used to add an External force to </summary>
        private void ApplyExternalForce()
        {
            if (ExternalForceValue <= 0 || ExternalForceDir == Vector3.zero)
                return;

            Vector3 externalForce = ExternalForceDir * ExternalForceValue;
            var Acel = ExternalForceAcel > 0 ? (DeltaTime * ExternalForceAcel) : 1; //Use Full for changing

            CurrentExternalForce = Vector3.Lerp(CurrentExternalForce, externalForce, Acel);

            if (CurrentExternalForce.sqrMagnitude <= 0.01f)
                CurrentExternalForce = Vector3.zero; //clean Tiny forces

            if (CurrentExternalForce != Vector3.zero)
                AdditivePosition += CurrentExternalForce * DeltaTime;

            // 力的衰减
            if (m_ExternalForceAttenuation > 0)
            {
                ExternalForceValue -= DeltaTime * m_ExternalForceAttenuation;
                if (ExternalForceValue < 0)
                    ExternalForceValue = 0;
            }
        }
    }
}