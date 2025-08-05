using System.Collections.Generic;
using UnityEngine;
using System;

namespace Saber.CharacterController
{
    public class AnimatorBaseLayer : AnimatorLayer
    {
        public AnimatorBaseLayer(Animator animator, IHandler handler)
            : base(animator, 0, handler)
        {
            
        }
    }
}