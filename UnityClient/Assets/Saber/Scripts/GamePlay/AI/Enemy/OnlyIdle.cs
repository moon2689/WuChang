using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Saber.CharacterController;
using UnityEngine.Rendering.UI;

namespace Saber.AI
{
    public class OnlyIdle : EnemyAIBase
    {
        protected override void OnStart()
        {
            base.OnStart();
            SwitchCoroutine(SearchEnemy());
        }

        protected override void OnFoundEnemy(EFoundEnemyType foundType)
        {
            SwitchCoroutine(StalemateItor());
        }

        // 对峙
        IEnumerator StalemateItor()
        {
            while (true)
            {
                if (CalcProbability(80))
                {
                    // 随机游走
                    if (m_DistanceToEnemy > 6)
                    {
                        Actor.StartMove(EMoveSpeedV.Walk, new Vector3(0, 0, 1));
                    }
                    else if (m_DistanceToEnemy < 2)
                    {
                        Actor.StartMove(EMoveSpeedV.Walk, new Vector3(0, 0, -1));
                    }
                    else
                    {
                        Vector3 axis = CalcProbability(50) ? new Vector3(1, 0, 0) : new Vector3(-1, 0, 0);
                        Actor.StartMove(EMoveSpeedV.Walk, axis);
                    }
                }
                else
                {
                    Actor.StopMove();
                }

                yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 2f));
            }
        }
    }
}