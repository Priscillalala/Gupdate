using System;
using BepInEx;
using RoR2.CharacterAI;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gupdate.Gameplay.Monsters
{
    public class XiConstruct : ModBehaviour
    {
        public void Awake()
        {
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/MajorAndMinorConstruct/MegaConstructMaster.prefab").Completed += handle =>
            {
                foreach (AISkillDriver skillDriver in handle.Result.GetComponents<AISkillDriver>())
                {
                    switch (skillDriver.customName)
                    {
                        case "FollowStep":
                            skillDriver.minDistance = 40f;
                            break;
                        case "StrafeStep":
                            skillDriver.maxDistance = 40f;
                            skillDriver.minDistance = 15f;
                            skillDriver.driverUpdateTimerOverride = 1f;
                            break;
                        case "FleeStep":
                            skillDriver.maxDistance = 15f;
                            break;
                    }
                }
            };
        }
    }
}
