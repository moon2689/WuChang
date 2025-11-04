using System;
using CombatEditor;
using Saber.Frame;
using UnityEngine;

namespace Saber.CharacterController
{
    public static class DamageHelper
    {
        public static bool CanDamageEnemy(IDamageMaker actor, SActor enemy)
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

        public static bool TryHit(Collider other, IDamageMaker actor, WeaponDamageSetting damageSetting, DamageInfo curDmgInfo)
        {
            HurtBox hurtBox = other.GetComponent<HurtBox>();
            return TryHit(hurtBox, actor, damageSetting, curDmgInfo);
        }

        public static bool TryHit(HurtBox hurtBox, IDamageMaker actor, WeaponDamageSetting damageSetting, DamageInfo curDmgInfo)
        {
            if (hurtBox == null)
            {
                return false;
            }

            if (!CanDamageEnemy(actor, hurtBox.Actor))
            {
                return false;
            }

            return TryHitEnemy(actor, hurtBox, damageSetting, curDmgInfo);
        }

        static bool TryHitEnemy(IDamageMaker dmgMaker, HurtBox hurtBox, WeaponDamageSetting damageSetting, DamageInfo curDmgInfo)
        {
            //WeaponDamageSetting damageSetting = base.m_EventObj.EventObj.m_WeaponDamageSetting;

            //Debug.Log($"Do damage,hurt:{hurtBox.name}, pos:{position}, dir:{direction}", hurtBox);
            SActor enemy = hurtBox.Actor;
            if (enemy.IsDead)
            {
                return false;
            }

            if (enemy.Invincible)
            {
                if (enemy.AddYuMaoWhenHitted)
                {
                    enemy.AddYuMao(1);
                    enemy.AddYuMaoWhenHitted = false;
                }

                return false;
            }

            //Debug.Log($"Do damage,hurt:{hurtBox.name}, actor:{actor}", hurtBox);

            curDmgInfo.Attacker = dmgMaker;
            /*
            curDmgInfo.DamagePosition = GetDamagePos(actor, hurtBox, damageSetting, out Vector3 waveDir,
                out EWeaponType weaponType);
            curDmgInfo.DamageDirection = waveDir;
            */
            curDmgInfo.DamageConfig = damageSetting;
            curDmgInfo.HitType = damageSetting.m_HitType;
            curDmgInfo.m_HurtBox = hurtBox;

            float enchantedDamageRatio = 1;
            if (curDmgInfo.HitType == EHitType.Weapon)
            {
                var weapon = dmgMaker.GetWeaponByPos(damageSetting.m_WeaponBone);
                if (weapon == null)
                {
                    Debug.LogError($"No weapon in bone:{damageSetting.m_WeaponBone}");
                }

                curDmgInfo.DamagingWeaponType = weapon.WeaponType;

                if (weapon.IsEnchanted)
                {
                    enchantedDamageRatio = 1.5f;
                }
            }

            curDmgInfo.DamageValue = damageSetting.m_DamageValue * enchantedDamageRatio * UnityEngine.Random.Range(0.8f, 1.2f);

            //curDmgInfo.Time = Time.time;

            // SDebug.DrawWireSphere(curDmgInfo.DmgPos, Color.red, 0.2f, 10);
            // SDebug.DrawArrow(curDmgInfo.DmgPos, curDmgInfo.DmgDir * 3, Color.red, 10);

            enemy.CMelee.AttackedDamageInfo = curDmgInfo;

            // 尝试格挡
            if (dmgMaker is SActor actor && TryDefense(actor, hurtBox, curDmgInfo, out bool isTanFanSucceed))
            {
                if (!isTanFanSucceed)
                {
                    // 卡帧 Freeze Frame.
                    FreezeFrame(dmgMaker, enemy, curDmgInfo);
                }

                return false;
            }

            // 卡帧 Freeze Frame.
            FreezeFrame(dmgMaker, enemy, curDmgInfo);

            // 扣血
            if (curDmgInfo.DamageValue > 0)
                enemy.CStats.TakeDamage(curDmgInfo.DamageValue);

            PlayDamageEffect(hurtBox, curDmgInfo);

            // 声音
            PlayHitSound(false, hurtBox, curDmgInfo);

            // 骨骼受击抖动 ik
            float force = GameApp.Entry.Config.GameSetting.IKBoneForceOnHit;
            curDmgInfo.m_HurtBox.OnHit(curDmgInfo.DamageDirection * force, curDmgInfo.DamagePosition);

            // 击退的力
            if (curDmgInfo.DamageConfig.m_ForceWhenGround.x > 0)
            {
                Vector3 dir = dmgMaker.transform.position - enemy.transform.position;
                dir.y = 0;
                enemy.CPhysic.Force_Add(-dir, curDmgInfo.DamageConfig.m_ForceWhenGround.x, 0, false);
            }

            // 死亡
            if (enemy.IsDead)
            {
                enemy.Die();
                return true;
            }

            // 打击恢复
            if (!enemy.IsInSpecialStun)
            {
                bool toHitRec = (int)damageSetting.m_ImpactForce > (int)enemy.CurrentResilience;
                if (!toHitRec)
                {
                    toHitRec = GetHit.ToBlockBroken(curDmgInfo, enemy); //是否破防
                }

                if (toHitRec)
                {
                    // 受击反馈
                    enemy.OnHit(curDmgInfo);
                }
            }

            return true;
        }

        // 卡帧
        static void FreezeFrame(IDamageMaker dmgMaker, SActor enemy, DamageInfo curDmgInfo)
        {
            float speed = 0.1f;
            float time = 0.1f;

            enemy.CAnim.CartonFrames(time, speed);

            if (curDmgInfo.HitType == EHitType.Weapon && dmgMaker is SActor actor)
            {
                actor.CAnim.CartonFrames(time, speed);
            }
            // enemy.m_AnimSpeedExecutor.AddSpeedModifiers(speed, time);
            // actor.m_AnimSpeedExecutor.AddSpeedModifiers(speed, time);
        }

        static bool TryDefense(SActor actor, HurtBox hurtBox, DamageInfo curDmgInfo, out bool isTanFanSucceed)
        {
            isTanFanSucceed = false;
            bool hitTypeCanBeDefense = curDmgInfo.HitType == EHitType.Weapon || curDmgInfo.HitType == EHitType.FeiDao;
            if (!hitTypeCanBeDefense)
            {
                return false;
            }

            SActor enemy = hurtBox.Actor;

            if (enemy.CurrentStateType != EStateType.Defense)
            {
                return false;
            }

            // 是否被弹反
            if (curDmgInfo.DamageConfig.CanBeTanFan)
            {
                bool beParried = WhetherBeparried(enemy, actor, out var defenseState);
                if (beParried)
                {
                    if (curDmgInfo.DamageConfig.BreakByTanFan)
                        actor.OnParried(defenseState.Actor); //被弹反打断技能
                    defenseState.OnTanFanSucceed(actor, curDmgInfo);
                    isTanFanSucceed = true;
                    return true;
                }
            }

            Defense defenseObj = (Defense)enemy.CStateMachine.CurrentState;
            if (!defenseObj.CanDefense(actor))
            {
                return false;
            }

            PlayHitSound(true, hurtBox, curDmgInfo);
            PlayDefenseEffect(enemy, curDmgInfo);

            // 卡帧 Freeze Frame.
            enemy.m_AnimSpeedExecutor.AddSpeedModifiers(0, 0.12f);
            actor.m_AnimSpeedExecutor.AddSpeedModifiers(0, 0.12f);

            enemy.DefenseHit(curDmgInfo);
            return true;
        }

        static bool WhetherBeparried(SActor defenser, SActor attacker, out Defense defenseState)
        {
            defenseState = (Defense)defenser.CStateMachine.CurrentState;
            if (!defenseState.InTanFanTime)
            {
                return false;
            }

            Vector3 dirToMe = attacker.transform.position - defenser.transform.position;
            if (Vector3.Dot(dirToMe, defenser.transform.forward) > 0)
                return true;

            return false;
        }

        static void PlayDefenseEffect(SActor enemy, DamageInfo curDmgInfo)
        {
            // if (curDmgInfo.HitType == EHitType.Weapon)
            // {
            GameApp.Entry.Game.Effect.CreateEffect("Particles/SwordHitSword", curDmgInfo.DamagePosition, Quaternion.identity, 1f);
            // }
            // else
            // {
            //     GameApp.Entry.Game.Effect.CreateEffect("Particles/FistLight", curDmgInfo.DamagePosition, Quaternion.identity, 1f);
            // }
        }

        static void PlayDamageEffect(HurtBox hurtBox, DamageInfo curDmgInfo)
        {
            SActor enemy = hurtBox.Actor;
            GameObject prefabHit = null;
            bool showBlood = false;

            if (curDmgInfo.HitType == EHitType.Weapon)
            {
                if (curDmgInfo.DamagingWeaponType == EWeaponType.MiaoDao ||
                    curDmgInfo.DamagingWeaponType == EWeaponType.YueYaChan ||
                    curDmgInfo.DamagingWeaponType == EWeaponType.Sword)
                {
                    prefabHit = GameApp.Entry.Config.SkillCommon.GetRandomEffectPrefab_SharpWeaponHitBody();
                    showBlood = true;
                }
                else if (curDmgInfo.DamagingWeaponType == EWeaponType.WoodStick)
                {
                    prefabHit = GameApp.Entry.Config.SkillCommon.GetRandomEffectPrefab_BoxingHitBody();
                }
                else
                {
                    Debug.LogError($"Unknown damage weapon type:{curDmgInfo.DamagingWeaponType}");
                }
            }
            else if (curDmgInfo.HitType == EHitType.Boxing || curDmgInfo.HitType == EHitType.Leg)
            {
                prefabHit = GameApp.Entry.Config.SkillCommon.GetRandomEffectPrefab_BoxingHitBody();
            }
            else if (curDmgInfo.HitType == EHitType.Magic)
            {
                prefabHit = GameApp.Entry.Config.SkillCommon.GetRandomCommonEffectPrefab_MagicHitBody();
            }
            else if (curDmgInfo.HitType == EHitType.FeiDao)
            {
                prefabHit = GameApp.Entry.Config.SkillCommon.GetRandomEffectPrefab_SharpWeaponHitBody();
                showBlood = true;
            }
            else
            {
                Debug.LogError($"Unknown damage hit type:{curDmgInfo.HitType}");
            }

            Quaternion hitDir = Quaternion.LookRotation(curDmgInfo.DamageDirection);
            Vector3 hitPos = curDmgInfo.DamagePosition;
            //SDebug.DrawArrow(hitPos, curDmgInfo.DamageDirection.normalized, Color.red, 3);

            if (prefabHit)
            {
                // SDebug.DrawArrow(curDmgInfo.DamagePosition, curDmgInfo.DamageDirection, Color.green, 3);
                GameApp.Entry.Game.Effect.CreateEffect(prefabHit, null, hitPos, hitDir, 0.6f);
            }

            enemy.OnPlayDamageEffect(curDmgInfo.DamagePosition);

            // blood
            if (showBlood)
            {
                GameObject prefabBlood = GameApp.Entry.Config.SkillCommon.GetRandomEffectPrefab_Blood();
                GameApp.Entry.Game.Effect.CreateEffect(prefabBlood, null, hitPos, hitDir, 0.6f);
            }

            // 振屏
            if (curDmgInfo.DamageConfig.m_ShakeScreenWhenHitted)
            {
                GameApp.Entry.Game.PlayerCamera.ShakeCamera(0.2f, 0.8f, 5f);
            }
        }

        static void PlayHitSound(bool block, HurtBox hurtBox, DamageInfo curDmgInfo)
        {
            AudioClip sound = null;
            if (block)
            {
                sound = GameApp.Entry.Config.SkillCommon.GetRandomSound_SwordHitSword();
            }
            else
            {
                if (curDmgInfo.HitType == EHitType.Weapon)
                {
                    sound = GameApp.Entry.Config.SkillCommon.GetRandomHitBodySound(curDmgInfo.DamagingWeaponType);
                }
                else if (curDmgInfo.HitType == EHitType.Boxing || curDmgInfo.HitType == EHitType.Leg)
                {
                    sound = GameApp.Entry.Config.SkillCommon.GetRandomBoxingHitBodySound();
                }
                else if (curDmgInfo.HitType == EHitType.Magic)
                {
                    sound = GameApp.Entry.Config.SkillCommon.GetRandomMagicHitBodyCommonSound();
                }
                else if (curDmgInfo.HitType == EHitType.FeiDao)
                {
                    sound = GameApp.Entry.Config.SkillCommon.GetRandomFeiDaoHitBodyCommonSound();
                }
            }

            if (sound != null)
            {
                GameApp.Entry.Game.Audio.Play3DSound(sound, hurtBox.transform.position);
            }
            else
            {
                Debug.LogError($"Hit sound is null, block:{block}, hit type:{curDmgInfo.HitType}");
            }
        }
    }
}