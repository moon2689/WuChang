namespace Saber.CharacterController
{
    public class SNPC : SActor
    {
        private NPCStateMachine m_CStates;

        public override BaseActorInfo m_BaseActorInfo => null;

        public override ActorStateMachine CStateMachine
        {
            get
            {
                if (m_CStates == null)
                    m_CStates = new NPCStateMachine(this);
                return m_CStates;
            }
        }
    }
}