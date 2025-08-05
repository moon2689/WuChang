using System.Collections;
using System.Collections.Generic;
using Saber;
using UnityEngine;

//If you need to create object with handle, you can just inherit the AbilityEventPreview_CreateObjWithHandle
#if UNITY_EDITOR

namespace CombatEditor
{
    public partial class AbilityEventPreview_ShakeCamera : AbilityEventPreview
    {
        private PlayerCameraShake m_PlayerCameraShake;

        //Init Preview, for e.g, you can create your preview object here.
        public override void InitPreview()
        {
            base.InitPreview();
            m_PlayerCameraShake = GameObject.FindObjectOfType<PlayerCameraShake>();
        }

        public override void PassStartFrame()
        {
            base.PassStartFrame();
            if (CombatGlobalEditorValue.IsPlaying || CombatGlobalEditorValue.IsLooping)
            {
                m_PlayerCameraShake.ActivateCameraShake(EventObj.m_Duration, EventObj.m_Amount, EventObj.m_Speed);
            }
        }

        //Update Preview when drag the timeline or timeline is playing.
        public override void PreviewRunning(float CurrentTimePercentage)
        {
            base.PreviewRunning(CurrentTimePercentage);
        }

        //Update Preview when drag the timeline or timeline is playing, but only called once per frame.
        public override void PreviewUpdateFrame(float CurrentTimePercentage)
        {
            base.PreviewUpdateFrame(CurrentTimePercentage);
        }

        //Update the preview, but sometimes the preview is effected by scale time event.
        //For example, the particle's preview is effected by time scale event. So the Scaledpercentage * AnimClip.length is real time.
        public override void PreviewRunningInScale(float ScaledPercentage)
        {
            base.PreviewRunningInScale(ScaledPercentage);
        }

        //Sometimes the preview need the value from startFrame. For e.g, a particle that dont follow the movement of node, should know the start position
        //Set this func to true if requires the data.
        public override bool NeedStartFrameValue()
        {
            return base.NeedStartFrameValue();
        }

        // Execute before previewRunning. When in this function, the character data is previewing at startframe.
        public override void GetStartFrameDataBeforePreview()
        {
            base.GetStartFrameDataBeforePreview();
        }

        //Destroy the preview. You need to clear your preview object here.
        public override void DestroyPreview()
        {
            base.DestroyPreview();
        }

        //Preview Go back to start frame. For example , the dynamic trail of a weapon should clear when you restart the preview.
        public override void BackToStart()
        {
            base.BackToStart();
        }
    }

    public partial class AbilityEventPreview_ShakeCamera : AbilityEventPreview
    {
        public AbilityEventObj_ShakeCamera EventObj => (AbilityEventObj_ShakeCamera)m_EventObj;

        public AbilityEventPreview_ShakeCamera(AbilityEventObj Obj) : base(Obj)
        {
            m_EventObj = Obj;
        }
    }
}
#endif