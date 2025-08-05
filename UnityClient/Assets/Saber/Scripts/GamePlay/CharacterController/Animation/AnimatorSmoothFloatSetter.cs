using UnityEngine;

namespace Saber.CharacterController
{
    public class AnimatorSmoothFloatSetter
    {
        CharacterAnimation m_Parent;
        float m_CurVel;
        private bool m_Setted;

        public EAnimatorParams ID { get; private set; }
        public float Target { get; set; }
        public float CurValue { get; private set; }


        public AnimatorSmoothFloatSetter(CharacterAnimation parent, EAnimatorParams id)
        {
            m_Parent = parent;
            ID = id;
            CurValue = parent.AnimatorObj.GetFloat(id.GetAnimatorHash());
        }

        public void ResetValue(float v)
        {
            CurValue = Target = v;
        }

        public void Update()
        {
            if (Mathf.Abs(CurValue - Target) > 0.001f)
            {
                CurValue = Mathf.SmoothDamp(CurValue, Target, ref m_CurVel, 0.2f);
            }
            else
            {
                CurValue = Target;
            }

            m_Parent.SetFloat(ID, CurValue);
        }
    }
}