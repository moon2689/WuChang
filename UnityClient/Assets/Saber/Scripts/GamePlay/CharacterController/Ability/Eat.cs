using System;
using System.Collections;
using System.Collections.Generic;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class Eat : CommonAbilityUseProp
    {
        protected override string AnimName => "Eat";


        public Eat() : base(EAbilityType.Eat)
        {
        }
    }
}