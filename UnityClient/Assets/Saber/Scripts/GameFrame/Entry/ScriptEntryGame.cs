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
        // public Root3D Root3D => Root3D.Instance;
        public PlayerCamera PlayerCamera => PlayerCamera.Instance;

        public RootUI RootUI => RootUI.Instance;

        public PlayerInput PlayerAI
        {
            get
            {
                var gameSetting = GameApp.Entry.Config.GameSetting;
                if (gameSetting.PlayerInputType == GameSettingInfo.EPlayerInputType.PC)
                {
                    return PlayerPCInput.Instance;
                }
                else if (gameSetting.PlayerInputType == GameSettingInfo.EPlayerInputType.Phone)
                {
                    return PlayerPhoneInput.Instance;
                }

                return null;
            }
        }

        public AudioManager Audio => AudioManager.GetInstance();

        public EffectPool Effect => EffectPool.GetInstance();

        public BigWorld World { get; set; }
        public SActor Player => World?.Player;


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