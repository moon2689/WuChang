using System;
using System.Collections;
using System.Collections.Generic;
using Saber.Frame;
using UnityEngine;
using UnityEngine.Playables;
using Saber.CharacterController;
using UnityEngine.Timeline;

namespace Saber.Timeline
{
    public class TimelineManager : MonoBehaviour
    {
        private List<SActor> m_Actors = new();
        private PlayableDirector m_PlayableDirector;


        private PlayableDirector PlayableDirector
        {
            get
            {
                if (!m_PlayableDirector)
                    m_PlayableDirector = gameObject.GetComponentInChildren<PlayableDirector>();
                return m_PlayableDirector;
            }
        }

        public static TimelineManager Create(string name)
        {
            GameObject obj = GameApp.Entry.Asset.LoadGameObject($"Timeline/{name}");
            TimelineManager timelineManager = obj.GetComponent<TimelineManager>();
            return timelineManager;
        }

        public void BindPlayer()
        {
            var player = GameApp.Entry.Game.Player;
            if (!m_Actors.Contains(player))
            {
                m_Actors.Add(player);
            }

            foreach (var item in PlayableDirector.playableAsset.outputs)
            {
                if (item.streamName == "Player")
                {
                    PlayableDirector.SetGenericBinding(item.sourceObject, player);
                }
                else if (item.streamName == "PlayerAnimator")
                {
                    PlayableDirector.SetGenericBinding(item.sourceObject, player.CAnim.AnimatorObj);
                }
                else if (item.streamName == "PlayerTransform")
                {
                    PlayableDirector.SetGenericBinding(item.sourceObject, player.transform);
                }
            }
        }

        public void BindOtherActor(SActor actor, int index)
        {
            if (!m_Actors.Contains(actor))
            {
                m_Actors.Add(actor);
            }

            foreach (var item in PlayableDirector.playableAsset.outputs)
            {
                if (item.streamName == $"Actor{index}")
                {
                    PlayableDirector.SetGenericBinding(item.sourceObject, actor);
                }
                else if (item.streamName == $"ActorAnimator{index}")
                {
                    PlayableDirector.SetGenericBinding(item.sourceObject, actor.CAnim.AnimatorObj);
                }
                else if (item.streamName == $"ActorTransform{index}")
                {
                    PlayableDirector.SetGenericBinding(item.sourceObject, actor.transform);
                }
            }
        }

        public void Play(Vector3 position, Quaternion rotation, Action onFinished)
        {
            transform.position = position;
            transform.rotation = rotation;
            TimelineAsset timelineAsset = (TimelineAsset)m_PlayableDirector.playableAsset;
            var rootTracks = timelineAsset.GetRootTracks();
            foreach (var t in rootTracks)
            {
                if (t.name == "PlayerAnimator")
                {
                    AnimationTrack at = (AnimationTrack)t;
                    at.position = position;
                    at.rotation = rotation;
                }
            }

            StartCoroutine(PlayItor(onFinished));
        }

        IEnumerator PlayItor(Action onFinished)
        {
            foreach (var actor in m_Actors)
            {
                actor.gameObject.SetActive(false);
                actor.IsTimelineMode = true;
            }

            GameApp.Entry.Game.PlayerCamera.Cam.enabled = false;
            GameApp.Entry.UI.RootUIObj.HideAllWnd();
            PlayableDirector.RebuildGraph();

            foreach (var actor in m_Actors)
            {
                actor.gameObject.SetActive(true);
            }

            PlayableDirector.Play(PlayableDirector.playableAsset);

            while (PlayableDirector.state == PlayState.Playing)
            {
                yield return null;
            }

            foreach (var actor in m_Actors)
            {
                actor.IsTimelineMode = false;
            }

            GameApp.Entry.Game.PlayerCamera.Cam.enabled = true;
            GameApp.Entry.UI.RootUIObj.RevertHideAllWnd();

            Destroy();

            onFinished?.Invoke();
        }

        public void Destroy()
        {
            GameObject.Destroy(gameObject);
        }
    }
}