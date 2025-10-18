using System;


using UnityEngine;

namespace Saber.CharacterController
{
    public class NPCStateMachine : ActorStateMachine
    {
        public NPCStateMachine(SNPC owner) : base(owner)
        {
        }

        protected override void RegisterStates()
        {
            RegisterState(new Idle());
            RegisterState(new NPCMove());
            RegisterState(new Die());
        }
    }
}