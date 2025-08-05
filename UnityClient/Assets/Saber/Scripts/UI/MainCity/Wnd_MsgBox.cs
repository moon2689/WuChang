using Saber;
using UnityEngine;
using UnityEngine.UI;
using System;
using Saber.Frame;


namespace Saber.UI
{
    public class Wnd_MsgBox : WndBase
    {
        [SerializeField] Button m_btnClose, m_btnConfirm, m_btnCancel;
        [SerializeField] Text m_text;

        Action m_onConfirm, m_onCancel;


        string Messenge
        {
            set => m_text.text = value;
        }


        protected override bool PauseGame => true;

        protected override void OnAwake()
        {
            base.OnAwake();
            m_btnClose.onClick.AddListener(OnClickClose);
            m_btnConfirm.onClick.AddListener(OnClickConfirm);
            m_btnCancel.onClick.AddListener(OnClickCancel);
            GameApp.Entry.Game.Audio.Play2DSound("OpenWnd");
        }

        void OnClickClose()
        {
            Destroy();
        }

        void OnClickConfirm()
        {
            GameApp.Entry.Game.Audio.PlayCommonClick();
            Destroy();
            if (m_onConfirm != null)
            {
                m_onConfirm();
                m_onConfirm = null;
            }
        }

        void OnClickCancel()
        {
            GameApp.Entry.Game.Audio.PlayCommonClick();
            Destroy();
            if (m_onCancel != null)
            {
                m_onCancel();
                m_onCancel = null;
            }
        }

        public void Reset(string msg, Action onConfirm, Action onCancel)
        {
            Messenge = msg;

            m_onConfirm = onConfirm;
            m_onCancel = onCancel;
        }

        public void Reset(string msg, Action onConfirm)
        {
            Messenge = msg;
            m_onConfirm = onConfirm;
            m_onCancel = null;

            m_btnCancel.gameObject.SetActive(false);

            RectTransform rt = m_btnConfirm.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 40);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GameApp.Entry.Game.Audio.Play2DSound("CloseWnd");
        }
    }
}