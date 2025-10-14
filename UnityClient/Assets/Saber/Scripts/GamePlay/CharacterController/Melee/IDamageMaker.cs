using UnityEngine;

namespace Saber.CharacterController
{
    public interface IDamageMaker
    {
        EActorCamp Camp { get; }
        Transform transform { get; }

        WeaponBase GetWeaponByPos(ENodeType bone);
    }
}