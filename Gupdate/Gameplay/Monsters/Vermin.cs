using System;
using BepInEx;
using EntityStates.FlyingVermin.Weapon;
using EntityStates.Vermin.Weapon;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gupdate.Gameplay.Monsters
{
    public class Vermin : ModBehaviour
    {
        public DamageAPI.ModdedDamageType Poison1sOnHit { get; private set; }

        public void Awake()
        {
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/FlyingVermin/FlyingVerminBody.prefab").Completed += handle =>
            {
                CharacterBody flyingVerminBody = handle.Result.GetComponent<CharacterBody>();
                flyingVerminBody.baseDamage = 12f;
                flyingVerminBody.levelDamage = 2.4f;
            };
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Vermin/VerminBody.prefab").Completed += handle =>
            {
                CharacterBody verminBody = handle.Result.GetComponent<CharacterBody>();
                verminBody.baseDamage = 12f;
                verminBody.levelDamage = 2.4f;
            };

            Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/DLC1/Vermin/EntityStates.Vermin.Weapon.TongueLash.asset").Completed += handle =>
            {
                handle.Result.TryModifyFieldValue(nameof(TongueLash.damageCoefficient), 1f);
                handle.Result.TryModifyFieldValue(nameof(TongueLash.procCoefficient), 0.75f);
            };
            Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/DLC1/FlyingVermin/EntityStates.FlyingVermin.Weapon.Spit.asset").Completed += handle =>
            {
                handle.Result.TryModifyFieldValue(nameof(Spit.damageCoefficient), 1f);
            };

            Poison1sOnHit = DamageAPI.ReserveDamageType();
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/FlyingVermin/VerminSpitProjectile.prefab").Completed += handle =>
            {
                DamageAPI.ModdedDamageTypeHolderComponent holder = handle.Result.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>();
                holder.Add(Poison1sOnHit);
                handle.Result.GetComponent<ProjectileController>().procCoefficient = 0.75f;
            };

            On.EntityStates.Vermin.Weapon.TongueLash.AuthorityModifyOverlapAttack += TongueLash_AuthorityModifyOverlapAttack;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            if (damageInfo.procCoefficient == 0f || damageInfo.rejected || !NetworkServer.active)
            {
                return;
            }
            if (damageInfo.attacker && victim && damageInfo.HasModdedDamageType(Poison1sOnHit))
            {
                uint? maxStacksFromAttacker = null;

                if (damageInfo.inflictor && damageInfo.inflictor.TryGetComponent(out ProjectileDamage projectileDamage) && projectileDamage.useDotMaxStacksFromAttacker)
                {
                    maxStacksFromAttacker = new uint?(projectileDamage.dotMaxStacksFromAttacker);
                }

                DotController.InflictDot(victim, damageInfo.attacker, DotController.DotIndex.Poison, 1f * damageInfo.procCoefficient, 1f, maxStacksFromAttacker);
            }
        }

        private void TongueLash_AuthorityModifyOverlapAttack(On.EntityStates.Vermin.Weapon.TongueLash.orig_AuthorityModifyOverlapAttack orig, TongueLash self, OverlapAttack overlapAttack)
        {
            overlapAttack.AddModdedDamageType(Poison1sOnHit);
            orig(self, overlapAttack);
        }
    }
}
