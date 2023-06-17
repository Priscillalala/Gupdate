using System;
using BepInEx;
using EntityStates.FlyingVermin.Weapon;
using EntityStates.Vermin.Weapon;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using RoR2.Items;

namespace Gupdate.Gameplay.Items
{
    public class Preon : ModBehaviour
    {
        public override (string, string)[] GetLang() => new[]
        {
            ("EQUIPMENT_BFG_DESC", "Fires preon tendrils, zapping enemies within 35m for up to <style=cIsDamage>1600% damage/second</style>. On contact, detonate in an enormous 20m explosion for <style=cIsDamage>10,000% damage</style>."),
        };

        public void Awake()
        {
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/BFG/BeamSphere.prefab").Completed += handle =>
            {
                handle.Result.GetComponent<ProjectileExplosion>().blastDamageCoefficient = 50f;
                handle.Result.GetComponent<ProjectileProximityBeamController>().listClearInterval = 0.25f;
            };
        }
    }
}
