﻿using System;
using BepInEx;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gupdate.Gameplay.Monsters
{
    public class Huntress : ModBehaviour
    {
        public override (string, string)[] GetLang() => new[]
        {
            ("HUNTRESS_SPECIAL_DESCRIPTION", "<style=cIsUtility>Teleport</style> into the sky. Target an area to rain arrows, <style=cIsUtility>slowing</style> all enemies and dealing <style=cIsDamage>330% damage per second</style>."),
        };

        public void Awake()
        {
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/HuntressArrowRain.prefab").Completed += handle =>
            {
                handle.Result.GetComponent<ProjectileDotZone>().overlapProcCoefficient = 0.5f;
                if (handle.Result.transform.TryFind("FX/Hitbox", out Transform hitbox))
                {
                    hitbox.localPosition = new Vector3(0, 0.5f, 0);
                    hitbox.localScale = new Vector3(hitbox.localScale.x, 1.2f, hitbox.localScale.z);
                } 
            };
        }
    }
}
