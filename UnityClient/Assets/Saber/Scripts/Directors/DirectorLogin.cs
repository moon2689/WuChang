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
            // 场景
            AsyncOperation h = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Single);
            yield return h;

            // UI
            var wndLogin = GameApp.Entry.UI.CreateWnd<Wnd_Login>(null, this);
            yield return null;

            // 加载配置
            yield return GameApp.Entry.Config.LoadAsync().StartCoroutine();

            // 播放音乐
            AudioClip bgmStart = GameApp.Entry.Config.MusicInfo.m_LoginBGMStart;
            GameApp.Entry.Game.Audio.PlayBGM(bgmStart, 1, false, audioPlayer =>
            {
                AudioClip bgmLoop = GameApp.Entry.Config.MusicInfo.m_LoginBGMLoop;
                GameApp.Entry.Game.Audio.PlayBGM(bgmLoop, 1, true, null);
            });

            // 进度
            GameProgressManager.Instance.Read();
            yield return null;

            wndLogin.EnableContinueGameButton = GameProgressManager.Instance.HasSavePointBefore;
        }

        void Wnd_Login.IHandler.NewGame()
        {
            GameProgressManager.Instance.Clear();
            m_NextDirector = new DirectorWorld(EStartGameType.NewGame);
        }

        void Wnd_Login.IHandler.ContinueGame()
        {
            m_NextDirector = new DirectorWorld(EStartGameType.ContineGame);
        }
    }
}