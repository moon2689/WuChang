#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using UIAnimation;
using System;
using System.Collections.Generic;

namespace UIAnimation.Editor 
{
    [CustomEditor(typeof(Tween.TweenCubicBezier))]
    public class TweenCubicBezierInspector : UnityEditor.Editor
    {
        private Tween.TweenCubicBezier tw;
        
        void OnEnable()
        {
            tw = target as Tween.TweenCubicBezier;
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
                tw.EndPosition = EditorGUILayout.Vector3Field("End Position", tw.EndPosition);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(274f));
            {
                EditorGUIUtility.labelWidth = 90f;
                tw.ControlPosition1 = EditorGUILayout.Vector3Field("Control Position 1", tw.ControlPosition1);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(274f));
            {
                EditorGUIUtility.labelWidth = 90f;
                tw.ControlPosition2 = EditorGUILayout.Vector3Field("Control Position 2", tw.ControlPosition2);
            }
            EditorGUILayout.EndVertical();
        }
    }
}
#endif