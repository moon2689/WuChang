using System;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public partial class CharacterPhysic
    {
        public const float k_GroundMinDis = 0.2f;

        public event Action<bool> Event_OnGrounded;

        private Transform m_BoneChest;
        private Transform m_BoneHips;
        private RaycastHit m_RaycastHitChest, m_RaycastHitHip;

        private bool m_Grounded;

        /// <summary> Is the Actor on a surface, when True the Raycasting for the Ground is Applied</summary>
        public bool Grounded
        {
            get => m_Grounded;
            set
            {
                if (m_Grounded != value)
                {
                    m_Grounded = value;

                    if (value)
                    {
                        ResetGravityValues();
                        Force_Reset();

                        // UpDownAdditive = 0; //Reset UpDown Additive 
                        // UsingUpDownExternal = false; //Reset UpDown Additive 
                        // GravityMultiplier = 1;
                        // ExternalForceAirControl = true; //Reset the External Force Air Control
                        //UseGravity = false;

                        //Reset the PosLerp
                        AlignPosLerpDelta = 0;
                        AlignRotLerpDelta = 0;
                    }
                    else
                    {
                        SetPlatform(null); //If groundes is false remove the stored Platform 
                        SlopeNormal = UpVector; //Reset the Slope Normal when the animal is not grounded
                    }

                    // SetBoolParameter(hash_Grounded, grounded.Value);
                    //
                    Event_OnGrounded?.Invoke(value);

                    //Debug.Log($"Grounded = {value}, Ground Distance = {GroundDistance}");
                }
            }
        }

        public LayerMask GroundLayerMask
        {
            get
            {
                int layerMask = EStaticLayers.Default.GetLayerMask();
                return layerMask;
            }
        }

        /// <summary>Slope Normal from the ground</summary>
        public Vector3 SlopeNormal { get; internal set; }

        /// <summary>Main Pivot Slope Angle</summary>
        public float MainPivotSlope { get; private set; }

        /// <summary>Direction Vector of the Terrain</summary>
        public Vector3 SlopeDirection { get; private set; }

        /// <summary>Angle value from the Vector Up to the ground</summary>
        public float SlopeDirectionAngle { get; internal set; }

        /// <summary>Smooth Lerp Value of Direction Vector of the Terrain</summary>
        public Vector3 SlopeDirectionSmooth { get; set; }

        /// <summary>Calculation of the Average Surface Normal</summary>
        public Vector3 SurfaceNormal { get; internal set; }

        /// <summary> Up Vector is the Opposite direction of the Gravity dir</summary>
        public Vector3 UpVector => -m_gravityDir;

        private Vector3 m_gravityDir = Vector3.down;

        /// <summary>Slope Calculate from the Surface Normal. Positive = Higher Slope, Negative = Lower Slope </summary>
        public float TerrainSlope { get; private set; }

        /// <summary>Check if can Fall on slope while on the ground "Decline Slope"</summary>
        public bool DeepSlope => SlopeDirectionAngle > m_SlopeLimit;

        /// <summary> Direction of the Gravity </summary>
        public Vector3 Gravity
        {
            get => m_gravityDir;
            set => m_gravityDir = value;
        }

        /// <summary>Main pivot Point is the Pivot Chest Position, if not the Pivot Hip Position one</summary>
        Vector3 PivotChest => m_BoneChest.position + DeltaVelocity;
        Vector3 PivotHip => m_BoneHips.position + DeltaVelocity;

        private Vector3 PivotMain => PivotHip;


        private void AddForceToGround(Collider collider, Vector3 point)
        {
            collider.attachedRigidbody?.AddForceAtPosition(Gravity * (RB.mass / 2), point, ForceMode.Force);
        }

        /// <summary> If the Actor has touched the ground then Grounded will be set to true  </summary>
        void CheckIfGrounded()
        {
            if (m_PhysicInfo.m_IsWalkingUpright)
                CheckIfGround_WalkingUpright();
            else
                CheckIfGround_WalkingQuadrupedal();
        }

        /// <summary>直立行走生物地面检测</summary>
        void CheckIfGround_WalkingUpright()
        {
            m_RaycastHitChest = new RaycastHit()
            {
                normal = UpVector,
                distance = 10,
            };

            float dist = 10f;
            if (Physics.Raycast(PivotMain, Gravity, out var raycastHit, CenterHeight + dist, GroundLayerMask,
                    QueryTriggerInteraction.Ignore))
            {
                dist = Actor.transform.position.y - m_PhysicInfo.m_GroundOffset - raycastHit.point.y;
                m_RaycastHitChest = raycastHit;
            }

            // SDebug.DrawCircle(PivotMain, Quaternion.identity, 0.2f, Color.red);
            // SDebug.DrawArrow(PivotMain, Gravity.normalized * (CenterHeight + dist), Color.red, 0.2f);

            /*
            if (dist >= k_GroundMinDis)
            {
                Vector3 rayOrigin = Actor.transform.position + UpVector * Radius;
                if (Physics.SphereCast(rayOrigin, Radius * 0.9f, Gravity, out var raycastHit2, Radius + 0.5f, GroundLayerMask, QueryTriggerInteraction.Ignore))
                {
                    Physics.Linecast(raycastHit2.point + UpVector * 0.1f, raycastHit2.point + Gravity * 0.15f, out raycastHit2, GroundLayerMask);
                    float newDist = Actor.transform.position.y - raycastHit2.point.y;
                    if (newDist < dist)
                    {
                        dist = newDist;
                        m_RaycastHitChest = raycastHit2;
                    }
                }
            }
            */

            GroundDistance = dist;
            Grounded = GroundDistance < k_GroundMinDis;

            //Store the Downward Slope Direction
            SurfaceNormal = SlopeNormal = m_RaycastHitChest.normal;
            if (SlopeNormal != UpVector)
            {
                MainPivotSlope = Vector3.SignedAngle(SlopeNormal, UpVector, Actor.transform.right);
                SlopeDirection = Vector3.ProjectOnPlane(Gravity, SlopeNormal).normalized;

                SlopeDirectionAngle = 90 - Vector3.Angle(Gravity, SlopeDirection);
                if (Mathf.Approximately(SlopeDirectionAngle, 90))
                    SlopeDirectionAngle = 0;

                TerrainSlope = Vector3.SignedAngle(SurfaceNormal, UpVector, Actor.transform.right);

                //Debug.Log("SlopeDirectionAngle:"+SlopeDirectionAngle);
            }
            else
            {
                MainPivotSlope = 0;
                SlopeDirection = Vector3.zero;
                SlopeDirectionAngle = 0;
                TerrainSlope = 0;
            }

            if (Grounded)
            {
                //Physic Logic (Push RigidBodys Down with the Weight)
                AddForceToGround(m_RaycastHitChest.collider, m_RaycastHitChest.point);
                SetPlatform(m_RaycastHitChest.collider.transform);
            }
            else
            {
                AlignPosLerpDelta = 0;
            }

            //   Debug.Log($"hit_Hip {hit_Hip.distance}: hit_Chest {hit_Chest.distance}");
            // if (ground_Changes_Gravity)
            //     Gravity = -hit_Hip.normal;

            //Grounded = MainRay && !DeepSlope;
        }

        /// <summary>四足行走生物地面检测</summary>
        void CheckIfGround_WalkingQuadrupedal()
        {
            m_RaycastHitChest = new RaycastHit()
            {
                normal = UpVector,
                distance = 10,
            };

            m_RaycastHitHip = new RaycastHit()
            {
                normal = UpVector,
                distance = 10,
            };

            bool chestRay = false;
            float distChest = 10f;
            if (Physics.Raycast(PivotChest, -Actor.transform.up, out var raycastHitChest, CenterHeight + distChest,
                    GroundLayerMask, QueryTriggerInteraction.Ignore))
            {
                distChest = raycastHitChest.distance - m_PhysicInfo.m_ChestHeight;
                m_RaycastHitChest = raycastHitChest;
                chestRay = true;
            }

            bool hipRay = false;
            if (Physics.Raycast(PivotHip, -Actor.transform.up, out var raycastHitHip, CenterHeight + 10,
                    GroundLayerMask, QueryTriggerInteraction.Ignore))
            {
                m_RaycastHitHip = raycastHitHip;
                hipRay = true;
            }

            GroundDistance = distChest - m_PhysicInfo.m_GroundOffset;
            Grounded = GroundDistance < k_GroundMinDis;

            //Store the Downward Slope Direction
            if (chestRay && hipRay)
            {
                Vector3 direction = (m_RaycastHitChest.point - m_RaycastHitHip.point).normalized;
                Vector3 side = Vector3.Cross(UpVector, direction).normalized;
                SurfaceNormal = SlopeNormal = Vector3.Cross(direction, side).normalized;

                /*
                SDebug.DrawWireSphere(m_RaycastHitChest.point, Color.red, 0.3f);
                SDebug.DrawWireSphere(m_RaycastHitHip.point, Color.red, 0.3f);
                SDebug.DrawLine(m_RaycastHitChest.point, m_RaycastHitHip.point, Color.green);
                SDebug.Draw_Arrow(Actor.transform.position, side * 3, Color.green);
                SDebug.Draw_Arrow(Actor.transform.position, SurfaceNormal * 4, Color.green);
                */
            }
            else if (chestRay)
            {
                SurfaceNormal = SlopeNormal = m_RaycastHitChest.normal;
            }
            else if (hipRay)
            {
                SurfaceNormal = SlopeNormal = m_RaycastHitHip.normal;
            }
            else
            {
                SurfaceNormal = SlopeNormal = UpVector;
            }

            if (SlopeNormal != UpVector)
            {
                MainPivotSlope = Vector3.SignedAngle(SlopeNormal, UpVector, Actor.transform.right);
                SlopeDirection = Vector3.ProjectOnPlane(Gravity, SlopeNormal).normalized;

                SlopeDirectionAngle = 90 - Vector3.Angle(Gravity, SlopeDirection);
                if (Mathf.Approximately(SlopeDirectionAngle, 90))
                    SlopeDirectionAngle = 0;

                TerrainSlope = Vector3.SignedAngle(SurfaceNormal, UpVector, Actor.transform.right);
            }
            else
            {
                MainPivotSlope = 0;
                SlopeDirection = Vector3.zero;
                SlopeDirectionAngle = 0;
                TerrainSlope = 0;
            }

            if (chestRay)
            {
                SetPlatform(m_RaycastHitChest.transform);
            }
            else if (hipRay)
            {
                SetPlatform(m_RaycastHitHip.transform);
            }
            else
            {
                SetPlatform(null);
            }

            if (Grounded)
            {
                //Physic Logic (Push RigidBodys Down with the Weight)
                if (chestRay)
                {
                    AddForceToGround(m_RaycastHitChest.collider, m_RaycastHitChest.point);
                }

                if (hipRay)
                {
                    AddForceToGround(m_RaycastHitHip.collider, m_RaycastHitHip.point);
                }
            }
            else
            {
                AlignPosLerpDelta = 0;
            }

            //   Debug.Log($"hit_Hip {hit_Hip.distance}: hit_Chest {hit_Chest.distance}");
            // if (ground_Changes_Gravity)
            //     Gravity = -hit_Hip.normal;

            //Grounded = MainRay && !DeepSlope;
        }
    }
}