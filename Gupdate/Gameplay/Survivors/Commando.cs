using System;
using BepInEx;
using EntityStates.Commando.CommandoWeapon;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using RoR2.Skills;
using UnityEngine.Events;
using RoR2.Audio;

namespace Gupdate.Gameplay.Monsters
{
    public class Commando : ModBehaviour
    {
        public override (string, string)[] GetLang() => new[]
        {
            ("COMMANDO_SPECIAL_ALT1_DESCRIPTION", "Throw a grenade that explodes for <style=cIsDamage>900% damage</style>. Can hold up to 2."),
        };

        public void Awake()
        {
            Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Junk/CommandoPerformanceTest/EntityStates.Commando.CommandoWeapon.FireBarrage.asset").Completed += handle =>
            {
                handle.Result.TryModifyFieldValue(nameof(FireBarrage.bulletRadius), 3f);
                handle.Result.TryModifyFieldValue(nameof(FireBarrage.totalDuration), 1.2f);
                handle.Result.TryModifyFieldValue(nameof(FireBarrage.baseBulletCount), 8);
            };

            /*Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Commando/CommandoBodyBarrage.asset").Completed += handle =>
            {
                handle.Result.mustKeyPress = false;
            };*/

            Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Commando/CommandoBodyRoll.asset").Completed += handle =>
            {
                handle.Result.cancelSprintingOnActivation = false;
            };

            Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/Commando/EntityStates.Commando.CommandoWeapon.ThrowGrenade.asset").Completed += handle =>
            {
                handle.Result.TryModifyFieldValue(nameof(ThrowGrenade.damageCoefficient), 9f);
            };

            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/CommandoGrenadeProjectile.prefab").Completed += handle =>
            {
                ProjectileImpactExplosion impactExplosion = handle.Result.GetComponent<ProjectileImpactExplosion>();
                ProjectileExplosion explosion = handle.Result.AddComponent<ProjectileExplosion>();
                explosion.applyDot = false;
                explosion.blastAttackerFiltering = impactExplosion.blastAttackerFiltering;
                explosion.blastDamageCoefficient = impactExplosion.blastDamageCoefficient;
                explosion.blastProcCoefficient = impactExplosion.blastProcCoefficient;
                explosion.blastRadius = impactExplosion.blastRadius;
                explosion.bonusBlastForce = impactExplosion.bonusBlastForce;
                explosion.canRejectForce = impactExplosion.canRejectForce;
                explosion.explosionEffect = impactExplosion.explosionEffect;
                explosion.falloffModel = BlastAttack.FalloffModel.Linear;
                explosion.fireChildren = false;
                explosion.explosionEffect = impactExplosion.impactEffect;
                ProjectileFuse fuse = handle.Result.AddComponent<ProjectileFuse>();
                fuse.fuse = 1f;
                fuse.onFuse = new UnityEvent();
                handle.Result.AddComponent<SetupFuseEvent>();
                RigidbodySoundOnImpact rigidbodySoundOnImpact = handle.Result.AddComponent<RigidbodySoundOnImpact>();
                rigidbodySoundOnImpact.networkedSoundEvent = impactExplosion.lifetimeExpiredSound;
                DestroyImmediate(impactExplosion);
            };

            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/OmniExplosionVFXCommandoGrenade.prefab").Completed += handle =>
            {
                if (handle.Result.transform.TryFind("ScaledHitsparks 1", out Transform scaledHitsparks))
                {
                    scaledHitsparks.transform.localScale = Vector3.one * 1.5f;
                }
                if (handle.Result.transform.TryFind("ScaledSmoke, Billboard", out Transform scaleSmoke))
                {
                    scaleSmoke.transform.localScale = Vector3.one * 1.8f;
                }
                if (handle.Result.transform.TryFind("ScaledSmokeRing, Mesh", out Transform scaleSmokeRing))
                {
                    scaleSmokeRing.transform.localScale = Vector3.one * 2f;
                }
                if (handle.Result.transform.TryFind("Unscaled Flames", out Transform unscaledFlames) && unscaledFlames.TryGetComponent(out ParticleSystem flameParticles))
                {
                    var main = flameParticles.main;
                    main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                    unscaledFlames.transform.localScale = Vector3.one * 0.8f;
                }
                if (handle.Result.transform.TryFind("Unscaled Smoke, Billboard", out Transform unscaledSmoke) && unscaledSmoke.TryGetComponent(out ParticleSystem smokeParticles))
                {
                    var main = smokeParticles.main;
                    main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                }
            };

            On.EntityStates.Commando.CommandoWeapon.FirePistol2.OnEnter += FirePistol2_OnEnter;
        }

        private void FirePistol2_OnEnter(On.EntityStates.Commando.CommandoWeapon.FirePistol2.orig_OnEnter orig, FirePistol2 self)
        {
            orig(self);
            if (self.pistol % 2 != 0)
            {
                self.duration *= 1.2f;
            }
        }

        public class SetupFuseEvent : MonoBehaviour
        {
            public void Awake()
            {
                ProjectileFuse fuse = base.GetComponent<ProjectileFuse>();
                fuse.onFuse.AddListener(PlaySound);
                fuse.onFuse.AddListener(base.GetComponent<ProjectileExplosion>().Detonate);
            }

            public void PlaySound()
            {
                PointSoundManager.EmitSoundLocal(AkSoundEngine.GetIDFromString("Play_item_proc_behemoth"), base.transform.position);
            }
        }
    }
}
