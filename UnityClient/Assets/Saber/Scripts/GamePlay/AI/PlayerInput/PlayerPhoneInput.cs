using System;
using Saber.Frame;
using Saber.UI;
using Saber.World;
using UnityEngine;
using Saber.CharacterController;

namespace Saber.AI
{
    public class PlayerPhoneInput : PlayerInput, Wnd_JoyStick.IHandler
    {
        private static Wnd_JoyStick s_WndJoyStick;
        private float? m_oldTouchDis;
        private bool m_Sprint;
        private bool m_PressDefense;
        private Portal m_CurrentStayingPortal;
        private GodStatue m_CurrentStayingGodStatue;
        private Vector3 m_Stick;
        private float m_StickLength;
        private bool m_ToCheckChargeAttack;
        private float m_PressDownHeavyAttackTime;


        public override bool Active
        {
            set => s_WndJoyStick.IsShow = value;
        }

        public override void Init(SActor actor)
        {
            base.Init(actor);

            // wnd
            if (s_WndJoyStick == null)
            {
                s_WndJoyStick = GameApp.Entry.UI.CreateWnd<Wnd_JoyStick>(null, this);
            }

            GameApp.Entry.Unity.DoActionOneFrameLater(() =>
            {
                this.Actor.CStats.OnHPPointCountChange += RefreshHPPointCount;
                RefreshHPPointCount();
            });

            actor.CStateMachine.Event_OnStateChange += OnStateChange;
        }

        private void OnStateChange(EStateType from, EStateType to)
        {
        }


        void RefreshHPPointCount()
        {
            int count = GameApp.Entry.Game.Player.CStats.HPPotionCount;
            s_WndJoyStick.RefreshMedicineCount(count);
        }

        public override void Update()
        {
            base.Update();

            UpdateCamera();

            if (m_PressDefense)
            {
                if (Actor.CurrentStateType != EStateType.Defense)
                    Actor.DefenseStart();
            }
            else
            {
                if (Actor.CurrentStateType == EStateType.Defense)
                    Actor.DefenseEnd();
            }
            
            UpdateMovement();
                
            UpdateHeavyAttack();
        }

        // 镜头
        void UpdateCamera()
        {
#if UNITY_EDITOR
            // 缩放
            float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
            float scale = GameHelper.IsAndroid ? 2f : 0.2f;
            float offset = scrollWheel * scale;
            if (Mathf.Abs(offset) > 0.001f)
            {
                PlayerCameraObj.Zoom(offset);
            }
#else
            // 缩放
            if (Input.touchCount == 2 && !Widget_JoyStick.IsDragging)
            {
                var touch0 = Input.GetTouch(0);
                var touch1 = Input.GetTouch(1);
                if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
                {
                    Vector2 v2Dis = touch0.position - touch1.position;
                    float dis = v2Dis.magnitude;

                    if (m_oldTouchDis != null)
                    {
                        float offset = (m_oldTouchDis.Value - dis) * 0.005f;
                        if (Mathf.Abs(offset) > 0.001f)
                        {
                            PlayerCameraObj.Zoom(-offset);
                        }
                    }

                    m_oldTouchDis = dis;
                }
            }
            else
            {
                m_oldTouchDis = null;
            }
#endif
        }


        #region Scene Interact

        public override void OnPlayerEnterPortal(Portal portal)
        {
            m_CurrentStayingPortal = portal;
            s_WndJoyStick.ShowButtonInteract(ESceneInteractType.Portal);
        }

        public override void OnPlayerExitPortal(Portal portal)
        {
            m_CurrentStayingPortal = null;
            s_WndJoyStick.HideButtonInteract();
        }

        public override void OnPlayerEnterGodStatue(GodStatue godStatue)
        {
            m_CurrentStayingGodStatue = godStatue;

            ESceneInteractType interactType = godStatue.IsFired ? ESceneInteractType.Rest : ESceneInteractType.Worship;
            s_WndJoyStick.ShowButtonInteract(interactType);
        }

        public override void OnPlayerExitGodStatue(GodStatue godStatue)
        {
            m_CurrentStayingGodStatue = null;
            s_WndJoyStick.HideButtonInteract();
        }

        #endregion


        #region Wnd_JoyStick.IHandler

        void Wnd_JoyStick.IHandler.OnClickInteract(ESceneInteractType interactType)
        {
            if (interactType == ESceneInteractType.Portal)
            {
                if (m_CurrentStayingPortal)
                    m_CurrentStayingPortal.Transmit();
            }
            else if (interactType == ESceneInteractType.Worship)
            {
                if (m_CurrentStayingGodStatue)
                {
                    m_CurrentStayingGodStatue.Worship();
                    s_WndJoyStick.ShowButtonInteract(ESceneInteractType.Rest);
                }
            }
            else if (interactType == ESceneInteractType.Rest)
            {
                if (m_CurrentStayingGodStatue)
                    m_CurrentStayingGodStatue.Rest();
            }
            else
            {
                throw new InvalidOperationException($"Unknown ESceneInteractType:{interactType}");
            }
        }

        void Wnd_JoyStick.IHandler.OnPressDefense(bool value)
        {
            m_PressDefense = value;
            if (value)
            {
                Actor.DefenseStart();
            }
            else
            {
                Actor.DefenseEnd();
            }
        }

        void Wnd_JoyStick.IHandler.OnUseCamStick(float x, float y)
        {
            PlayerCameraObj.MovementAxis = new Vector2(x, y);
        }

        void Wnd_JoyStick.IHandler.OnUseStick(Vector2 axisV2, bool isDragging)
        {
            m_Stick = new Vector3(axisV2.x, 0, axisV2.y);
            m_StickLength = m_Stick.magnitude;
            Actor.IsDraggingMovementAxis = isDragging;
        }

        private bool m_IsTryMoving;

        void UpdateMovement()
        {
            EMoveSpeedV moveSpeedV;
            if (m_StickLength <= 0.1f)
            {
                moveSpeedV = EMoveSpeedV.None;
            }
            else if (m_Sprint)
            {
                moveSpeedV = EMoveSpeedV.Sprint;
            }
            else if (m_StickLength > 0.5f)
            {
                moveSpeedV = EMoveSpeedV.Run;
            }
            else if (m_StickLength > 0.1f)
            {
                moveSpeedV = EMoveSpeedV.Walk;
            }
            else
            {
                moveSpeedV = EMoveSpeedV.None;
            }

            if (moveSpeedV != EMoveSpeedV.None)
            {
                if (!m_IsTryMoving)
                {
                    ClearAheadInput();
                }

                m_IsTryMoving = true;
                Actor.StartMove(moveSpeedV, m_Stick);
            }
            else
            {
                m_IsTryMoving = false;
                Actor.StopMove();
            }
        }

        void Wnd_JoyStick.IHandler.OnClickLightAttack()
        {
            OnTriggerSkill(ESkillType.LightAttack);
        }


        void Wnd_JoyStick.IHandler.OnPressFly(bool value)
        {
        }

        void Wnd_JoyStick.IHandler.OnPressDodge(bool value)
        {
            m_Sprint = value;
            if (value)
            {
                OnDodgeDown();
            }
        }

        void Wnd_JoyStick.IHandler.OnClickLockOn()
        {
            OnLockEnemyDown();
        }

        void Wnd_JoyStick.IHandler.OnClickDrinkMedicine()
        {
            Actor.UseItem(UseItem.EItemType.HpMedicine);
        }

        void Wnd_JoyStick.IHandler.OnClickSkill(ESkillType type)
        {
            OnTriggerSkill(type);
        }

        void Wnd_JoyStick.IHandler.OnPressHeavyAttack(bool press)
        {
            m_ToCheckChargeAttack = press;
            if (press)
            {
                m_PressDownHeavyAttackTime = Time.time;
            }
            else
            {
                if (Time.time - m_PressDownHeavyAttackTime < 0.2f)
                {
                    OnTriggerSkill(ESkillType.HeavyAttack);
                }
            }
        }

        void UpdateHeavyAttack()
        {
            if (m_ToCheckChargeAttack)
            {
                if (Time.time - m_PressDownHeavyAttackTime >= 0.2f)
                {
                    m_ToCheckChargeAttack = false;
                    OnTriggerSkill(ESkillType.ChargeAttack);
                }
            }
        }

        #endregion

        public override void Release()
        {
            base.Release();
            this.Actor.CStats.OnHPPointCountChange -= RefreshHPPointCount;
            if (s_WndJoyStick)
            {
                s_WndJoyStick.Destroy();
                s_WndJoyStick = null;
            }
        }
    }
}