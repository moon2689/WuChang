using System.Collections;
using System.Collections.Generic;
using Saber.CharacterController;
using UnityEngine;
using UnityEngine.Serialization;

namespace CombatEditor
{
    public class ExampleTopdownHitBox : HitBox
    {
        /// <summary>
        /// The Effects on Hit.
        /// </summary>
        public ExampleTopdownController.Team Team;

        public ENodeType onHitParticlesENode;
        public ParticleSystem OnHitParticles;
        public AudioClip OnHitSFX;
        public float FreezeFrameTime;
        public ExampleTopdownController.CharacterState TargetState;


        private void OnTriggerEnter(Collider other)
        {
            var controller = other.GetComponent<ExampleTopdownController>();
            if (controller == null) return;
            if (controller.team != Team)
            {
                return;
            }

            controller.ChangeState(TargetState);
            if (OnHitSFX != null)
            {
                OnHitSFX.PlayClip(1);
            }

            TryHitController(controller.GetComponent<CombatController>());
        }

        public void TryHitController(CombatController _controller)
        {
            var _combatController = _controller;
            if (_combatController != null)
            {
                // ChangeDirection
                var AttackDir = (Actor.transform.position - _combatController.transform.position).normalized;
                _combatController._animator.transform.forward = new Vector3(AttackDir.x, 0, AttackDir.z);

                //Freeze Frame.
                _combatController._animSpeedExecutor.AddSpeedModifiers(0, FreezeFrameTime);
                //Actor._animSpeedExecutor.AddSpeedModifiers(0, FreezeFrameTime);
                //CreateParticles
                if (OnHitParticles != null)
                {
                    var NodeTransform = _combatController.GetNodeTranform(onHitParticlesENode);
                    if (NodeTransform != null)
                    {
                        var Particles = Instantiate(OnHitParticles);
                        Particles.transform.position = NodeTransform.transform.position;
                    }
                }
            }
        }
    }
}