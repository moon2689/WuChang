using System.Collections;
using Saber.Frame;
using Saber.UI;
using UnityEngine;

namespace Saber.Director
{
    public class DirectorInitGame : DirectorBase
    {
        private DirectorBase m_NextDir;

        public override DirectorBase GetNextDirector()
        {
            return m_NextDir;
        }

        protected override IEnumerator EnterAsync()
        {
            // load yoo asset
            yield return YooAssetManager.Instance.Init();

            yield return RootUI.Create();

            GameApp.Entry.UI.ShowTips("欢迎来到游戏！", 0.5f);

            // 加载配置
            yield return GameApp.Entry.Config.LoadAsync().StartCoroutine();

            yield return PlayerCamera.Create();

            // fps
            if (GameApp.Entry.Config.TestGame.DebugFPS)
            {
                FPSCounter.Create();
                yield return null;
            }

            GameSetting.Init();
            yield return null;

            m_NextDir = new DirectorLogin();
        }
    }
}