using System.Numerics;
using Saber.CharacterController;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace CombatEditor
{
    [AbilityEvent]
    [CreateAssetMenu(menuName = "AbilityEvents / AlignToEnemy")]
    public class AbilityEventObj_AlignToEnemy : AbilityEventObj
    {
        public float m_RotateSpeed = 360;
        //public float m_AngleOffset;

        public override EventTimeType GetEventTimeType()
        {
            return EventTimeType.EventRange;
        }

        public override AbilityEventEffect Initialize()
        {
            return new AbilityEventEffect_AlignToEnemy(this);
        }
    }

    public partial class AbilityEventEffect_AlignToEnemy : AbilityEventEffect
    {
        /*
        private HurtBox m_AlighToEnemyHurtBox;

        public override void StartEffect()
        {
            base.StartEffect();
            m_AlighToEnemyHurtBox = null;
            m_AlighToEnemyHurtBox = GetAlighToEnemyHurtBox();
        }
    

        private HurtBox GetAlighToEnemyHurtBox()
        {
            if (Actor.AI.LockingEnemy != null && Actor.AI.LockingEnemy.HurtBoxes.Length > 1)
            {
                float tarDistance = float.MaxValue;
                HurtBox tarHurtBox = null;
                Vector3 ownerCenter = Actor.transform.position + Vector3.up * Actor.CPhysic.CenterHeight;
                foreach (HurtBox hurtBox in Actor.AI.LockingEnemy.HurtBoxes)
                {
                    Vector3 dir = hurtBox.CenterPos - ownerCenter;
                    float distance = dir.magnitude;
                    if (distance < tarDistance)
                    {
                        tarDistance = distance;
                        tarHurtBox = hurtBox;
                    }
                }

                return tarHurtBox;
            }
            else
            {
                return null;
            }
        }
        */

        public override void EffectRunning(float currentTimePercentage)
        {
            base.EffectRunning(currentTimePercentage);

            /*
            Vector3 dir = Actor.DesiredLookDir;
            if (m_AlighToEnemyHurtBox)
            {
                Vector3 ownerCenter = Actor.transform.position + Vector3.up * Actor.CPhysic.CenterHeight;
                dir = m_AlighToEnemyHurtBox.CenterPos - ownerCenter;
                dir.y = 0;
            }
            else
            {
                dir = Actor.DesiredLookDir;
            }
            */

            /*
            if (EventObj.m_AngleOffset != 0)
            {
                dir = Quaternion.Euler(0, EventObj.m_AngleOffset, 0) * dir;
            }
            */

            if (Actor.AI.LockingEnemy != null)
            {
                Actor.CPhysic.AlignForwardTo(Actor.DesiredLookDir, EventObj.m_RotateSpeed * Actor.PhysicInfo.TurnRotSpeedRate);
            }
        }
    }

    public partial class AbilityEventEffect_AlignToEnemy : AbilityEventEffect
    {
        private AbilityEventObj_AlignToEnemy EventObj { get; set; }

        public AbilityEventEffect_AlignToEnemy(AbilityEventObj InitObj) : base(InitObj)
        {
            m_EventObj = InitObj;
            EventObj = (AbilityEventObj_AlignToEnemy)m_EventObj;
        }
    }
}