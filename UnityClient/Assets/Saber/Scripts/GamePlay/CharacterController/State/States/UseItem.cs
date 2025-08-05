using System;
using System.Collections;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class UseItem : ActorStateBase
    {
        public enum EItemType
        {
            HpMedicine,
        }

        public EItemType ItemType { get; set; }

        public UseItem() : base(EStateType.UseItem)
        {
        }

        public override void Enter()
        {
            base.Enter();
            Actor.CAnim.Play("UseItemPinch", onFinished: Exit);
        }

        public override void OnTriggerAnimEvent(AnimPointTimeEvent eventObj)
        {
            base.OnTriggerAnimEvent(eventObj);
            if (eventObj.EventType == EAnimTriggerEvent.RecoverHP && !this.Actor.CStats.IsHPFull)
            {
                float hpValue = Actor.CStats.MaxHp;
                Actor.CStats.PlayHealingEffect(hpValue);
                --Actor.CStats.HPPotionCount;
            }
        }
    }
}