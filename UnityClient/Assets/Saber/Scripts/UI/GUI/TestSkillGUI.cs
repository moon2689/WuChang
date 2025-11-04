using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;
using Saber.CharacterController;
using Saber.Frame;
using UnityEngine.Rendering.UI;


namespace Saber.UI
{
    public class TestSkillGUI : MonoBehaviour
    {
        public Action OnClickPrevious;
        public Action OnClickNext;

        private GUIStyle m_StyleButton;
        private GUIStyle m_LabelStyle;
        private string m_Msg;

        public static TestSkillGUI Create()
        {
            GameObject root = GameObject.Find("GUI");
            if (root == null)
            {
                root = new GameObject("GUI");
                DontDestroyOnLoad(root);
            }

            GameObject go = new(nameof(TestSkillGUI));
            go.transform.SetParent(root.transform);
            return go.AddComponent<TestSkillGUI>();
        }

        private void Awake()
        {
            m_LabelStyle = new()
            {
                fontSize = 40,
                normal =
                {
                    textColor = Color.white
                }
            };
        }

        void OnGUI()
        {
            if (m_StyleButton == null)
            {
                m_StyleButton = new GUIStyle(GUI.skin.button);
                m_StyleButton.fontSize = 40;
            }

            if (GUI.Button(new Rect(200, 0, 180, 90), "上一个", m_StyleButton))
            {
                OnClickPrevious?.Invoke();
            }

            if (GUI.Button(new Rect(390, 0, 180, 90), "下一个", m_StyleButton))
            {
                OnClickNext?.Invoke();
            }

            if (!string.IsNullOrEmpty(m_Msg))
            {
                GUI.Box(new Rect(0, 100, 500, 500), "");
                GUI.Label(new Rect(0, 110, 500, 40), m_Msg, m_LabelStyle);
            }
        }

        public void SetMsg(string msg)
        {
            m_Msg = msg;
        }
    }
}