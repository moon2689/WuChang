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
        private List<SkillItem> m_CurSkill = new();
        private int m_ComboSkillIndex;
        private List<int> m_SkillIDs;

        protected override void OnStart()
        {
            base.OnStart();
            ToSearchEnemy();
            if (m_TestSkillGUI == null)
            {
                m_TestSkillGUI = TestSkillGUI.Create();
                m_TestSkillGUI.OnClickPrevious = SwitchPreviousSkill;
                m_TestSkillGUI.OnClickNext = SwitchNextSkill;
            }

            m_SkillIDs = new(GameApp.Entry.Config.TestGame.TestingSkillID);
            if (m_SkillIDs.Count == 0)
            {
                foreach (var skillItem in Actor.CMelee.SkillConfig.m_SkillItems)
                {
                    if (skillItem.m_FirstSkillOfCombo)
                    {
                        m_SkillIDs.Add(skillItem.m_ID);
                    }
                }
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

            if (m_SkillIndex >= m_SkillIDs.Count)
            {
                m_SkillIndex = 0;
            }
            else if (m_SkillIndex < 0)
            {
                m_SkillIndex = m_SkillIDs.Count - 1;
            }

            int skillID = m_SkillIDs[m_SkillIndex];
            SkillItem firstSkill = Actor.CMelee.SkillConfig.GetSkillItemByID(skillID);
            m_CurSkill.Clear();
            m_ComboSkillIndex = 0;
            if (firstSkill.m_FirstSkillOfCombo)
            {
                m_CurSkill.Add(firstSkill);
                AddComboSkills(firstSkill);
            }

            if (m_CurSkill.Count > 0)
            {
                var animState = m_CurSkill[0].m_AnimStates.FirstOrDefault();
                if (animState != null)
                    m_TestSkillGUI.SetMsg($"{skillID} {animState.m_Name}");
                else
                    m_TestSkillGUI.SetMsg(null);
            }
            else
            {
                m_TestSkillGUI.SetMsg(null);
            }
        }

        void AddComboSkills(SkillItem skillItem)
        {
            if (skillItem.m_ChainSkills.Length > 0)
            {
                SkillItem chainSkill = Actor.CMelee.SkillConfig.GetSkillItemByID(skillItem.m_ChainSkills[0].m_SkillID);
                m_CurSkill.Add(chainSkill);
                AddComboSkills(chainSkill);
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
                bool succeed = Actor.TryTriggerSkill(m_CurSkill[m_ComboSkillIndex]);
                if (succeed)
                {
                    ++m_ComboSkillIndex;
                    if (m_ComboSkillIndex >= m_CurSkill.Count)
                    {
                        m_ComboSkillIndex = 0;
                    }
                }
                else
                {
                    if (Actor.CurrentStateType != EStateType.Skill)
                    {
                        m_ComboSkillIndex = 0;
                    }
                }

                yield return null;
            }
        }

        public override void Release()
        {
            base.Release();
            if (m_TestSkillGUI)
            {
                GameObject.Destroy(m_TestSkillGUI.gameObject);
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