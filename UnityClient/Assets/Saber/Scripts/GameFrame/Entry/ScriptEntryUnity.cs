using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Saber.Frame
{
    public class ScriptEntryUnity
    {
        private Queue<IEnumerator> m_QueueCoroutine = new();
        private Coroutine m_CoroutineTask;

        public Coroutine StartCoroutine(IEnumerator routine)
        {
            if (GameApp.Instance)
                return GameApp.Instance.StartCoroutine(routine);
            return null;
        }

        public void StopCoroutine(Coroutine coroutine)
        {
            if (coroutine != null && GameApp.Instance)
                GameApp.Instance.StopCoroutine(coroutine);
        }

        public void StopAllCoroutines()
        {
            if (GameApp.Instance)
                GameApp.Instance.StopAllCoroutines();
        }

        public Coroutine DoDelayAction(float seconds, Action action)
        {
            if (action != null)
            {
                return StartCoroutine(DelayActionItor(seconds, action));
            }

            return null;
        }

        private IEnumerator DelayActionItor(float seconds, Action action)
        {
            yield return new WaitForSeconds(seconds);
            action();
        }

        public void DoActionOneFrameLater(Action action)
        {
            if (action != null)
            {
                StartCoroutine(DoActionOneFrameLaterItor(action));
            }
        }

        private IEnumerator DoActionOneFrameLaterItor(Action action)
        {
            yield return null;
            action();
        }

        public void StartCoroutineQueued(IEnumerator routine)
        {
            m_QueueCoroutine.Enqueue(routine);
            if (m_CoroutineTask == null && GameApp.Instance)
            {
                m_CoroutineTask = GameApp.Instance.StartCoroutine(LoopCoroutineQueued());
            }
        }

        private IEnumerator LoopCoroutineQueued()
        {
            while (true)
            {
                if (m_QueueCoroutine.Count > 0)
                {
                    var routine = m_QueueCoroutine.Dequeue();
                    yield return StartCoroutine(routine);
                }
                else
                {
                    m_CoroutineTask = null;
                    yield break;
                }
            }
        }

        public void DoActionPerFrame(Action action, float seconds)
        {
            if (action != null && seconds > 0)
            {
                StartCoroutine(DoActionPerFrameItor(action, seconds));
            }
        }

        private IEnumerator DoActionPerFrameItor(Action action, float seconds)
        {
            float timer = seconds;
            while (timer > 0)
            {
                timer -= Time.fixedTime;
                action();
                yield return null;
            }
        }
    }
}