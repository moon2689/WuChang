using UnityEngine;
using UnityEditor;

public class SimpleTransparencyShaderGUI : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        // 渲染默认属性
        materialEditor.PropertiesDefaultGUI(properties);
        
        EditorGUILayout.Space();
        
        // 简单的透明度切换
        Material material = materialEditor.target as Material;
        
        EditorGUI.BeginChangeCheck();
        bool isTransparent = EditorGUILayout.Toggle("Enable Transparency", IsMaterialTransparent(material));
        
        if (EditorGUI.EndChangeCheck())
        {
            SetMaterialTransparency(material, isTransparent);
        }
    }

    private bool IsMaterialTransparent(Material material)
    {
        return material.renderQueue == (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }

    private void SetMaterialTransparency(Material material, bool transparent)
    {
        if (transparent)
        {
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            material.SetOverrideTag("RenderType", "Transparent");
        }
        else
        {
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt("_ZWrite", 1);
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
            material.SetOverrideTag("RenderType", "Opaque");
        }
    }
}