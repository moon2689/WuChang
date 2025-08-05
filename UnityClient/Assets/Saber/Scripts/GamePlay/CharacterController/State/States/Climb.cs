


using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Saber.CharacterController
{
    public class Climb : ActorStateBase
    {
        private float m_WallDistance = 0.3f;
        float m_RayRadius = 0.1f;
        private RaycastHit m_HitChest;
        private Vector3 m_AverageNormal;
        private Transform m_ValidWall;
        private string m_EndAnim;
        private readonly RaycastHit[] m_EdgeHit = new RaycastHit[1];


        public override bool ApplyRootMotionSetWhenEnter => true;


        public override bool CanEnter
        {
            get
            {
                Transform validWall = CheckClimbRay();
                return validWall != null;
            }
        }

        private Vector3 Point_Chest => Actor.transform.position + Vector3.up * Actor.CPhysic.Height * 0.9f;
        int ClimbLayer => Actor.CPhysic.GroundLayerMask;


        public Climb() : base(EStateType.Climb)
        {
        }

        public override void Enter()
        {
            base.Enter();

            m_EndAnim = null;

            Actor.CAnim.Play("ClimbEnterGround");

            Actor.CPhysic.UseGravity = false;
            Actor.CPhysic.EnableSlopeMovement = false;
            Actor.UpdateMovementAxisAnimatorParams = false;

            // foreach (HurtBox hurtBox in Actor.HurtBoxes)
            //     hurtBox.SmallColliderSize();
        }

        protected override void OnExit()
        {
            base.OnExit();

            m_EndAnim = null;

            Actor.CPhysic.UseGravity = true;
            Actor.CPhysic.EnableSlopeMovement = true;
            Actor.UpdateMovementAxisAnimatorParams = true;
            Actor.CPhysic.RB.isKinematic = false;
            Actor.CPhysic.SetPlatform(null);

            // foreach (HurtBox hurtBox in Actor.HurtBoxes)
            //     hurtBox.RevertColliderSize();
        }

        void PlayEndAnim(string endAnim)
        {
            m_EndAnim = endAnim;
            Actor.CAnim.Play(endAnim, onFinished: Exit);
        }

        public void EndClimb()
        {
            //Debug.Log("end climb");
            if (base.StateMachine.Fall(false))
            {
                Actor.CAnim.Play("ClimbDrop", onFinished: Exit);
            }
            else
            {
                PlayEndAnim("ClimbDown");
            }

            Actor.CPhysic.UseGravity = true;
        }

        public override void OnStay()
        {
            base.OnStay();

            if (!string.IsNullOrEmpty(m_EndAnim))
                return;

            m_ValidWall = CheckClimbRay();

            if (m_ValidWall)
            {
                Actor.CAnim.SetSmoothFloat(EAnimatorParams.Horizontal, Actor.MovementAxis.x);
                Actor.CAnim.SetSmoothFloat(EAnimatorParams.Vertical, Actor.MovementAxis.z);

                Actor.CPhysic.SetPlatform(m_ValidWall);
                AlignToWall(m_HitChest.distance);
                OrientToWall(m_AverageNormal);

                if (Actor.MovementAxis.z > 0.1f && CheckLedgeExit())
                    ClimbEdge();
            }
            else
            {
                EndClimb();
            }
        }

        void ClimbEdge()
        {
            PlayEndAnim("ClimbEdge");
            // foreach (HurtBox hurtBox in Actor.HurtBoxes)
            //     hurtBox.RevertColliderSize();
        }

        public override void OnTriggerRangeEvent(AnimRangeTimeEvent eventObj, bool enter)
        {
            base.OnTriggerRangeEvent(eventObj, enter);
            if (eventObj.EventType == EAnimRangeEvent.Kinematic)
            {
                Actor.CPhysic.RB.isKinematic = enter;
            }
        }

        private void AlignToWall(float distance)
        {
            if (!Mathf.Approximately(distance, m_WallDistance))
            {
                float difference = distance - m_WallDistance;
                Vector3 align = 10 * DeltaTime * difference * Actor.transform.forward;
                Actor.CPhysic.AdditivePosition += align;
            }
        }

        private void OrientToWall(Vector3 normal)
        {
            Quaternion AlignRot = Quaternion.FromToRotation(Actor.transform.forward, -normal) * Actor.transform.rotation; //Calculate the orientation to Terrain 
            Quaternion Inverse_Rot = Quaternion.Inverse(Actor.transform.rotation);
            Quaternion Target = Inverse_Rot * AlignRot;
            Quaternion Delta = Quaternion.Lerp(Quaternion.identity, Target, Actor.DeltaTime * 10); //Calculate the Delta Align Rotation
            Actor.CPhysic.AdditiveRotation *= Delta;
        }


        private Transform CheckClimbRay()
        {
            m_HitChest = new RaycastHit();

            m_AverageNormal = Actor.transform.forward;
            float length = 1.5f;
            Vector3 rayDir = Actor.transform.forward;

            Debug.DrawRay(Point_Chest, rayDir * length, Color.green);
            Debug.DrawRay(Point_Chest, rayDir * m_WallDistance, Color.red);

            if (Physics.SphereCast(Point_Chest, m_RayRadius, rayDir, out m_HitChest, length, ClimbLayer, QueryTriggerInteraction.Ignore))
            {
                var valid = m_HitChest.collider.gameObject.CompareTag("Climb");

                if (valid)
                {
                    m_AverageNormal = m_HitChest.normal;
                    //DebugRays(m_HitChest.point, m_HitChest.normal);

                    //Get the Wall Angle!!
                    float wallAngle = Vector3.Angle(m_AverageNormal, Vector3.up);
                    if (wallAngle < 30)
                    {
                        return null;
                    }

                    return m_HitChest.collider.transform;
                }
            }

            return null;
        }

        /*
        private void DebugRays(Vector3 p, Vector3 Normal)
        {
            SDebug.DrawCircle(p, Normal, m_RayRadius, Color.green, true);
            // SDebug.DrawWireSphere(p + (Normal * RayRadius), Color.green, RayRadius);
            Debug.DrawRay(p, 2 * m_RayRadius * Normal, Color.green);
        }
        */

        private bool CheckLedgeExit()
        {
            var ledgePivotUP = Actor.transform.position + Vector3.up * (Actor.CPhysic.Height + 1f);
            Vector3 forward = Actor.transform.forward;
            float rayLedgeLength = Actor.CPhysic.Radius * 2f + 0.5f;
            //Check Upper Ground legde Detection
            bool ledgeHit = Physics.Raycast(ledgePivotUP, forward, out m_EdgeHit[0], rayLedgeLength, ClimbLayer, QueryTriggerInteraction.Ignore);

            SDebug.DrawWireSphere(ledgePivotUP, Color.green, 0.01f);
            SDebug.DrawWireSphere(m_EdgeHit[0].point, Color.green, 0.01f);
            Debug.DrawRay(ledgePivotUP, rayLedgeLength * forward, Color.green);

            if (ledgeHit)
            {
                return false;
            }

            Vector3 secondRayPivot = ledgePivotUP + forward * rayLedgeLength;

            SDebug.DrawWireSphere(secondRayPivot, Color.green, 0.01f);

            float ledgeExitDistance = 1.15f;
            Debug.DrawRay(secondRayPivot, ledgeExitDistance * Vector3.down, Color.yellow);

            if (Physics.Raycast(secondRayPivot, Vector3.down, out var DownHit, Actor.CPhysic.Height * 2f, ClimbLayer, QueryTriggerInteraction.Ignore))
            {
                SDebug.DrawWireSphere(DownHit.point, Color.white, 0.01f);

                if (DownHit.distance > ledgeExitDistance)
                {
                    return true;
                }
            }

            return false;
        }
    }
}