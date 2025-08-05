using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Saber.CharacterController
{
    public class CharacterExpression
    {
        enum EExpressionState
        {
            Start,
            Hold,
            End,
        }

        private CharacterExpressionInfo m_Info;
        private EyeBlink m_EyeBlink;
        private int? m_CurExpressionIndex;
        private EExpressionState m_ExpressionState;
        private float m_TimerExpressionHold;
        private float m_CurExpressionWeight;

        public bool EnableEyeBlink
        {
            set { m_EyeBlink.Enable = value; }
        }

        public CharacterExpression(CharacterExpressionInfo info)
        {
            m_Info = info;
            m_EyeBlink = new();
            m_EyeBlink.OnBlendShapeUpdate = OnEyeBlink;
        }

        private void OnEyeBlink(float obj)
        {
            SetBlendShapeWeight(m_Info.m_BlendShapeIndex_Blink, obj);
        }

        void SetBlendShapeWeight(int index, float weight)
        {
            for (int i = 0; i < m_Info.m_FaceSMRs.Length; i++)
            {
                m_Info.m_FaceSMRs[i].SetBlendShapeWeight(index, weight);
            }
        }

        public void Update(float deltaTime)
        {
            m_EyeBlink.Update(deltaTime);

            if (m_CurExpressionIndex != null)
            {
                if (m_ExpressionState == EExpressionState.Start)
                {
                    m_CurExpressionWeight += deltaTime * 300;
                    if (m_CurExpressionWeight >= 100)
                    {
                        m_CurExpressionWeight = 100;
                        m_ExpressionState = EExpressionState.Hold;
                    }
                }
                else if (m_ExpressionState == EExpressionState.Hold)
                {
                    m_CurExpressionWeight = 100;
                    m_TimerExpressionHold -= deltaTime;
                    if (m_TimerExpressionHold <= 0)
                    {
                        m_ExpressionState = EExpressionState.End;
                    }
                }
                else if (m_ExpressionState == EExpressionState.End)
                {
                    m_CurExpressionWeight -= deltaTime * 300;
                    if (m_CurExpressionWeight <= 0)
                    {
                        SetBlendShapeWeight(m_CurExpressionIndex.Value, 0);
                        m_CurExpressionWeight = 0;
                        m_CurExpressionIndex = null;
                    }
                }

                if (m_CurExpressionIndex != null)
                {
                    SetBlendShapeWeight(m_CurExpressionIndex.Value, m_CurExpressionWeight);
                }
            }
        }

        public void CloseEye()
        {
            m_EyeBlink.CloseEye();
        }

        public void OpenEye()
        {
            m_EyeBlink.OpenEye();
        }

        public void DoExpression(EExpressionType expressionType, float holdTime)
        {
            if (expressionType == EExpressionType.OpenMouth)
            {
                m_CurExpressionIndex = m_Info.m_BlendShapeIndex_OpenMouth;
                m_ExpressionState = EExpressionState.Start;
                m_CurExpressionWeight = 0;
                m_TimerExpressionHold = holdTime;
            }
            else
            {
                throw new InvalidOperationException($"Unknown expression:{expressionType}");
            }
        }
    }
}