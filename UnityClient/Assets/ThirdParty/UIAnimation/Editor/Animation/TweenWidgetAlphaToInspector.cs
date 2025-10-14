#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace UIAnimation.Editor
{
    [CustomEditor(typeof(Tween.TweenUIAlphaTo))]
    public class TweenWidgetAlphaToInspector : UnityEditor.Editor
    {
        private Tween.TweenUIAlphaTo tw;

        void OnEnable()
        {
            tw = target as Tween.TweenUIAlphaTo;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            TweenActionCommonInspector.DrawTweenActionBase(tw);

            DrawTweenAlphaTo();

            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }

        private void DrawTweenAlphaTo()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(274f));
            {
                EditorGUIUtility.labelWidth = 90f;
                tw.ToAlpha = EditorGUILayout.Slider("To Alpha", tw.ToAlpha, 0f, 1f);
            }
            EditorGUILayout.EndVertical();
        }
    }
}
#endif