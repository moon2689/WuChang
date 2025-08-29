using System.Collections.Generic;
using UnityEngine;
using System;

namespace Saber.CharacterController
{
    public enum EAnimatorParams : int
    {
        None,

        // trigger
        ToGround,

        // int
        //State,

        // float
        Horizontal,
        Vertical,

        // bool

        // anim name ----->
    }


    public static class AnimatorHelper
    {
        static Dictionary<string, int> s_dicAnimatorHash;


        public static int GetAnimatorHash(this EAnimatorParams p)
        {
            return p.ToString().GetAnimatorHash();
        }

        public static int GetAnimatorHash(this string str)
        {
            if (s_dicAnimatorHash == null)
                s_dicAnimatorHash = new Dictionary<string, int>();

            if (!s_dicAnimatorHash.TryGetValue(str, out int hash))
            {
                hash = Animator.StringToHash(str);
                s_dicAnimatorHash[str] = hash;
            }

            return hash;
        }
    }
}