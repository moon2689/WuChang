using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace TATools
{
    // 源码来源于：https://zhuanlan.zhihu.com/p/384871087
    public class SimpleShaderGUI : ShaderGUI
    {
        public const int FoldoutIndent = 1; //折叠页缩进等级
        public const string FoldoutSign = "_Foldout"; //折叠页标记，在折叠页属性 显示名字后面务必添加他，他将用来标识该属性为折叠页。其他属性务必不要添加

        //当前折叠等级，他将用来描述PropertyGUI绘制在那级折叠页中
        int m_foldoutLevel = 0;
        //折叠页编辑等级
        int m_foldoutLevel_Editor = 0;
        //折叠页状态, true展开, false折叠
        bool m_foldoutOpen = true;
        //折叠页中的属性是否可以被编辑
        bool m_foldoutEditor = true;
        //绘制的所有材质属性
        MaterialProperty[] m_allProperties;


        public int FoldoutLevel => m_foldoutLevel; //当前折叠等级，他将用来描述PropertyGUI绘制在那级折叠页中
        public int FoldoutLevel_Editor => m_foldoutLevel_Editor; //折叠页编辑等级
        public bool FoldoutOpen => m_foldoutOpen; //折叠页状态, true展开, false折叠
        public bool FoldoutEditor => m_foldoutEditor; //折叠页中的属性是否可以被编辑

        public List<string> SwitchList = new List<string>(); //面板切换列表


        #region static

        //判断该属性是否是折叠页
        public static bool IsFoldout(MaterialProperty property)
        {
            //通过正则表达式 匹配属性显示名字的末尾， 从而判断该属性是否是折叠页
            string pattern = FoldoutSign + @"\s*$";
            return Regex.IsMatch(property.displayName, pattern);
        }

        //获取折叠页显示名字，这将displayName通过正则表达式将_Foldout标记移除
        public static string GetFoldoutDisplayName(MaterialProperty property)
        {
            string pattern = FoldoutSign + @"\s*$";
            Regex reg = new Regex(pattern);
            return reg.Replace(property.displayName, "");
        }

        #endregion


        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            InitializationData();//初始化数据
            m_allProperties = properties;

            //依次绘制所有属性
            for (int i = 0; i < properties.Length; i++)
            {
                //如果该属性不是折叠页，则考虑是否禁用
                if (!IsFoldout(properties[i]))
                    EditorGUI.BeginDisabledGroup(!m_foldoutEditor);

                //如果折叠页为展开状态，或该属性是折叠页则进行属性绘制
                if (m_foldoutOpen || IsFoldout(properties[i]))
                {
                    if ((properties[i].flags & MaterialProperty.PropFlags.HideInInspector) != MaterialProperty.PropFlags.HideInInspector)
                        materialEditor.ShaderProperty(properties[i], properties[i].displayName);
                }

                if (!IsFoldout(properties[i]))
                    EditorGUI.EndDisabledGroup();
            }

            //如果折叠页为展开状态，或该属性是折叠页则进行属性绘制
            if (m_foldoutOpen)
            {
                EditorGUI.BeginDisabledGroup(!m_foldoutEditor);
                //双面全局光照UI
                materialEditor.DoubleSidedGIField();
                //绘制调节队列的控件
                materialEditor.RenderQueueField();
                EditorGUI.EndDisabledGroup();
            }

            // 尝试刷新材质属性
            TryUpdateMatProp();
        }


        //折叠页设置，
        //foldoutLevel  折叠页等级
        //foldoutState  折叠页展开状态， true展开  false折叠
        //foldoutEditor 折叠页中的属性是否可以编辑
        public void SetFoldout(int foldoutLevel, int foldoutLevel_Editor, bool foldoutState, bool foldoutEditor = true)
        {
            EditorGUI.indentLevel += (foldoutLevel - m_foldoutLevel) * FoldoutIndent;
            m_foldoutLevel = foldoutLevel;
            m_foldoutLevel_Editor = foldoutLevel_Editor;
            m_foldoutOpen = foldoutState;
            m_foldoutEditor = foldoutEditor;
        }

        //判断是否显示该控件, 只有包含在SwitchList中才会显示
        public bool WhetherShowThisControl(string[] showList, bool allSatisfy = false)
        {
            if (showList == null || showList.Length < 1)
                return true;

            if (allSatisfy)
            {
                foreach (string show in showList)
                {
                    if (!SwitchList.Contains(show))
                        return false;
                }
                return true;
            }
            else
            {
                foreach (string show in showList)
                {
                    if (SwitchList.Contains(show))
                        return true;
                }
                return false;
            }
        }

        //查找一个属性
        public MaterialProperty FindProperty(string name)
        {
            return FindProperty(name, m_allProperties, false);
        }

        //初始化数据
        void InitializationData()
        {
            m_foldoutLevel = 0;
            m_foldoutLevel_Editor = 0;
            m_foldoutOpen = true;
            m_foldoutEditor = true;
            EditorGUI.indentLevel = 0;
            SwitchList.Clear();
        }

        void TryUpdateMatProp()
        {
            MaterialProperty propIsOpaque = TryUpdateMatProp_Opaque();
            TryUpdateMatProp_Clip(propIsOpaque);
        }

        // 更新固体/半透明相关设置
        MaterialProperty TryUpdateMatProp_Opaque()
        {
            MaterialProperty propIsOpaque = FindProperty("_IsOpaque");
            if (propIsOpaque == null || propIsOpaque.type != MaterialProperty.PropType.Float)
                return propIsOpaque;

            bool isOpaque = propIsOpaque.floatValue > 0;
            string strRenderType = isOpaque ? "Opaque" : "Transparent";
            int intZwrite = isOpaque ? 1 : 0;
            int intCull = (int)(isOpaque ? CullMode.Back : CullMode.Off);
            int intSrcBlend = (int)(isOpaque ? BlendMode.One : BlendMode.SrcAlpha);
            int intDstBlend = (int)(isOpaque ? BlendMode.Zero : BlendMode.OneMinusSrcAlpha);
            foreach (Material m in propIsOpaque.targets)
            {
                bool needReset = false;
                // Render Queue
                if (isOpaque)
                {
                    if (m.renderQueue > (int)RenderQueue.GeometryLast)
                    {
                        m.renderQueue = (int)RenderQueue.Geometry;
                        needReset = true;
                    }
                }
                else
                {
                    if (m.renderQueue < (int)RenderQueue.Transparent)
                    {
                        m.renderQueue = (int)RenderQueue.Transparent;
                        needReset = true;
                    }
                }

                if (needReset)
                {
                    // Render Type
                    m.SetOverrideTag("RenderType", strRenderType);

                    // ZWrite
                    if (m.HasProperty("_ZWrite"))
                        m.SetFloat("_ZWrite", intZwrite);

                    // Cull
                    if (m.HasProperty("_Cull"))
                        m.SetFloat("_Cull", intCull);

                    // Blend
                    if (m.HasProperty("_SrcBlend"))
                        m.SetFloat("_SrcBlend", intSrcBlend);
                    if (m.HasProperty("_DstBlend"))
                        m.SetFloat("_DstBlend", intDstBlend);
                }
            }
            return propIsOpaque;
        }

        // 更新固体的Clip相关设置
        void TryUpdateMatProp_Clip(MaterialProperty propIsOpaque)
        {
            // 对于固体，设置clip才有效
            if (propIsOpaque != null && (propIsOpaque.type != MaterialProperty.PropType.Float || propIsOpaque.floatValue <= 0))
                return;

            MaterialProperty propIsClip = FindProperty("_Clip");
            if (propIsClip == null)
                return;

            bool isEnableClip = propIsClip.floatValue > 0;
            if (propIsClip == null || propIsClip.type != MaterialProperty.PropType.Float)
                return;

            foreach (Material m in propIsClip.targets)
            {
                if (isEnableClip)
                {
                    if (m.renderQueue < (int)RenderQueue.AlphaTest)
                        m.renderQueue = (int)RenderQueue.AlphaTest;
                }
                else
                {
                    if (m.renderQueue >= (int)RenderQueue.AlphaTest)
                        m.renderQueue = (int)RenderQueue.Geometry;
                }
            }
        }

    }
}
