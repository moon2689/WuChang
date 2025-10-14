using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UIAnimation.Actions
{
    [Serializable]
    public class LegacyAnimationSequenceMember
    {
        [SerializeField]
        private AnimationClip clip;
        public AnimationClip Clip {
            get {
                return clip;
            }
            set {
                clip = value;
            }
        }        

        [SerializeField]
        private float playFor = -1f;
        public float PlayFor {
            get {
                if (playFor < 0) {
                    return Clip.length;
                }
                return playFor;
            }
            set {
                playFor = value;
            }
        }

        [SerializeField]
        private float crossFadeLength = 0.1f;
        public float CrossFadeLength {
            get {
                return crossFadeLength;
            }
            set {
                crossFadeLength = value;
            }
        }

        public float Duration {
            get {
                return Clip.length;
            }
        }

        public LegacyAnimationSequenceMember()
        {
            
        }
    }
}