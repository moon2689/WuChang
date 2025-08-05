using Saber.CharacterController;
using UnityEngine;

namespace Saber.World
{
    [RequireComponent(typeof(SphereCollider))]
    public abstract class SceneInteractObjectBase : MonoBehaviour
    {
        protected abstract void OnPlayerEnter();
        protected abstract void OnPlayerExit();

        private void Awake()
        {
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            if (!rb)
                rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            var collider = GetComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.includeLayers = EStaticLayers.Actor.GetLayerMask();
        }

        private void OnTriggerEnter(Collider other)
        {
            SActor actor = other.GetComponent<SActor>();
            if (actor && actor.IsPlayer)
            {
                // Debug.Log($"Enter:{transform.name} {other.name}, actor:{actor.BaseInfo.m_Name}", other);
                OnPlayerEnter();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            SActor actor = other.GetComponent<SActor>();
            if (actor && actor.IsPlayer)
            {
                // Debug.Log($"Exit:{transform.name} {other.name}, actor:{actor.BaseInfo.m_Name}", other);
                OnPlayerExit();
            }
        }
    }
}