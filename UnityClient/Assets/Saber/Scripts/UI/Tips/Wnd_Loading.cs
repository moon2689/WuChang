using DuloGames.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Saber.UI
{
    public class Wnd_Loading : WndBase
    {
        [SerializeField] UIProgressBar m_ProgressBar;
        [SerializeField] private Text m_PercentText;

        public override EWindowMode WindowMode => EWindowMode.Exclusive;
        

        public float Percent
        {
            set
            {
                m_ProgressBar.fillAmount = value / 100f;
                m_ProgressBar.gameObject.SetActive(value > 0);
                m_PercentText.text = value.ToString("0") + "%";
            }
        }
        

        protected override void OnAwake()
        {
            base.OnAwake();
            m_ProgressBar.gameObject.SetActive(false);
        }
    }
}
