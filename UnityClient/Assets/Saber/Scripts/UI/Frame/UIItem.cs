using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Saber.UI
{
    public class UIItem : MonoBehaviour
    {
        public RootUI RootUIObj => RootUI.Instance;

        public virtual void Destroy()
        {
            GameObject.Destroy(gameObject);
        }

        protected virtual void Awake()
        {
        }

        protected virtual void Start()
        {
        }

        protected virtual void Update()
        {
        }

        protected virtual void OnDestroy()
        {
        }
    }
}