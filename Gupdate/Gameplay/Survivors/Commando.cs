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

            Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Commando/CommandoBodyBarrage.asset").Completed += handle =>
            {
                handle.Result.mustKeyPress = false;
            };

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
                explosion.explosionEffect = impactExplosion.explosionEffect;
                ProjectileFuse fuse = handle.Result.AddComponent<ProjectileFuse>();
                fuse.fuse = 1f;
                fuse.onFuse = new UnityEvent();
                handle.Result.AddComponent<SetupFuseEvent>();
                RigidbodySoundOnImpact rigidbodySoundOnImpact = handle.Result.AddComponent<RigidbodySoundOnImpact>();
                rigidbodySoundOnImpact.networkedSoundEvent = impactExplosion.lifetimeExpiredSound;
                DestroyImmediate(impactExplosion);
            };

            On.RoR2.Skills.SteppedSkillDef.OnFixedUpdate += SteppedSkillDef_OnFixedUpdate;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void SteppedSkillDef_OnFixedUpdate(On.RoR2.Skills.SteppedSkillDef.orig_OnFixedUpdate orig, SteppedSkillDef self, GenericSkill skillSlot)
        {
            if (self.skillName == "CrocoSlash" && self.canceledFromSprinting && skillSlot.characterBody.isSprinting && skillSlot.stateMachine.state.GetType() == self.activationState.stateType)
            {
                ((SteppedSkillDef.InstanceData)skillSlot.skillInstanceData).step = 0;
            }
            orig(self, skillSlot);
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            args.armorAdd -= sender.GetBuffCount(RoR2Content.Buffs.Blight) * 5f;
        }

        public class SetupFuseEvent : MonoBehaviour
        {
            public void Awake()
            {
                base.GetComponent<ProjectileFuse>().onFuse.AddListener(base.GetComponent<ProjectileExplosion>().Detonate);
                Destroy(this);
            }
        }
    }
}
