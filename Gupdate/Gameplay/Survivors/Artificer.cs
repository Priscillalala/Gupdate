using System;
using BepInEx;
using EntityStates.Mage.Weapon;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using RoR2.Skills;

namespace Gupdate.Gameplay.Monsters
{
    public class Artificer : ModBehaviour
    {
        public override (string, string)[] GetLang() => new[]
        {
            ("MAGE_SPECIAL_FIRE_DESCRIPTION", "<style=cIsDamage>Ignite</style>. Burn all enemies in front of you for <style=cIsDamage>2400% damage</style>."),
        };

        public void Awake()
        {
            Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/Mage/EntityStates.Mage.Weapon.Flamethrower.asset").Completed += handle =>
            {
                handle.Result.TryModifyFieldValue(nameof(Flamethrower.totalDamageCoefficient), 24f);
            };
        }
    }
}
