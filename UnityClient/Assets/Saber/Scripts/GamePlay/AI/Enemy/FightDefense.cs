using System.Collections;
using UnityEngine;

namespace Saber.AI
{
    public class FightDefense : EnemyAIBase
    {
        protected override void OnStart()
        {
            base.OnStart();
            SwitchCoroutine(SearchEnemy());
        }

        IEnumerator Follow()
        {
            Vector3 dirToEnemy = LockingEnemy.transform.position - Actor.transform.position;
            float disToEnemy = dirToEnemy.magnitude;
            EMoveSpeedV moveSpeed;
            if (disToEnemy > 30)
            {
                moveSpeed = EMoveSpeedV.Sprint;
            }
            else if (disToEnemy > 15)
            {
                moveSpeed = EMoveSpeedV.Run;
            }
            else
            {
                moveSpeed = EMoveSpeedV.Walk;
            }

            while (true)
            {
                //Debug.Log("Follow");
                Actor.DesiredLookDir = Vector3.ProjectOnPlane(dirToEnemy, Vector3.up).normalized;
                Actor.StartMove(moveSpeed, new Vector3(0, 0, 1));

                if (disToEnemy < 5)
                {
                    Actor.StopMove();
                    SwitchCoroutine(Idle());
                    yield break;
                }

                yield return null;

                if (LockingEnemy == null)
                {
                    SwitchCoroutine(SearchEnemy());
                    yield break;
                }

                dirToEnemy = LockingEnemy.transform.position - Actor.transform.position;
                disToEnemy = dirToEnemy.magnitude;
                if (Actor.MoveSpeedV == EMoveSpeedV.Walk)
                {
                    if (disToEnemy > 15)
                    {
                        Actor.MoveSpeedV = EMoveSpeedV.Run;
                    }
                }
                else if (Actor.MoveSpeedV == EMoveSpeedV.Run)
                {
                    if (disToEnemy > 30)
                    {
                        Actor.MoveSpeedV = EMoveSpeedV.Sprint;
                    }
                    else if (disToEnemy < 7)
                    {
                        Actor.MoveSpeedV = EMoveSpeedV.Walk;
                    }
                }
                else
                {
                    if (disToEnemy < 7)
                    {
                        Actor.MoveSpeedV = EMoveSpeedV.Walk;
                    }
                }
            }
        }

        IEnumerator Idle()
        {
            while (true)
            {
                //Debug.Log("Idle");
                if (LockingEnemy == null)
                {
                    SwitchCoroutine(SearchEnemy());
                    yield break;
                }

                Actor.StopMove();

                Vector3 dirToEnemy = LockingEnemy.transform.position - Actor.transform.position;
                if (dirToEnemy.magnitude > 10)
                {
                    SwitchCoroutine(Follow());
                    yield break;
                }

                SwitchCoroutine(Defense());

                yield return new WaitForSeconds(1f);
            }
        }


        IEnumerator Defense()
        {
            while (true)
            {
                //Debug.Log("Defense");
                if (LockingEnemy == null)
                {
                    SwitchCoroutine(SearchEnemy());
                    yield break;
                }

                Actor.DefenseStart();

                yield return new WaitForSeconds(1f);
            }
        }

        protected override void OnFoundEnemy(EFoundEnemyType foundType)
        {
            SwitchCoroutine(Follow());
        }
    }
}