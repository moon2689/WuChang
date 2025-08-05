using System.Collections;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class DefenseBase : ActorStateBase
    {
        protected enum EState
        {
            None,
            DefenseStart,
            DefenseLoop,
            DefenseEnd,
            DefenseHit,
            DefenseBroken,
        }

        protected EState m_CurState;


        public DefenseBase() : base(EStateType.Defense)
        {
        }

        public bool CanDefense(SActor enemy)
        {
            if (m_CurState == EState.DefenseEnd || m_CurState == EState.DefenseBroken)
            {
                return false;
            }

            bool isFaceToFace = Vector3.Dot(Actor.transform.forward, enemy.transform.forward) < 0;
            if (!isFaceToFace)
            {
                return false;
            }

            return true;
        }

        public virtual void PlayParriedSucceedAnim(bool isLeftDir, float dmgHeightRate)
        {
        }
    }
}