using System.Collections.Generic;
using UnityEngine;

namespace Saber.CharacterController
{
    public class MonsterDie : ActorStateBase
    {
        enum EState
        {
            PlayDieAnim,
            Dissolve,
            Destroy,
        }

        private float m_DissolveWeight;
        private EState m_State;
        private List<Material> m_DissolveMaterials;


        public override bool CanEnter => Actor.IsDead;


        public MonsterDie() : base(EStateType.Die)
        {
        }

        public override void Enter()
        {
            base.Enter();
            // 如果已经躺在地上，则无需再播放死亡动画
            // if (!Weak.IsLieOnGround(Actor))
            //     Actor.CAnim.Play("Death");
            Actor.CPhysic.UseGravity = true;

            m_State = EState.PlayDieAnim;
            m_DissolveWeight = 0;
            m_DissolveMaterials = GetDissolveMaterials();
            foreach (var m in m_DissolveMaterials)
                m.DisableKeyword("_DISSOLVE_ON");
        }

        public override void OnStay()
        {
            base.OnStay();

            if (m_State == EState.PlayDieAnim)
            {
                if (Actor.CAnim.GetAnimNormalizedTime(0) > 0.95f)
                    m_State = EState.Dissolve;
            }
            else if (m_State == EState.Dissolve)
            {
                m_DissolveWeight += DeltaTime * 0.3f;
                foreach (var m in m_DissolveMaterials)
                {
                    m.EnableKeyword("_DISSOLVE_ON");
                    m.SetFloat("_Dissolve", 1);
                    m.SetFloat("_DissolveWeight", m_DissolveWeight);
                }

                if (m_DissolveWeight > 1)
                {
                    m_State = EState.Destroy;
                    Actor.gameObject.SetActive(false);
                }
            }
            else if (m_State == EState.Destroy)
            {
            }
        }

        List<Material> GetDissolveMaterials()
        {
            List<Material> list = new();
            Renderer[] renderers = Actor.transform.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                foreach (var m in r.materials)
                {
                    if (m.HasProperty("_Dissolve"))
                        list.Add(m);
                }
            }

            return list;
        }

        protected override void OnExit()
        {
            base.OnExit();
            foreach (var m in m_DissolveMaterials)
                m.DisableKeyword("_DISSOLVE_ON");
        }
    }
}