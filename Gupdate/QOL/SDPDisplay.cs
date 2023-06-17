using System;
using BepInEx;
using EntityStates.FlyingVermin.Weapon;
using EntityStates.Vermin.Weapon;
using HG;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gupdate.QOL
{
    public class SDPDisplay : ModBehaviour
    {
        public void Awake()
        {
            AsyncOperationHandle<GameObject> chainGunDisplay = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/DroneWeapons/DisplayDroneWeaponMinigun.prefab");
            AsyncOperationHandle<GameObject> robotArmDisplay = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/DroneWeapons/DisplayDroneWeaponRobotArm.prefab");
            AsyncOperationHandle<GameObject> launcherDisplay = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/DroneWeapons/DisplayDroneWeaponLauncher.prefab");

            AsyncOperationHandle<ItemDef> DroneWeaponsDisplay1 = Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC1/DroneWeapons/DroneWeaponsDisplay1.asset");
            AsyncOperationHandle<ItemDef> DroneWeaponsDisplay2 = Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC1/DroneWeapons/DroneWeaponsDisplay2.asset");

            Addressables.LoadAssetAsync<ItemDisplayRuleSet>("RoR2/Base/Drones/idrsMegaDrone.asset").Completed += handle =>
            {
                TryModifyItemDisplayRule(handle.Result, DroneWeaponsDisplay1.WaitForCompletion(), chainGunDisplay.WaitForCompletion(), (ref ItemDisplayRule idr) =>
                {
                    idr.childName = "Head";
                    idr.localPos = new Vector3(-1.01503F, 0.125F, 0.00001F);
                    idr.localAngles = new Vector3(357.9697F, 179.9687F, 103.5902F);
                });
            };

            Addressables.LoadAssetAsync<ItemDisplayRuleSet>("RoR2/Base/Drones/idrsDrone1.asset").Completed += handle =>
            {
                TryModifyItemDisplayRule(handle.Result, DroneWeaponsDisplay1.WaitForCompletion(), chainGunDisplay.WaitForCompletion(), (ref ItemDisplayRule idr) =>
                {
                    idr.childName = "Head";
                    idr.localPos = new Vector3(0.86973F, -0.07F, -0.05036F);
                    idr.localAngles = new Vector3(273.0469F, 61.64452F, 25.27165F);
                });
            };


            Addressables.LoadAssetAsync<ItemDisplayRuleSet>("RoR2/Base/RoboBallBoss/idrsRoboBallMini.asset").Completed += handle =>
            {
                ItemDisplayRule chainGunRuleRoboBall = new ItemDisplayRule
                {
                    childName = "Muzzle",
                    followerPrefab = chainGunDisplay.WaitForCompletion(),
                    localPos = new Vector3(0F, -0.90161F, -0.92476F),
                    localAngles = new Vector3(0F, 267.2474F, 0F),
                    localScale = new Vector3(0.65679F, 0.65679F, 0.65679F)
                };
                ItemDisplayRule robotArmRuleRoboBall = new ItemDisplayRule
                {
                    childName = "Muzzle",
                    followerPrefab = robotArmDisplay.WaitForCompletion(),
                    localPos = new Vector3(-0.98133F, 0F, -0.88825F),
                    localAngles = new Vector3(37.33081F, 296.2495F, 348.2924F),
                    localScale = new Vector3(1.3F, 1.3F, 1.3F)
                };
                ItemDisplayRule launcherRuleRoboBall = new ItemDisplayRule
                {
                    childName = "Muzzle",
                    followerPrefab = launcherDisplay.WaitForCompletion(),
                    localPos = new Vector3(0F, 0.84144F, -1.48515F),
                    localAngles = new Vector3(292.1092F, 179.9797F, 359.2815F),
                    localScale = new Vector3(0.54199F, 0.54199F, 0.54199F)
                };
                ArrayUtils.ArrayAppend(ref handle.Result.keyAssetRuleGroups, new ItemDisplayRuleSet.KeyAssetRuleGroup { keyAsset = DroneWeaponsDisplay1.WaitForCompletion(), displayRuleGroup = new DisplayRuleGroup { rules = new[] { chainGunRuleRoboBall, launcherRuleRoboBall } } });
                ArrayUtils.ArrayAppend(ref handle.Result.keyAssetRuleGroups, new ItemDisplayRuleSet.KeyAssetRuleGroup { keyAsset = DroneWeaponsDisplay2.WaitForCompletion(), displayRuleGroup = new DisplayRuleGroup { rules = new[] { robotArmRuleRoboBall, launcherRuleRoboBall } } });
            };

            Addressables.LoadAssetAsync<ItemDisplayRuleSet>("RoR2/Base/Toolbot/idrsToolbot.asset").Completed += handle =>
            {
                ItemDisplayRule chainGunRuleToolbot = new ItemDisplayRule
                {
                    childName = "LowerArmL",
                    followerPrefab = chainGunDisplay.WaitForCompletion(),
                    localPos = new Vector3(1.18103F, 1.93993F, -0.10851F),
                    localAngles = new Vector3(356.0636F, 7.73336F, 74.58245F),
                    localScale = new Vector3(2.32586F, 2.32586F, 2.32586F)
                };
                ItemDisplayRule robotArmRuleToolbot = new ItemDisplayRule
                {
                    childName = "Chest",
                    followerPrefab = robotArmDisplay.WaitForCompletion(),
                    localPos = new Vector3(4.29751F, 2.2913F, 0.1107F),
                    localAngles = new Vector3(29.75314F, 56.29673F, 4.77582F),
                    localScale = new Vector3(-4.78734F, 4.78734F, 4.78734F)
                };
                ItemDisplayRule launcherRuleToolbot = new ItemDisplayRule
                {
                    childName = "Chest",
                    followerPrefab = launcherDisplay.WaitForCompletion(),
                    localPos = new Vector3(0.03985F, 2.20276F, -2.28506F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(1.67958F, 1.67958F, 1.67958F)
                };
                ArrayUtils.ArrayAppend(ref handle.Result.keyAssetRuleGroups, new ItemDisplayRuleSet.KeyAssetRuleGroup { keyAsset = DroneWeaponsDisplay1.WaitForCompletion(), displayRuleGroup = new DisplayRuleGroup { rules = new[] { chainGunRuleToolbot, launcherRuleToolbot } } });
                ArrayUtils.ArrayAppend(ref handle.Result.keyAssetRuleGroups, new ItemDisplayRuleSet.KeyAssetRuleGroup { keyAsset = DroneWeaponsDisplay2.WaitForCompletion(), displayRuleGroup = new DisplayRuleGroup { rules = new[] { robotArmRuleToolbot, launcherRuleToolbot } } });
            };

            Addressables.LoadAssetAsync<ItemDisplayRuleSet>("RoR2/Base/Treebot/idrsTreebot.asset").Completed += handle =>
            {
                ItemDisplayRule chainGunRuleTreebot = new ItemDisplayRule
                {
                    childName = "PlatformBase",
                    followerPrefab = chainGunDisplay.WaitForCompletion(),
                    localPos = new Vector3(0F, -0.61243F, -0.01515F),
                    localAngles = new Vector3(0.29367F, 269.1211F, 359.2129F),
                    localScale = new Vector3(0.6037F, 0.6037F, 0.6037F)
                };
                ItemDisplayRule robotArmRuleTreebot = new ItemDisplayRule
                {
                    childName = "PlatformBase",
                    followerPrefab = robotArmDisplay.WaitForCompletion(),
                    localPos = new Vector3(0.70485F, 0.60179F, -0.197F),
                    localAngles = new Vector3(61.1553F, 105.4397F, 34.0543F),
                    localScale = new Vector3(-1.52127F, 1.52127F, 1.52127F)
                };
                ItemDisplayRule launcherRuleTreebot = new ItemDisplayRule
                {
                    childName = "PlatformBase",
                    followerPrefab = launcherDisplay.WaitForCompletion(),
                    localPos = new Vector3(0F, 1.31884F, -0.85263F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(0.65136F, 0.65136F, 0.65136F)
                };
                ArrayUtils.ArrayAppend(ref handle.Result.keyAssetRuleGroups, new ItemDisplayRuleSet.KeyAssetRuleGroup { keyAsset = DroneWeaponsDisplay1.WaitForCompletion(), displayRuleGroup = new DisplayRuleGroup { rules = new[] { chainGunRuleTreebot, launcherRuleTreebot } } });
                ArrayUtils.ArrayAppend(ref handle.Result.keyAssetRuleGroups, new ItemDisplayRuleSet.KeyAssetRuleGroup { keyAsset = DroneWeaponsDisplay2.WaitForCompletion(), displayRuleGroup = new DisplayRuleGroup { rules = new[] { robotArmRuleTreebot, launcherRuleTreebot } } });
            };
        }

        public delegate void ModifyIdrCallback(ref ItemDisplayRule idr);

        public void TryModifyItemDisplayRule(ItemDisplayRuleSet idrs, UnityEngine.Object keyAsset, GameObject followerPrefab, ModifyIdrCallback callback)
        {
            for (int i = 0; i < idrs.keyAssetRuleGroups.Length; i++)
            {
                ref ItemDisplayRuleSet.KeyAssetRuleGroup ruleGroup = ref idrs.keyAssetRuleGroups[i];
                if (ruleGroup.keyAsset == keyAsset && !ruleGroup.displayRuleGroup.isEmpty)
                {
                    for (int j = 0; j < ruleGroup.displayRuleGroup.rules.Length; j++)
                    {
                        ref ItemDisplayRule rule = ref ruleGroup.displayRuleGroup.rules[j];
                        if (rule.followerPrefab == followerPrefab)
                        {
                            callback(ref rule);
                            return;
                        }
                    }
                }
            }
        }
    }
}
