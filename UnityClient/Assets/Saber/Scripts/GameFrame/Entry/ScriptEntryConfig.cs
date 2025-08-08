using System.Collections;
using System.Collections.Generic;
using Saber.CharacterController;
using Saber.Config;
using UnityEngine;

namespace Saber.Frame
{
    public class ScriptEntryConfig
    {
        public GameSettingInfo GameSetting { get; private set; }
        public ActorInfo ActorInfo { get; private set; }
        public SceneInfo SceneInfo { get; private set; }
        public SkillInfo SkillInfo { get; private set; }
        public ClothInfo ClothInfo { get; private set; }
        public SkillDecapitateConfig SkillDecapitateConfig { get; private set; }
        public MusicInfo MusicInfo { get; private set; }


        public void LoatConfigGameSetting()
        {
            GameSetting = Resources.Load<GameSettingInfo>("Config/GameSetting");
        }

        public IEnumerator LoadAsync()
        {
            if (GameSetting == null)
            {
                GameSetting = Resources.Load<GameSettingInfo>("Config/GameSetting");
                GameSetting.PreloadEffects();
                yield return null;
            }

            ActorInfo = Resources.Load<ActorInfo>("Config/ActorInfo");
            yield return null;
            SceneInfo = Resources.Load<SceneInfo>("Config/SceneInfo");
            yield return null;
            SkillInfo = Resources.Load<SkillInfo>("Config/SkillInfo");
            yield return null;
            ClothInfo = Resources.Load<ClothInfo>("Config/ClothInfo");
            yield return null;
            SkillDecapitateConfig = Resources.Load<SkillDecapitateConfig>("Config/SkillDecapitate");
            yield return null;
            MusicInfo = Resources.Load<MusicInfo>("Config/MusicInfo");
            yield return null;
        }
    }
}