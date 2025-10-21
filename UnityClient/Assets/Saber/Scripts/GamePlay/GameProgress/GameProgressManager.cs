using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Saber.Config;
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
        public int LastStayingShenKanID => m_ProgressData.m_LastStayingShenKanID;

        public List<SceneProgressData> SceneProgressDatas => m_ProgressData.m_SceneProgress;
        public int[] Clothes => m_ProgressData.m_Clothes;
        public PlayerPropItemInfo[] Items => m_ProgressData.m_Items;


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
                    m_LastStayingShenKanID = -1,
                    m_LastStayingSceneID = -1,
                    m_Clothes = GameApp.Entry.Config.GameSetting.PlayerStartClothes,
                    m_Items = new PlayerPropItemInfo[0],
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

        public bool IsShenKanActived(int sceneID, int shenKanID)
        {
            var tarSceneProgress = m_ProgressData.m_SceneProgress.FirstOrDefault(a => a.m_SceneID == sceneID);
            if (tarSceneProgress != null)
            {
                return tarSceneProgress.m_ActivedShenKan.Contains(shenKanID);
            }

            return false;
        }

        public void Save()
        {
            int curSceneID = GameApp.Entry.Game.World.SceneInfo.m_ID;
            int curShenKanID = GameApp.Entry.Game.World.CurrentStayingShenKanID;

            m_ProgressData.m_LastStayingSceneID = curSceneID;
            m_ProgressData.m_LastStayingShenKanID = curShenKanID;
            m_ProgressData.m_Clothes = GameApp.Entry.Game.Player.CDressUp.GetDressingClothes();
            m_ProgressData.m_Items = GameApp.Entry.Game.Bag.ToItemsArray();

            if (curShenKanID > 0)
            {
                var sceneProgress = m_ProgressData.m_SceneProgress.FirstOrDefault(a => a.m_SceneID == curSceneID);
                if (sceneProgress != null)
                {
                    if (!sceneProgress.m_ActivedShenKan.Contains(curShenKanID))
                        sceneProgress.m_ActivedShenKan.Add(curShenKanID);
                }
                else
                {
                    sceneProgress = new();
                    sceneProgress.m_SceneID = curSceneID;
                    sceneProgress.m_ActivedShenKan = new() { curShenKanID };
                    m_ProgressData.m_SceneProgress.Add(sceneProgress);
                }
            }

            string json = JsonUtility.ToJson(m_ProgressData);
            File.WriteAllText(SavePath, json);
        }
    }
}