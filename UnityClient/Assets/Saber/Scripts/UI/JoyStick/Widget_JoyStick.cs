using UnityEngine;
using UnityEngine.EventSystems;

namespace Saber.UI
{
    public class Widget_JoyStick : WidgetBase, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerDownHandler,
        IPointerUpHandler
    {
        public interface IHandler
        {
            void OnStickUsed(Vector2 axis, bool isDragging);
        }

        [SerializeField, Tooltip("The handling area that the handle is allowed to be moved in.")]
        private RectTransform m_HandlingArea;

        [SerializeField, Tooltip("The child graphic that will be moved around.")]
        private RectTransform m_Handle;

        [SerializeField, Tooltip("How close to the center that the axis will be output as 0")]
        private float m_DeadZone = 0.1f;

        [SerializeField, Tooltip("How fast the joystick will go back to the center")]
        private float m_Spring = 25f;

        [SerializeField] private Vector2 m_Axis;

        IHandler m_Handler;

        public static bool IsDragging { get; private set; }

        public Vector2 JoystickAxis
        {
            get
            {
                Vector2 outputPoint = this.m_Axis.magnitude > this.m_DeadZone ? this.m_Axis : Vector2.zero;
                float magnitude = outputPoint.magnitude;

                return outputPoint;
            }
            set { this.SetAxis(value); }
        }


        public void Init(IHandler handler)
        {
            m_Handler = handler;
        }

        private void SetAxis(Vector2 axis)
        {
            this.m_Axis = Vector2.ClampMagnitude(axis, 1);
            this.UpdateHandle();
        }

        void LateUpdate()
        {
            if (this.isActiveAndEnabled && !IsDragging)
            {
                if (this.m_Axis != Vector2.zero)
                {
                    Vector2 newAxis = this.m_Axis - (this.m_Axis * Time.unscaledDeltaTime * this.m_Spring);

                    if (newAxis.sqrMagnitude <= .0001f)
                        newAxis = Vector2.zero;

                    this.SetAxis(newAxis);
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Vector2 newAxis = this.m_HandlingArea.InverseTransformPoint(eventData.position);
            newAxis.x /= this.m_HandlingArea.sizeDelta.x * 0.5f;
            newAxis.y /= this.m_HandlingArea.sizeDelta.y * 0.5f;

            this.SetAxis(newAxis);
            IsDragging = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            IsDragging = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 axis = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(this.m_HandlingArea, eventData.position,
                eventData.pressEventCamera, out axis);

            axis -= this.m_HandlingArea.rect.center;
            axis.x /= this.m_HandlingArea.sizeDelta.x * 0.5f;
            axis.y /= this.m_HandlingArea.sizeDelta.y * 0.5f;

            this.SetAxis(axis);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            IsDragging = true;
            OnDrag(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsDragging = false;
        }

        private void UpdateHandle()
        {
            this.m_Handle.anchoredPosition = new Vector2(this.m_Axis.x * this.m_HandlingArea.sizeDelta.x * 0.5f,
                this.m_Axis.y * this.m_HandlingArea.sizeDelta.y * 0.5f);
            m_Handler?.OnStickUsed(JoystickAxis, IsDragging);
        }


#if UNITY_EDITOR
        private bool m_ToggleWalk;
        bool m_IsInputingKeyboard;

        static float GetKeyValue(KeyCode key)
        {
            return Input.GetKey(key) ? 1 : 0;
        }

        protected override void Update()
        {
            base.Update();
            float targetX = GetKeyValue(KeyCode.D) - GetKeyValue(KeyCode.A);
            if (targetX == 0)
                targetX = GetKeyValue(KeyCode.RightArrow) - GetKeyValue(KeyCode.LeftArrow);
            float targetY = GetKeyValue(KeyCode.W) - GetKeyValue(KeyCode.S);
            if (targetY == 0)
                targetY = GetKeyValue(KeyCode.UpArrow) - GetKeyValue(KeyCode.DownArrow);

            GameHelper.FixStick(targetX, targetY, out targetX, out targetY);

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                m_ToggleWalk = !m_ToggleWalk;
            }

            if (m_ToggleWalk)
            {
                targetX /= 2;
                targetY /= 2;
            }

            if (targetX != 0 || targetY != 0)
            {
                m_IsInputingKeyboard = true;
                SetAxis(new Vector2(targetX, targetY));
                IsDragging = true;
            }
            else if (m_IsInputingKeyboard)
            {
                m_IsInputingKeyboard = false;
                IsDragging = false;
            }
        }
#endif
    }
}