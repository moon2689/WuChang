using System.Collections;
using System.Collections.Generic;
using Saber.Frame;
using UnityEngine;
using YooAsset;

namespace Saber.World
{
    public class ScenePointPortal : ScenePoint
    {
        public int m_TargetSceneID;
        public string m_TargetSceneName;
        public int m_TargetPortalID;

        public Portal PortalObj { get; set; }


        public override EScenePointType m_PointType => EScenePointType.Portal;
        protected override string GizmosLabel => $"传送门{m_ID} {m_PointName}";

        public Vector3 GetPortalFixedPos(out Quaternion rot)
        {
            Vector3 pos = transform.position + transform.forward * 0.8f;
            rot = Quaternion.LookRotation(transform.forward);
            return GetFixedPos(pos);
        }

        public override AssetHandle Load(Transform parent, BigWorld bigWorld)
        {
            if (PortalObj != null)
            {
                return null;
            }

            AssetHandle assetHandle = GameApp.Entry.Asset.LoadGameObject("SceneProp/Portal", go =>
            {
                go.name = m_ID.ToString();
                Portal portal = go.GetComponent<Portal>();
                portal.Init(this, parent, bigWorld);
                PortalObj = portal;
            });
            return assetHandle;
        }

        public override void SetActive(bool active)
        {
            base.SetActive(active);
            if (PortalObj)
            {
                PortalObj.gameObject.SetActive(active);
            }
        }
    }
}