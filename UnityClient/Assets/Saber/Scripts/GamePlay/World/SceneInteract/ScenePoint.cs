using System;
using System.Collections;
using System.Collections.Generic;
using Saber.AI;
using Saber.CharacterController;
using UnityEngine;
using UnityEngine.Serialization;
using YooAsset;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Saber.World
{
    public abstract class ScenePoint : MonoBehaviour
    {
        public int m_ID;
        public string m_PointName;

        protected abstract string GizmosLabel { get; }

        public abstract EScenePointType m_PointType { get; }

        public Vector3 GetFixedBornPos(out Quaternion rot)
        {
            rot = transform.rotation;
            return GetFixedPos(transform.position);
        }

        protected Vector3 GetFixedPos(Vector3 originPos)
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

        public virtual AssetHandle Load(Transform parent, BigWorld bigWorld)
        {
            return null;
        }

        public virtual void Destroy()
        {
        }

        public virtual void SetActive(bool active)
        {
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.DrawCube(transform.position + new Vector3(0, 0.5f), new Vector3(0.5f, 1, 0.5f));
            Gizmos.DrawIcon(transform.position, "qizi.png");

            GUIStyle styleText = new GUIStyle()
            {
                normal = { textColor = Color.green },
                fontSize = 10,
            };
            string label = GizmosLabel;
            Handles.Label(transform.position + new Vector3(0, 0.5f), label, styleText);
        }
#endif
    }

    public enum EScenePointType
    {
        PlayerBornPosition,
        MonsterBornPosition,
        ShenKan,
        Portal,
    }
}