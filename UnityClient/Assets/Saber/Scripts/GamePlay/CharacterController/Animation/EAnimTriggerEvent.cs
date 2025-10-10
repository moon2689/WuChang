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
        Eated,
        AnimCanExit,
        LookArroundFirstFinished,
        FlyDash,
        PlaySound,
        RecoverHP,
        PlayExpression,
        DodgeToSprint,
        CanTanFanAgain,
        ShowWeapon,
        HideWeapon,
        WeaponFallToGround,
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