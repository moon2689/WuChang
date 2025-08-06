using System.Collections;
using System.Collections.Generic;
using CombatEditor;
using Saber.CharacterController;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkillConfig), false)]
public class SkillConfigEditor : EditorBase
{
    private List<bool> m_ListSelectedSkills;

    protected override string TitleString => "技能配置";


    string GetSKillItemTitle(SkillItem item)
    {
        if (item == null)
        {
            return "Skill";
        }

        string strTitle = $"{item.m_ID}";
        if (item.m_AnimStates.Length > 0)
        {
            strTitle += $" {item.m_AnimStates[0].m_Name}";
        }
        else
        {
            strTitle += $" {item.m_SkillType}";
        }

        if (item.m_ChainSkillIDs.Length > 0)
        {
            strTitle += " (->";
            foreach (var chainSkillID in item.m_ChainSkillIDs)
                strTitle += $"{chainSkillID}/";
            strTitle = strTitle.TrimEnd('/');
            strTitle += ")";
        }

        if (item.m_FirstSkillOfCombo)
        {
            strTitle += " ●";
        }

        return strTitle;
    }

    protected override void DrawGUI()
    {
        SkillConfig config = (SkillConfig)base.target;
        config.m_WeaponStyle = (EWeaponStyle)EditorGUILayout.EnumPopup("武器类型：", config.m_WeaponStyle);

        if (m_ListSelectedSkills == null)
        {
            m_ListSelectedSkills = new List<bool>();
            for (int i = 0; i < config.m_SkillItems.Length; i++)
            {
                m_ListSelectedSkills.Add(false);
            }
        }
        else if (m_ListSelectedSkills.Count < config.m_SkillItems.Length)
        {
            for (int i = m_ListSelectedSkills.Count; i < config.m_SkillItems.Length; i++)
                m_ListSelectedSkills.Add(true);
        }

        for (int i = 0; i < config.m_SkillItems.Length; i++)
        {
            SkillItem item = config.m_SkillItems[i];

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("=", GUILayout.Width(20)))
            {
                PaneOptions<SkillItem>(config.m_SkillItems, item, newArray => config.m_SkillItems = newArray);
            }

            GUILayout.Space(10);
            m_ListSelectedSkills[i] =
                EditorGUILayout.Foldout(m_ListSelectedSkills[i], GetSKillItemTitle(item), m_FoldStyle);
            GUILayout.EndHorizontal();

            if (m_ListSelectedSkills[i])
            {
                EditorGUILayout.BeginVertical(m_RootGroupStyle);
                DrawSkillItem(item);
                GUILayout.EndVertical();
            }
        }

        if (StyledButton("新增技能"))
        {
            SkillItem skillItem = new SkillItem()
            {
                m_ID = config.m_SkillItems.Length + 1,
                m_AnimStates = new SkillAnimStateMachine[1],
                CostStrength = 5,
                m_SkillType = ESkillType.LightAttack,
                m_TriggerCondition = ETriggerCondition.InGround,
                UseGravityWhenInAir = false,
                m_FirstSkillOfCombo = false,
                m_ChainSkillIDs = new int[0],
                m_AIPramAttackDistance = new RangedFloat(0.5f, 2f),
            };
            skillItem.m_AnimStates[0] = new SkillAnimStateMachine()
            {
                m_Name = "",
                m_EventData = null,
            };
            config.m_SkillItems = AddElement(config.m_SkillItems, skillItem);
        }

        EditorUtility.SetDirty(config);
    }

    void DrawSkillItem(SkillItem item)
    {
        item.m_ID = EditorGUILayout.IntField("ID：", item.m_ID);
        item.CostStrength = EditorGUILayout.FloatField("消耗体力:", item.CostStrength);

        item.m_SkillType = (ESkillType)EditorGUILayout.EnumPopup("输入键：", item.m_SkillType);
        item.m_TriggerCondition = (ETriggerCondition)EditorGUILayout.EnumPopup("触发条件:", item.m_TriggerCondition);
        if (item.m_TriggerCondition == ETriggerCondition.InAir)
        {
            item.UseGravityWhenInAir = EditorGUILayout.Toggle("空中开启重力：", item.UseGravityWhenInAir);
        }

        if (item.m_SkillType == ESkillType.MoveThenAttack)
        {
            item.m_AttackTriggerDistance = EditorGUILayout.FloatField("触发攻击的距离:", item.m_AttackTriggerDistance);
        }

        item.m_FirstSkillOfCombo = EditorGUILayout.Toggle("起手技能:", item.m_FirstSkillOfCombo);
        if (item.m_FirstSkillOfCombo)
        {
            item.m_CostPower = EditorGUILayout.IntField("消耗能量:", item.m_CostPower);
            item.m_CDSeconds = EditorGUILayout.FloatField("冷却时间（秒）:", item.m_CDSeconds);
        }

        // 动画
        GUILayout.BeginHorizontal();
        GUILayout.Label($"播放动画({item.m_AnimStates.Length})：", GUILayout.Width(100));
        if (GUILayout.Button("+", GUILayout.Width(20)))
        {
            SkillAnimStateMachine animObj = new SkillAnimStateMachine()
            {
                m_Name = "",
                EventWithEffects = null,
            };
            item.m_AnimStates = AddElement(item.m_AnimStates, animObj);
        }

        GUILayout.EndHorizontal();


        GUILayout.BeginVertical(m_SubGroupStyle);
        for (int i = 0; i < item.m_AnimStates.Length; i++)
        {
            var anim = item.m_AnimStates[i];
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                item.m_AnimStates = RemoveElement(item.m_AnimStates, item.m_AnimStates[i]);
            }

            GUILayout.Label(i + "：");
            GUILayout.EndHorizontal();

            anim.m_Name = EditorGUILayout.TextField("动画名：", anim.m_Name);
            anim.m_EventData = (AbilityScriptableObject)EditorGUILayout.ObjectField("事件:", anim.m_EventData,
                typeof(AbilityScriptableObject), false);
        }

        GUILayout.EndHorizontal();

        // 连招
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"连招技能ID({item.m_ChainSkillIDs.Length})：", GUILayout.Width(100));
        if (GUILayout.Button("+", GUILayout.Width(20)))
        {
            item.m_ChainSkillIDs = AddElement(item.m_ChainSkillIDs, 0);
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginVertical(m_SubGroupStyle);
        for (int i = 0; i < item.m_ChainSkillIDs.Length; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(80);
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                item.m_ChainSkillIDs = RemoveElementByIndex(item.m_ChainSkillIDs, i);
                continue;
            }

            GUILayout.Label($"{i}:", GUILayout.Width(30));
            item.m_ChainSkillIDs[i] = EditorGUILayout.IntField(item.m_ChainSkillIDs[i], GUILayout.Width(100));
            GUILayout.EndHorizontal();
        }

        GUILayout.EndHorizontal();

        //
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("AI攻击范围：", GUILayout.Width(100));
        item.m_AIPramAttackDistance.minValue =
            EditorGUILayout.FloatField(item.m_AIPramAttackDistance.minValue, GUILayout.Width(100));
        EditorGUILayout.LabelField("-->", GUILayout.Width(30));
        item.m_AIPramAttackDistance.maxValue =
            EditorGUILayout.FloatField(item.m_AIPramAttackDistance.maxValue, GUILayout.Width(100));
        GUILayout.EndHorizontal();
    }
}