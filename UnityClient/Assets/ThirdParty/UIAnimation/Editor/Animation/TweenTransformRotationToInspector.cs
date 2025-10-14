#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using UIAnimation;
using System;
using System.Collections.Generic;

namespace UIAnimation.Editor 
{
    [CustomEditor(typeof(Tween.TweenTransformRotationTo))]
    public class TweenTransformRotationToInspector : UnityEditor.Editor 
    {
        private Tween.TweenTransformRotationTo tw;
        
        void OnEnable()
        {
            tw = target as Tween.TweenTransformRotationTo;
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            TweenActionCommonInspector.DrawTweenActionBase(tw);
            
            DrawTweenRotation();
            
            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
        
        private void DrawTweenRotation()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(274f));
            {
                EditorGUIUtility.labelWidth = 90f;
                tw.ToRotation = EditorGUILayout.Vector3Field("To Rotation", tw.ToRotation);
            }
            EditorGUILayout.EndVertical();
        }
        
    }
    
}
#endif