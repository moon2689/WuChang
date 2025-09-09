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
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;

namespace Saber.World
{
    public class BigWorld : Wnd_MainCity.IHandler
        , Wnd_Menu.IHandler
        , Wnd_SelectWeapon.IHandler
        , Wnd_DressUp.IHandler
        , Portal.IHandler
        , Idol.IHandler
    {
        public enum ELoadType
        {
            None,
            NewGame,
            ToLastIdol,
            ToNextSceneByPortal,
            ToIdol,
        }


        public Action OnSetLockingEnemyEvent;


        private SActor m_Player;

        //private SSpirit m_Butterfly;
        private ELoadType m_LoadType;
        private SceneBaseInfo m_SceneInfo;

        private Wnd_MainCity m_WndMainCity;

        // private AzureTimeController m_AzureTime;
        // private AzureWeatherController m_AzureWeather;
        // private AzureEffectsController m_AzureEffects;
        private Wnd_Loading m_WndLoading;
        private Portal m_CurrentUsingPortalInfo;
        private Idol m_CurrentStayingIdol;
        private int m_TransmittingPortalID;
        private int m_TransmittingIdolID;
        private Coroutine m_CoroutineWitchTime;
        private Dictionary<SActor, CharacterAnimSpeedModifier> m_DicModifierWitchTimes = new();
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


        #region Load

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
            else if (loadType == ELoadType.ToLastIdol)
            {
                int sceneID = GameProgressManager.Instance.LastStayingSceneID;
                m_SceneInfo = GameApp.Entry.Config.SceneInfo.GetSceneInfoByID(sceneID);
                m_TransmittingIdolID = GameProgressManager.Instance.LastStayingIdolID;
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

            Wnd_JoyStick wndJoyStick = null;
            yield return GameApp.Entry.UI.CreateWnd<Wnd_JoyStick>(null, null, w => wndJoyStick = w);
            wndJoyStick.ActiveSticks = true;

            // scene
            yield return LoadScene().StartCoroutine();

            // player
            yield return CreatePlayer().StartCoroutine();

            // other actors
            yield return CreateOtherActors().StartCoroutine();

            // wnd
            if (m_WndMainCity == null)
            {
                yield return GameApp.Entry.UI.CreateWnd<Wnd_MainCity>(null, this, w => m_WndMainCity = w);
            }

            // effects
            GameApp.Entry.Config.GameSetting.PreloadEffects();
            yield return null;

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

            m_WndLoading.Percent = 100;
            //yield return new WaitForSeconds(0.1f);
            m_WndLoading.Destroy();

            /*
            if (m_LoadType == ELoadType.NewGame)
            {
                // 新游戏cg动画
                m_LoadType = ELoadType.None;
                
                Vector3 position = GameApp.Entry.Config.GameSetting.m_BornPos;
                Quaternion rotation = Quaternion.Euler(0, GameApp.Entry.Config.GameSetting.m_BornRotY, 0);
                TimelineManager timelineManager = TimelineManager.Create("CGNewGame");
                timelineManager.BindPlayer();
                timelineManager.Play(position, rotation, null);
                
                m_Player.transform.position = GameApp.Entry.Config.GameSetting.m_BornPos;
                m_Player.transform.rotation = Quaternion.Euler(0, GameApp.Entry.Config.GameSetting.m_BornRotY, 0);
            }
            else if (m_LoadType == ELoadType.ToNextSceneByPortal)
            {
                // 走出传送门动画
                var curPortal = m_TransmittingPortalID.PortalObject;
                GameApp.Entry.Game.PlayerCamera.LookAtTarget(curPortal.transform.rotation.eulerAngles.y + 150);
                curPortal.EnableGateCollider(false);
                Vector3 targetPos = m_TransmittingPortalID.PortalObject.transform.position + curPortal.transform.forward * 1f;
                yield return GameApp.Entry.Game.PlayerAI.PlayActionMoveToTargetPos(targetPos, 0, () => { curPortal.EnableGateCollider(true); });
            }
            */

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

                    if (scenePoint.PortalObj != null)
                    {
                        continue;
                    }

                    ++count;
                    m_WndLoading.Percent = 50 + 10 * count / portalCount;
                    AssetHandle assetHandle = GameApp.Entry.Asset.LoadGameObject("SceneProp/Portal", portalObj =>
                    {
                        portalObj.name = scenePoint.m_ID.ToString();
                        Portal portal = portalObj.GetComponent<Portal>();
                        portal.Init(scenePoint, parentPortal.transform, this);
                    });
                    while (!assetHandle.IsDone)
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

            // 雕像（存档点）
            int idolCount = m_ScenePoints.Count(a => a.m_PointType == EScenePointType.Idol);
            if (idolCount > 0)
            {
                string statueParentName = "Idols";
                GameObject parentStatue = GameObject.Find(statueParentName);
                if (!parentStatue)
                    parentStatue = new GameObject(statueParentName);
                int count = 0;
                foreach (var scenePoint in m_ScenePoints)
                {
                    if (scenePoint.m_PointType != EScenePointType.Idol)
                    {
                        continue;
                    }

                    if (scenePoint.IdolObj != null)
                    {
                        continue;
                    }

                    ++count;
                    m_WndLoading.Percent = 60 + 5 * count / idolCount;
                    AssetHandle assetHandle = GameApp.Entry.Asset.LoadGameObject("SceneProp/GodStatue", godStatueObj =>
                    {
                        godStatueObj.name = scenePoint.m_ID.ToString();
                        Idol idol = godStatueObj.GetComponent<Idol>();
                        idol.Init(m_SceneInfo.m_ID, scenePoint, parentStatue.transform, this);
                    });
                    while (!assetHandle.IsDone)
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
                    if (p.m_PointType == EScenePointType.MonsterBornPosition && p.Actor)
                        p.Actor.Destroy();
                }
            }
        }

        private IEnumerator CreateOtherActors()
        {
            DestroyOtherActors();
            yield return null;

            int totalCount = m_ScenePoints.Count(a => a.m_PointType == EScenePointType.MonsterBornPosition);
            if (totalCount <= 0)
            {
                yield break;
            }

            int count = 0;
            foreach (var scenePoint in m_ScenePoints)
            {
                if (scenePoint.m_PointType != EScenePointType.MonsterBornPosition)
                {
                    continue;
                }

                ++count;
                CreateActor(scenePoint);
                m_WndLoading.Percent = 70 + 20 * count / totalCount;
                yield return null;
            }
        }

        void CreateActor(ScenePoint bornPoint, int id = -1)
        {
            int enemyID = id > 0 ? id : bornPoint.m_ID;

            EAIType aiType = GameApp.Entry.Config.TestGame.TestSkill ? EAIType.TestSkill : bornPoint.m_AIType;
            EnemyAIBase ai = aiType.CreateEnemyAI();
            var camp = EActorCamp.Monster;
            Vector3 pos = bornPoint.GetFixedBornPos(out var rot);
            SActor.Create(enemyID, pos, rot, ai, camp, actor =>
            {
                actor.Event_OnDead += OnOtherActorDead;
                bornPoint.Actor = actor;
            });
        }

        private void OnOtherActorDead(SActor obj)
        {
            // todo
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
            else if (m_LoadType == ELoadType.ToIdol || m_LoadType == ELoadType.ToLastIdol)
            {
                ScenePoint point = m_ScenePoints.FirstOrDefault(a => a.m_PointType == EScenePointType.Idol && a.m_ID == m_TransmittingIdolID);
                if (point != null)
                {
                    return point.GetIdolFixedPos(out rot);
                }
            }
            else if (m_LoadType == ELoadType.ToNextSceneByPortal)
            {
                ScenePoint point = m_ScenePoints.FirstOrDefault(a => a.m_PointType == EScenePointType.Portal && a.m_ID == m_TransmittingPortalID);
                if (point != null)
                {
                    return point.GetPortalFixedPos(out rot);
                }
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
                ai.ClearLockEnemy();

                m_WndLoading.Percent = 70;
            }
            else
            {
                int id = GameApp.Entry.Config.GameSetting.PlayerID;

                yield return SActor.Create(id, playerPos, playerRot, ai, EActorCamp.Player, actor => m_Player = actor);
                m_Player.name += "(Player)";
                m_Player.Event_OnDead += OnPlayerDead;

                GameObject.DontDestroyOnLoad(m_Player.gameObject);
                yield return null;
                /*
                if (GameProgressManager.Instance.Clothes != null &&
                    GameProgressManager.Instance.Clothes.Length > 0)
                {
                    m_Player.CDressUp.DressClothes(GameProgressManager.Instance.Clothes);
                }
                else
                {
                    m_Player.CDressUp.DressStartClothes();
                }

                yield return null;
                */

                ai.OnSetLockingEnemy = OnSetLockingEnemy;

                for (int i = 0; i < 10; i++)
                {
                    m_WndLoading.Percent = 65 + i;
                    yield return null;
                }
            }

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
            GameApp.Entry.UI.ShowTips("你被击败了！", 5);
            GameApp.Entry.Unity.StartCoroutine(RebirthPlayerItor());
        }

        void OnSetLockingEnemy()
        {
            OnSetLockingEnemyEvent?.Invoke();
        }

        #endregion


        #region 复活

        /// <summary>死亡后重生，回到上次存档点</summary>
        IEnumerator RebirthPlayerItor()
        {
            yield return new WaitForSeconds(8);

            m_Player.Rebirth();
            yield return null;

            yield return BackToLastGodStatue();

            RecorverOtherActors();
            yield return null;

            //Timeline += 6;
        }

        Coroutine BackToLastGodStatue()
        {
            if (GameProgressManager.Instance.HasSavePointBefore)
            {
                int sceneID = GameProgressManager.Instance.LastStayingSceneID;
                m_SceneInfo = GameApp.Entry.Config.SceneInfo.GetSceneInfoByID(sceneID);
                m_TransmittingIdolID = GameProgressManager.Instance.LastStayingIdolID;
                return GameApp.Entry.Unity.StartCoroutine(LoadItor(ELoadType.ToLastIdol, null));
            }
            else
            {
                return GameApp.Entry.Unity.StartCoroutine(LoadItor(ELoadType.NewGame, null));
            }
        }

        /// <summary>其它角色复原</summary>
        public void RecorverOtherActors()
        {
            if (m_SceneInfo == null)
            {
                return;
            }

            foreach (var p in m_ScenePoints)
            {
                if (p.m_PointType != EScenePointType.MonsterBornPosition)
                {
                    continue;
                }

                var actor = p.Actor;
                if (actor != null)
                {
                    if (actor.IsDead)
                    {
                        actor.Rebirth();
                    }
                    else
                    {
                        actor.CStats.Reset();
                    }

                    Vector3 pos = p.GetFixedBornPos(out var rot);
                    actor.transform.position = pos;
                    actor.transform.rotation = rot;
                }
                else
                {
                    CreateActor(p);
                }
            }
        }

        #endregion


        public void Update(float deltaTime)
        {
            if (m_Player && m_Player.gameObject.activeSelf && m_Player.CurrentStateType == EStateType.Fall)
            {
                if (m_Player.transform.position.y < -200)
                {
                    m_Player.gameObject.SetActive(false);
                    BackToLastGodStatue();
                }
            }
        }


        #region Wnd_MainCity.IHandler

        public void OnClickMenu()
        {
            GameApp.Entry.UI.CreateWnd<Wnd_Menu>(null, this, null);
        }

        #endregion


        #region Wnd_Menu.IHandler

        void Wnd_Menu.IHandler.OnClickBackToLastGodStatue()
        {
            //GameApp.Entry.UI.CreateWnd<Wnd_SelectWeapon>(null, this);
            if (m_Player.AI.LockingEnemy != null)
            {
                GameApp.Entry.UI.ShowTips("战斗中不可执行此操作");
                return;
            }

            GoHome().StartCoroutine();
        }

        IEnumerator GoHome()
        {
            bool wait = true;
            if (m_Player.CStateMachine.PlayAction_GoHome(() => wait = false))
            {
                m_Player.CMelee.CWeapon.ToggleWeapon(false);
                while (wait)
                {
                    yield return null;
                }

                m_Player.CMelee.CWeapon.ToggleWeapon(true);

                yield return BackToLastGodStatue();
            }
            else
            {
                GameApp.Entry.UI.ShowTips("当前状态不能执行该操作");
                GameApp.Entry.Game.Audio.PlaySoundSkillFailed();
            }
        }

        void Wnd_Menu.IHandler.OnClickToMainWnd()
        {
            DirectorLogin dirLogin = new();
            GameApp.Instance.TryEnterNextDir(dirLogin);
        }

        void Wnd_Menu.IHandler.OnClickDressUp()
        {
            if (m_Player.CDressUp != null)
            {
                Wnd_DressUp.Content content = new()
                {
                    m_ListClothes = GameApp.Entry.Config.ClothInfo.GetAllClothesID(),
                };
                GameApp.Entry.UI.CreateWnd<Wnd_DressUp>(content, this, null);
            }
        }

        void Wnd_Menu.IHandler.OnClickWait()
        {
            GameApp.Entry.UI.CreateWnd<Wnd_Wait>(null);
        }

        /*
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


        #region Wnd_DressUp.IHandler

        void Wnd_DressUp.IHandler.OnClickDressUp(int id, Action onFinished)
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

        bool Wnd_DressUp.IHandler.IsDressing(int id)
        {
            if (m_Player.CDressUp != null)
            {
                return m_Player.CDressUp.IsDressing(id);
            }

            return false;
        }

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
            portal.EnableGateCollider(false);
            //GameApp.Entry.Game.PlayerCamera.LookAtTarget(portal.transform.rotation.eulerAngles.y + 150);
            GameApp.Entry.Game.PlayerAI.OnPlayerExitPortal(portal);

            Vector3 startPos = portal.transform.position + portal.transform.forward * 0.8f;
            Vector3 dir = portal.transform.position - m_Player.transform.position;
            bool wait = true;
            m_Player.CStateMachine.SetPosAndForward(startPos, dir, 0.2f, () => wait = false);
            while (wait)
            {
                yield return null;
            }

            GameApp.Entry.Game.PlayerAI.PlayActionMoveToTargetPos(portal.transform.position, m_Player.CPhysic.Radius,
                () =>
                {
                    m_CurrentUsingPortalInfo = portal;
                    Load(ELoadType.ToNextSceneByPortal, null);
                });
        }

        #endregion


        #region GodStatue.IHandler

        void Idol.IHandler.OnPlayerEnter(Idol idol)
        {
            m_CurrentStayingIdol = idol;
            GameApp.Entry.Game.PlayerAI.OnPlayerEnterGodStatue(idol);
        }

        public void OnPlayerExit(Idol idol)
        {
            m_CurrentStayingIdol = null;
            GameApp.Entry.Game.PlayerAI.OnPlayerExitGodStatue(idol);
        }

        void Idol.IHandler.OnPlayerWorship(Idol idol)
        {
            //GameApp.Entry.Game.PlayerCamera.LookAtTarget(godStatue.transform.rotation.eulerAngles.y + 150);

            GameApp.Entry.Game.PlayerAI.ActiveIdol(idol, () =>
            {
                GameProgressManager.Instance.OnIdolFire(idol.SceneID, idol.ID);
                idol.RefreshFire();
            });
        }

        Coroutine Idol.IHandler.OnPlayerRest(Idol idol)
        {
            return GameApp.Entry.Game.PlayerAI.PlayerRestBeforeIdol(idol);
        }

        #endregion


        #region Wnd_Rest

        public void Transmit(int sceneID, int idolID)
        {
            TransmitItor(sceneID, idolID).StartCoroutine();
        }

        IEnumerator TransmitItor(int sceneID, int idolID)
        {
            GameApp.Entry.Game.PlayerAI.Active = true;

            if (m_CurrentStayingIdol != null &&
                sceneID == m_CurrentStayingIdol.SceneID &&
                idolID == m_CurrentStayingIdol.ID)
            {
                GameApp.Entry.Game.PlayerAI.OnPlayerEnterGodStatue(m_CurrentStayingIdol);
                yield break;
            }

            OnPlayerExit(m_CurrentStayingIdol);

            GameProgressManager.Instance.OnGodStatueRest(sceneID, idolID);

            bool wait = true;
            m_Player.CStateMachine.PlayAction_BranchTeleport(() => wait = false);
            while (wait)
            {
                yield return null;
            }

            m_SceneInfo = GameApp.Entry.Config.SceneInfo.GetSceneInfoByID(sceneID);
            m_TransmittingIdolID = idolID;
            yield return GameApp.Entry.Unity.StartCoroutine(LoadItor(ELoadType.ToIdol, null));
        }

        public void CreateEnemy(int actorID)
        {
            DestroyOtherActors();
            var firstPoint = m_ScenePoints.FirstOrDefault(a => a.m_PointType == EScenePointType.MonsterBornPosition);
            CreateActor(firstPoint, actorID);
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