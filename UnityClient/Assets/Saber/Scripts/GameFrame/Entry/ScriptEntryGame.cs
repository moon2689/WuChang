using System.Collections;
using Saber.CharacterController;
using Saber.Config;
using Saber.UI;
using Saber.World;
using UnityEngine;
using Saber.AI;

namespace Saber.Frame
{
    public class ScriptEntryGame
    {
        private static GameProgressManager s_InstanceGameProgressManager;


        // public Root3D Root3D => Root3D.Instance;
        public PlayerCamera PlayerCamera => PlayerCamera.Instance;

        public RootUI RootUI => RootUI.Instance;

        public PlayerPhoneInput PlayerAI => PlayerPhoneInput.Instance;

        public AudioManager Audio => AudioManager.GetInstance();

        public EffectPool Effect => EffectPool.GetInstance();

        public BigWorld World { get; set; }
        public SActor Player => World?.Player;
        public GameProgressManager ProgressMgr => s_InstanceGameProgressManager ??= new GameProgressManager();


        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}