#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using UIAnimation;
using System;
using System.Collections.Generic;

namespace UIAnimation.Editor 
{
    public static class TweenActionCommonInspector
    {    
        public static void DrawTweenActionBase(Actions.TweenActionBase tw)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(250f));
            {
                EditorGUILayout.BeginHorizontal(GUILayout.Height(80f));
                {
                    EditorGUILayout.BeginVertical(GUILayout.Width(200f));
                    {
                        EditorGUIUtility.labelWidth = 90f;
                        tw.Description = EditorGUILayout.TextField("Description", tw.Description);                    
                        tw.WrapMode = (Actions.TweenActionBase.AnimationWrapMode)EditorGUILayout.EnumPopup("Wrap Mode", tw.WrapMode);                    
                        tw.DelaySeconds = EditorGUILayout.FloatField("Delay Seconds", tw.DelaySeconds);                    
                        tw.DurationSeconds = EditorGUILayout.FloatField("Duration in Sec", tw.DurationSeconds);
                        
                    }
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.BeginVertical(GUILayout.Width(60f), GUILayout.Height(70f));
                    {
                        tw.Curve = EditorGUILayout.CurveField(tw.Curve, GUILayout.Width(50f), GUILayout.Height(50f));
                        EditorGUIUtility.labelWidth = 50f;
                        EditorGUILayout.PrefixLabel("Curve");
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUIUtility.fieldWidth = 30f;
                tw.Speed = EditorGUILayout.Slider("Speed", tw.Speed, -20f, 20f);
            }
            EditorGUILayout.EndVertical();
        }
        
    }
    
}
#endif