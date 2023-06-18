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

namespace Gupdate.Gameplay.Monsters
{
    public class Acrid : ModBehaviour
    {
        public override (string, string)[] GetLang() => new[]
        {
            ("CROCO_PASSIVE_ALT_DESCRIPTION", "Attacks that apply <style=cIsHealing>Poison</style> apply stacking <style=cIsDamage>Blight</style> instead, dealing <style=cIsDamage>60% damage per second</style> and reducing armor by <style=cIsDamage>5</style>."),
        };

        public void Awake()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            args.armorAdd -= sender.GetBuffCount(RoR2Content.Buffs.Blight) * 5f;
        }
    }
}
