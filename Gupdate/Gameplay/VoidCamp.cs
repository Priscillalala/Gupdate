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
    public class VoidCamp : ModBehaviour
    {
        private AsyncOperationHandle<GameObject> voidChestPrefab;

        public void Awake()
        {
            Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/DLC1/VoidCamp/iscVoidCamp.asset").Completed += handle =>
            {
                handle.Result.maxSpawnsPerStage = 1;
            };

            voidChestPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidChest/VoidChest.prefab");

            SceneDirector.onGenerateInteractableCardSelection += SceneDirector_onGenerateInteractableCardSelection;
        }

        private void SceneDirector_onGenerateInteractableCardSelection(SceneDirector sceneDirector, DirectorCardCategorySelection dccs)
        {
            dccs.RemoveCardsThatFailFilter(x => x.spawnCard?.prefab != voidChestPrefab.WaitForCompletion());
        }
    }
}
