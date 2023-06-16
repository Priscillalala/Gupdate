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

namespace Gupdate.Gameplay
{
    public class LowHealthFraction : ModBehaviour
    {
        public void Awake()
        {
            HealthComponent.lowHealthFraction = 0.2499f;
        }
    }
}
