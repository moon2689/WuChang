using System;
using Saber.Frame;

using Saber.World;
using UnityEngine;

namespace Saber.Director
{
    public class DirectorWorld : DirectorBase
    {
        private BigWorld.ELoadType m_LoadType;

        public DirectorWorld(DirectorLogin.EStartGameType startGameType)
        {
            m_LoadType = startGameType switch
            {
                DirectorLogin.EStartGameType.NewGame => BigWorld.ELoadType.NewGame,
                DirectorLogin.EStartGameType.ContineGame => BigWorld.ELoadType.ToLastGodStatue,
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
            GameApp.Entry.Game.World.Load(m_LoadType);
        }

        public override void Update()
        {
            base.Update();
            GameApp.Entry.Game.World.Update(Time.deltaTime);
        }

        public override void Exit()
        {
            base.Exit();
            GameApp.Entry.Unity.StopAllCoroutines();
            if (GameApp.Entry.Game.World != null)
            {
                GameApp.Entry.Game.World.Release();
                GameApp.Entry.Game.World = null;
            }
        }
    }
}