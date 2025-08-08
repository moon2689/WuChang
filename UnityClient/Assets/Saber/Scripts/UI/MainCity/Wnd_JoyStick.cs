using System;
using Saber.Frame;
using Saber.CharacterController;
using Saber.World;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Saber.UI
{
    public class Wnd_JoyStick : WndBase
        , Widget_JoyStick.IHandler
        , Widget_MoveCamera.IHandler
        , Widget_SkillButton.IHandler
    {
        public interface IHandler : IWndHandler
        {
            void OnUseStick(Vector2 axis, bool isDragging);
            void OnUseCamStick(float x, float y);

            //void OnPressAttackA(bool value);
            void OnClickLightAttack();
            void OnPressHeavyAttack(bool press);

            //void OnClickHeavyAttack();
            void OnPressDodge(bool value);

            //void OnClickJump();

            //void OnPressJump(bool value);
            //void OnPressSprint(bool value);
            void OnClickLockOn();

            //void OnClickArm();
            void OnPressDefense(bool value);

            void OnClickInteract(ESceneInteractType interactType);

            //void OnTriggerSkill(SkillItem skillConfig);
            //void OnClickGlide();
            //void OnClickSlide();
            //void OnClickClimb();
            void OnPressFly(bool value);
            void OnClickDrinkMedicine();
            void OnClickSkill(ESkillType type);
        }


        [SerializeField] private Widget_JoyStick m_joystick;
        [SerializeField] private Widget_MoveCamera m_moveCamera;

        [SerializeField] private Button m_btnAttackA,
            m_ButtonHeavyAttack,
            m_btnRoll,
            m_btnLockOn,
            //m_btnArm,
            m_ButtonFly,
            m_btnInteract,
            m_ButtonDrinkMedicine,
            m_ButtonMedicineNone,
            m_ButtonDefense;

        [SerializeField] private Widget_SkillButton m_ButtonSkill1,
            m_ButtonSkill2,
            m_ButtonSkill3;

        [SerializeField] private GameObject m_parentButtons;
        [SerializeField] private Text m_TextBtnInteract;
        [SerializeField] private Text m_MedicineCount;

        private IHandler m_Handler;
        private ESceneInteractType m_InteractType;


        public bool IsShowJoyStick
        {
            set
            {
                m_joystick.gameObject.SetActive(value);
                m_parentButtons.gameObject.SetActive(value);
            }
        }

        bool ActiveButtonInteract
        {
            set => m_btnInteract.gameObject.SetActive(value);
        }

        protected override bool PauseGame => false;


        protected override void OnAwake()
        {
            base.OnAwake();
            m_Handler = m_WndHandler as IHandler;

            m_joystick.Init(this);
            m_moveCamera.Init(this);

            m_btnAttackA.onClick.AddListener(OnClickAttack1);
            m_ButtonHeavyAttack.AddEvent(EventTriggerType.PointerDown, OnPressDownHeavyAttack);
            m_ButtonHeavyAttack.AddEvent(EventTriggerType.PointerUp, OnPressUpHeavyAttack);
            //m_btnRoll.onClick.AddListener(OnClickRoll);
            //m_btnJump.onClick.AddListener(OnClickJump);
            m_btnLockOn.onClick.AddListener(OnClickLockOn);
            //m_btnArm.onClick.AddListener(OnClickArm);
            //m_ButtonFly.onClick.AddListener(OnClickFly);
            m_btnInteract.onClick.AddListener(OnClickInteract);
            m_ButtonDrinkMedicine.onClick.AddListener(OnClickDrinkMedicine);
            m_ButtonMedicineNone.onClick.AddListener(OnClickMedicineNone);

            m_ButtonFly.AddEvent(EventTriggerType.PointerDown, OnPressDownFly);
            m_ButtonFly.AddEvent(EventTriggerType.PointerUp, OnPressUpFly);

            m_btnRoll.AddEvent(EventTriggerType.PointerDown, OnPressDownDodge);
            m_btnRoll.AddEvent(EventTriggerType.PointerUp, OnPressUpDodge);

            m_ButtonDefense.AddEvent(EventTriggerType.PointerDown, OnPressDownDefense);
            m_ButtonDefense.AddEvent(EventTriggerType.PointerUp, OnPressUpDefense);

            ActiveButtonInteract = false;

            m_ButtonSkill1.Init(ESkillType.Skill1, this);
            m_ButtonSkill2.Init(ESkillType.Skill2, this);
            m_ButtonSkill3.Init(ESkillType.Skill3, this);
        }

        private void OnPressDownFly(BaseEventData arg0)
        {
            m_Handler?.OnPressFly(true);
        }

        private void OnPressUpFly(BaseEventData arg0)
        {
            m_Handler?.OnPressFly(false);
        }

        void OnPressDownHeavyAttack(BaseEventData arg0)
        {
            m_Handler?.OnPressHeavyAttack(true);
        }

        void OnPressUpHeavyAttack(BaseEventData arg0)
        {
            m_Handler?.OnPressHeavyAttack(false);
        }

        void OnPressDownDefense(BaseEventData arg0)
        {
            m_Handler?.OnPressDefense(true);
        }

        void OnPressUpDefense(BaseEventData arg0)
        {
            m_Handler?.OnPressDefense(false);
        }

        private void OnClickMedicineNone()
        {
            GameApp.Entry.Game.Audio.PlaySoundSkillFailed();
            GameApp.Entry.UI.ShowTips("药喝光了", 0.1f);
        }

        void Widget_SkillButton.IHandler.OnClickSkillButton(ESkillType type)
        {
            m_Handler.OnClickSkill(type);
        }

        public void RefreshMedicineCount(int count)
        {
            m_ButtonDrinkMedicine.gameObject.SetActive(count > 0);
            m_ButtonMedicineNone.gameObject.SetActive(count <= 0);
            if (count > 0)
            {
                m_MedicineCount.text = "x" + count;
            }
        }

        private void OnClickDrinkMedicine()
        {
            m_Handler?.OnClickDrinkMedicine();
        }

        public void ShowButtonInteract(ESceneInteractType interactType)
        {
            m_InteractType = interactType;
            m_TextBtnInteract.text = m_InteractType switch
            {
                ESceneInteractType.Portal => "进入",
                ESceneInteractType.Worship => "膜拜",
                ESceneInteractType.Rest => "祈福",
                _ => throw new InvalidOperationException($"Unknown EInteractType:{m_InteractType}"),
            };
            ActiveButtonInteract = true;
        }

        public void HideButtonInteract()
        {
            ActiveButtonInteract = false;
        }

        private void OnClickAttack1()
        {
            m_Handler?.OnClickLightAttack();
        }

        private void OnPressDownDodge(BaseEventData arg0)
        {
            m_Handler?.OnPressDodge(true);
        }

        private void OnPressUpDodge(BaseEventData arg0)
        {
            m_Handler?.OnPressDodge(false);
        }

        /*
        private void OnClickArm()
        {
            m_Handler?.OnClickArm();
        }*/

        void Widget_JoyStick.IHandler.OnStickUsed(Vector2 axis, bool isDragging)
        {
            m_Handler?.OnUseStick(axis, isDragging);
        }

        void Widget_MoveCamera.IHandler.OnCamStickUsed(float x, float y)
        {
            m_Handler?.OnUseCamStick(x, y);
        }

        // event ---------->

        void OnClickInteract()
        {
            m_Handler?.OnClickInteract(m_InteractType);
        }

        /*
        void OnClickJump()
        {
            m_Handler?.OnClickJump();
        }*/

        void OnClickLockOn()
        {
            m_Handler?.OnClickLockOn();
        }

        protected override void Update()
        {
            base.Update();
#if UNITY_EDITOR
            UpdatePCInput();
#endif
        }

#if UNITY_EDITOR
        void UpdatePCInput()
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                m_btnAttackA.OnSubmit(null);
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                m_ButtonHeavyAttack.TriggerEvent(EventTriggerType.PointerDown);
            }

            if (Input.GetKeyUp(KeyCode.L))
            {
                m_ButtonHeavyAttack.TriggerEvent(EventTriggerType.PointerUp);
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                m_ButtonSkill1.m_Button.OnSubmit(null);
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                m_btnLockOn.OnSubmit(null);
            }

            /*
            if (Input.GetKeyDown(KeyCode.R))
            {
                m_btnArm.OnSubmit(null);
            }

            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                m_btnJump.OnSubmit(null);
            }*/

            if (Input.GetKeyDown(KeyCode.Space))
            {
                m_btnRoll.TriggerEvent(EventTriggerType.PointerDown);
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                m_btnRoll.TriggerEvent(EventTriggerType.PointerUp);
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                m_ButtonDefense.TriggerEvent(EventTriggerType.PointerDown);
            }

            if (Input.GetKeyUp(KeyCode.K))
            {
                m_ButtonDefense.TriggerEvent(EventTriggerType.PointerUp);
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (m_ButtonDrinkMedicine.gameObject.activeSelf)
                    m_ButtonDrinkMedicine.OnSubmit(null);
                else if (m_ButtonMedicineNone.gameObject.activeSelf)
                    m_ButtonMedicineNone.OnSubmit(null);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                m_btnInteract.OnSubmit(null);
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                m_ButtonFly.TriggerEvent(EventTriggerType.PointerDown);
            }

            if (Input.GetKeyUp(KeyCode.F))
            {
                m_ButtonFly.TriggerEvent(EventTriggerType.PointerUp);
            }
        }
#endif
    }
}