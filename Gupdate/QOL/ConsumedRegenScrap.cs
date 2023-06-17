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

namespace Gupdate.QOL
{
    public class ConsumedRegenScrap : ModBehaviour
    {
        public void Awake()
        {
            Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC1/RegeneratingScrap/RegeneratingScrapConsumed.asset").Completed += handle =>
            {
                handle.Result.canRemove = false;
                handle.Result.pickupIconSprite = Gupdate.assets.LoadAsset<Sprite>("texRegeneratingScrapConsumedIcon");
            };
        }
    }
}
