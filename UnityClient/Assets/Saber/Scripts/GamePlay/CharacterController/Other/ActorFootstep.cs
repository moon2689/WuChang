using Saber.Frame;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Saber.CharacterController
{
    [RequireComponent(typeof(SphereCollider))]
    public class ActorFootstep : MonoBehaviour
    {
        [SerializeField] private AudioClip[] m_Clips;
        private SphereCollider m_Collider;

        private float m_Timer;


        public bool ActiveSelf
        {
            get => m_Collider.enabled;
            set { m_Collider.enabled = value; }
        }

        private void Awake()
        {
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            if (!rb)
                rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            m_Collider = GetComponent<SphereCollider>();
            m_Collider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            int groundLayer = EStaticLayers.Default.GetLayerMask();
            bool isGround = ((1 << other.gameObject.layer) & groundLayer) != 0;
            if (isGround && m_Timer < 0)
            {
                var clip = m_Clips[Random.Range(0, m_Clips.Length)];
                GameApp.Entry.Game.Audio.Play3DSound(clip, transform.position);

                m_Timer = 0.1f;
            }
        }

        private void Update()
        {
            if (m_Timer >= 0)
            {
                m_Timer -= Time.deltaTime;
            }
        }
    }
}