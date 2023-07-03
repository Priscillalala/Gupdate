using System;
using BepInEx;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gupdate.Gameplay
{
    public class Directors : ModBehaviour
    {
        public float multiplier = 1.15f;

        public void Awake()
        {
            On.RoR2.CombatDirector.Awake += CombatDirector_Awake;
        }

        private void CombatDirector_Awake(On.RoR2.CombatDirector.orig_Awake orig, CombatDirector self)
        {
            if (self.gameObject == DirectorCore.instance.gameObject)
            {
                self.creditMultiplier *= multiplier;
                self.minSeriesSpawnInterval /= multiplier;
                self.maxSeriesSpawnInterval /= multiplier;
                self.minRerollSpawnInterval /= multiplier;
                self.maxRerollSpawnInterval /= multiplier;
            }
            orig(self);
        }
    }
}
