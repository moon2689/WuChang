using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Saber.Frame;
using Unity.Mathematics;

namespace Saber
{
    public class EffectPool
    {
        static EffectPool s_instance;
        Dictionary<string, List<GameObject>> m_dicEffects = new();


        public static EffectPool GetInstance()
        {
            return s_instance ??= new EffectPool();
        }


        public void CreateEffect(GameObject prefab, Transform parent, Vector3 pos, Quaternion rot, float time)
        {
            m_dicEffects.TryGetValue(prefab.name, out List<GameObject> list);
            if (list == null)
            {
                list = new List<GameObject>();
                m_dicEffects[prefab.name] = list;
            }

            GameObject go = list.Find(i => i != null && !i.gameObject.activeSelf);
            if (go == null)
            {
                go = GameObject.Instantiate<GameObject>(prefab);
                list.Add(go);
            }

            go.transform.parent = parent;
            go.transform.position = pos;
            go.transform.rotation = rot;

            if (time > 0)
                GameApp.Entry.Unity.StartCoroutine(PlayEtor(go, time));
            else
                go.SetActive(true);
        }

        public GameObject CreateEffect(GameObject prefab)
        {
            m_dicEffects.TryGetValue(prefab.name, out List<GameObject> list);
            if (list == null)
            {
                list = new List<GameObject>();
                m_dicEffects[prefab.name] = list;
            }

            GameObject e = list.Find(i => i != null && !i.gameObject.activeSelf);
            if (e == null)
            {
                e = GameObject.Instantiate<GameObject>(prefab);
                list.Add(e);
            }

            e.SetActive(true);
            return e;
        }

        public void PreloadEffect(GameObject prefab)
        {
            for (int i = 0; i < 3; i++)
            {
                CreateEffect(prefab);
            }

            foreach (var e in m_dicEffects[prefab.name])
            {
                if (e)
                    e.SetActive(false);
            }
        }

        public void CreateEffect(string name, Transform parent, Vector3 pos, Quaternion rot, float time)
        {
            GetOrCreateEffect(name, parent, pos, rot, e =>
            {
                if (time > 0)
                    GameApp.Entry.Unity.StartCoroutine(PlayEtor(e, time));
                else
                    e.SetActive(true);
            });
        }

        public void CreateEffect(string name, Vector3 pos, Quaternion rot, float time)
        {
            GetOrCreateEffect(name, null, pos, rot, e =>
            {
                if (time > 0)
                    GameApp.Entry.Unity.StartCoroutine(PlayEtor(e, time));
                else
                    e.SetActive(true);
            });
        }

        IEnumerator PlayEtor(GameObject gameObject, float time)
        {
            gameObject.SetActive(true);

            yield return new WaitForSeconds(time);
            if (gameObject)
            {
                gameObject.SetActive(false);
                gameObject.transform.parent = null;
            }
        }

        public void GetOrCreateEffect(string name, Transform parent, Vector3 pos, Quaternion rot, Action<GameObject> onGetted)
        {
            GetOrCreateEffect(name, e =>
            {
                e.transform.parent = parent;
                e.transform.position = pos;
                e.transform.rotation = rot;
            });
        }

        public void GetOrCreateEffect(string name, Transform parent, Action<GameObject> onGetted)
        {
            GetOrCreateEffect(name, e =>
            {
                e.transform.parent = parent;
                e.transform.localPosition = Vector3.zero;
                e.transform.localRotation = quaternion.identity;
            });
        }

        public void GetOrCreateEffect(string name, Action<GameObject> onGetted)
        {
            m_dicEffects.TryGetValue(name, out List<GameObject> list);
            if (list == null)
            {
                list = new List<GameObject>();
                m_dicEffects[name] = list;
            }

            GameObject e = list.Find(i => i != null && !i.gameObject.activeSelf);
            if (e)
            {
                onGetted?.Invoke(e);
            }
            else
            {
                GameApp.Entry.Asset.LoadGameObject(name, e =>
                {
                    list.Add(e);
                    onGetted?.Invoke(e);
                });
            }
        }

        public void HideEffect(string name)
        {
            m_dicEffects.TryGetValue(name, out List<GameObject> list);
            if (list != null)
            {
                GameObject e = list.Find(i => i != null && !i.gameObject.activeSelf);
                if (e != null)
                {
                    e.gameObject.SetActive(false);
                }
            }
        }

        public void Release()
        {
            foreach (var pair in m_dicEffects)
            {
                for (int i = 0; i < pair.Value.Count; i++)
                {
                    var item = pair.Value[i];
                    if (item != null)
                        GameObject.Destroy(item);
                }
            }

            m_dicEffects.Clear();
        }
    }
}