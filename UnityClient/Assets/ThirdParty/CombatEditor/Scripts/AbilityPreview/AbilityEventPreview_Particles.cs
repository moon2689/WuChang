using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CombatEditor
{
# if UNITY_EDITOR
    public class AbilityEventPreview_Particles : AbilityEventPreview_CreateObjWithHandle
    {
        float m_ParticleInitSimulateSpeed = 1;
        ParticleSystem m_Particle;


        public AbilityEventObj_Particles Obj => (AbilityEventObj_Particles)m_EventObj;

        public AbilityEventPreview_Particles(AbilityEventObj Obj) : base(Obj)
        {
            m_EventObj = Obj;
        }


        public bool PreviewActive()
        {
            return eve.Previewable;
        }

        public override void InitPreview()
        {
            base.InitPreview();
            SetParticleData();
        }

        public void SetParticleData()
        {
            if (InstantiatedObj != null)
            {
                m_Particle = InstantiatedObj.GetComponent<ParticleSystem>();
                if (m_Particle != null)
                {
                    m_Particle.Stop();
                    m_Particle.useAutoRandomSeed = false;
                    var main = m_Particle.main;
                    m_ParticleInitSimulateSpeed = main.simulationSpeed;
                    //main.simulationSpeed = 0;
                }
            }
        }

        /// <summary>
        /// The m_Particle's real time is influenced by timescale event.
        /// </summary>
        /// <param name="ScaledPercentage"></param>
        public override void PreviewRunningInScale(float ScaledPercentage)
        {
            base.PreviewRunningInScale(ScaledPercentage);
            if (InstantiatedObj == null) return;
            //Debug.Log("Simulate?");
            SimulateParticles(ScaledPercentage);
        }

        public void SimulateParticles(float ScaledPercentage)
        {
            //Set Preview Percentage
            if (Obj.IsActive)
            {
                ParticleSystem ps = InstantiatedObj.GetComponent<ParticleSystem>();
                if (ps == null)
                {
                    ps = InstantiatedObj.GetComponentInChildren<ParticleSystem>();
                }

                if (ps == null)
                {
                    return;
                }
                var main = ps.main;
                main.simulationSpeed = m_ParticleInitSimulateSpeed;

                bool IsInRange = false;
                if (EventObj.GetEventTimeType() == AbilityEventObj.EventTimeType.EventRange && CurrentInScaledRange)
                {
                    IsInRange = true;
                }

                if (EventObj.GetEventTimeType() == AbilityEventObj.EventTimeType.EventTime &&
                    ScaledPercentage >= StartTimeScaledPercentage)
                {
                    IsInRange = true;
                }

                //ParticleSystem need 1/60f to start simulate
                if (IsInRange)
                {
                    m_Particle.Simulate(1 / 60f + (ScaledPercentage - StartTimeScaledPercentage) * AnimLength, true,
                        true);
                    //InstantiatedObj.GetComponent<ParticleSystem>().Simulate(0.1f, true, false);
                    SceneView.RepaintAll();
                }
                else
                {
                    m_Particle.Simulate(0, true, true);
                    SceneView.RepaintAll();
                }
            }
        }
    }

#endif
}