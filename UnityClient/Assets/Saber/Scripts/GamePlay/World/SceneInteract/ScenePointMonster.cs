using System.Collections;
using System.Collections.Generic;
using Saber.AI;
using Saber.CharacterController;
using Saber.Frame;
using UnityEngine;
using YooAsset;

namespace Saber.World
{
    public class ScenePointMonster : ScenePoint
    {
        public EAIType m_AIType;

        public override EScenePointType m_PointType => EScenePointType.MonsterBornPosition;
        protected override string GizmosLabel => $"敌人{m_ID} {m_PointName}";

        public SActor Actor { get; set; }

        public override void Destroy()
        {
            if (Actor)
            {
                Actor.Destroy();
                Actor = null;
            }
        }

        public override AssetHandle Load(Transform parent, BigWorld bigWorld)
        {
            if (Actor)
            {
                return null;
            }

            EAIType aiType = GameApp.Entry.Config.TestGame.DebugFight ? GameApp.Entry.Config.TestGame.EnemyAI : m_AIType;
            EnemyAIBase ai = aiType.CreateEnemyAI();
            var camp = EActorCamp.Monster;
            Vector3 pos = GetFixedBornPos(out var rot);
            AssetHandle assetHandle = SActor.Create(m_ID, pos, rot, ai, camp, actor =>
            {
                Actor = actor;
                Actor.transform.SetParent(parent);
            });
            return assetHandle;
        }

        public override void SetActive(bool active)
        {
            base.SetActive(active);
            if (Actor)
            {
                Actor.gameObject.SetActive(active);
            }
        }

        public void RebirthActor()
        {
            Actor.RecoverOrigin();
            
            Vector3 pos = GetFixedBornPos(out var rot);
            Actor.transform.position = pos;
            Actor.transform.rotation = rot;
        }
    }
}