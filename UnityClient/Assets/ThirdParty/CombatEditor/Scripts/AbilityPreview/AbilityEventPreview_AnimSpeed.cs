using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CombatEditor
{
    public class AbilityEventPreview_AnimSpeed : AbilityEventPreview
    {
        public AbilityEventObj_AnimSpeed Obj => (AbilityEventObj_AnimSpeed)m_EventObj;
        public PreviewObject_AnimSpeed m_Speed;

        public AbilityEventPreview_AnimSpeed(AbilityEventObj Obj) : base(Obj)
        {
            m_EventObj = Obj;
        }

#if UNITY_EDITOR
        public override void InitPreview()
        {
            base.InitPreview();

            var SpeedObj = new GameObject("Preview_AnimSpeed");
            SpeedObj.transform.SetParent(previewGroup.transform);
            m_Speed = SpeedObj.AddComponent<PreviewObject_AnimSpeed>();
            m_Speed._preview = this;
            m_Speed.CurrentAnimSpeedModifier = Obj.Speed;
            m_Speed.transform.SetParent(previewGroup.transform);
        }

        public override void PreviewRunning(float CurrentTimePercentage)
        {
            base.PreviewRunning(CurrentTimePercentage);

            if (PreviewInRange(CurrentTimePercentage))
            {
                m_Speed.CurrentAnimSpeedModifier = Obj.Speed;
            }
            else
            {
                m_Speed.CurrentAnimSpeedModifier = 1;
            }
        }
#endif
    }
}