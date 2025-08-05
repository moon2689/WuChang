using System;
using Saber.Frame;
using Saber.CharacterController;
using UnityEngine;

//Replace the "Magic" with the event you want to create
//If you want to create a object with handle in preview, please inherit the "AbilityEventObj" with "AbilityEventObj_CreateObjWithHandle"

namespace CombatEditor
{
    [AbilityEvent]
    [CreateAssetMenu(menuName = "AbilityEvents / Magic")]
    public class AbilityEventObj_Magic : AbilityEventObj
    {
        public EMagicType m_MagicType;

        //Write the data you need here.
        public override EventTimeType GetEventTimeType()
        {
            return EventTimeType.EventTime;
        }

        public override AbilityEventEffect Initialize()
        {
            return new AbilityEventEffect_Magic(this);
        }
    }

//Write you logic here
    public partial class AbilityEventEffect_Magic : AbilityEventEffect
    {
        public override void StartEffect()
        {
            base.StartEffect();
            PlayMagic(EventObj.m_MagicType);
        }

        /*
        public override void EffectRunning(float currentTimePercentage)
        {
            base.EffectRunning(currentTimePercentage);
        }

        protected override void EndEffect()
        {
            base.EndEffect();
        }
        */

        void PlayMagic(EMagicType magicType)
        {
            Debug.Log("Play magic:" + magicType);
            if (magicType == EMagicType.ClearDay)
            {
               // GameApp.Entry.Game.World?.SetWeather_ClearDay();
            }
            else if (magicType == EMagicType.RecoverHP)
            {
                Actor.CStats.PlayHealingEffect(60);
            }
            else
            {
                throw new InvalidOperationException("Unknown magic:" + magicType);
            }
        }
    }

    public partial class AbilityEventEffect_Magic : AbilityEventEffect
    {
        private AbilityEventObj_Magic EventObj { get; set; }

        public AbilityEventEffect_Magic(AbilityEventObj initObj) : base(initObj)
        {
            m_EventObj = initObj;
            EventObj = (AbilityEventObj_Magic)initObj;
        }
    }
}