using Saber.CharacterController;
using UnityEngine;

namespace CombatEditor
{
    public class NodeFollower : MonoBehaviour
    {
        public Transform NodeTrans;
        public bool FollowPos;
        public bool FollowRotation;
        public Vector3 PosOffset;
        public Quaternion RotOverNode;
        public SActor _controller;

        public void Init(Transform trans, Vector3 Offset, Quaternion Rot, bool followPos, bool followRot, SActor controller)
        {
            NodeTrans = trans;
            PosOffset = Offset;
            RotOverNode = Rot;
            FollowPos = followPos;
            FollowRotation = followRot;
            _controller = controller;

            transform.position = NodeTrans.position + NodeTrans.rotation * PosOffset;
            if (FollowRotation)
            {
                transform.rotation = NodeTrans.rotation * RotOverNode;
            }
            else
            {
                transform.rotation = _controller.GetNodeTransform(ENodeType.Animator).rotation * RotOverNode;
            }

            enabled = true;
        }

        public void SetTransform()
        {
            if (FollowPos)
            {
                transform.position = NodeTrans.position + NodeTrans.rotation * PosOffset;
            }

            if (FollowRotation && FollowPos)
            {
                transform.rotation = NodeTrans.rotation * RotOverNode;
            }
        }

        private void Update()
        {
            SetTransform();
        }
    }
}