﻿using System;
using BepInEx;
using EntityStates.Treebot.Weapon;
using EntityStates.Treebot;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using RoR2.Skills;
using EntityStates;
using System.Linq;
using RoR2.Audio;

namespace Gupdate.Gameplay.Monsters
{
    public class REX : ModBehaviour
    {
        public string[] skillDefsToPromote = new[]
        {
            "RoR2/Base/Treebot/TreebotBodyPlantSonicBoom.asset",
            "RoR2/Base/Treebot/TreebotBodySonicBoom.asset",
        };

        public static BuffDef StackableFuiting { get; private set; } = ScriptableObject.CreateInstance<BuffDef>();
        public static NetworkSoundEventDef InjectFruit { get; private set; } = ScriptableObject.CreateInstance<NetworkSoundEventDef>(); 
        private static DamageAPI.ModdedDamageType fruitinOrFreakin;
        private static DotController.DotIndex fruitingDot;

        public override (string, string)[] GetLang() => new[]
        {
            ("TREEBOT_SPECIAL_ALT1_NAME", "DIRECTIVE: Harvest"),
        };

        public void Awake()
        {
            StackableFuiting.name = "bdStackableFruiting";
            StackableFuiting.canStack = true;
            StackableFuiting.isDebuff = false;
            StackableFuiting.isCooldown = false;
            StackableFuiting.buffColor = Color.white;
            StackableFuiting.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Treebot/texBuffFruiting.tif").WaitForCompletion();
            ContentAddition.AddBuffDef(StackableFuiting);

            InjectFruit.name = "nseInjectFruit";
            InjectFruit.eventName = "Play_treeBot_R_expire";
            ContentAddition.AddNetworkSoundEventDef(InjectFruit);

            fruitinOrFreakin = DamageAPI.ReserveDamageType();
            fruitingDot = DotAPI.RegisterDotDef(3f, 1f, DamageColorIndex.Default, null, (controller, stack) => 
            {
                stack.damageType |= DamageType.BypassArmor;
                stack.AddModdedDamageType(fruitinOrFreakin);
                if (stack.attackerObject && stack.attackerObject.TryGetComponent(out CharacterBody attackerBody) && attackerBody.healthComponent)
                {
                    stack.damage = Math.Max(attackerBody.healthComponent.fullHealth * 0.25f, stack.damage);
                }
            }, null);

            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Treebot/TreebotFruitPack.prefab").Completed += handle =>
            {
                handle.Result.GetComponentInChildren<GravitatePickup>().gravitateAtFullHealth = false;
                handle.Result.GetComponent<DestroyOnTimer>().duration = 30f;
            };

            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Treebot/TreebotFruitSeedProjectile.prefab").Completed += handle =>
            {
                handle.Result.GetComponent<ProjectileDamage>().damageType &= ~DamageType.FruitOnHit;
                handle.Result.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(fruitinOrFreakin);
            };

            Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/Treebot/EntityStates.Treebot.Weapon.FirePlantSonicBoom.asset").Completed += handle =>
            {
                handle.Result.TryModifyFieldValue(nameof(FirePlantSonicBoom.healthFractionPerHit), 0.05f);
            };

            Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/Treebot/EntityStates.Treebot.TreebotFireFruitSeed.asset").Completed += handle =>
            {
                handle.Result.TryModifyFieldValue(nameof(TreebotFireFruitSeed.baseDuration), 0.8f);
            };
            Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/Treebot/EntityStates.FireFlower2.asset").Completed += handle =>
            {
                handle.Result.TryModifyFieldValue(nameof(FireFlower2.baseDuration), 0.8f);
            };

            foreach (string key in skillDefsToPromote)
            {
                Addressables.LoadAssetAsync<SkillDef>(key).Completed += handle =>
                {
                    handle.Result.interruptPriority = InterruptPriority.PrioritySkill;
                };
            }

            On.RoR2.DotController.OnDotStackRemovedServer += DotController_OnDotStackRemovedServer;
            On.RoR2.CharacterBody.SetBuffCount += CharacterBody_SetBuffCount;
            DotController.onDotInflictedServerGlobal += DotController_onDotInflictedServerGlobal;
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            On.EntityStates.PrepFlower2.GetMinimumInterruptPriority += PrepFlower2_GetMinimumInterruptPriority;
            On.EntityStates.Treebot.Weapon.FireSonicBoom.GetMinimumInterruptPriority += FireSonicBoom_GetMinimumInterruptPriority;
            On.EntityStates.Treebot.Weapon.ChargeSonicBoom.GetMinimumInterruptPriority += ChargeSonicBoom_GetMinimumInterruptPriority;
        }

        private void DotController_OnDotStackRemovedServer(On.RoR2.DotController.orig_OnDotStackRemovedServer orig, DotController self, object dotStack)
        {
            orig(self, dotStack);
            if (dotStack is DotController.DotStack && (dotStack as DotController.DotStack).dotIndex == fruitingDot && self.victimBody)
            {
                self.victimBody.SetBuffCount(StackableFuiting.buffIndex, 0);
            }
        }

        private void CharacterBody_SetBuffCount(On.RoR2.CharacterBody.orig_SetBuffCount orig, CharacterBody self, BuffIndex buffType, int newCount)
        {
            bool dropFruits = false;
            if (buffType == StackableFuiting.buffIndex && newCount > 8)
            {
                newCount = 8;
                dropFruits = true;
            }
            orig(self, buffType, newCount);
            if (buffType == StackableFuiting.buffIndex)
            {
                if (self.HasBuff(StackableFuiting))
                {
                    FruitingBehaviour fruitingBehaviour = self.GetComponent<FruitingBehaviour>() ?? self.gameObject.AddComponent<FruitingBehaviour>();
                    fruitingBehaviour.victimBody = self;
                    fruitingBehaviour.RecalculateFruitVFX();
                    if (NetworkServer.active && dropFruits)
                    {
                        fruitingBehaviour.DropFruitsServer();
                    }
                }
                else if (self.TryGetComponent(out FruitingBehaviour fruitingBehaviour))
                {
                    Destroy(fruitingBehaviour);
                }
            }
        }

        private void DotController_onDotInflictedServerGlobal(DotController dotController, ref InflictDotInfo inflictDotInfo)
        {
            if (inflictDotInfo.dotIndex == fruitingDot)
            {
                dotController.dotTimers[(int)fruitingDot] = 0f;
            }
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport damageReport)
        {
            if ((damageReport.damageInfo.HasModdedDamageType(fruitinOrFreakin) || damageReport.dotType == fruitingDot) && damageReport.victimBody)
            {
                if (DotController.dotControllerLocator.TryGetValue(damageReport.victimBody.gameObject.GetInstanceID(), out DotController dotController)
                    && dotController.dotStackList.Any(x => x.dotIndex == fruitingDot))
                {
                    damageReport.victimBody.AddBuff(StackableFuiting);
                    EffectManager.SpawnEffect(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Treebot/OmniImpactVFXSlashSyringe.prefab").WaitForCompletion(), new EffectData
                    {
                        origin = damageReport.victimBody.corePosition,
                        rotation = UnityEngine.Random.rotation,
                        scale = damageReport.victimBody.radius,
                    }, true);
                    EntitySoundManager.EmitSoundServer(InjectFruit.index, damageReport.victimBody.gameObject);
                    return;
                }
                InflictDotInfo inflictDotInfo = new InflictDotInfo
                {
                    attackerObject = damageReport.attacker,
                    victimObject = damageReport.victimBody.gameObject,
                    dotIndex = fruitingDot,
                    duration = Mathf.Infinity
                };
                DotController.InflictDot(ref inflictDotInfo);
            }
        }

        private InterruptPriority PrepFlower2_GetMinimumInterruptPriority(On.EntityStates.PrepFlower2.orig_GetMinimumInterruptPriority orig, PrepFlower2 self)
        {
            return InterruptPriority.Pain;
        }

        private InterruptPriority FireSonicBoom_GetMinimumInterruptPriority(On.EntityStates.Treebot.Weapon.FireSonicBoom.orig_GetMinimumInterruptPriority orig, FireSonicBoom self)
        {
            return InterruptPriority.Pain;
        }

        private InterruptPriority ChargeSonicBoom_GetMinimumInterruptPriority(On.EntityStates.Treebot.Weapon.ChargeSonicBoom.orig_GetMinimumInterruptPriority orig, ChargeSonicBoom self)
        {
            return InterruptPriority.Pain;
        }

        public class FruitingBehaviour : MonoBehaviour, IOnKilledServerReceiver
        {
            public CharacterBody victimBody;
            private TemporaryVisualEffect fruitingVFXInstance;
            private ParticleSystem fruitingParticles;

            public void Start()
            {
                if (victimBody)
                {
                    GameObject instance = Instantiate(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Treebot/FruitingEffect.prefab").WaitForCompletion(), victimBody.corePosition, Quaternion.identity);
                    fruitingVFXInstance = instance.GetComponent<TemporaryVisualEffect>();
                    fruitingVFXInstance.parentTransform = victimBody.coreTransform;
                    fruitingVFXInstance.visualState = TemporaryVisualEffect.VisualState.Enter;
                    fruitingVFXInstance.healthComponent = victimBody.healthComponent;
                    fruitingVFXInstance.radius = victimBody.radius;
                    fruitingParticles = instance.transform.Find("MeshHolder/Ring Particle System").GetComponent<ParticleSystem>();
                    RecalculateFruitVFX();
                }
            }

            public void OnDestroy()
            {
                if (fruitingVFXInstance)
                {
                    fruitingVFXInstance.visualState = TemporaryVisualEffect.VisualState.Exit;
                }
            }

            public void RecalculateFruitVFX()
            {
                if (fruitingParticles && victimBody)
                {
                    int fruitCount = victimBody.GetBuffCount(StackableFuiting);
                    var main = fruitingParticles.main;
                    main.maxParticles = fruitCount;
                    var emission = fruitingParticles.emission;
                    emission.rateOverTime = fruitCount;
                }
            }

            public void DropFruitsServer()
            {
                if (!NetworkServer.active || !victimBody)
                {
                    return;
                }
                int fruitCount = victimBody.GetBuffCount(StackableFuiting);
                GameObject original = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/TreebotFruitPack");
                for (int i = 0; i < fruitCount; i++)
                {
                    GameObject instance = Instantiate(original, victimBody.corePosition + UnityEngine.Random.insideUnitSphere * victimBody.radius * 0.5f, UnityEngine.Random.rotation);
                    TeamFilter teamFilter = instance.GetComponent<TeamFilter>();
                    if (teamFilter)
                    {
                        teamFilter.teamIndex = TeamIndex.Player;
                    }
                    instance.GetComponentInChildren<HealthPickup>();
                    instance.transform.localScale = Vector3.one;
                    NetworkServer.Spawn(instance);
                }
                if (DotController.dotControllerLocator.TryGetValue(victimBody.gameObject.GetInstanceID(), out DotController dotController))
                {
                    dotController.ClearDotStacksForType(fruitingDot);
                }
            }

            public void OnKilledServer(DamageReport damageReport)
            {
                DropFruitsServer();
            }
        }
    }
}