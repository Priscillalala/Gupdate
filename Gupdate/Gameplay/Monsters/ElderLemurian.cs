using System;
using BepInEx;
using RoR2.CharacterAI;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine.Events;
using EntityStates.LemurianBruiserMonster;
using Mono.Cecil;

namespace Gupdate.Gameplay.Monsters
{
    public class ElderLemurian : ModBehaviour
    {
        public void Awake()
        {
            Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/LemurianBruiser/EntityStates.LemurianBruiserMonster.Flamebreath.asset").Completed += handle =>
            {
                handle.Result.TryModifyFieldValue(nameof(Flamebreath.tickFrequency), 6f);
                handle.Result.TryModifyFieldValue(nameof(Flamebreath.totalDamageCoefficient), 6.5f);
            };
        }
    }
}
