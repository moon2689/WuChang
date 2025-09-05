using System;
using System.Collections;
using System.Collections.Generic;
using Saber.AI;
using Saber.CharacterController;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Saber.World
{
    public class ScenePoint : MonoBehaviour
    {
        public EScenePointType m_PointType;
        public int m_ID;
        public string m_Name;
        public EAIType m_AIType;
        public int m_TargetSceneID;
        public int m_TargetPortalID;


        public SActor Actor { get; set; }
        public Portal PortalObj { get; set; }
        public Idol IdolObj { get; set; }


        public Vector3 GetFixedBornPos()
        {
            Vector3 rayOriginPos = transform.position + Vector3.up * 100;
            if (Physics.Raycast(rayOriginPos, Vector3.down, out var hitInfo, 200, EStaticLayers.Default.GetLayerMask()))
            {
                return hitInfo.point;
            }
            else
            {
                Debug.LogError($"Born position is error, id:{m_ID}");
                return transform.position + Vector3.up * 10;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.DrawCube(transform.position, new Vector3(0.5f, 1, 0.5f));
            Gizmos.DrawIcon(transform.position, "qizi.png");

            GUIStyle styleText = new GUIStyle()
            {
                normal = { textColor = Color.green },
                fontSize = 10,
            };
            string label = !string.IsNullOrEmpty(m_Name) ? $"{m_Name} {m_ID}" : $"??? {m_ID}";
            Handles.Label(transform.position + new Vector3(0, 0.5f), label, styleText);
        }
#endif
    }

    public enum EScenePointType
    {
        PlayerBornPosition,
        MonsterBornPosition,
        Idol,
        Portal,
    }
}