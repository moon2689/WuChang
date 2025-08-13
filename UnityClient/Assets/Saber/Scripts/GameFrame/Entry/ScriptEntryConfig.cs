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
        public SkillCommonConfig SkillCommon { get; private set; }
        public MusicInfo MusicInfo { get; private set; }


        public IEnumerator LoadAsync()
        {
            Dictionary<string, ResourceRequest> dic = new();
            dic["GameSetting"] = Resources.LoadAsync<GameSettingInfo>("Config/GameSetting");
            dic["ActorInfo"] = Resources.LoadAsync<ActorInfo>("Config/ActorInfo");
            dic["SceneInfo"] = Resources.LoadAsync<SceneInfo>("Config/SceneInfo");
            dic["SkillInfo"] = Resources.LoadAsync<SkillInfo>("Config/SkillInfo");
            dic["ClothInfo"] = Resources.LoadAsync<ClothInfo>("Config/ClothInfo");
            dic["SkillCommon"] = Resources.LoadAsync<SkillCommonConfig>("Config/SkillCommon");
            dic["MusicInfo"] = Resources.LoadAsync<MusicInfo>("Config/MusicInfo");

            foreach (var pair in dic)
            {
                while (!pair.Value.isDone)
                {
                    yield return null;
                }
            }

            GameSetting = dic["GameSetting"].asset as GameSettingInfo;
            ActorInfo = dic["ActorInfo"].asset as ActorInfo;
            SceneInfo = dic["SceneInfo"].asset as SceneInfo;
            SkillInfo = dic["SkillInfo"].asset as SkillInfo;
            ClothInfo = dic["ClothInfo"].asset as ClothInfo;
            SkillCommon = dic["SkillCommon"].asset as SkillCommonConfig;
            MusicInfo = dic["MusicInfo"].asset as MusicInfo;
        }
    }
}