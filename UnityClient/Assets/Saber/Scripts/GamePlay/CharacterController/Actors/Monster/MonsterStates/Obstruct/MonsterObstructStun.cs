using System;
using System.Collections.Generic;



using UnityEngine;

namespace Saber.CharacterController
{
    public class MonsterObstructStun : ObstructStun
    {
        public MonsterObstructStun(SActor actor, Action actionExit) : base(actor, actionExit)
        {
        }

        public override void Enter(DamageInfo damageInfo)
        {
            Actor.CAnim.Play("Stun");
        }
    }
}