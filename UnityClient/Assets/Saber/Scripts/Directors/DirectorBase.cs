using System.Collections;
using Saber.Frame;
using UnityEngine;

namespace Saber.Director
{
    public abstract class DirectorBase
    {
        public abstract DirectorBase GetNextDirector();

        public virtual void Enter()
        {
            //Debug.Log("Enter director: " + GetType().Name);
            GameApp.Entry.Unity.StartCoroutine(EnterAsync());
        }

        protected virtual IEnumerator EnterAsync()
        {
            yield return null;
        }

        public virtual void Update()
        {

        }

        public virtual void Exit()
        {
            //Debug.Log("Exit director: " + GetType().Name);
        }

    }
}
