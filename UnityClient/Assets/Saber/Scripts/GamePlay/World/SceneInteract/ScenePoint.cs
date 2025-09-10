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
        public string m_PointName;
        public EAIType m_AIType;
        public int m_TargetSceneID;
        public string m_TargetSceneName;
        public int m_TargetPortalID;


        public SActor Actor { get; set; }
        public Portal PortalObj { get; set; }
        public Idol IdolObj { get; set; }


        public Vector3 GetFixedBornPos(out Quaternion rot)
        {
            rot = transform.rotation;
            return GetFixedPos(transform.position);
        }

        Vector3 GetFixedPos(Vector3 originPos)
        {
            int groundLayer = EStaticLayers.Default.GetLayerMask();
            if (Physics.Raycast(originPos, Vector3.down, out var hitInfo, 200, groundLayer, QueryTriggerInteraction.Ignore))
            {
                return hitInfo.point;
            }
            else
            {
                //Debug.LogError($"Born position is error, id:{m_ID}");
                return originPos;
            }
        }

        public Vector3 GetIdolFixedPos(out Quaternion rot)
        {
            Vector3 pos = transform.position + transform.forward * 1.5f;
            rot = Quaternion.LookRotation(-transform.forward);
            return GetFixedPos(pos);
        }

        public Vector3 GetPortalFixedPos(out Quaternion rot)
        {
            Vector3 pos = transform.position + transform.forward * 0.8f;
            rot = Quaternion.LookRotation(transform.forward);
            return GetFixedPos(pos);
        }

#if UNITY_EDITOR
        string GetLabel()
        {
            string startStr = m_PointType switch
            {
                EScenePointType.Idol => $"神像{m_ID}",
                EScenePointType.Portal => $"传送门{m_ID}",
                EScenePointType.PlayerBornPosition => "玩家",
                EScenePointType.MonsterBornPosition => $"敌人{m_ID}",
                _ => "???",
            };
            string label = !string.IsNullOrEmpty(m_PointName) ? $"{startStr} {m_PointName}" : $"{startStr} ???";
            return label;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawCube(transform.position + new Vector3(0, 0.5f), new Vector3(0.5f, 1, 0.5f));
            Gizmos.DrawIcon(transform.position, "qizi.png");

            GUIStyle styleText = new GUIStyle()
            {
                normal = { textColor = Color.green },
                fontSize = 10,
            };
            string label = GetLabel();
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