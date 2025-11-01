using UnityEngine;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine.Rendering;

public class UnBlinnPhongUniversalShaderGUI : ShaderGUI
{
    // 渲染模式枚举
    public enum ERenderMode
    {
        固体,
        固体裁剪,
        半透明,
        // Additive,
        // Multiply
    }

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
    private MaterialProperty m_srcBlendProp;
    private MaterialProperty m_dstBlendProp;
    private MaterialProperty m_zWriteProp;
    private MaterialProperty m_alphaClipProp;
    private MaterialProperty m_cutoffProp;

    private ERenderMode m_CurRenderMode;
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
        m_srcBlendProp = FindProperty("_SrcBlend", m_Properties);
        m_dstBlendProp = FindProperty("_DstBlend", m_Properties);
        m_zWriteProp = FindProperty("_ZWrite", m_Properties);
        m_alphaClipProp = FindProperty("_AlphaClip", m_Properties);
        m_cutoffProp = FindProperty("_Cutoff", m_Properties);
    }

    private void RenderShaderGUI()
    {
        EditorGUI.BeginChangeCheck();

        // 获取当前渲染模式
        m_CurRenderMode = GetCurrentRenderMode();
        ERenderMode newMode = (ERenderMode)EditorGUILayout.EnumPopup("渲染模式", m_CurRenderMode);

        if (newMode != m_CurRenderMode)
        {
            SetRenderMode(newMode);
        }

        EditorGUILayout.LabelField("注意：开启固体裁剪和半透明会有额外消耗，非必要不开启", EditorStyles.linkLabel);

        // 输入
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("输入", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            m_MaterialEditor.ShaderProperty(FindProperty("_BaseMap", m_Properties), "基础图");
            m_MaterialEditor.ShaderProperty(FindProperty("_BaseColor", m_Properties), "基础颜色");

            if (m_CurRenderMode == ERenderMode.固体裁剪)
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
            EditorGUILayout.LabelField("混合模式", EditorStyles.miniBoldLabel);
            m_MaterialEditor.ShaderProperty(m_srcBlendProp, "Source Blend");
            m_MaterialEditor.ShaderProperty(m_dstBlendProp, "Destination Blend");
            m_MaterialEditor.ShaderProperty(m_zWriteProp, "Z Write");
        }
        EditorGUILayout.EndVertical();

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

    private ERenderMode GetCurrentRenderMode()
    {
        float srcBlend = m_srcBlendProp.floatValue;
        float dstBlend = m_dstBlendProp.floatValue;
        float zWrite = m_zWriteProp.floatValue;
        bool alphaClip = m_alphaClipProp.floatValue > 0.5f;

        if (alphaClip)
            return ERenderMode.固体裁剪;
        if (zWrite < 0.5f)
            return ERenderMode.半透明;

        if (srcBlend == (float)UnityEngine.Rendering.BlendMode.SrcAlpha)
            return ERenderMode.半透明;

        return ERenderMode.固体;
    }

    private void SetRenderMode(ERenderMode mode)
    {
        switch (mode)
        {
            case ERenderMode.固体:
                SetOpaqueMode();
                break;
            case ERenderMode.固体裁剪:
                SetCutoutMode();
                break;
            case ERenderMode.半透明:
                SetTransparentMode();
                break;
        }

        UpdateKeywords();
    }

    private void SetOpaqueMode()
    {
        m_srcBlendProp.floatValue = (float)UnityEngine.Rendering.BlendMode.One;
        m_dstBlendProp.floatValue = (float)UnityEngine.Rendering.BlendMode.Zero;
        m_zWriteProp.floatValue = 1f;
        m_alphaClipProp.floatValue = 0f;

        SetRenderQueue("Geometry");
        SetRenderType("Opaque");
    }

    private void SetCutoutMode()
    {
        m_srcBlendProp.floatValue = (float)UnityEngine.Rendering.BlendMode.One;
        m_dstBlendProp.floatValue = (float)UnityEngine.Rendering.BlendMode.Zero;
        m_zWriteProp.floatValue = 1f;
        m_alphaClipProp.floatValue = 1f;

        SetRenderQueue("AlphaTest");
        SetRenderType("TransparentCutout");
    }

    private void SetTransparentMode()
    {
        m_srcBlendProp.floatValue = (float)UnityEngine.Rendering.BlendMode.SrcAlpha;
        m_dstBlendProp.floatValue = (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
        m_zWriteProp.floatValue = 0f;
        m_alphaClipProp.floatValue = 0f;

        SetRenderQueue("Transparent");
        SetRenderType("Transparent");
    }

    private void SetRenderQueue(string queue)
    {
        foreach (Material material in m_MaterialEditor.targets)
        {
            switch (queue)
            {
                case "Geometry":
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
                    break;
                case "AlphaTest":
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                    break;
                case "Transparent":
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
            }
        }
    }

    private void SetRenderType(string renderType)
    {
        foreach (Material material in m_MaterialEditor.targets)
        {
            material.SetOverrideTag("RenderType", renderType);
        }
    }

    private void UpdateKeywords()
    {
        foreach (Material material in m_MaterialEditor.targets)
        {
            // 更新 Alpha 测试关键字
            bool alphaClip = m_alphaClipProp.floatValue > 0.5f;
            CoreUtils.SetKeyword(material, "_ALPHATEST_ON", alphaClip);

            // 更新 Alpha 预乘关键字
            // bool alphaPremultiply = m_alphaPremultiplyProp.floatValue > 0.5f;
            // CoreUtils.SetKeyword(material, "_ALPHAPREMULTIPLY_ON", alphaPremultiply);

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