using UnityEngine;
using UnityEditor;

public class VFXGeneralShaderGUI : ShaderGUI
{
    private static class Styles
    {
        //主贴图
        public static GUIContent MainTexAlphaText = new GUIContent("透明通道选择");
        public static GUIContent MainTexText = new GUIContent("主贴图");
        public static GUIContent MainTexColorText = new GUIContent("颜色");
        public static GUIContent BrightnessText = new GUIContent("主贴图强度");
        public static GUIContent PivotMainTexText = new GUIContent("旋转中心");
        public static GUIContent MainTexRotationAngleText = new GUIContent("旋转角度");
        public static GUIContent UspeedMainTexText = new GUIContent("Uspeed");
        public static GUIContent VspeedMainTexText = new GUIContent("Vspeed");

        //Mask贴图

        public static GUIContent MaskTexAlphaText = new GUIContent("透明通道选择");
        public static GUIContent MaskTexText = new GUIContent("遮罩贴图");
        public static GUIContent UspeedMaskTexText = new GUIContent("Uspeed");
        public static GUIContent VspeedMaskTexText = new GUIContent("Vspeed");
        public static GUIContent MaskTexRotationAngleText = new GUIContent("旋转角度");

        //扰动贴图
        public static GUIContent DistortTexText = new GUIContent("扭曲贴图");
        public static GUIContent DistortTexIntensityText = new GUIContent("扭曲强度");
        public static GUIContent UspeedDistortTexText = new GUIContent("Uspeed");
        public static GUIContent VspeedDistortTexText = new GUIContent("Vspeed");

        //溶解
        public static GUIContent DissolveModeText = new GUIContent("溶解模式选择");
        public static GUIContent ReverseDissolveText = new GUIContent("反向");
        public static GUIContent DissolveTexText = new GUIContent("溶解贴图");
        public static GUIContent DissolveFactorText = new GUIContent("溶解值");
        public static GUIContent HardnessFactorText = new GUIContent("溶解边缘硬度");
        public static GUIContent DissolveWidthText = new GUIContent("溶解边缘宽度");
        public static GUIContent WidthColorText = new GUIContent("溶解边缘颜色");
        public static GUIContent UspeedDissolveTexText = new GUIContent("Uspeed");
        public static GUIContent VspeedDissolveTexText = new GUIContent("Vspeed");
    }

    #region 自定义变量

    //切换模式
    private static bool _Base_Foldout = true;

    private static bool _MainTex_Foldout = true;

    //private static bool _MaskTex_Foldout = true;
    //private static bool _DistortTex_Foldout = true;
    private static bool _DissolveTex_Foldout = true;

    private bool m_isClipEnable;
    private bool m_isPannerEnable;
    private bool m_isMaskEnable;

    private bool m_isDissolveEnable;

    //主贴图
    //private static string MainTexAlphaName = "_MainTexAlpha";
    private static string ColorName = "_MainTexColor";
    private static string BrightnessName = "_Brightness";
    private static string PivotMainTexName = "_Pivot_MainTex";
    private static string MainTexRotationAngleName = "_MainTexRotationAngle";
    private static string UspeedMainTexName = "_Uspeed_MainTex";

    private static string VspeedMainTexName = "_Vspeed_MainTex";

    //mask贴图
    private static string MaskTexAlphaName = "_MaskTexAlpha";

    //private static string MaskTexName = "_MaskTex";
    private static string UspeedMaskTexName = "_Uspeed_MaskTex";
    private static string VspeedMaskTexName = "_Vspeed_MaskTex";

    private static string MaskTexRotationAngleName = "_MaskTexRotationAngle";

    //扭曲贴图
    private static string DistortTexName = "_DistortTex";
    private static string DistortTexIntensityName = "_DistortTexIntensity";
    private static string UspeedDistortTexName = "_Uspeed_DistortTex";

    private static string VspeedDistortTexName = "_Vspeed_DistortTex";

    //溶解贴图
    private static string _DissolveModeName = "_DissolveMode";
    private static string ReverseDissolveName = "_ReverseDissolve";
    private static string DissolveTexName = "_DissolveTex";
    private static string DissolveFactorName = "_DissolveFactor";
    private static string HardnessFactorName = "_HardnessFactor";
    private static string DissolveWidthName = "_DissolveWidth";
    private static string WidthColorName = "_WidthColor";
    private static string UspeedDissolveTexName = "_Uspeed_DissolveTex";
    private static string VspeedDissolveTexName = "_Vspeed_DissolveTex";


    MaterialEditor m_MaterialEditon;

    enum BlendMode
    {
        AlphaBlend,
        ADD,
        Opaque
    }

    enum ZTest
    {
        Default,
        Always
    }

    //材质属性查找自定义命名
    private ZTest m_zTestEnum = ZTest.Default;
    private MaterialProperty m_zTestProp;
    private MaterialProperty m_cullMode;
    private MaterialProperty m_ZwriteMode;
    private MaterialProperty m_particleModeProp;
    private string[] m_blendModeNames = System.Enum.GetNames(typeof(BlendMode));
    private MaterialProperty m_option;

    private MaterialProperty m_blendTempProp, m_srcBlendProp, m_dstBlendProp;

    //主贴图查找自定义命名
    private MaterialProperty _MainTexAlpha;
    private MaterialProperty _MainTex = null;
    private MaterialProperty _MainTexColor = null;
    private MaterialProperty _Brightness = null;
    private MaterialProperty _Pivot_MainTex = null;
    private MaterialProperty _MainTexRotationAngle = null;
    private MaterialProperty _Uspeed_MainTex = null;

    private MaterialProperty _Vspeed_MainTex = null;

    //Mask贴图查找自定义命名
    private MaterialProperty _MaskTexAlpha;
    private MaterialProperty _MaskTex = null;
    private MaterialProperty _Uspeed_MaskTex = null;
    private MaterialProperty _Vspeed_MaskTex = null;

    private MaterialProperty _MaskTexRotationAngle = null;

    //扭曲贴图查找自定义命名
    private MaterialProperty _DistortTex;
    private MaterialProperty _DistortTexIntensity = null;
    private MaterialProperty _Uspeed_DistortTex = null;

    private MaterialProperty _Vspeed_DistortTex = null;

    //溶解贴图查找自定义命名
    private MaterialProperty _DissolveMode;
    int m_dissolveModeChoose;
    private MaterialProperty _ReverseDissolve = null;
    private MaterialProperty _DissolveTex;
    private MaterialProperty _DissolveFactor = null;
    private MaterialProperty _HardnessFactor = null;
    private MaterialProperty _DissolveWidth = null;
    private MaterialProperty _WidthColor = null;
    private MaterialProperty _Uspeed_DissolveTex = null;
    private MaterialProperty _Vspeed_DissolveTex = null;

    private MaterialProperty _BlurSize = null;
    
    #endregion

    #region 自定义下拉菜单样式

    static bool Foldout(bool display, string title)
    {
        var style = new GUIStyle("ShurikenModuleTitle");
        style.font = new GUIStyle(EditorStyles.boldLabel).font;
        style.border = new RectOffset(15, 7, 4, 4);
        style.fixedHeight = 30;
        style.contentOffset = new Vector2(30f, -2f);
        style.fontSize = (int)(style.fontSize * 1.5f);
        style.normal.textColor = new Color(0.7f, 0.8f, 0.9f);

        var rect = GUILayoutUtility.GetRect(15f, 25f, style);
        //GUI.backgroundColor = isFolding ? Color.white : new Color(0.85f, 0.85f, 0.85f);
        GUI.Box(rect, title, style);
        //背景颜色
        GUI.backgroundColor = Color.white;

        var e = Event.current;

        var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
        if (e.type == EventType.Repaint)
        {
            EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
        }

        if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
        {
            display = !display;
            e.Use();
        }

        return display;
    }

    #endregion

    #region 自定义下拉菜单2

    static bool Foldout2(bool display, string title)
    {
        var style = new GUIStyle("ShurikenModuleTitle");
        style.font = new GUIStyle(EditorStyles.boldLabel).font;
        style.border = new RectOffset(15, 7, 4, 4);
        style.fixedHeight = 18;
        style.contentOffset = new Vector2(30f, -2f);
        style.fontSize = 10;
        style.normal.textColor = new Color(0.75f, 0.75f, 0.75f);

        var rect = GUILayoutUtility.GetRect(16f, 15f, style);
        GUI.Box(rect, title, style);

        var e = Event.current;

        var toggleRect = new Rect(rect.x + 15f, rect.y + 2f, 13f, 13f);
        if (e.type == EventType.Repaint)
        {
            EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
        }

        if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
        {
            display = !display;
            e.Use();
        }

        return display;
    }

    #endregion


    public void FindProperties(MaterialProperty[] properties)
    {
        //主贴图
        _MainTexAlpha = FindProperty("_MainTexAlpha", properties);
        _MainTex = FindProperty("_MainTex", properties);
        _MainTexColor = FindProperty(ColorName, properties);
        _Brightness = FindProperty(BrightnessName, properties);
        _Pivot_MainTex = FindProperty(PivotMainTexName, properties);
        _MainTexRotationAngle = FindProperty(MainTexRotationAngleName, properties);
        _Uspeed_MainTex = FindProperty(UspeedMainTexName, properties);
        _Vspeed_MainTex = FindProperty(VspeedMainTexName, properties);
        //mask贴图
        _MaskTexAlpha = FindProperty(MaskTexAlphaName, properties);
        _MaskTex = FindProperty("_MaskTex", properties);
        _Uspeed_MaskTex = FindProperty(UspeedMaskTexName, properties);
        _Vspeed_MaskTex = FindProperty(VspeedMaskTexName, properties);
        _MaskTexRotationAngle = FindProperty(MaskTexRotationAngleName, properties);
        //扭曲贴图
        _DistortTex = FindProperty(DistortTexName, properties);
        _DistortTexIntensity = FindProperty(DistortTexIntensityName, properties);
        _Uspeed_DistortTex = FindProperty(UspeedDistortTexName, properties);
        _Vspeed_DistortTex = FindProperty(VspeedDistortTexName, properties);
        //溶解
        _DissolveMode = FindProperty(_DissolveModeName, properties);
        _ReverseDissolve = FindProperty(ReverseDissolveName, properties);
        _DissolveTex = FindProperty(DissolveTexName, properties);
        _DissolveFactor = FindProperty(DissolveFactorName, properties);
        _HardnessFactor = FindProperty(HardnessFactorName, properties);
        _DissolveWidth = FindProperty(DissolveWidthName, properties);
        _WidthColor = FindProperty(WidthColorName, properties);
        _Uspeed_DissolveTex = FindProperty(UspeedDissolveTexName, properties);
        _Vspeed_DissolveTex = FindProperty(VspeedDissolveTexName, properties);

        _BlurSize = FindProperty("_BlurSize", properties);
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        FindProperties(properties);
        m_MaterialEditon = materialEditor;
        Material material = materialEditor.target as Material;

        #region 属性设置区域

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _Base_Foldout = Foldout(_Base_Foldout, "基础设置(BasicSetting)");
        if (_Base_Foldout)
        {
            EditorGUI.indentLevel++;
            GUI_Base(material, properties);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();

        #endregion

        #region 主贴图

        //if (mainTex_Sampler.textureValue != null)
        {
            EditorGUILayout.BeginVertical((EditorStyles.helpBox));
            _MainTex_Foldout = EditorGUILayout.BeginFoldoutHeaderGroup(_MainTex_Foldout, "主贴图");
            if (_MainTex_Foldout)
            {
                EditorGUI.indentLevel++;
                GUI_MainTex(material, properties);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        #endregion

        #region 遮罩贴图

        EditorGUILayout.BeginVertical((EditorStyles.helpBox));
        bool oldMaskEnable = material.GetInt("_MAINTEX_MASK_ON") == 1;
        bool newMaskEnable = EditorGUILayout.Toggle("启用遮罩", oldMaskEnable);
        if (oldMaskEnable != newMaskEnable)
        {
            material.SetInt("_MAINTEX_MASK_ON", newMaskEnable ? 1 : 0);

            if (!newMaskEnable)
            {
                material.SetTexture("_MaskTex", null);
            }

            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
        }

        if (newMaskEnable) // 当启用遮罩被勾选时才显示后续属性
        {
            EditorGUI.indentLevel++;
            GUI_MaskTex(material, properties);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();

        #endregion

        #region 扭曲贴图

        EditorGUILayout.BeginVertical((EditorStyles.helpBox));
        bool oldDistortEnable = material.GetInt("_DISTORT_ON") == 1;
        bool newDistortEnable = EditorGUILayout.Toggle("启用扭曲", oldDistortEnable);
        //isDistortEnable = EditorGUILayout.Toggle("启用扭曲", isDistortEnable);
        if (oldDistortEnable != newDistortEnable)
        {
            material.SetInt("_DISTORT_ON", newDistortEnable ? 1 : 0);
            if (!newDistortEnable)
            {
                material.SetTexture("_DistortTex", null);
            }

            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
        }

        if (newDistortEnable)
        {
            EditorGUI.indentLevel++;
            GUI_DistortTex(material, properties);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();

        #endregion

        #region 溶解贴图

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        //isDissolveEnable = material.IsKeywordEnabled("_DISSOLVE");
        m_isDissolveEnable = material.IsKeywordEnabled("_DISSOLVE") || material.IsKeywordEnabled("_DISSOLVEPLUS");
        //isDissolveEnable = EditorGUILayout.Toggle("溶解", isDissolveEnable);
        if (_DissolveTex_Foldout)
        {
            EditorGUI.indentLevel++;
            GUI_DissolveTex(material, properties);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();

        #endregion

        #region 模糊

        EditorGUILayout.BeginVertical((EditorStyles.helpBox));
        bool oldBlurEnable = material.GetInt("_BlurOn") == 1;
        bool newBlurEnable = EditorGUILayout.Toggle("启用模糊", oldBlurEnable);
        if (oldBlurEnable != newBlurEnable)
        {
            material.SetInt("_BlurOn", newBlurEnable ? 1 : 0);
            if (newBlurEnable)
            {
                material.EnableKeyword("_BLUR_ON");
            }
            else
            {
                material.DisableKeyword("_BLUR_ON");
            }

            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
        }

        if (newBlurEnable)
        {
            EditorGUI.indentLevel++;
            m_MaterialEditon.ShaderProperty(_BlurSize, "模糊尺寸");
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();

        #endregion

        //材质层级
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        materialEditor.RenderQueueField();
        EditorGUILayout.EndVertical();
    }

    public void GUI_Base(Material material, MaterialProperty[] properties)
    {
        //EditorGUI.showMixedValue = option.hasMixedValue;//选择多个显示横线
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        #region BlendMode

        EditorGUILayout.BeginHorizontal();
        //EditorGUILayout.PrefixLabel("混合模式");
        m_srcBlendProp = FindProperty("_BlendModeSrc", properties);
        m_dstBlendProp = FindProperty("_BlendModeDst", properties);
        m_blendTempProp = FindProperty("_BlendTemp", properties);
        EditorGUI.BeginChangeCheck();
        m_blendTempProp.floatValue = EditorGUILayout.Popup("混合模式(BlendMode)", (int)m_blendTempProp.floatValue, m_blendModeNames);
        if (EditorGUI.EndChangeCheck())
        {
            SetupBlentMode(material);
        }

        EditorGUILayout.EndHorizontal();

        #endregion

        #region ZTest

        EditorGUILayout.BeginHorizontal();
        m_zTestProp = FindProperty("_ZTest", properties);
        switch (m_zTestProp.floatValue)
        {
            case 4:
                m_zTestEnum = ZTest.Default;
                break;
            case 8:
                m_zTestEnum = ZTest.Always;
                break;
        }

        m_zTestEnum = (ZTest)EditorGUILayout.EnumPopup("深度测试(ZTest)", m_zTestEnum);
        if (m_zTestEnum == ZTest.Default)
        {
            material.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
        }

        if (m_zTestEnum == ZTest.Always)
        {
            material.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
        }

        EditorGUILayout.EndHorizontal();

        #endregion

        #region ZWrite

        EditorGUILayout.BeginHorizontal();
        m_ZwriteMode = FindProperty("_ZWrite", properties);
        m_MaterialEditon.ShaderProperty(m_ZwriteMode, "深度写入（Zwrite）");
        EditorGUILayout.EndHorizontal();

        #endregion

        #region CULLMODE

        EditorGUILayout.BeginHorizontal();
        m_cullMode = FindProperty("_CullMode", properties);
        m_MaterialEditon.ShaderProperty(m_cullMode, "剔除模式（CullMode）");
        EditorGUILayout.EndHorizontal();

        #endregion

        #region Clip

        EditorGUILayout.BeginHorizontal();
        m_isPannerEnable = material.IsKeywordEnabled("_CLIP_ON");
        bool newClipEnable = EditorGUILayout.Toggle("启用剪裁(Clip)", material.GetInt("_Clip_On") == 1);
        if (material.GetInt("_Clip_On") == 1 != newClipEnable)
        {
            material.SetInt("_Clip_On", newClipEnable ? 1 : 0);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
        }

        EditorGUILayout.EndHorizontal();

        #endregion

        #region 是否为Panner

        EditorGUILayout.BeginHorizontal();
        //isPannerEnable = material.IsKeywordEnabled("_PANNER_ON"); // 不需要再直接获取关键字状态作为初始值
        bool newPannerEnable = EditorGUILayout.Toggle("启用UV流动（所有纹理）", material.GetInt("_Panner_ON") == 1);
        if (material.GetInt("_Panner_ON") == 1 != newPannerEnable)
        {
            material.SetInt("_Panner_ON", newPannerEnable ? 1 : 0);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
        }

        m_isPannerEnable = newPannerEnable;
        EditorGUILayout.EndHorizontal();

        #endregion

        #region 粒子模式

        m_particleModeProp = FindProperty("_ParticleMode", properties);
        m_MaterialEditon.ShaderProperty(m_particleModeProp, "粒子系统CustomData模式");
        switch (m_particleModeProp.floatValue)
        {
            case 0:
                material.SetFloat("_ParticleModeTemp01", 0);
                material.SetFloat("_ParticleModeTemp02", 0);
                break;
            case 1:
                material.SetFloat("_ParticleModeTemp01", 1);
                material.SetFloat("_ParticleModeTemp02", 0);
                EditorGUILayout.HelpBox("CustomData1和2的xyzw分别对应主纹理、蒙版、溶解贴图、扰动贴图的uvPanner", MessageType.None);
                break;
            case 2:
                material.SetFloat("_ParticleModeTemp01", 0);
                material.SetFloat("_ParticleModeTemp02", 1);
                EditorGUILayout.HelpBox("Custom1的x对应溶解值取值范围0~1，yz对应maintex的uv偏移，w对应扰动的强度，Custom2需要选择Color模式，对应亮边颜色", MessageType.None);
                EditorUtility.SetDirty(material);
                AssetDatabase.SaveAssets();
                break;
        }

        #endregion

        EditorGUILayout.EndVertical();
    }

    public void GUI_MainTex(Material material, MaterialProperty[] properties)
    {
        // (_MainTex != null)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            m_MaterialEditon.ShaderProperty(_MainTexAlpha, Styles.MainTexAlphaText);
            m_MaterialEditon.TextureProperty(_MainTex, Styles.MainTexText.text);
            m_MaterialEditon.ShaderProperty(_MainTexColor, Styles.MainTexColorText);
            m_MaterialEditon.ShaderProperty(_Brightness, Styles.BrightnessText);
            m_MaterialEditon.ShaderProperty(_Pivot_MainTex, Styles.PivotMainTexText);
            m_MaterialEditon.ShaderProperty(_MainTexRotationAngle, Styles.MainTexRotationAngleText);
            if (m_isPannerEnable)
            {
                m_MaterialEditon.ShaderProperty(_Uspeed_MainTex, Styles.UspeedMainTexText);
                m_MaterialEditon.ShaderProperty(_Vspeed_MainTex, Styles.VspeedMainTexText);
            }

            EditorGUILayout.EndVertical();
        }
    }

    public void GUI_MaskTex(Material material, MaterialProperty[] properties)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        // 直接读取 Material Property 的值
        bool isMaskEnabledInShader = material.GetInt("_MAINTEX_MASK_ON") == 1;

        if (isMaskEnabledInShader)
        {
            // 如果 Shader 中启用了遮罩，则显示相关属性并确保关键字被启用
            material.EnableKeyword("_MAINTEX_MASK_ON");

            m_MaterialEditon.ShaderProperty(_MaskTexAlpha, Styles.MaskTexAlphaText);
            m_MaterialEditon.TextureProperty(_MaskTex, Styles.MaskTexText.text);
            m_MaterialEditon.ShaderProperty(_MaskTexRotationAngle, Styles.MaskTexRotationAngleText.text);
            if (m_isPannerEnable)
            {
                m_MaterialEditon.ShaderProperty(_Uspeed_MaskTex, Styles.UspeedMaskTexText.text);
                m_MaterialEditon.ShaderProperty(_Vspeed_MaskTex, Styles.VspeedMaskTexText.text);
            }
        }
        else
        {
            // 如果 Shader 中禁用了遮罩，则确保关键字也被禁用
            material.DisableKeyword("_MAINTEX_MASK_ON");
        }

        EditorGUILayout.EndVertical();
    }

    public void GUI_DistortTex(Material material, MaterialProperty[] properties)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        bool isDistortEnableInShader = material.GetInt("_DISTORT_ON") == 1;

        if (isDistortEnableInShader)
        {
            material.EnableKeyword("_DISTORT_ON");

            m_MaterialEditon.TextureProperty(_DistortTex, Styles.DistortTexText.text);
            m_MaterialEditon.ShaderProperty(_DistortTexIntensity, Styles.DistortTexIntensityText.text);
            if (m_isPannerEnable)
            {
                m_MaterialEditon.ShaderProperty(_Uspeed_DistortTex, Styles.UspeedDistortTexText.text);
                m_MaterialEditon.ShaderProperty(_Vspeed_DistortTex, Styles.VspeedDistortTexText.text);
            }
        }
        else
        {
            material.DisableKeyword("_DISTORT_ON");
        }

        EditorGUILayout.EndVertical();
    }

    private void GUI_DissolveTex(Material material, MaterialProperty[] properties)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _DissolveMode = FindProperty("_DissolveMode", properties);
        m_MaterialEditon.ShaderProperty(_DissolveMode, "溶解模式");
        int oldDissolveModeChoose = m_dissolveModeChoose;
        switch (_DissolveMode.floatValue)
        {
            case 0:
                material.DisableKeyword("_DISSOLVE");
                material.DisableKeyword("_DISSOLVEPLUS");
                m_dissolveModeChoose = 0;
                break;
            case 1:
                material.EnableKeyword("_DISSOLVE");
                material.DisableKeyword("_DISSOLVEPLUS");
                m_dissolveModeChoose = 1;
                break;
            case 2:
                material.EnableKeyword("_DISSOLVEPLUS");
                material.DisableKeyword("_DISSOLVE");
                m_dissolveModeChoose = 2;
                break;
        }

        if (m_isDissolveEnable)
        {
            if (m_dissolveModeChoose == 1)
            {
                material.EnableKeyword("_DISSOLVE");

                m_MaterialEditon.TextureProperty(_DissolveTex, Styles.DissolveTexText.text);
                m_MaterialEditon.ShaderProperty(_ReverseDissolve, Styles.ReverseDissolveText.text);
                m_MaterialEditon.ShaderProperty(_DissolveFactor, Styles.DissolveFactorText.text);
                m_MaterialEditon.ShaderProperty(_HardnessFactor, Styles.HardnessFactorText.text);
                if (m_isPannerEnable)
                {
                    m_MaterialEditon.ShaderProperty(_Uspeed_DissolveTex, Styles.UspeedDissolveTexText);
                    m_MaterialEditon.ShaderProperty(_Vspeed_DissolveTex, Styles.VspeedDissolveTexText);
                }
            }
            else if (m_dissolveModeChoose == 2)
            {
                material.EnableKeyword("_DISSOLVEPLUS");
                m_MaterialEditon.ShaderProperty(_ReverseDissolve, Styles.ReverseDissolveText.text);
                m_MaterialEditon.TextureProperty(_DissolveTex, Styles.DissolveTexText.text);
                m_MaterialEditon.ShaderProperty(_DissolveFactor, Styles.DissolveFactorText.text);
                m_MaterialEditon.ShaderProperty(_HardnessFactor, Styles.HardnessFactorText.text);
                m_MaterialEditon.ShaderProperty(_WidthColor, Styles.WidthColorText.text);
                m_MaterialEditon.ShaderProperty(_DissolveWidth, Styles.DissolveWidthText.text);
                if (m_isPannerEnable)
                {
                    m_MaterialEditon.ShaderProperty(_Uspeed_DissolveTex, Styles.UspeedDissolveTexText);
                    m_MaterialEditon.ShaderProperty(_Vspeed_DissolveTex, Styles.VspeedDissolveTexText);
                }
            }
            else
            {
                material.DisableKeyword("_DISSOLVEPLUS");
            }
        }
        else
        {
            if (oldDissolveModeChoose != m_dissolveModeChoose)
            {
                material.SetTexture("_DissolveTex", null);
            }
        }

        if (oldDissolveModeChoose != m_dissolveModeChoose)
        {
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
        }

        EditorGUILayout.EndVertical();
    }

    void SetupBlentMode(Material targetMat)
    {
        int value = (int)targetMat.GetFloat("_BlendTemp");
        switch (value)
        {
            case 0:
                targetMat.SetInt("_BlendModeSrc", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                targetMat.SetInt("_BlendModeDst", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                targetMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                //targetMat.SetInt("_ZWrite", 0);
                targetMat.SetOverrideTag("RenderType", "Transparent");
                break;
            case 1:
                targetMat.SetInt("_BlendModeSrc", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                targetMat.SetInt("_BlendModeDst", (int)UnityEngine.Rendering.BlendMode.One);
                targetMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                //targetMat.SetInt("_ZWrite", 0);
                targetMat.SetOverrideTag("RenderType", "Transparent");
                break;
            case 2:
                targetMat.SetInt("_BlendModeSrc", (int)UnityEngine.Rendering.BlendMode.One);
                targetMat.SetInt("_BlendModeDst", (int)UnityEngine.Rendering.BlendMode.Zero);
                targetMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
                //targetMat.SetInt("_ZWrite", 1);
                break;
        }
    }
}