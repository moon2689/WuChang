namespace Saber.CharacterController
{
    public class BuffHealHP : BuffBase
    {
        public BuffHealHP(SActor actor) : base(actor, EBuffType.HeadlHP)
        {
        }

        protected override void OnStart()
        {
        }

        protected override void OnUpdate(float deltaTime)
        {
            m_Actor.CStats.AddHp(deltaTime * m_Value);
        }

        protected override void OnEnd()
        {
        }
    }
}