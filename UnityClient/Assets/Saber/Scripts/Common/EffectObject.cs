using System;
using System.Collections;
using UnityEngine;

public class EffectObject : MonoBehaviour
{
    public float m_DelaySeconds = 3;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(DelayHide());
    }

    IEnumerator DelayHide()
    {
        yield return new WaitForSeconds(m_DelaySeconds);
        gameObject.SetActive(false);
    }

    public void Show(Vector3 position)
    {
        transform.position = position;
        Show();
    }

    public void Show()
    {
        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }
}