using System;
using System.Collections;
using Saber.Frame;
using Saber.UI;
using Saber.World;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Saber.Director
{
    public class DirectorLogin : DirectorBase, Wnd_Login.IHandler
    {
        public enum EStartGameType
        {
            NewGame,
            ContineGame,
        }

        private DirectorBase m_NextDirector;

        public override DirectorBase GetNextDirector()
        {
            return m_NextDirector;
        }

        protected override IEnumerator EnterAsync()
        {
            // 播放音乐
            AudioClip bgmStart = GameApp.Entry.Config.MusicInfo.m_LoginBGMStart;
            GameApp.Entry.Game.Audio.PlayBGM(bgmStart, 1, false, audioPlayer =>
            {
                AudioClip bgmLoop = GameApp.Entry.Config.MusicInfo.m_LoginBGMLoop;
                GameApp.Entry.Game.Audio.PlayBGM(bgmLoop, 1, true, null);
            });
            yield return null;

            // 场景
            yield return GameApp.Entry.Asset.LoadScene("Empty", null);

            // UI
            Wnd_Login wndLogin = null;
            yield return GameApp.Entry.UI.CreateWnd<Wnd_Login>(null, this, w => wndLogin = w);

            RootUI.Instance.HideBackground();

            // 进度
            GameApp.Entry.Game.ProgressMgr.Read();
            yield return null;

            wndLogin.EnableContinueGameButton = GameApp.Entry.Game.ProgressMgr.HasSavePointBefore;
        }

        void Wnd_Login.IHandler.NewGame()
        {
            GameApp.Entry.Game.ProgressMgr.Clear();
            m_NextDirector = new DirectorWorld(EStartGameType.NewGame);
        }

        void Wnd_Login.IHandler.ContinueGame()
        {
            m_NextDirector = new DirectorWorld(EStartGameType.ContineGame);
        }
    }
}