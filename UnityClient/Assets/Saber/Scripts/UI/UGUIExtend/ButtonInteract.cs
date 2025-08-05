using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonInteract : MonoBehaviour //, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float m_ScaleValue = 1.2f;
    [SerializeField] private bool m_ScaleWhenClick = true;
    [SerializeField] private bool m_ScaleWhenPress;
    private Button m_Button;

    private void Awake()
    {
        m_Button = GetComponent<Button>();
        if (m_ScaleWhenClick)
        {
            m_Button.onClick.AddListener(OnClickButton);
        }

        if (m_ScaleWhenPress)
        {
            m_Button.AddEvent(EventTriggerType.PointerDown, OnPressDown);
            m_Button.AddEvent(EventTriggerType.PointerUp, OnPressUp);
        }
    }

    private void OnPressUp(BaseEventData arg0)
    {
        RevertScale();
    }

    private void OnPressDown(BaseEventData arg0)
    {
        ToScale();
    }

    private async void OnClickButton()
    {
        ToScale();
        await Task.Delay(100);
        RevertScale();
    }

    void ToScale()
    {
        transform.localScale = Vector3.one * m_ScaleValue;
    }

    void RevertScale()
    {
        transform.localScale = Vector3.one;
    }
}