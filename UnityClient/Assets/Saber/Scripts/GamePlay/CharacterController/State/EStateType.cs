using System;
using System.Collections.Generic;

namespace Saber.CharacterController
{
    // 修改此枚举得相应修改 CharacterStateHelper.StateSwitchTable
    public enum EStateType
    {
        None = -1,
        Idle, //空闲
        Move, //移动
        Skill, //技能
        Dodge, //闪避
        GetHit, //受击
        Defense, //格挡
        Die, //死亡
        Fall, //下落
    }

    public enum EStateSwitchType
    {
        CannotSwitch = 0, //不可转换
        CanSwitchAnyTime = 1, //可随时转换
        WaitStateCanExit = 2, //等待状态可退出时即可转换
        DodgeToSprint = 3,
        CanTriggerSkill = 4, //检查是否可以释放技能
    }

    public static class StateHelper
    {
        // 判断状态之间是否可转换的表
        public static readonly int[,] StateSwitchTable =
        {
            // to:
            //Id Mo Sk Do GH De Di Fa    // from:
            { 1, 1, 1, 1, 1, 1, 1, 1, }, // Idle
            { 1, 1, 4, 1, 1, 1, 1, 1, }, // Move
            { 0, 2, 1, 2, 1, 2, 1, 2, }, // Skill
            { 0, 3, 4, 2, 1, 2, 1, 2, }, // Dodge
            { 0, 0, 0, 2, 1, 0, 0, 0, }, // GetHit
            { 0, 2, 4, 2, 1, 1, 1, 2, }, // Defense
            { 0, 0, 0, 0, 0, 0, 0, 0, }, // Die
            { 0, 0, 1, 0, 0, 0, 1, 0, }, // Fall
        };

        public static EStateSwitchType CanSwitchTo(EStateType from, EStateType to)
        {
            return (EStateSwitchType)StateSwitchTable[(int)from, (int)to];
        }
    }

    public interface ISkillCanTrigger
    {
        bool CanTriggerSkill(SkillItem skill);
        
    }
}