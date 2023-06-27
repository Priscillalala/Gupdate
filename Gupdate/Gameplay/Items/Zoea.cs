using System;
using BepInEx;
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
using HG;

namespace Gupdate.Gameplay.Items
{
    public class Zoea : ModBehaviour
    {
        public void Awake()
        {
            Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC1/VoidMegaCrabItem.asset").Completed += handle =>
            {
                ArrayUtils.ArrayAppend(ref handle.Result.tags, ItemTag.CannotCopy);
            };
        }
    }
}
