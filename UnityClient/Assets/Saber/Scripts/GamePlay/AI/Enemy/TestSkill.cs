using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Saber.CharacterController;
using Saber.Frame;
using UnityEngine.Rendering.UI;

namespace Saber.AI
{
    public class TestSkill : EnemyAIBase
    {
        protected override void OnStart()
        {
            base.OnStart();
            SwitchCoroutine(SearchEnemy());
        }

        protected override void OnFoundEnemy(EFoundEnemyType foundType)
        {
            SwitchCoroutine(TestSkillItor());
        }

        // 对峙
        IEnumerator TestSkillItor()
        {
            while (true)
            {
                int skillID = GameApp.Entry.Config.TestGame.TestingSkillID;
                SkillItem tarSkill = Actor.CMelee.SkillConfig.GetSkillItemByID(skillID);
                Actor.TryTriggerSkill(tarSkill);
                yield return new WaitForSeconds(GameApp.Entry.Config.TestGame.TriggerSkillInterval);
            }
        }
    }
}