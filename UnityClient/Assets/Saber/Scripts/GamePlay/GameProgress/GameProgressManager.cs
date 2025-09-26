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
        private GameProgressData m_ProgressData;

        private string SavePath => $"{Application.persistentDataPath}/SaberProgress.json";
        public bool HasSavePointBefore => m_ProgressData != null && m_ProgressData.m_LastStayingSceneID > 0;
        public int LastStayingSceneID => m_ProgressData.m_LastStayingSceneID;
        public int LastStayingIdolID => m_ProgressData.m_lastStayingIdolID;

        public List<SceneProgressData> SceneProgressDatas => m_ProgressData.m_SceneProgress;
        public int[] Clothes => m_ProgressData.m_Clothes;
        
        

        public void Save()
        {
            // Debug.Log($"Save progress file:{SavePath}");
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
            }
            else
            {
                m_ProgressData = new()
                {
                    m_SceneProgress = new(),
                    m_lastStayingIdolID = -1,
                    m_LastStayingSceneID = -1,
                    m_Clothes = GameApp.Entry.Config.GameSetting.PlayerStartClothes,
                };
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

        public bool IsIdolFired(int sceneID, int idolID)
        {
            var tarSceneProgress = m_ProgressData.m_SceneProgress.FirstOrDefault(a => a.m_SceneID == sceneID);
            if (tarSceneProgress != null)
            {
                return tarSceneProgress.m_FiredIdols.Contains(idolID);
            }

            return false;
        }

        public void SaveOnIdolFire(int sceneID, int idolID)
        {
            var tarSceneProgress = m_ProgressData.m_SceneProgress.FirstOrDefault(a => a.m_SceneID == sceneID);
            if (tarSceneProgress == null)
            {
                tarSceneProgress = new SceneProgressData()
                {
                    m_FiredIdols = new(),
                    m_SceneID = sceneID,
                };
                m_ProgressData.m_SceneProgress.Add(tarSceneProgress);
            }

            tarSceneProgress.m_FiredIdols.Add(idolID);
            m_ProgressData.m_LastStayingSceneID = sceneID;
            m_ProgressData.m_lastStayingIdolID = idolID;

            Save();
        }

        public void SaveOnIdolRest(int sceneID, int idolID)
        {
            m_ProgressData.m_LastStayingSceneID = sceneID;
            m_ProgressData.m_lastStayingIdolID = idolID;
            Save();
        }

        public void SaveOnDressClothes()
        {
            m_ProgressData.m_Clothes = GameApp.Entry.Game.Player.CDressUp.GetDressingClothes();
            Save();
        }
    }
}