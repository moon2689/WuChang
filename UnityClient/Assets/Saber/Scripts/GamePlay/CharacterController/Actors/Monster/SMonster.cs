using System.Collections;
using System.Collections.Generic;
using Saber.AI;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class SMonster : SActor
    {
        [SerializeField] public MonsterConfig m_MonsterConfig;

        private MonsterStateMachine m_CStates;
        // private Coroutine m_CoroutineDamageEffect;


        public override BaseActorInfo m_BaseActorInfo => m_MonsterConfig.m_BaseActorInfo;
        public MonsterInfo m_MonsterInfo => m_MonsterConfig.m_MonsterInfo;
        public override ActorStateMachine CStateMachine
        {
            get
            {
                if (m_CStates == null)
                    m_CStates = new MonsterStateMachine(this);
                return m_CStates;
            }
        }

        public override EMoveSpeedV MoveSpeedV
        {
            get => m_MoveSpeedV;
            set
            {
                m_MoveSpeedV = value;
                if (m_MoveSpeedV == EMoveSpeedV.Sprint)
                {
                    m_MoveSpeedV = EMoveSpeedV.Run;
                }
            }
        }


        protected override void Awake()
        {
            base.Awake();
            CStats.EnableStamina = false;
        }

        protected override void Start()
        {
            base.Start();
            if (AI == null)
            {
                AI = m_MonsterInfo.m_DefaultAI.CreateEnemyAI();
            }
        }

        /*
        public override void OnPlayDamageEffect(Vector3 pos)
        {
            base.OnPlayDamageEffect(pos);
            if (m_CoroutineDamageEffect != null)
                StopCoroutine(m_CoroutineDamageEffect);
            m_CoroutineDamageEffect = StartCoroutine(MaterialPlayHurtEffectItor(pos));
        }

        IEnumerator MaterialPlayHurtEffectItor(Vector3 pos)
        {
            SkinnedMeshRenderer[] smrArray = GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var smr in smrArray)
            {
                foreach (var mat in smr.materials)
                {
                    if (mat.HasProperty("_OnHurt"))
                    {
                        mat.SetFloat("_OnHurt", 1);
                        mat.EnableKeyword("_ONHURT_ON");
                        mat.SetFloat("_HurtRadius", CPhysic.Radius);
                        mat.SetVector("_HurtPos", new Vector4(pos.x, pos.y, pos.z, 0));
                    }
                }
            }

            yield return new WaitForSeconds(0.3f);

            foreach (var smr in smrArray)
            {
                foreach (var mat in smr.materials)
                {
                    if (mat.HasProperty("_OnHurt"))
                    {
                        mat.SetFloat("_OnHurt", 0);
                        mat.DisableKeyword("_ONHURT_ON");
                    }
                }
            }

            m_CoroutineDamageEffect = null;
        }
        */
    }
}