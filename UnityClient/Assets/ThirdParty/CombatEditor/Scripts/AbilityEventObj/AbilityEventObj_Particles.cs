using Saber.Frame;
using Saber.CharacterController;
using UnityEngine;
using UnityEngine.Serialization;

namespace CombatEditor
{
    [System.Serializable]
    public class InsedObject
    {
        public GameObject TargetObj;
        public PreviewTransformHandle.ControlTypeEnum controlType;
        public Vector3 Offset;
        public Quaternion Rot;
        public ENodeType TargetNode;
        public bool FollowNode = true;
        public bool RotateByNode;

        public GameObject CreateObject(SActor controller)
        {
            GameObject go = null;
            if (TargetObj != null)
            {
                //go = Object.Instantiate(TargetObj);
                go = GameApp.Entry.Game.Effect.CreateEffect(TargetObj);

                NodeFollower follower = go.GetComponent<NodeFollower>();
                if (!follower)
                    follower = go.AddComponent<NodeFollower>();
                follower.Init(
                    controller.GetNodeTransform(TargetNode),
                    Offset,
                    Rot,
                    FollowNode,
                    RotateByNode,
                    controller
                );
            }

            return go;
        }

        public void PreloadObjects()
        {
            GameApp.Entry.Game.Effect.PreloadEffect(TargetObj);
        }
    }

    [AbilityEvent]
    [CreateAssetMenu(menuName = "AbilityEvents / Particles")]
    public class AbilityEventObj_Particles : AbilityEventObj_CreateObjWithHandle
    {
        //public InsedObject ParticleData = new InsedObject();
        public EventTimeType TimeType = EventTimeType.EventTime;
        public float m_HoldTime = 1f;
        public bool OnlyTriggerWhenPowerEnough;

        public override EventTimeType GetEventTimeType()
        {
            return TimeType;
        }

        public override AbilityEventEffect Initialize()
        {
            ObjData.PreloadObjects();
            return new AbilityEventEffect_Particles(this);
        }

# if UNITY_EDITOR
        public override AbilityEventPreview InitializePreview()
        {
            return new AbilityEventPreview_Particles(this);
        }
#endif
    }


    public partial class AbilityEventEffect_Particles : AbilityEventEffect
    {
        GameObject m_InsedParticle;

        protected override void EndEffect()
        {
            base.EndEffect();
            if (EventObj.GetEventTimeType() == AbilityEventObj.EventTimeType.EventRange)
            {
                DestroyObj();
            }
        }

        void DestroyObj()
        {
            if (m_InsedParticle)
            {
                // Debug.Log("destroy "+m_InsedParticle.name);
                //Object.Destroy(m_InsedParticle);
                m_InsedParticle.SetActive(false);
                m_InsedParticle = null;
            }
        }

        public override void StartEffect()
        {
            base.StartEffect();
            if (EventObj.OnlyTriggerWhenPowerEnough && !base.CurrentSkill.IsPowerEnough)
            {
                return;
            }

            DestroyObj();
            m_InsedParticle = EventObj.ObjData.CreateObject(Actor);
            //Debug.Log("create "+m_InsedParticle.name);

            /*
            var effect = config.m_Effect[index];
            Vector3 pos = Actor.transform.TransformPoint(effect.m_Position);
            Quaternion rot = Actor.transform.rotation * Quaternion.Euler(effect.m_Rotation);
            GameApp.Entry.Game.Effect.CreateEffect(effect.m_Effect, Actor.transform, pos, rot, effect.m_HoldSeconds);
             */

            if (EventObj.m_HoldTime > 0 && EventObj.GetEventTimeType() == AbilityEventObj.EventTimeType.EventTime)
            {
                GameApp.Entry.Unity.DoDelayAction(EventObj.m_HoldTime, DestroyObj);
            }
        }

        public override void Release()
        {
            base.Release();
            if (EventObj.m_HoldTime <= 0)
                DestroyObj();
        }
    }

    public partial class AbilityEventEffect_Particles : AbilityEventEffect
    {
        private AbilityEventObj_Particles EventObj { get; set; }

        public AbilityEventEffect_Particles(AbilityEventObj obj) : base(obj)
        {
            m_EventObj = obj;
            EventObj = (AbilityEventObj_Particles)obj;
        }

        /*
       
        //Vector3 TargetPos => _combatController.transform.position + _combatController._animator.transform.rotation * Obj.Offset;

        GameObject m_InsedParticle;

        public override void EndEffect()
        {
            base.EndEffect();
            if (Obj.GetEventTimeType() == AbilityEventObj.EventTimeType.EventRange)
            {
                if (m_InsedParticle != null)
                {
                    Object.Destroy(m_InsedParticle);
                }
            }
        }

        public override void StartEffect()
        {
            base.StartEffect();
            m_InsedParticle = Obj.ObjData.CreateObject(_combatController);
        }
        */
    }
}