using UnityEditor;
using UnityEngine;

//该脚本用于纹理的绘制
namespace TATools
{
    public class TexDrawer : MaterialPropertyDrawer
    {
        string[] m_showList; //该控件会在以下选项中显示
        SimpleShaderGUI m_simpleShaderGUI; //绘制折叠页的ShaderGUI

        //填写任意数量的选项，当其中一个选项被选中时，该控件会被渲染
        public TexDrawer()
        {
            m_showList = null;
        }

        public TexDrawer(params string[] showList)
        {
            m_showList = showList;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            m_simpleShaderGUI = editor.customShaderGUI as SimpleShaderGUI;
            if (m_simpleShaderGUI == null)
                return base.GetPropertyHeight(prop, label, editor);
            //检查属性是否要被绘制
            if (!m_simpleShaderGUI.WhetherShowThisControl(m_showList))
                return -2f;

            return base.GetPropertyHeight(prop, label, editor);
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            //检查
            m_simpleShaderGUI = editor.customShaderGUI as SimpleShaderGUI;
            if (m_simpleShaderGUI == null)
            {
                GUILayout.Label(prop.displayName + " :   Please use SimpleShaderGUI in your shader");
                return;
            }
            if (prop.type != MaterialProperty.PropType.Texture)
            {
                GUILayout.Label(prop.displayName + " :   Property must be of type texture");
                return;
            }

            //检查属性是否要被绘制
            if (!m_simpleShaderGUI.WhetherShowThisControl(m_showList))
                return;

            //绘制单行纹理
            editor.TexturePropertyMiniThumbnail(position, prop, label.text, "");

            //在纹理后面绘制纹理缩放控件
            if (prop.flags != MaterialProperty.PropFlags.NoScaleOffset)
            {
                EditorGUI.indentLevel++;
                EditorGUI.showMixedValue = prop.hasMixedValue;
                editor.TextureScaleOffsetProperty(prop);
                EditorGUI.showMixedValue = false;
                EditorGUI.indentLevel--;
            }
        }



    }
}
