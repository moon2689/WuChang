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
        ExecuteDamage,
        ShowWeapon,
        HideWeapon,
    }

    [Serializable]
    public enum EAnimRangeEvent
    {
        CanTriggerSkill,
        InChargeTime,
        CanBeExecute, //可被处决
        AlignDirection,
        CancelGravity,
        
        Invincible,
        Kinematic,
        
        CreateObject,
    }
}