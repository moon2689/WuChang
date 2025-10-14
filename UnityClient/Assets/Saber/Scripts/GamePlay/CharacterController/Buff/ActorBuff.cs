using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MagicaCloth2;
using Saber.Config;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class ActorBuff
    {
        private SActor m_Actor;
        private List<BuffBase> m_CurrentBuffs = new();
        private List<BuffBase> m_OverdueBuffs = new();
        private List<BuffBase> m_CachedBuffs = new();


        public ActorBuff(SActor actor)
        {
            m_Actor = actor;
        }

        public void Update(float deltaTime)
        {
            for (int i = 0; i < m_CurrentBuffs.Count; ++i)
            {
                BuffBase buff = m_CurrentBuffs[i];
                buff.Update(deltaTime);
                if (!buff.IsRunning)
                {
                    m_OverdueBuffs.Add(buff);
                }
            }

            if (m_OverdueBuffs.Count > 0)
            {
                for (int i = 0; i < m_OverdueBuffs.Count; i++)
                {
                    m_CurrentBuffs.Remove(m_OverdueBuffs[i]);
                    m_CachedBuffs.Add(m_OverdueBuffs[i]);
                }

                m_OverdueBuffs.Clear();
            }
        }

        public void AddBuff(EBuffType buffType, float value, float holdSeconds)
        {
            BuffBase buff = m_CurrentBuffs.FirstOrDefault(a => a.BuffType == buffType);
            if (buff == null)
            {
                buff = m_CachedBuffs.FirstOrDefault(a => a.BuffType == buffType);
                m_CachedBuffs.Remove(buff);
            }

            if (buff == null)
            {
                buff = buffType switch
                {
                    EBuffType.HeadlHP => new BuffHealHP(m_Actor),
                    _ => throw new InvalidOperationException($"Unknown buff:{buffType}"),
                };
            }

            if (!m_CurrentBuffs.Contains(buff))
            {
                m_CurrentBuffs.Add(buff);
            }

            buff.Start(value, holdSeconds);
        }
    }
}