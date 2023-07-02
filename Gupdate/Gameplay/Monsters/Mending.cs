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
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine.Events;
using Mono.Cecil;

namespace Gupdate.Gameplay.Monsters
{
    public class Mending : ModBehaviour
    {
        public void Awake()
        {
            /*Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/EliteEarth/AffixEarthHealerBody.prefab").Completed += handle =>
            {
                CharacterBody affixEarthHealerBody = handle.Result.GetComponent<CharacterBody>();
                affixEarthHealerBody.baseDamage = 
            };*/
        }
    }
}
