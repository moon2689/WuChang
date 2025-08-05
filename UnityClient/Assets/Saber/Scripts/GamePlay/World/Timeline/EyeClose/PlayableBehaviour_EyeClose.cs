using UnityEngine;
using UnityEngine.Playables;
using Saber.CharacterController;

namespace Saber.Timeline
{
    public class PlayableBehaviour_EyeClose : PlayableBehaviour
    {
        public bool CloseEye { get; set; }

        /*
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            //base.ProcessFrame(playable, info, playerData);
            SCharacter character = playerData as SCharacter;
            if (character)
            {
                if (CloseEye)
                {
                    character.CExpression.EnableEyeBlink = false;
                    character.CExpression.CloseEye();
                }
                else
                {
                    character.CExpression.EnableEyeBlink = true;
                    character.CExpression.OpenEye();
                }
            }
        }
        */
    }
}