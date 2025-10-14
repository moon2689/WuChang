using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UIAnimation.Actions
{
    [RequireComponent(typeof(Animation))]
    public class LegacyAnimationSequenceAction : IAction
    {
        [SerializeField]
        private List<LegacyAnimationSequenceMember> members = new List<LegacyAnimationSequenceMember>();

        private Animation animationComponent;
        public Animation AnimationComponent
        {
            get
            {
                if (animationComponent == null)
                {
                    animationComponent = GetComponent<Animation>();
                }
                return animationComponent;
            }
        }

        private float timeElapsedDuringDelay = 0;

        private float totalLength = -1;
        public float TotalLength
        {
            get
            {
                if (totalLength < 0)
                {
                    totalLength = 0f;
                    for (var i = 0; i < members.Count - 1; i++)
                    {
                        totalLength += members[i].PlayFor;
                    }
                    totalLength += members[members.Count - 1].Duration;
                }
                return totalLength;
            }
        }


        private bool isPlayed = false;
        private float timeElapsedInSec = 0f;
        private List<float> playForList = new List<float>();
        private int currentPlayingIndex = 0;

        #region implemented abstract members of IAction
        public override void FinalizeAction(bool isFastforward = false)
        {
            AnimationComponent.Play(members[members.Count - 1].Clip.name);
            AnimationStateFor(members[members.Count - 1].Clip.name).normalizedTime = 1f;
            base.CallOnActionDoneEvent();
        }

        public override bool IsDone()
        {
            return timeElapsedInSec >= TotalLength;
        }
        public override void OnStep(float deltaTime, bool shouldPause)
        {
            if (timeElapsedDuringDelay < DelaySeconds)
            {
                timeElapsedDuringDelay += deltaTime;
                return;
            }

            // NOTE: Pause is temporarily unsupported!
            if (!isPlayed)
            {
                AnimationComponent.Play(members[0].Clip.name);
                currentPlayingIndex = 0;
                isPlayed = true;
            }

            if (currentPlayingIndex < playForList.Count && timeElapsedInSec >= playForList[currentPlayingIndex])
            {
                var nextIndex = currentPlayingIndex + 1;
                AnimationComponent.CrossFade(members[nextIndex].Clip.name, members[currentPlayingIndex].CrossFadeLength);
                currentPlayingIndex++;
            }

            if (IsDone())
            {
                FinalizeAction();
            }

            timeElapsedInSec += deltaTime;
        }
        public override void ResetStatus()
        {
            Prepare();
        }
        public override void Prepare()
        {
            if (members.Count < 1)
            {
                throw new System.Exception("Memebers cannot be empty");
            }
            foreach (var member in members)
            {
                if (!AnimationComponent.GetClip(member.Clip.name))
                {
                    AnimationComponent.AddClip(member.Clip, member.Clip.name);
                }
            }

            playForList.Clear();
            var previousPlayfor = 0f;
            for (var i = 0; i < members.Count - 1; i++)
            {
                previousPlayfor += members[i].PlayFor;
                playForList.Add(previousPlayfor);
            }

            timeElapsedDuringDelay = 0f;
            timeElapsedInSec = 0f;
            currentPlayingIndex = 0;
            isPlayed = false;
            AnimationComponent.Stop();
        }
        #endregion

        public AnimationState AnimationStateFor(string clipname)
        {
            foreach (AnimationState state in AnimationComponent)
            {
                if (state.name == clipname)
                {
                    return state;
                }
            }
            return null;
        }
    }
}