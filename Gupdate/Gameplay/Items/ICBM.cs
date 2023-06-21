using System;
using BepInEx;
using EntityStates.FlyingVermin.Weapon;
using EntityStates.Vermin.Weapon;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using RoR2.Items;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using EntityStates.VoidMegaCrab.BackWeapon;
using EntityStates.Drone.DroneWeapon;
using EntityStates.VoidRaidCrab.Weapon;
using RoR2.Orbs;

namespace Gupdate.Gameplay.Items
{
    public class ICBM : ModBehaviour
    {
        private static GameObject ballisticMissile;
        private static GameObject ballisticMissileGhost;
        public override (string, string)[] GetLang() => new[]
        {
            ("ITEM_MOREMISSILE_PICKUP", "Mutually assured destruction."),
            ("ITEM_MOREMISSILE_DESC", "Whenever a <style=cIsDamage>missile</style> fires anywhere, launch a <style=cIsDamage>second strike</style> of <style=cIsDamage>1 <style=cStack>(+1 per stack)</style> missiles</style>."),
        };

        public void Awake()
        {
            ICBehaviour.missileProcChainMask.AddProc(ProcType.Missile);
            ICBehaviour.missileProcChainMask.AddProc(ProcType.MicroMissile);

            ballisticMissile = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/MissileProjectile.prefab").WaitForCompletion()
                .InstantiateClone("BallisticMissileProjectile", true);
            if (ballisticMissile.TryGetComponent(out ProjectileSingleTargetImpact projectileSingleTargetImpact))
            {
                DestroyImmediate(projectileSingleTargetImpact);
            }
            ProjectileImpactExplosion impactExplosion = ballisticMissile.AddComponent<ProjectileImpactExplosion>();
            impactExplosion.blastRadius = 4;
            impactExplosion.falloffModel = BlastAttack.FalloffModel.None;
            impactExplosion.applyDot = false;
            impactExplosion.blastDamageCoefficient = 1;
            impactExplosion.blastProcCoefficient = 1;
            impactExplosion.blastAttackerFiltering = AttackerFiltering.Default;
            impactExplosion.fireChildren = false;
            impactExplosion.impactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/OmniExplosionVFXQuick.prefab").WaitForCompletion();
            impactExplosion.destroyOnEnemy = true;
            impactExplosion.destroyOnWorld = true;
            impactExplosion.lifetime = 16f;
#pragma warning disable CS0618
            impactExplosion.explosionSoundString = "Play_item_proc_missile_explo";
#pragma warning restore CS0618

            ballisticMissileGhost = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/MissileGhost.prefab").WaitForCompletion()
                .InstantiateClone("BallisticMissileGhost", false);
            if (ballisticMissileGhost.transform.TryFind("missile VFX", out Transform missilevfx))
            {
                DestroyImmediate(missilevfx.gameObject);
            }
            GameObject icbmDisplay = Instantiate(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/MoreMissile/DisplayICBM.prefab").WaitForCompletion(), ballisticMissileGhost.transform);
            icbmDisplay.transform.localPosition = new Vector3(0f, 0f, 0.22f);
            icbmDisplay.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
            icbmDisplay.transform.localScale = new Vector3(0.15f, 0.18f, 0.15f);
            ballisticMissileGhost.GetComponentInChildren<Light>().color = new Color(1f, 0.709644f, 0.4009434f);
            TrailRenderer trail = ballisticMissileGhost.transform.Find("Trail").GetComponent<TrailRenderer>();
            trail.material = Instantiate(trail.material);
            trail.material.SetColor("_TintColor", new Color32(255, 111, 101, 255));

            ballisticMissile.GetComponent<ProjectileController>().ghostPrefab = ballisticMissileGhost;

            PrefabAPI.RegisterNetworkPrefab(ballisticMissile);
            ContentAddition.AddProjectile(ballisticMissile);

            IL.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            IL.RoR2.DroneWeaponsBoostBehavior.OnEnemyHit += DroneWeaponsBoostBehavior_OnEnemyHit;
            IL.EntityStates.VoidRaidCrab.Weapon.FireMissiles.FixedUpdate += FireMissiles_FixedUpdate;
            On.EntityStates.Drone.DroneWeapon.FireMissileBarrage.FireMissile += FireMissileBarrage_FireMissile;
            On.EntityStates.VoidMegaCrab.BackWeapon.FireVoidMissiles.FireMissile += FireVoidMissiles_FireMissile;
            IL.RoR2.MissileUtils.FireMissile_Vector3_CharacterBody_ProcChainMask_GameObject_float_bool_GameObject_DamageColorIndex_Vector3_float_bool += MissileUtils_FireMissile_Vector3_CharacterBody_ProcChainMask_GameObject_float_bool_GameObject_DamageColorIndex_Vector3_float_bool;
        }

        private void GlobalEventManager_OnHitEnemy(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (ilfound = Gutil.TryGotoStackLocIndex(c, typeof(DLC1Content), nameof(DLC1Content.Items.MissileVoid), out int locStackIndex))
            {
                ilfound = c.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld(typeof(DLC1Content.Items).GetField(nameof(DLC1Content.Items.MoreMissile))),
                x => x.MatchCallOrCallvirt<Inventory>(nameof(Inventory.GetItemCount))
                );

                if (ilfound)
                {
                    c.Emit(OpCodes.Pop);
                    c.Emit(OpCodes.Ldc_I4, 0);
                }

                int locOrbInfo = -1;
                ilfound = c.TryGotoNext(MoveType.Before,
                x => x.MatchLdloc(out locOrbInfo),
                x => x.MatchCallOrCallvirt<OrbManager>(nameof(OrbManager.AddOrb))
                );

                if (ilfound)
                {
                    c.Emit(OpCodes.Ldarg, 1);
                    c.Emit(OpCodes.Ldarg, 2);
                    c.Emit(OpCodes.Ldloc, locOrbInfo);
                    c.EmitDelegate<Action<DamageInfo, GameObject, MicroMissileOrb>>((damageInfo, victim, orb) => ICBehaviour.OnMissileFired(damageInfo.attacker?.GetComponent<CharacterBody>(), victim, orb.damageValue));
                }
            }
        }

        private void DroneWeaponsBoostBehavior_OnEnemyHit(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            int locOrbInfo = -1;
            ilfound = c.TryGotoNext(MoveType.Before,
            x => x.MatchLdloc(out locOrbInfo),
            x => x.MatchCallOrCallvirt<OrbManager>(nameof(OrbManager.AddOrb))
            );

            if (ilfound)
            {
                c.Emit(OpCodes.Ldarg, 0);
                c.Emit(OpCodes.Ldarg, 2);
                c.Emit(OpCodes.Ldloc, locOrbInfo);
                c.EmitDelegate<Action<DroneWeaponsBoostBehavior, CharacterBody, MicroMissileOrb>>((behaviour, victimBody, orb) => ICBehaviour.OnMissileFired(behaviour.body, victimBody.gameObject, orb.damageValue));
            }
        }

        private void FireMissiles_FixedUpdate(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ilfound = c.TryGotoNext(MoveType.After,
            x => x.MatchCallOrCallvirt<ProjectileManager>(nameof(ProjectileManager.FireProjectile))
            );

            if (ilfound)
            {
                c.Emit(OpCodes.Ldarg, 0);
                c.EmitDelegate<Action<FireMissiles>>(fireMissiles => ICBehaviour.OnMissileFired(fireMissiles.characterBody, null, fireMissiles.damageStat * fireMissiles.damageCoefficient));
            }
        }

        private void FireMissileBarrage_FireMissile(On.EntityStates.Drone.DroneWeapon.FireMissileBarrage.orig_FireMissile orig, FireMissileBarrage self, string targetMuzzle)
        {
            orig(self, targetMuzzle);
            if (self.isAuthority)
            {
                ICBehaviour.OnMissileFired(self.characterBody, null, self.damageStat * FireMissileBarrage.damageCoefficient);
            }
        }

        private void FireVoidMissiles_FireMissile(On.EntityStates.VoidMegaCrab.BackWeapon.FireVoidMissiles.orig_FireMissile orig, FireVoidMissiles self)
        {
            orig(self);
            if (self.isAuthority)
            {
                ICBehaviour.OnMissileFired(self.characterBody, null, self.damageStat * FireVoidMissiles.damageCoefficient);
                ICBehaviour.OnMissileFired(self.characterBody, null, self.damageStat * FireVoidMissiles.damageCoefficient);
            }
        }

        private void MissileUtils_FireMissile_Vector3_CharacterBody_ProcChainMask_GameObject_float_bool_GameObject_DamageColorIndex_Vector3_float_bool(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.Emit(OpCodes.Ldarg, 1);
            c.Emit(OpCodes.Ldarg, 3);
            c.Emit(OpCodes.Ldarg, 4);
            c.EmitDelegate<Action<CharacterBody, GameObject, float>>(ICBehaviour.OnMissileFired);

            ilfound = c.TryGotoNext(MoveType.After,
            x => x.MatchLdsfld(typeof(DLC1Content.Items).GetField(nameof(DLC1Content.Items.MoreMissile))),
            x => x.MatchCallOrCallvirt<Inventory>(nameof(Inventory.GetItemCount))
            );

            if (ilfound)
            {
                c.Emit(OpCodes.Pop);
                c.Emit(OpCodes.Ldc_I4, 0);
            }
        }

        public class ICBehaviour : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnServer = true, useOnClient = false)]
            public static ItemDef GetItemDef() => DLC1Content.Items.MoreMissile;

            public static ProcChainMask missileProcChainMask;

            public static void OnMissileFired(CharacterBody attackerBody, GameObject victim, float damage)
            {
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    projectilePrefab = ballisticMissile,
                    procChainMask = missileProcChainMask,
                    damage = damage,
                    force = 200f,
                    damageColorIndex = DamageColorIndex.Item
                };

                foreach (ICBehaviour behaviour in InstanceTracker.GetInstancesList<ICBehaviour>())
                {
                    behaviour.FireMissiles(fireProjectileInfo, attackerBody);
                }
            }

            public void FireMissiles(FireProjectileInfo fireProjectileInfo, CharacterBody attackerBody)
            {
                fireProjectileInfo.position = body.corePosition;
                fireProjectileInfo.owner = body.gameObject;
                fireProjectileInfo.crit = body.RollCrit();
                fireProjectileInfo.target = attackerBody && attackerBody.teamComponent.teamIndex == body.teamComponent.teamIndex ? null : attackerBody?.gameObject;
                for (int i = 0; i < stack; i++)
                {
                    fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(Vector3.up + UnityEngine.Random.insideUnitSphere * 0.2f);
                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                }
            }

            public void OnEnable()
            {
                InstanceTracker.Add(this);
            }

            public void OnDisable()
            {
                InstanceTracker.Remove(this);
            }
        }
    }
}
