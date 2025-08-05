using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Saber.CharacterController
{
    [Serializable]
    public enum EAnimTriggerEvent
    {
        None,
        DrawOrStoreWeapon,
        ArmWeapon,
        AnimCanExit,
        LookArroundFirstFinished,
        FlyDash,
        PlaySound,
        RecoverHP,
        PlayExpression,
        DodgeToSprint,
    }

    [Serializable]
    public enum EAnimRangeEvent
    {
        CanTriggerSkill,
        HeavyAttackSlowAnim,
        CanBeDecapitate, //可被处决
        AlignDirection,
        CancelGravity,
        
        Invincible,
        Kinematic,
        
        CreateObject,
    }
    
    // 绑定在动画clip上的事件
    public enum EAnimClipEventType
    {
        FootL,
        FootR,
        Foot1,
        Foot2,
        Foot3,
        Foot4,
        ActionFootL,
        ActionFootR,
        RushStopLeft,
        RollEvent,
        PlayAudio,
    }
}