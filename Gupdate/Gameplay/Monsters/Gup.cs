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
using EntityStates.VoidJailer.Weapon;
using Mono.Cecil;

namespace Gupdate.Gameplay.Monsters
{
    public class Gup : ModBehaviour
    {
        public void Awake()
        {
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Gup/GupBody.prefab").Completed += handle =>
            {
                handle.Result.AddComponent<GoldRewardCoefficient>();
            };
        }

        public class GoldRewardCoefficient : MonoBehaviour
        {
            public float coefficient = 0.5f;

            public void Start()
            {
                if (base.TryGetComponent(out DeathRewards deathRewards))
                {
                    deathRewards.goldReward = (uint)Mathf.FloorToInt(deathRewards.goldReward * coefficient);
                }
            }
        }
    }
}
