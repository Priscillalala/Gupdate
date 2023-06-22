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
using EntityStates.BeetleQueenMonster;
using UnityEngine.Events;

namespace Gupdate.Gameplay.Monsters
{
    public class BeetleQueen : ModBehaviour
    {
        private static DeployableSlot beetleGuardSummons;
        private AsyncOperationHandle<SpawnCard> cscBeetle;

        public void Awake()
        {
            beetleGuardSummons = DeployableAPI.RegisterDeployableSlot((self, deployableCountMultiplier) =>
            {
                return 2 * deployableCountMultiplier;
            });
            cscBeetle = Addressables.LoadAssetAsync<SpawnCard>("RoR2/Base/Beetle/cscBeetle.asset");

            IL.EntityStates.BeetleQueenMonster.SummonEggs.SummonEgg += SummonEggs_SummonEgg;
        }

        private void SummonEggs_SummonEgg(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ilfound = c.TryGotoNext(MoveType.After, x => x.MatchLdsfld<SummonEggs>(nameof(SummonEggs.spawnCard)));

            if (ilfound)
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<SpawnCard, SummonEggs, SpawnCard>>((spawnCard, summonEggs) => 
                {
                    CharacterMaster master = summonEggs.characterBody?.master;
                    return master && master.GetDeployableCount(beetleGuardSummons) < master.GetDeployableSameSlotLimit(beetleGuardSummons) ? spawnCard : cscBeetle.WaitForCompletion();
                });
            }

            ilfound = c.TryGotoNext(MoveType.Before, x => x.MatchStfld<DirectorSpawnRequest>(nameof(DirectorSpawnRequest.onSpawnedServer)));

            if (ilfound)
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<Action<SpawnCard.SpawnResult>, SummonEggs, Action<SpawnCard.SpawnResult>>>((onSpawned, summonEggs) =>
                {
                    return (Action<SpawnCard.SpawnResult>)Delegate.Combine(onSpawned, (SpawnCard.SpawnResult result) =>
                    {
                        if (result.success && result.spawnedInstance && result.spawnedInstance.TryGetComponent(out CharacterMaster spawnedMaster))
                        {
                            if (spawnedMaster.masterIndex == MasterCatalog.FindMasterIndex("BeetleGuardMaster") && summonEggs?.characterBody?.master)
                            {
                                Deployable deployable = result.spawnedInstance.AddComponent<Deployable>();
                                deployable.onUndeploy = new UnityEvent();
                                deployable.onUndeploy.AddListener(new UnityAction(spawnedMaster.TrueKill));
                                summonEggs.characterBody.master.AddDeployable(deployable, beetleGuardSummons);
                            }
                        }
                    });
                });
            }
        }
    }
}
