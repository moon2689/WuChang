using System;

using UnityEngine;

namespace Saber.CharacterController
{
    public class AnimEvent_PlayExpression : AnimPointTimeEvent
    {
        [SerializeField] private EExpressionType m_ExpressionType;
        [SerializeField] private float m_HoldTime;

        public override EAnimTriggerEvent EventType => EAnimTriggerEvent.PlayExpression;

        protected override void OnTrigger(Animator anim, AnimatorStateInfo state)
        {
            //base.OnTrigger(anim, state);
            if (m_Actor is SCharacter character)
            {
                //character.CExpression.DoExpression(m_ExpressionType, m_HoldTime);
            }
        }
    }
}