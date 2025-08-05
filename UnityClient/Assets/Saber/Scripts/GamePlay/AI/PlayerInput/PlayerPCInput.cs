using Saber.CharacterController;
using Saber.Frame;

using Saber.World;
using UnityEngine;

namespace Saber.AI
{
    public class PlayerPCInput : PlayerInput
    {
        private SInput m_Input;


        public override bool Active
        {
            set => throw new System.NotImplementedException();
        }


        public PlayerPCInput()
        {
            InitInputKeys();
        }


        public override void OnPlayerExitPortal(Portal portal)
        {
            throw new System.NotImplementedException();
        }

        public override void OnPlayerEnterPortal(Portal portal)
        {
            throw new System.NotImplementedException();
        }

        public override void OnPlayerEnterGodStatue(GodStatue godStatue)
        {
            throw new System.NotImplementedException();
        }

        public override void OnPlayerExitGodStatue(GodStatue godStatue)
        {
            throw new System.NotImplementedException();
        }

        public override void Init(SActor actor)
        {
            base.Init(actor);
            Actor.MoveSpeedV = EMoveSpeedV.Walk;
        }

        void InitInputKeys()
        {
            m_Input = new();

            // camera
            var moveCamLeft = new InputRow(true, "MoveCamera", "MoveCamera", KeyCode.Q, InputButton.Press, InputType.Key);
            moveCamLeft.OnInputDown.AddListener(() => PlayerCameraObj.MovementAxis = new Vector2(-0.3f, 0));
            moveCamLeft.OnInputUp.AddListener(() => PlayerCameraObj.MovementAxis = Vector2.zero);
            m_Input.Inputs.Add(moveCamLeft);

            var moveCamRight = new InputRow(true, "MoveCamera", "MoveCamera", KeyCode.E, InputButton.Press, InputType.Key);
            moveCamRight.OnInputDown.AddListener(() => PlayerCameraObj.MovementAxis = new Vector2(0.3f, 0));
            moveCamRight.OnInputUp.AddListener(() => PlayerCameraObj.MovementAxis = Vector2.zero);
            m_Input.Inputs.Add(moveCamRight);

            var moveCamUp = new InputRow(true, "MoveCamera", "MoveCamera", KeyCode.Z, InputButton.Press, InputType.Key);
            moveCamUp.OnInputDown.AddListener(() => PlayerCameraObj.MovementAxis = new Vector2(0, -0.1f));
            moveCamUp.OnInputUp.AddListener(() => PlayerCameraObj.MovementAxis = Vector2.zero);
            m_Input.Inputs.Add(moveCamUp);

            var moveCamDown = new InputRow(true, "MoveCamera", "MoveCamera", KeyCode.X, InputButton.Press, InputType.Key);
            moveCamDown.OnInputDown.AddListener(() => PlayerCameraObj.MovementAxis = new Vector2(0, 0.1f));
            moveCamDown.OnInputUp.AddListener(() => PlayerCameraObj.MovementAxis = Vector2.zero);
            m_Input.Inputs.Add(moveCamDown);

            // move
            m_Input.MovementEvent.AddListener(OnMovementInput);

            // sprint
            var sprint = new InputRow(true, "Sprint", "Sprint", KeyCode.LeftShift, InputButton.Press, InputType.Key);
            sprint.OnInputDown.AddListener(OnSprintDown);
            sprint.OnInputUp.AddListener(OnSprintUp);
            m_Input.Inputs.Add(sprint);

            // jump
            var jump = new InputRow(true, "Jump", "Jump", KeyCode.G, InputButton.Down, InputType.Key);
            jump.OnInputDown.AddListener(OnJumpDown);
            m_Input.Inputs.Add(jump);

            // dodge
            var dodge = new InputRow(true, "Dodge", "Dodge", KeyCode.Space, InputButton.Down, InputType.Key);
            dodge.OnInputDown.AddListener(OnDodgeDown);
            m_Input.Inputs.Add(dodge);

            // lockEnemy
            var lockEnemy = new InputRow(true, "LockingEnemy", "LockingEnemy", KeyCode.Tab, InputButton.Down, InputType.Key);
            lockEnemy.OnInputDown.AddListener(OnLockEnemyDown);
            m_Input.Inputs.Add(lockEnemy);

            // defense
            var defense = new InputRow(true, "Defense", "Defense", KeyCode.F, InputButton.Press, InputType.Key);
            defense.OnInputDown.AddListener(OnDefenseDown);
            defense.OnInputUp.AddListener(OnDefenseUp);
            m_Input.Inputs.Add(defense);

            // skill
            var attack1 = new InputRow(true, "Attack1", "Attack1", KeyCode.Alpha1, InputButton.Down, InputType.Key);
            attack1.OnInputDown.AddListener(() => OnTriggerSkill(ESkillType.LightAttack));
            m_Input.Inputs.Add(attack1);

            var attack2 = new InputRow(true, "Attack2", "Attack2", KeyCode.Alpha2, InputButton.Down, InputType.Key);
            attack2.OnInputDown.AddListener(() => OnTriggerSkill(ESkillType.HeavyAttack));

            // ESC
            var esc = new InputRow(true, "ESC", "ESC", KeyCode.Escape, InputButton.Down, InputType.Key);
            esc.OnInputDown.AddListener(OnClickESC);
            m_Input.Inputs.Add(esc);
        }

        private void OnClickESC()
        {
            GameApp.Entry.Game.World?.OnClickMenu();
        }

        private void OnSprintUp()
        {
            Actor.MoveSpeedV = EMoveSpeedV.Walk;
        }

        private void OnSprintDown()
        {
            Actor.MoveSpeedV = EMoveSpeedV.Run;
        }

        void OnMovementInput(Vector3 input)
        {
            Actor.MovementAxis = input;

            if (input != Vector3.zero)
            {
                Actor.StartMove();
            }
        }

        public override void Update()
        {
            base.Update();
            m_Input.Update();
        }
    }
}