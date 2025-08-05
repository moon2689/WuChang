using System;


namespace Saber.CharacterController
{
    public class ObstructTired : ObstructBase
    {
        enum EState
        {
            TiredLoop,
            TiredEnd,
        }

        private EState m_EState;

        public ObstructTired(SActor actor, Action actionExit) : base(actor, actionExit)
        {
        }

        public override void Enter(DamageInfo damageInfo)
        {
            m_EState = EState.TiredLoop;
            Actor.CAnim.PlayClip("Animation/Obstruct/TiredStart", () => { Actor.CAnim.PlayClip("Animation/Obstruct/TiredLoop", null); });
        }

        public override void ReEnter(DamageInfo damageInfo)
        {
        }

        public override void OnStay(DamageInfo damageInfo, float deltaTime)
        {
            base.OnStay(damageInfo, deltaTime);
            if (m_EState == EState.TiredLoop)
            {
                if (Actor.CStats.IsStaminaFull)
                {
                    m_EState = EState.TiredEnd;
                    Actor.CAnim.PlayClip("Animation/Obstruct/TiredEnd", Exit);
                }
            }
        }
    }
}