using System.Collections;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class Defense : DefenseBase
    {
        private float m_TimerAlign;
        private bool m_LeftSide;
        private bool m_AutoExit;
        private string m_AnimParry;

        public override bool ApplyRootMotionSetWhenEnter => true;

        public override bool CanEnter => Actor.CPhysic.Grounded;
        public override bool CanExit => m_CurState == EState.DefenseLoop;
        private string AnimEndStr => m_LeftSide ? "Left" : "Right";

        public bool ParriedSucceed { get; set; }


        #region static

        static string Get3PosString(float dmgHeightRate)
        {
            if (dmgHeightRate < 0.5f)
            {
                return "Bottom";
            }
            else if (dmgHeightRate < 0.75f)
            {
                return "Middle";
            }
            else
            {
                return "Top";
            }
        }

        static string Get2PosString(float dmgHeightRate)
        {
            if (dmgHeightRate < 0.5f)
            {
                return "Bottom";
            }
            else
            {
                return "Top";
            }
        }

        static string GetDirString(bool isLeftDir)
        {
            return isLeftDir ? "Left" : "Right";
        }

        #endregion


        public override void Enter()
        {
            base.Enter();
            m_LeftSide = false;
            OnEnter();
        }

        public override void ReEnter()
        {
            base.ReEnter();
            OnEnter();
        }

        void OnEnter()
        {
            Actor.CAnim.StopMaskLayerAnims();

            // fix weapon location
            Actor.CMelee.CWeapon.TryFixDefenseLocation(true, m_LeftSide);

            if (m_CurState == EState.DefenseEnd)
            {
                m_CurState = EState.DefenseLoop;
                Actor.CAnim.Play($"DefenseLoop{AnimEndStr}");
            }
            else if (m_CurState == EState.None)
            {
                Actor.CAnim.Play($"DefenseStart{AnimEndStr}", onFinished: () => m_CurState = EState.DefenseLoop);
                m_CurState = EState.DefenseStart;
                m_TimerAlign = 0.1f;
            }
        }

        public override void PlayParriedSucceedAnim(bool isLeftDir, float dmgHeightRate)
        {
            base.PlayParriedSucceedAnim(isLeftDir, dmgHeightRate);
            m_AutoExit = false;

            if (m_LeftSide)
            {
                if (isLeftDir)
                {
                    string dirString = GetDirString(isLeftDir);
                    string posString = Get3PosString(dmgHeightRate);
                    m_AnimParry = $"Parry{posString}{dirString}";
                }
                else
                {
                    string posStr = Get2PosString(dmgHeightRate);
                    m_AnimParry = $"Parry{posStr}Left2Right";
                    m_LeftSide = false;
                }
            }
            else
            {
                if (!isLeftDir)
                {
                    string dirString = GetDirString(isLeftDir);
                    string posString = Get3PosString(dmgHeightRate);
                    m_AnimParry = $"Parry{posString}{dirString}";
                }
                else
                {
                    string posStr = Get2PosString(dmgHeightRate);
                    m_AnimParry = $"Parry{posStr}Right2Left";
                    m_LeftSide = true;
                }
            }

            Actor.CAnim.Play(m_AnimParry, force: true);
            GameApp.Entry.Game.Audio.Play3DSound("Sound/Skill/Parry", base.Actor.transform.position);
        }

        public override void OnStay()
        {
            base.OnStay();
            if (m_TimerAlign > 0)
            {
                m_TimerAlign -= Time.deltaTime;
                Actor.CPhysic.AlignForwardTo(Actor.DesiredLookDir, 1080f);
            }

            if (ParriedSucceed && m_AutoExit && !Actor.CAnim.IsPlayingOrWillPlay(m_AnimParry))
            {
                Exit();
            }
        }

        protected override void OnExit()
        {
            base.OnExit();
            // fix weapon location
            Actor.CMelee.CWeapon.TryFixDefenseLocation(false, m_LeftSide);
            m_CurState = EState.None;
        }

        public void EndDefense()
        {
            if (m_CurState != EState.DefenseEnd)
            {
                m_CurState = EState.DefenseEnd;

                if (ParriedSucceed && Actor.CAnim.IsPlayingOrWillPlay(m_AnimParry))
                {
                    m_AutoExit = true;
                }
                else
                {
                    Actor.CAnim.Play($"DefenseEnd{AnimEndStr}", exitTime: 0.9f, onFinished: Exit);
                }
            }
        }

        public void OnHit(DamageInfo dmgInfo)
        {
            Actor.CStats.CostStamina(20);
            Actor.CStats.TakeDamage(dmgInfo.DamageValue * 0.3f);

            if (Actor.CStats.CurrentStamina <= 0)
            {
                m_CurState = EState.DefenseBroken;
                Actor.CAnim.Play($"DefenseBroken{AnimEndStr}", force: true, onFinished: Exit);
            }
            else
            {
                m_CurState = EState.DefenseHit;
                int randomID = UnityEngine.Random.Range(1, 3);
                string randomHitAnim = $"DefenseHit{AnimEndStr}{randomID}";
                Actor.CAnim.Play(randomHitAnim, force: true, onFinished: () => m_CurState = EState.DefenseLoop);
            }

            // face to attacher
            Vector3 dir = dmgInfo.Attacker.transform.position - Actor.transform.position;
            dir.y = 0;
            Actor.transform.rotation = Quaternion.LookRotation(dir);

            // force
            if (dmgInfo.DamageConfig.m_ForceWhenGround.x > 0)
            {
                Actor.CPhysic.Force_Add(-dir, dmgInfo.DamageConfig.m_ForceWhenGround.x, 1, false);
            }
        }
    }
}