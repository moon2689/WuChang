using System;
using System.Collections;
using System.Collections.Generic;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class UseSoul : CommonAbilityUseProp
    {
        protected override string AnimName => "UseSoul";


        public UseSoul() : base(EAbilityType.UseSoul)
        {
        }
    }
}