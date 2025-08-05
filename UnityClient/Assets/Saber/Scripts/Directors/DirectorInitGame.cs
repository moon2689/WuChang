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
            GameApp.Entry.Config.LoatConfigGameSetting();
            yield return null;

            if (GameApp.Entry.Config.GameSetting.DebugFPS)
            {
                GameApp.Entry.Asset.LoadGameObject("Game/AdvancedFPSCounter");
                yield return null;
            }

            /*
            if (GameApp.Entry.Config.GameSetting.OpenIngameConsole)
            {
                GameApp.Entry.Asset.LoadGameObject("Game/IngameDebugConsole");
                yield return null;
            }
            */

            PlayerCamera.Create();
            yield return null;

            GameSetting.Init();

            RootUI.Create(() => { m_NextDir = new DirectorLogin(); });
        }
    }
}