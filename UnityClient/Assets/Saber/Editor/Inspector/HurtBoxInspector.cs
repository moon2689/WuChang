using System.Collections;
using System.Collections.Generic;
using Saber.CharacterController;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HurtBox))]
public class HurtBoxInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("ReName"))
        {
            HurtBox hurtBox = (HurtBox)base.target;
            hurtBox.transform.name = $"HurtBox_{hurtBox.transform.parent.name}";
        }
    }
}