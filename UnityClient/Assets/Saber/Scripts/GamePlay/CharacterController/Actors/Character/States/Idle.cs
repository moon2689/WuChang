using System;
using System.Collections;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class Idle : ActorStateBase
    {
        public override bool ApplyRootMotionSetWhenEnter => true;
        protected override ActorBaseStats.EStaminaRecSpeed StaminaRecSpeed => ActorBaseStats.EStaminaRecSpeed.Fast;


        public Idle() : base(EStateType.Idle)
        {
        }
    }
}