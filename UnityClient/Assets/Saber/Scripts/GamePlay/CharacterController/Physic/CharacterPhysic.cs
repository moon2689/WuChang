using System;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    /// <summary>处理角色物理相关事务</summary>
    public class CharacterPhysic
    {
        public const float k_GroundMinDis = 0.2f;

        public event Action<bool> Event_OnGrounded;

        /// <summary>Angle on the terrain the animal can walk. If the Terrain Angle is higher than the Max value: the animal will slideDown</summary>
        private float m_SlopeLimit;

        private PhysicInfo m_PhysicInfo;
        private CapsuleCollider m_CapsuleCollider;
        private bool m_UseGravity;
        private Vector3 m_VectorSmoothDamp = Vector3.zero;
        private Vector3 m_LastPosition; // World Position on the last Frame
        private bool m_Active;
        private Quaternion? m_AlignForwardAddRot;


        private SActor Actor { get; set; }
        public Rigidbody RB { get; private set; }

        public LayerMask GroundLayerMask
        {
            get
            {
                int layerMask = EStaticLayers.Default.GetLayerMask();
                return layerMask;
            }
        }

        public bool ApplyRootMotion { get; set; }
        public Quaternion AdditiveRotation { get; set; }
        public Vector3 AdditivePosition { get; set; }
        private float DeltaTime => Actor.DeltaTime;
        public float Radius => m_PhysicInfo.m_Radius;
        public float Height => m_PhysicInfo.m_Height;
        public float CenterHeight => Height / 2f;
        public float GroundDistance { get; private set; }
        public bool EnableSlopeMovement { get; set; } = true;
        public bool EnablePlatformMovement { get; set; } = true;
        public bool CanClimb { get; set; } = true;

        /// <summary>Difference from the Last Frame and the Current Frame</summary>
        public Vector3 DeltaPos { get; private set; }

        public float SlopeLimit => m_SlopeLimit;
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

            m_CapsuleCollider.sharedMaterial = GameApp.Entry.Asset.LoadPhysicMaterial(physicInfo.m_PhysicMaterialType);


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

        public void SetSlopeLimit(float slopeLimit)
        {
            m_SlopeLimit = slopeLimit;
        }

        public void DefaultSlopeLimit()
        {
            SetSlopeLimit(45);
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
            return angle == 0;
        }

        public void ControlInAir()
        {
            AlignForwardTo(Actor.DesiredLookDir, 120);
            AdditivePosition += Actor.DesiredMoveDir * DeltaTime * 1f;
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

        private RaycastHit m_RaycastHitChest, m_RaycastHitHip;

        /// <summary>Main pivot Point is the Pivot Chest Position, if not the Pivot Hip Position one</summary>
        Vector3 PivotChest
        {
            get
            {
                Vector3 pivotPoint;
                if (m_PhysicInfo.m_IsWalkingUpright)
                {
                    if (m_PhysicInfo.m_PivotChest)
                    {
                        pivotPoint = m_PhysicInfo.m_PivotChest.position;
                    }
                    else if (m_PhysicInfo.m_PivotHip)
                    {
                        pivotPoint = m_PhysicInfo.m_PivotHip.position;
                    }
                    else
                    {
                        pivotPoint = Actor.transform.TransformPoint(new Vector3(0, CenterHeight));
                    }
                }
                else
                {
                    pivotPoint = m_PhysicInfo.m_PivotChest.position;
                }

                return pivotPoint + DeltaVelocity;
            }
        }

        Vector3 PivotHip
        {
            get
            {
                Vector3 pivotPoint;
                if (m_PhysicInfo.m_IsWalkingUpright)
                {
                    if (m_PhysicInfo.m_PivotHip)
                    {
                        pivotPoint = m_PhysicInfo.m_PivotHip.position;
                    }
                    else if (m_PhysicInfo.m_PivotChest)
                    {
                        pivotPoint = m_PhysicInfo.m_PivotChest.position;
                    }
                    else
                    {
                        pivotPoint = Actor.transform.TransformPoint(new Vector3(0, CenterHeight));
                    }
                }
                else
                {
                    pivotPoint = m_PhysicInfo.m_PivotHip.position;
                }

                return pivotPoint + DeltaVelocity;
            }
        }

        private Vector3 PivotMain => PivotHip;

        /// <summary> Delta Actor Velocity  </summary>
        public Vector3 DeltaVelocity { get; internal set; }

        /// <summary>Smoothness Position value when Entering from Non Grounded States </summary>
        public float AlignPosLerpDelta { get; internal set; }

        private bool grounded;

        /// <summary> Is the Actor on a surface, when True the Raycasting for the Ground is Applied</summary>
        public bool Grounded
        {
            get => grounded;
            set
            {
                if (grounded != value)
                {
                    grounded = value;

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


        private Transform m_Platform;
        private Vector3 m_LastPlatformPos;
        private Quaternion m_LastPlatformRot;

        public void SetPlatform(Transform newPlatform)
        {
            if (m_Platform != newPlatform)
            {
                //Debug.Log($"SetPlatform: {newPlatform}", newPlatform);
                //GroundRootPosition = true;

                m_Platform = newPlatform;

                /*
                bool isTransparentFXLayer = m_Platform != null && ((1 << m_Platform.gameObject.layer) & EStaticLayers.BodyPart.GetLayerMask()) != 0;
                if (isTransparentFXLayer)
                {
                    m_CapsuleCollider.excludeLayers = 1 << EStaticLayers.Animal.GetLayer();
                }
                else
                {
                    m_CapsuleCollider.excludeLayers = 1 << EStaticLayers.Animal.GetLayer() | 1 << EStaticLayers.BodyPart.GetLayer();
                }
                */

                if (m_Platform != null)
                {
                    //Debug.Log($"#1 isTransparentFXLayer:{isTransparentFXLayer}, platform:{newPlatform.name}", newPlatform);
                    /*
                    var NewGroundChanger = newPlatform.GetComponent<GroundSpeedChanger>();

                    if (NewGroundChanger)
                    {
                        GroundRootPosition = false; //Important! Calculate RootMotion instead of adding it
                        GroundChanger?.OnExit?.React(this); //set to the ground changer that this has enter 
                        GroundChanger = NewGroundChanger;
                        GroundChanger.OnEnter?.React(this); //set to the ground changer that this has enter 
                    }
                    else
                    {
                        GroundChanger?.OnExit?.React(this); //set to the ground changer that this has enter 
                        GroundChanger = null;
                    }
                    */

                    m_LastPlatformPos = m_Platform.position;
                    m_LastPlatformRot = m_Platform.rotation;
                }
                else
                {
                    /*
                    GroundChanger?.OnExit?.React(this); //set to the ground changer that this has enter 
                    GroundChanger = null;
                    */

                    MainPivotSlope = 0;
                    ResetSlopeValues();
                }

                /*
                InGroundChanger = GroundChanger != null;
                foreach (var s in states)
                    s.OnPlataformChanged(platform);
                */
            }
        }

        void ResetSlopeValues()
        {
            SlopeDirection = Vector3.zero;
            SlopeDirectionSmooth = Vector3.ProjectOnPlane(SlopeDirectionSmooth, UpVector);
            SlopeDirectionAngle = 0;
        }

        void PlatformMovement()
        {
            if (m_Platform == null)
                return;
            if (m_Platform.gameObject.isStatic)
                return; //means it cannot move

            //Set it Directly to the Transform.. Additive Position can be reset any time.
            var deltaPlatformPos = m_Platform.position - m_LastPlatformPos;
            Actor.transform.position += deltaPlatformPos;

            Quaternion inverseRot = Quaternion.Inverse(m_LastPlatformRot);
            Quaternion deltaRot = inverseRot * m_Platform.rotation;

            if (deltaRot != Quaternion.identity) // no rotation founded.. Skip the code below
            {
                var pos = Actor.transform.DeltaPositionFromRotate(m_Platform.position, deltaRot);
                Actor.transform.position +=
                    pos; //Set it Directly to the Transform.. Additive Position can be reset any time..
            }

            //AdditiveRotation *= Delta;
            Actor.transform.rotation *=
                deltaRot; //Set it Directly to the Transform.. Additive Position can be reset any time..

            m_LastPlatformPos = m_Platform.position;
            m_LastPlatformRot = m_Platform.rotation;
        }
    }
}