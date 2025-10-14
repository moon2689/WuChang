#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using UIAnimation;
using System;
using System.Collections.Generic;

namespace UIAnimation.Editor
{
    [CustomEditor(typeof(Tween.TweenTransformMoveTo))]
    public class TweenTransformMoveToInspector : UnityEditor.Editor
    {
        private Tween.TweenTransformMoveTo tw;
        private Transform toTransform;

        void OnEnable()
        {
            tw = target as Tween.TweenTransformMoveTo;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            TweenActionCommonInspector.DrawTweenActionBase(tw);

            DrawTweenMoveTo();

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }

        private void DrawTweenMoveTo()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(274f));
            {
                EditorGUIUtility.labelWidth = 90f;
                toTransform = EditorGUILayout.ObjectField("To Transform", toTransform, typeof(Transform), true) as Transform;
                if (toTransform != null)
                {
                    tw.ToPosition = toTransform.localPosition;
                }

                tw.ToPosition = EditorGUILayout.Vector3Field("To Position", tw.ToPosition);
            }
            EditorGUILayout.EndVertical();
        }
    }
}
#endif