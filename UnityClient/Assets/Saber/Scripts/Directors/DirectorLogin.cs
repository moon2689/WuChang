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
            AsyncOperation h = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Single);
            yield return h;
            
            var wndLogin = GameApp.Entry.UI.CreateWnd<Wnd_Login>(null, this);
            yield return null;
            
            yield return GameApp.Entry.Unity.StartCoroutine(GameApp.Entry.Config.LoadAsync());

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