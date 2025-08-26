using System;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public partial class CharacterPhysic
    {
         /// <summary>Does it use Gravity or not? </summary>
        public bool UseGravity
        {
            get => m_UseGravity;
            set
            {
                m_UseGravity = value;

                if (!m_UseGravity)
                    ResetGravityValues(); //Reset Gravity Logic when Use gravity is false
                //Debug.Log($"UseGravity = {value}");
            }
        }

        public int GravityTime { get; internal set; }


        private int m_gravityTime = 10;

        private float m_gravityPower = 9.8f;

        public float GravityPower
        {
            get => m_gravityPower;
            set => m_gravityPower = value;
        }


        /// <summary>Stored Gravity Velocity when the animal is using Gravity</summary>
        public Vector3 GravityStoredVelocity { get; internal set; }

        /// <summary>Value of Gravity Offset acumulation. (From Fake Gravity stuff) E.g. Jump</summary>
        public Vector3 GravityOffset { get; internal set; }

        /// <summary>Gravity ExtraPower (From Fake Gravity stuff) E.g. Jump</summary>
        public float GravityExtraPower { get; internal set; }

        // Clamp Gravity Speed. Zero will ignore this
        private float m_clampGravitySpeed = 0; //20f;


        public float ClampGravitySpeed
        {
            get => m_clampGravitySpeed;
            internal set => m_clampGravitySpeed = value;
        }


        /// <summary>Clears the Gravity Logic</summary>
        internal void ResetGravityValues()
        {
            GravityTime = m_gravityTime;
            GravityStoredVelocity = Vector3.zero;
            GravityOffset = Vector3.zero;
            GravityExtraPower = 1;
        }

        /// <summary> Do the Gravity Logic </summary>
        void GravityLogic()
        {
            if (!UseGravity || Grounded)
                return;

            GravityStoredVelocity = StoredGravityVelocity();

            if (ClampGravitySpeed > 0 && (ClampGravitySpeed * ClampGravitySpeed) < GravityStoredVelocity.sqrMagnitude)
            {
                GravityTime--; //Clamp the Gravity Speed
            }

            AdditivePosition += (DeltaTime * GravityExtraPower * GravityStoredVelocity) //Add Gravity if is in use
                                + GravityOffset * DeltaTime; //Add Gravity Offset JUMP if is in use

            GravityTime++;
        }

        internal Vector3 StoredGravityVelocity()
        {
            float GTime = DeltaTime * GravityTime;
            return (GTime * GTime / 2) * GravityPower * Actor.TimeMultiplier * Gravity;
        }
    }
}