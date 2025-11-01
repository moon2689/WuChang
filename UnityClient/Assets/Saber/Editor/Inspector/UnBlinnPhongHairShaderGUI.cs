using UnityEngine;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine.Rendering;

public class UnBlinnPhongHairShaderGUI : ShaderGUI
{
    public enum ELightMode
    {
        无光,
        半兰伯特,
        渐变纹理,
    }

    // GUI 元素
    private MaterialEditor m_MaterialEditor;
    private MaterialProperty[] m_Properties;
    private Material m_TargetMaterial;

    // 属性引用
    private MaterialProperty m_cutoffProp;

    private ELightMode m_CurLightMode;


    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        m_MaterialEditor = materialEditor;
        m_Properties = properties;
        m_TargetMaterial = materialEditor.target as Material;

        FindProperties();
        RenderShaderGUI();
    }

    private void FindProperties()
    {
        m_cutoffProp = FindProperty("_Cutoff", m_Properties);
    }

    private void RenderShaderGUI()
    {
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("注意：此shader专门用于头发，会开启2个PASS，性能消耗较高", EditorStyles.linkLabel);

        // 输入
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("输入", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            m_MaterialEditor.ShaderProperty(FindProperty("_BaseMap", m_Properties), "基础图");
            m_MaterialEditor.ShaderProperty(FindProperty("_BaseColor", m_Properties), "基础颜色");
            m_MaterialEditor.ShaderProperty(m_cutoffProp, "裁剪值");
            m_CurLightMode = GetCurrentLightMode();

            ELightMode newLightMode = (ELightMode)EditorGUILayout.EnumPopup("光照模式", m_CurLightMode);
            if (newLightMode != m_CurLightMode)
            {
                SetLightMode(newLightMode);
            }

            if (m_CurLightMode == ELightMode.渐变纹理)
            {
                m_MaterialEditor.ShaderProperty(FindProperty("_RampMap", m_Properties), "渐变纹理");
            }
        }
        EditorGUILayout.EndVertical();

        // 高级选项
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("高级选项", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            m_MaterialEditor.ShaderProperty(FindProperty("_Cull", m_Properties), "剔除模式");
            m_MaterialEditor.RenderQueueField();
        }
        EditorGUILayout.EndVertical();

        if (EditorGUI.EndChangeCheck())
        {
            UpdateRenderState();
        }
    }

    private void SetLightMode(ELightMode newLightMode)
    {
        m_CurLightMode = newLightMode;
        UpdateKeywords();
    }

    ELightMode GetCurrentLightMode()
    {
        //_UNLIT_ON _HALFLAMBERT_ON _RAMP_ON
        if (m_TargetMaterial.IsKeywordEnabled("_UNLIT_ON"))
            return ELightMode.无光;
        else if (m_TargetMaterial.IsKeywordEnabled("_HALFLAMBERT_ON"))
            return ELightMode.半兰伯特;
        else if (m_TargetMaterial.IsKeywordEnabled("_RAMP_ON"))
            return ELightMode.渐变纹理;
        else
            return ELightMode.无光;
    }

    private void UpdateKeywords()
    {
        foreach (Material material in m_MaterialEditor.targets)
        {
            CoreUtils.SetKeyword(material, "_UNLIT_ON", m_CurLightMode == ELightMode.无光);
            CoreUtils.SetKeyword(material, "_HALFLAMBERT_ON", m_CurLightMode == ELightMode.半兰伯特);
            CoreUtils.SetKeyword(material, "_RAMP_ON", m_CurLightMode == ELightMode.渐变纹理);
        }
    }

    private void UpdateRenderState()
    {
        UpdateKeywords();

        // 强制刷新材质
        foreach (Material material in m_MaterialEditor.targets)
        {
            EditorUtility.SetDirty(material);
        }
    }
}