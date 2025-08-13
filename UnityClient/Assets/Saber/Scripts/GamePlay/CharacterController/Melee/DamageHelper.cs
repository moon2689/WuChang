using System;
using CombatEditor;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public static class DamageHelper
    {
        public static bool CanDamageEnemy(SActor actor, SActor enemy)
        {
            return enemy != null && enemy != actor && !enemy.IsDead && enemy.Camp != actor.Camp;
        }

        /*
        static Vector3 GetDamagePos(SActor actor, HurtBox hurtBox, WeaponDamageSetting damageSetting,
            out Vector3 waveDir, out EWeaponType weaponType)
        {
            Vector3 dmgPos = hurtBox.transform.position;
            var weapon = actor.GetWeaponByPos(damageSetting.m_WeaponBone);
            if (weapon == null)
            {
                waveDir = Vector3.down;
                weaponType = EWeaponType.Boxing;
                return dmgPos;
            }

            waveDir = weapon.WeaponWaveDirection.normalized;
            weaponType = weapon.WeaponType;
            dmgPos.y = Mathf.Min(weapon.CurWeaponPosition.y,
                hurtBox.Actor.transform.position.y + hurtBox.Actor.CPhysic.Height);

            return dmgPos;
        }
        */

        public static bool TryHit(Collider other, SActor actor,
            WeaponDamageSetting damageSetting, DamageInfo curDmgInfo)
        {
            HurtBox hurtBox = other.GetComponent<HurtBox>();
            return TryHit(hurtBox, actor, damageSetting, curDmgInfo);
        }

        public static bool TryHit(HurtBox hurtBox, SActor actor,
            WeaponDamageSetting damageSetting, DamageInfo curDmgInfo)
        {
            if (hurtBox == null)
            {
                return false;
            }

            if (!CanDamageEnemy(actor, hurtBox.Actor))
            {
                return false;
            }

            if (hurtBox.Actor.Invincible)
            {
                return false;
            }

            TryHitEnemy(actor, hurtBox, damageSetting, curDmgInfo);
            return true;
        }

        static void TryHitEnemy(SActor actor, HurtBox hurtBox, WeaponDamageSetting damageSetting, DamageInfo curDmgInfo)
        {
            //WeaponDamageSetting damageSetting = base.m_EventObj.EventObj.m_WeaponDamageSetting;

            //Debug.Log($"Do damage,hurt:{hurtBox.name}, pos:{position}, dir:{direction}", hurtBox);
            SActor enemy = hurtBox.Actor;
            if (enemy.IsDead || enemy.Invincible)
            {
                return;
            }

            if (enemy.CMelee.AttackedDamageInfo != null && Time.time - enemy.CMelee.AttackedDamageInfo.Time < 0.2f)
            {
                return;
            }

            //Debug.Log($"Do damage,hurt:{hurtBox.name}, actor:{actor}", hurtBox);

            var weapon = actor.GetWeaponByPos(damageSetting.m_WeaponBone);
            if (weapon == null)
            {
                Debug.LogError($"No weapon in bone:{damageSetting.m_WeaponBone}");
            }

            curDmgInfo.Attacker = actor;
            /*
            curDmgInfo.DamagePosition = GetDamagePos(actor, hurtBox, damageSetting, out Vector3 waveDir,
                out EWeaponType weaponType);
            curDmgInfo.DamageDirection = waveDir;
            */
            curDmgInfo.DamageConfig = damageSetting;
            curDmgInfo.ObstructType = EObstructType.Normal;
            curDmgInfo.DamageValue =
                UnityEngine.Random.Range(damageSetting.m_DamageValue * 0.8f, damageSetting.m_DamageValue * 1.2f);
            curDmgInfo.DamagingWeaponType = weapon.WeaponType;
            curDmgInfo.m_HurtBox = hurtBox;
            curDmgInfo.Time = Time.time;

            // SDebug.DrawWireSphere(curDmgInfo.DmgPos, Color.red, 0.2f, 10);
            // SDebug.DrawArrow(curDmgInfo.DmgPos, curDmgInfo.DmgDir * 3, Color.red, 10);

            enemy.CMelee.AttackedDamageInfo = curDmgInfo;

            // 尝试格挡
            if (TryDefense(actor, hurtBox, curDmgInfo))
                return;

            // 卡帧 Freeze Frame.
            FreezeFrame(actor, hurtBox);

            // 扣血
            if (curDmgInfo.DamageValue > 0)
                enemy.CStats.TakeDamage(curDmgInfo.DamageValue);

            PlayDamageEffect(actor, hurtBox, curDmgInfo);

            // 声音
            PlayHitSound(actor, false, hurtBox, curDmgInfo);

            // // 死亡
            // if (enemy.IsDead)
            // {
            //     enemy.Die(curDmgInfo);
            //     return;
            // }

            bool isBlockBroken = enemy.CurrentStateType == EStateType.GetHit &&
                                 enemy.CStateMachine.CurrentState is IHitRecovery hitRec &&
                                 hitRec.HurtType == EHitRecHurtType.BlockBroken;
            if (!isBlockBroken)
            {
                // 受击反馈
                enemy.OnHit(curDmgInfo);
            }

            // 骨骼受击抖动
            float force = GameApp.Entry.Config.GameSetting.IKBoneForceOnHit;
            curDmgInfo.m_HurtBox.OnHit(curDmgInfo.DamageDirection * force, curDmgInfo.DamagePosition);

            // 击退的力
            if (curDmgInfo.DamageConfig.m_ForceWhenGround.x > 0)
            {
                Vector3 dir = actor.transform.position - enemy.transform.position;
                dir.y = 0;
                enemy.CPhysic.Force_Add(-dir, curDmgInfo.DamageConfig.m_ForceWhenGround.x, 0, false);
            }
        }

        // 卡帧
        static void FreezeFrame(SActor actor, HurtBox hurtBox)
        {
            SActor enemy = hurtBox.Actor;
            float speed = 0.1f;
            float time = 0.1f;

            enemy.CAnim.CartonFrames(time, speed);
            actor.CAnim.CartonFrames(time, speed);
            // enemy.m_AnimSpeedExecutor.AddSpeedModifiers(speed, time);
            // actor.m_AnimSpeedExecutor.AddSpeedModifiers(speed, time);
        }

        static bool TryDefense(SActor actor, HurtBox hurtBox, DamageInfo curDmgInfo)
        {
            SActor enemy = hurtBox.Actor;

            if (enemy.CurrentStateType != EStateType.Defense)
            {
                return false;
            }

            DefenseBase defenseObj = (DefenseBase)enemy.CStateMachine.CurrentState;
            if (!defenseObj.CanDefense(actor))
            {
                return false;
            }

            PlayHitSound(actor, true, hurtBox, curDmgInfo);
            PlayDefenseEffect(enemy, curDmgInfo);

            // 卡帧 Freeze Frame.
            enemy.m_AnimSpeedExecutor.AddSpeedModifiers(0, 0.12f);
            actor.m_AnimSpeedExecutor.AddSpeedModifiers(0, 0.12f);

            // 是否成功弹反
            if (enemy.CStateMachine.ParriedSuccssSkills.Count > 0 &&
                actor.CurrentSkill != null &&
                enemy.CStateMachine.ParriedSuccssSkills.Contains(actor.CurrentSkill))
            {
                enemy.CStateMachine.ParriedSuccssSkills.Clear();

                bool isLeftDir = Vector3.Dot(curDmgInfo.DamageDirection, actor.transform.right) < 0;
                float dmgHeight = curDmgInfo.DamagePosition.y - enemy.transform.position.y;
                float dmgHeightRate = dmgHeight / enemy.CPhysic.Height;
                // SDebug.DrawArrow(actor.transform.position, curDmgInfo.DamageDirection, Color.yellow, 1);
                // SDebug.DrawArrow(actor.transform.position, actor.transform.right, Color.green, 1);
                defenseObj.PlayParriedSucceedAnim(isLeftDir, dmgHeightRate);
                return true;
            }

            enemy.DefenseHit(curDmgInfo);
            return true;
        }

        static void PlayDefenseEffect(SActor enemy, DamageInfo curDmgInfo)
        {
            if (curDmgInfo.DamagingWeaponType == EWeaponType.Boxing)
            {
                GameApp.Entry.Game.Effect.CreateEffect("Particles/FistLight", curDmgInfo.DamagePosition,
                    Quaternion.identity, 1f);
            }
            else
            {
                GameApp.Entry.Game.Effect.CreateEffect("Particles/SwordHitSword", curDmgInfo.DamagePosition,
                    Quaternion.identity, 1f);
            }
        }

        static void PlayDamageEffect(SActor actor, HurtBox hurtBox, DamageInfo curDmgInfo)
        {
            SActor enemy = hurtBox.Actor;
            GameObject prefabHit = null;
            bool showBlood = false;

            if (curDmgInfo.DamagingWeaponType == EWeaponType.Boxing)
            {
                prefabHit = GameApp.Entry.Config.GameSetting.GetRandomEffectPrefab_FistHitBody();
            }
            else if (curDmgInfo.DamagingWeaponType == EWeaponType.Claw)
            {
                prefabHit = GameApp.Entry.Config.GameSetting.GetRandomEffectPrefab_ClawHitBody();
            }
            else if (curDmgInfo.DamagingWeaponType == EWeaponType.Sword)
            {
                prefabHit = GameApp.Entry.Config.GameSetting.GetRandomEffectPrefab_SharpWeaponHitBody();
                showBlood = true;
            }
            else
            {
                Debug.LogError($"Unknown damage weapon type:{curDmgInfo.DamagingWeaponType}");
            }

            if (prefabHit)
            {
                Vector3 camForward = GameApp.Entry.Game.PlayerCamera.transform.forward;
                Vector3 project = Vector3.ProjectOnPlane(curDmgInfo.DamageDirection, camForward);
                Quaternion rot = project != Vector3.zero ? Quaternion.LookRotation(project) : Quaternion.identity;
                Vector3 pos = curDmgInfo.DamagePosition;
                // SDebug.DrawArrow(curDmgInfo.DamagePosition, curDmgInfo.DamageDirection, Color.green, 3);
                GameApp.Entry.Game.Effect.CreateEffect(prefabHit, null, pos, rot, 3f);
            }

            enemy.OnPlayDamageEffect(curDmgInfo.DamagePosition);

            // blood
            if (showBlood)
            {
                GameObject prefabBlood = GameApp.Entry.Config.GameSetting.GetRandomEffectPrefab_Blood();
                Vector3 project = Vector3.ProjectOnPlane(curDmgInfo.DamageDirection, actor.transform.forward);
                // SDebug.DrawArrow(curDmgInfo.DmgPos, project, Color.red, 3);
                Quaternion rot = project != Vector3.zero ? Quaternion.LookRotation(project) : actor.transform.rotation;
                GameApp.Entry.Game.Effect.CreateEffect(prefabBlood, null, curDmgInfo.DamagePosition, rot, 30f);
            }

            // shake camera
            if (actor.IsPlayer && GameApp.Entry.Config.GameSetting.m_ShakeCameraHitOther)
            {
                float duration = GameApp.Entry.Config.GameSetting.m_ShakeCameraDuratiHitOther;
                float amount = GameApp.Entry.Config.GameSetting.m_ShakeCameraAmountHitOther;
                float speed = GameApp.Entry.Config.GameSetting.m_ShakeCameraSpeedHitOther;
                GameApp.Entry.Game.PlayerCamera.ShakeCamera(duration, amount, speed);
            }
            else if (enemy.IsPlayer && GameApp.Entry.Config.GameSetting.m_ShakeCameraOnHurt)
            {
                float duration = GameApp.Entry.Config.GameSetting.m_ShakeCameraDurationOnHurt;
                float amount = GameApp.Entry.Config.GameSetting.m_ShakeCameraAmountOnHurt;
                float speed = GameApp.Entry.Config.GameSetting.m_ShakeCameraSpeedOnHurt;
                GameApp.Entry.Game.PlayerCamera.ShakeCamera(duration, amount, speed);
            }
        }

        static void PlayHitSound(SActor actor, bool block, HurtBox hurtBox, DamageInfo curDmgInfo)
        {
            EWeaponType weaponType = curDmgInfo.DamagingWeaponType;

            AudioClip sound = null;
            if (block)
            {
                sound = GameApp.Entry.Config.GameSetting.GetRandomSound_SwordHitSword();
            }
            else
            {
                if (weaponType == EWeaponType.Boxing)
                {
                    sound = GameApp.Entry.Config.GameSetting.GetRandomSound_FistHitBody();
                }
                else if (weaponType == EWeaponType.Claw)
                {
                    sound = GameApp.Entry.Config.GameSetting.GetRandomSound_ClawHitBody();
                }
                else if (weaponType == EWeaponType.Sword)
                {
                    sound = GameApp.Entry.Config.GameSetting.GetRandomSound_SharpWeaponHitBody();
                }
                else
                {
                    Debug.LogError("Play sound, unknown weapon:" + actor.CurrentWeaponStyle);
                }
            }

            if (sound != null)
            {
                GameApp.Entry.Game.Audio.Play3DSound(sound, actor.transform.position);
            }
            else
            {
                Debug.LogError($"Hit sound is null, block:{block}, weapon type:{weaponType}");
            }
        }
    }
}