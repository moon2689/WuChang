using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TA.Tool
{
    public class FBXShower : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private Object m_FBXFolderObj;
        [SerializeField] private int m_CurIndex;
        [SerializeField] private float m_RotateSpeed = 50;

        private string m_FBXFolder;
        private string[] m_FBXFiles;
        private GameObject m_CurObject;

        private void Awake()
        {
            m_FBXFolder = AssetDatabase.GetAssetPath(m_FBXFolderObj);
            m_FBXFiles = Directory.GetFiles(m_FBXFolder, "*.fbx", SearchOption.AllDirectories);
            ShowFBX(0);
        }

        private void Update()
        {
            if (m_CurObject)
            {
                m_CurObject.transform.Rotate(new Vector3(0, m_RotateSpeed * Time.deltaTime, 0));
            }
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(0, 0, 300, 100), "上一个"))
            {
                --m_CurIndex;
                ShowFBX(m_CurIndex);
            }
            else if (GUI.Button(new Rect(0, 100, 300, 100), "下一个"))
            {
                ++m_CurIndex;
                ShowFBX(m_CurIndex);
            }
            else if (GUI.Button(new Rect(0, 200, 300, 100), "刷新"))
            {
                ShowFBX(m_CurIndex);
            }
        }

        void ShowFBX(int index)
        {
            if (index >= m_FBXFiles.Length)
            {
                index = 0;
            }

            if (index < 0)
            {
                index = m_FBXFiles.Length - 1;
            }

            if (m_CurObject != null)
            {
                GameObject.Destroy(m_CurObject);
            }

            m_CurIndex = index;
            string filePath = m_FBXFiles[index];
            m_CurObject = GameObject.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(filePath));
        }

#endif
    }
}