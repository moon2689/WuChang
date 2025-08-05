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
        Jump, //跳跃
        Skill, //技能
        Dodge, //闪避
        GetHit, //受击
        Defense, //格挡
        Die, //死亡
        Fall, //下落
        Weak, //虚弱
        UseItem, //使用物品
        Swim, //游泳
        Fly, //飞行
        Slide,
        Glide,
        Climb,
    }

    public enum EStateSwitchType
    {
        CannotSwitch = 0, //不可转换
        CanSwitchAnyTime = 1, //可随时转换
        WaitStateCanExit = 2, //等待状态可退出时即可转换
        SkillCanBreak = 3, //技能可被打断，如前遥
        CanTriggerSkill = 4, //检查是否可以释放技能
    }

    public static class StateHelper
    {
        // 判断状态之间是否可转换的表
        public static readonly int[,] StateSwitchTable =
        {
            // to:
            //Id Mo Ju Sk Do GH De Di Fa We UI Sw Fl Sl Gl Cl    // from:
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, }, // Idle
            { 1, 1, 1, 4, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, }, // Move
            { 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 2, 1, 0, 0, 1, 1, }, // Jump
            { 0, 2, 2, 1, 1, 1, 2, 1, 2, 1, 2, 2, 0, 0, 0, 0, }, // Skill
            { 0, 2, 0, 4, 2, 1, 2, 1, 2, 1, 2, 2, 0, 0, 0, 0, }, // Dodge
            { 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, }, // GetHit
            { 0, 0, 0, 0, 0, 1, 1, 1, 2, 1, 0, 0, 0, 0, 0, 0, }, // Defense
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, // Die
            { 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 1, 1, }, // Fall
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, }, // Weak
            { 0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 0, 1, 0, 0, 0, 0, }, // UseItem
            { 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 1, }, // Swim
            { 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 1, 0, 0, 0, 1, }, // Fly
            { 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 2, 0, 1, }, // Slide
            { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 1, 1, 1, }, // Glide
            { 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, }, // Climb
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