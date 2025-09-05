using System;
using Saber.Frame;
using System.Collections;
using Saber.Config;
using Saber.World;
using UnityEngine;

namespace Saber.Director
{
    public class DirectorWorld : DirectorBase
    {
        private BigWorld.ELoadType m_LoadType;
        private Coroutine m_CoroutinePlayBGM;

        public DirectorWorld(DirectorLogin.EStartGameType startGameType)
        {
            m_LoadType = startGameType switch
            {
                DirectorLogin.EStartGameType.NewGame => BigWorld.ELoadType.NewGame,
                DirectorLogin.EStartGameType.ContineGame => BigWorld.ELoadType.ToLastIdol,
                _ => throw new InvalidOperationException($"Unknown startGameType:{startGameType}"),
            };
            GameApp.Entry.Game.World = new();
        }

        public override DirectorBase GetNextDirector()
        {
            return null;
        }

        public override void Enter()
        {
            base.Enter();
            GameApp.Entry.Game.World.Load(m_LoadType, StartBGM);
            GameApp.Entry.Game.World.OnSetLockingEnemyEvent = OnSetLockingEnemy;
        }

        private void OnSetLockingEnemy()
        {
            var lockEnemy = GameApp.Entry.Game.PlayerAI.LockingEnemy;
            if (lockEnemy != null && lockEnemy.BaseInfo.m_ActorType == EActorType.Boss)
            {
                AudioClip clip = GameApp.Entry.Config.MusicInfo.m_BattleCommon;
                GameApp.Entry.Game.Audio.PlayBGM(clip, 0.5f, true, null);
            }
            else
            {
                GameApp.Entry.Game.Audio.StopBGM();
            }
        }

        void StartBGM()
        {
            if (m_CoroutinePlayBGM != null)
            {
                m_CoroutinePlayBGM.StopCoroutine();
                m_CoroutinePlayBGM = null;
            }

            m_CoroutinePlayBGM = PlayBGMLoop().StartCoroutine();
        }

        public override void Update()
        {
            base.Update();
            GameApp.Entry.Game.World.Update(Time.deltaTime);
        }

        public override void Exit()
        {
            base.Exit();
            if (m_CoroutinePlayBGM != null)
            {
                m_CoroutinePlayBGM.StopCoroutine();
                m_CoroutinePlayBGM = null;
            }

            GameApp.Entry.Unity.StopAllCoroutines();
            if (GameApp.Entry.Game.World != null)
            {
                GameApp.Entry.Game.World.Release();
                GameApp.Entry.Game.World = null;
            }
        }

        IEnumerator PlayBGMLoop()
        {
            PlayRandomBGM();
            while (true)
            {
                yield return new WaitForSeconds(60);
                if (!GameApp.Entry.Game.Audio.IsBGMPlaying && (
                        GameApp.Entry.Game.PlayerAI.LockingEnemy == null ||
                        GameApp.Entry.Game.PlayerAI.LockingEnemy.BaseInfo.m_ActorType != EActorType.Boss))
                {
                    PlayRandomBGM();
                }
            }
        }

        void PlayRandomBGM()
        {
            AudioClip clip = GameApp.Entry.Config.MusicInfo.GetRandomExploreMusic();
            GameApp.Entry.Game.Audio.PlayBGM(clip, 0.5f, false, null);
        }
    }
}