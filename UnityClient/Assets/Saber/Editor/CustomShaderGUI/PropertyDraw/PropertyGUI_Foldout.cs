using UnityEditor;
using UnityEngine;

//该脚本主要是对折叠页的处理
namespace TATools
{
    //折叠页样式
    public enum FoldoutStyle
    {
        Big = 1,
        Median = 2,
        Small = 3
    }

    //折叠页绘制
    //思路来源于word中的标题，可以设置1级，2级标题进行层层嵌套
    public class FoldoutDrawer : MaterialPropertyDrawer
    {
        int m_foldoutLevel = 1; //折叠页等级
        float m_foldoutIndent = 15; //折叠页自身缩进大小
        bool m_foldoutOpen = true; //折叠页状态, true展开, false折叠
        FoldoutStyle m_foldoutStyle = FoldoutStyle.Big; //折叠页样式
        bool m_foldoutToggleDraw = false; //是否绘制折叠页复选框
        bool m_foldoutEditor = true; //折叠页内容是否可以被编辑
        SimpleShaderGUI m_simpleShaderGUI; //绘制折叠页的ShaderGUI
        MaterialProperty m_property; //材质属性
        string[] m_showList = new string[0]; //该控件会在以下选项中显示
        bool m_isAlwaysShow = true; //是否总是显示


        //foldoutLevel      折叠页等级，折叠页最低等级为1级(默认1级折叠页)
        //foldoutStyle      折叠页外观样式(默认第一种)，目前有3种 1 大折叠页样式， 2 中折叠页样式, 3 小折叠页样式
        //foldoutToggleDraw 折叠页 复选框是否绘制， 0 不绘制 , 1绘制 
        //foldoutOpen       折叠页初始展开状态，    0 折叠， 1展开
        //showList          填写任意数量的选项，当其中一个选项被选中时，该控件会被渲染
        public FoldoutDrawer() : this(1) { }

        public FoldoutDrawer(float foldoutLevel) : this(foldoutLevel, 1) { }

        public FoldoutDrawer(float foldoutLevel, float foldoutStyle) : this(foldoutLevel, foldoutStyle, 0) { }

        public FoldoutDrawer(float foldoutLevel, float foldoutStyle, float foldoutToggleDraw) : this(foldoutLevel, foldoutStyle, foldoutToggleDraw, 1) { }

        public FoldoutDrawer(float foldoutLevel, float foldoutStyle, float foldoutToggleDraw, float foldoutOpen, params string[] showList)
        {
            int level = (int)foldoutLevel;
            int style = (int)foldoutStyle;
            int toggleDraw = (int)foldoutToggleDraw;
            int open = (int)foldoutOpen;

            //设置折叠页等级
            m_foldoutLevel = level < 1 ? 1 : level;

            //设置折叠页样式
            switch (style)
            {
                case 2: m_foldoutStyle = FoldoutStyle.Median; break;
                case 3: m_foldoutStyle = FoldoutStyle.Small; toggleDraw = 0; break;//如果样式是 小折叠页，则不进行复选框的绘制
                default: m_foldoutStyle = FoldoutStyle.Big; break;
            }

            //是否绘制复选框
            m_foldoutToggleDraw = toggleDraw != 0;

            //折叠页默认展开状态
            m_foldoutOpen = open != 0;

            //设置显示列表
            m_showList = showList;
            m_isAlwaysShow = showList == null || showList.Length == 0;
        }

        public override void Apply(MaterialProperty prop)
        {
            base.Apply(prop);
            //设置初始KeyWorld
            if (prop.type == MaterialProperty.PropType.Float)
            {
                bool foldoutEditor = prop.floatValue != 0 ? true : false;
                SetFoldoutEditorKeyword(prop, foldoutEditor);
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return -2;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            //折叠页检查
            m_simpleShaderGUI = editor.customShaderGUI as SimpleShaderGUI;
            if (m_simpleShaderGUI == null)
            {
                GUILayout.Label(prop.displayName + " :   Please use SimpleShaderGUI in your shader");
                return;
            }
            if (prop.type != MaterialProperty.PropType.Float)
            {
                GUILayout.Label(prop.displayName + " :   Property must be of type float");
                return;
            }
            if (!SimpleShaderGUI.IsFoldout(prop))
            {
                GUILayout.Label(prop.displayName + " :   Please add " + SimpleShaderGUI.FoldoutSign + " after displayName");
                return;
            }

            //如果该折叠页属于上个折叠页的内容，并且上个折叠页是折叠状态，则该折叠页不显示
            if (m_foldoutLevel > m_simpleShaderGUI.FoldoutLevel && !m_simpleShaderGUI.FoldoutOpen)
                return;

            //检查该折叠页是否要被绘制，
            if (!(m_isAlwaysShow || m_simpleShaderGUI.WhetherShowThisControl(m_showList)))
            {
                m_simpleShaderGUI.SetFoldout(m_foldoutLevel, m_simpleShaderGUI.FoldoutLevel_Editor, false, m_simpleShaderGUI.FoldoutEditor);
                return;
            }

            //折叠页复选框状态， 非0勾选, 0 不勾选
            m_foldoutEditor = prop.floatValue != 0 ? true : false;
            m_property = prop;

            //绘制折叠页
            FoldoutGUIDraw();

            //计算该折叠页属性实际的禁用状态
            int actual_foldoutEditorLevel = m_simpleShaderGUI.FoldoutLevel_Editor;
            bool actual_foldoutEditor = m_simpleShaderGUI.FoldoutEditor;

            //如果记录的折叠页是启用，该折叠页是禁用，则记录该折叠页等级和状态
            bool state1 = m_simpleShaderGUI.FoldoutEditor && !m_foldoutEditor;
            //如果记录的折叠页是禁用，该折叠页是启用，并且该折叠页不属于记录的折叠页中，则记录该折叠页等级和状态
            bool state2 = !m_simpleShaderGUI.FoldoutEditor && m_foldoutEditor && m_foldoutLevel <= m_simpleShaderGUI.FoldoutLevel_Editor;
            if (state1 || state2)
            {
                actual_foldoutEditorLevel = m_foldoutLevel;
                actual_foldoutEditor = m_foldoutEditor;
            }
            //设置折叠页
            m_simpleShaderGUI.SetFoldout(m_foldoutLevel, actual_foldoutEditorLevel, m_foldoutOpen, actual_foldoutEditor);

            //当更改属性名时，unity不会调用Apply函数进行初始化(只会调用构造函数), 这里每次渲染时都来设置关键字,这或许是unity的bug?...因为在官方的[Toggle]也会出现这种问题(当修改属性名时不会初始化构造函数)
            //即便如此，OnGUI函数只有在绘制时才会被调用，所以如果该属性不会被绘制时(被折叠或不显示)，同时在shader里修改他的名字,同样不会设置关键字，一般不会有这种操作
            if (!m_property.hasMixedValue)
                SetFoldoutEditorKeyword(m_property, m_foldoutEditor);
        }

        //折叠页绘制
        void FoldoutGUIDraw()
        {
            switch (m_foldoutStyle)
            {
                case FoldoutStyle.Big: FoldoutGUIDraw_Shuriken(30, 3); break;
                case FoldoutStyle.Median: FoldoutGUIDraw_Shuriken(25, 2); break;
                case FoldoutStyle.Small: FoldoutGUIDraw_Small(); break;
            }
        }

        //折叠页绘制 大中折叠页 , 返回折叠页复选框状态
        void FoldoutGUIDraw_Shuriken(float height, int fontSize)
        {
            //如果记录的折叠页是禁用，并且该折叠页属于他，则禁用该折叠页
            if (m_foldoutLevel > m_simpleShaderGUI.FoldoutLevel_Editor && !m_simpleShaderGUI.FoldoutEditor)
                EditorGUI.BeginDisabledGroup(true);

            GUIStyle style = new GUIStyle("ShurikenModuleTitle");//获取折叠页背景样式
            style.border = new RectOffset(15, 7, 4, 4);
            style.font = EditorStyles.boldLabel.font;
            style.fontStyle = EditorStyles.boldLabel.fontStyle;
            style.fontSize = EditorStyles.boldLabel.fontSize + fontSize;
            style.fixedHeight = height;
            style.contentOffset = new Vector2(20f, -1);//折叠页文本偏移
            if (m_foldoutToggleDraw)
                style.contentOffset += new Vector2(18f, 0); //如果绘制复选框，文本向后偏移

            Rect rect = GUILayoutUtility.GetRect(0, height, style);
            rect.xMin += (m_foldoutLevel - 1) * m_foldoutIndent;
            GUI.Box(rect, SimpleShaderGUI.GetFoldoutDisplayName(m_property), style);//绘制折叠页外观

            Rect triangleRect = new Rect(rect.x + 4, rect.y + rect.height / 2 - 7, 14f, 14f);//创建三角形外观矩形
            Event e = Event.current;
            //绘制折叠三角形外观
            if (e.type == EventType.Repaint)
                EditorStyles.foldout.Draw(triangleRect, false, false, m_foldoutOpen, false);

            //复选框绘制
            Rect toggleRect = new Rect(triangleRect.x + 16, triangleRect.y - 1, 14f, 14f);//创建复选框矩形
            if (m_foldoutToggleDraw)
            {
                EditorGUI.BeginChangeCheck();
                if (m_property.hasMixedValue)
                    m_foldoutEditor = GUI.Toggle(toggleRect, false, "", new GUIStyle("ToggleMixed"));
                else
                    m_foldoutEditor = GUI.Toggle(toggleRect, m_foldoutEditor, "");
                if (EditorGUI.EndChangeCheck())
                {
                    m_property.floatValue = m_foldoutEditor ? 1 : 0;
                    //设置关键字
                    SetFoldoutEditorKeyword(m_property, m_foldoutEditor);
                }
            }

            EditorGUI.EndDisabledGroup();

            //鼠标点击事件处理
            if (e.type == EventType.MouseDown)//在折叠页内点击，进行切换
            {
                //当在折叠框内点击时，切换折叠状态
                if (rect.Contains(e.mousePosition) && !(m_foldoutToggleDraw && toggleRect.Contains(e.mousePosition)))
                {
                    m_foldoutOpen = !m_foldoutOpen;
                    e.Use();//标记该事件已被使用
                }
            }
        }

        //绘制小的折叠页
        void FoldoutGUIDraw_Small()
        {
            //如果记录的折叠页是禁用，并且该折叠页属于他，则禁用该折叠页
            if (m_foldoutLevel > m_simpleShaderGUI.FoldoutLevel_Editor && !m_simpleShaderGUI.FoldoutEditor)
                EditorGUI.BeginDisabledGroup(true);

            Rect rect = GUILayoutUtility.GetRect(0, 25, EditorStyles.foldout);
            rect.xMin += (m_foldoutLevel - 1) * m_foldoutIndent;
            Event e = Event.current;
            if (e.type == EventType.Repaint)
                EditorStyles.foldout.Draw(rect, SimpleShaderGUI.GetFoldoutDisplayName(m_property), false, false, m_foldoutOpen, false);

            EditorGUI.EndDisabledGroup();

            //鼠标点击事件处理
            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                m_foldoutOpen = !m_foldoutOpen;
                e.Use();
            }
        }

        //设置折叠页编辑 关键字
        void SetFoldoutEditorKeyword(MaterialProperty pro, bool foldoutEditor)
        {
            //设置正常关键字
            string keyword = pro.name.ToUpperInvariant() + "_ON";
            foreach (Material m in pro.targets)
            {
                if (foldoutEditor)
                    m.EnableKeyword(keyword);
                else
                    m.DisableKeyword(keyword);
            }
        }
    }

    //跳出折叠页，可以使下面的属性跳出到任意等级折叠页
    //原理是和绘制折叠页一样，但是他不会进行绘制，而会更改SimpleShaderGUI.FoldoutLevel来达到跳出折叠页的目的
    public class Foldout_Out : MaterialPropertyDrawer
    {
        int m_foldoutLevel = 1; //退出到哪个等级中，如果退出到1级，他将和1级折叠页并起
        SimpleShaderGUI m_simpleShaderGUI; //绘制折叠页的ShaderGUI

        //默认跳出等级为1
        public Foldout_Out() : this(1) { }

        public Foldout_Out(float foldoutLevel)
        {
            int level = (int)foldoutLevel - 1;
            m_foldoutLevel = level < 0 ? 0 : level;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return -2;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            //折叠页检查
            m_simpleShaderGUI = editor.customShaderGUI as SimpleShaderGUI;
            if (m_simpleShaderGUI == null)
            {
                GUILayout.Label(prop.displayName + " :   Please use SimpleShaderGUI in your shader");
                return;
            }
            if (!SimpleShaderGUI.IsFoldout(prop))
            {
                GUILayout.Label(prop.displayName + " :   Please add " + SimpleShaderGUI.FoldoutSign + " after displayName");
                return;
            }
            //如果该折叠页属于上个折叠页的内容，并且上个折叠页是折叠状态，则该折叠页不显示
            if (m_foldoutLevel >= m_simpleShaderGUI.FoldoutLevel && !m_simpleShaderGUI.FoldoutOpen)
                return;

            //计算该折叠页属性实际的禁用状态
            int actual_foldoutEditorLevel = m_simpleShaderGUI.FoldoutLevel_Editor;
            bool actual_foldoutEditor = m_simpleShaderGUI.FoldoutEditor;

            //如果记录的折叠页是禁用，该折叠页是启用，并且该折叠页不属于记录的折叠页中，则记录该折叠页等级和状态
            bool state2 = !m_simpleShaderGUI.FoldoutEditor && m_foldoutLevel < m_simpleShaderGUI.FoldoutLevel_Editor;
            if (state2)
            {
                actual_foldoutEditorLevel = m_foldoutLevel;
                actual_foldoutEditor = true;
            }
            //设置折叠页
            m_simpleShaderGUI.SetFoldout(m_foldoutLevel, actual_foldoutEditorLevel, true, actual_foldoutEditor);
        }






    }
}
