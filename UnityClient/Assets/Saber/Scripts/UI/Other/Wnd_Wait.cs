using Saber;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using Saber.Director;
using Saber.Frame;
using Saber.World;
using UnityEngine.PlayerLoop;


namespace Saber.UI
{
    public class Wnd_Wait : WndBase
    {
        [SerializeField] private Button m_BtnQuit;
        [SerializeField] private Slider m_Slider;
        [SerializeField] private Text m_TextCurTime;

        private Coroutine m_Coroutine;


        protected override void OnAwake()
        {
            base.OnAwake();
            m_BtnQuit.onClick.AddListener(OnClickQuit);
            m_Coroutine = GameApp.Entry.Unity.StartCoroutine(SetTimeItor());
            m_TextCurTime.text = "";
            m_Slider.value = 0;
        }

        IEnumerator SetTimeItor()
        {
            BigWorld dirWorld = GameApp.Entry.Game.World;
            if (dirWorld == null)
            {
                yield break;
            }

            /*
            for (int i = 0; i < 24; i++)
            {
                dirWorld.Timeline += 1;
                m_Slider.value = (i + 1) / 24f;
                m_TextCurTime.text = $"{dirWorld.Date.x}-{dirWorld.Date.y}-{dirWorld.Date.z} {dirWorld.Timeline:N0}h";
                yield return new WaitForSeconds(0.3f);
            }
            */

            Destroy();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GameApp.Entry.Unity.StopCoroutine(m_Coroutine);
        }

        void OnClickQuit()
        {
            GameApp.Entry.Game.Audio.PlayCommonClick();
            Destroy();
        }
    }
}