using System.Collections;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public class Defense : ActorStateBase
    {
        protected enum EState
        {
            None,
            DefenseStart,
            DefenseLoop,
            DefenseEnd,
            DefenseHit,
            DefenseBroken,
        }


        private float m_TimerAlign;
        private bool m_AutoExit;
        protected EState m_CurState;
        private SCharacter m_Character;


        public override bool ApplyRootMotionSetWhenEnter => true;
        public override bool CanEnter => Actor.CPhysic.Grounded;
        public override bool CanExit => true;//m_CurState == EState.DefenseLoop;
        public bool ParriedSucceed { get; set; }
        public SCharacter Character => m_Character ??= (SCharacter)Actor;


        public Defense() : base(EStateType.Defense)
        {
        }

        public bool CanDefense(SActor enemy)
        {
            if (m_CurState == EState.DefenseEnd || m_CurState == EState.DefenseBroken)
            {
                return false;
            }

            bool isFaceToFace = Vector3.Dot(Actor.transform.forward, enemy.transform.forward) < 0;
            if (!isFaceToFace)
            {
                return false;
            }

            return true;
        }

        public override void Enter()
        {
            base.Enter();
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

            Actor.UpdateMovementAxisAnimatorParams = false;

            // fix weapon location
            //Actor.CMelee.CWeapon.TryFixDefenseLocation(true, m_LeftSide);

            if (m_CurState == EState.DefenseEnd)
            {
                m_CurState = EState.DefenseLoop;
                Actor.CAnim.Play("DefenseLoop");
            }
            else if (m_CurState == EState.None)
            {
                Actor.CAnim.Play("DefenseStart", onFinished: () => m_CurState = EState.DefenseLoop);
                m_CurState = EState.DefenseStart;
                m_TimerAlign = 0.1f;
            }
        }

        public void PlayParriedSucceedAnim(bool isLeftDir, float dmgHeightRate)
        {
            m_AutoExit = false;

            Actor.CAnim.Play("TanFan", force: true);
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

            if (m_CurState == EState.DefenseLoop)
            {
                if (Actor.MovementAxisMagnitude >= 0.1f)
                {
                    Actor.CAnim.SetSmoothFloat(EAnimatorParams.Horizontal, Actor.MovementAxis.x);
                    Actor.CAnim.SetSmoothFloat(EAnimatorParams.Vertical, Actor.MovementAxis.z);

                    // 位移
                    float speed = GetSpeed2D();
                    Actor.CPhysic.AdditivePosition += Actor.DesiredMoveDir * speed * base.DeltaTime;
                    Actor.CPhysic.AlignForwardTo(Actor.DesiredLookDir, 720);

                    // 播放移动动画
                    Actor.CAnim.Play("DefenseMove");
                }
                else
                {
                    Actor.CAnim.SetSmoothFloat(EAnimatorParams.Horizontal, 0);
                    Actor.CAnim.SetSmoothFloat(EAnimatorParams.Vertical, 0);
                }
            }

            if (ParriedSucceed && m_AutoExit && !Actor.CAnim.IsPlayingOrWillPlay("TanFan"))
            {
                Exit();
            }
        }

        float GetSpeed2D()
        {
            float curSmoothFloatH = Actor.CAnim.GetCurSmoothFloat(EAnimatorParams.Horizontal);
            float curSmoothFloatV = Actor.CAnim.GetCurSmoothFloat(EAnimatorParams.Vertical);
            float curSmoothFloat = Mathf.Sqrt(curSmoothFloatH * curSmoothFloatH + curSmoothFloatV * curSmoothFloatV);
            // float curSmoothFloat = 2 * GameHelper.GetStickLength(curSmoothFloatH * 0.5f, curSmoothFloatV * 0.5f);
            float speed = Mathf.Lerp(0, Character.m_CharacterInfo.m_SpeedDefenseWalk, curSmoothFloat);
            return speed;
        }

        protected override void OnExit()
        {
            base.OnExit();
            Actor.UpdateMovementAxisAnimatorParams = true;
            m_CurState = EState.None;
        }

        public void EndDefense()
        {
            if (m_CurState != EState.DefenseEnd)
            {
                m_CurState = EState.DefenseEnd;

                if (ParriedSucceed && Actor.CAnim.IsPlayingOrWillPlay("TanFan"))
                {
                    m_AutoExit = true;
                }
                else
                {
                    Actor.CAnim.Play($"DefenseEnd", exitTime: 0.9f, onFinished: Exit);
                }
            }

            Exit();
        }

        public void OnHit(DamageInfo dmgInfo)
        {
            Actor.CStats.CostStamina(20);
            Actor.CStats.TakeDamage(dmgInfo.DamageValue * 0.3f);

            if (Actor.CStats.CurrentStamina <= 0)
            {
                m_CurState = EState.DefenseBroken;
                Actor.CAnim.Play($"DefenseBroken", force: true, onFinished: Exit);
            }
            else
            {
                m_CurState = EState.DefenseHit;
                int randomID = UnityEngine.Random.Range(1, 3);
                string randomHitAnim = $"DefenseHit{randomID}";
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