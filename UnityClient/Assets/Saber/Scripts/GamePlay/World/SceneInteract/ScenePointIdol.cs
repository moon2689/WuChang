using System.Collections;
using System.Collections.Generic;
using Saber.Frame;
using UnityEngine;
using YooAsset;

namespace Saber.World
{
    public class ScenePointIdol : ScenePoint
    {
        [SerializeField] private ScenePointMonster[] m_LinkMonsterPoints;

        public Idol IdolObj { get; set; }

        public override EScenePointType m_PointType => EScenePointType.Idol;
        protected override string GizmosLabel => $"神像{m_ID} {m_PointName}";
        public ScenePointMonster[] LinkMonsterPoints => m_LinkMonsterPoints;


        public Vector3 GetIdolFixedPos(out Quaternion rot)
        {
            Vector3 pos = transform.position + transform.forward * 1.5f;
            rot = Quaternion.LookRotation(-transform.forward);
            return GetFixedPos(pos);
        }

        public override AssetHandle Load(Transform parent, BigWorld bigWorld)
        {
            AssetHandle assetHandle = GameApp.Entry.Asset.LoadGameObject("SceneProp/GodStatue", godStatueObj =>
            {
                godStatueObj.name = m_ID.ToString();
                Idol idol = godStatueObj.GetComponent<Idol>();
                idol.Init(bigWorld.SceneInfo.m_ID, this, parent, bigWorld);
                IdolObj = idol;
            });
            return assetHandle;
        }

        public override void SetActive(bool active)
        {
            base.SetActive(active);
            if (IdolObj)
            {
                IdolObj.gameObject.SetActive(active);
            }
        }
    }
}