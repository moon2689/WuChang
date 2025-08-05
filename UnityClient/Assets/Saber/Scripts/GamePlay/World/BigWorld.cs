using System;
using System.Collections;
using System.Collections.Generic;
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

namespace Saber.World
{
    public class BigWorld : Wnd_MainCity.IHandler
        , Wnd_Menu.IHandler
        , Wnd_SelectWeapon.IHandler
        , Wnd_DressUp.IHandler
        , Portal.IHandler
        , GodStatue.IHandler
        , Wnd_Rest.IHandler
    {
        public enum ELoadType
        {
            None,
            NewGame,
            ToLastGodStatue,
            ToNextSceneByPortal,
            ToGodStatue,
        }

        private SActor m_Player;

        //private SSpirit m_Butterfly;
        private ELoadType m_LoadType;
        private SceneBaseInfo m_SceneInfo;

        private Wnd_MainCity m_WndMainCity;

        // private AzureTimeController m_AzureTime;
        // private AzureWeatherController m_AzureWeather;
        // private AzureEffectsController m_AzureEffects;
        private Wnd_Loading m_WndLoading;
        private PortalPoint m_CurrentUsingPortalInfo;
        private Vector3 m_PlayerPos;
        private Quaternion m_PlayerRot;
        private GodStatue m_CurrentStayingGodStatue;
        private Dictionary<OtherActorBornPoint, SActor> m_DicOtherActors = new();
        private PortalPoint m_TransmittingPortal;
        private Coroutine m_CoroutineWitchTime;
        private Dictionary<SActor, CharacterAnimSpeedModifier> m_DicModifierWitchTimes = new();
        private EffectObject m_EffectWitchTimeBoom;
        private Dictionary<GodStatuePoint, GodStatue> m_DicGodStatues = new();


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

        public bool InWitchTime { get; private set; }


        public BigWorld()
        {
            GameObject effectWitchTimeBoom = GameApp.Entry.Asset.LoadGameObject("Particles/WitchTimeBoom");
            m_EffectWitchTimeBoom = effectWitchTimeBoom.GetComponent<EffectObject>();
        }


        #region Load

        public void Load(ELoadType loadType)
        {
            if (loadType == ELoadType.NewGame)
            {
                // 新游戏
                int sceneID = GameApp.Entry.Config.GameSetting.StartSceneID;
                m_SceneInfo = GameApp.Entry.Config.SceneInfo.GetSceneInfoByID(sceneID);
                m_PlayerPos = GameApp.Entry.Config.GameSetting.m_BornPos;
                m_PlayerRot = Quaternion.Euler(0, GameApp.Entry.Config.GameSetting.m_BornRotY, 0);
            }
            else if (loadType == ELoadType.ToNextSceneByPortal)
            {
                // 传送
                int sceneID = m_CurrentUsingPortalInfo.m_TargetSceneID;
                m_SceneInfo = GameApp.Entry.Config.SceneInfo.GetSceneInfoByID(sceneID);
                m_TransmittingPortal = m_SceneInfo.m_PortalPoints[m_CurrentUsingPortalInfo.m_TargetPortalIndex];
                Quaternion tarPortalRot = Quaternion.Euler(0, m_TransmittingPortal.m_RotationY, 0);
                m_PlayerPos = m_TransmittingPortal.m_Position;
                m_PlayerRot = tarPortalRot;
            }
            else if (loadType == ELoadType.ToLastGodStatue)
            {
                int sceneID = GameProgressManager.Instance.LastStayingSceneID;
                m_SceneInfo = GameApp.Entry.Config.SceneInfo.GetSceneInfoByID(sceneID);
                int statueIndex = GameProgressManager.Instance.LastStayingStatueIndex;
                var targetStatueInfo = m_SceneInfo.m_GodStatuePoint[statueIndex];
                Quaternion tarStatueRot = Quaternion.Euler(0, targetStatueInfo.m_RotationY, 0);
                m_PlayerPos = targetStatueInfo.m_Position + tarStatueRot * new Vector3(0, 0.2f, 1.2f);
                m_PlayerRot = Quaternion.Euler(0, targetStatueInfo.m_RotationY + 180, 0);
            }
            else
            {
                throw new InvalidOperationException($"Unknown load type:{loadType}");
            }

            GameApp.Entry.Unity.StartCoroutine(LoadItor(loadType));
        }

        IEnumerator LoadItor(ELoadType loadType)
        {
            m_LoadType = loadType;

            if (m_WndLoading == null)
                m_WndLoading = GameApp.Entry.UI.CreateWnd<Wnd_Loading>();

            // scene
            yield return LoadScene().StartCoroutine();

            // player
            yield return CreatePlayer().StartCoroutine();

            // other actors
            if (m_SceneInfo.m_ActiveOtherActors)
            {
                yield return CreateOtherActors().StartCoroutine();
            }

            // wnd
            if (m_WndMainCity == null)
            {
                m_WndMainCity = GameApp.Entry.UI.CreateWnd<Wnd_MainCity>(null, this);
            }

            m_WndLoading.Percent = 100;
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

            if (m_LoadType == ELoadType.NewGame)
            {
                // 新游戏cg动画
                m_LoadType = ELoadType.None;
                Vector3 position = GameApp.Entry.Config.GameSetting.m_BornPos;
                Quaternion rotation = Quaternion.Euler(0, GameApp.Entry.Config.GameSetting.m_BornRotY, 0);
                TimelineManager timelineManager = TimelineManager.Create("CGNewGame");
                timelineManager.BindPlayer();
                timelineManager.Play(position, rotation, null);
                /*
                m_Player.transform.position = GameApp.Entry.Config.GameSetting.m_BornPos;
                m_Player.transform.rotation = Quaternion.Euler(0, GameApp.Entry.Config.GameSetting.m_BornRotY, 0);
                */
            }
            else if (m_LoadType == ELoadType.ToNextSceneByPortal)
            {
                // 走出传送门动画
                var curPortal = m_TransmittingPortal.PortalObject;
                GameApp.Entry.Game.PlayerCamera.LookAtTarget(curPortal.transform.rotation.eulerAngles.y + 150);
                curPortal.EnableGateCollider(false);
                Vector3 targetPos = m_TransmittingPortal.PortalObject.transform.position +
                                    curPortal.transform.forward * 1f;
                yield return GameApp.Entry.Game.PlayerAI.PlayActionMoveToTargetPos(targetPos, 0,
                    () => { curPortal.EnableGateCollider(true); });
            }
        }

        IEnumerator LoadScene()
        {
            if (m_Player)
            {
                m_Player.gameObject.SetActive(false);
            }

            if (SceneManager.GetActiveScene().name != m_SceneInfo.m_SceneName)
            {
                // destroy actors
                DestroyOtherActors();

                AsyncOperation h = SceneManager.LoadSceneAsync(m_SceneInfo.m_SceneName, LoadSceneMode.Single);
                yield return h;
            }

            m_WndLoading.Percent = 50;

            OnSceneLoaded();
            yield return null;

            // 传送门
            if (m_SceneInfo.m_PortalPoints.Length > 0)
            {
                string portalParentName = "Portals";
                GameObject parentPortal = GameObject.Find(portalParentName);
                if (parentPortal)
                {
                    GameObject.Destroy(parentPortal);
                }

                parentPortal = new GameObject(portalParentName);

                for (int i = 0; i < m_SceneInfo.m_PortalPoints.Length; i++)
                {
                    var portalInfo = m_SceneInfo.m_PortalPoints[i];
                    if (!portalInfo.m_Active)
                    {
                        continue;
                    }

                    m_WndLoading.Percent = 50 + 10 * (i + 1) / m_SceneInfo.m_PortalPoints.Length;
                    GameObject portalObj = GameApp.Entry.Asset.LoadGameObject("SceneProp/Portal");
                    portalObj.name = i.ToString();
                    Portal portal = portalObj.GetComponent<Portal>();
                    portal.Init(portalInfo, parentPortal.transform, this);
                }
            }

            if (m_LoadType == ELoadType.ToNextSceneByPortal)
            {
                m_TransmittingPortal.PortalObject.EnableGateCollider(false);
            }

            m_WndLoading.Percent = 60;

            // 雕像（存档点）
            if (m_SceneInfo.m_GodStatuePoint.Length > 0)
            {
                string statueParentName = "Statues";
                GameObject parentStatue = GameObject.Find(statueParentName);
                if (parentStatue)
                {
                    GameObject.Destroy(parentStatue);
                }

                parentStatue = new GameObject(statueParentName);
                m_DicGodStatues.Clear();
                for (int i = 0; i < m_SceneInfo.m_GodStatuePoint.Length; i++)
                {
                    var statueInfo = m_SceneInfo.m_GodStatuePoint[i];
                    m_WndLoading.Percent = 60 + 10 * (i + 1) / m_SceneInfo.m_GodStatuePoint.Length;
                    GameObject godStatueObj = GameApp.Entry.Asset.LoadGameObject("SceneProp/GodStatue");
                    godStatueObj.name = i.ToString();
                    GodStatue godStatue = godStatueObj.GetComponent<GodStatue>();
                    godStatue.Init(m_SceneInfo.m_ID, i, statueInfo, parentStatue.transform, this);
                    m_DicGodStatues.Add(statueInfo, godStatue);
                }
            }

            GameSetting.URPAsset.shadowDistance = m_SceneInfo.m_ShadowDistance;

            m_WndLoading.Percent = 70;
        }

        void OnSceneLoaded()
        {
            // volume
            if (m_SceneInfo.m_OpenPostprocess)
            {
                GameObject.Instantiate(Resources.Load("Game/GlobalVolume"));
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
            foreach (var pair in m_DicOtherActors)
            {
                if (pair.Value)
                {
                    pair.Value.Destroy();
                }
            }

            m_DicOtherActors.Clear();
        }

        private IEnumerator CreateOtherActors()
        {
            DestroyOtherActors();
            yield return null;

            for (int i = 0; i < m_SceneInfo.m_OtherActorBornPoints.Length; i++)
            {
                var bornPoint = m_SceneInfo.m_OtherActorBornPoints[i];
                var actor = CreateActor(bornPoint);
                m_WndLoading.Percent = 70 + 20 * (i + 1) / m_SceneInfo.m_OtherActorBornPoints.Length;
                yield return null;
            }
        }

        SActor CreateActor(OtherActorBornPoint bornPoint, int id = -1)
        {
            Vector3 rayOriginPos = bornPoint.m_Position + Vector3.up * 100;
            int enemyID = id > 0 ? id : bornPoint.m_EnemyID;
            if (Physics.Raycast(rayOriginPos, Vector3.down, out var hitInfo, 200, EStaticLayers.Default.GetLayerMask()))
            {
                bornPoint.FixedBornPos = hitInfo.point;
            }
            else
            {
                Debug.LogError($"Born position is error, id:{enemyID}");
                bornPoint.FixedBornPos = bornPoint.m_Position + Vector3.up * 10;
            }

            var ai = bornPoint.m_AI.CreateEnemyAI();
            var camp = EActorCamp.Monster;
            var actor = SActor.Create(enemyID, bornPoint.FixedBornPos, bornPoint.BornRot, ai, camp);
            actor.Event_OnDead += OnOtherActorDead;

            m_DicOtherActors[bornPoint] = actor;

            return actor;
        }

        private void OnOtherActorDead(SActor obj)
        {
            // todo
        }

        IEnumerator CreatePlayer()
        {
            var ai = GameApp.Entry.Game.PlayerAI;

            if (m_Player)
            {
                m_Player.gameObject.SetActive(true);
                m_Player.transform.position = m_PlayerPos;
                m_Player.transform.rotation = m_PlayerRot;
                ai.ClearLockEnemy();

                m_WndLoading.Percent = 70;
            }
            else
            {
                int id = GameApp.Entry.Config.GameSetting.PlayerID;

                m_Player = SActor.Create(id, m_PlayerPos, m_PlayerRot, ai, EActorCamp.Player);
                m_Player.name += "(Player)";
                m_Player.IsPlayer = true;
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

                for (int i = 0; i < 10; i++)
                {
                    m_WndLoading.Percent = 61 + i;
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

        #endregion


        #region 复活

        /// <summary>死亡后重生，回到上次存档点</summary>
        IEnumerator RebirthPlayerItor()
        {
            yield return new WaitForSeconds(5);

            m_Player.Rebirth();
            yield return null;

            yield return BackToLastGodStatue();

            RecorverOtherActors();
            yield return null;

            //Timeline += 6;
        }

        Coroutine BackToLastGodStatue()
        {
            int sceneID = GameProgressManager.Instance.LastStayingSceneID;
            m_SceneInfo = GameApp.Entry.Config.SceneInfo.GetSceneInfoByID(sceneID);
            if (m_SceneInfo.m_GodStatuePoint == null || m_SceneInfo.m_GodStatuePoint.Length < 1)
            {
                return null;
            }

            int statueIndex = GameProgressManager.Instance.LastStayingStatueIndex;
            var targetStatueInfo = m_SceneInfo.m_GodStatuePoint[statueIndex];
            Quaternion tarStatueRot = Quaternion.Euler(0, targetStatueInfo.m_RotationY, 0);
            m_PlayerPos = targetStatueInfo.m_Position + tarStatueRot * new Vector3(0, 0.2f, 1.2f);
            m_PlayerRot = Quaternion.Euler(0, targetStatueInfo.m_RotationY + 180, 0);

            return GameApp.Entry.Unity.StartCoroutine(LoadItor(ELoadType.ToLastGodStatue));
        }

        /// <summary>其它角色复原</summary>
        void RecorverOtherActors()
        {
            if (!m_SceneInfo.m_ActiveOtherActors)
            {
                return;
            }

            foreach (var bornPoint in m_SceneInfo.m_OtherActorBornPoints)
            {
                m_DicOtherActors.TryGetValue(bornPoint, out var actor);
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

                    actor.transform.position = bornPoint.FixedBornPos;
                    actor.transform.rotation = bornPoint.BornRot;
                }
                else
                {
                    CreateActor(bornPoint);
                }
            }
        }

        #endregion


        public void Update(float deltaTime)
        {
            if (m_Player && m_Player.gameObject.activeSelf)
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
            GameApp.Entry.UI.CreateWnd<Wnd_Menu>(null, this);
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
            if (m_Player.CStateMachine.PlayAction_GoHome())
            {
                yield return new WaitForSeconds(3);
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
                GameApp.Entry.UI.CreateWnd<Wnd_DressUp>(content, this);
            }
        }

        void Wnd_Menu.IHandler.OnClickWait()
        {
            GameApp.Entry.UI.CreateWnd<Wnd_Wait>();
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


        #region WitchTime

        public void BeginSkillWitchTime()
        {
            if (m_CoroutineWitchTime != null)
            {
                m_CoroutineWitchTime.StopCoroutine();
                EndWitchTime(false);
            }

            m_CoroutineWitchTime = BeginSkillWitchTimeItor().StartCoroutine();
        }

        IEnumerator BeginSkillWitchTimeItor()
        {
            InWitchTime = true;

            Player.Invincible = true;
            Player.ToggleTrailShadowEffect(true);

            URPFeatureWitchTime.s_IsActive = true;

            yield return new WaitForSeconds(0.1f);

            GameApp.Entry.Game.Audio.Play2DSound("Sound/Skill/WitchTimeBegin");

            m_DicModifierWitchTimes.Clear();
            foreach (var pair in m_DicOtherActors)
            {
                if (pair.Value && !pair.Value.IsDead)
                {
                    m_DicModifierWitchTimes[pair.Value] = pair.Value.m_AnimSpeedExecutor.AddAnimSpeedModifier(0.1f);
                }
            }
        }

        public void BeginWitchTime()
        {
            if (m_CoroutineWitchTime != null)
            {
                m_CoroutineWitchTime.StopCoroutine();
                EndWitchTime(false);
            }

            m_CoroutineWitchTime = BeginWitchTimeItor().StartCoroutine();
        }

        IEnumerator BeginWitchTimeItor()
        {
            InWitchTime = true;

            Player.Invincible = true;
            Player.ToggleTrailShadowEffect(true);

            URPFeatureWitchTime.s_IsActive = true;

            yield return new WaitForSeconds(0.1f);

            GameApp.Entry.Game.Audio.Play2DSound("Sound/Skill/WitchTimeBegin");

            m_DicModifierWitchTimes.Clear();
            foreach (var pair in m_DicOtherActors)
            {
                if (pair.Value && !pair.Value.IsDead)
                {
                    m_DicModifierWitchTimes[pair.Value] = pair.Value.m_AnimSpeedExecutor.AddAnimSpeedModifier(0.2f);
                }
            }

            m_EffectWitchTimeBoom.Show(m_Player.transform.position + new Vector3(0, m_Player.CPhysic.CenterHeight));

            m_DicModifierWitchTimes[m_Player] = m_Player.m_AnimSpeedExecutor.AddAnimSpeedModifier(0.2f);

            yield return new WaitForSeconds(0.5f);

            foreach (var pair in m_DicModifierWitchTimes)
            {
                pair.Key.m_AnimSpeedExecutor.RemoveAnimSpeedModifier(pair.Value);
            }

            m_DicModifierWitchTimes.Clear();
            foreach (var pair in m_DicOtherActors)
            {
                if (pair.Value && !pair.Value.IsDead)
                {
                    m_DicModifierWitchTimes[pair.Value] = pair.Value.m_AnimSpeedExecutor.AddAnimSpeedModifier(0.1f);
                }
            }

            /*
            if (m_DicModifierWitchTimes.TryGetValue(m_Player, out var playerSlow))
            {
                m_Player.m_AnimSpeedExecutor.RemoveAnimSpeedModifier(playerSlow);
                m_DicModifierWitchTimes.Remove(m_Player);
            }*/

            yield return new WaitForSeconds(GameApp.Entry.Config.GameSetting.WitchTimeSeconds);
            EndWitchTime(true);
        }

        public void BeginWitchTimeFailed()
        {
            BeginWitchTimeFailedItor().StartCoroutine();
        }

        IEnumerator BeginWitchTimeFailedItor()
        {
            Player.Invincible = true;
            Player.ToggleTrailShadowEffect(true);

            //      URPFeatureWitchTime.s_IsActive = true;

            yield return new WaitForSeconds(0.1f);

            GameApp.Entry.Game.Audio.Play2DSound("Sound/Skill/WitchTimeFailed");

            m_DicModifierWitchTimes.Clear();
            foreach (var pair in m_DicOtherActors)
            {
                if (pair.Value && !pair.Value.IsDead)
                {
                    m_DicModifierWitchTimes[pair.Value] = pair.Value.m_AnimSpeedExecutor.AddAnimSpeedModifier(0.2f);
                }
            }

            // m_EffectWitchTimeBoom.Show(m_Player.transform.position + new Vector3(0, m_Player.CPhysic.CenterHeight));

            m_DicModifierWitchTimes[m_Player] = m_Player.m_AnimSpeedExecutor.AddAnimSpeedModifier(0.2f);

            yield return new WaitForSeconds(0.5f);

            Player.ToggleTrailShadowEffect(false);
            Player.Invincible = false;

            foreach (var pair in m_DicModifierWitchTimes)
            {
                pair.Key.m_AnimSpeedExecutor.RemoveAnimSpeedModifier(pair.Value);
            }

            m_DicModifierWitchTimes.Clear();
        }

        public void EndWitchTime(bool playSound)
        {
            if (!InWitchTime)
            {
                return;
            }

            InWitchTime = false;

            if (playSound)
            {
                GameApp.Entry.Game.Audio.Play2DSound("Sound/Skill/WitchTimeEnd");
            }

            foreach (var pair in m_DicModifierWitchTimes)
            {
                pair.Key.m_AnimSpeedExecutor.RemoveAnimSpeedModifier(pair.Value);
            }

            m_DicModifierWitchTimes.Clear();

            URPFeatureWitchTime.s_IsActive = false;
            Player.ToggleTrailShadowEffect(false);
            Player.Invincible = false;

            m_CoroutineWitchTime = null;
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
            portal.EnableGateCollider(false);
            GameApp.Entry.Game.PlayerCamera.LookAtTarget(portal.transform.rotation.eulerAngles.y + 150);
            GameApp.Entry.Game.PlayerAI.OnPlayerExitPortal(portal);

            m_Player.transform.position = portal.transform.position + portal.transform.forward * 0.8f;
            Vector3 dir = portal.transform.position - m_Player.transform.position;
            m_Player.transform.rotation = Quaternion.LookRotation(dir);

            GameApp.Entry.Game.PlayerAI.PlayActionMoveToTargetPos(portal.transform.position, m_Player.CPhysic.Radius,
                () =>
                {
                    m_CurrentUsingPortalInfo = portal.PortalInfo;
                    Load(ELoadType.ToNextSceneByPortal);
                });
        }

        #endregion


        #region GodStatue.IHandler

        void GodStatue.IHandler.OnPlayerEnter(GodStatue godStatue)
        {
            m_CurrentStayingGodStatue = godStatue;
            GameApp.Entry.Game.PlayerAI.OnPlayerEnterGodStatue(godStatue);
        }

        public void OnPlayerExit(GodStatue godStatue)
        {
            m_CurrentStayingGodStatue = null;
            GameApp.Entry.Game.PlayerAI.OnPlayerExitGodStatue(godStatue);
        }

        void GodStatue.IHandler.OnPlayerWorship(GodStatue godStatue)
        {
            //GameApp.Entry.Game.PlayerCamera.LookAtTarget(godStatue.transform.rotation.eulerAngles.y + 150);

            GameApp.Entry.Game.PlayerAI.WorshipGodStatue(godStatue, () =>
            {
                GameProgressManager.Instance.OnGodStatueFire(godStatue.SceneID, godStatue.StatueIndex);
                godStatue.RefreshFire();
            });
        }

        Coroutine GodStatue.IHandler.OnPlayerRest(GodStatue godStatue)
        {
            return OnPlayerResetItor(godStatue).StartCoroutine();
        }

        IEnumerator OnPlayerResetItor(GodStatue godStatue)
        {
            while (true)
            {
                Vector3 dirToStatue = godStatue.transform.position - m_Player.transform.position;
                if (m_Player.CPhysic.AlignForwardTo(dirToStatue, 720))
                    break;

                yield return null;
            }

            m_Player.CStateMachine.PlayAction_BranchRest();

            GameApp.Entry.Game.PlayerAI.Active = false;
            GameApp.Entry.Game.PlayerAI.OnPlayerExitGodStatue(godStatue);
            GameProgressManager.Instance.OnGodStatueRest(godStatue.SceneID, godStatue.StatueIndex);
            GameApp.Entry.UI.CreateWnd<Wnd_Rest>(null, this);

            yield return new WaitForSeconds(1);

            m_Player.OnGodStatueRest();

            yield return null;

            RecorverOtherActors();
        }

        #endregion


        #region Wnd_Rest.IHandler

        void Wnd_Rest.IHandler.OnClickQuit()
        {
            GameApp.Entry.Game.PlayerAI.Active = true;
            GameApp.Entry.Game.PlayerAI.OnPlayerEnterGodStatue(m_CurrentStayingGodStatue);

            m_Player.CStateMachine.PlayAction_BranchRestEnd();
        }

        void Wnd_Rest.IHandler.OnClickTransmit(int sceneID, int statueIndex)
        {
            TransmitItor(sceneID, statueIndex).StartCoroutine();
        }

        IEnumerator TransmitItor(int sceneID, int statueIndex)
        {
            GameApp.Entry.Game.PlayerAI.Active = true;

            if (m_CurrentStayingGodStatue != null &&
                sceneID == m_CurrentStayingGodStatue.SceneID &&
                statueIndex == m_CurrentStayingGodStatue.StatueIndex)
            {
                GameApp.Entry.Game.PlayerAI.OnPlayerEnterGodStatue(m_CurrentStayingGodStatue);
                yield break;
            }

            OnPlayerExit(m_CurrentStayingGodStatue);

            GameProgressManager.Instance.OnGodStatueRest(sceneID, statueIndex);

            m_Player.CStateMachine.PlayAction_BranchTeleport();

            yield return new WaitForSeconds(2);

            m_SceneInfo = GameApp.Entry.Config.SceneInfo.GetSceneInfoByID(sceneID);
            var targetStatueInfo = m_SceneInfo.m_GodStatuePoint[statueIndex];
            Quaternion tarStatueRot = Quaternion.Euler(0, targetStatueInfo.m_RotationY, 0);
            m_PlayerPos = targetStatueInfo.m_Position + tarStatueRot * new Vector3(0, 0.2f, 1.2f);
            m_PlayerRot = Quaternion.Euler(0, targetStatueInfo.m_RotationY + 180, 0);

            yield return GameApp.Entry.Unity.StartCoroutine(LoadItor(ELoadType.ToGodStatue));
        }

        public void OnSelectEnemy(int actorID)
        {
            DestroyOtherActors();
            var bornPoint = m_SceneInfo.m_OtherActorBornPoints[0];
            CreateActor(bornPoint, actorID);
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