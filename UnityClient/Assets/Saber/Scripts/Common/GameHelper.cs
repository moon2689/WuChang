using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Networking;
using Saber;
using Saber.Frame;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

public static class GameHelper
{
    public enum EDir4
    {
        Front,
        Back,
        Left,
        Right,
    }

    public enum EDir8
    {
        Front,
        Back,
        Left,
        Right,
        FrontLeft,
        FrontRight,
        BackLeft,
        BackRight,
    }

    public static bool IsEditor => UnityHelper.IsEditor;
    public static bool IsAndroid => UnityHelper.IsAndroid;
    public static bool IsIOS => UnityHelper.IsIOS;


    #region String

    public static bool IsNotEmpty(this string str)
    {
        return !str.IsEmpty();
    }

    public static bool IsEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    /// <summary>解析配置中的一维数组</summary>
    public static string[] ParseStringArray(this string arrayString)
    {
        if (string.IsNullOrEmpty(arrayString))
            return new string[0];

        return arrayString.Split(new char[] { '|', '[', ',', ']' }, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>解析配置中的一维数组</summary>
    public static int[] ParseIntArray(this string arrayString)
    {
        if (string.IsNullOrEmpty(arrayString))
            return new int[0];

        string[] words = arrayString.Split(new char[] { '|', '[', ',', ']' }, StringSplitOptions.RemoveEmptyEntries);
        int[] array = new int[words.Length];

        for (int i = 0; i < words.Length; i++)
        {
            bool canParse = int.TryParse(words[i], out array[i]);
            if (!canParse)
                Debug.LogError(string.Format("Parse to int failed, config: {0}", arrayString));
        }

        return array;
    }

    /// <summary>
    /// 解析配置中的一维数组
    /// </summary>
    public static uint[] ParseUIntArray(this string arrayString)
    {
        if (string.IsNullOrEmpty(arrayString))
            return new uint[0];

        string[] words = arrayString.Split(new char[] { '|', '[', ',', ']' }, StringSplitOptions.RemoveEmptyEntries);
        uint[] array = new uint[words.Length];

        for (int i = 0; i < words.Length; i++)
        {
            bool canParse = uint.TryParse(words[i], out array[i]);
            if (!canParse)
                Debug.LogError(string.Format("Parse to uint failed, config: {0}", arrayString));
        }

        return array;
    }

    /// <summary>
    /// 解析配置中的一维数组
    /// </summary>
    public static ushort[] ParseUShortArray(this string arrayString)
    {
        if (string.IsNullOrEmpty(arrayString))
            return new ushort[0];

        string[] words = arrayString.Split(new char[] { '|', '[', ',', ']' }, StringSplitOptions.RemoveEmptyEntries);
        ushort[] array = new ushort[words.Length];

        for (int i = 0; i < words.Length; i++)
        {
            bool canParse = ushort.TryParse(words[i], out array[i]);
            if (!canParse)
                Debug.LogError(string.Format("Parse to ushort failed, config: {0}", array));
        }

        return array;
    }

    /// <summary>
    /// 解析配置中的一维数组
    /// </summary>
    public static short[] ParseShortArray(this string arrayString)
    {
        if (string.IsNullOrEmpty(arrayString))
            return new short[0];

        string[] words = arrayString.Split(new char[] { '|', '[', ',', ']' }, StringSplitOptions.RemoveEmptyEntries);
        short[] array = new short[words.Length];

        for (int i = 0; i < words.Length; i++)
        {
            bool canParse = short.TryParse(words[i], out array[i]);
            if (!canParse)
                Debug.LogError(string.Format("Parse to ushort failed, config: {0}", array));
        }

        return array;
    }

    /// <summary>
    /// 解析配置中的一维数组
    /// </summary>
    public static float[] ParseFloatArray(this string arrayString)
    {
        if (string.IsNullOrEmpty(arrayString))
            return new float[0];

        string[] words = arrayString.Split(new char[] { '|', '[', ',', ']' }, StringSplitOptions.RemoveEmptyEntries);
        float[] array = new float[words.Length];

        for (int i = 0; i < words.Length; i++)
        {
            bool canParse = float.TryParse(words[i], out array[i]);
            if (!canParse)
                Debug.LogError(string.Format("Parse to float failed, config: {0}", arrayString));
        }

        return array;
    }

    public static Vector3[] ParseVector3Array(this string arrayString)
    {
        if (string.IsNullOrEmpty(arrayString))
            return new Vector3[0];

        string[] lines = arrayString.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        Vector3[] array = new Vector3[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            string[] words = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            float x = 0, y = 0, z = 0;

            if (words.Length > 0)
                float.TryParse(words[0], out x);
            if (words.Length > 1)
                float.TryParse(words[1], out y);
            if (words.Length > 2)
                float.TryParse(words[2], out z);

            Vector3 vec3 = new Vector3(x, y, z);

            array[i] = vec3;
        }

        return array;
    }

    public static Vector4[] ParseVector4Array(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return new Vector4[0];

        string[] lines = str.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        Vector4[] array = new Vector4[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            string[] words = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            float x = 0, y = 0, z = 0, w = 0;

            if (words.Length > 0)
                float.TryParse(words[0], out x);
            if (words.Length > 1)
                float.TryParse(words[1], out y);
            if (words.Length > 2)
                float.TryParse(words[2], out z);
            if (words.Length > 3)
                float.TryParse(words[3], out w);

            Vector4 vec4 = new Vector4(x, y, z, w);

            array[i] = vec4;
        }

        return array;
    }

    public static Vector2[] ParseVector2Array(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return new Vector2[0];

        string[] lines = str.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        Vector2[] array = new Vector2[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            string[] words = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            float x = 0, y = 0;

            if (words.Length > 0)
                float.TryParse(words[0], out x);
            if (words.Length > 1)
                float.TryParse(words[1], out y);

            Vector2 vec2 = new Vector2(x, y);

            array[i] = vec2;
        }

        return array;
    }

    public static Dictionary<uint, uint> ParseDic(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return null;

        Dictionary<uint, uint> dic = null;
        string[] lines = str.Split('|');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            string[] words = line.Split(',');
            if (words.Length < 2)
                continue;

            if (!uint.TryParse(words[0], out uint k))
                continue;
            if (!uint.TryParse(words[1], out uint v))
                continue;

            if (dic == null)
                dic = new Dictionary<uint, uint>();
            dic[k] = v;
        }

        return dic;
    }


    public static Vector2 ParseVector2(this string strV2)
    {
        Vector2 v = Vector2.zero;
        if (!string.IsNullOrEmpty(strV2))
        {
            string[] arrayStr = strV2.Split(new char[] { '[', ']', ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (arrayStr.Length > 0)
            {
                float.TryParse(arrayStr[0], out v.x);
            }

            if (arrayStr.Length > 1)
            {
                float.TryParse(arrayStr[1], out v.y);
            }
        }

        return v;
    }

    public static Vector3 ParseVector3(this string strV3)
    {
        Vector3 v3 = Vector3.zero;
        if (!string.IsNullOrEmpty(strV3))
        {
            string[] arrayStr = strV3.Split(new char[] { '[', ']', ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (arrayStr.Length > 0)
            {
                float.TryParse(arrayStr[0], out v3.x);
            }

            if (arrayStr.Length > 1)
            {
                float.TryParse(arrayStr[1], out v3.y);
            }

            if (arrayStr.Length > 2)
            {
                float.TryParse(arrayStr[2], out v3.z);
            }
        }

        return v3;
    }

    #endregion


    #region Transform

    public static void FindChildRecursive(this Transform trans, string name, ref Transform target)
    {
        if (!trans)
        {
            Debug.LogError("FindChildRecursive,Transform is null");
            return;
        }

        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError("FindChildRecursive,name is empty");
            return;
        }

        if (trans.name == name)
            target = trans;
        else
        {
            foreach (Transform item in trans)
                FindChildRecursive(item, name, ref target);
        }
    }

    public static void FindChildRecursiveByStartString(this Transform trans, string nameStartString,
        ref Transform target)
    {
        if (!trans || string.IsNullOrEmpty(nameStartString))
            return;

        if (trans.name.StartsWith(nameStartString))
            target = trans;
        else
        {
            foreach (Transform item in trans)
                FindChildRecursiveByStartString(item, nameStartString, ref target);
        }
    }

    public static List<Transform> FindChildrenByStartString(this Transform trans, string nameStartString)
    {
        if (!trans || string.IsNullOrEmpty(nameStartString))
            return null;

        List<Transform> list = null;
        Transform[] children = trans.GetComponentsInChildren<Transform>();
        for (int i = 0; i < children.Length; i++)
        {
            Transform c = children[i];
            if (c.name.StartsWith(nameStartString))
            {
                if (list == null)
                    list = new List<Transform>();
                list.Add(c);
            }
        }

        return list;
    }

    public static void FindChildrenRecursive(this Transform trans, string[] names, ref List<Transform> list)
    {
        if (!trans || names == null)
            return;

        if (list == null)
            list = new List<Transform>();

        int index = Array.FindIndex(names, n => n == trans.name);
        if (index > -1)
            list.Add(trans);

        foreach (Transform item in trans)
            FindChildrenRecursive(item, names, ref list);
    }

    public static void SetLayerRecursive(this Transform trans, EStaticLayers layer)
    {
        if (trans)
        {
            trans.gameObject.layer = (int)layer;
            foreach (Transform c in trans)
                c.SetLayerRecursive(layer);
        }
    }

    public static void SetLayerRecursive(this GameObject obj, EStaticLayers layer)
    {
        if (obj)
        {
            obj.layer = (int)layer;
            foreach (Transform c in obj.transform)
                c.SetLayerRecursive(layer);
        }
    }
    
    public static void SetRenderingLayerRecursive(this GameObject obj, ERenderingLayers layer)
    {
        if (obj)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                r.renderingLayerMask = layer.GetLayerMask();
            }
        }
    }

    public static GameObject InstantiateAsChild(this GameObject prefab, Transform parent)
    {
        if (prefab)
        {
            GameObject obj = GameObject.Instantiate(prefab);
            obj.transform.parent = parent;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = quaternion.identity;
            obj.transform.localScale = Vector3.one;
            return obj;
        }

        return null;
    }

    public static List<T> GetComponentsInTopChildren<T>(this GameObject obj) where T : Component
    {
        return obj.transform.GetComponentsInTopChildren<T>();
    }

    public static List<T> GetComponentsInTopChildren<T>(this Transform trans) where T : Component
    {
        List<T> list = new();
        foreach (Transform c in trans)
        {
            T t = c.GetComponent<T>();
            if (t)
            {
                list.Add(t);
            }
        }

        return list;
    }

    #endregion


    #region UGUI

    public static void AddEvent(this Button button, EventTriggerType id, UnityAction<BaseEventData> callback)
    {
        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (!trigger)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = trigger.triggers.Find(e => e.eventID == id);
        if (entry != null)
        {
            if (entry.callback == null)
                entry.callback = new EventTrigger.TriggerEvent();
        }
        else
        {
            entry = new EventTrigger.Entry()
            {
                eventID = id,
                callback = new EventTrigger.TriggerEvent(),
            };
        }

        entry.callback.AddListener(callback);
        trigger.triggers.Add(entry);
    }

    public static void TriggerEvent(this Button button, EventTriggerType id)
    {
        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (!trigger)
            return;

        EventTrigger.Entry entry = trigger.triggers.Find(e => e.eventID == id);
        if (entry != null)
            entry.callback.Invoke(null);
    }

    // public static T GetFromContainer<T>(this GameObjectContainer container, string key) where T : Component
    // {
    //     if (container)
    //     {
    //         return container.GetGameObj(key)?.GetComponent<T>();
    //     }
    //     return null;
    // }

    #endregion


    // 椭圆映射，解决斜角移动
    // 参考：https://blog.csdn.net/euphorias/article/details/95314827
    public static void FixStick(float x, float y, out float fixedX, out float fixedY)
    {
        fixedX = x * Mathf.Sqrt(1 - y * y / 2);
        fixedY = y * Mathf.Sqrt(1 - x * x / 2);
    }

    public static float GetStickLength(float x, float y)
    {
        float x2 = x * x;
        float y2 = y * y;
        return Mathf.Sqrt(x2 + y2 - x2 * y2);
    }


    public static void GetWebRequest(string url, int timeOut, Action<string, byte[]> callback,
        Action<float> progress = null)
    {
        GameApp.Entry.Unity.StartCoroutine(GetWebRequestEtor(url, timeOut, callback, progress));
    }

    static IEnumerator GetWebRequestEtor(string url, int timeOut, Action<string, byte[]> callback,
        Action<float> progress = null)
    {
        Debug.Log($"UnityHttp.GetRequest url:{url}");

        using UnityWebRequest webRequest = new UnityWebRequest(url, "GET");
        if (timeOut > 0)
        {
            webRequest.timeout = timeOut;
        }

        webRequest.downloadHandler = new DownloadHandlerBuffer();

        if (progress != null)
        {
            webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.InProgress &&
                webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"PostRequest ERROR({url}): {webRequest.error}.\n {webRequest.downloadHandler.text}");
                callback?.Invoke("HTTP_ERROR", null);
            }

            while (!webRequest.isDone)
            {
                progress(webRequest.downloadProgress);
                yield return null;
            }
        }
        else
            yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"PostRequest ERROR({url}): {webRequest.error}.\n {webRequest.downloadHandler.text}");
            callback?.Invoke("HTTP_ERROR", null);
        }
        else
        {
            progress?.Invoke(1);
            string response = webRequest.downloadHandler.text;
            callback?.Invoke(response, webRequest.downloadHandler.data);
        }

        webRequest.downloadHandler.Dispose();
        webRequest.Dispose();
    }

    public static EDir4 Calc4Dir(this Vector3 tarDir, Vector3 frontDir, out float angle)
    {
        angle = Vector3.SignedAngle(frontDir, tarDir, Vector3.up);
        if ((angle >= 0 && angle < 45f) || (angle <= 0 && angle >= -45f))
        {
            return EDir4.Front;
        }
        else if (angle >= 45 && angle < 135)
        {
            return EDir4.Right;
        }
        else if (angle >= 135 || angle <= -135)
        {
            return EDir4.Back;
        }
        else
        {
            return EDir4.Left;
        }
    }

    public static EDir8 Calc8Dir(this Vector3 tarDir, Vector3 frontDir, out float angle)
    {
        angle = Vector3.SignedAngle(frontDir, tarDir, Vector3.up);
        if ((angle >= 0 && angle < 22.5f) || (angle <= 0 && angle >= -22.5f))
        {
            return EDir8.Front;
        }
        else if (angle >= 22.5f && angle < 67.5f)
        {
            return EDir8.FrontRight;
        }
        else if (angle >= 67.5f && angle < 112.5f)
        {
            return EDir8.Right;
        }
        else if (angle >= 112.5f && angle < 157.5f)
        {
            return EDir8.BackRight;
        }
        else if (angle >= 157.5f || angle < -157.5f)
        {
            return EDir8.Back;
        }
        else if (angle >= -157.5f && angle < -112.5f)
        {
            return EDir8.BackLeft;
        }
        else if (angle >= -112.5f && angle < -67.5f)
        {
            return EDir8.Left;
        }
        else
        {
            return EDir8.FrontLeft;
        }
    }

    public static Coroutine StartCoroutine(this IEnumerator enumerator)
    {
        return GameApp.Entry.Unity.StartCoroutine(enumerator);
    }

    public static void StopCoroutine(this Coroutine coroutine)
    {
        GameApp.Entry.Unity.StopCoroutine(coroutine);
    }
    
    /// <summary>
    /// 计算概率
    /// </summary>
    /// <param name="percent">百分之{percent}的概率（0-100）</param>
    /// <returns>是否发生</returns>
    public static bool CalcProbability(int percent)
    {
        return UnityEngine.Random.Range(0, 100) < percent;
    }
}