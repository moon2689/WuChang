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

            ResourceRequest rr = Resources.LoadAsync<ActorInfo>("Config/ActorInfo");
            yield return rr;
            ActorInfo = rr.asset as ActorInfo;

            rr = Resources.LoadAsync<SceneInfo>("Config/SceneInfo");
            yield return rr;
            SceneInfo = rr.asset as SceneInfo;

            rr = Resources.LoadAsync<SkillInfo>("Config/SkillInfo");
            yield return rr;
            SkillInfo = rr.asset as SkillInfo;

            rr = Resources.LoadAsync<ClothInfo>("Config/ClothInfo");
            yield return rr;
            ClothInfo = rr.asset as ClothInfo;

            rr = Resources.LoadAsync<SkillDecapitateConfig>("Config/SkillDecapitate");
            yield return rr;
            SkillDecapitateConfig = rr.asset as SkillDecapitateConfig;

            rr = Resources.LoadAsync<MusicInfo>("Config/MusicInfo");
            yield return rr;
            MusicInfo = rr.asset as MusicInfo;
        }
    }
}