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
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine.Rendering.PostProcessing;

namespace Gupdate.Gameplay.Monsters
{
    public class MeadowMeetsPeak : ModBehaviour
    {
        private AsyncOperationHandle<SpawnCard> cscMegaConstruct;
        private AsyncOperationHandle<SpawnCard> cscMinorConstruct;
        private AsyncOperationHandle<SpawnCard> cscMagmaWorm;
        private AsyncOperationHandle<SpawnCard> cscTitanDampCave;

        public void Awake()
        {
            cscMegaConstruct = Addressables.LoadAssetAsync<SpawnCard>("RoR2/DLC1/MajorAndMinorConstruct/cscMegaConstruct.asset");
            cscMinorConstruct = Addressables.LoadAssetAsync<SpawnCard>("RoR2/DLC1/MajorAndMinorConstruct/cscMinorConstruct.asset");
            cscMagmaWorm = Addressables.LoadAssetAsync<SpawnCard>("RoR2/Base/MagmaWorm/cscMagmaWorm.asset");
            cscTitanDampCave = Addressables.LoadAssetAsync<SpawnCard>("RoR2/Base/Titan/cscTitanDampCave.asset");

            Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/Base/skymeadow/dccsSkyMeadowMonstersDLC1.asset").Completed += handle =>
            {
                if (handle.Result.TryFindCategory("Champions", out DirectorCardCategorySelection.Category champions))
                {
                    foreach (DirectorCard directorCard in champions.cards)
                    {
                        if (directorCard.spawnCard == cscMegaConstruct.WaitForCompletion())
                        {
                            directorCard.spawnCard = cscMagmaWorm.WaitForCompletion();
                            break;
                        }
                    }
                }
                handle.Result.RemoveCardsThatFailFilter(x => x.spawnCard != cscMinorConstruct.WaitForCompletion());
            };

            Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/Base/dampcave/dccsDampCaveMonstersDLC1.asset").Completed += handle =>
            {
                if (handle.Result.TryFindCategory("Champions", out DirectorCardCategorySelection.Category champions))
                {
                    foreach (DirectorCard directorCard in champions.cards)
                    {
                        if (directorCard.spawnCard == cscTitanDampCave.WaitForCompletion())
                        {
                            directorCard.spawnCard = cscMegaConstruct.WaitForCompletion();
                            break;
                        }
                    }
                }
                if (handle.Result.TryFindCategoryIndex("Basic Monsters", out int basicMonstersIndex))
                {
                    handle.Result.AddCard(basicMonstersIndex, new DirectorCard
                    {
                        spawnCard = cscMinorConstruct.WaitForCompletion(),
                        selectionWeight = 1,
                        spawnDistance = DirectorCore.MonsterSpawnDistance.Standard,
                    });
                }
            };
        }
    }
}
