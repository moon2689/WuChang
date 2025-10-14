#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using UIAnimation;
using System;
using System.Collections.Generic;

namespace UIAnimation.Editor
{
    [CustomEditor(typeof(Tween.TweenTransformScaleTo))]
    public class TweenTransformScaleToInspector : UnityEditor.Editor
    {
        private Tween.TweenTransformScaleTo tw;

        void OnEnable()
        {
            tw = target as Tween.TweenTransformScaleTo;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            TweenActionCommonInspector.DrawTweenActionBase(tw);

            DrawTweenScale();

            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }

        private void DrawTweenScale()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(250f));
            {
                EditorGUIUtility.labelWidth = 96f;

                EditorGUILayout.BeginHorizontal();
                {
                    tw.ToScaleX = EditorGUILayout.FloatField("X To Scale", tw.ToScaleX);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    tw.ToScaleY = EditorGUILayout.FloatField("Y To Scale", tw.ToScaleY);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    tw.ToScaleZ = EditorGUILayout.FloatField("Z To Scale", tw.ToScaleZ);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    tw.Pivot = EditorGUILayout.Vector2Field("Pivot", tw.Pivot);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
    }
}
#endif