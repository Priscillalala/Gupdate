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
    public class Mithrix : ModBehaviour
    {
        public void Awake()
        {
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherBody.prefab").Completed += handle =>
            {
                CharacterBody brotherBody = handle.Result.GetComponent<CharacterBody>();
                brotherBody.baseMaxHealth = 1400f;
                brotherBody.levelMaxHealth = 420f;
            };
        }
    }
}
