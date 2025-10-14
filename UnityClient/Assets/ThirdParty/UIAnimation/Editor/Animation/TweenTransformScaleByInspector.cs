#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using UIAnimation;
using System;
using System.Collections.Generic;

namespace UIAnimation.Editor 
{
    [CustomEditor(typeof(Tween.TweenTransformScaleBy))]
    public class TweenTransformScaleByInspector : UnityEditor.Editor 
    {
        private Tween.TweenTransformScaleBy tw;
        
        void OnEnable()
        {
            tw = target as Tween.TweenTransformScaleBy;
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
                    tw.DeltaScaleX = EditorGUILayout.FloatField("Delta X Scale", tw.DeltaScaleX);
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                {
                    tw.DeltaScaleY = EditorGUILayout.FloatField("Delta Y Scale", tw.DeltaScaleY);
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                {
                    tw.DeltaScaleZ = EditorGUILayout.FloatField("Delta Z Scale", tw.DeltaScaleZ);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
    }
    
}
#endif