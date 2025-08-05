using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//该脚本主要是针对控件切换的处理
namespace TATools
{
    //切换按钮 控制显示
    public class Toggle_SwitchDrawer : MaterialPropertyDrawer
    {
        bool m_enbale = true; //该Toggle是否勾选
        SimpleShaderGUI m_simpleShaderGUI; //绘制折叠页的ShaderGUI
        string[] m_showList = new string[0]; //该控件会在以下选项中显示

        public Toggle_SwitchDrawer() { }
        public Toggle_SwitchDrawer(params string[] showList)
        {
            m_showList = showList;
        }

        public override void Apply(MaterialProperty prop)
        {
            base.Apply(prop);

            //初始化关键字,并设置列表
            if (prop.type == MaterialProperty.PropType.Float)
            {
                m_enbale = (int)prop.floatValue == 0 ? false : true;
                SetToggleKeyword(prop, m_enbale);
                SetToggleSwitch(prop, m_enbale);
            }
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            //检查
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

            if (!m_simpleShaderGUI.WhetherShowThisControl(m_showList, true))
            {
                return;
            }

            EditorGUI.showMixedValue = prop.hasMixedValue;
            EditorGUI.BeginChangeCheck();
            m_enbale = EditorGUI.Toggle(position, label, (int)prop.floatValue == 0 ? false : true);
            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = m_enbale ? 1 : 0;
                SetToggleKeyword(prop, m_enbale);
            }
            EditorGUI.showMixedValue = false;

            if (!prop.hasMixedValue)
                SetToggleKeyword(prop, m_enbale);
            SetToggleSwitch(prop, m_enbale);
        }

        //设置Toggle 关键字
        void SetToggleKeyword(MaterialProperty pro, bool enable)
        {
            //设置Keyword
            string keyword = pro.name.ToUpperInvariant() + "_ON";
            foreach (Material m in pro.targets)
            {
                if (enable)
                    m.EnableKeyword(keyword);
                else
                    m.DisableKeyword(keyword);
            }
        }

        //设置switch列表
        void SetToggleSwitch(MaterialProperty pro, bool enable)
        {
            if (m_simpleShaderGUI == null)
                return;

            if (enable)
            {
                if (!m_simpleShaderGUI.SwitchList.Contains(pro.name))
                    m_simpleShaderGUI.SwitchList.Add(pro.name);
            }
            else
            {
                m_simpleShaderGUI.SwitchList.Remove(pro.name);
            }
        }
    }

    //菜单按钮控制显示
    public class Enum_SwitchDrawer : MaterialPropertyDrawer
    {
        string[] m_enumList = new string[0]; //菜单栏列表
        int[] m_enumValue = new int[0]; //菜单栏数值
        int m_optionIndex; //当前选择的选项
        SimpleShaderGUI m_simpleShaderGUI; //绘制折叠页的ShaderGUI
        string[] m_showList = new string[0]; //该控件会在以下选项中显示

        public Enum_SwitchDrawer(params string[] array)
        {
            List<string> listEnumList = new();
            List<string> listShowList = new();
            foreach (string item in array)
            {
                if (item.StartsWith("enum_"))
                    listEnumList.Add(item.Substring(5));
                else
                    listShowList.Add(item);
            }

            m_enumList = listEnumList.ToArray();
            m_enumValue = new int[m_enumList.Length];
            for (int i = 0; i < m_enumValue.Length; i++)
                m_enumValue[i] = i;

            m_showList = listShowList.ToArray();
        }

        public override void Apply(MaterialProperty prop)
        {
            base.Apply(prop);

            //初始化关键字,并设置列表
            if (prop.type == MaterialProperty.PropType.Float)
            {
                ApplyEnumData(prop);
                SetEnumKeyword(prop);
                SetEnumSwitch(prop);
            }
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            //检查
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

            if (!m_simpleShaderGUI.WhetherShowThisControl(m_showList, true))
            {
                return;
            }

            ApplyEnumData(prop);

            EditorGUI.showMixedValue = prop.hasMixedValue;
            EditorGUI.BeginChangeCheck();
            m_optionIndex = EditorGUI.IntPopup(position, label, m_optionIndex, m_enumList, m_enumValue);
            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = m_optionIndex;
                SetEnumKeyword(prop);
            }
            EditorGUI.showMixedValue = false;

            if (!prop.hasMixedValue)
                SetEnumKeyword(prop);
            SetEnumSwitch(prop);
        }


        //初始化枚举数据
        void ApplyEnumData(MaterialProperty prop)
        {
            int index = (int)prop.floatValue;
            index = Mathf.Clamp(index, 0, m_enumList.Length - 1);
            // prop.floatValue = index;
            m_optionIndex = index;
        }

        //设置菜单栏的关键字
        void SetEnumKeyword(MaterialProperty pro)
        {
            //设置Keyword
            foreach (Material m in pro.targets)
            {
                foreach (string options in m_enumList)
                {
                    string keyword = (pro.name + "_" + options).ToUpperInvariant();
                    if (options == m_enumList[m_optionIndex])
                        m.EnableKeyword(keyword);
                    else
                        m.DisableKeyword(keyword);
                }
            }
        }

        //设置switch列表
        void SetEnumSwitch(MaterialProperty pro)
        {
            if (m_simpleShaderGUI == null)
                return;

            foreach (string options in m_enumList)
            {
                if (options == m_enumList[m_optionIndex])
                {
                    if (!m_simpleShaderGUI.SwitchList.Contains(options))
                        m_simpleShaderGUI.SwitchList.Add(options);
                }
                else
                {
                    m_simpleShaderGUI.SwitchList.Remove(options);
                }
            }

        }
    }

    //控件显示控制
    public abstract class SwitchBaseDrawer : MaterialPropertyDrawer
    {
        string[] m_showList = new string[0]; //该控件会在以下选项中显示
        SimpleShaderGUI m_simpleShaderGUI; //绘制折叠页的ShaderGUI

        protected abstract bool AllSatisfy { get; }


        //填写任意数量的选项，当其中一个选项被选中时，该控件会被渲染
        public SwitchBaseDrawer() { }
        public SwitchBaseDrawer(params string[] showList)
        {
            m_showList = showList;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return -1.5f;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            //检查
            m_simpleShaderGUI = editor.customShaderGUI as SimpleShaderGUI;
            if (m_simpleShaderGUI == null)
            {
                GUILayout.Label(prop.displayName + " :   Please use SimpleShaderGUI in your shader");
                return;
            }

            if (m_simpleShaderGUI.WhetherShowThisControl(m_showList, AllSatisfy))
            {
                editor.DefaultShaderProperty(prop, label);
            }
        }
    }

    //控件显示控制
    public class SwitchOrDrawer : SwitchBaseDrawer
    {
        protected override bool AllSatisfy => false;

        public SwitchOrDrawer() : base()
        {

        }

        public SwitchOrDrawer(params string[] showList) : base(showList)
        {

        }
    }

    //控件显示控制
    public class SwitchAndDrawer : SwitchBaseDrawer
    {
        protected override bool AllSatisfy => true;

        public SwitchAndDrawer() : base()
        {

        }

        public SwitchAndDrawer(params string[] showList) : base(showList)
        {

        }
    }

    public class SwitchNotDrawer : MaterialPropertyDrawer
    {
        string[] m_showList = new string[0]; //该控件会在以下选项中显示
        SimpleShaderGUI m_simpleShaderGUI; //绘制折叠页的ShaderGUI


        //填写任意数量的选项，当其中一个选项被选中时，该控件会被渲染
        public SwitchNotDrawer() { }
        public SwitchNotDrawer(params string[] showList)
        {
            m_showList = showList;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return -1.5f;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            //检查
            m_simpleShaderGUI = editor.customShaderGUI as SimpleShaderGUI;
            if (m_simpleShaderGUI == null)
            {
                GUILayout.Label(prop.displayName + " :   Please use SimpleShaderGUI in your shader");
                return;
            }

            if (!m_simpleShaderGUI.WhetherShowThisControl(m_showList, true))
            {
                editor.DefaultShaderProperty(prop, label);
            }
        }
    }

}