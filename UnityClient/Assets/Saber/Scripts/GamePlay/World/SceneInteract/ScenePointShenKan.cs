using System.Collections;
using System.Collections.Generic;
using Saber.Frame;
using UnityEngine;
using YooAsset;

namespace Saber.World
{
    public class ScenePointShenKan : ScenePoint
    {
        [SerializeField] private ScenePointMonster[] m_LinkMonsterPoints;

        public ShenKan ShenKanObj { get; set; }

        public override EScenePointType m_PointType => EScenePointType.ShenKan;
        protected override string GizmosLabel => $"神龛{m_ID} {m_PointName}";
        public ScenePointMonster[] LinkMonsterPoints => m_LinkMonsterPoints;


        public Vector3 GetShenKanFixedPos(out Quaternion rot)
        {
            Vector3 pos = transform.position + transform.forward * 1.5f;
            rot = Quaternion.LookRotation(-transform.forward);
            return GetFixedPos(pos);
        }

        public override AssetHandle Load(Transform parent, BigWorld bigWorld)
        {
            if (ShenKanObj)
            {
                return null;
            }

            AssetHandle assetHandle = GameApp.Entry.Asset.LoadGameObject("SceneProp/ShenKan", shenKanObj =>
            {
                shenKanObj.name = m_ID.ToString();
                ShenKan shenKan = shenKanObj.GetComponent<ShenKan>();
                shenKan.Init(bigWorld.SceneInfo.m_ID, this, parent, bigWorld);
                ShenKanObj = shenKan;
            });
            return assetHandle;
        }

        public override void SetActive(bool active)
        {
            base.SetActive(active);
            if (ShenKanObj)
            {
                ShenKanObj.gameObject.SetActive(active);
            }
        }
    }
}