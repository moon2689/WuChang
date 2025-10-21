using SakashoUISystem;
using UIAnimation.Actions;
using UnityEngine;
using UnityEngine.UI;

public class TweenImageFillAmount : TweenActionBase
{
    [SerializeField]
    float m_toFillAmount = 1;

    Image m_Image;

    float m_fromFillAmount;
    float m_originalFillAmount;

    protected override void Awake()
    {
        base.Awake();
        m_Image = GetComponent<Image>();
        m_originalFillAmount = m_Image.fillAmount;
    }

    public override void ResetStatus()
    {
        base.ResetStatus();
        m_Image.fillAmount = m_originalFillAmount;
    }

    public override void Prepare()
    {
        base.Prepare();
        m_fromFillAmount = m_Image.fillAmount;
    }

    #region implemented abstract members of TweenerBase
    protected override void Lerp(float normalizedTime)
    {
        m_Image.fillAmount = Mathematics.LerpFloat(m_fromFillAmount, m_toFillAmount, normalizedTime);
    }
    #endregion

    protected override void OnActionIsDone()
    {
        base.OnActionIsDone();
        m_Image.fillAmount = m_toFillAmount;
    }
}
