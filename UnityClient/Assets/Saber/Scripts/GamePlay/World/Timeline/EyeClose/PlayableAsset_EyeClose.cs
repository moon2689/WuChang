using UnityEngine;
using UnityEngine.Playables;

namespace Saber.Timeline
{
    public class PlayableAsset_EyeClose : PlayableAsset
    {
        [SerializeField] private bool m_CloseEye;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<PlayableBehaviour_EyeClose>.Create(graph);

            var behaviour = playable.GetBehaviour();
            behaviour.CloseEye = m_CloseEye;
            return playable;
        }
    }
}