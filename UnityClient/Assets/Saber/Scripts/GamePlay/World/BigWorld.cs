using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CombatEditor;
using Saber.AI;
using Saber.CharacterController;
using Saber.Config;
using Saber.Director;
using Saber.Frame;
using Saber.Timeline;
using Saber.UI;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;

namespace Saber.World
{
    public class BigWorld : Wnd_MainCity.IHandler
        , Wnd_CharacterInfo.IHandler
        , Portal.IHandler
        , ShenKan.IHandler
        , Wnd_Rest.IHandler
    {
        public enum ELoadType
        {
            None,
            NewGame,
            ToLastShenKan,
            ToNextSceneByPortal,
            ToShenKan,
            ToNearestShenKan,
        }


        public Action<SMonster> Event_OnStartOrEndFightingBoss;


        private SActor m_Player;
        private ELoadType m_LoadType;
        private SceneBaseInfo m_SceneInfo;
        private Wnd_MainCity m_WndMainCity;
        private Wnd_Loading m_WndLoading;
        private Wnd_JoyStick m_WndJoyStick;
        private Wnd_Rest m_WndRest;
        private Portal m_CurrentUsingPortalInfo;
        private ScenePointShenKan m_CurrentStayingShenKan;
        private int m_TransmittingPortalID;
        private int m_TransmittingShenKanID;
        private Coroutine m_CoroutineWitchTime;
        private EffectObject m_EffectWitchTimeBoom;
        private ScenePoint[] m_ScenePoints;


        public SActor Player => m_Player;

        /*
        public float Timeline
        {
            get => m_AzureTime != null ? m_AzureTime.GetTimeline() : 0;
            set
            {
                if (m_AzureTime)
                    m_AzureTime.SetTimeline(value);
            }
        }
        */

        //public Vector3 Date => m_AzureTime != null ? m_AzureTime.GetDate() : Vector3.zero;

        public Light MainLight { get; private set; }
        public SMonster CurrentFightingBoss { get; private set; }
        public SceneBaseInfo SceneInfo => m_SceneInfo;
        public ScenePointShenKan CurrentStayingShenKan => m_CurrentStayingShenKan;
        public int CurrentStayingShenKanID => m_CurrentStayingShenKan != null ? m_CurrentStayingShenKan.m_ID : 0;


        #region 加载

        public void Load(ELoadType loadType, Action onLoaded)
        {
            if (loadType == ELoadType.NewGame)
            {
                // 新游戏
                int sceneID = GameApp.Entry.Config.GameSetting.StartSceneID;
                m_SceneInfo = GameApp.Entry.Config.SceneInfo.GetSceneInfoByID(sceneID);
                // m_PlayerPos = GameApp.Entry.Config.GameSetting.m_BornPos;
                // m_PlayerRot = Quaternion.Euler(0, GameApp.Entry.Config.GameSetting.m_BornRotY, 0);
            }
            else if (loadType == ELoadType.ToNextSceneByPortal)
            {
                // 传送
                int sceneID = m_CurrentUsingPortalInfo.TargetSceneID;
                m_SceneInfo = GameApp.Entry.Config.SceneInfo.GetSceneInfoByID(sceneID);
                m_TransmittingPortalID = m_CurrentUsingPortalInfo.TargetPortalID;
            }
            else if (loadType == ELoadType.ToLastShenKan)
            {
                int sceneID = GameApp.Entry.Game.ProgressMgr.LastStayingSceneID;
                m_SceneInfo = GameApp.Entry.Config.SceneInfo.GetSceneInfoByID(sceneID);
                m_TransmittingShenKanID = GameApp.Entry.Game.ProgressMgr.LastStayingShenKanID;
            }
            else
            {
                throw new InvalidOperationException($"Unknown load type:{loadType}");
            }

            GameApp.Entry.Unity.StartCoroutine(LoadItor(loadType, onLoaded));
        }

        IEnumerator LoadItor(ELoadType loadType, Action onLoaded)
        {
            m_LoadType = loadType;

            if (m_WndLoading == null)
                yield return GameApp.Entry.UI.CreateWnd<Wnd_Loading>(w => m_WndLoading = w);

            yield return GameApp.Entry.UI.CreateWnd<Wnd_JoyStick>(null, null, w => m_WndJoyStick = w);
            //m_WndJoyStick.Default();

            // scene
            yield return LoadScene().StartCoroutine();

            //
            m_CurrentStayingShenKan = null;
            if (m_LoadType == ELoadType.ToShenKan || m_LoadType == ELoadType.ToLastShenKan || m_LoadType == ELoadType.ToNearestShenKan)
            {
                ScenePoint point = m_ScenePoints.FirstOrDefault(a => a.m_PointType == EScenePointType.ShenKan && a.m_ID == m_TransmittingShenKanID);
                m_CurrentStayingShenKan = point as ScenePointShenKan;
            }

            // player
            yield return CreatePlayer().StartCoroutine();

            // other actors
            yield return CreateOtherActors().StartCoroutine();

            // wnd
            if (m_WndMainCity == null)
            {
                yield return GameApp.Entry.UI.CreateWnd<Wnd_MainCity>(null, this, w => m_WndMainCity = w);
            }

            // preload effects
            GameApp.Entry.Config.SkillCommon.PreloadEffects();
            yield return null;

            SetFilmEffect(false);

            /*
            // 在神像休息
            if (loadType == ELoadType.ToGodStatue ||
                loadType == ELoadType.ToLastGodStatue ||
                loadType == ELoadType.ToNextSceneByPortal)
            {
                if (m_CurrentStayingGodStatue != null)
                {
                    yield return m_CurrentStayingGodStatue.Rest();
                }
            }
            */

            GameApp.Entry.Game.ProgressMgr.Save();
            yield return null;

            m_WndLoading.Percent = 100;
            //yield return new WaitForSeconds(0.1f);
            m_WndLoading.Destroy();

            if (m_LoadType == ELoadType.NewGame)
            {
                /*
                // 新游戏cg动画
                m_LoadType = ELoadType.None;
                
                Vector3 position = GameApp.Entry.Config.GameSetting.m_BornPos;
                Quaternion rotation = Quaternion.Euler(0, GameApp.Entry.Config.GameSetting.m_BornRotY, 0);
                TimelineManager timelineManager = TimelineManager.Create("CGNewGame");
                timelineManager.BindPlayer();
                timelineManager.Play(position, rotation, null);
                
                m_Player.transform.position = GameApp.Entry.Config.GameSetting.m_BornPos;
                m_Player.transform.rotation = Quaternion.Euler(0, GameApp.Entry.Config.GameSetting.m_BornRotY, 0);
                */
            }
            else if (m_LoadType == ELoadType.ToNextSceneByPortal)
            {
                // 走出传送门动画
                ScenePoint portalPoint = m_ScenePoints.FirstOrDefault(a =>
                    a.m_PointType == EScenePointType.Portal && a.m_ID == m_TransmittingPortalID);
                GameApp.Entry.Game.PlayerCamera.LookAtTarget(portalPoint.transform.rotation.eulerAngles.y + 150);
                Vector3 targetPos = portalPoint.transform.position + portalPoint.transform.forward * 1f;
                yield return GameApp.Entry.Game.PlayerAI.PlayActionMoveToTargetPos(targetPos, null);
            }

            onLoaded?.Invoke();
        }

        IEnumerator LoadScene()
        {
            if (m_Player)
            {
                m_Player.gameObject.SetActive(false);
            }

            if (SceneManager.GetActiveScene().name != m_SceneInfo.m_ResName)
            {
                // destroy actors
                DestroyOtherActors();
                SceneHandle sceneHandle = GameApp.Entry.Asset.LoadScene(m_SceneInfo.m_ResName, null);
                while (!sceneHandle.IsDone)
                {
                    m_WndLoading.Percent = 50 * sceneHandle.Progress;
                    yield return null;
                }
            }

            m_ScenePoints = GameObject.FindObjectsOfType<ScenePoint>();

            m_WndLoading.Percent = 50;

            OnSceneLoaded();
            yield return null;

            // 传送门
            int portalCount = m_ScenePoints.Count(a => a.m_PointType == EScenePointType.Portal);
            if (portalCount > 0)
            {
                string portalParentName = "Portals";
                GameObject parentPortal = GameObject.Find(portalParentName);
                if (!parentPortal)
                    parentPortal = new GameObject(portalParentName);

                int count = 0;
                foreach (var scenePoint in m_ScenePoints)
                {
                    if (scenePoint.m_PointType != EScenePointType.Portal)
                    {
                        continue;
                    }

                    ++count;
                    m_WndLoading.Percent = 50 + 10 * count / portalCount;
                    AssetHandle assetHandle = scenePoint.Load(parentPortal.transform, this);
                    while (assetHandle != null && !assetHandle.IsDone)
                    {
                        yield return null;
                    }
                }
            }

            /*
            if (m_LoadType == ELoadType.ToNextSceneByPortal)
            {
                m_TransmittingPortalID.PortalObject.EnableGateCollider(false);
            }
            */

            m_WndLoading.Percent = 60;

            // 神龛（存档点）
            int shenKanCount = m_ScenePoints.Count(a => a.m_PointType == EScenePointType.ShenKan);
            if (shenKanCount > 0)
            {
                string shenKanParentName = "ShenKans";
                GameObject parentShenKan = GameObject.Find(shenKanParentName);
                if (!parentShenKan)
                    parentShenKan = new GameObject(shenKanParentName);
                int count = 0;
                foreach (var scenePoint in m_ScenePoints)
                {
                    if (scenePoint.m_PointType != EScenePointType.ShenKan)
                    {
                        continue;
                    }

                    ++count;
                    m_WndLoading.Percent = 60 + 5 * count / shenKanCount;
                    AssetHandle assetHandle = scenePoint.Load(parentShenKan.transform, this);
                    while (assetHandle != null && !assetHandle.IsDone)
                    {
                        yield return null;
                    }
                }
            }

            GameSetting.URPAsset.shadowDistance = m_SceneInfo.m_ShadowDistance;

            m_WndLoading.Percent = 65;
        }

        void OnSceneLoaded()
        {
            // volume
            if (m_SceneInfo.m_OpenPostprocess)
            {
                GameApp.Entry.Asset.LoadGameObject("Game/GlobalVolume", null);
            }

            // Dynamic Skybox
            if (m_SceneInfo.m_SkyboxType == ESkyboxType.DynamicSkybox)
            {
                /*
                GameObject goAzure = (GameObject)GameObject.Instantiate(Resources.Load("Game/AzureDynamicSkybox"));
                MainLight = goAzure.GetComponentInChildren<Light>();

                m_AzureTime = goAzure.GetComponent<AzureTimeController>();
                m_AzureTime.SetNewDayLength(GameApp.Entry.Config.GameSetting.DayLengthMinutes);
                m_AzureTime.SetFollowTarget(PlayerCamera.Instance.CamT);
                m_AzureTime.SetDate(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                //m_AzureTime.SetTimeline(DateTime.Now.Hour);
                m_AzureTime.SetTimeline(8);

                m_AzureWeather = m_AzureTime.GetComponent<AzureWeatherController>();
                m_AzureEffects = m_AzureTime.GetComponent<AzureEffectsController>();

                m_AzureWeather.SetNewWeatherProfile(0);

                // 当过了1天
                m_AzureTime.RegisterEvent_OnDayChange(() =>
                {
                    if (UnityEngine.Random.Range(0, 100) > 5)
                        m_AzureWeather.SetNewWeatherProfile(0);
                    else
                        m_AzureWeather.SetNewRandomWeather();
                });

                // 当天气变化
                m_AzureWeather.OnWeatherChange += curWeather =>
                {
                    // wet
                    GameApp.Entry.Unity.DoDelayAction(1, () =>
                    {
                        bool rain = curWeather.name.Contains("Rain");
                        m_Player.GetWet(rain);
                    });

                    // 暴雨，打雷
                    if (curWeather.name == "Heavy Rain")
                    {
                        PlayThunderEffect();
                    }
                };
                */
            }
            // Static skybox
            else if (m_SceneInfo.m_SkyboxType == ESkyboxType.StaticSkybox)
            {
                GameObject objSkyBox = (GameObject)GameObject.Instantiate(Resources.Load("Game/StaticSkybox"));
                MainLight = objSkyBox.GetComponentInChildren<Light>();
            }
            else
            {
                GameObject[] rootObjs = SceneManager.GetActiveScene().GetRootGameObjects();
                foreach (var rootObj in rootObjs)
                {
                    MainLight = rootObj.GetComponent<Light>();
                    if (MainLight != null && MainLight.type == LightType.Directional)
                    {
                        break;
                    }
                }
            }
        }

        /*
        /// <summary>打雷</summary>
        void PlayThunderEffect()
        {
            var curWeather = m_AzureWeather.GetCurrentWeatherProfile();
            bool isHeavyRain = curWeather != null && curWeather.name == "Heavy Rain";
            if (isHeavyRain)
            {
                m_AzureEffects.InstantiateThunderEffect(1);
                float time = UnityEngine.Random.Range(5, 15);
                GameApp.Entry.Unity.DoDelayAction(time, PlayThunderEffect);
            }
        }

        /// <summary>天气变晴</summary>
        public void SetWeather_ClearDay()
        {
            if (m_AzureWeather)
                m_AzureWeather.SetNewWeatherProfile(0);
        }
        */

        void DestroyOtherActors()
        {
            if (m_ScenePoints != null)
            {
                foreach (var p in m_ScenePoints)
                {
                    if (p.m_PointType == EScenePointType.MonsterBornPosition)
                        p.Destroy();
                }
            }
        }

        private IEnumerator CreateOtherActors()
        {
            foreach (var p in m_ScenePoints)
            {
                if (p is ScenePointShenKan ip && ip != m_CurrentStayingShenKan)
                {
                    foreach (var mp in ip.LinkMonsterPoints)
                        mp.Destroy();
                }
            }

            yield return null;

            if (m_CurrentStayingShenKan == null)
            {
                yield break;
            }

            int count = 0;
            foreach (var scenePoint in m_CurrentStayingShenKan.LinkMonsterPoints)
            {
                ++count;
                yield return CreateActor(scenePoint).StartCoroutine();
                m_WndLoading.Percent = 70 + 20 * count / m_CurrentStayingShenKan.LinkMonsterPoints.Length;
                yield return null;
            }
        }

        IEnumerator CreateActor(ScenePointMonster bornPoint)
        {
            if (bornPoint.Actor)
            {
                bornPoint.RebirthActor();
                yield return null;
            }
            else
            {
                string parentName = "Monsters";
                GameObject parentMonster = GameObject.Find(parentName);
                if (!parentMonster)
                    parentMonster = new GameObject(parentName);
                AssetHandle assetHandle = bornPoint.Load(parentMonster.transform, this);
                while (!assetHandle.IsDone)
                {
                    yield return null;
                }

                bornPoint.Actor.Event_OnDeadAnimPlayFinished += OnOtherActorDead;
                bornPoint.Actor.AI.OnSetLockingEnemy = OnActorSetLockingEnemy;
            }
        }

        private void OnOtherActorDead(SActor obj)
        {
            if (obj.BaseInfo.m_ActorType == EActorType.Boss)
            {
                GameApp.Entry.UI.ShowPopScreen(Wnd_PopScreen.EStyle.BossDead);
            }
        }

        Vector3 GetPlayerPosWhenEnterScene(out Quaternion rot)
        {
            if (m_LoadType == ELoadType.NewGame)
            {
                ScenePoint point = m_ScenePoints.FirstOrDefault(a => a.m_PointType == EScenePointType.PlayerBornPosition);
                if (point != null)
                {
                    return point.GetFixedBornPos(out rot);
                }
            }
            else if (m_LoadType == ELoadType.ToNextSceneByPortal)
            {
                ScenePoint point = m_ScenePoints.FirstOrDefault(a => a.m_PointType == EScenePointType.Portal && a.m_ID == m_TransmittingPortalID);
                if (point != null)
                {
                    return point.GetFixedBornPos(out rot);
                    //return point.GetPortalFixedPos(out rot);
                }
            }
            else if (m_CurrentStayingShenKan != null)
            {
                return m_CurrentStayingShenKan.GetShenKanFixedPos(out rot);
            }
            else
            {
                throw new InvalidOperationException($"Unknown load type:{m_LoadType}");
            }

            rot = Quaternion.identity;
            return Vector3.zero;
        }

        IEnumerator CreatePlayer()
        {
            var ai = GameApp.Entry.Game.PlayerAI;
            Vector3 playerPos = GetPlayerPosWhenEnterScene(out var playerRot);

            if (m_Player)
            {
                m_Player.gameObject.SetActive(true);
                m_Player.transform.position = playerPos;
                m_Player.transform.rotation = playerRot;

                m_WndLoading.Percent = 70;
            }
            else
            {
                int id = GameApp.Entry.Config.GameSetting.PlayerID;

                yield return SActor.Create(id, playerPos, playerRot, ai, EActorCamp.Player, actor => m_Player = actor);
                m_Player.name += "(Player)";
                m_Player.Event_OnDeadAnimPlayFinished += OnPlayerDead;

                GameObject.DontDestroyOnLoad(m_Player.gameObject);
                yield return null;
                /*
                if (GameApp.Entry.Game.ProgressMgr.Clothes != null &&
                    GameApp.Entry.Game.ProgressMgr.Clothes.Length > 0)
                {
                    m_Player.CDressUp.DressClothes(GameApp.Entry.Game.ProgressMgr.Clothes);
                }
                else
                {
                    m_Player.CDressUp.DressStartClothes();
                }

                yield return null;
                */

                ai.OnSetLockingEnemy = OnActorSetLockingEnemy;

                for (int i = 0; i < 10; i++)
                {
                    m_WndLoading.Percent = 65 + i;
                    yield return null;
                }
            }

            m_Player.CMelee.CWeapon.ShowOrHideWeapon(true);
            yield return null;
            GameApp.Entry.Game.PlayerCamera.LookAtTarget(m_Player.transform.rotation.eulerAngles.y + 30);
            GameApp.Entry.Game.PlayerCamera.ResetPosition();

            /*
            if (m_Butterfly == null)
                m_Butterfly = SSpirit.Create("Actor/Spirit/Butterfly");
            m_Butterfly.SetFollowTarget(m_Player.transform, new Vector3(-0.2f, m_Player.CPhysic.Height, 0.4f));
            */
        }

        private void OnPlayerDead(SActor obj)
        {
            GameApp.Entry.Game.Audio.Play2DSound("Sound/Game/GameLose");
            //GameApp.Entry.UI.ShowTips("你被击败了！", 5);
            GameApp.Entry.Unity.StartCoroutine(RebirthPlayerItor());
            GameApp.Entry.UI.ShowPopScreen(Wnd_PopScreen.EStyle.PlayerDead);
        }

        void OnActorSetLockingEnemy(SActor owner, SActor enemy)
        {
            if (owner.BaseInfo.m_ActorType == EActorType.Boss && owner is SMonster monster)
            {
                OnBeginBossFighting(enemy == m_Player ? monster : null);
            }
        }

        void OnBeginBossFighting(SMonster fightingBoss)
        {
            CurrentFightingBoss = fightingBoss;
            Event_OnStartOrEndFightingBoss?.Invoke(CurrentFightingBoss);

            foreach (var p in m_ScenePoints)
            {
                if (p.m_PointType == EScenePointType.ShenKan)
                {
                    p.SetActive(!fightingBoss);
                }
                else if (p.m_PointType == EScenePointType.Portal)
                {
                    p.SetActive(!fightingBoss);
                }
                else if (p.m_PointType == EScenePointType.MonsterBornPosition && p is ScenePointMonster monserPoint)
                {
                    if (monserPoint.Actor != CurrentFightingBoss)
                    {
                        monserPoint.SetActive(!fightingBoss);
                    }
                }
            }
        }

        public void SetFilmEffect(bool open)
        {
            GameSetting.ActiveVignette = open;
            GameSetting.ActiveDepthOfField = open;
            if (m_WndJoyStick)
            {
                m_WndJoyStick.ActiveSticks = !open;
            }

            if (open)
            {
                RootUI.Instance.HideAllNormalWnd(nameof(Wnd_JoyStick));
            }
            else
            {
                RootUI.Instance.RevertHideAllNormalWnd();
            }
        }

        #endregion


        #region 复活

        /// <summary>死亡后重生，回到上次存档点</summary>
        IEnumerator RebirthPlayerItor()
        {
            yield return new WaitForSeconds(GameApp.Entry.Config.GameSetting.PlayerRebirthDelaySeconds);

            m_Player.RecoverOrigin();
            yield return null;

            yield return BackToLastShenKan();

            RecorverOtherActors();
            yield return null;

            //Timeline += 6;
        }

        Coroutine BackToLastShenKan()
        {
            if (GameApp.Entry.Game.ProgressMgr.HasSavePointBefore)
            {
                int sceneID = GameApp.Entry.Game.ProgressMgr.LastStayingSceneID;
                m_SceneInfo = GameApp.Entry.Config.SceneInfo.GetSceneInfoByID(sceneID);
                m_TransmittingShenKanID = GameApp.Entry.Game.ProgressMgr.LastStayingShenKanID;
                return GameApp.Entry.Unity.StartCoroutine(LoadItor(ELoadType.ToLastShenKan, null));
            }
            else
            {
                return GameApp.Entry.Unity.StartCoroutine(LoadItor(ELoadType.NewGame, null));
            }
        }

        /// <summary>其它角色复原</summary>
        public void RecorverOtherActors()
        {
            RecorverOtherActorsItor().StartCoroutine();
        }

        IEnumerator RecorverOtherActorsItor()
        {
            foreach (var p in m_ScenePoints)
            {
                if (p is ScenePointShenKan ip && ip != m_CurrentStayingShenKan)
                {
                    foreach (var mp in ip.LinkMonsterPoints)
                        mp.Destroy();
                }
            }

            yield return null;

            if (m_CurrentStayingShenKan == null)
            {
                yield break;
            }

            foreach (var p in m_CurrentStayingShenKan.LinkMonsterPoints)
            {
                yield return CreateActor(p).StartCoroutine();
            }
        }

        #endregion


        public void Update(float deltaTime)
        {
        }

        public bool GoNearestShenKan()
        {
            //GameApp.Entry.UI.CreateWnd<Wnd_SelectWeapon>(null, this);
            if (m_Player.AI.LockingEnemy != null)
            {
                GameApp.Entry.UI.ShowTips("战斗中不可执行此操作");
                return false;
            }

            if (m_Player.PlayAction(PlayActionState.EActionType.ShenKanRest, () => BackToNearestShenKan()))
            {
                SetFilmEffect(true);
                m_Player.CMelee.CWeapon.ShowOrHideWeapon(false);
                return true;
            }
            else
            {
                GameApp.Entry.UI.ShowTips("当前状态不能执行该操作");
                GameApp.Entry.Game.Audio.PlaySoundSkillFailed();
                return false;
            }
        }

        Coroutine BackToNearestShenKan()
        {
            if (GameApp.Entry.Game.ProgressMgr.HasSavePointBefore)
            {
                m_TransmittingShenKanID = -1;
                if (m_SceneInfo != null)
                {
                    float minDis = float.MaxValue;
                    foreach (var scenePoint in m_ScenePoints)
                    {
                        if (scenePoint.m_PointType != EScenePointType.ShenKan)
                        {
                            continue;
                        }

                        ScenePointShenKan shenKanPoint = (ScenePointShenKan)scenePoint;

                        if (!shenKanPoint.ShenKanObj.IsActived)
                        {
                            continue;
                        }

                        float dis = Vector3.Distance(m_Player.transform.position, scenePoint.transform.position);
                        if (dis < minDis)
                        {
                            minDis = dis;
                            m_TransmittingShenKanID = scenePoint.m_ID;
                        }
                    }
                }

                if (m_TransmittingShenKanID < 0)
                {
                    return BackToLastShenKan();
                }

                return GameApp.Entry.Unity.StartCoroutine(LoadItor(ELoadType.ToNearestShenKan, null));
            }
            else
            {
                return GameApp.Entry.Unity.StartCoroutine(LoadItor(ELoadType.NewGame, null));
            }
        }


        #region Wnd_MainCity.IHandler

        void Wnd_MainCity.IHandler.OnClickMenu()
        {
            GameApp.Entry.UI.CreateWnd<Wnd_CharacterInfo>(null, this, null);
        }

        void Widget_SlotObject.IHandler.OnClickSlot(MainWndSlotData slotData)
        {
            if (slotData.m_SlotType == Widget_SlotObject.ESlotDataType.TheurgyItem)
            {
                var skillObj = GameApp.Entry.Game.Player.CMelee.GetTheurgy(slotData.m_ID, out _);
                if (skillObj != null)
                {
                    GameApp.Entry.Game.PlayerAI.TryTriggerSkill(skillObj.SkillConfig.m_SkillType);
                }
            }
            else if (slotData.m_SlotType == Widget_SlotObject.ESlotDataType.PropItem)
            {
                if (GameApp.Entry.Game.Bag.GetItemCount(slotData.m_ID) > 0)
                {
                    GameApp.Entry.Game.Bag.UseItem(slotData.m_ID);
                }
                else
                {
                    m_Player.PlayAction(PlayActionState.EActionType.UseItemNone);
                    GameApp.Entry.UI.ShowTips("道具数量不足");
                    GameApp.Entry.Game.Audio.PlaySoundSkillFailed();
                }
            }
            else
            {
                Debug.LogError($"Unknown slot type:{slotData.m_SlotType}");
            }
        }

        #endregion


        #region Wnd_CharacterInfo.IHandler

        void Wnd_CharacterInfo.IHandler.OnClickExitGame()
        {
            GameApp.Entry.Game.ProgressMgr.Save();
            DirectorLogin dirLogin = new();
            GameApp.Instance.TryEnterNextDir(dirLogin);
        }

        void Wnd_CharacterInfo.IHandler.OnSlotChange()
        {
            m_WndMainCity.ResetSlots();
        }

        /*
        void Wnd_Menu.IHandler.OnClickSave()
        {
            GameApp.Entry.Game.ProgressMgr.Save();
        }

        void Wnd_SelectEnemy.IHandler.OnClickConfirm(ActorItemInfo enemyInfo, int option)
        {
            EEnemyAIType fightType = (EEnemyAIType)option;
            CreateEnemy(enemyInfo.m_ID, fightType, 1);
        }

        void CreateEnemy(int id, EEnemyAIType aiType, int count)
        {
            if (id < 0)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                Vector3 rayOriginPos = m_Player.transform.position +
                                       m_Player.transform.forward * 10 +
                                       m_Player.transform.right * (-count / 2f + i) +
                                       Vector3.up * 100;
                Vector3 pos;
                if (Physics.Raycast(rayOriginPos, Vector3.down, out var hitInfo, 200, EStaticLayers.Default.GetLayerMask()))
                {
                    pos = hitInfo.point;
                }
                else
                {
                    pos = m_Player.transform.position + Vector3.up * 10;
                }

                var ai = aiType.CreateEnemyAI();
                var enemy = SActor.Create(id, pos, Quaternion.identity, ai, EActorCamp.Monster);
                //enemy.CStats.ResetMaxHP(GameApp.Entry.Config.GameSetting.EnemyMaxHP);
                enemy.Event_OnDead += OnOtherActorDead;

                if (id == m_PlayerID)
                {
                    enemy.SetVariantColor(Color.red);
                }

                RegisterOtherActor(enemy);
            }
        }
        */

        #endregion

        /*
        #region Wnd_DressUp.IHandler

        void Widget_DressUp.IHandler.OnClickDressUp(int id, Action onFinished)
        {
            if (m_Player.CDressUp != null)
            {
                if (m_Player.CDressUp.IsDressing(id))
                {
                    m_Player.CDressUp.UndressCloth(id);
                    onFinished?.Invoke();
                }
                else
                {
                    m_Player.CDressUp.DressCloth(id, onFinished);
                }
            }
        }

        bool Widget_DressUp.IHandler.IsDressing(int id)
        {
            if (m_Player.CDressUp != null)
            {
                return m_Player.CDressUp.IsDressing(id);
            }

            return false;
        }

        void Widget_DressUp.IHandler.OnCloseWnd()
        {
            EndDressUp().StartCoroutine();
        }

        void Widget_DressUp.IHandler.TakeOffAllClothes()
        {
            m_Player.CDressUp.UndressAll();
        }

        #endregion
        */


        #region Wnd_Rest.IHandler

        void Wnd_Rest.IHandler.OnClickQuit()
        {
            QuitShenKanRest();
        }

        void QuitShenKanRest()
        {
            GameApp.Entry.Game.World.SetFilmEffect(false);

            GameApp.Entry.Game.PlayerAI.Active = true;
            GameApp.Entry.Game.PlayerAI.OnPlayerEnterGodStatue(m_CurrentStayingShenKan.ShenKanObj);

            m_Player.PlayAction(PlayActionState.EActionType.ShenKanRestEnd, () => m_Player.CMelee.CWeapon.ShowOrHideWeapon(true));
        }

        void Wnd_Rest.IHandler.OnClickTransmit(int sceneID, int shenKanID)
        {
            TransmitItor(sceneID, shenKanID).StartCoroutine();
        }

        void Wnd_Rest.IHandler.OnClickDressUp()
        {
            if (m_Player.CDressUp == null)
            {
                Debug.LogError("m_Player.CDressUp == null");
                return;
            }

            StartDressUp().StartCoroutine();
        }

        IEnumerator StartDressUp()
        {
            /*
            // 打开界面
            Wnd_DressUp.Content content = new()
            {
                m_ListClothes = GameApp.Entry.Config.ClothInfo.GetAllClothesID(),
            };
            GameApp.Entry.UI.CreateWnd<Wnd_DressUp>(content, this, null);
            */

            m_WndRest.ActiveRoot = false;
            Vector3 dir = -m_CurrentStayingShenKan.transform.right;
            Quaternion q = Quaternion.LookRotation(dir);
            GameApp.Entry.Game.PlayerCamera.LookAtTarget(q.eulerAngles.y);
            GameApp.Entry.Game.PlayerCamera.CameraStyle = PlayerCamera.ECameraStyle.DressUp;

            // 人物站立起来
            bool wait = true;
            m_Player.PlayAction(PlayActionState.EActionType.ToDressUp, () => wait = false);
            while (wait)
            {
                yield return null;
            }
        }

        IEnumerator EndDressUp()
        {
            Vector3 dir = -m_CurrentStayingShenKan.transform.forward;
            Quaternion q = Quaternion.LookRotation(dir);
            GameApp.Entry.Game.PlayerCamera.LookAtTarget(q.eulerAngles.y);
            GameApp.Entry.Game.PlayerCamera.CameraStyle = PlayerCamera.ECameraStyle.Normal;
            yield return null;

            GameApp.Entry.Game.ProgressMgr.Save();
            yield return null;

            // 
            bool wait = true;
            Vector3 shenKanRestPos = CurrentStayingShenKan.GetShenKanFixedPos(out _);
            m_Player.CStateMachine.SetPosAndForward(shenKanRestPos, -CurrentStayingShenKan.transform.forward, () => wait = false);
            while (wait)
            {
                yield return null;
            }

            m_Player.CMelee.CWeapon.ShowOrHideWeapon(false);
            yield return null;

            wait = true;
            m_Player.PlayAction(PlayActionState.EActionType.ShenKanRest, () => wait = false);
            while (wait)
            {
                yield return null;
            }

            m_WndRest.ActiveRoot = true;
        }

        IEnumerator TransmitItor(int sceneID, int shenKanID)
        {
            GameApp.Entry.Game.PlayerAI.Active = true;

            if (m_CurrentStayingShenKan != null &&
                sceneID == m_CurrentStayingShenKan.ShenKanObj.SceneID &&
                shenKanID == m_CurrentStayingShenKan.ShenKanObj.ID)
            {
                QuitShenKanRest();
                yield break;
            }

            OnPlayerExit(m_CurrentStayingShenKan.ShenKanObj);

            /*
            bool wait = true;
            m_Player.CStateMachine.PlayAction_BranchTeleport(() => wait = false);
            while (wait)
            {
                yield return null;
            }
            */
            yield return null;

            m_SceneInfo = GameApp.Entry.Config.SceneInfo.GetSceneInfoByID(sceneID);
            m_TransmittingShenKanID = shenKanID;
            yield return GameApp.Entry.Unity.StartCoroutine(LoadItor(ELoadType.ToShenKan, null));
        }

        /*
        public void CreateEnemy(int actorID)
        {
            DestroyOtherActors();
            var firstPoint = m_ScenePoints.FirstOrDefault(a => a.m_PointType == EScenePointType.MonsterBornPosition);
            CreateActor(firstPoint, actorID);
        }
        */

        #endregion


        #region Portal.IHandler

        void Portal.IHandler.OnPlayerEnter(Portal portal)
        {
            GameApp.Entry.Game.PlayerAI.OnPlayerEnterPortal(portal);
        }

        void Portal.IHandler.OnPlayerExit(Portal portal)
        {
            GameApp.Entry.Game.PlayerAI.OnPlayerExitPortal(portal);
        }

        void Portal.IHandler.OnPlayerTransmit(Portal portal)
        {
            PlayerTransmitByPortal(portal).StartCoroutine();
        }

        IEnumerator PlayerTransmitByPortal(Portal portal)
        {
            //GameApp.Entry.Game.PlayerCamera.LookAtTarget(portal.transform.rotation.eulerAngles.y + 150);

            Vector3 dirFromPortal = m_Player.transform.position - portal.transform.position;
            dirFromPortal.y = 0;
            Vector3 startPos = portal.transform.position + Vector3.Project(dirFromPortal, portal.transform.forward);
            Vector3 tarForward = portal.transform.position - startPos;
            bool wait = true;
            bool succeed = m_Player.CStateMachine.SetPosAndForward(startPos, tarForward, () => wait = false);
            if (!succeed)
            {
                GameApp.Entry.UI.ShowTips("当前状态不能执行该操作");
                yield break;
            }

            m_WndJoyStick.ActiveSticks = false;
            GameApp.Entry.Game.PlayerAI.OnPlayerExitPortal(portal);
            SetFilmEffect(true);

            while (wait)
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.5f);

            yield return GameApp.Entry.Game.PlayerAI.PlayActionMoveToTargetPos(portal.transform.position, () =>
            {
                m_CurrentUsingPortalInfo = portal;
                Load(ELoadType.ToNextSceneByPortal, null);
            });
        }

        #endregion


        #region ShenKan.IHandler

        void ShenKan.IHandler.OnPlayerEnter(ShenKan shenKan)
        {
            m_CurrentStayingShenKan = shenKan.Point;
            GameApp.Entry.Game.PlayerAI.OnPlayerEnterGodStatue(shenKan);
        }

        public void OnPlayerExit(ShenKan shenKan)
        {
            GameApp.Entry.Game.PlayerAI.OnPlayerExitGodStatue(shenKan);
        }

        void ShenKan.IHandler.OnPlayerActiveFire(ShenKan shenKan)
        {
            //GameApp.Entry.Game.PlayerCamera.LookAtTarget(godStatue.transform.rotation.eulerAngles.y + 150);

            GameApp.Entry.Game.PlayerAI.ActiveShenKan(shenKan, () =>
            {
                GameApp.Entry.Game.ProgressMgr.Save();
                shenKan.RefreshFire();
            });
        }

        Coroutine ShenKan.IHandler.OnPlayerRest(ShenKan shenKan)
        {
            return PlayerRestOnShenKanItor(shenKan).StartCoroutine();
        }

        IEnumerator PlayerRestOnShenKanItor(ShenKan shenKan)
        {
            bool wait = true;
            Vector3 shenKanRestPos = shenKan.Point.GetShenKanFixedPos(out _);
            bool succeed = m_Player.CStateMachine.SetPosAndForward(shenKanRestPos, -shenKan.transform.forward, () => wait = false);
            if (!succeed)
            {
                GameApp.Entry.UI.ShowTips("当前状态不能执行该操作");
                yield break;
            }

            GameApp.Entry.Game.World.SetFilmEffect(true);

            m_WndRest = null;
            yield return GameApp.Entry.UI.CreateWnd<Wnd_Rest>(null, this, w => m_WndRest = w);
            m_WndRest.ActiveRoot = false;

            while (wait)
            {
                yield return null;
            }

            GameApp.Entry.Game.ProgressMgr.Save();
            yield return null;

            m_Player.CMelee.CWeapon.ShowOrHideWeapon(false);
            yield return null;

            wait = true;
            m_Player.PlayAction(PlayActionState.EActionType.ShenKanRest, () => wait = false);
            while (wait)
            {
                yield return null;
            }

            m_WndRest.ActiveRoot = true;
            m_Player.OnShenKanRest();

            yield return null;

            GameApp.Entry.Game.World.RecorverOtherActors();
            yield return null;
        }

        #endregion

        public void Release()
        {
            if (m_Player)
            {
                m_Player.Destroy();
            }

            DestroyOtherActors();

            if (m_WndMainCity)
            {
                m_WndMainCity.Destroy();
            }

            GameApp.Entry.Game.PlayerAI.Release();

            if (m_EffectWitchTimeBoom)
            {
                GameObject.Destroy(m_EffectWitchTimeBoom);
            }
        }
    }
}