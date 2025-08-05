using UnityEngine;
using UnityEngine.EventSystems;

namespace Saber.UI
{
    public class Widget_MoveCamera : WidgetBase, IPointerUpHandler, IPointerDownHandler
    {
        private const float
            k_SensitivityX = 0.05f,
            k_SensitivityY = 0.025f;

        [SerializeField] RectTransform touchPointer;

        IHandler m_handler;
        bool m_isInputingKeyboard;


        public Vector2 TouchDirection { get; protected set; }
        public Vector2 PrevioustouchPosition { get; protected set; }
        public int CurrentPointerID { get; protected set; }
        public bool IsPressed { get; protected set; }


        public interface IHandler
        {
            void OnCamStickUsed(float x, float y);
        }


        public void Init(IHandler handler)
        {
            m_handler = handler;
        }

        protected override void Start()
        {
            base.Start();
            if (touchPointer)
                touchPointer.gameObject.SetActive(false);
        }

        protected override void Update()
        {
            base.Update();
            HandleTouchDirection();

#if UNITY_EDITOR
            UpdateCamStick();
#endif
        }

        void HandleTouchDirection()
        {
            if (IsPressed)
            {
                bool valid = Input.touchCount < 2 || Widget_JoyStick.IsDragging;
                if (!valid)
                {
                    EndPress();
                    return;
                }

                if (CurrentPointerID >= 0 && CurrentPointerID < Input.touches.Length)
                {
                    Vector2 touchPos = Input.touches[CurrentPointerID].position;
                    TouchDirection = touchPos - PrevioustouchPosition;
                    if (touchPointer)
                        touchPointer.position = touchPos;
                    PrevioustouchPosition = Input.touches[CurrentPointerID].position;
                }
                else
                {
                    Vector2 touchPos = Input.mousePosition;
                    TouchDirection = touchPos - PrevioustouchPosition;
                    if (touchPointer)
                        touchPointer.position = touchPos;
                    PrevioustouchPosition = Input.mousePosition;
                }

                UpdateVirtualAxes(TouchDirection.x * k_SensitivityX, TouchDirection.y * k_SensitivityY);
            }
        }

        void UpdateVirtualAxes(float x, float y)
        {
            m_handler.OnCamStickUsed(x, y);
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData data)
        {
            if (Input.touchCount > 2)
            {
                EndPress();
                return;
            }

            if (touchPointer)
            {
                touchPointer.position = data.position;
                touchPointer.gameObject.SetActive(true);
            }

            IsPressed = true;
            CurrentPointerID = data.pointerId;
            PrevioustouchPosition = data.position;
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData data)
        {
            EndPress();
        }

        void EndPress()
        {
            if (touchPointer)
            {
                touchPointer.gameObject.SetActive(false);
            }

            IsPressed = false;
            TouchDirection = Vector2.zero;
            UpdateVirtualAxes(0, 0);
        }

#if UNITY_EDITOR
        static float GetKeyValue(KeyCode key)
        {
            return Input.GetKey(key) ? 1 : 0;
        }

        void UpdateCamStick()
        {
            float targetX = GetKeyValue(KeyCode.E) - GetKeyValue(KeyCode.Q);
            float targetY = GetKeyValue(KeyCode.X) - GetKeyValue(KeyCode.Z);
            if (targetX != 0 || targetY != 0)
            {
                m_isInputingKeyboard = true;
                UpdateVirtualAxes(targetX * 0.2f, targetY * 0.1f);
            }
            else if (m_isInputingKeyboard)
            {
                m_isInputingKeyboard = false;
                UpdateVirtualAxes(0, 0);
            }
        }
#endif
    }
}