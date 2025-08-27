using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Saber.CharacterController;
using Saber.Frame;
using UnityEngine.Rendering.UI;

namespace Saber.AI
{
    public class TestSkill : EnemyAIBase
    {
        private TestSkillGUI m_TestSkillGUI;
        private int m_SkillIndex;
        private SkillItem m_CurSkill;

        protected override void OnStart()
        {
            base.OnStart();
            SwitchCoroutine(SearchEnemy());
            if (m_TestSkillGUI == null)
            {
                m_TestSkillGUI = TestSkillGUI.Create();
                m_TestSkillGUI.OnClickPrevious = SwitchPreviousSkill;
                m_TestSkillGUI.OnClickNext = SwitchNextSkill;
            }

            SetCurSkill(0);
        }

        private void SwitchNextSkill()
        {
            SetCurSkill(m_SkillIndex + 1);
        }

        private void SwitchPreviousSkill()
        {
            SetCurSkill(m_SkillIndex - 1);
        }

        void SetCurSkill(int index)
        {
            m_SkillIndex = index;

            int[] skillIDs = GameApp.Entry.Config.TestGame.TestingSkillID;
            if (m_SkillIndex >= skillIDs.Length)
            {
                m_SkillIndex = 0;
            }
            else if (m_SkillIndex < 0)
            {
                m_SkillIndex = skillIDs.Length - 1;
            }

            int skillID = skillIDs[m_SkillIndex];
            m_CurSkill = Actor.CMelee.SkillConfig.GetSkillItemByID(skillID);

            var animState = m_CurSkill.m_AnimStates.FirstOrDefault();
            if (animState != null)
            {
                m_TestSkillGUI.SetMsg($"{skillID} {animState.m_Name}");
            }
        }

        protected override void OnFoundEnemy(EFoundEnemyType foundType)
        {
            SwitchCoroutine(TestSkillItor());
        }

        IEnumerator TestSkillItor()
        {
            while (true)
            {
                bool succeed = Actor.TryTriggerSkill(m_CurSkill);
                if (succeed)
                {
                    yield return new WaitForSeconds(GameApp.Entry.Config.TestGame.TriggerSkillInterval);
                }
                else
                {
                    yield return null;
                }
            }
        }
    }

    public class TestSkillGUI : MonoBehaviour
    {
        public Action OnClickPrevious;
        public Action OnClickNext;

        private GUIStyle m_StyleButton;
        private GUIStyle m_LabelStyle;
        private string m_Msg;

        public static TestSkillGUI Create()
        {
            GameObject go = new(nameof(TestSkillGUI));
            return go.AddComponent<TestSkillGUI>();
        }

        private void Awake()
        {
            m_LabelStyle = new()
            {
                fontSize = 40,
                normal =
                {
                    textColor = Color.green
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

            if (GUI.Button(new Rect(0, 50, 180, 90), "上一个", m_StyleButton))
            {
                OnClickPrevious?.Invoke();
            }

            if (GUI.Button(new Rect(190, 50, 180, 90), "下一个", m_StyleButton))
            {
                OnClickNext?.Invoke();
            }

            if (!string.IsNullOrEmpty(m_Msg))
            {
                GUI.Label(new Rect(0, 150, 300, 40), m_Msg, m_LabelStyle);
            }
        }

        public void SetMsg(string msg)
        {
            m_Msg = msg;
        }
    }
}