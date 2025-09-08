using System.Collections;
using Saber.Frame;
using UnityEngine;

namespace Saber
{
    public class EffectObj : MonoBehaviour
    {
        public bool IsPlaying => gameObject.activeSelf;
        
        public void Play(float time)
        {
            GameApp.Entry.Unity.StartCoroutine(PlayEtor(time));
        }

        public void Play(Transform parent,Vector3 pos,  Quaternion rot, float time)
        {
            transform.parent = parent;
            transform.position = pos;
            transform.rotation = rot;
            Play(time);
        }

        IEnumerator PlayEtor(float time)
        {
            gameObject.SetActive(true);
            if (time > 0)
            {
                yield return new WaitForSeconds(time);
                if (gameObject)
                {
                    gameObject.SetActive(false);
                    transform.parent = null;
                }
            }
        }

        public void Destroy()
        {
            GameObject.Destroy(gameObject);
        }
    }
}