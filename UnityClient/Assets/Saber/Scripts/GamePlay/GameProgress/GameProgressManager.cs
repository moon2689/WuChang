using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Saber.Frame;
using UnityEngine;

namespace Saber
{
    public class GameProgressManager
    {
        private static GameProgressManager s_Instance;
        public static GameProgressManager Instance => s_Instance ??= new();

        private GameProgressData m_ProgressData;

        private string SavePath => $"{Application.persistentDataPath}/SaberProgress.json";
        public bool HasSavePointBefore { get; private set; }
        public int LastStayingSceneID => m_ProgressData.m_LastStayingSceneID;
        public int LastStayingStatueIndex => m_ProgressData.m_lastStayingGodStateIndex;
        public List<SceneProgressData> SceneProgressDatas => m_ProgressData.m_SceneProgress;
        //public int[] Clothes => m_ProgressData.m_Clothes;
        

        private GameProgressManager()
        {
        }

        public void Save()
        {
            // Debug.Log($"Save progress file:{SavePath}");
            //m_ProgressData.m_Clothes = GameApp.Entry.Game.Player.CDressUp.GetDressingClothes();
            string json = JsonUtility.ToJson(m_ProgressData);
            File.WriteAllText(SavePath, json);
        }

        public void Read()
        {
            // Debug.Log($"Read progress file:{SavePath}");
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                m_ProgressData = JsonUtility.FromJson<GameProgressData>(json);
                HasSavePointBefore = true;
            }
            else
            {
                m_ProgressData = new()
                {
                    m_SceneProgress = new(),
                    m_lastStayingGodStateIndex = -1,
                    m_LastStayingSceneID = -1,
                };
                HasSavePointBefore = false;
            }
        }

        public void Clear()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
            }

            Read();
        }

        public bool IsGodStatueFired(int sceneID, int statueIndex)
        {
            var tarSceneProgress = m_ProgressData.m_SceneProgress.FirstOrDefault(a => a.m_SceneID == sceneID);
            if (tarSceneProgress != null)
            {
                return tarSceneProgress.m_FiredGodStatues.Contains(statueIndex);
            }

            return false;
        }

        public void OnGodStatueFire(int sceneID, int statueIndex)
        {
            var tarSceneProgress = m_ProgressData.m_SceneProgress.FirstOrDefault(a => a.m_SceneID == sceneID);
            if (tarSceneProgress == null)
            {
                tarSceneProgress = new SceneProgressData()
                {
                    m_FiredGodStatues = new(),
                    m_SceneID = sceneID,
                };
                m_ProgressData.m_SceneProgress.Add(tarSceneProgress);
            }

            tarSceneProgress.m_FiredGodStatues.Add(statueIndex);
            m_ProgressData.m_LastStayingSceneID = sceneID;
            m_ProgressData.m_lastStayingGodStateIndex = statueIndex;

            Save();
        }

        public void OnGodStatueRest(int sceneID, int statueIndex)
        {
            m_ProgressData.m_LastStayingSceneID = sceneID;
            m_ProgressData.m_lastStayingGodStateIndex = statueIndex;
            Save();
        }
    }
}