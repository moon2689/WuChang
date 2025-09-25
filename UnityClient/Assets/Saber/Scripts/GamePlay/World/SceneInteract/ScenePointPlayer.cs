using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Saber.World
{
    public class ScenePointPlayer : ScenePoint
    {
        public override EScenePointType m_PointType => EScenePointType.PlayerBornPosition;
        protected override string GizmosLabel => $"玩家 {m_PointName}";
    }
}
