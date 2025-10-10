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
        public TestGameInfo TestGame { get; private set; }
        public ActorInfo ActorInfo { get; private set; }
        public SceneInfo SceneInfo { get; private set; }
        public ClothInfo ClothInfo { get; private set; }
        public SkillCommonConfig SkillCommon { get; private set; }
        public MusicInfo MusicInfo { get; private set; }
        public PropInfo PropInfo { get; set; }


        public IEnumerator LoadAsync()
        {
            yield return GameApp.Entry.Asset.LoadAsset<GameSettingInfo>("Config/GameSetting", s => GameSetting = s);
            yield return GameApp.Entry.Asset.LoadAsset<TestGameInfo>("Config/TestGame", s => TestGame = s);
            yield return GameApp.Entry.Asset.LoadAsset<ActorInfo>("Config/ActorInfo", s => ActorInfo = s);
            yield return GameApp.Entry.Asset.LoadAsset<SceneInfo>("Config/SceneInfo", s => SceneInfo = s);
            yield return GameApp.Entry.Asset.LoadAsset<ClothInfo>("Config/ClothInfo", s => ClothInfo = s);
            yield return GameApp.Entry.Asset.LoadAsset<SkillCommonConfig>("Config/SkillCommon", s => SkillCommon = s);
            yield return GameApp.Entry.Asset.LoadAsset<MusicInfo>("Config/MusicInfo", s => MusicInfo = s);
            yield return GameApp.Entry.Asset.LoadAsset<PropInfo>("Config/PropInfo", s => PropInfo = s);

            yield return null;
            ClothInfo.Init();
        }
    }
}