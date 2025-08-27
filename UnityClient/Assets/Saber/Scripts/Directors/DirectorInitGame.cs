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
            RootUI.Create();
            yield return null;
            
            // 加载配置
            yield return GameApp.Entry.Config.LoadAsync().StartCoroutine();

            PlayerCamera.Create();
            yield return null;

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