using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Saber
{
    [ExecuteInEditMode]
    public class PlayerCameraShake : MonoBehaviour
    {
        // ********************* Settings ********************

        //Total time for shaking in seconds
        /// <summary>
        /// The duration which the camera will shake for
        /// </summary>
        public float m_ShakeDuration = 2.0f;

        /// <summary>
        /// The amount to shake by
        /// </summary>
        public Vector3 m_ShakeAmount = new Vector3(1f, 1f, 0);

        /// <summary>
        /// The speed of the camera shake
        /// </summary>
        public float m_ShakeSpeed = 2.0f;

        /// <summary>
        /// If enabled then the camera will start shaking
        /// </summary>
        public bool m_Shake;


        // ********************* Variables ********************


        /// <summary>
        /// Camera component on object
        /// </summary>
        private Camera m_Camera;


        /// <summary>
        /// Determines if the camera is currently shaking
        /// </summary>
        private bool m_IsShaking;

        /// <summary>
        /// Amount over Lifetime [0,1]
        /// </summary>
        private AnimationCurve m_Curve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        /// <summary>
        /// Set it to true: The camera position is set in reference to the old position of the camera
        /// Set it to false: The camera position is set in absolute values or is fixed to an object
        /// </summary>
        private bool m_DeltaMovement = true;


        /// <summary>
        /// Last position of shake
        /// </summary>
        private Vector3 m_LastPos;

        /// <summary>
        /// Next position of shake
        /// </summary>
        private Vector3 m_NextPos;

        /// <summary>
        /// Last field of view
        /// </summary>
        private float m_LastFoV;

        /// <summary>
        /// Next field of view
        /// </summary>
        private float m_NextFoV;


        // ********************* Private Methods ********************


        /// <summary>
        /// Will shake the object this component is attached too
        /// </summary>
        private void Shake(float Duration, float Amount, float Speed)
        {
            //If already shaking then end here
            if (this.m_IsShaking)
                return;

            //Reset cam ready
            ResetCam();

            //Update global values ready to use when camera is shaken 
            this.m_ShakeAmount = new Vector3(Amount, Amount, 0);
            this.m_ShakeSpeed = Speed;

            //Start the shake 
            this.m_ShakeDuration = Duration;


            // we are shaking now
            this.m_IsShaking = true;
        }

        /// <summary>
        /// Resets the camera
        /// </summary>
        private void ResetCam()
        {
            //reset the last delta
            transform.Translate(m_DeltaMovement ? -m_LastPos : Vector3.zero);
            m_Camera.fieldOfView -= m_LastFoV;

            //clear values
            m_LastPos = m_NextPos = Vector3.zero;
            m_LastFoV = m_NextFoV = 0f;
        }


        // ********************* Public Methods ********************

        /// <summary>
        /// Will initiate the camera shake
        /// </summary>
        public void ActivateCameraShake()
        {
            if (this.isActiveAndEnabled == false)
                return;


            //If already shaking then end here
            if (this.m_IsShaking)
                return;


            this.Shake(this.m_ShakeDuration, this.m_ShakeAmount.x, this.m_ShakeSpeed);
        }


        /// <summary>
        /// Will initiate the camera shake with the parameters provided
        /// </summary>
        /// <param name="Duration">Duration to shake camera for</param>
        /// <param name="Amount">Amount to shake camera by</param>
        /// <param name="Speed">The speed of the camera shake</param>
        public void ActivateCameraShake(float Duration, float Amount, float Speed)
        {
            if (this.isActiveAndEnabled == false)
                return;

            //If already shaking then end here
            if (this.m_IsShaking)
                return;


            Shake(Duration, Amount, Speed);
        }


        // ********************* Game ********************

        void Awake()
        {
            m_Camera = this.GetComponent<Camera>();
        }


        void Update()
        {
            //For Component Editor testing 
            if (this.m_Shake == true)
            {
                this.ActivateCameraShake();
                this.m_Shake = false;
            }
        }


        private void LateUpdate()
        {
            //If duration higher then 0 then shake has started
            if (m_IsShaking)
            {
                this.m_ShakeDuration -= Time.deltaTime;

                if (this.m_ShakeDuration > 0)
                {
                    //next position based on perlin noise
                    m_NextPos =
                        (Mathf.PerlinNoise(this.m_ShakeDuration * this.m_ShakeSpeed,
                            m_ShakeDuration * this.m_ShakeSpeed * 2) - 0.5f) * this.m_ShakeAmount.x * transform.right *
                        m_Curve.Evaluate(1f - m_ShakeDuration / this.m_ShakeDuration) +
                        (Mathf.PerlinNoise(this.m_ShakeDuration * this.m_ShakeSpeed * 2,
                            m_ShakeDuration * this.m_ShakeSpeed) - 0.5f) * this.m_ShakeAmount.y * transform.up *
                        m_Curve.Evaluate(1f - m_ShakeDuration / this.m_ShakeDuration);

                    m_NextFoV =
                        (Mathf.PerlinNoise(this.m_ShakeDuration * this.m_ShakeSpeed * 2,
                            m_ShakeDuration * this.m_ShakeSpeed * 2) - 0.5f) * this.m_ShakeAmount.z *
                        m_Curve.Evaluate(1f - m_ShakeDuration / this.m_ShakeDuration);

                    m_Camera.fieldOfView += (m_NextFoV - m_LastFoV);
                    m_Camera.transform.Translate(m_DeltaMovement ? (m_NextPos - m_LastPos) : m_NextPos);

                    m_LastPos = m_NextPos;
                    m_LastFoV = m_NextFoV;
                }
                else
                {
                    //shake ending
                    ResetCam();
                    this.m_IsShaking = false;
                }
            }
        }
    }
}