using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;
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
        private StringBuilder m_SB = new();

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
                    if (skillItem.m_Active && skillItem.m_FirstSkillOfCombo)
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
        }

        void AddComboSkills(SkillItem skillItem)
        {
            if (skillItem.m_ChainSkills.Length > 0)
            {
                int ranChainIndex = UnityEngine.Random.Range(0, skillItem.m_ChainSkills.Length);
                SkillItem chainSkill = Actor.CMelee.SkillConfig.GetSkillItemByID(skillItem.m_ChainSkills[ranChainIndex].m_SkillID);
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
                var curSkill = m_CurSkill[m_ComboSkillIndex];
                bool succeed = Actor.TryTriggerSkill(curSkill);
                if (succeed)
                {
                    m_SB.Length = 0;
                    m_SB.AppendLine($"序号\t技能名");
                    for (int i = 0; i < m_CurSkill.Count; i++)
                    {
                        var item = m_CurSkill[i];
                        string endStr = item == curSkill ? " √" : "";
                        m_SB.AppendLine($"{i + 1}\t{item.m_SkillName} ({item.m_ID}){endStr}");
                    }

                    m_TestSkillGUI.SetMsg(m_SB.ToString());

                    ++m_ComboSkillIndex;
                    if (m_ComboSkillIndex >= m_CurSkill.Count)
                    {
                        SetCurSkill(m_SkillIndex);
                    }
                }
                else
                {
                    if (Actor.CurrentStateType != EStateType.Skill)
                    {
                        SetCurSkill(m_SkillIndex);
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