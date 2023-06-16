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
    public class VoidBarnacleMaterial : ModBehaviour
    {
        public void Awake()
        {
            Addressables.LoadAssetAsync<Material>("RoR2/DLC1/VoidBarnacle/matVoidBarnacle.mat").Completed += handle =>
            {
                handle.Result.SetTexture("_FresnelRamp", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampNullifierOffset.png").WaitForCompletion());

            };
        }
    }
}
