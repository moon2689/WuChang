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
            void OnClickLightAttack();
            void OnPressHeavyAttack(bool press);
            void OnPressDodge(bool value);
            void OnPressLockOn(bool value);
            void OnPressDefense(bool value);

            void OnClickInteract(ESceneInteractType interactType);
            void OnClickDrinkMedicine();
            void OnClickSkill(ESkillType type);
            void OnClickBag();
        }


        [SerializeField] private Widget_JoyStick m_joystick;
        [SerializeField] private Widget_MoveCamera m_moveCamera;

        [SerializeField] private Button m_btnAttackA,
            m_ButtonHeavyAttack,
            m_btnRoll,
            m_btnLockOn,
            m_btnInteract,
            m_ButtonDrinkMedicine,
            m_ButtonMedicineNone,
            m_ButtonDefense,
            m_ButtonBag;

        [SerializeField] private Widget_SkillButton m_ButtonSkill1,
            m_ButtonSkill2,
            m_ButtonSkill3;

        [SerializeField] private GameObject m_parentButtons;
        [SerializeField] private Text m_TextBtnInteract;
        [SerializeField] private Text m_MedicineCount;
        [SerializeField] private GameObject m_Sticks;

        public IHandler Handler { get; set; }
        private ESceneInteractType m_InteractType;


        bool ActiveButtonInteract
        {
            set => m_btnInteract.gameObject.SetActive(value);
        }

        public bool ActiveSticks
        {
            set
            {
                m_Sticks.SetActive(value);
                //Debug.Log($"ActiveSticks:{value}");
            }
        }

        public void Default()
        {
            ActiveButtonInteract = false;
            ActiveSticks = true;
        }

        protected override void OnAwake()
        {
            base.OnAwake();

            m_joystick.Init(this);
            m_moveCamera.Init(this);

            m_btnAttackA.AddEvent(EventTriggerType.PointerDown, OnClickAttack1);
            m_ButtonHeavyAttack.AddEvent(EventTriggerType.PointerDown, OnPressDownHeavyAttack);
            m_ButtonHeavyAttack.AddEvent(EventTriggerType.PointerUp, OnPressUpHeavyAttack);
            m_btnInteract.onClick.AddListener(OnClickInteract);
            m_ButtonDrinkMedicine.onClick.AddListener(OnClickDrinkMedicine);
            m_ButtonMedicineNone.onClick.AddListener(OnClickMedicineNone);
            m_ButtonBag.onClick.AddListener(OnClickBag);

            m_btnRoll.AddEvent(EventTriggerType.PointerDown, OnPressDownDodge);
            m_btnRoll.AddEvent(EventTriggerType.PointerUp, OnPressUpDodge);

            m_ButtonDefense.AddEvent(EventTriggerType.PointerDown, OnPressDownDefense);
            m_ButtonDefense.AddEvent(EventTriggerType.PointerUp, OnPressUpDefense);

            m_btnLockOn.AddEvent(EventTriggerType.PointerDown, OnPressDownLockOn);
            m_btnLockOn.AddEvent(EventTriggerType.PointerUp, OnPressUpLockOn);

            ActiveButtonInteract = false;

            m_ButtonSkill1.Init(ESkillType.Skill1, this);
            m_ButtonSkill2.Init(ESkillType.FaShu2, this);
            //m_ButtonSkill3.Init(ESkillType.Skill3, this);
        }

        private void OnClickBag()
        {
            Handler?.OnClickBag();
        }

        void OnPressDownHeavyAttack(BaseEventData arg0)
        {
            Handler?.OnPressHeavyAttack(true);
        }

        void OnPressUpHeavyAttack(BaseEventData arg0)
        {
            Handler?.OnPressHeavyAttack(false);
        }

        void OnPressDownDefense(BaseEventData arg0)
        {
            Handler?.OnPressDefense(true);
        }

        void OnPressUpDefense(BaseEventData arg0)
        {
            Handler?.OnPressDefense(false);
        }

        private void OnClickMedicineNone()
        {
            Handler?.OnClickDrinkMedicine();
            GameApp.Entry.Game.Audio.PlaySoundSkillFailed();
            GameApp.Entry.UI.ShowTips("药喝光了", 0.1f);
        }

        void Widget_SkillButton.IHandler.OnClickSkillButton(ESkillType type)
        {
            Handler.OnClickSkill(type);
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
            Handler?.OnClickDrinkMedicine();
        }

        public void ShowButtonInteract(ESceneInteractType interactType)
        {
            m_InteractType = interactType;
            m_TextBtnInteract.text = m_InteractType switch
            {
                ESceneInteractType.Portal => "进入",
                ESceneInteractType.ActiveShenKan => "奉血",
                ESceneInteractType.Rest => "拜命",
                _ => throw new InvalidOperationException($"Unknown EInteractType:{m_InteractType}"),
            };
            ActiveButtonInteract = true;
        }

        public void HideButtonInteract()
        {
            ActiveButtonInteract = false;
        }

        private void OnClickAttack1(BaseEventData arg0)
        {
            Handler?.OnClickLightAttack();
        }

        private void OnPressDownDodge(BaseEventData arg0)
        {
            Handler?.OnPressDodge(true);
        }

        private void OnPressUpDodge(BaseEventData arg0)
        {
            Handler?.OnPressDodge(false);
        }

        /*
        private void OnClickArm()
        {
            m_Handler?.OnClickArm();
        }*/

        void Widget_JoyStick.IHandler.OnStickUsed(Vector2 axis, bool isDragging)
        {
            Handler?.OnUseStick(axis, isDragging);
        }

        void Widget_MoveCamera.IHandler.OnCamStickUsed(float x, float y)
        {
            Handler?.OnUseCamStick(x, y);
        }

        // event ---------->

        void OnClickInteract()
        {
            Handler?.OnClickInteract(m_InteractType);
        }

        /*
        void OnClickJump()
        {
            m_Handler?.OnClickJump();
        }*/

        private void OnPressDownLockOn(BaseEventData arg0)
        {
            Handler?.OnPressLockOn(true);
        }

        private void OnPressUpLockOn(BaseEventData arg0)
        {
            Handler?.OnPressLockOn(false);
        }


#if UNITY_EDITOR
        protected override void Update()
        {
            base.Update();
            UpdatePCInput();
        }

        void UpdatePCInput()
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                //m_btnAttackA.OnSubmit(null);
                m_btnAttackA.TriggerEvent(EventTriggerType.PointerDown);
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                m_ButtonHeavyAttack.TriggerEvent(EventTriggerType.PointerDown);
            }

            if (Input.GetKeyUp(KeyCode.L))
            {
                m_ButtonHeavyAttack.TriggerEvent(EventTriggerType.PointerUp);
            }

            if (Input.GetKeyDown(KeyCode.U))
            {
                m_ButtonSkill1.m_Button.TriggerEvent(EventTriggerType.PointerDown);
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                m_ButtonSkill2.m_Button.TriggerEvent(EventTriggerType.PointerDown);
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                m_btnLockOn.TriggerEvent(EventTriggerType.PointerDown);
            }

            if (Input.GetKeyUp(KeyCode.Tab))
            {
                m_btnLockOn.TriggerEvent(EventTriggerType.PointerUp);
            }

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

            if (Input.GetKeyDown(KeyCode.H))
            {
                if (m_ButtonDrinkMedicine.gameObject.activeSelf)
                    m_ButtonDrinkMedicine.OnSubmit(null);
                else if (m_ButtonMedicineNone.gameObject.activeSelf)
                    m_ButtonMedicineNone.OnSubmit(null);
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                m_btnInteract.OnSubmit(null);
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                m_ButtonBag.OnSubmit(null);
            }
        }
#endif
    }
}