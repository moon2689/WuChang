using System.Collections;
using UnityEngine;

namespace Saber.AI
{
    public class Follow : EnemyAIBase
    {
        private float m_TimerSpeech;

        protected override void OnStart()
        {
            base.OnStart();
            SwitchCoroutine(SearchEnemy());
        }

        IEnumerator FollowTarget()
        {
            Vector3 dirToEnemy = LockingEnemy.transform.position - Actor.transform.position;
            float disToEnemy = dirToEnemy.magnitude;
            EMoveSpeedV moveSpeedV;
            if (disToEnemy > 30)
            {
                moveSpeedV = EMoveSpeedV.Sprint;
            }
            else if (disToEnemy > 15)
            {
                moveSpeedV = EMoveSpeedV.Run;
            }
            else
            {
                moveSpeedV = EMoveSpeedV.Walk;
            }

            while (true)
            {
                //Debug.Log("Follow");
                Actor.DesiredLookDir = Vector3.ProjectOnPlane(dirToEnemy, Vector3.up).normalized;
                Actor.StartMove(moveSpeedV, new Vector3(0, 0, 1));

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
                float dis = dirToEnemy.magnitude;
                if (dis > 10)
                {
                    SwitchCoroutine(FollowTarget());
                    yield break;
                }

                if (Actor.CanSpeech && dis < 3 && !Actor.IsSpeeching)
                {
                    if (m_TimerSpeech > 0)
                    {
                        m_TimerSpeech -= 1;
                    }
                    else
                    {
                        m_TimerSpeech = UnityEngine.Random.Range(0.1f, 8f);
                        Actor.Speech();
                    }
                }

                yield return new WaitForSeconds(1f);
            }
        }

        protected override void OnFoundEnemy(EFoundEnemyType foundType)
        {
            SwitchCoroutine(FollowTarget());
            Actor.EyeLockAt(LockingEnemy);
        }
    }
}