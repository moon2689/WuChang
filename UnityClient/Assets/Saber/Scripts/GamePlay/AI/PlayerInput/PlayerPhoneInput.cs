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
        private float m_PressDodgeDownTime, m_PressDodgeUpTime;
        private float m_PressFlyDownTime, m_PressFlyUpTime;
        private Vector3 m_Stick;
        private float m_StickLength;


        public override bool Active
        {
            set { s_WndJoyStick.IsShow = value; }
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

            // 冲刺处理输入
            UpdateSprintInput();

            UpdateFlyInput();
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
            /*
            if (!Actor.ArmedWeapon)
            {
                OnToggleWeapon();
                return;
            }

            m_PressDefense = value;
            if (value)
            {
                Actor.DefenseStart();
            }
            else
            {
                Actor.DefenseEnd();
            }
            */
            if (value)
                OnTriggerSkill(ESkillType.Defense);
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

        void UpdateMovement()
        {
            if (m_StickLength <= 0.1f)
            {
                Actor.StopMove();
            }
            else if (m_Sprint)
            {
                Actor.MoveSpeedV = EMoveSpeedV.Sprint;
                Actor.MovementAxis = m_Stick;
                Actor.StartMove();
            }
            else if (m_StickLength > 0.5f)
            {
                Actor.MoveSpeedV = EMoveSpeedV.Run;
                Actor.MovementAxis = m_Stick;
                Actor.StartMove();
            }
            else if (m_StickLength > 0.1f)
            {
                Actor.MoveSpeedV = EMoveSpeedV.Walk;
                Actor.MovementAxis = m_Stick;
                Actor.StartMove();
            }
            else
            {
                Actor.StopMove();
            }
        }

        void Wnd_JoyStick.IHandler.OnClickLightAttack()
        {
            OnTriggerSkill(ESkillType.LightAttack);
        }


        void Wnd_JoyStick.IHandler.OnPressFly(bool value)
        {
            if (value)
            {
                m_PressFlyDownTime = Time.time;
            }
            else
            {
                if (Time.time - m_PressFlyDownTime < 0.2f)
                {
                    Actor.ToggleFly();
                }
                else if (Actor.CurrentStateType == EStateType.Fly)
                {
                    Actor.ToggleFlyRise(false);
                }

                m_PressFlyUpTime = Time.time;
            }
        }

        void UpdateFlyInput()
        {
            if (Actor.CurrentStateType == EStateType.Fly)
            {
                if (m_PressFlyDownTime > m_PressFlyUpTime && Time.time - m_PressFlyDownTime > 0.2f)
                {
                    Actor.ToggleFlyRise(true);
                }
            }
        }

        void Wnd_JoyStick.IHandler.OnPressDodge(bool value)
        {
            if (value)
            {
                // 长按冲刺后，再按则跳跃
                if (Time.time - m_PressDodgeUpTime < 0.2f && m_PressDodgeUpTime - m_PressDodgeDownTime > 1f)
                {
                    OnJumpDown();
                }
                else
                {
                    bool glideSucceed = Actor.ToggleGlide();
                    if (!glideSucceed)
                        OnDodgeDown();
                }

                m_PressDodgeDownTime = Time.time;
            }
            else
            {
                m_PressDodgeUpTime = Time.time;
            }
        }

        /// <summary>处理冲刺输入</summary>
        void UpdateSprintInput()
        {
            if (m_Sprint)
            {
                // 放开后在0.2秒内未按下则取消冲刺
                if (m_PressDodgeDownTime < m_PressDodgeUpTime && Time.time - m_PressDodgeUpTime > 0.2f)
                {
                    m_Sprint = false;
                    // if (Actor.CurrentStateType == EStateType.Fly)
                    // {
                    //     Actor.ToggleFlyRise(false);
                    // }
                }
            }
            else
            {
                // 按下后在0.2秒内未放开则冲刺
                if (m_PressDodgeDownTime > m_PressDodgeUpTime && Time.time - m_PressDodgeDownTime > 0.2f)
                {
                    m_Sprint = true;

                    // if (Actor.CurrentStateType == EStateType.Fly)
                    // {
                    //     Actor.ToggleFlyRise(true);
                    // }
                }
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
            Actor.CMelee.IsPressingHeavyAttack = press;
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