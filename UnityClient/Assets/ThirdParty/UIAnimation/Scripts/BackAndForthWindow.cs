using System;
using UIAnimation.Actions;
using UnityEngine;

public class BackAndForthWindow : MonoBehaviour
{
    [SerializeField]
    ActionRunner m_showAnim;
    [SerializeField]
    ActionRunner m_hideAnim;
    [SerializeField]
    bool m_showOnEnable = true;

    ActionRunner m_animToPlay;
    Action m_onShow;
    Action m_onHide;

    public Action OnShow
    {
        set { m_onShow = value; }
    }

    public Action OnHide
    {
        set { m_onHide = value; }
    }

    void Awake()
    {
        if (m_showAnim)
            m_showAnim.OnFinishedAllActionsEvent += OnShowFinished;
        if (m_hideAnim)
            m_hideAnim.OnFinishedAllActionsEvent += OnHideFinished;
    }

    void OnShowFinished()
    {
        if (m_onShow != null)
            m_onShow();
    }

    void OnHideFinished()
    {
        if (m_onHide != null)
            m_onHide();
    }

    void OnEnable()
    {
        if (m_showOnEnable)
            Show();
    }

    void OnDisable()
    {
        if (m_showAnim)
            m_showAnim.Stop();
        if (m_hideAnim)
            m_hideAnim.Stop();
    }

    public void Show()
    {
        m_animToPlay = m_showAnim;
    }

    public void Hide()
    {
        m_animToPlay = m_hideAnim;

        if (m_hideAnim == null)
            m_showAnim.Stop();
    }

    void Update()
    {
        if (m_animToPlay)
        {
            if (m_animToPlay.Run())
                m_animToPlay = null;
        }
    }
}
