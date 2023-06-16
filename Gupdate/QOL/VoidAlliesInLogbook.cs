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
    public class VoidAlliesInLogbook : ModBehaviour
    {
        public string[] voidAllyBodyKeys = new[]
        {
            "RoR2/Base/Nullifier/NullifierAllyBody.prefab",
            "RoR2/DLC1/VoidJailer/VoidJailerAllyBody.prefab",
            "RoR2/DLC1/VoidMegaCrab/VoidMegaCrabAllyBody.prefab",
        };

        public void Awake()
        {
            foreach (string key in voidAllyBodyKeys)
            {
                Addressables.LoadAssetAsync<GameObject>(key).Completed += handle =>
                {
                    if (handle.Result && handle.Result.TryGetComponent(out DeathRewards deathRewards))
                    {
                        DestroyImmediate(deathRewards);
                    }
                };                
            }
        }
    }
}
