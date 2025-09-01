using System;
using System.Collections;
using UnityEngine;

namespace Saber.CharacterController
{
    public class SCharacter : SActor
    {
        [SerializeField] public CharacterInfo m_CharacterInfo;

        public event Action<Collider> EventOnTriggerEnter, EventOnTriggerExit;
        public event Action<EMoveSpeedV, EMoveSpeedV> EventOnMoveSpeedVChange;

        private CharacterStateMachine m_CStates;
        

        public CharacterRender CRender { get; private set; }
        // public CharacterSpeech CSpeech { get; private set; }
        // public CharacterIK CIK { get; private set; }
        // public CharacterExpression CExpression { get; private set; }


        public override ActorStateMachine CStateMachine => m_CStates;

        // public override bool CanSpeech => CSpeech.IsValid;
        // public override bool IsSpeeching => CSpeech.IsSpeeching;


        public EMoveSpeedV MaxMoveSpeedV { get; set; } = EMoveSpeedV.Sprint;

        public override EMoveSpeedV MoveSpeedV
        {
            get => m_MoveSpeedV;
            set
            {
                EMoveSpeedV oldSpeed = MoveSpeedV;

                m_MoveSpeedV = value;
                if (m_MoveSpeedV == EMoveSpeedV.Sprint && CStats.CurrentStamina <= 0)
                {
                    m_MoveSpeedV = EMoveSpeedV.Run;
                }

                if (CurrentStateType == EStateType.Move &&
                    CRender.IsInWater &&
                    (m_MoveSpeedV == EMoveSpeedV.Sprint || m_MoveSpeedV == EMoveSpeedV.Run))
                {
                    m_MoveSpeedV = EMoveSpeedV.Walk;
                }

                if (m_MoveSpeedV > MaxMoveSpeedV)
                {
                    m_MoveSpeedV = MaxMoveSpeedV;
                }

                if (m_MoveSpeedV != oldSpeed)
                    EventOnMoveSpeedVChange?.Invoke(oldSpeed, m_MoveSpeedV);
            }
        }


        protected override void Awake()
        {
            gameObject.SetLayerRecursive(EStaticLayers.Actor);

            CBuff = new();
            CStats = new ActorBaseStats(this);
            CPhysic = new CharacterPhysic(this, m_BaseActorInfo.m_PhysicInfo);
            CAnim = new CharacterAnimation(this, this);
            //CIK = new CharacterIK(this, CAnim.AnimatorObj, m_CharacterInfo.m_IKInfo);
            m_CStates = new CharacterStateMachine(this);
            CAbility = new CharacterAbility(this);
            CMelee = new CharacterMelee(this, base.SkillConfigs);
            CRender = new CharacterRender(this);
            //CSpeech = new CharacterSpeech();
            CDressUp = new CharacterDressUp(this, m_CharacterInfo.m_ClothInfo);
            //CExpression = new CharacterExpression( m_CharacterInfo.m_ExpressionInfo);

            // 设置默认武器
            CMelee.SetWeapon(m_BaseActorInfo.m_WeaponPrefabs);

            /*
            if (CDressUp.Enable)
            {
                CDressUp.UpdateFootRotationForHighHeels = !CIK.Enable;
            }

            if (CIK.Enable)
            {
                CIK.Event_OnUpdateIKRotation += OnIKUpdateRotation;
            }*/
        }

        protected override void OnAnimatorMove()
        {
            DeltaTime = Time.fixedDeltaTime;

            UpdateAnimatorParams();

            CPhysic.ResetMotionValues();

            CStateMachine.Update();
            CAbility.Update();
            CAnim.Update();

            CPhysic.Update();

            //CExpression.Update(DeltaTime);
            CMelee.Update();
        }

        /*
        private void FixedUpdate()
        {
            StartCoroutine(AfterFixedUpdate());
        }

        private IEnumerator AfterFixedUpdate()
        {
            yield return new WaitForFixedUpdate();
            CDressUp.AfterFixedUpdate();
            CIK.AfterFixedUpdate();
        }
        */

        void UpdateAnimatorParams()
        {
            if (UpdateMovementAxisAnimatorParams)
            {
                CAnim.SetSmoothFloat(EAnimatorParams.Horizontal, MovementAxis.x * (int)MoveSpeedV);
                CAnim.SetSmoothFloat(EAnimatorParams.Vertical, MovementAxis.z * (int)MoveSpeedV);
                /*
                if (StrafeMode)
                {
                    CAnim.SetSmoothFloat(EAnimatorParams.Horizontal, MovementAxis.x * (int)MoveSpeedV);
                    CAnim.SetSmoothFloat(EAnimatorParams.Vertical, MovementAxis.z * (int)MoveSpeedV);
                }
                else
                {
                    CAnim.SetSmoothFloat(EAnimatorParams.Horizontal, 0);
                    CAnim.SetSmoothFloat(EAnimatorParams.Vertical, MovementAxisMagnitude * (int)MoveSpeedV);
                }*/
            }
        }

        /*
        #region IK

        // 当使用IK时，使用foot ik 来更新高跟靯，否则自己来更新
        private Quaternion OnIKUpdateRotation(CharacterIK.IKGoal arg1, Quaternion arg2, Vector3 arg3)
        {
            if (CDressUp.Enable)
            {
                if (arg1 == CharacterIK.IKGoal.LeftFoot)
                {
                    return arg2 * CDressUp.LeftFootRot;
                }
                else if (arg1 == CharacterIK.IKGoal.RightFoot)
                {
                    return arg2 * CDressUp.RightFootRot;
                }
            }

            return arg2;
        }

        private void OnAnimatorIK(int layerIndex)
        {
            CIK.OnAnimatorIK(layerIndex);
        }

        public override void SetGroundOffset(float offset)
        {
            if (CIK.Enable)
            {
                m_BaseActorInfo.m_PhysicInfo.m_GroundOffset = 0;
                m_CharacterInfo.m_IKInfo.m_HighHeelsHeight = offset;
            }
            else
            {
                base.SetGroundOffset(offset);
            }
        }

        #endregion
        */


        /// <summary>当触发动画事件，此函数通过SendMessange触发</summary>
        public override void OnTriggerAnimEvent(AnimPointTimeEvent eventObj)
        {
            //Debug.Log($"OnTriggerAnimEvent {eventItem.m_AnimEvent}");
            base.OnTriggerAnimEvent(eventObj);
            CAbility.CurAbility?.OnTriggerAnimEvent(eventObj);
        }

        private void OnTriggerEnter(Collider other)
        {
            EventOnTriggerEnter?.Invoke(other);
            //Debug.Log($"OnTriggerEnter {other.name}", other);
        }

        private void OnTriggerExit(Collider other)
        {
            EventOnTriggerExit?.Invoke(other);
            //Debug.Log($"OnTriggerExit {other.name}", other);
        }

        /*
        private void OnTriggerStay(Collider other)
        {
            Debug.Log($"OnTriggerStay {other.name}", other);
        }
        */

        public override void OnAnimEnter(int nameHash, int layer)
        {
            base.OnAnimEnter(nameHash, layer);
            CAbility.CurAbility?.OnAnimEnter(nameHash, layer);
        }

        public override void OnAnimExit(int nameHash, int layer)
        {
            base.OnAnimExit(nameHash, layer);
            CAbility.CurAbility?.OnAnimExit(nameHash, layer);
        }

        public override void Speech()
        {
            // CSpeech.RandomSpeech();
        }

        public override void DrinkPotion()
        {
            base.DrinkPotion();
            CAbility.DrinkMedicine();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CRender.Release();
            AI = null;
        }

        public override void OnGodStatueRest()
        {
            base.OnGodStatueRest();
            CRender.OnGodStatueRest();
        }
    }
}