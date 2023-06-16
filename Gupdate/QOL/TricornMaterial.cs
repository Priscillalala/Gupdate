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
    public class TricornMaterial : ModBehaviour
    {
        public void Awake()
        {
            Addressables.LoadAssetAsync<Material>("RoR2/DLC1/BossHunter/matBlunderbussPickup.mat").Completed += handle =>
            {
                handle.Result.SetColor("_Color", new Color32(43, 121, 142, 255));
                handle.Result.SetColor("_EmColor", new Color32(95, 208, 255, 255));
                handle.Result.SetTexture("_FresnelRamp", Addressables.LoadAssetAsync<Texture>("RoR2/DLC1/Common/ColorRamps/texGipRamp.png").WaitForCompletion());
            };
        }
    }
}
