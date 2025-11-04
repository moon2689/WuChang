using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

 namespace CombatEditor
{	
	[CustomEditor(typeof(AbilityEventObj_SFX))]
	public class AbilityEventObj_SFXEditor : Editor
	{
	    public override void OnInspectorGUI()
	    {
	        serializedObject.Update();
	        EditorGUI.BeginChangeCheck();
	        EditorGUI.BeginDisabledGroup(true);
	        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
	        EditorGUI.EndDisabledGroup();
	
	        EditorGUILayout.PropertyField(serializedObject.FindProperty("clips"));
	        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Volume"));
	        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TriggerProbability"));
	        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FollowAnim"));
	
	        if (EditorGUI.EndChangeCheck())
	        {
	            if (CombatEditorUtility.EditorExist())
	            {
	                CombatEditorUtility.GetCurrentEditor().RequirePreviewReload();
	            }
	        }
	
	
	        serializedObject.ApplyModifiedProperties();
	    }
	}
}
