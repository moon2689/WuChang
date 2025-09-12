using System;
using Saber.Frame;
using Saber.CharacterController;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using System.Collections;
using UnityEngine.Serialization;
using YooAsset;

namespace Saber
{
    public class PlayerCamera : MonoBehaviour
    {
        public interface ITarget
        {
            Vector3 Position { get; }
            Vector3 LockPosition { get; }
            Quaternion Rotation { get; }
            float Height { get; }
            bool IsMoving { get; }
        }

        enum ELookAtLockTargetType
        {
            None,
            Slow,
            Fast,
        }

        // comparer for check distances in ray cast hits
        public class RayHitComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                return ((RaycastHit)x).distance.CompareTo(((RaycastHit)y).distance);
            }
        }

        // 单例
        private static PlayerCamera s_Instance;
        public static PlayerCamera Instance => s_Instance;


        public Action<float> OnDistanceChange;

        [Header("Base Setting")]
        // How fast the rig will move to keep up with the target's position.
        [SerializeField]
        float m_MoveSpeed = 10f;

        [Range(0f, 10f)] [SerializeField] float m_TurnSpeed = 10f; // How fast the rig will rotate from user input.

        // How much smoothing to apply to the turn input, to reduce mouse-turn jerkiness
        [SerializeField] float m_TurnSmoothing = 10f;

        [SerializeField] float m_TiltMax = 75f; // The maximum value of the x axis rotation of the pivot.
        [SerializeField] float m_TiltMin = 45f; // The minimum value of the x axis rotation of the pivot.

        [SerializeField] Vector2 m_MovementAxis;

        private float m_LookAngle; // The rig's y axis rotation.
        private float m_TiltAngle; // The pivot's x axis rotation.
        private Vector3 m_PivotEulers;
        private Vector3 m_UpVector;
        private Quaternion m_PivotTargetRot;
        private Quaternion m_TransformTargetRot;
        private float m_Height;
        private float m_TimerLockTarget;
        ITarget m_Target; // The target object to follow
        private ITarget m_LockTarget;

        private PlayerCameraShake m_Shake;

        // private Vector3 m_LockTargetViewportPoint;
        private bool m_IsInputingMovementAxis;
        private ELookAtLockTargetType m_LookAtLockTargetType;


        [Header("Wall Stop")]
        // time taken to move when avoiding cliping (low value = fast, which it should be)
        [SerializeField]
        float m_ClipMoveTime = 0.05f;

        // time taken to move back towards desired position, when not clipping (typically should be a higher value than clipMoveTime)
        [SerializeField] float m_ReturnTime = 0.4f;

        // the radius of the sphere used to test for object between camera and target
        [SerializeField] float m_SphereCastRadius = 0.15f;

        // the closest distance the camera can be from the target
        [SerializeField] float m_ClosestDistance = 0.5f;

        //Dont clip Player
        [SerializeField] LayerMask m_DontClip = 1 << 20;

        // the original distance to the camera before any modification are made
        [SerializeField] private float m_OriginalDist;

        // the velocity at which the camera moved
        private float m_MoveVelocity;

        // the current distance from the camera to the target
        private float m_CurrentDist;

        // the ray used in the lateupdate for casting between the camera and the target
        private Ray m_Ray = new Ray();

        // the hits between the camera and the target
        private RaycastHit[] hits;

        // variable to compare raycast hit distances
        private RayHitComparer m_RayHitComparer;

        private float m_CamNearClipPlaneHeight;


        public ITarget Target
        {
            get => m_Target;
            private set => m_Target = value;
        }

        /// <summary>Main Camera</summary>
        public Camera Cam { get; private set; }

        /// <summary>Main Camera Transform</summary>
        public Transform CamT { get; private set; }

        public Transform Pivot { get; private set; }

        public ITarget LockTarget
        {
            get => m_LockTarget;
            set
            {
                m_LockTarget = value;

                if (value != null)
                {
                    m_TimerLockTarget = 1.5f;
                }
                else
                {
                    LookAtTarget(CamT.rotation.eulerAngles.y);
                }
            }
        }


        public Vector2 MovementAxis
        {
            get => m_MovementAxis;
            set
            {
                m_MovementAxis = value;
                m_IsInputingMovementAxis = true;
            }
        }

        public float CamDistance => CamT.transform.localPosition.z;

        // used for determining if there is an object between the target and the camera
        public bool Protecting { get; private set; }


        public static AssetHandle Create()
        {
            if (s_Instance == null)
            {
                return GameApp.Entry.Asset.LoadGameObject("Game/PlayerCameraRig", go => s_Instance = go.GetComponent<PlayerCamera>());
            }

            return null;
        }


        public void SetTarget(ITarget target)
        {
            Target = target;
            m_Height = target.Height * 0.7f;
            SetHeight(m_Height);
            Zoom(0);
        }

        public void ClearTarget()
        {
            Target = null;
            m_LockTarget = null;
        }

        void SetHeight(float height)
        {
            height = Mathf.Clamp(height, 0.1f, m_Height * 1.5f);
            Pivot.transform.localPosition = new Vector3(0, height, 0);
        }

        private void OnDestroy()
        {
            s_Instance = null;
        }

        protected void Awake()
        {
            DontDestroyOnLoad(gameObject);

            Cam = GetComponentInChildren<Camera>();
            CamT = Cam.transform;
            Pivot = Cam.transform.parent;

            m_PivotEulers = Pivot.rotation.eulerAngles;
            m_PivotTargetRot = Pivot.transform.localRotation;
            m_TransformTargetRot = transform.localRotation;

            MovementAxis = Vector2.zero;

            ResetWithState();
            m_RayHitComparer = new RayHitComparer(); // create a new RayHitComparer
        }

        private void HandleFreeMovement(float deltaTime)
        {
            if (Time.timeScale < float.Epsilon)
                return;

            // Move the rig towards target position.
            transform.position = Vector3.Lerp(transform.position, Target.Position, deltaTime * m_MoveSpeed);

            // Adjust the look angle by an amount proportional to the turn speed and horizontal input.
            m_LookAngle += MovementAxis.x * m_TurnSpeed;

            //if (TargetGravity != null) m_UpVector = Vector3.Slerp(m_UpVector, TargetGravity.UpVector, time * 15);
            // transform.rotation = Quaternion.FromToRotation(transform.up, m_UpVector) * transform.rotation; //This Make it f
            //m_TransformTargetRot = Quaternion.FromToRotation(transform.up, m_UpVector) * Quaternion.Euler(0f, m_LookAngle, 0f); // Rotate the rig (the root object) around Y axis only:

            // Rotate the rig (the root object) around Y axis only:
            m_TransformTargetRot = Quaternion.Euler(0f, m_LookAngle, 0f);

            bool isClosest = CamDistance == -m_ClosestDistance;
            if (isClosest)
            {
                float newHeight = Pivot.transform.localPosition.y - MovementAxis.y * deltaTime;
                SetHeight(newHeight);
            }
            else
            {
                // on platforms with a mouse, we adjust the current angle based on Y mouse input and turn speed
                m_TiltAngle -= MovementAxis.y * m_TurnSpeed;
            }

            // and make sure the new value is within the tilt range
            m_TiltAngle = Mathf.Clamp(m_TiltAngle, -m_TiltMin, m_TiltMax);
            // Tilt input around X is applied to the pivot (the child of this object)
            m_PivotTargetRot = Quaternion.Euler(m_TiltAngle, m_PivotEulers.y, m_PivotEulers.z);

            float deltaSmooth = m_TurnSmoothing * deltaTime;
            Pivot.localRotation = Quaternion.Slerp(Pivot.localRotation, m_PivotTargetRot, deltaSmooth);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, m_TransformTargetRot, deltaSmooth);
        }

        public void LookAtTarget(float eulerAnglesY)
        {
            m_LookAngle = eulerAnglesY;
            m_TiltAngle = 10;
            MovementAxis = Vector2.zero;
        }

        public void LookAtTargetBack()
        {
            LookAtTarget(m_Target.Rotation.eulerAngles.y);
        }

        public void ResetPosition()
        {
            transform.position = Target.Position;
        }

        void LateUpdate()
        {
            //HandleFreeMovement(Time.deltaTime);
            WallStop();
        }

        void FixedUpdate()
        {
            if (Target == null)
            {
                return;
            }

            if (LockTarget != null)
            {
                HandleLockTarget(Time.fixedDeltaTime);
            }
            else
            {
                HandleFreeMovement(Time.fixedDeltaTime);
            }

            m_IsInputingMovementAxis = false;
        }

        void CalcViewPoint(out bool outScreen, out bool outCenterRect)
        {
            Vector3 lockPos = LockTarget.LockPosition;
            Vector3 viewPoint = Cam.WorldToViewportPoint(lockPos);
            outScreen = viewPoint.x < 0.1f || viewPoint.x > 0.9f || viewPoint.y < 0.1f || viewPoint.y > 0.9f;
            outCenterRect = viewPoint.x < 0.4f || viewPoint.x > 0.7f || viewPoint.y < 0.5f || viewPoint.y > 0.8f;
        }

        void CheckWhetherLookAtTarget(Vector3 dirToLockTarget)
        {
            if (m_IsInputingMovementAxis || MovementAxis != Vector2.zero)
            {
                m_LookAtLockTargetType = ELookAtLockTargetType.None;
                return;
            }

            if (m_TimerLockTarget > 0)
                m_TimerLockTarget -= Time.deltaTime;
            else
                m_LookAtLockTargetType = ELookAtLockTargetType.None;

            if (m_LookAtLockTargetType == ELookAtLockTargetType.None)
            {
                // 是否在移动
                if (Target.IsMoving)
                {
                    m_LookAtLockTargetType = ELookAtLockTargetType.Fast;
                }

                // 是否在背面
                if (m_LookAtLockTargetType == ELookAtLockTargetType.None)
                {
                    if (Vector3.Dot(dirToLockTarget, CamT.forward) < 0)
                    {
                        m_LookAtLockTargetType = ELookAtLockTargetType.Fast;
                    }
                }

                // 是否超出屏幕
                if (m_LookAtLockTargetType == ELookAtLockTargetType.None)
                {
                    CalcViewPoint(out bool outScreen, out bool outCenterRect);
                    if (outScreen)
                        m_LookAtLockTargetType = ELookAtLockTargetType.Fast;
                    else if (outCenterRect)
                        m_LookAtLockTargetType = ELookAtLockTargetType.Slow;
                }

                if (m_LookAtLockTargetType != ELookAtLockTargetType.None)
                    m_TimerLockTarget = 1.5f;
            }
            else if (m_LookAtLockTargetType == ELookAtLockTargetType.Slow)
            {
                // 是否超出屏幕
                CalcViewPoint(out bool outScreen, out bool _);
                if (outScreen)
                {
                    m_LookAtLockTargetType = ELookAtLockTargetType.Fast;
                    m_TimerLockTarget = 1.5f;
                }
            }

            //Debug.Log($"ViewPoint:{viewPoint}  {m_LookAtLockTargetType}");
        }

        private void HandleLockTarget(float deltaTime)
        {
            if (Time.timeScale < float.Epsilon)
                return;

            Vector3 dirToLockTarget = LockTarget.Position - Target.Position;
            Vector3 lockDirRight = Vector3.Cross(dirToLockTarget, Vector3.down).normalized;
            Vector3 tarPos = Target.Position + lockDirRight * 0.5f; //摄像机位置偏向于主角右边
            transform.position = Vector3.Lerp(transform.position, tarPos, deltaTime * m_MoveSpeed);

            // 锁定目标时，仍可手动调整摄像机方向
            if (m_IsInputingMovementAxis || MovementAxis != Vector2.zero)
            {
                float speed = m_TurnSmoothing * deltaTime * 10;
                transform.localRotation *= Quaternion.Euler(0f, MovementAxis.x * speed, 0f);
                Pivot.localRotation *= Quaternion.Euler(-MovementAxis.y * speed, 0, 0f);
                return;
            }

            // whether look at target
            CheckWhetherLookAtTarget(dirToLockTarget);

            if (m_LookAtLockTargetType != ELookAtLockTargetType.None)
            {
                Vector3 newLockDir = LockTarget.LockPosition - (Target.Position + Vector3.up * Target.Height + lockDirRight * 0.5f);

                // 垂直方向
                Vector3 projectDir = Vector3.ProjectOnPlane(newLockDir, transform.right);
                float angle = Vector3.SignedAngle(transform.forward, projectDir, transform.right);
                angle += 10f;

                float speed = m_LookAtLockTargetType == ELookAtLockTargetType.Fast ? 1 : 0.1f;
                speed *= m_TurnSmoothing * deltaTime;

                if (Mathf.Abs(angle) < 90)
                {
                    m_PivotTargetRot = Quaternion.Euler(angle, m_PivotEulers.y, m_PivotEulers.z);
                    Pivot.localRotation = Quaternion.Slerp(Pivot.localRotation, m_PivotTargetRot, speed);
                }

                // 水平方向,摄像机目标点在锁定敌人左边
                m_TransformTargetRot = Quaternion.LookRotation(new Vector3(newLockDir.x, 0, newLockDir.z));
                transform.localRotation = Quaternion.Slerp(transform.localRotation, m_TransformTargetRot, speed);
            }
        }

        /*
        private void OnGUI()
        {
            if (m_LockTargetCirclePoint && LockTarget != null)
            {
                float x = Screen.width * m_LockTargetViewportPoint.x;
                float y = Screen.height * (1 - m_LockTargetViewportPoint.y);
                GUI.DrawTexture(new Rect(x, y, 10, 10), m_LockTargetCirclePoint);
            }
        }
        */

        public void Zoom(float offset)
        {
            Vector3 pos = CamT.transform.localPosition;
            pos.z += offset;
            pos.z = Mathf.Clamp(pos.z, -15, -m_ClosestDistance);
            CamT.transform.localPosition = pos;
            ResetWithState();

            OnDistanceChange?.Invoke(Mathf.Abs(pos.z));
        }

        void ResetWithState()
        {
            m_OriginalDist = CamT.localPosition.magnitude;
            m_CurrentDist = m_OriginalDist;
        }

        private void OnTriggerEnter(Collider other)
        {
            bool isWater = CharacterRender.IsWater(other.gameObject);
            if (isWater)
            {
                URPFeatureUnderWater.s_IsActibe = true;
                m_CamNearClipPlaneHeight = 2 * Cam.nearClipPlane * Mathf.Tan(Cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            }
            //Debug.Log($"camera OnTriggerEnter {other.name} isWater:{isWater}", other);
        }

        private void OnTriggerExit(Collider other)
        {
            bool isWater = CharacterRender.IsWater(other.gameObject);
            if (isWater)
            {
                URPFeatureUnderWater.s_IsActibe = false;
            }
            //Debug.Log($"camera OnTriggerExit {other.name} isWater:{isWater}", other);
        }

        private void OnTriggerStay(Collider other)
        {
            bool isWater = CharacterRender.IsWater(other.gameObject);
            if (isWater && URPFeatureUnderWater.s_Material)
            {
                float waterY = other.gameObject.transform.position.y;
                float rate = 1;
                if (Cam.transform.position.y > waterY - m_CamNearClipPlaneHeight)
                {
                    Vector3 camNearClipCenter = Cam.transform.position + Cam.transform.forward * Cam.nearClipPlane;
                    Vector3 camNearClipPlaneUpV = Cam.transform.up * m_CamNearClipPlaneHeight;
                    Vector3 camNearClipTop = camNearClipCenter + camNearClipPlaneUpV * 0.5f;
                    Vector3 camNearClipBottom = camNearClipCenter - camNearClipPlaneUpV * 0.5f;
                    rate = Mathf.Clamp01((waterY - camNearClipBottom.y) / (camNearClipTop.y - camNearClipBottom.y));
                }

                URPFeatureUnderWater.s_Material.SetFloat("_ScreenPosY", rate);
            }
        }

        public void ShakeCamera(float duration, float amount, float speed)
        {
            if (m_Shake == null)
            {
                m_Shake = Cam.GetComponent<PlayerCameraShake>();
            }

            m_Shake.ActivateCameraShake(duration, amount, speed);
        }

        void WallStop()
        {
            float targetDist = m_OriginalDist; // initially set the target distance

            m_Ray.origin = Pivot.position + Pivot.forward * m_SphereCastRadius;
            m_Ray.direction = -Pivot.forward;

            // initial check to see if start of spherecast intersects anything
            var cols = Physics.OverlapSphere(m_Ray.origin, m_SphereCastRadius);

            bool initialIntersect = false;
            bool hitSomething = false;

            for (int i = 0; i < cols.Length; i++) // loop through all the collisions to check if something we care about
            {
                //is on a layer we don't want to clip
                if ((!cols[i].isTrigger) && !(STools.CollidersLayer(cols[i], m_DontClip)))
                {
                    initialIntersect = true;
                    break;
                }
            }

            if (initialIntersect) // if there is a collision
            {
                m_Ray.origin += Pivot.forward * m_SphereCastRadius;
                // do a raycast and gather all the intersections
                hits = Physics.RaycastAll(m_Ray, m_OriginalDist - m_SphereCastRadius);
            }
            else // if there was no collision do a sphere cast to see if there were any other collisions
            {
                hits = Physics.SphereCastAll(m_Ray, m_SphereCastRadius, m_OriginalDist + m_SphereCastRadius);
            }

            Array.Sort(hits, m_RayHitComparer); // sort the collisions by distance


            float nearest = Mathf.Infinity; // set the variable used for storing the closest to be as far as possible


            for (int i = 0; i < hits.Length; i++) // loop through all the collisions
            {
                // only deal with the collision if it was closer than the previous one, not a trigger, not in the Layer Mask
                if (hits[i].distance < nearest &&
                    (!hits[i].collider.isTrigger) &&
                    !STools.CollidersLayer(hits[i].collider, m_DontClip))
                {
                    nearest = hits[i].distance; // change the nearest collision to latest
                    targetDist = -Pivot.InverseTransformPoint(hits[i].point).z;
                    hitSomething = true;
                }
            }


            if (hitSomething) // visualise the cam clip effect in the editor
            {
                Debug.DrawRay(m_Ray.origin, -Pivot.forward * (targetDist + m_SphereCastRadius), Color.red);
            }

            // hit something so move the camera to a better position
            Protecting = hitSomething;
            m_CurrentDist = Mathf.SmoothDamp(m_CurrentDist, targetDist, ref m_MoveVelocity,
                m_CurrentDist > targetDist ? m_ClipMoveTime : m_ReturnTime);

            m_CurrentDist = Mathf.Clamp(m_CurrentDist, m_ClosestDistance, m_OriginalDist);
            CamT.localPosition = -Vector3.forward * m_CurrentDist;
        }
    }
}