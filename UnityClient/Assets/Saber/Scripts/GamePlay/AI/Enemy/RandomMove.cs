using System.Collections;
using UnityEngine;

namespace Saber.AI
{
    public class RandomMove : EnemyAIBase
    {
        protected override void OnFoundEnemy(EFoundEnemyType foundType)
        {
        }

        protected override void OnStart()
        {
            base.OnStart();
            SwitchCoroutine(Idle());
        }

        IEnumerator WalkItor()
        {
            Vector3 dir = Random.onUnitSphere;
            dir.y = 0;
            dir.Normalize();
            float timer = 5;
            while (true)
            {
                timer -= Time.deltaTime;
                if (timer < 0)
                {
                    SwitchCoroutine(Idle());
                    yield break;
                }

                Actor.DesiredLookDir = dir;
                Actor.StartMove(EMoveSpeedV.Walk, new Vector3(0, 0, 1));

                yield return null;
            }
        }

        IEnumerator Idle()
        {
            float timer = 15;
            while (true)
            {
                timer -= Time.deltaTime;
                if (timer < 0)
                {
                    SwitchCoroutine(WalkItor());
                    yield break;
                }

                Actor.StopMove();
                yield return null;
            }
        }
    }
}