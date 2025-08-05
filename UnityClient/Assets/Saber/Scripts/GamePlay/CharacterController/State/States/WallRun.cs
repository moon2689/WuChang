using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Saber.CharacterController
{
    public class WallRun : ActorStateBase
    {
        public const float StartHeight = 0.3f;

        private float m_WallDistance = 0.5f;
        private float m_SpeedYReduce;
        private RaycastHit m_WallHit;
        private string m_WallTag = "WallRun";
        private bool m_RightSide;
        private float m_SpeedY;
        private Transform m_WallFound; //Is the Actor on a Valid Wall
        private SCharacter Character;

        private float WallCheck => Character.CPhysic.Radius + m_WallDistance + 0.1f;
        private int Layer => EStaticLayers.Default.GetLayerMask();
        public override bool ApplyRootMotionSetWhenEnter => true;

        public override bool CanEnter
        {
            get
            {
                if (Character.CurrentStateType == EStateType.Jump)
                {
                    var jump = StateMachine.GetState<Jump>(EStateType.Jump);
                    if (!jump.InAir)
                        return false;
                }

                return IsInputing && CheckWallRay();
            }
        }

        private bool IsInputing => Character.MovementAxisMagnitude > 0.3f;
        Vector3 RayOriginPos => Character.transform.position + Vector3.up * Character.CPhysic.Height * 0.1f;


        public WallRun() : base(EStateType.None)
        {
        }
        
        public override void Init(ActorStateMachine parent)
        {
            base.Init(parent);
            Character = base.Actor as SCharacter;
        }

        public override void Enter()
        {
            base.Enter();
            m_WallFound = null;

            Character.CPhysic.UseGravity = false;
            Character.CPhysic.EnableSlopeMovement = false;

            m_SpeedY = Mathf.Clamp(Character.CPhysic.DeltaPos.y / DeltaTime, 0, 20);
            m_SpeedYReduce = m_SpeedY * 0.5f;
        }

        public override void OnStay()
        {
            base.OnStay();
            UpdateWallRun();

            bool canExit = !m_WallFound || Character.CPhysic.Grounded || !IsInputing;
            if (canExit)
                Exit();
        }

        protected override void OnExit()
        {
            base.OnExit();
            Character.CPhysic.UseGravity = true;
            Character.CPhysic.EnableSlopeMovement = true;
            m_WallFound = null;
        }

        private bool CheckWallRay()
        {
            Vector3 right = Character.transform.right;
            Debug.DrawRay(RayOriginPos, right * WallCheck, Color.green);
            Debug.DrawRay(RayOriginPos, -right * WallCheck, Color.green);
            Debug.DrawRay(RayOriginPos, right * m_WallDistance, Color.red);
            Debug.DrawRay(RayOriginPos, -right * m_WallDistance, Color.red);

            //Do not activate if the animal is to close to the ground
            if (StartHeight > 0 && Character.CPhysic.GroundDistance < StartHeight)
                return false;

            m_WallHit = new RaycastHit();

            if (Physics.Raycast(RayOriginPos, right, out m_WallHit, WallCheck, Layer))
            {
                var newWall = m_WallHit.transform;

                SDebug.DrawWireSphere(m_WallHit.point, Color.green, 0.05f, 0.5f);

                if (newWall != m_WallFound)
                {
                    m_WallFound = newWall;

                    if (string.IsNullOrEmpty(m_WallTag) || m_WallFound.CompareTag(m_WallTag)) //Check Wall Filter
                    {
                        m_RightSide = true; //Wall on the RIght Side
                        Character.CPhysic.SetPlatform(m_WallFound);
                        //Debug.Log("Wall detected on the Right");
                        return true;
                    }
                }
            }
            else if (Physics.Raycast(RayOriginPos, -right, out m_WallHit, WallCheck, Layer))
            {
                var newWall = m_WallHit.transform;
                SDebug.DrawWireSphere(m_WallHit.point, Color.green, 0.05f, 0.5f);

                if (newWall != m_WallFound)
                {
                    m_WallFound = newWall;
                    if (string.IsNullOrEmpty(m_WallTag) || m_WallFound.CompareTag(m_WallTag)) //Check Wall Filter
                    {
                        m_RightSide = false; //Wall on the Left Side
                        Character.CPhysic.SetPlatform(m_WallFound);
                        //Debug.Log("Wall detected on the Left");
                        return true;
                    }
                }
            }
            else
            {
                m_WallFound = null; //Clean wall, no wall was found
            }

            return false;
        }

        private void UpdateWallRun()
        {
            int dir = m_RightSide ? 1 : -1;
            Vector3 right = dir * Character.transform.right;
            string animName = m_RightSide ? "WallRunRight" : "WallRunLeft";

            Debug.DrawRay(RayOriginPos, right * WallCheck, Color.green);
            Debug.DrawRay(RayOriginPos, right * m_WallDistance, Color.red);

            // vertical speed
            if (m_SpeedY > 0)
            {
                m_SpeedY = Mathf.Max(0, m_SpeedY - m_SpeedYReduce * DeltaTime);
                Character.CPhysic.AdditivePosition += Vector3.up * m_SpeedY * DeltaTime;
            }

            if (Physics.Raycast(RayOriginPos, right, out m_WallHit, WallCheck, Layer))
            {
                var newWall = m_WallHit.transform;

                SDebug.DrawWireSphere(m_WallHit.point, Color.green, 0.05f, 0.5f);

                if (newWall != m_WallFound)
                {
                    m_WallFound = m_WallHit.transform;

                    if (!string.IsNullOrEmpty(m_WallTag) && !m_WallFound.CompareTag(m_WallTag)) //Check Wall Filter
                    {
                        m_WallFound = null; //No wall found so skip
                        return;
                    }
                }

                OrientToWall(right, m_WallHit.normal);
                AlignToWall(right, m_WallHit.distance);
            }
            else
            {
                m_WallFound = null; //No wall found so skip
                return;
            }

            if (!Character.CAnim.IsPlayingOrWillPlay(animName))
            {
                Character.CAnim.Play(animName);
            }

            /*
            //Do Wall Bank
            animal.Bank = Mathf.Lerp(animal.Bank, Dir * Bank, deltatime * animal.CurrentSpeedSet.BankLerp);

            //Debug.Log(Vector3.SignedAngle(animal.Forward,animal.DeltaVelocity,-animal.Right));

            var Pich = Vector3.SignedAngle(animal.Forward, animal.DeltaVelocity, animal.Right);
            animal.PitchAngle = Mathf.Lerp(animal.PitchAngle, Pich, deltatime * animal.CurrentSpeedSet.PitchLerpOn);

            animal.State_SetFloat(animal.PitchAngle);

            animal.CalculateRotator(); //Calculate the Rotator Rotation.
            */
        }

        //Align the Actor to the Wall
        private void AlignToWall(Vector3 direction, float distance)
        {
            float difference = distance - m_WallDistance;
            Vector3 align = direction * difference * DeltaTime * 10;
            Character.CPhysic.AdditivePosition += align;
        }

        private void OrientToWall(Vector3 right, Vector3 normal)
        {
            Quaternion tRot = Character.transform.rotation;
            Quaternion alignRot = Quaternion.FromToRotation(right, -normal) * tRot; //Calculate the orientation to Terrain 
            Quaternion inverseRot = Quaternion.Inverse(tRot);
            Quaternion target = inverseRot * alignRot;
            Quaternion delta = Quaternion.Lerp(Quaternion.identity, target, DeltaTime * 10); //Calculate the Delta Align Rotation
            Character.CPhysic.AdditiveRotation *= delta;
        }
    }
}