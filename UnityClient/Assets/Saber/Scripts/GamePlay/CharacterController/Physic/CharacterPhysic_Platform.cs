using System;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public partial class CharacterPhysic
    {
        private Transform m_Platform;
        private Vector3 m_LastPlatformPos;
        private Quaternion m_LastPlatformRot;

        public bool EnablePlatformMovement { get; set; } = true;

        public void SetPlatform(Transform newPlatform)
        {
            if (m_Platform != newPlatform)
            {
                //Debug.Log($"SetPlatform: {newPlatform}", newPlatform);
                //GroundRootPosition = true;

                m_Platform = newPlatform;

                /*
                bool isTransparentFXLayer = m_Platform != null && ((1 << m_Platform.gameObject.layer) & EStaticLayers.BodyPart.GetLayerMask()) != 0;
                if (isTransparentFXLayer)
                {
                    m_CapsuleCollider.excludeLayers = 1 << EStaticLayers.Animal.GetLayer();
                }
                else
                {
                    m_CapsuleCollider.excludeLayers = 1 << EStaticLayers.Animal.GetLayer() | 1 << EStaticLayers.BodyPart.GetLayer();
                }
                */

                if (m_Platform != null)
                {
                    //Debug.Log($"#1 isTransparentFXLayer:{isTransparentFXLayer}, platform:{newPlatform.name}", newPlatform);
                    /*
                    var NewGroundChanger = newPlatform.GetComponent<GroundSpeedChanger>();

                    if (NewGroundChanger)
                    {
                        GroundRootPosition = false; //Important! Calculate RootMotion instead of adding it
                        GroundChanger?.OnExit?.React(this); //set to the ground changer that this has enter 
                        GroundChanger = NewGroundChanger;
                        GroundChanger.OnEnter?.React(this); //set to the ground changer that this has enter 
                    }
                    else
                    {
                        GroundChanger?.OnExit?.React(this); //set to the ground changer that this has enter 
                        GroundChanger = null;
                    }
                    */

                    m_LastPlatformPos = m_Platform.position;
                    m_LastPlatformRot = m_Platform.rotation;
                }
                else
                {
                    /*
                    GroundChanger?.OnExit?.React(this); //set to the ground changer that this has enter 
                    GroundChanger = null;
                    */

                    MainPivotSlope = 0;
                    ResetSlopeValues();
                }

                /*
                InGroundChanger = GroundChanger != null;
                foreach (var s in states)
                    s.OnPlataformChanged(platform);
                */
            }
        }

        void PlatformMovement()
        {
            if (m_Platform == null)
                return;
            if (m_Platform.gameObject.isStatic)
                return; //means it cannot move

            //Set it Directly to the Transform.. Additive Position can be reset any time.
            var deltaPlatformPos = m_Platform.position - m_LastPlatformPos;
            Actor.transform.position += deltaPlatformPos;

            Quaternion inverseRot = Quaternion.Inverse(m_LastPlatformRot);
            Quaternion deltaRot = inverseRot * m_Platform.rotation;

            if (deltaRot != Quaternion.identity) // no rotation founded.. Skip the code below
            {
                var pos = Actor.transform.DeltaPositionFromRotate(m_Platform.position, deltaRot);
                Actor.transform.position +=
                    pos; //Set it Directly to the Transform.. Additive Position can be reset any time..
            }

            //AdditiveRotation *= Delta;
            Actor.transform.rotation *=
                deltaRot; //Set it Directly to the Transform.. Additive Position can be reset any time..

            m_LastPlatformPos = m_Platform.position;
            m_LastPlatformRot = m_Platform.rotation;
        }
    }
}