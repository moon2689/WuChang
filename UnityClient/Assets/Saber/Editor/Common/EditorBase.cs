using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class EditorBase : Editor
{
    protected string m_TitleStyle;
    protected string m_RootGroupStyle;
    protected string m_SubGroupStyle;
    protected string m_FoldStyle;
    protected string m_AddButtonStyle;

    private bool m_UseNewGUI = true;

    protected abstract string TitleString { get; }
    protected abstract void DrawGUI();


    private void OnEnable()
    {
        m_TitleStyle = "MeTransOffRight";
        m_RootGroupStyle = "GroupBox";
        m_SubGroupStyle = "ObjectFieldThumb";
        m_FoldStyle = "Foldout";
        m_AddButtonStyle = "CN CountBadge";
    }

    public override void OnInspectorGUI()
    {
        m_UseNewGUI = GUILayout.Toggle(m_UseNewGUI, "使用新的GUI");
        if (m_UseNewGUI)
        {
            DrawTitle();
            DrawGUI();
        }
        else
        {
            base.OnInspectorGUI();
        }
    }

    void DrawTitle()
    {
        EditorGUILayout.BeginVertical(m_TitleStyle);
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUIStyle fontStyle = new GUIStyle();
                fontStyle.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Saber/Editor/Common/Res/BiLuoSiJianHeLuoQingSong-2.TTF");
                fontStyle.fontSize = 30;
                fontStyle.alignment = TextAnchor.UpperCenter;
                fontStyle.normal.textColor = Color.white;
                fontStyle.hover.textColor = Color.white;
                EditorGUILayout.LabelField("", TitleString, fontStyle, GUILayout.Height(32));
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }


    #region 数组操作

    protected void PaneOptions<T>(T[] elements, T element, System.Action<T[]> callback)
    {
        if (elements == null || elements.Length == 0)
            return;
        GenericMenu toolsMenu = new GenericMenu();

        if ((elements[0] != null && elements[0].Equals(element)) || (elements[0] == null && element == null) ||
            elements.Length == 1)
        {
            toolsMenu.AddDisabledItem(new GUIContent("上移"));
            toolsMenu.AddDisabledItem(new GUIContent("移到顶部"));
        }
        else
        {
            toolsMenu.AddItem(new GUIContent("上移"), false,
                delegate() { callback(MoveElement<T>(elements, element, -1)); });
            toolsMenu.AddItem(new GUIContent("移到顶部"), false,
                delegate() { callback(MoveElement<T>(elements, element, -elements.Length)); });
        }

        if ((elements[elements.Length - 1] != null && elements[elements.Length - 1].Equals(element)) ||
            elements.Length == 1)
        {
            toolsMenu.AddDisabledItem(new GUIContent("下移"));
            toolsMenu.AddDisabledItem(new GUIContent("移到底部"));
        }
        else
        {
            toolsMenu.AddItem(new GUIContent("下移"), false,
                delegate() { callback(MoveElement<T>(elements, element, 1)); });
            toolsMenu.AddItem(new GUIContent("移到底部"), false,
                delegate() { callback(MoveElement<T>(elements, element, elements.Length)); });
        }

        toolsMenu.AddSeparator("");

        if (element != null && element is System.ICloneable)
        {
            toolsMenu.AddItem(new GUIContent("复制"), false,
                delegate() { callback(CopyElement<T>(elements, element)); });
        }
        else
        {
            toolsMenu.AddDisabledItem(new GUIContent("复制"));
        }

        if (element != null && CloneObject.objCopy != null && CloneObject.objCopy.GetType() == typeof(T))
        {
            toolsMenu.AddItem(new GUIContent("粘贴"), false,
                delegate() { callback(PasteElement<T>(elements, element)); });
        }
        else
        {
            toolsMenu.AddDisabledItem(new GUIContent("粘贴"));
        }

        toolsMenu.AddSeparator("");

        if (!(element is System.ICloneable))
        {
            toolsMenu.AddDisabledItem(new GUIContent("拷贝"));
        }
        else
        {
            toolsMenu.AddItem(new GUIContent("拷贝"), false,
                delegate() { callback(DuplicateElement<T>(elements, element)); });
        }

        toolsMenu.AddItem(new GUIContent("移除"), false,
            delegate() { callback(RemoveElement<T>(elements, element)); });

        toolsMenu.ShowAsContext();
        EditorGUIUtility.ExitGUI();
    }

    public T[] RemoveElement<T>(T[] elements, T element)
    {
        List<T> elementsList = new List<T>(elements);
        elementsList.Remove(element);
        return elementsList.ToArray();
    }

    public List<T> RemoveElement<T>(List<T> elementsList, T element)
    {
        elementsList.Remove(element);
        return elementsList;
    }

    public T[] RemoveElementByIndex<T>(T[] elements, int index)
    {
        List<T> elementsList = new List<T>(elements);
        elementsList.RemoveAt(index);
        return elementsList.ToArray();
    }

    public T[] AddElement<T>(T[] elements, T element)
    {
        List<T> elementsList = new List<T>(elements);
        elementsList.Add(element);
        return elementsList.ToArray();
    }

    public T[] CopyElement<T>(T[] elements, T element)
    {
        CloneObject.objCopy = (object)(element as ICloneable).Clone();
        return elements;
    }

    public T[] PasteElement<T>(T[] elements, T element)
    {
        if (CloneObject.objCopy == null) return elements;
        List<T> elementsList = new List<T>(elements);
        elementsList.Insert(elementsList.IndexOf(element) + 1, (T)CloneObject.objCopy);
        //CloneObject.objCopy = null;
        return elementsList.ToArray();
    }

    public T[] DuplicateElement<T>(T[] elements, T element)
    {
        List<T> elementsList = new List<T>(elements);
        elementsList.Insert(elementsList.IndexOf(element) + 1, (T)(element as ICloneable).Clone());
        return elementsList.ToArray();
    }

    public T[] MoveElement<T>(T[] elements, T element, int steps)
    {
        List<T> elementsList = new List<T>(elements);
        int newIndex = Mathf.Clamp(elementsList.IndexOf(element) + steps, 0, elements.Length - 1);
        elementsList.Remove(element);
        elementsList.Insert(newIndex, element);
        return elementsList.ToArray();
    }

    protected bool StyledButton(string label)
    {
        EditorGUILayout.Space();
        GUILayoutUtility.GetRect(1, 20);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        bool clickResult = GUILayout.Button(label, m_AddButtonStyle);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        return clickResult;
    }

    #endregion
}