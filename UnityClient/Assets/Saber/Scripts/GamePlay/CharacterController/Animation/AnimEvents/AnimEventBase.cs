using UnityEngine;

namespace Saber.CharacterController
{
    public abstract class AnimEventBase : StateMachineBehaviour
    {
        protected SActor m_Actor;

        public override void OnStateEnter(Animator anim, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!m_Actor)
            {
                m_Actor = anim.GetComponent<SActor>();
            }
        }
    }
}