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
            RegisterState(new NPCIdle());
            RegisterState(new NPCMove());
            RegisterState(new HumanObstruct());
            RegisterState(new Die());
        }
    }
}