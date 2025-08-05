using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Saber.CharacterController
{
    public class ActorBuffManager
    {
        private Coroutine m_CoroutineFreeSprint;

        public bool FreeSprint { get; set; }


        public void BeginFreeSprint()
        {
            if (m_CoroutineFreeSprint != null)
            {
                m_CoroutineFreeSprint.StopCoroutine();
            }

            m_CoroutineFreeSprint = FreeSprintItor().StartCoroutine();
        }

        IEnumerator FreeSprintItor()
        {
            FreeSprint = true;
            yield return new WaitForSeconds(0.6f);
            FreeSprint = false;
            m_CoroutineFreeSprint = null;
        }
    }
}